namespace MiniMart.Shared.Responses;

public class PagedResponse<T> : ApiResponse<List<T>>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 0;

    public static PagedResponse<T> Create(List<T> items, int totalItems, int page, int pageSize, string message = "Lấy dữ liệu thành công")
    {
        return new PagedResponse<T>
        {
            Success = true,
            Message = message,
            Data = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }
}
