using System;

namespace WeihanLi.EntityFramework
{
    public class EFRepositoryGeneratorOptions
    {
        public bool GenerateInterface { get; set; } = true;

        /// <summary>
        /// RepositoryNameResolver
        /// </summary>
        public Func<string, string> RepositoryNameResolver { get; set; }
            = entityName => $"{entityName}Repository";
    }
}
