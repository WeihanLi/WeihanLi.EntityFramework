using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WeihanLi.EntityFramework
{
    internal sealed class EFRepositoryFactory<TDbContext> : IEFRepositoryFactory<TDbContext>
    where TDbContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;

        public EFRepositoryFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEFRepository<TDbContext, TEntity> GetRepository<TEntity>() where TEntity : class
        {
            return _serviceProvider.GetRequiredService<IEFRepository<TDbContext, TEntity>>();
        }
    }
}
