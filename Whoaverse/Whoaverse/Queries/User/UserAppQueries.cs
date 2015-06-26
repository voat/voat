namespace Voat.Queries.User
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Models;
    using Models.ViewModels;

    public static class UserAppQueries
    {
        public static Task<bool> IsSubverseAdminAsync(this IQueryable<SubverseAdmin> query, SubverseUserData userData)
        {
            return
                query
                    .AnyAsync(
                        a => a.Username == userData.UserName && a.SubverseName == userData.Subverse && a.Power == 1);
        }

        public static Task<bool> IsSubverseModeratorAsync(this IQueryable<SubverseAdmin> query,
            SubverseUserData userData)
        {
            return
                query
                    .AnyAsync(
                        a => a.Username == userData.UserName && a.SubverseName == userData.Subverse && a.Power == 2);
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
    }
}