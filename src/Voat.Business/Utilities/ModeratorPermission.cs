#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Voat.Common;
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
                return modList.Where(x => x.Subverse.ToLower() == subverse.ToLower());
            }
            else
            {
                var q = new QuerySubverseModerators(subverse);
                return q.Execute();
            }
        }

        public static bool IsModerator(IPrincipal user, string subverse, ModeratorLevel[] levels = null, IEnumerable<Data.Models.SubverseModerator> modList = null)
        {
            if (user.IsInAnyRole(new[] { UserRole.GlobalAdmin, UserRole.Admin, UserRole.DelegateAdmin, UserRole.GlobalJanitor }))
            {
                return true;
            }
            return IsModerator(user.Identity.Name, subverse, levels, modList);
        }
        public static bool IsModerator(string userName, string subverse, ModeratorLevel[] levels = null, IEnumerable<Data.Models.SubverseModerator> modList = null)
        {
            var mods = GetModerators(subverse, modList);
            return mods.Any(x =>
                x.UserName.ToLower() == userName.ToLower()
                && (levels == null || (levels != null && levels.Any(l => x.Power == (int)l)))
            );
        }

        public static bool IsLevel(IPrincipal user, string subverse, ModeratorLevel level, IEnumerable<Data.Models.SubverseModerator> modList = null)
        {
            return IsModerator(user, subverse, new ModeratorLevel[] { level }, modList);
        }

        public static ModeratorLevel? Level(IPrincipal user, string subverse, IEnumerable<Data.Models.SubverseModerator> modList = null)
        {
            if (user.IsInAnyRole(new[] { UserRole.GlobalAdmin, UserRole.Admin, UserRole.DelegateAdmin }))
            {
                return ModeratorLevel.Owner;
            }

            var userName = user.Identity.Name;
            var mods = GetModerators(subverse, modList);
            var o = mods.FirstOrDefault(x => x.UserName.ToLower() == userName.ToLower());
            if (o != null)
            {
                return (ModeratorLevel)Enum.Parse(typeof(ModeratorLevel), o.Power.ToString());
            }
            else if (user.IsInAnyRole(new[] { UserRole.GlobalJanitor }))
            {
                return ModeratorLevel.Janitor;
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
                case ModeratorAction.AccessReports:
                case ModeratorAction.MarkReports:
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
        public static bool HasPermission(IPrincipal user, string subverse, ModeratorAction action, IEnumerable<Data.Models.SubverseModerator> modList = null)
        {

            if (user.IsInAnyRole(new[] { UserRole.GlobalAdmin, UserRole.Admin, UserRole.DelegateAdmin }))
            {
                return true;
            }
            var userName = user.Identity.Name;
            bool result = false;
            if (!String.IsNullOrEmpty(userName) && !String.IsNullOrEmpty(subverse))
            {
                var r = GetModerators(subverse, modList);

                if (r != null && r.Any())
                {
                    result = r.Any(x =>
                                x.UserName.ToLower() == userName.ToLower() &&
                                HasPermission((ModeratorLevel)x.Power, action));
                }
                //if they don't have permissions check if global janitor and request is for janitor role 
                if (!result)
                {
                    if (user.IsInAnyRole(new[] { UserRole.GlobalJanitor }))
                    {
                        result = HasPermission(ModeratorLevel.Janitor, action);
                    }
                }
            }
            return result;
        }
    }
}
