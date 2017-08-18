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
            //no op
            return Task.FromResult(IdentityResult.Success);
        }
    }

    //This class is a CORE PORT Shim 
    public class VoatUserManager : UserManager<VoatIdentityUser>
    {
        private VoatUserManager(
            UserStore<VoatIdentityUser, IdentityRole<string>, IdentityDataContext, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>> store, 
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
                new UserStore<VoatIdentityUser, IdentityRole<string>, IdentityDataContext, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>(new IdentityDataContext(), new IdentityErrorDescriber()),
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
