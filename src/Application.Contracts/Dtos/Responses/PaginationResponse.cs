using System.Text.Json.Serialization;

namespace Application.Contracts.Dtos.Responses;

public class PaginationResponse<T>
{
    public IEnumerable<T>? Data { get; init; }

    public Paging<T>? Paging { get; init; }

    public PaginationResponse(IEnumerable<T> data, Paging<T> paging)
    {
        Data = data;
        Paging = paging;
    }

    public PaginationResponse(
        IEnumerable<T> data,
        long totalItemCount,
        long currentPage,
        long pageSize
    )
    {
        Data = data;
        Paging = new Paging<T>(totalItemCount, currentPage, pageSize);
    }

    public PaginationResponse(
        IEnumerable<T> data,
        long totalItemCount,
        long pageSize,
        string? previousCursor = null,
        string? nextCursor = null
    )
    {
        Data = data;
        Paging = new Paging<T>(totalItemCount, pageSize, previousCursor, nextCursor);
    }
}

public class Paging<T>
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? CurrentPage { get; set; }

    public long PageSize { get; set; }

    public long TotalPage { get; set; }

    public long TotalItemCount { get; set; }

    public bool? HasNextPage { get; set; }

    public bool? HasPreviousPage { get; set; }

    public string? Before { get; set; }

    public string? After { get; set; }

    public Paging(long totalItemCount, long currentPage = 1, long pageSize = 10)
    {
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalPage = (long)Math.Ceiling(totalItemCount / (double)pageSize);
        TotalItemCount = totalItemCount;
        HasNextPage = CurrentPage < TotalPage;
        HasPreviousPage = currentPage > 1;
    }

    public Paging(
        long totalItemCount,
        long pageSize = 10,
        string? previousCursor = null,
        string? nextCursor = null
    )
    {
        PageSize = pageSize;
        TotalPage = (long)Math.Ceiling(totalItemCount / (double)pageSize);
        TotalItemCount = totalItemCount;
        After = nextCursor;
        HasNextPage = nextCursor != null;
        Before = previousCursor;
        HasPreviousPage = previousCursor != null;
    }

    public Paging(
        long? currentPage,
        long pageSize,
        long totalPage,
        bool? hasNext,
        bool? hasPrevious,
        string? before = null,
        string? after = null
    )
    {
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalPage = totalPage;
        HasNextPage = hasNext;
        HasPreviousPage = hasPrevious;
        Before = before;
        After = after;
    }
}
