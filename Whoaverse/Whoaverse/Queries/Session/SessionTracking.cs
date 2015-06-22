namespace Voat.Queries.Session
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Models;
    using Models.ViewModels;

    public static class SessionTracking
    {
        public static Task<bool> SessionExistsAsync(this IQueryable<Sessiontracker> query, string sessionId, string subverse)
        {
            return
                query
                    .AsNoTracking()
                    .Where(s => s.SessionId == sessionId && s.Subverse == subverse)
                    .AnyAsync();
        }

        public static Task<int> GetSubverseActiveSessionCountAsync(this IQueryable<Sessiontracker> query, string subverse)
        {
            return
                query
                    .AsNoTracking()
                    .Where(s => s.Subverse == subverse)
                    .CountAsync();
        }

        public static async Task<IReadOnlyList<ActiveSubverseViewModel>> GetMostActiveSubversesAsync(
            this IQueryable<Sessiontracker> query, int take = 7)
        {
            var items =
                await query
                    .AsNoTracking()
                    .GroupBy(s => s.Subverse)
                    .Select(s => new ActiveSubverseViewModel
                    {
                        Name = s.Key,
                        UsersOnline = s.Count()
                    })
                    .OrderByDescending(vm => vm.UsersOnline)
                    .Take(take)
                    .ToListAsync()
                    .ConfigureAwait(false);

            return items;
        }
    }
}