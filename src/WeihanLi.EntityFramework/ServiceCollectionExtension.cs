using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WeihanLi.EntityFramework
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddEFRepository(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddScoped(typeof(IEFRepository<,>), typeof(EFRepository<,>));
            return services;
        }
    }
}
