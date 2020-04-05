using Xunit;

namespace WeihanLi.EntityFramework.Test
{
    public class EFExtensionsTest : EFTestBase
    {
        [Fact]
        public void GetTableNameTest()
        {
            if (Repository.DbContext.IsRelationalDatabase())
            {
                var tableName = Repository.DbContext.GetTableName<TestEntity>();
                Assert.Equal("TestEntities", tableName);
            }
        }

        public EFExtensionsTest(EFTestFixture fixture) : base(fixture)
        {
        }
    }
}
