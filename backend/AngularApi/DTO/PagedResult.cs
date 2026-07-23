namespace AngularApi.DTO
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int TotalCount { get; init; }
        public int PageCount { get; init; }
        public int CurrentPage { get; init; }
        public int PageSize { get; init; }
    }
}
