using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Linq;
using Voat.Caching;
using Voat.Configuration;
using Voat.Rules;

namespace Voat.Tests
{
    [TestClass]
    public class AssemblyInit
    {
        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            var defaultHandler = CacheHandlerSection.Instance.Handlers.FirstOrDefault(x => x.Type.ToLower().Contains("redis")).Construct();
            defaultHandler.Purge();
        }

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            //This causes the voat rules engine to init usin config section for load
            var x = VoatRulesEngine.Instance;

            LiveConfigurationManager.Reload(ConfigurationManager.AppSettings);
            LiveConfigurationManager.Start();
        }
    }
}
