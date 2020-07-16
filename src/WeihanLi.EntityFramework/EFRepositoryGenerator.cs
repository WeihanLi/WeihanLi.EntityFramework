using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework
{
    internal sealed class EFRepositoryGenerator : IEFRepositoryGenerator
    {
        private readonly EFRepositoryGeneratorOptions _generatorOptions;

        public EFRepositoryGenerator(IOptions<EFRepositoryGeneratorOptions> options)
        {
            _generatorOptions = options.Value;
        }

        public string GenerateRepositoryCodeTextFor<TDbContext>(string repositoryNamespace) where TDbContext : DbContext
        {
            var dbContextType = typeof(TDbContext);
            var entities = dbContextType.GetProperties()
                .Where(p => p.PropertyType.IsGenericType && typeof(DbSet<>) == p.PropertyType.GetGenericTypeDefinition())
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

        private string GenerateRepository(string dbContextName, string entityName)
        {
            var repositoryName = _generatorOptions.RepositoryNameResolver(entityName);
            if (_generatorOptions.GenerateInterface)
            {
                return $@"
    public partial interface I{repositoryName} : IEFRepository<{dbContextName}, {entityName}> {{ }}
    public partial class {repositoryName} : EFRepository<{dbContextName}, {entityName}>, I{repositoryName}
    {{
        public {repositoryName}({dbContextName} dbContext) : base(dbContext) {{ }}
    }}";
            }
            else
            {
                return $@"
    public partial class {repositoryName} : EFRepository<{dbContextName}, {entityName}>
    {{
        public {repositoryName}({dbContextName} dbContext) : base(dbContext) {{ }}
    }}";
            }
        }
    }
}
