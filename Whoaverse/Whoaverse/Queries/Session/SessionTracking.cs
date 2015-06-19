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
        public static Task<bool> SessionExistsAsync(this DbContext context, string sessionId, string subverse)
        {
            return
                context.Set<Sessiontracker>()
                    .AsNoTracking()
                    .Where(s => s.SessionId == sessionId && s.Subverse == subverse)
                    .AnyAsync();
        }

        public static Task<int> GetSubverseActiveSessionCountAsync(this DbContext context, string subverse)
        {
            return
                context.Set<Sessiontracker>()
                    .AsNoTracking()
                    .Where(s => s.Subverse == subverse)
                    .CountAsync();
        }

        public static async Task<IReadOnlyList<ActiveSubverseViewModel>> GetMostActiveSubversesAsync(
            this DbContext context, int take = 7)
        {
            var items =
                await context.Set<Sessiontracker>()
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