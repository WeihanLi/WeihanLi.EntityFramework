using System.IO;
using Microsoft.EntityFrameworkCore;

namespace WeihanLi.EntityFramework
{
    public static class EFRepositoryGeneratorExtensions
    {
        public static void GenerateRepositoryCodeFor<TDbContext>(this IEFRepositoryGenerator repositoryGenerator, string repositoryNamespace,
            string outputPath = null) where TDbContext : DbContext
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
            File.WriteAllText(outputPath, repositoryText);
        }
    }
}
