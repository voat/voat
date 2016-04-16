#region LICENSE

/*

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All portions of the code written by Voat, Inc. are Copyright(c) Voat, Inc.

    All Rights Reserved.

*/

#endregion LICENSE

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Data.Entity;
using Voat.Data.Models;

namespace Voat.Tests.Repository
{
    public abstract class DatabaseRequiredUnitTest
    {
        protected voatEntities _entities;
        protected Voat.Data.Repository db;

        static DatabaseRequiredUnitTest()
        {
            Database.SetInitializer(new VoatDataInitializer());

            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>());
            //var appDbContext = new ApplicationDbContext();
            //appDbContext.Database.Initialize(true);

            //Database.SetInitializer(new VoatUsersInitializer());
        }

        [TestCleanup]
        public void CleanUp()
        {
            db.Dispose();
        }

        [TestInitialize]
        public void Init()
        {
            //CacheHandler.Instance = new RedisCacheHandler();

            _entities = new voatEntities();
            db = new Voat.Data.Repository(_entities);
        }
    }
}
