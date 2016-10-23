using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Voat.Data.Models
{
    public class VoatUser : IdentityUser
    {
        public DateTime RegistrationDateTime { get; set; }

        public DateTime LastLoginDateTime { get; set; }

        [StringLength(50)]
        public string LastLoginFromIp { get; set; }

        // user registered as partner: original content creator - in form of submissions or comments
        public bool Partner { get; set; }

        //For WebApi OAuth2
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<VoatUser> manager, string authenticationType)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<VoatUser>
    {
        public ApplicationDbContext() : base("voatUsers")
        {
        }
    }
}
