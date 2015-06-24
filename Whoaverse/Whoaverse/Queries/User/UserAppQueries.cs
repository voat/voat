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
                    .AnyAsync(a => a.Username == userData.UserName && a.SubverseName == userData.Subverse && a.Power == 1);
        }

        public static Task<bool> IsSubverseModeratorAsync(this IQueryable<SubverseAdmin> query, SubverseUserData userData)
        {
            return
                query
                    .AnyAsync(a => a.Username == userData.UserName && a.SubverseName == userData.Subverse && a.Power == 2);
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

        public static async Task<IReadOnlyList<BadgeViewModel>> GetBadgesAsync(this IQueryable<Userbadge> query, string userName)
        {
            var list = await query.Where(b => b.Username == userName).Select(b => new BadgeViewModel
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

        private static IQueryable<INotification> FilterNotifications(this IQueryable<INotification> query,
            string userName)
        {
            return query.Where(m => m.Recipient == userName && m.Status && !m.Markedasunread);
        }

        public static Task<bool> HasNewMessagesAsync(this DbContext context, string userName)
        {
            var privateMessages =
                context.Set<Privatemessage>()
                    .FilterNotifications(userName)
                    .Select(x => x.Id);
            var commentReplies =
                context.Set<Commentreplynotification>()
                    .FilterNotifications(userName)
                    .Select(x => x.Id);
            var postReplies =
                context.Set<Postreplynotification>()
                    .FilterNotifications(userName)
                    .Select(x => x.Id);

            return privateMessages.Concat(commentReplies).Concat(postReplies).AnyAsync();
        }
    }
}