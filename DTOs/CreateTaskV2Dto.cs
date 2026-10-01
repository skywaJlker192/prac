using TaskFlowApi.Entities;

namespace TaskFlowApi.DTOs;

/// <summary>Данные для создания задачи в API v2</summary>
public class CreateTaskV2Dto : CreateTaskDto
{
    /// <summary>Приоритет задачи. Если не указан, будет Medium</summary>
    public TaskPriority? Priority { get; set; }
}
