using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WeihanLi.Common.Aspect;

namespace WeihanLi.EntityFramework
{
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// AddEFRepository
        /// </summary>
        /// <param name="services">services</param>
        /// <param name="efServiceLifetime">ef dbContext and repository serviceLifetime</param>
        /// <returns></returns>
        public static IEFRepositoryBuilder AddEFRepository(this IServiceCollection services, ServiceLifetime efServiceLifetime = ServiceLifetime.Scoped)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAdd(new ServiceDescriptor(typeof(IEFRepository<,>), typeof(EFRepository<,>), efServiceLifetime));
            services.TryAdd(new ServiceDescriptor(typeof(IEFUnitOfWork<>), typeof(EFUnitOfWork<>), efServiceLifetime));
            services.TryAdd(new ServiceDescriptor(typeof(IEFRepositoryFactory<>), typeof(EFRepositoryFactory<>), efServiceLifetime));
            services.TryAddSingleton<IEFRepositoryGenerator, EFRepositoryGenerator>();

            return new EFRepositoryBuilder(services);
        }

        /// <summary>
        /// AddProxyDbContext
        /// </summary>
        /// <typeparam name="TDbContext">DbContext Type</typeparam>
        /// <param name="services">services</param>
        /// <param name="optionsAction">optionsAction</param>
        /// <param name="serviceLifetime">serviceLifetime</param>
        /// <returns></returns>
        public static IServiceCollection AddProxyDbContext<TDbContext>(this IServiceCollection services,
            Action<DbContextOptionsBuilder> optionsAction, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) where TDbContext : DbContext
        {
            services.AddDbContext<TDbContext>(optionsAction, serviceLifetime);
            services.AddProxyService<TDbContext>(serviceLifetime);
            return services;
        }

        /// <summary>
        /// AddProxyDbContextPool
        /// </summary>
        /// <typeparam name="TDbContext">DbContext Type</typeparam>
        /// <param name="services">services</param>
        /// <param name="optionsAction">optionsAction</param>
        /// <param name="poolSize">poolSize</param>
        /// <param name="serviceLifetime">serviceLifetime</param>
        /// <returns></returns>
        public static IServiceCollection AddProxyDbContextPool<TDbContext>(this IServiceCollection services,
            Action<DbContextOptionsBuilder> optionsAction, int poolSize = 100, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) where TDbContext : DbContext
        {
            services.AddDbContextPool<TDbContext>(optionsAction, poolSize);
            services.Add(new ServiceDescriptor(typeof(TDbContext), sp =>
            {
                var dbContext = sp.GetService<DbContextPool<TDbContext>.Lease>().Context;
                var proxyFactory = sp.GetRequiredService<IProxyFactory>();
                return proxyFactory.CreateProxyWithTarget<TDbContext, TDbContext>(dbContext);
            }, serviceLifetime));

            return services;
        }
    }
}
