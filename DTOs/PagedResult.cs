namespace TaskFlowApi.DTOs;

/// <summary>Страница результатов</summary>
public class PagedResult<T>
{
    /// <summary>Элементы текущей страницы</summary>
    public List<T> Items { get; set; } = new();

    /// <summary>Номер страницы</summary>
    public int Page { get; set; }

    /// <summary>Размер страницы</summary>
    public int PageSize { get; set; }

    /// <summary>Общее количество элементов</summary>
    public int TotalCount { get; set; }

    /// <summary>Общее количество страниц</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
