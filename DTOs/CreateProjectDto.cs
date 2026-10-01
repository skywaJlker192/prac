using System.ComponentModel.DataAnnotations;

namespace TaskFlowApi.DTOs;

/// <summary>Данные для создания проекта</summary>
public class CreateProjectDto
{
    /// <summary>Название проекта (от 3 до 100 символов)</summary>
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Описание проекта</summary>
    public string? Description { get; set; }
}
