using System.ComponentModel.DataAnnotations;
using TaskFlowApi.Entities;
using TaskFlowApi.Validation;

namespace TaskFlowApi.DTOs;

/// <summary>Данные для создания задачи</summary>
public class CreateTaskDto
{
    /// <summary>Заголовок задачи (минимум 5 символов)</summary>
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Описание задачи</summary>
    public string? Description { get; set; }

    /// <summary>Статус задачи. Если не указан, будет ToDo</summary>
    public TaskItemStatus? Status { get; set; }

    /// <summary>Идентификатор проекта</summary>
    [Range(1, int.MaxValue)]
    public int ProjectId { get; set; }

    /// <summary>Идентификатор исполнителя</summary>
    public int? AssignedToId { get; set; }

    /// <summary>Срок выполнения. Если указан, должен быть в будущем</summary>
    [FutureDate]
    public DateTime? DueDate { get; set; }
}
