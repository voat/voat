using System;
using System.Collections.Generic;
using System.Linq;
using Voat.Domain.Models;
using Voat.Domain.Query;

namespace Voat.Utilities
{
    public static class ModeratorPermission
    {
        private static IEnumerable<Data.Models.SubverseModerator> GetModerators(string subverse, IEnumerable<Data.Models.SubverseModerator> modList = null)
        {
            if (modList != null)
            {
                //just ensure we filter - probably don't have to but this protects a bit
                return modList.Where(x => x.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                var q = new QuerySubverseModerators(subverse);
                return q.Execute();
            }
        }

        public static bool IsModerator(string userName, string subverse, ModeratorLevel[] levels = null, IEnumerable<Data.Models.SubverseModerator> modList = null)
        {
            var mods = GetModerators(subverse, modList);
            return mods.Any(x =>
                x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                && (levels == null || (levels != null && levels.Any(l => x.Power == (int)l)))
            );
        }

        public static bool IsLevel(string userName, string subverse, ModeratorLevel level, IEnumerable<Data.Models.SubverseModerator> modList = null)
        {
            return IsModerator(userName, subverse, new ModeratorLevel[] { level }, modList);
        }

        public static ModeratorLevel? Level(string userName, string subverse, IEnumerable<Data.Models.SubverseModerator> modList = null)
        {
            var mods = GetModerators(subverse, modList);
            var o = mods.FirstOrDefault(x => x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
            if (o != null)
            {
                return (ModeratorLevel)Enum.Parse(typeof(ModeratorLevel), o.Power.ToString());
            }
            return null;
        }
        public static bool HasPermission(ModeratorLevel level, ModeratorAction action)
        {
            bool result = false;

            var levelEvaluator = new Func<ModeratorLevel, IEnumerable<ModeratorLevel>, bool>((currentLevel, allowedLevels) =>
            {
                bool allowed = false;
                if (allowedLevels != null && allowedLevels.Any())
                {
                    allowed = allowedLevels.Any(x => currentLevel == x);
                }
                return allowed;
            });

            switch (action)
            {
                case ModeratorAction.InviteMods:
                case ModeratorAction.RemoveMods:
                case ModeratorAction.ModifySettings:
                case ModeratorAction.AssignStickies:
                    result = levelEvaluator(level, new ModeratorLevel[] { ModeratorLevel.Owner, ModeratorLevel.Moderator });
                    break;

                case ModeratorAction.DeleteComments:
                case ModeratorAction.DeletePosts:
                case ModeratorAction.Banning:
                case ModeratorAction.DistinguishContent:
                case ModeratorAction.AssignFlair:
                case ModeratorAction.ReadMail:
                case ModeratorAction.SendMail:
                    result = levelEvaluator(level, new ModeratorLevel[] { ModeratorLevel.Owner, ModeratorLevel.Moderator, ModeratorLevel.Janitor });
                    break;

                case ModeratorAction.ModifyCSS:
                case ModeratorAction.ModifyFlair:
                    result = levelEvaluator(level, new ModeratorLevel[] { ModeratorLevel.Owner, ModeratorLevel.Moderator, ModeratorLevel.Designer });
                    break;

                case ModeratorAction.DeleteMail: //mod mail should not be deleted
                default:
                    result = false;
                    break;
            }

            return result;
        }
        public static bool HasPermission(string userName, string subverse, ModeratorAction action, IEnumerable<Data.Models.SubverseModerator> modList = null)
        {
            bool result = false;
            if (!String.IsNullOrEmpty(userName) && !String.IsNullOrEmpty(subverse))
            {
                var r = GetModerators(subverse, modList);

                if (r != null && r.Any())
                {
                    result = r.Any(x =>
                                x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) &&
                                HasPermission((ModeratorLevel)x.Power, action));
                }
            }
            return result;
        }
    }
}
