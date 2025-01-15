using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace WeihanLi.EntityFramework;

internal sealed class DbContextOptionsConfiguration<TContext>(
    Action<IServiceProvider, DbContextOptionsBuilder> optionsAction
    )
    : IDbContextOptionsConfiguration<TContext>
    where TContext : DbContext
{
    private readonly Action<IServiceProvider, DbContextOptionsBuilder> _optionsAction = optionsAction ?? throw new ArgumentNullException(nameof(optionsAction));

    public void Configure(IServiceProvider serviceProvider, DbContextOptionsBuilder optionsBuilder)
    {
        _optionsAction.Invoke(serviceProvider, optionsBuilder);
    }
}
