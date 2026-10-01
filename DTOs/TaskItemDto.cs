using TaskFlowApi.Entities;

namespace TaskFlowApi.DTOs;

/// <summary>Задача в ответе API v1</summary>
public class TaskItemDto
{
    /// <summary>Идентификатор задачи</summary>
    public int Id { get; set; }

    /// <summary>Заголовок задачи</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Описание задачи</summary>
    public string? Description { get; set; }

    /// <summary>Статус задачи</summary>
    public TaskItemStatus Status { get; set; }

    /// <summary>Идентификатор проекта</summary>
    public int ProjectId { get; set; }

    /// <summary>Идентификатор исполнителя</summary>
    public int? AssignedToId { get; set; }

    /// <summary>Имя пользователя-исполнителя</summary>
    public string? AssignedToUsername { get; set; }

    /// <summary>Срок выполнения</summary>
    public DateTime? DueDate { get; set; }
}
