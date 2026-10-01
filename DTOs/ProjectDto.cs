namespace TaskFlowApi.DTOs;

/// <summary>Проект в ответе API</summary>
public class ProjectDto
{
    /// <summary>Идентификатор проекта</summary>
    public int Id { get; set; }

    /// <summary>Название проекта</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Описание проекта</summary>
    public string? Description { get; set; }

    /// <summary>Дата создания (UTC)</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Количество задач в проекте</summary>
    public int TaskCount { get; set; }
}
