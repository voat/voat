using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using Voat.Caching;
using Voat.Configuration;
using Voat.Data.Models;
using Voat.Rules;
using Voat.Tests.Repository;


[NUnit.Framework.SetUpFixture]
public class UnitTestSetup
{
    [NUnit.Framework.OneTimeSetUp()]
    public void SetUp()
    {

        if (ConfigurationManager.AppSettings["PreventDatabaseDrop"] != "true")
        {
            //Force db to drop & seed
            Database.SetInitializer(new VoatDataInitializer());
            using (var db = new voatEntities())
            {
                var data = db.DefaultSubverses.ToList();
            }
        }

        //load web.config.live monitor
        LiveConfigurationManager.Reload(ConfigurationManager.AppSettings);
        LiveConfigurationManager.Start();

        //This causes the voat rules engine to init using config section for load
        var rulesEngine = VoatRulesEngine.Instance;

        //purge redis for unit tests if enabled
        var defaultHandler = CacheHandlerSection.Instance.Handlers.FirstOrDefault(x => x.Enabled && x.Type.ToLower().Contains("redis"));
        if (defaultHandler != null)
        {
            var instance = defaultHandler.Construct();
            instance.Purge();
        }

    }

    [NUnit.Framework.OneTimeTearDown()]
    public void TearDown()
    {

    }
}
