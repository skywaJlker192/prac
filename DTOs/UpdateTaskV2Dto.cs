using TaskFlowApi.Entities;

namespace TaskFlowApi.DTOs;

/// <summary>Данные для полного обновления задачи в API v2</summary>
public class UpdateTaskV2Dto : UpdateTaskDto
{
    /// <summary>Приоритет задачи. Если не указан, остаётся прежним</summary>
    public TaskPriority? Priority { get; set; }
}
