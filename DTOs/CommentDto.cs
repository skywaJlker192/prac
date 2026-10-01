namespace TaskFlowApi.DTOs;

/// <summary>Комментарий в ответе API</summary>
public class CommentDto
{
    /// <summary>Идентификатор комментария</summary>
    public int Id { get; set; }

    /// <summary>Текст комментария</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Имя пользователя-автора</summary>
    public string AuthorUsername { get; set; } = string.Empty;

    /// <summary>Дата создания (UTC)</summary>
    public DateTime CreatedAt { get; set; }
}
