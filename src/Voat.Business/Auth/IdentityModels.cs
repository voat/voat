#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Voat.Utilities;

namespace Voat.Data.Models
{

    public class VoatIdentityUser : IdentityUser
    {
        public DateTime RegistrationDateTime { get; set; }

        public DateTime LastLoginDateTime { get; set; }

        [StringLength(50)]
        public string LastLoginFromIp { get; set; }

        ////For WebApi OAuth2
        //public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<VoatIdentityUser> manager, string authenticationType)
        //{
        //    //CORE_PORT: No extension
        //    throw new NotImplementedException("Core Port Build");
        //    //var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
        //    //return userIdentity;
        //}
    }

    public class IdentityDataContext : IdentityDbContext<VoatIdentityUser>
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema(SqlFormatter.DefaultSchema);
            base.OnModelCreating(builder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            this.Configure(optionsBuilder, CONSTANTS.CONNECTION_USERS);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
