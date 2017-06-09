using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

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
    }
}
