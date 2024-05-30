using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WeihanLi.EntityFramework.Interceptors;

public interface IEntitySavingHandler
{
    void Handle(EntityEntry entityEntry);
}

public sealed class AutoUpdateInterceptor(IEnumerable<IEntitySavingHandler> handlers) : SaveChangesInterceptor
{
    private readonly IEntitySavingHandler[] _handlers = handlers.ToArray();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        OnSavingChanges(eventData);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        OnSavingChanges(eventData);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void OnSavingChanges(DbContextEventData eventData)
    {
        ArgumentNullException.ThrowIfNull(eventData.Context);
        foreach (var entityEntry in eventData.Context.ChangeTracker.Entries())
        {
            foreach (var handler in _handlers)
            {
                handler.Handle(entityEntry);
            }
        }
    }
}
