using System.Security.Principal;

namespace Voat.Tests
{
    public class TestHelper
    {
        public static IPrincipal User
        {
            get
            {
                return System.Threading.Thread.CurrentPrincipal;
            }
        }

        /// <summary>
        /// Sets the current threads User Context for unit tests.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="roles"></param>
        public static void SetPrincipal(string name, string[] roles = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                System.Threading.Thread.CurrentPrincipal = null;
            }
            else
            {
                GenericPrincipal p = new GenericPrincipal(new GenericIdentity(name), roles);
                System.Threading.Thread.CurrentPrincipal = p;
            }
        }
       
    }
}
