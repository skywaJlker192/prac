using TaskFlowApi.DTOs;
using TaskFlowApi.Entities;

namespace TaskFlowApi.Mapping;

public static class MappingExtensions
{
    public static IQueryable<ProjectDto> SelectDto(this IQueryable<Project> source)
    {
        return source.Select(p => new ProjectDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            CreatedAt = p.CreatedAt,
            TaskCount = p.Tasks.Count
        });
    }

    public static IQueryable<TaskItemDto> SelectDto(this IQueryable<TaskItem> source)
    {
        return source.Select(t => new TaskItemDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            ProjectId = t.ProjectId,
            AssignedToId = t.AssignedToId,
            AssignedToUsername = t.AssignedTo != null ? t.AssignedTo.Username : null,
            DueDate = t.DueDate
        });
    }

    public static IQueryable<TaskItemV2Dto> SelectV2Dto(this IQueryable<TaskItem> source)
    {
        return source.Select(t => new TaskItemV2Dto
        {
            Id = t.Id,
            Title = t.Title,
            Status = t.Status,
            Priority = t.Priority,
            ProjectId = t.ProjectId,
            AssignedToId = t.AssignedToId,
            AssignedToUsername = t.AssignedTo != null ? t.AssignedTo.Username : null,
            DueDate = t.DueDate,
            CreatedAt = t.CreatedAt
        });
    }

    public static IQueryable<CommentDto> SelectDto(this IQueryable<Comment> source)
    {
        return source.Select(c => new CommentDto
        {
            Id = c.Id,
            Content = c.Content,
            AuthorUsername = c.Author.Username,
            CreatedAt = c.CreatedAt
        });
    }
}
