namespace Query.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using Effort;
    using Voat.Models;
    using Voat.Models.ViewModels;
    using Voat.Queries.User;
    using Xunit;

    public sealed class SubverseDetailsViewModelEqualityComparer : IEqualityComparer<SubverseDetailsViewModel>
    {
        public bool Equals(SubverseDetailsViewModel x, SubverseDetailsViewModel y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return string.Equals(x.Name, y.Name) && string.Equals(x.Title, y.Title)
                   && string.Equals(x.Description, y.Description) && x.Creation_date.Equals(y.Creation_date)
                   && x.Subscribers == y.Subscribers;
        }

        public int GetHashCode(SubverseDetailsViewModel obj)
        {
            unchecked
            {
                var hashCode = (obj.Name != null ? obj.Name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (obj.Title != null ? obj.Title.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (obj.Description != null ? obj.Description.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ obj.Creation_date.GetHashCode();
                hashCode = (hashCode*397) ^ obj.Subscribers.GetHashCode();
                return hashCode;
            }
        }
    }

    public sealed class BadgeViewModelEqualityComparer : IEqualityComparer<BadgeViewModel>
    {
        public bool Equals(BadgeViewModel x, BadgeViewModel y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Id == y.Id && string.Equals(x.UserName, y.UserName) && string.Equals(x.Name, y.Name)
                   && string.Equals(x.Graphics, y.Graphics) && string.Equals(x.Title, y.Title)
                   && x.Awarded.Equals(y.Awarded);
        }

        public int GetHashCode(BadgeViewModel obj)
        {
            unchecked
            {
                var hashCode = obj.Id;
                hashCode = (hashCode*397) ^ (obj.UserName != null ? obj.UserName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (obj.Name != null ? obj.Name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (obj.Graphics != null ? obj.Graphics.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (obj.Title != null ? obj.Title.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ obj.Awarded.GetHashCode();
                return hashCode;
            }
        }
    }


    [Trait("Category", "Semi-integration"), Trait("Subcategory", "Queries")]
    public class UserQueries : IDisposable
    {
        private readonly DbContext context;

        public UserQueries()
        {
            var effortConnection = EntityConnectionFactory.CreateTransient(EntityString.Value);
            context = new whoaverseEntities(effortConnection);

            context.Set<Subverse>().Add(new Subverse
            {
                name = "test1",
                title = "Test1"
            });
            context.Set<Subverse>().Add(new Subverse
            {
                name = "test2",
                title = "Test2"
            });

            context.Set<SubverseAdmin>().Add(new SubverseAdmin
            {
                SubverseName = "test1",
                Username = "johnny",
                Power = 1
            });
            context.Set<SubverseAdmin>().Add(new SubverseAdmin
            {
                SubverseName = "test2",
                Username = "jane",
                Power = 2
            });
            context.Set<Subscription>().Add(new Subscription
            {
                SubverseName = "test1",
                Username = "dorothy"
            });
            context.Set<Subscription>().Add(new Subscription
            {
                SubverseName = "test2",
                Username = "dorothy"
            });
            context.Set<UserBlockedSubverse>().Add(new UserBlockedSubverse
            {
                SubverseName = "test2",
                Username = "dan"
            });
            context.Set<Userset>().Add(new Userset
            {
                Set_id = 1,
                Name = "Just a set",
                Created_by = "admin",
                Description = string.Empty
            });
            context.Set<Usersetsubscription>().Add(new Usersetsubscription
            {
                Set_id = 1,
                Username = "joan"
            });
            context.Set<Privatemessage>().Add(new Privatemessage
            {
                Recipient = "joan",
                Status = true,
                Body = string.Empty,
                Subject = string.Empty,
                Sender = "no-sender",
                Markedasunread = false
            });
            context.Set<Privatemessage>().Add(new Privatemessage
            {
                Recipient = "dorothy",
                Status = false,
                Body = string.Empty,
                Subject = string.Empty,
                Sender = "no-sender",
                Markedasunread = true
            });
            context.Set<Privatemessage>().Add(new Privatemessage
            {
                Recipient = "dan",
                Status = true,
                Body = string.Empty,
                Subject = string.Empty,
                Sender = "no-sender",
                Markedasunread = true
            });
            context.Set<Commentreplynotification>().Add(new Commentreplynotification
            {
                Recipient = "harry",
                Status = true,
                Body = string.Empty,
                Subject = string.Empty,
                Sender = "no-sender",
                Subverse = "test1",
                Markedasunread = false
            });
            context.Set<Postreplynotification>().Add(new Postreplynotification
            {
                Recipient = "henry",
                Status = true,
                Body = string.Empty,
                Subject = string.Empty,
                Sender = "no-sender",
                Subverse = "test1",
                Markedasunread = false
            });

            var badges = new[]
            {
                context.Set<Badge>().Add(new Badge
                {
                    BadgeId = "b1",
                    BadgeGraphics = "abc1",
                    BadgeTitle = "test badge",
                    BadgeName = "another test"
                }),
                context.Set<Badge>().Add(new Badge
                {
                    BadgeId = "b2",
                    BadgeGraphics = "abc2",
                    BadgeTitle = "test badge2",
                    BadgeName = "yet another test"
                })
            };

            context.Set<Userbadge>().Add(new Userbadge
            {
                Id = 1,
                Awarded = new DateTime(2015, 2, 20),
                Badge = badges[0],
                Username = "jane"
            });

            context.Set<Userbadge>().Add(new Userbadge
            {
                Id = 2,
                Awarded = new DateTime(2015, 3, 14),
                Badge = badges[1],
                Username = "jane"
            });

            context.Set<Userbadge>().Add(new Userbadge
            {
                Id = 3,
                Awarded = new DateTime(2015, 4, 11),
                Badge = badges[0],
                Username = "johnny"
            });

            try
            {
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Theory(DisplayName = "Subverse admin check correctly identifies admins")]
        [InlineData("test1", "johnny", true)]
        [InlineData("test2", "johnny", false)]
        [InlineData("test2", "jane", false)]
        [InlineData("doesntexist", "jane", false)]
        [InlineData("test1", "doesntexist", false)]
        public async Task SubverseAdminCheck(string subverse, string username, bool expectedResult)
        {
            var result =
                await context.Set<SubverseAdmin>().IsSubverseAdminAsync(new SubverseUserData(username, subverse));

            Assert.Equal(expectedResult, result);
        }

        [Theory(DisplayName = "Subverse moderator check correctly identifies moderators")]
        [InlineData("test1", "johnny", false)]
        [InlineData("test2", "johnny", false)]
        [InlineData("test2", "jane", true)]
        [InlineData("doesntexist", "jane", false)]
        [InlineData("test1", "doesntexist", false)]
        public async Task SubverseModeratorCheck(string subverse, string username, bool expectedResult)
        {
            var result =
                await context.Set<SubverseAdmin>().IsSubverseModeratorAsync(new SubverseUserData(username, subverse));

            Assert.Equal(expectedResult, result);
        }

        [Theory(DisplayName = "Subverse subscription check correctly identifies subscribers")]
        [InlineData("test1", "johnny", false)]
        [InlineData("test2", "johnny", false)]
        [InlineData("test2", "dorothy", true)]
        [InlineData("test1", "dorothy", true)]
        [InlineData("doesntexist", "jane", false)]
        [InlineData("test1", "doesntexist", false)]
        public async Task SubverseSubscriberCheck(string subverse, string username, bool expectedResult)
        {
            var result =
                await context.Set<Subscription>().IsSubverseSubscriberAsync(new SubverseUserData(username, subverse));

            Assert.Equal(expectedResult, result);
        }

        [Theory(DisplayName = "Subverse blocking check correctly identifies users who have specified subverse blocked")]
        [InlineData("test1", "johnny", false)]
        [InlineData("test2", "johnny", false)]
        [InlineData("test2", "dan", true)]
        [InlineData("test1", "dan", false)]
        [InlineData("doesntexist", "jane", false)]
        [InlineData("test1", "doesntexist", false)]
        public async Task SubverseBlockCheck(string subverse, string username, bool expectedResult)
        {
            var result =
                await context.Set<UserBlockedSubverse>().IsBlockingSubverseAsync(new SubverseUserData(username, subverse));

            Assert.Equal(expectedResult, result);
        }

        [Theory(DisplayName = "Subverse blocking check correctly identifies users who have specified subverse blocked")]
        [InlineData(1, "johnny", false)]
        [InlineData(1, "joan", true)]
        [InlineData(40, "jane", false)]
        [InlineData(1, "doesntexist", false)]
        public async Task SetSubscriptionCheck(int setId, string username, bool expectedResult)
        {
            var result =
                await context.Set<Usersetsubscription>().IsSetSubscriberAsync(username, setId);

            Assert.Equal(expectedResult, result);
        }

        [Theory(DisplayName = "Subverse subscription count is correctly retrieved for various users.")]
        [InlineData("johnny", 0)]
        [InlineData("joan", 0)]
        [InlineData("dorothy", 2)]
        [InlineData("doesntexist", 0)]
        public async Task SubscriptionCount(string username, int expectedResult)
        {
            var result =
                await context.Set<Subscription>().GetSubscriptionCountAsync(username);

            Assert.Equal(expectedResult, result);
        }

        [Fact(DisplayName = "Subscription list is correctly retrieved for specified user.")]
        public async Task SubscriptionListRetrieval()
        {
            var result = await context.Set<Subscription>().GetSubscriptionsAsync("dorothy");

            var expectedResult = new[]
            {
                new SubverseDetailsViewModel{Name = "test1"},
                new SubverseDetailsViewModel{Name = "test2"}
            };

            Assert.Equal(expectedResult, result, new SubverseDetailsViewModelEqualityComparer());
        }

        [Fact(DisplayName = "Badges are correctly retrieved for specified user.")]
        public async Task BadgeRetrieval()
        {
            var result = await context.Set<Userbadge>().GetBadgesAsync("jane");

            var expected = new []
            {
                new BadgeViewModel
                {
                    Awarded = new DateTime(2015, 2, 20),
                    Id = 1,
                    Graphics = "abc1",
                    Title = "test badge",
                    Name = "another test",
                    UserName = "jane"
                },
                new BadgeViewModel
                {
                    Awarded = new DateTime(2015, 3, 14),
                    Id = 2,
                    Graphics = "abc2",
                    Title = "test badge2",
                    Name = "yet another test",
                    UserName = "jane"
                }
            };

            Assert.Equal(expected, result, new BadgeViewModelEqualityComparer());
        }

        [Theory(DisplayName = "New message check should check private messages, comment replies and post replies.")]
        [InlineData("harry", true)]
        [InlineData("henry", true)]
        [InlineData("joan", true)]
        [InlineData("dorothy", false)]
        [InlineData("dan", false)]
        [InlineData("doesntexist", false)]
        public async Task NewMessagesCheck(string userName, bool expectedResult)
        {
            var result = await context.HasNewMessagesAsync(userName);

            Assert.Equal(expectedResult, result);
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
