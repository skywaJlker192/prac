using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowApi.Data;
using TaskFlowApi.Entities;
using TaskFlowApi.Exceptions;

namespace TaskFlowApi.Controllers;

public abstract class TasksControllerBase : ControllerBase
{
    protected readonly AppDbContext Db;

    protected TasksControllerBase(AppDbContext db)
    {
        Db = db;
    }

    protected async Task EnsureProjectExistsAsync(int projectId)
    {
        if (!await Db.Projects.AnyAsync(p => p.Id == projectId))
        {
            throw new NotFoundException($"Проект с Id={projectId} не найден");
        }
    }

    protected async Task EnsureUserExistsAsync(int? userId)
    {
        if (userId.HasValue && !await Db.Users.AnyAsync(u => u.Id == userId.Value))
        {
            throw new NotFoundException($"Пользователь с Id={userId.Value} не найден");
        }
    }

    protected async Task EnsureCanSetStatusAsync(int taskId, TaskItemStatus status)
    {
        if (status == TaskItemStatus.Done && !await Db.Comments.AnyAsync(c => c.TaskItemId == taskId))
        {
            throw new BusinessRuleException("Нельзя завершить задачу без комментариев");
        }
    }
}
