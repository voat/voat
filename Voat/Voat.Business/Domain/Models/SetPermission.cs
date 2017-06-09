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
using System.Text;
using System.Threading.Tasks;
using Voat.Common;

namespace Voat.Domain.Models
{
    public class SetPermission
    {
        public SetPermission() { }

        public static SetPermission GetPermissions(Domain.Models.Set set, System.Security.Principal.IIdentity user)
        {
            var perms = new SetPermission();
            perms.View = set.IsPublic;

            //only allow user owned sets to be changed in any capacity
            if (!String.IsNullOrEmpty(set.UserName))
            {
                //Authenticated users
                if (user != null && user.IsAuthenticated && !String.IsNullOrEmpty(user.Name))
                {
                    var isCurrentUserOwner = set.UserName.IsEqual(user.Name);

                    switch ((SetType)set.Type)
                    {
                        case SetType.Normal:
                            perms.View = set.IsPublic || isCurrentUserOwner;
                            perms.Delete = isCurrentUserOwner;
                            perms.EditList = isCurrentUserOwner;
                            perms.EditProperties = isCurrentUserOwner;
                            break;
                        case SetType.Blocked:
                        case SetType.Front:
                        case SetType.Following:
                            perms.View = isCurrentUserOwner;
                            perms.Delete = false;
                            perms.EditList = isCurrentUserOwner;
                            perms.EditProperties = false;
                            break;
                    }
                }
            }
            return perms;
        }
        public bool View { get; private set; } = false;
        public bool EditList { get; private set; } = false;
        public bool EditProperties { get; private set; } = false;
        public bool Delete { get; private set; } = false;
    }
}
