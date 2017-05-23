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
            //Database.SetInitializer();
            using (var db = new voatEntities())
            {
                var init = new VoatDataInitializer();
                init.InitializeDatabase(db); //This attempts to create and seed unit test db
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
