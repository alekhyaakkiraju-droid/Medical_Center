using Microsoft.EntityFrameworkCore;

namespace AngularApi.DTO
{
    public static class QueryablePaginationExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            PaginationParameters pagination,
            CancellationToken cancellationToken = default)
        {
            var totalCount = await query.CountAsync(cancellationToken);
            var pageSize = pagination.PageSize;
            var currentPage = pagination.Page;
            var pageCount = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageCount = pageCount,
                CurrentPage = currentPage,
                PageSize = pageSize
            };
        }
    }
}
