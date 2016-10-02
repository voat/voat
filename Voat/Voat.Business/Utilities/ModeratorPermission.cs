using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Models;
using Voat.Domain.Query;

namespace Voat.Utilities
{
    public static class ModeratorPermission
    {

        public static bool HasPermission(string userName, string subverse, ModeratorAction action)
        {
            bool result = false;
            if (!String.IsNullOrEmpty(userName) && !String.IsNullOrEmpty(subverse))
            {
                var q = new QuerySubverseModerators(subverse);
                var r = q.Execute();

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
                        case ModeratorAction.Banning:
                            result = r.Any(x =>
                                x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) &&
                                levelEvaluator(x.Power, new ModeratorLevel[] { ModeratorLevel.Primary, ModeratorLevel.Moderator }));
                            break;
                        case ModeratorAction.ChangeCSS:
                            result = r.Any(x =>
                                x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) &&
                                levelEvaluator(x.Power, new ModeratorLevel[] { ModeratorLevel.Primary, ModeratorLevel.Moderator, ModeratorLevel.Designer }));
                            break;
                        case ModeratorAction.ChangeSettings:
                            result = r.Any(x =>
                                x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) &&
                                levelEvaluator(x.Power, new ModeratorLevel[] { ModeratorLevel.Primary, ModeratorLevel.Moderator }));
                            break;
                        case ModeratorAction.CreateFlair:
                            result = r.Any(x =>
                                x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) &&
                                levelEvaluator(x.Power, new ModeratorLevel[] { ModeratorLevel.Primary, ModeratorLevel.Moderator, ModeratorLevel.Janitor, ModeratorLevel.Designer }));
                            break;
                        case ModeratorAction.DeleteComments:
                        case ModeratorAction.DeletePosts:
                            result = r.Any(x =>
                                x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) &&
                                levelEvaluator(x.Power, new ModeratorLevel[] { ModeratorLevel.Primary, ModeratorLevel.Moderator, ModeratorLevel.Janitor }));
                            break;
                        case ModeratorAction.Distinguish:
                            result = r.Any(x =>
                                x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) &&
                                levelEvaluator(x.Power, new ModeratorLevel[] { ModeratorLevel.Primary, ModeratorLevel.Moderator, ModeratorLevel.Janitor }));
                            break;
                        case ModeratorAction.InviteMods:
                        case ModeratorAction.RemoveMods:
                            result = r.Any(x =>
                                x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) &&
                                levelEvaluator(x.Power, new ModeratorLevel[] { ModeratorLevel.Primary, ModeratorLevel.Moderator }));
                            break;
                        case ModeratorAction.Stickies:
                            result = r.Any(x =>
                                x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) &&
                                levelEvaluator(x.Power, new ModeratorLevel[] { ModeratorLevel.Primary, ModeratorLevel.Moderator, ModeratorLevel.Janitor }));
                            break;
                        case ModeratorAction.UseFlair:
                            result = r.Any(x =>
                                x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) &&
                                levelEvaluator(x.Power, new ModeratorLevel[] { ModeratorLevel.Primary, ModeratorLevel.Moderator, ModeratorLevel.Janitor }));
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
