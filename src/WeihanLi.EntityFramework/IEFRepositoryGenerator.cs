using Microsoft.EntityFrameworkCore;

namespace WeihanLi.EntityFramework
{
    public interface IEFRepositoryGenerator
    {
        string GenerateRepositoryCodeTextFor<TDbContext>(string repositoryNamespace) where TDbContext : DbContext;
    }
}
