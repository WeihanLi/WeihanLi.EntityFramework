using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WeihanLi.Common.Data;

namespace WeihanLi.EntityFramework
{
    public interface IEFRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        //
        int Update(TEntity entity);

        int Update(TEntity entity, string[] parameters);

        int Update<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression);

        Task<int> UpdateAsync(TEntity entity);

        Task<int> UpdateAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression);

        Task<int> UpdateAsync(TEntity entity, string[] parameters);
    }
}
