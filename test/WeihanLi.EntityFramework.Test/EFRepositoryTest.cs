using System;
using Xunit;

namespace WeihanLi.EntityFramework.Test
{
    public class EFRepositoryTest : EFTestBase
    {
        public EFRepositoryTest(EFTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void InsertTest()
        {
            var entity = new TestEntity()
            {
                Name = "abc1",
                CreatedAt = DateTime.UtcNow,
                Extra = ""
            };
            Repository.Insert(entity);
        }
    }
}
