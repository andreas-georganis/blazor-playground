using BlazorPlayground.Contracts;
using Microsoft.EntityFrameworkCore;

namespace BlazorPlayground.API.Infrastructure;

public static class QueryableExtensions
{
    extension<T>(IQueryable<T> source)
    {
        public async Task<PagedResult<T>> ToPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            pageNumber = Math.Max(1, pageNumber);
            var skip = (pageNumber - 1) * pageSize;
            
            var totalCount = await source.CountAsync(cancellationToken);
            var items = await source
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<T>(items, pageNumber, pageSize, totalCount);
        }
    }
    
}