using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using WeihanLi.Common.Data;
using WeihanLi.Common.Models;

namespace WeihanLi.EntityFramework
{
    public interface IEFRepository<TDbContext, TEntity> : IRepository<TEntity>
        where TDbContext : DbContext
        where TEntity : class
    {
        /// <summary>
        /// Gets the <see cref="List{TEntity}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="orderBy">A function to order elements.</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <remarks>This method default no-tracking query.</remarks>
        List<TEntity> Get(Expression<Func<TEntity, bool>> predicate = null,
                                         Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                         Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                         bool disableTracking = true);

        /// <summary>
        /// Gets the <see cref="List{TEntity}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="orderBy">A function to order elements.</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <remarks>This method default no-tracking query.</remarks>
        List<TResult> Get<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> predicate = null,
                                         Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                         Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                         bool disableTracking = true);

        /// <summary>
        /// Gets the <see cref="List{TEntity}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="orderBy">A function to order elements.</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <remarks>This method default no-tracking query.</remarks>
        Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate = null,
                                         Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                         Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                         bool disableTracking = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the <see cref="List{TResult}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
        /// </summary>
        /// <param name="selector">selector</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="orderBy">A function to order elements.</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="cancellationToken"></param>
        /// <remarks>This method default no-tracking query.</remarks>
        Task<List<TResult>> GetAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> predicate = null,
                                         Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                         Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                         bool disableTracking = true,
                                         CancellationToken cancellationToken = default);

        List<TEntity> Top(int count, Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = true);

        List<TResult> Top<TResult>(int count, Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = true);

        Task<List<TEntity>> TopAsync(int count, Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = true, CancellationToken cancellationToken = default);

        Task<List<TResult>> TopAsync<TResult>(int count, Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the <see cref="IPagedListModel{TEntity}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="orderBy">A function to order elements.</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="pageNumber">The pageNumber of page.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <returns>An <see cref="IPagedListModel{TEntity}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        IPagedListModel<TEntity> Paged(Expression<Func<TEntity, bool>> predicate = null,
                                         Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                         Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                         int pageNumber = 1,
                                         int pageSize = 20,
                                         bool disableTracking = true);

        /// <summary>
        /// Gets the <see cref="IPagedListModel{TEntity}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="orderBy">A function to order elements.</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="pageNumber">The number of page.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>An <see cref="IPagedListModel{TEntity}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        Task<IPagedListModel<TEntity>> PagedAsync(Expression<Func<TEntity, bool>> predicate = null,
                                                    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                                    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                                    int pageNumber = 1,
                                                    int pageSize = 20,
                                                    bool disableTracking = true,
                                                    CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the <see cref="IPagedListModel{TResult}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
        /// </summary>
        /// <param name="selector">The selector for projection.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="orderBy">A function to order elements.</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="pageNumber">pageNumber</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <returns>An <see cref="IPagedListModel{TResult}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        IPagedListModel<TResult> Paged<TResult>(Expression<Func<TEntity, TResult>> selector,
                                                  Expression<Func<TEntity, bool>> predicate = null,
                                                  Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                                  Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                                  int pageNumber = 1,
                                                  int pageSize = 20,
                                                  bool disableTracking = true) where TResult : class;

        /// <summary>
        /// Gets the <see cref="IPagedListModel{TEntity}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
        /// </summary>
        /// <param name="selector">The selector for projection.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="orderBy">A function to order elements.</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="pageNumber">The pageNumber of page.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>An <see cref="IPagedListModel{TEntity}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        Task<IPagedListModel<TResult>> PagedAsync<TResult>(Expression<Func<TEntity, TResult>> selector,
                                                             Expression<Func<TEntity, bool>> predicate = null,
                                                             Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                                             Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                                             int pageNumber = 1,
                                                             int pageSize = 20,
                                                             bool disableTracking = true,
                                                             CancellationToken cancellationToken = default) where TResult : class;

        /// <summary>
        /// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
        /// </summary>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>The found entity or null.</returns>
        TEntity Find(params object[] keyValues);

        /// <summary>
        /// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
        /// </summary>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>A <see cref="Task{TEntity}"/> that represents the asynchronous find operation. The task result contains the found entity or null.</returns>
        Task<TEntity> FindAsync(params object[] keyValues);

        /// <summary>
        /// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
        /// </summary>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TEntity}"/> that represents the asynchronous find operation. The task result contains the found entity or null.</returns>
        Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken);

        int Update<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression);

        int Update<TProperty1, TProperty2>(TEntity entity, Expression<Func<TEntity, TProperty1>> propertyExpression1, Expression<Func<TEntity, TProperty2>> propertyExpression2);

        int Update<TProperty1, TProperty2, TProperty3>(TEntity entity,
            Expression<Func<TEntity, TProperty1>> propertyExpression1,
            Expression<Func<TEntity, TProperty2>> propertyExpression2,
            Expression<Func<TEntity, TProperty3>> propertyExpression3);

        int Update<TProperty1, TProperty2, TProperty3, TProperty4>(TEntity entity,
            Expression<Func<TEntity, TProperty1>> propertyExpression1,
            Expression<Func<TEntity, TProperty2>> propertyExpression2,
            Expression<Func<TEntity, TProperty3>> propertyExpression3,
            Expression<Func<TEntity, TProperty4>> propertyExpression4
            );

        int Update<TProperty1, TProperty2, TProperty3, TProperty4, TProperty5>(TEntity entity,
            Expression<Func<TEntity, TProperty1>> propertyExpression1,
            Expression<Func<TEntity, TProperty2>> propertyExpression2,
            Expression<Func<TEntity, TProperty3>> propertyExpression3,
            Expression<Func<TEntity, TProperty4>> propertyExpression4,
            Expression<Func<TEntity, TProperty5>> propertyExpression5
            );

        int Update<TProperty1, TProperty2, TProperty3, TProperty4, TProperty5, TProperty6>(TEntity entity,
            Expression<Func<TEntity, TProperty1>> propertyExpression1,
            Expression<Func<TEntity, TProperty2>> propertyExpression2,
            Expression<Func<TEntity, TProperty3>> propertyExpression3,
            Expression<Func<TEntity, TProperty4>> propertyExpression4,
            Expression<Func<TEntity, TProperty5>> propertyExpression5,
            Expression<Func<TEntity, TProperty6>> propertyExpression6
            );

        Task<int> UpdateAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression, CancellationToken cancellationToken = default);

        Task<int> UpdateAsync<TProperty1, TProperty2>(TEntity entity, Expression<Func<TEntity, TProperty1>> propertyExpression1, Expression<Func<TEntity, TProperty2>> propertyExpression2, CancellationToken cancellationToken = default);

        Task<int> UpdateAsync<TProperty1, TProperty2, TProperty3>(TEntity entity,
            Expression<Func<TEntity, TProperty1>> propertyExpression1,
            Expression<Func<TEntity, TProperty2>> propertyExpression2,
            Expression<Func<TEntity, TProperty3>> propertyExpression3, CancellationToken cancellationToken = default
            );

        Task<int> UpdateAsync<TProperty1, TProperty2, TProperty3, TProperty4>(TEntity entity,
            Expression<Func<TEntity, TProperty1>> propertyExpression1,
            Expression<Func<TEntity, TProperty2>> propertyExpression2,
            Expression<Func<TEntity, TProperty3>> propertyExpression3,
            Expression<Func<TEntity, TProperty4>> propertyExpression4, CancellationToken cancellationToken = default
            );

        Task<int> UpdateAsync<TProperty1, TProperty2, TProperty3, TProperty4, TProperty5>(TEntity entity,
            Expression<Func<TEntity, TProperty1>> propertyExpression1,
            Expression<Func<TEntity, TProperty2>> propertyExpression2,
            Expression<Func<TEntity, TProperty3>> propertyExpression3,
            Expression<Func<TEntity, TProperty4>> propertyExpression4,
            Expression<Func<TEntity, TProperty5>> propertyExpression5, CancellationToken cancellationToken = default
            );

        Task<int> UpdateAsync<TProperty1, TProperty2, TProperty3, TProperty4, TProperty5, TProperty6>(TEntity entity,
            Expression<Func<TEntity, TProperty1>> propertyExpression1,
            Expression<Func<TEntity, TProperty2>> propertyExpression2,
            Expression<Func<TEntity, TProperty3>> propertyExpression3,
            Expression<Func<TEntity, TProperty4>> propertyExpression4,
            Expression<Func<TEntity, TProperty5>> propertyExpression5,
            Expression<Func<TEntity, TProperty6>> propertyExpression6, CancellationToken cancellationToken = default
            );

        int Update(TEntity entity, params string[] parameters);

        Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default, params string[] parameters);
    }
}
