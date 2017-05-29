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
            var mgr = new VoatUserManager(
                new UserStore<VoatUser>(new ApplicationDbContext()),
                null,
                new PasswordHasher<VoatUser>(),
                null,
                new[] { new VoatPasswordValidator() },
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null,
                new Logger<UserManager<VoatUser>>(new LoggerFactory()));

            return mgr;



            //throw new NotImplementedException("Core Port: UserManager not implemented");
        }

        public IdentityResult Create(VoatUser user, string password)
        {
            var task = Task.Run(() => CreateAsync(user, password));
            task.Wait();
            return task.Result;
            //throw new NotImplementedException("Core Port: Create not implemented");
        }

        public VoatUser FindByName(string name)
        {
            throw new NotImplementedException("Core Port: FindByName not implemented");
        }
        public VoatUser Find(string userID, string password)
        {
            throw new NotImplementedException("Core Port: Find not implemented");
        }
    }
}
