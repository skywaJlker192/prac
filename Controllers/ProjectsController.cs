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
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProjectsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Получить список всех проектов
    /// </summary>
    /// <returns>Список проектов</returns>
    [HttpGet]
    [ProducesResponseType<List<ProjectDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProjectDto>>> GetAll()
    {
        var projects = await _db.Projects
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .SelectDto()
            .ToListAsync();

        return Ok(projects);
    }

    /// <summary>
    /// Получить проект по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор проекта</param>
    /// <returns>Проект</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ProjectDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> GetById(int id)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .Where(p => p.Id == id)
            .SelectDto()
            .FirstOrDefaultAsync();

        if (project == null)
        {
            throw new NotFoundException($"Проект с Id={id} не найден");
        }

        return Ok(project);
    }

    /// <summary>
    /// Создать проект
    /// </summary>
    /// <param name="dto">Данные нового проекта</param>
    /// <returns>Созданный проект</returns>
    [HttpPost]
    [ProducesResponseType<ProjectDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProjectDto>> Create(CreateProjectDto dto)
    {
        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        var result = await _db.Projects
            .AsNoTracking()
            .Where(p => p.Id == project.Id)
            .SelectDto()
            .FirstAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { version = RouteData.Values["version"], id = project.Id },
            result);
    }

    /// <summary>
    /// Обновить проект целиком
    /// </summary>
    /// <param name="id">Идентификатор проекта</param>
    /// <param name="dto">Новые данные проекта</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateProjectDto dto)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project == null)
        {
            throw new NotFoundException($"Проект с Id={id} не найден");
        }

        project.Name = dto.Name;
        project.Description = dto.Description;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Удалить проект вместе с его задачами и комментариями
    /// </summary>
    /// <param name="id">Идентификатор проекта</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project == null)
        {
            throw new NotFoundException($"Проект с Id={id} не найден");
        }

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
