using System;
using Microsoft.EntityFrameworkCore;
using WeihanLi.Common.Data;

namespace WeihanLi.EntityFramework
{
    public interface IEFUnitOfWork<out TDbContext> : IUnitOfWork, IDisposable where TDbContext : DbContext
    {
        TDbContext DbContext { get; }

        IEFRepository<TDbContext, TEntity> GetRepository<TEntity>() where TEntity : class;
    }
}
