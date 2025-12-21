namespace BlazorPlayground.Contracts;

public abstract record PagedResult(
    int PageIndex,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    public int FirstRowOnPage
    {
        get { return (PageIndex - 1) * PageSize + 1; }
    }

    public int LastRowOnPage
    {
        get { return Math.Min(PageIndex * PageSize, TotalCount); }
    }

}

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageIndex,
    int PageSize,
    int TotalCount): PagedResult(PageIndex, PageSize, TotalCount)
{
  
}