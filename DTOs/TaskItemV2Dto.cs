using TaskFlowApi.Entities;

namespace TaskFlowApi.DTOs;

/// <summary>Задача в ответе API v2 (с приоритетом, без описания)</summary>
public class TaskItemV2Dto
{
    /// <summary>Идентификатор задачи</summary>
    public int Id { get; set; }

    /// <summary>Заголовок задачи</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Статус задачи</summary>
    public TaskItemStatus Status { get; set; }

    /// <summary>Приоритет задачи</summary>
    public TaskPriority Priority { get; set; }

    /// <summary>Идентификатор проекта</summary>
    public int ProjectId { get; set; }

    /// <summary>Идентификатор исполнителя</summary>
    public int? AssignedToId { get; set; }

    /// <summary>Имя пользователя-исполнителя</summary>
    public string? AssignedToUsername { get; set; }

    /// <summary>Срок выполнения</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Дата создания (UTC)</summary>
    public DateTime CreatedAt { get; set; }
}
