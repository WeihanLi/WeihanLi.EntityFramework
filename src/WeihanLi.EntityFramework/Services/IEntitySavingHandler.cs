using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace WeihanLi.EntityFramework.Services;

public interface IEntitySavingHandler
{
    void Handle(EntityEntry entityEntry);
}

