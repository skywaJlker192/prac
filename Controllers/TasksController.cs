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
[ApiVersion("1.0", Deprecated = true)]
[Route("api/v{version:apiVersion}/tasks")]
[Produces("application/json")]
[Obsolete("API v1 устарел, используйте v2")]
public class TasksController : TasksControllerBase
{
    public TasksController(AppDbContext db) : base(db)
    {
    }

    /// <summary>
    /// Получить список всех задач
    /// </summary>
    /// <returns>Список задач</returns>
    [HttpGet]
    [ProducesResponseType<List<TaskItemDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TaskItemDto>>> GetAll()
    {
        var tasks = await Db.TaskItems
            .AsNoTracking()
            .OrderBy(t => t.Id)
            .SelectDto()
            .ToListAsync();

        return Ok(tasks);
    }

    /// <summary>
    /// Получить задачу по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <returns>Задача</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType<TaskItemDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItemDto>> GetById(int id)
    {
        var task = await Db.TaskItems
            .AsNoTracking()
            .Where(t => t.Id == id)
            .SelectDto()
            .FirstOrDefaultAsync();

        if (task == null)
        {
            throw new NotFoundException($"Задача с Id={id} не найдена");
        }

        return Ok(task);
    }

    /// <summary>
    /// Создать задачу
    /// </summary>
    /// <param name="dto">Данные новой задачи</param>
    /// <returns>Созданная задача</returns>
    [HttpPost]
    [ProducesResponseType<TaskItemDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItemDto>> Create(CreateTaskDto dto)
    {
        await EnsureProjectExistsAsync(dto.ProjectId);
        await EnsureUserExistsAsync(dto.AssignedToId);

        var task = new TaskItem
        {
            ProjectId = dto.ProjectId,
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status ?? TaskItemStatus.ToDo,
            AssignedToId = dto.AssignedToId,
            DueDate = dto.DueDate?.ToUniversalTime()
        };

        Db.TaskItems.Add(task);
        await Db.SaveChangesAsync();

        var result = await Db.TaskItems
            .AsNoTracking()
            .Where(t => t.Id == task.Id)
            .SelectDto()
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
    public async Task<IActionResult> Update(int id, UpdateTaskDto dto)
    {
        var task = await Db.TaskItems.FindAsync(id);
        if (task == null)
        {
            throw new NotFoundException($"Задача с Id={id} не найдена");
        }

        await EnsureUserExistsAsync(dto.AssignedToId);

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Status = dto.Status!.Value;
        task.AssignedToId = dto.AssignedToId;
        task.DueDate = dto.DueDate?.ToUniversalTime();
        await Db.SaveChangesAsync();

        return NoContent();
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
