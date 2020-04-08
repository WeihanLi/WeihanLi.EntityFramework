using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
    }
}
