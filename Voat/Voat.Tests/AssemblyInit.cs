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

using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.IO;
using System.Linq;
using Voat.Caching;
using Voat.Common.Components;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.Logging;
using Voat.Rules;
using Voat.RulesEngine;
using Voat.Tests;
using Voat.Tests.Infrastructure;
using Voat.Tests.Repository;


[NUnit.Framework.SetUpFixture]
[TestClass]
public class UnitTestSetup
{



    [NUnit.Framework.OneTimeSetUp()]
    [AssemblyInitialize()]
    public static void SetUp(TestContext context)
    {
        //Configure App
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();
        config.ConfigureVoat();

        FilePather.Instance = new FilePather(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));


        if (config["voat:test:preventDatabaseDrop"] != "True")
        {
            //Force db to drop & seed
            //Database.SetInitializer();
            using (var db = new VoatDataContext())
            {
                try
                {
                    //Force configuration
                    var x = db.Ad.FirstOrDefault();
                }
                catch {
                    if (db.Connection.State == System.Data.ConnectionState.Open)
                    {
                        db.Connection.Close();

                    }
                }

                var init = new TestDataInitializer();
                init.InitializeDatabase(db); //This attempts to create and seed unit test db
            }
        }

        //This causes the voat rules engine to init using config section for load
        var rulesEngine = VoatRulesEngine.Instance;

        //purge redis for unit tests if enabled
        var defaultHandler = CacheConfigurationSettings.Instance.Handlers.FirstOrDefault(x => x.Enabled && x.Type.ToLower().Contains("redis"));
        if (defaultHandler != null)
        {
            var instance = defaultHandler.Construct<ICacheHandler>();
            instance.Purge();
        }

    }

    [NUnit.Framework.OneTimeTearDown()]
    public void TearDown()
    {

    }
}
