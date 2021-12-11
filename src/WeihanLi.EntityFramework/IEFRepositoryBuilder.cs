using Microsoft.Extensions.DependencyInjection;
using System;

namespace WeihanLi.EntityFramework
{
    public interface IEFRepositoryBuilder
    {
        IServiceCollection Services { get; }
    }

    internal class EFRepositoryBuilder : IEFRepositoryBuilder
    {
        public IServiceCollection Services { get; }

        public EFRepositoryBuilder(IServiceCollection services) => Services = services ?? throw new ArgumentNullException(nameof(services));
    }
}
