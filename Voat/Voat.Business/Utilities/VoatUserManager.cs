using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Data.Models;

namespace Voat
{
    public class VoatPasswordValidator : IPasswordValidator<VoatUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<VoatUser> manager, VoatUser user, string password)
        {
            //no op
            return Task.FromResult(IdentityResult.Success);
        }
    }

    //This class is a CORE PORT Shim 
    public class VoatUserManager : UserManager<VoatUser>
    {
        private VoatUserManager(
            UserStore<VoatUser> store, 
            IOptions<IdentityOptions> optionsAccessor, 
            IPasswordHasher<VoatUser> passwordHasher, 
            IEnumerable<IUserValidator<VoatUser>> userValidators, 
            IEnumerable<IPasswordValidator<VoatUser>> passwordValidators, 
            ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errorDescribers, 
            IServiceProvider services, 
            ILogger<UserManager<VoatUser>> logger
            ) 
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errorDescribers, services, logger)
        {

        }

        public static VoatUserManager Create()
        {

            var options = new IdentityOptions();
            var ioptions = Microsoft.Extensions.Options.Options.Create(options);

            var mgr = new VoatUserManager(
                new UserStore<VoatUser>(new ApplicationDbContext()),
                ioptions,
                new PasswordHasher<VoatUser>(),
                null,
                new[] { new VoatPasswordValidator() },
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null,
                new Logger<UserManager<VoatUser>>(new LoggerFactory()));

            return mgr;

        }

        public IdentityResult Create(VoatUser user, string password)
        {
            var task = Task.Run(() => {
                var x = CreateAsync(user, password);
                return x;
            });
            task.Wait();
            return task.Result;
        }

        public VoatUser FindByName(string name)
        {
            var task = Task.Run(() => FindByNameAsync(name));
            task.Wait();
            return task.Result;
        }
        public VoatUser Find(string userID, string password)
        {
            var task = Task.Run(() => FindByLoginAsync(userID, password));
            task.Wait();
            return task.Result;
        }
    }
}
