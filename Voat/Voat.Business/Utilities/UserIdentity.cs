using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Voat
{
    //This class is a shim for User Principal context to help isolate 
    //direct access to thread context encountered during the Core Port
    public static class UserIdentity
    {
        public static IPrincipal Principal
        {
            get
            {
                var p = Thread.CurrentPrincipal;
                return p;
            }
        }
        public static bool IsAuthenticated
        {
            get
            {
                var i = Identity;
                return i?.IsAuthenticated ?? false;
            }
        }
        public static IIdentity Identity
        {
            get
            {
                IIdentity i = null;

                var p = Principal;
                if (p != null && p.Identity != null)
                {
                    i = p.Identity;
                }

                return i;
            }
        }
        public static string UserName
        {
            get
            {
                string userName = null;

                var i = Identity;
                if (i != null && i.IsAuthenticated)
                {
                    userName = i.Name;
                }

                return userName;
            }
        }

    }
}
