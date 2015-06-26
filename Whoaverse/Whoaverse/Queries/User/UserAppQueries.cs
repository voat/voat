namespace Voat.Queries.User
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Models;
    using Models.Quotas;
    using Models.ViewModels;

    public static class UserAppQueries
    {
        public static async Task<Permissions?> GetHighestPermissionsAsync(this IQueryable<SubverseAdmin> query,
            SubverseUserData userData)
        {
            var result =
                await
                    query.Where(x => x.Username == userData.UserName && x.SubverseName == userData.Subverse)
                        .GroupBy(x => new {x.Username, x.SubverseName})
                        .SelectMany(x => x.Select(y => (int?) y.Power).OrderBy(y => y))
                        .FirstOrDefaultAsync()
                        .ConfigureAwait(false);

            if (!result.HasValue)
            {
                return null;
            }

            if (!Enum.IsDefined(typeof (Permissions), result))
            {
                throw new InvalidOperationException("Unknown power level: " + result);
            }

            return (Permissions) result;
        }

        public static Task<bool> IsSubverseSubscriberAsync(this IQueryable<Subscription> query,
            SubverseUserData userData)
        {
            return query.AnyAsync(s => s.SubverseName == userData.Subverse && s.Username == userData.UserName);
        }

        public static Task<bool> IsBlockingSubverseAsync(this IQueryable<UserBlockedSubverse> query,
            SubverseUserData userData)
        {
            return query.AnyAsync(s => s.SubverseName == userData.Subverse && s.Username == userData.UserName);
        }

        public static Task<bool> IsSetSubscriberAsync(this IQueryable<Usersetsubscription> query, string userName,
            int setId)
        {
            return query.AnyAsync(x => x.Username == userName && x.Set_id == setId);
        }

        public static Task<int> GetSubscriptionCountAsync(this IQueryable<Subscription> query, string userName)
        {
            return query.CountAsync(x => x.Username == userName);
        }

        public static async Task<IReadOnlyList<SubverseDetailsViewModel>> GetSubscriptionsAsync(
            this IQueryable<Subscription> query, string userName)
        {
            var list =
                await query.Where(x => x.Username == userName)
                    .OrderBy(x => x.SubverseName)
                    .Select(x => new SubverseDetailsViewModel {Name = x.SubverseName})
                    .ToListAsync()
                    .ConfigureAwait(false);

            return list;
        }

        public static async Task<IReadOnlyList<BadgeViewModel>> GetBadgesAsync(this IQueryable<Userbadge> query,
            string userName)
        {
            var list = await query.Where(b => b.Username == userName)
                .Select(b => new BadgeViewModel
                {
                    Name = b.Badge.BadgeName,
                    Awarded = b.Awarded,
                    Id = b.Id,
                    UserName = b.Username,
                    Title = b.Badge.BadgeTitle,
                    Graphics = b.Badge.BadgeGraphics
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return list;
        }

        public static Task<bool> HasMessagesAsync(this IQueryable<INotificationCount> query, string userName)
        {
            return
                query.AnyAsync(
                    x => x.UserName == userName && (x.CommentReplies > 0 || x.PostReplies > 0 || x.PrivateMessages > 0));
        }

        public static async Task<NotificationCountModel> GetNotificationCountAsync(
            this IQueryable<INotificationCount> query,
            string userName)
        {
            var notificationCount =
                await query.Where(x => x.UserName == userName)
                    .AsNoTracking()
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);

            // TODO: Apparently all counts got infered as INT NULL, the view can be changed to force it to be INT
            return notificationCount == null
                ? new NotificationCountModel()
                : new NotificationCountModel(notificationCount.CommentReplies ?? 0, notificationCount.PostReplies ?? 0,
                    notificationCount.PrivateMessages ?? 0);
        }

        public static Task<int> VotesUsedAsync(this DbContext context, string userName)
        {
            var now = DateTime.Now;
            var startDate = now.Add(TimeSpan.FromHours(-24));

            Expression<Func<IVoteTracked, bool>> filter =
                tracked => tracked.Timestamp >= startDate && tracked.Timestamp <= now && tracked.UserName == userName;

            var commentVotesUsed =
                context.Set<Commentvotingtracker>()
                    .Where(filter)
                    .Select(x => new {Vote = 1});
            var submissionVotesUsed =
                context.Set<Votingtracker>()
                    .Where(filter)
                    .Select(x => new {Vote = 1});
            return
                commentVotesUsed.Concat(submissionVotesUsed)
                    .GroupBy(_ => 1)
                    .Select(x => x.Count())
                    .DefaultIfEmpty(0)
                    .FirstAsync();
        }

        public static async Task<CommentCountModel> GetCommentCountAsync(this IQueryable<Comment> query, string userName)
        {
            var now = DateTime.Now;
            var lastDay = now.AddDays(-1);
            var count =
                await
                    query.CountAsync(x => x.Name == userName && x.Date >= lastDay && x.Date <= now)
                        .ConfigureAwait(false);
            return new CommentCountModel {LastDayTotalCommentCount = count};
        }

        public static Task<PostCountModel> GetPostCountAsync(this IQueryable<Message> query, QuotaRetrievalData data)
        {
            var now = DateTime.Now;
            var lastDay = now.AddDays(-1);
            var lastHour = now.AddHours(-1);

            return query.GroupBy(_ => 1)
                .Select(g =>
                    new PostCountModel
                    {
                        DailyCrossPostCount =
                            g.Count(
                                x =>
                                    x.MessageContent == data.Url
                                    && x.Name == data.UserName
                                    && x.Date >= lastDay
                                    && x.Date <= now),
                        LastHourSubPostCount =
                            g.Count(
                                x =>
                                    x.Subverse == data.Subverse
                                    && x.Name == data.UserName
                                    && x.Date >= lastHour
                                    && x.Date <= now),
                        LastDaySubPostCount =
                            g.Count(
                                x =>
                                    x.Subverse == data.Subverse
                                    && x.Name == data.UserName
                                    && x.Date >= lastDay
                                    && x.Date <= now),
                        LastDayTotalPostCount =
                            g.Count(
                                x =>
                                    x.Name == data.UserName
                                    && x.Date >= lastDay
                                    && x.Date <= now)
                    })
                .FirstAsync();
        }
    }

    public class QuotaRetrievalData
    {
        public QuotaRetrievalData(string userName, string subverse, string url)
        {
            UserName = userName;
            Subverse = subverse;
            Url = url;
        }

        public string UserName { get; private set; }
        public string Subverse { get; private set; }
        public string Url { get; private set; }
    }
}