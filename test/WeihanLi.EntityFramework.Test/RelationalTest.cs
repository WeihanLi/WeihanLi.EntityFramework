using Microsoft.EntityFrameworkCore;
using Xunit;

namespace WeihanLi.EntityFramework.Test
{
    public class RelationalTest : EFTestBase
    {
        public RelationalTest(EFTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void IsRelationTest()
        {
            Assert.Equal(!Repository.DbContext.Database.IsInMemory(),
                Repository.DbContext.IsRelational());
        }
    }
}
