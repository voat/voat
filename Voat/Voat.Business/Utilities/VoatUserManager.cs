using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data.Models;

namespace Voat
{
    public class VoatPasswordValidator : PasswordValidator<VoatIdentityUser>
    {
        public VoatPasswordValidator()
        {
            
        }
        
        public override Task<IdentityResult> ValidateAsync(UserManager<VoatIdentityUser> manager, VoatIdentityUser user, string password)
        {
            //no op
            return Task.FromResult(IdentityResult.Success);
        }
    }

    //This class is a CORE PORT Shim 
    public class VoatUserManager : UserManager<VoatIdentityUser>
    {
        private VoatUserManager(
            UserStore<VoatIdentityUser> store, 
            IOptions<IdentityOptions> optionsAccessor, 
            IPasswordHasher<VoatIdentityUser> passwordHasher, 
            IEnumerable<IUserValidator<VoatIdentityUser>> userValidators, 
            IEnumerable<IPasswordValidator<VoatIdentityUser>> passwordValidators, 
            ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errorDescribers, 
            IServiceProvider services, 
            ILogger<UserManager<VoatIdentityUser>> logger
            ) 
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errorDescribers, services, logger)
        {

        }

        public static VoatUserManager Create()
        {

            var options = new IdentityOptions();
            var ioptions = Microsoft.Extensions.Options.Options.Create(options);

            var mgr = new VoatUserManager(
                new UserStore<VoatIdentityUser>(new IdentityDataContext()),
                ioptions,
                new PasswordHasher<VoatIdentityUser>(),
                new[] { new UserValidator<VoatIdentityUser>() },
                new[] { new VoatPasswordValidator() },
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null,
                new Logger<UserManager<VoatIdentityUser>>(new LoggerFactory()));

            return mgr;

        }

        public IdentityResult Create(VoatIdentityUser user, string password)
        {
            var task = Task.Run(() => {
                var x = CreateAsync(user, password);
                return x;
            });
            task.Wait();
            return task.Result;
        }

        public VoatIdentityUser FindByName(string userName)
        {
            var task = Task.Run(() => FindByNameAsync(userName));
            task.Wait();
            return task.Result;
        }

        public VoatIdentityUser Find(string userName, string password)
        {
            var t = Task.Run(() => FindAsync(userName, password));
            t.Wait();
            return t.Result;
        }
        public async Task<VoatIdentityUser> FindAsync(string userName, string password)
        {
            var user = await FindByNameAsync(userName);
            if (user != null)
            {
                var result = await CheckPasswordAsync(user, password);
                if (result)
                {
                    return user;
                }
            }
            return null;
        }
    }
}
