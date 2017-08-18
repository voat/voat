using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Voat.Configuration;

namespace Voat.Common
{
    public static class SecurityExtentions
    {

        public static T SetUserContext<T>(this T context, IPrincipal principal) where T : SecurityContext<IPrincipal>
        {
            context.Context = new ActivityContext(principal);
            return context;
        }
        public static T SetActivityContext<T>(this T context, IActivityContext existingContext) where T : SecurityContext<IPrincipal>
        {
            context.Context = existingContext;
            return context;
        }
        public static bool IsInAnyRole<T>(this IPrincipal user, IEnumerable<T> roles)
        {
            return IsInAnyRole(user, roles.Select(x => x.ToString()).ToArray());
        }
        public static bool IsInAnyRole(this IPrincipal user, params string[] roles)
        {
            var result = false;
            if (VoatSettings.Instance.EnableRoles)
            {
                if (roles != null && roles.Length > 0 && user != null && user.Identity.IsAuthenticated)
                {
                    for (int i = 0; i < roles.Length; i++)
                    {
                        result = user.IsInRole(roles[i]);
                        if (result)
                        {
                            break;
                        }
                    }
                }
            }
            return result;
        }
    }
}
