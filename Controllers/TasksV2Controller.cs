using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowApi.Data;
using TaskFlowApi.DTOs;
using TaskFlowApi.Entities;
using TaskFlowApi.Exceptions;
using TaskFlowApi.Mapping;

namespace TaskFlowApi.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/tasks")]
[Produces("application/json")]
public class TasksV2Controller : TasksControllerBase
{
    private const int MaxPageSize = 50;

    public TasksV2Controller(AppDbContext db) : base(db)
    {
    }

    /// <summary>
    /// Получить задачи с пагинацией, фильтрацией и сортировкой
    /// </summary>
    /// <param name="page">Номер страницы (по умолчанию 1)</param>
    /// <param name="pageSize">Размер страницы (по умолчанию 10, максимум 50)</param>
    /// <param name="status">Фильтр по статусу: ToDo, InProgress, Done</param>
    /// <param name="priority">Фильтр по приоритету: Low, Medium, High</param>
    /// <param name="sortBy">Поле сортировки: title, dueDate, createdAt</param>
    /// <param name="sortDir">Направление сортировки: asc, desc</param>
    /// <param name="search">Поиск по заголовку (содержит)</param>
    /// <returns>Страница задач</returns>
    [HttpGet]
    [ProducesResponseType<PagedResult<TaskItemV2Dto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<TaskItemV2Dto>>> GetAll(
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? priority = null,
        string? sortBy = null,
        string? sortDir = null,
        string? search = null)
    {
        if (page < 1)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid page", detail: "page должен быть не меньше 1");
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid pageSize", detail: $"pageSize должен быть от 1 до {MaxPageSize}");
        }

        var query = Db.TaskItems.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<TaskItemStatus>(status, true, out var parsedStatus) || !Enum.IsDefined(parsedStatus))
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid status", detail: "Допустимые значения status: ToDo, InProgress, Done");
            }

            query = query.Where(t => t.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(priority))
        {
            if (!Enum.TryParse<TaskPriority>(priority, true, out var parsedPriority) || !Enum.IsDefined(parsedPriority))
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid priority", detail: "Допустимые значения priority: Low, Medium, High");
            }

            query = query.Where(t => t.Priority == parsedPriority);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(t => t.Title.Contains(term));
        }

        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(sortDir) && !descending && !string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid sortDir", detail: "Допустимые значения sortDir: asc, desc");
        }

        IOrderedQueryable<TaskItem> ordered;
        switch (sortBy?.Trim().ToLowerInvariant())
        {
            case null:
            case "":
                ordered = query.OrderBy(t => t.Id);
                break;
            case "title":
                ordered = descending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title);
                break;
            case "duedate":
                ordered = descending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate);
                break;
            case "createdat":
                ordered = descending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt);
                break;
            default:
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid sortBy", detail: "Допустимые значения sortBy: title, dueDate, createdAt");
        }

        var totalCount = await query.CountAsync();

        var items = await ordered
            .ThenBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .SelectV2Dto()
            .ToListAsync();

        return Ok(new PagedResult<TaskItemV2Dto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    /// <summary>
    /// Получить задачу по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <returns>Задача</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType<TaskItemV2Dto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItemV2Dto>> GetById(int id)
    {
        var task = await Db.TaskItems
            .AsNoTracking()
            .Where(t => t.Id == id)
            .SelectV2Dto()
            .FirstOrDefaultAsync();

        if (task == null)
        {
            throw new NotFoundException($"Задача с Id={id} не найдена");
        }

        return Ok(task);
    }

    /// <summary>
    /// Создать задачу (защищено от дублей через X-Idempotency-Key)
    /// </summary>
    /// <param name="idempotencyKey">Уникальный ключ запроса (GUID), обязателен</param>
    /// <param name="dto">Данные новой задачи</param>
    /// <returns>Созданная задача</returns>
    [HttpPost]
    [ProducesResponseType<TaskItemV2Dto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaskItemV2Dto>> Create(
        [FromHeader(Name = "X-Idempotency-Key")] Guid? idempotencyKey,
        CreateTaskV2Dto dto)
    {
        await EnsureProjectExistsAsync(dto.ProjectId);
        await EnsureUserExistsAsync(dto.AssignedToId);

        var task = new TaskItem
        {
            ProjectId = dto.ProjectId,
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status ?? TaskItemStatus.ToDo,
            Priority = dto.Priority ?? TaskPriority.Medium,
            AssignedToId = dto.AssignedToId,
            DueDate = dto.DueDate?.ToUniversalTime()
        };

        Db.TaskItems.Add(task);
        await Db.SaveChangesAsync();

        var result = await Db.TaskItems
            .AsNoTracking()
            .Where(t => t.Id == task.Id)
            .SelectV2Dto()
            .FirstAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { version = RouteData.Values["version"], id = task.Id },
            result);
    }

    /// <summary>
    /// Обновить задачу целиком
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <param name="dto">Новые данные задачи (поле status обязательно)</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(int id, UpdateTaskV2Dto dto)
    {
        var task = await Db.TaskItems.FindAsync(id);
        if (task == null)
        {
            throw new NotFoundException($"Задача с Id={id} не найдена");
        }

        await EnsureUserExistsAsync(dto.AssignedToId);
        await EnsureCanSetStatusAsync(id, dto.Status!.Value);

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Status = dto.Status.Value;
        task.Priority = dto.Priority ?? task.Priority;
        task.AssignedToId = dto.AssignedToId;
        task.DueDate = dto.DueDate?.ToUniversalTime();
        await Db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Изменить статус задачи. Нельзя завершить задачу без комментариев
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <param name="dto">Новый статус</param>
    /// <returns>Обновлённая задача</returns>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType<TaskItemV2Dto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TaskItemV2Dto>> UpdateStatus(int id, UpdateTaskStatusDto dto)
    {
        var task = await Db.TaskItems.FindAsync(id);
        if (task == null)
        {
            throw new NotFoundException($"Задача с Id={id} не найдена");
        }

        await EnsureCanSetStatusAsync(id, dto.Status!.Value);

        task.Status = dto.Status.Value;
        await Db.SaveChangesAsync();

        var result = await Db.TaskItems
            .AsNoTracking()
            .Where(t => t.Id == id)
            .SelectV2Dto()
            .FirstAsync();

        return Ok(result);
    }

    /// <summary>
    /// Удалить задачу
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await Db.TaskItems.FindAsync(id);
        if (task == null)
        {
            throw new NotFoundException($"Задача с Id={id} не найдена");
        }

        Db.TaskItems.Remove(task);
        await Db.SaveChangesAsync();

        return NoContent();
    }
}
