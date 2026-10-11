using Dapper;
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
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.Utilities;

namespace Voat
{
    public class VoatPasswordValidator : PasswordValidator<VoatIdentityUser>
    {
        public VoatPasswordValidator()
        {
            
        }
        
        public override Task<IdentityResult> ValidateAsync(UserManager<VoatIdentityUser> manager, VoatIdentityUser user, string password)
        {
            //Enforce the configured password policy (VoatSettings.PasswordOptions) instead of accepting anything.
            return base.ValidateAsync(manager, user, password);
        }
    }

    //This class is a CORE PORT Shim 
    public class VoatUserManager : UserManager<VoatIdentityUser>
    {
        public VoatUserManager(
            UserStore<VoatIdentityUser, IdentityRole, IdentityDataContext, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>> store, 
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
        public VoatUserManager() : this(new UserStore<VoatIdentityUser, IdentityRole, IdentityDataContext, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>(new IdentityDataContext(), new IdentityErrorDescriber()),
                //Use the same password policy this shim's validator enforces as the web (DI) pipeline does.
                Microsoft.Extensions.Options.Options.Create(new IdentityOptions() { Password = VoatSettings.Instance.PasswordOptions }),
                //IdentityV3 = PBKDF2-HMAC-SHA256 with a pinned iteration count (hashes carry their own
                //count and format byte, so existing IdentityV2 hashes keep verifying and are upgraded on
                //next successful login).
                new PasswordHasher<VoatIdentityUser>(Microsoft.Extensions.Options.Options.Create(new PasswordHasherOptions() { CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3, IterationCount = 100000 })),
                new[] { new UserValidator<VoatIdentityUser>() },
                new[] { new VoatPasswordValidator() },
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null,
                new Logger<UserManager<VoatIdentityUser>>(new LoggerFactory()))
        { 

        }
        public static VoatUserManager Create()
        {
            return new VoatUserManager();
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
        //TODO: This needs to be moved to the repo
        public static async Task<bool> UserNameExistsAsync(IEnumerable<string> userNames)
        {
            //Use Dapper
            using (var context = new VoatDataContext(CONSTANTS.CONNECTION_USERS))
            {
                var q = new DapperQuery();
                q.Select = $"\"UserName\" FROM {SqlFormatter.Table("AspNetUsers")}";
                q.Where = $"lower(\"UserName\") {SqlFormatter.In("@UserNameList")}";
                q.Parameters.Add("UserNameList", userNames.Select(x => x.ToLower()).ToList());

                var matches = await context.Connection.QueryAsync<string>(q.ToString(), q.Parameters);
                var result = matches.Any();

                return result;
            }
        }
    }
}
