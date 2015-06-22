namespace Query.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Voat.Models;
    using Voat.Models.ViewModels;
    using Voat.Queries.Session;
    using Xunit;

    [Trait("Category", "Semi-integration"), Trait("Subcategory", "Queries")]
    public class SessionQueries : IDisposable
    {
        private readonly whoaverseEntities dbContext;

        public SessionQueries()
        {
            var effortConnection = Effort.EntityConnectionFactory.CreateTransient(EntityString.Value);
            dbContext = new whoaverseEntities(effortConnection);

            dbContext.Set<Sessiontracker>().Add(new Sessiontracker
            {
                SessionId = "abc22",
                Subverse = "test"
            });
            dbContext.Set<Sessiontracker>().Add(new Sessiontracker
            {
                SessionId = "def111",
                Subverse = "test"
            });
            dbContext.Set<Sessiontracker>().Add(new Sessiontracker
            {
                SessionId = "abc22",
                Subverse = "test2"
            });
            dbContext.Set<Sessiontracker>().Add(new Sessiontracker
            {
                SessionId = "27818john",
                Subverse = "test"
            });

            dbContext.SaveChanges();
        }

        [Theory(
            DisplayName =
                "Checking for session existence should return 'true' if the session exists, 'false' otherwise.")]
        [InlineData("abc22", "test", true)]
        [InlineData("abc22", "test2", true)]
        [InlineData("def111", "test", true)]
        [InlineData("def111", "test2", false)]
        [InlineData("27818john", "test2", false)]
        [InlineData("27818john", "test", true)]
        [InlineData("no_session", "another_subverse", false)]
        public async Task CheckIfSessionExists(string sessionId, string subverse, bool expectedResult)
        {
            var result = await dbContext.Set<Sessiontracker>().SessionExistsAsync(sessionId, subverse);
            Assert.Equal(expectedResult, result);
        }

        [Theory(DisplayName = "Correct active session count should be returned for given subverse.")]
        [InlineData("test", 3)]
        [InlineData("test2", 1)]
        [InlineData("other_subverse", 0)]
        public async Task GettingActiveSessionCount(string subverse, int expectedCount)
        {
            var result = await dbContext.Set<Sessiontracker>().GetSubverseActiveSessionCountAsync(subverse);
            Assert.Equal(expectedCount, result);
        }

        [Fact(DisplayName = "Most active subverses should be fetched in correct order.")]
        public async Task GettingMostActiveSubverses()
        {
            var result = await dbContext.Set<Sessiontracker>().GetMostActiveSubversesAsync();
            var expectedList = new[]
            {
                new ActiveSubverseViewModel
                {
                    Name = "test",
                    UsersOnline = 3
                },
                new ActiveSubverseViewModel
                {
                    Name = "test2",
                    UsersOnline = 1
                }
            };

            Assert.Equal(expectedList.Length, result.Count);

            foreach (var item in result.Zip(expectedList, (r, e) => new {Result = r, Expected = e}))
            {
                Assert.Equal(item.Expected.Name, item.Result.Name);
                Assert.Equal(item.Expected.UsersOnline, item.Result.UsersOnline);
            }
        }

        [Fact(DisplayName = "Most active subverse list can be trimmed to desired length.")]
        public async Task MostActiveSubverseListCanBeTrimmed()
        {
            var result = await dbContext.Set<Sessiontracker>().GetMostActiveSubversesAsync(take: 1);
            var expectedList = new[]
            {
                new ActiveSubverseViewModel
                {
                    Name = "test",
                    UsersOnline = 3
                }
            };

            Assert.Equal(expectedList.Length, result.Count);

            foreach (var item in result.Zip(expectedList, (r, e) => new { Result = r, Expected = e }))
            {
                Assert.Equal(item.Expected.Name, item.Result.Name);
                Assert.Equal(item.Expected.UsersOnline, item.Result.UsersOnline);
            }
        }

        public void Dispose()
        {
            dbContext.Dispose();    
        }
    }
}
