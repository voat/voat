using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace Voat.Common
{
    public interface ISecurityContext
    {
        string UserName { get; }
    }
    public abstract class SecurityContext<T> : ISecurityContext where T : class, IPrincipal
    {
        private IActivityContext _context = new ActivityContext();

        public SecurityContext()
        {

        }

        public SecurityContext(IActivityContext activityContext)
        {
            _context = activityContext;
        }
        public SecurityContext(T principal)
        {
            _context = new ActivityContext(principal);
        }
        public IPrincipal User {
            get
            {
                return _context.User;
            }
        }
        public IActivityContext Context 
        {
            get
            {
                return _context;
            }
            set
            {
                _context = value;
            }
        }
        [JsonIgnore()]
        public virtual string UserName
        {
            get
            {
                if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
                {
                    return User.Identity.Name;
                }

                return String.Empty;
            }
        }

        public virtual void DemandAuthentication()
        {
            if (User == null)
            {
                throw new VoatSecurityException("CorePort: User context not available");
            }
            if (!User.Identity.IsAuthenticated || String.IsNullOrEmpty(User.Identity.Name))
            {
                throw new VoatSecurityException("Current process not authenticated");
            }
        }
    }
}
