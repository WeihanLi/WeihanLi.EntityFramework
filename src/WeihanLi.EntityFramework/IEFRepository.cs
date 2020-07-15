using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using WeihanLi.Common.Data;
using WeihanLi.Common.Models;

namespace WeihanLi.EntityFramework
{
    public interface IEFRepository<out TDbContext, TEntity> : IRepository<TEntity>
        where TDbContext : DbContext
        where TEntity : class
    {
        TDbContext DbContext { get; }

        /// <summary>
        /// Find an entity
        /// </summary>
        /// <param name="keyValues">keyValues</param>
        /// <returns>the entity founded, if not found, null returned</returns>
        TEntity Find(params object[] keyValues);

        /// <summary>
        /// Find an entity
        /// </summary>
        /// <param name="keyValues">keyValues</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>the entity founded, if not found, null returned</returns>
        ValueTask<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken);

        /// <summary>
        /// Delete a entity
        /// </summary>
        /// <param name="keyValues">keyValues</param>
        /// <returns>affected rows</returns>
        int Delete(params object[] keyValues);

        /// <summary>
        /// Delete a entity
        /// </summary>
        /// <param name="keyValues">entity</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>affected rows</returns>
        Task<int> DeleteAsync(object[] keyValues, CancellationToken cancellationToken);

        /// <summary>
        /// Delete a entity
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns>affected rows</returns>
        int Delete(TEntity entity);

        /// <summary>
        /// Delete a entity
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>affected rows</returns>
        Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="propertyNames">properties to update</param>
        /// <returns>affected rows</returns>
        int Update(TEntity entity, params string[] propertyNames);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="propertyNames">properties not to update</param>
        /// <returns>affected rows</returns>
        int UpdateWithout(TEntity entity, params string[] propertyNames);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="propertyNames">properties to update</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>affected rows</returns>
        Task<int> UpdateAsync(TEntity entity, string[] propertyNames, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="propertyNames">properties not to update</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>affected rows</returns>
        Task<int> UpdateWithoutAsync(TEntity entity, string[] propertyNames, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="propertyExpressions">properties to update</param>
        /// <returns>affected rows</returns>
        int Update(TEntity entity, params Expression<Func<TEntity, object>>[] propertyExpressions);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="propertyExpressions">properties not to update</param>
        /// <returns>affected rows</returns>
        int UpdateWithout(TEntity entity, params Expression<Func<TEntity, object>>[] propertyExpressions);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="propertyExpressions">properties to update</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>affected rows</returns>
        Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, object>>[] propertyExpressions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="propertyExpressions">properties not to update</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>affected rows</returns>
        Task<int> UpdateWithoutAsync(TEntity entity, Expression<Func<TEntity, object>>[] propertyExpressions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the <see cref="IQueryable{TEntity}"/> based on a predicate
        /// </summary>
        /// <param name="queryBuilderAction">queryBuilder</param>
        /// <remarks>This method default no-tracking query.</remarks>
        IQueryable<TEntity> Query(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null);

        /// <summary>
        /// Gets the <see cref="List{TEntity}"/> based on a predicate
        /// </summary>
        /// <param name="queryBuilderAction">queryBuilderAction</param>
        /// <remarks>This method default no-tracking query.</remarks>
        List<TEntity> Get(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null);

        /// <summary>
        /// Gets the <see cref="List{TEntity}"/> based on a predicate
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="queryBuilderAction">queryBuilderAction</param>
        /// <remarks>This method default no-tracking query.</remarks>
        List<TResult> GetResult<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null);

        /// <summary>
        /// Gets the <see cref="List{TEntity}"/> based on a predicate
        /// </summary>
        /// <param name="queryBuilderAction">queryBuilder</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <remarks>This method default no-tracking query.</remarks>
        Task<List<TEntity>> GetAsync(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the <see cref="List{TResult}"/> based on a predicate
        /// </summary>
        /// <param name="selector">selector</param>
        /// <param name="queryBuilderAction">queryBuilder</param>
        /// <param name="cancellationToken"></param>
        /// <remarks>This method default no-tracking query.</remarks>
        Task<List<TResult>> GetResultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the <see cref="List{TEntity}"/> based on a predicate
        /// </summary>
        /// <param name="queryBuilderAction">queryBuilderAction</param>
        /// <remarks>This method default no-tracking query.</remarks>
        bool Any(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null);

        /// <summary>
        /// Gets the <see cref="List{TEntity}"/> based on a predicate
        /// </summary>
        /// <param name="queryBuilderAction">queryBuilder</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <remarks>This method default no-tracking query.</remarks>
        Task<bool> AnyAsync(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the <see cref="List{TEntity}"/> based on a predicate
        /// </summary>
        /// <param name="queryBuilderAction">queryBuilderAction</param>
        /// <remarks>This method default no-tracking query.</remarks>
        TEntity FirstOrDefault(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null);

        /// <summary>
        /// Gets the <see cref="List{TEntity}"/> based on a predicate
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="queryBuilderAction">queryBuilderAction</param>
        /// <remarks>This method default no-tracking query.</remarks>
        TResult FirstOrDefaultResult<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null);

        /// <summary>
        /// Gets the <see cref="List{TEntity}"/> based on a predicate
        /// </summary>
        /// <param name="queryBuilderAction">queryBuilder</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <remarks>This method default no-tracking query.</remarks>
        Task<TEntity> FirstOrDefaultAsync(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the <see cref="List{TResult}"/> based on a predicate
        /// </summary>
        /// <param name="selector">selector</param>
        /// <param name="queryBuilderAction">queryBuilder</param>
        /// <param name="cancellationToken"></param>
        /// <remarks>This method default no-tracking query.</remarks>
        Task<TResult> FirstOrDefaultResultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the <see cref="IPagedListResult{TEntity}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
        /// </summary>
        /// <param name="queryBuilderAction">queryBuilderAction</param>
        /// <param name="pageNumber">The pageNumber of page.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <returns>An <see cref="IPagedListResult{TEntity}"/> that contains elements that satisfy the condition specified by <paramref name="queryBuilderAction"/>.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        IPagedListResult<TEntity> GetPagedList(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, int pageNumber = 1, int pageSize = 20);

        /// <summary>
        /// Gets the <see cref="IPagedListResult{TEntity}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
        /// </summary>
        /// <param name="queryBuilderAction">A function to test each element for a condition.</param>
        /// <param name="pageNumber">The number of page.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>An <see cref="IPagedListResult{TEntity}"/> that contains elements that satisfy the condition specified by <paramref name="queryBuilderAction"/>.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        Task<IPagedListResult<TEntity>> GetPagedListAsync(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the <see cref="IPagedListResult{TResult}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
        /// </summary>
        /// <param name="selector">The selector for projection.</param>
        /// <param name="queryBuilderAction">A function to test each element for a condition.</param>
        /// <param name="pageNumber">pageNumber</param>
        /// <param name="pageSize">pageSize</param>
        /// <returns>An <see cref="IPagedListResult{TResult}"/> that contains elements that satisfy the condition specified by <paramref name="queryBuilderAction"/>.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        IPagedListResult<TResult> GetPagedListResult<TResult>([NotNull]Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null,
                                                  int pageNumber = 1, int pageSize = 20);

        /// <summary>
        /// Gets the <see cref="IPagedListResult{TEntity}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
        /// </summary>
        /// <param name="selector">The selector for projection.</param>
        /// <param name="queryBuilderAction">A function to test each element for a condition.</param>
        /// <param name="pageSize"></param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <param name="pageNumber"></param>
        /// <returns>An <see cref="IPagedListResult{TEntity}"/> that contains elements that satisfy the condition specified by <paramref name="queryBuilderAction"/>.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        Task<IPagedListResult<TResult>> GetPagedListResultAsync<TResult>([NotNull]Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null,
                                                             int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    }
}
