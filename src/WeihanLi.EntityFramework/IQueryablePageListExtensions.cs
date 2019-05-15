using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WeihanLi.Common.Models;

namespace WeihanLi.EntityFramework
{
    public static class IQueryablePageListExtensions
    {
        /// <summary>
        /// Converts the specified source to <see cref="IPagedListModel{T}"/> by the specified <paramref name="pageIndex"/> and <paramref name="pageSize"/>.
        /// </summary>
        /// <typeparam name="T">The type of the source.</typeparam>
        /// <param name="source">The source to paging.</param>
        /// <param name="pageNumber">The number of the page, index from 1.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <returns>An instance of  implements <see cref="IPagedListModel{T}"/> interface.</returns>
        public static IPagedListModel<T> ToPagedList<T>(this IQueryable<T> source, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }

            var count = source.Count();
            if (count == 0)
            {
                return new PagedListModel<T>() { PageNumber = pageNumber, PageSize = pageSize, TotalCount = 0 };
            }

            var items = source.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();
            var pagedList = new PagedListModel<T>()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = count,
                Data = items
            };

            return pagedList;
        }

        /// <summary>
        /// Converts the specified source to <see cref="IPagedListModel{T}"/> by the specified <paramref name="pageIndex"/> and <paramref name="pageSize"/>.
        /// </summary>
        /// <typeparam name="T">The type of the source.</typeparam>
        /// <param name="source">The source to paging.</param>
        /// <param name="pageNumber">The number of the page, index from 1.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>An instance of  implements  <see cref="IPagedListModel{T}"/> interface.</returns>
        public static async Task<IPagedListModel<T>> ToPagedListAsync<T>(this IQueryable<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken = default(CancellationToken))
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
                return new PagedListModel<T>() { PageNumber = pageNumber, PageSize = pageSize, TotalCount = 0 };
            }

            var items = await source.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync(cancellationToken).ConfigureAwait(false);
            var pagedList = new PagedListModel<T>()
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
