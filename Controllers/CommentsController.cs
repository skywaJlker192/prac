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
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}")]
[Produces("application/json")]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public CommentsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Получить комментарии задачи
    /// </summary>
    /// <param name="taskId">Идентификатор задачи</param>
    /// <returns>Список комментариев</returns>
    [HttpGet("tasks/{taskId:int}/comments")]
    [ProducesResponseType<List<CommentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<CommentDto>>> GetByTask(int taskId)
    {
        if (!await _db.TaskItems.AnyAsync(t => t.Id == taskId))
        {
            throw new NotFoundException($"Задача с Id={taskId} не найдена");
        }

        var comments = await _db.Comments
            .AsNoTracking()
            .Where(c => c.TaskItemId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .SelectDto()
            .ToListAsync();

        return Ok(comments);
    }

    /// <summary>
    /// Добавить комментарий к задаче
    /// </summary>
    /// <param name="taskId">Идентификатор задачи</param>
    /// <param name="dto">Текст комментария и автор</param>
    /// <returns>Созданный комментарий</returns>
    [HttpPost("tasks/{taskId:int}/comments")]
    [ProducesResponseType<CommentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentDto>> Create(int taskId, CreateCommentDto dto)
    {
        if (!await _db.TaskItems.AnyAsync(t => t.Id == taskId))
        {
            throw new NotFoundException($"Задача с Id={taskId} не найдена");
        }

        if (!await _db.Users.AnyAsync(u => u.Id == dto.AuthorId))
        {
            throw new NotFoundException($"Пользователь с Id={dto.AuthorId} не найден");
        }

        var comment = new Comment
        {
            TaskItemId = taskId,
            AuthorId = dto.AuthorId,
            Content = dto.Content
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        var result = await _db.Comments
            .AsNoTracking()
            .Where(c => c.Id == comment.Id)
            .SelectDto()
            .FirstAsync();

        return CreatedAtAction(
            nameof(GetByTask),
            new { version = RouteData.Values["version"], taskId },
            result);
    }

    /// <summary>
    /// Удалить комментарий
    /// </summary>
    /// <param name="id">Идентификатор комментария</param>
    [HttpDelete("comments/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var comment = await _db.Comments.FindAsync(id);
        if (comment == null)
        {
            throw new NotFoundException($"Комментарий с Id={id} не найден");
        }

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
