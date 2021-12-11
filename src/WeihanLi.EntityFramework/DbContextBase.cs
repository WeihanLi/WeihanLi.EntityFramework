using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace WeihanLi.EntityFramework;

/// <summary>
/// DbContextBase
/// Custom DbContext template
/// </summary>
public abstract class DbContextBase : DbContext
{
    protected DbContextBase()
    {
    }

    protected DbContextBase(DbContextOptions dbContextOptions) : base(dbContextOptions)
    {
    }

    protected virtual Task BeforeSaveChanges() => Task.CompletedTask;

    protected virtual Task AfterSaveChanges() => Task.CompletedTask;

    public override int SaveChanges()
    {
        BeforeSaveChanges().ConfigureAwait(false).GetAwaiter().GetResult();
        var result = base.SaveChanges();
        AfterSaveChanges().ConfigureAwait(false).GetAwaiter().GetResult();
        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await BeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        await AfterSaveChanges();
        return result;
    }
}
