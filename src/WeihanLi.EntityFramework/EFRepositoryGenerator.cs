using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework
{
    public static class EFRepositoryGenerator
    {
        public static void GenerateRepositoryCodeFor<TDbContext>(string repositoryNamespace,
            string outputPath = null) where TDbContext : DbContext
        {
            var repositoryText = GenerateRepositoryCodeTextFor<TDbContext>(repositoryNamespace);
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

        public static string GenerateRepositoryCodeTextFor<TDbContext>(string repositoryNamespace) where TDbContext : DbContext
        {
            var dbContextType = typeof(TDbContext);
            var entities = dbContextType.GetProperties()
                .Where(p => typeof(IQueryable).IsAssignableFrom(p.PropertyType) && typeof(IInfrastructure<IServiceProvider>).IsAssignableFrom(p.PropertyType))
                .ToArray()
                ;

            var modelNamespaces = entities.Select(p => p.PropertyType.GetGenericArguments()[0].Namespace).Distinct().ToList();
            modelNamespaces.AddIfNotContains(dbContextType.Namespace);
            var entityNames = entities.Select(p => p.PropertyType.GetGenericArguments()[0].Name).ToArray();
            //
            var builder = new StringBuilder();
            builder.AppendLine("using WeihanLi.EntityFramework;");
            foreach (var @namespace in modelNamespaces)
            {
                builder.AppendLine($"using {@namespace};");
            }
            builder.AppendLine();
            builder.AppendLine($"namespace {repositoryNamespace}");
            builder.AppendLine("{");
            foreach (var name in entityNames)
            {
                builder.AppendLine(GenerateRepository(dbContextType.Name, name));
            }
            builder.AppendLine("}");

            return builder.ToString();
        }

        private static string GenerateRepository(string dbContextName, string entityName)
        {
            return $@"
    public partial interface I{entityName}Repository : IEFRepository<{dbContextName}, {entityName}> {{ }}
    public partial class {entityName}Repository : EFRepository<{dbContextName}, {entityName}>, I{entityName}Repository
    {{
        public {entityName}Repository({dbContextName} dbContext) : base(dbContext) {{ }}
    }}";
        }
    }
}
