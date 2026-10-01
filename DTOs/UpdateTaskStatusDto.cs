using System.ComponentModel.DataAnnotations;
using TaskFlowApi.Entities;

namespace TaskFlowApi.DTOs;

/// <summary>Данные для смены статуса задачи</summary>
public class UpdateTaskStatusDto
{
    /// <summary>Новый статус задачи</summary>
    [Required]
    public TaskItemStatus? Status { get; set; }
}
