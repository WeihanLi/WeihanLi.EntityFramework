using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using WeihanLi.Common.Models;

namespace WeihanLi.EntityFramework
{
    public static class QueryablePageListExtensions
    {
        /// <summary>
        /// Converts the specified source to <see cref="IPagedListResult{T}"/> by the specified <paramref name="pageNumber"/> and <paramref name="pageSize"/>.
        /// </summary>
        /// <typeparam name="T">The type of the source.</typeparam>
        /// <param name="source">The source to paging.</param>
        /// <param name="pageNumber">The number of the page, index from 1.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <returns>An instance of  implements <see cref="IPagedListResult{T}"/> interface.</returns>
        public static IPagedListResult<T> ToPagedList<T>([NotNull] this IQueryable<T> source, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 10;
            }
            var count = source.Count();
            if (count == 0)
            {
                return PagedListResult<T>.Empty;
            }

            if (pageNumber > 1)
            {
                source = source.Skip((pageNumber - 1) * pageSize);
            }
            var items = source
                                    .Take(pageSize)
                                    .ToArray();
            var pagedList = new PagedListResult<T>()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = count,
                Data = items
            };

            return pagedList;
        }

        /// <summary>
        /// Converts the specified source to <see cref="IPagedListResult{T}"/> by the specified <paramref name="pageNumber"/> and <paramref name="pageSize"/>.
        /// </summary>
        /// <typeparam name="T">The type of the source.</typeparam>
        /// <param name="source">The source to paging.</param>
        /// <param name="pageNumber">The number of the page, index from 1.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>An instance of  implements  <see cref="IPagedListResult{T}"/> interface.</returns>
        public static async Task<IPagedListResult<T>> ToPagedListAsync<T>([NotNull] this IQueryable<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            var count = await source.CountAsync(cancellationToken).ConfigureAwait(false);
            if (count == 0)
            {
                return PagedListResult<T>.Empty;
            }

            if (pageNumber > 1)
            {
                source = source.Skip((pageNumber - 1) * pageSize);
            }
            var items = await source
                                    .Take(pageSize)
                                    .ToArrayAsync(cancellationToken);
            var pagedList = new PagedListResult<T>()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = count,
                Data = items
            };

            return pagedList;
        }
    }
}
