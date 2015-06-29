namespace Voat.Queries.User
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Models;
    using Models.ViewModels;

    public static class SubscriptionQueries
    {
        public static Task<bool> IsSubverseSubscriberAsync(this IQueryable<Subscription> query,
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
                    .Select(x => new SubverseDetailsViewModel { Name = x.SubverseName })
                    .ToListAsync()
                    .ConfigureAwait(false);

            return list;
        }
    }
}