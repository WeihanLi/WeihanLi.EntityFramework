using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;

namespace WeihanLi.EntityFramework
{
    public static class EFRepositoryGeneratorExtensions
    {
        public static Task GenerateRepositoryCodeFor<TDbContext>(this IEFRepositoryGenerator repositoryGenerator, string repositoryNamespace,
            string? outputPath = null) where TDbContext : DbContext
        {
            var repositoryText = repositoryGenerator.GenerateRepositoryCodeTextFor<TDbContext>(repositoryNamespace);
            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = $"{typeof(TDbContext).Name.Replace("DbContext", "").Replace("Context", "")}Repository";
            }
            if (!outputPath.EndsWith(".cs"))
            {
                outputPath += ".generated.cs";
            }
            return File.WriteAllTextAsync(outputPath, repositoryText);
        }
    }
}
