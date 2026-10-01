using System.ComponentModel.DataAnnotations;

namespace TaskFlowApi.DTOs;

/// <summary>Данные для создания комментария</summary>
public class CreateCommentDto
{
    /// <summary>Текст комментария (от 1 до 500 символов)</summary>
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Content { get; set; } = string.Empty;

    /// <summary>Идентификатор автора</summary>
    [Range(1, int.MaxValue)]
    public int AuthorId { get; set; }
}
