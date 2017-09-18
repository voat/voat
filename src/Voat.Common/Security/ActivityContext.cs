using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace Voat.Common
{
    public interface IActivityContext
    {
        string ActivityID { get; }
        IPrincipal User { get; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.Fields)]
    public class ActivityIdentity : IIdentity
    {
        private string _name = null;
        //If we deserialize we do not want to retain auth settings and will
        //need to reauth. Since serialization is for distribution in the future
        //we are not going to implement the code for this right now. 
        [JsonIgnore()]
        private bool _isAuthenticated = false;
        [JsonIgnore()]
        private string _authenticationType = "";

        public ActivityIdentity()
        {
        }
        public ActivityIdentity(string name) : this(name, false, "")
        {
        }
        internal ActivityIdentity(string name, bool isAuthenticated, string authenticationType)
        {
            this._name = name;
            this._isAuthenticated = isAuthenticated;
            this._authenticationType = authenticationType;
        }

        public string Name { get => _name; }
        public bool IsAuthenticated { get => _isAuthenticated; }
        public string AuthenticationType { get => _authenticationType; }

        public static ActivityIdentity Unathenticated => new ActivityIdentity();

    }
    [JsonObject(MemberSerialization = MemberSerialization.Fields)]
    public class ActivityPrincipal : IPrincipal
    {
        private ActivityIdentity _activityIdentity = null;
        private string[] _roles = null;

        public ActivityPrincipal() : this(ActivityIdentity.Unathenticated, null)
        {

        }

        public ActivityPrincipal(ActivityIdentity activityIdentity, params string[] roles)
        {
            _activityIdentity = activityIdentity;
            _roles = roles;
        } 

        public ActivityIdentity Identity => _activityIdentity;

        IIdentity IPrincipal.Identity => _activityIdentity;

        public bool IsInRole(string role)
        {
            if (_roles != null && _roles.Length > 0)
            {
                return _roles.Contains(role);
            }
            return false;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.Fields)]
    public class ActivityContext : IDisposable, IActivityContext
    {
        private string _activityID = Guid.NewGuid().ToString();
        private DateTime _startDate = DateTime.UtcNow;
        private DateTime? _endDate = null;
        private ActivityPrincipal _activityPrincipal = null;

        public ActivityContext() : this(null)
        {
        }
        public ActivityContext(IPrincipal principal)
        {
            _activityPrincipal = Convert(principal);
        }

        private ActivityPrincipal Convert(IPrincipal principal)
        {
            if (principal == null)
            {
                return new ActivityPrincipal();
            }

            var activityPrincipal = principal as ActivityPrincipal;
            if (activityPrincipal != null)
            {
                return activityPrincipal;
            }
            var claimsPrincipal = principal as ClaimsPrincipal;
            if (claimsPrincipal != null)
            {
                //This is wrong but we are not using roles yet
                var roles = claimsPrincipal.Claims.Select(x => x.Value);
                var p = new ActivityPrincipal(
                    new ActivityIdentity(claimsPrincipal.Identity.Name, claimsPrincipal.Identity.IsAuthenticated, claimsPrincipal.Identity.AuthenticationType) 
                , roles.ToArray()
                );

                return p;
            }

            throw new NotSupportedException($"Converting {principal.GetType()} not supported");
        }

        public void Dispose()
        {
            if (!_endDate.HasValue)
            {
                _endDate = DateTime.UtcNow;
            }
        }

        public string ActivityID { get => _activityID; }
        public DateTime StartDate { get => _startDate; }
        public DateTime? EndDate { get => _endDate; }

        public IPrincipal User
        {
            get
            {
                return _activityPrincipal;
            }
        }

    }
}
