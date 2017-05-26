using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Voat.Data.Models;

namespace Voat
{
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
            throw new NotImplementedException("Core Port: UserManager not implemented");
        }
        public VoatUser FindByName(string name)
        {
            throw new NotImplementedException("Core Port: UserManager not implemented");
        }
        
    }
}
