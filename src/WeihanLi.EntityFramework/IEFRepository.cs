﻿using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WeihanLi.Common.Data;

namespace WeihanLi.EntityFramework
{
    public interface IEFRepository<TDbContext, TEntity> : IRepository<TEntity>
        where TDbContext : DbContext
        where TEntity : class
    {
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

        Task<int> UpdateAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression);

        Task<int> UpdateAsync<TProperty1, TProperty2>(TEntity entity, Expression<Func<TEntity, TProperty1>> propertyExpression1, Expression<Func<TEntity, TProperty2>> propertyExpression2);

        Task<int> UpdateAsync<TProperty1, TProperty2, TProperty3>(TEntity entity,
            Expression<Func<TEntity, TProperty1>> propertyExpression1,
            Expression<Func<TEntity, TProperty2>> propertyExpression2,
            Expression<Func<TEntity, TProperty3>> propertyExpression3);

        Task<int> UpdateAsync<TProperty1, TProperty2, TProperty3, TProperty4>(TEntity entity,
            Expression<Func<TEntity, TProperty1>> propertyExpression1,
            Expression<Func<TEntity, TProperty2>> propertyExpression2,
            Expression<Func<TEntity, TProperty3>> propertyExpression3,
            Expression<Func<TEntity, TProperty4>> propertyExpression4
            );

        Task<int> UpdateAsync<TProperty1, TProperty2, TProperty3, TProperty4, TProperty5>(TEntity entity,
            Expression<Func<TEntity, TProperty1>> propertyExpression1,
            Expression<Func<TEntity, TProperty2>> propertyExpression2,
            Expression<Func<TEntity, TProperty3>> propertyExpression3,
            Expression<Func<TEntity, TProperty4>> propertyExpression4,
            Expression<Func<TEntity, TProperty5>> propertyExpression5
            );

        Task<int> UpdateAsync<TProperty1, TProperty2, TProperty3, TProperty4, TProperty5, TProperty6>(TEntity entity,
            Expression<Func<TEntity, TProperty1>> propertyExpression1,
            Expression<Func<TEntity, TProperty2>> propertyExpression2,
            Expression<Func<TEntity, TProperty3>> propertyExpression3,
            Expression<Func<TEntity, TProperty4>> propertyExpression4,
            Expression<Func<TEntity, TProperty5>> propertyExpression5,
            Expression<Func<TEntity, TProperty6>> propertyExpression6
            );

        int Update(TEntity entity, params string[] parameters);

        Task<int> UpdateAsync(TEntity entity, params string[] parameters);
    }
}
