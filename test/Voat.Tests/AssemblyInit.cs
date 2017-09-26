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

namespace Voat.Tests
{

    [NUnit.Framework.SetUpFixture]
    [TestClass]
    public class UnitTestSetup
    {
        [NUnit.Framework.OneTimeSetUp()]
        [AssemblyInitialize()]
        public static void SetUp(TestContext context)
        {
            SetUp(context, new TestDataInitializer(), true);
        }

        public static void SetUp(TestContext context, TestDataInitializer intializer, bool configure = true)
        {
            //bool preventDrop = false;

            if (configure)
            {
                //Configure App
                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
                config.ConfigureVoat();
                //config["voat:test:preventDatabaseDrop"] != "True"

            }

            FilePather.Instance = new FilePather(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));

            //Drop and reseed database
            using (var db = new VoatDataContext())
            {
                intializer.InitializeDatabase(db); //This attempts to create and seed unit test db
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
}