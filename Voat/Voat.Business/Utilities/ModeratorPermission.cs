using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Domain.Models;
using Voat.Domain.Query;

namespace Voat.Utilities
{
    public static class ModeratorPermission
    {
        private static IEnumerable<Data.Models.SubverseModerator> GetModerators(string subverse)
        {
            var q = new QuerySubverseModerators(subverse);
            return q.Execute();
        }
        public static bool IsModerator(string userName, string subverse)
        {
            var mods = GetModerators(subverse);
            return mods.Any(x => x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
        }
        public static bool IsLevel(string userName, string subverse, ModeratorLevel level)
        {
            var mods = GetModerators(subverse);
            return mods.Any(x => x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) && x.Power == (int)level);
        }
        public static ModeratorLevel? Level(string userName, string subverse)
        {
            var mods = GetModerators(subverse);
            var o = mods.FirstOrDefault(x => x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
            if (o != null)
            {
                return (ModeratorLevel)Enum.Parse(typeof(ModeratorLevel), o.Power.ToString());
            }
            return null;
        }
        public static bool HasPermission(string userName, string subverse, ModeratorAction action)
        {
            bool result = false;
            if (!String.IsNullOrEmpty(userName) && !String.IsNullOrEmpty(subverse))
            {
                var r = GetModerators(subverse);

                if (r != null && r.Any())
                {
                    var levelEvaluator = new Func<int, IEnumerable<ModeratorLevel>, bool>((currentLevel, allowedLevels) =>
                    {
                        bool allowed = false;
                        if (allowedLevels != null && allowedLevels.Any())
                        {
                            allowed = allowedLevels.Any(x => currentLevel == (int)x);
                        }
                        return allowed;
                    });

                    switch (action)
                    {
                        case ModeratorAction.InviteMods:
                        case ModeratorAction.RemoveMods:
                        case ModeratorAction.ModifySettings:
                        case ModeratorAction.AssignStickies:
                            result = r.Any(x =>
                                x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) &&
                                levelEvaluator(x.Power, new ModeratorLevel[] { ModeratorLevel.Owner, ModeratorLevel.Moderator }));
                            break;
                        case ModeratorAction.DeleteComments:
                        case ModeratorAction.DeletePosts:
                        case ModeratorAction.Banning:
                        case ModeratorAction.DistinguishContent:
                        case ModeratorAction.AssignFlair:
                            result = r.Any(x =>
                                x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) &&
                                levelEvaluator(x.Power, new ModeratorLevel[] { ModeratorLevel.Owner, ModeratorLevel.Moderator, ModeratorLevel.Janitor }));
                            break;
                        case ModeratorAction.ModifyCSS:
                        case ModeratorAction.ModifyFlair:
                            result = r.Any(x =>
                                x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) &&
                                levelEvaluator(x.Power, new ModeratorLevel[] { ModeratorLevel.Owner, ModeratorLevel.Moderator, ModeratorLevel.Designer }));
                            break;
                        default:
                            result = false;
                            break;
                    }
                }
            }
            return result;
        }
    }
}
