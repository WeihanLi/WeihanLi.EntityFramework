using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WeihanLi.Common.Aspect;

namespace WeihanLi.EntityFramework
{
    public static class ServiceCollectionExtension
    {
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
    }
}
