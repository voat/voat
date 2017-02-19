using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Domain.Models
{
    public class SetPermission
    {
        private SetPermission() { }

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

                    perms.View = isCurrentUserOwner;

                    switch ((SetType)set.Type)
                    {
                        case SetType.Normal:
                            perms.Delete = isCurrentUserOwner;
                            perms.EditList = isCurrentUserOwner;
                            perms.EditProperties = isCurrentUserOwner;
                            break;
                        case SetType.Blocked:
                        case SetType.Front:
                        case SetType.Following:
                            perms.Delete = false;
                            perms.EditList = isCurrentUserOwner;
                            perms.EditProperties = false;
                            break;
                    }
                }
            }
            return perms;
        }
        public bool View { get; set; } = false;
        public bool EditList { get; set; } = false;
        public bool EditProperties { get; set; } = false;
        public bool Delete { get; set; } = false;
    }
}
