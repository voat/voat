using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using Voat.Caching;
using Voat.Configuration;
using Voat.Data.Models;
using Voat.Rules;
using Voat.Tests.Repository;

namespace Voat.Tests
{
    [TestClass]
    public class AssemblyInit
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            //Force db to drop & seed
            Database.SetInitializer(new VoatDataInitializer());
            using (var db = new voatEntities())
            {
                var data = db.DefaultSubverses.ToList();
            }


            //This causes the voat rules engine to init usin config section for load
            var x = VoatRulesEngine.Instance;

            LiveConfigurationManager.Reload(ConfigurationManager.AppSettings);
            LiveConfigurationManager.Start();
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            var defaultHandler = CacheHandlerSection.Instance.Handlers.FirstOrDefault(x => x.Type.ToLower().Contains("redis")).Construct();
            defaultHandler.Purge();
        }

    }
}
