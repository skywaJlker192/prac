using System.ComponentModel.DataAnnotations;
using TaskFlowApi.Entities;
using TaskFlowApi.Validation;

namespace TaskFlowApi.DTOs;

/// <summary>Данные для полного обновления задачи</summary>
public class UpdateTaskDto
{
    /// <summary>Заголовок задачи (минимум 5 символов)</summary>
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Описание задачи</summary>
    public string? Description { get; set; }

    /// <summary>Статус задачи (обязателен при полной замене)</summary>
    [Required]
    public TaskItemStatus? Status { get; set; }

    /// <summary>Идентификатор исполнителя</summary>
    public int? AssignedToId { get; set; }

    /// <summary>Срок выполнения. Если указан, должен быть в будущем</summary>
    [FutureDate]
    public DateTime? DueDate { get; set; }
}
