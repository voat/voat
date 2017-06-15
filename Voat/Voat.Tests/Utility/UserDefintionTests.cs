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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Models;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class UserDefintionTests : BaseUnitTest
    {

        [TestMethod]
        [TestCategory("Utility"), TestCategory("UserDefinition")]
        public void TestSubverseDefintionParse0()
        {
            var name = "Test";
            var input = $"V/{name}";
            var d = UserDefinition.Parse(input);
            Assert.AreEqual(d.Name, name);
            Assert.AreEqual(d.Type, IdentityType.Subverse);
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("UserDefinition")]
        public void TestSubverseDefintionParse1()
        {
            var name = "Test";
            var input = $"v/{name}";
            var d = UserDefinition.Parse(input);
            Assert.AreEqual(d.Name, name);
            Assert.AreEqual(d.Type, IdentityType.Subverse);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("UserDefinition")]
        public void TestSubverseDefintionParse2()
        {
            var name = "Test";
            var input = $"/v/{name}";
            var d = UserDefinition.Parse(input);
            Assert.AreEqual(d.Name, name);
            Assert.AreEqual(d.Type, IdentityType.Subverse);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("UserDefinition")]
        public void TestUserDefintionParse1()
        {
            var name = "Test";
            var input = $"{name}";
            var d = UserDefinition.Parse(input);
            Assert.AreEqual(d.Name, name);
            Assert.AreEqual(d.Type, IdentityType.User);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("UserDefinition")]
        public void TestUserDefintionParse2()
        {
            var name = "Test";
            var input = $"u/{name}";
            var d = UserDefinition.Parse(input);
            Assert.AreEqual(d.Name, name);
            Assert.AreEqual(d.Type, IdentityType.User);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("UserDefinition")]
        public void TestUserDefintionParse3()
        {
            var name = "Test";
            var input = $"@{name}";
            var d = UserDefinition.Parse(input);
            Assert.AreEqual(d.Name, name);
            Assert.AreEqual(d.Type, IdentityType.User);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("UserDefinition")]
        public void TestUserDefintionParse4()
        {
            var name = "Test";
            var input = $"b/{name}";
            var d = UserDefinition.Parse(input);
            Assert.AreEqual(null, d);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("UserDefinition")]
        public void TestUserDefintionParse5()
        {
            var name = "Test";
            var input = $"U/{name}";
            var d = UserDefinition.Parse(input);
            Assert.AreEqual(d.Name, name);
            Assert.AreEqual(d.Type, IdentityType.User);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("UserDefinition")]
        public void TestUserDefintionFormat1()
        {
            var name = "Test";
            var d = UserDefinition.Format(name, IdentityType.User);
            Assert.AreEqual(d, "Test");
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("UserDefinition")]
        public void TestSubverseDefintionFormat1()
        {
            var name = "Test";
            var d = UserDefinition.Format(name, IdentityType.Subverse);
            Assert.AreEqual(d, "v/Test");
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("UserDefinition")]
        public void TestSubverseDefintionFormat2()
        {
            var name = "Test";
            var d = UserDefinition.Format(name, IdentityType.Subverse);
            Assert.AreEqual(d, "v/Test");
        }


        [TestMethod]
        [TestCategory("Utility"), TestCategory("UserDefinition")]
        public void TestParseMany1()
        {
            var d = UserDefinition.ParseMany("  Atko PuttItOut, FuzzyWords, v/test, @Amalek,   u/Mick; /u/moe");
            Assert.AreEqual(7, d.Count());
            Assert.AreEqual(1, d.Count(x => x.Type == IdentityType.Subverse));
            Assert.AreEqual(6, d.Count(x => x.Type == IdentityType.User));

            foreach (var def in d)
            {
                Assert.AreEqual(def.Name, def.Name.Trim());
            }

        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("UserDefinition")]
        public void TestParseMany2()
        {
            var d = UserDefinition.ParseMany("  Atko aTko , ATko; atko AtkO; atko, atko, atko    atko");
            Assert.AreEqual(1, d.Count());
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("UserDefinition")]
        public void TestParseMany3()
        {
            var d = UserDefinition.ParseMany("  Atko aTko , ATko; atko AtkO", false);
            Assert.AreEqual(5, d.Count());
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("UserDefinition")]
        public void TestParseMany4()
        {
            var d = UserDefinition.ParseMany(" /u /Atko PuttItOut, Fuzzy.Words, @v/test, @Amalek,   u/Mick; /u/moe", false);
            Assert.AreEqual(6, d.Count());
        }



        [TestMethod]
        [TestCategory("Utility"), TestCategory("UserDefinition")]
        public void TestUserDefintionParse_BugFix1()
        {
            var name = "API-Words";
            var input = $"{name}";
            var d = UserDefinition.Parse(input);
            Assert.AreEqual(d.Name, name);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("UserDefinition")]
        public void TestUserDefintionParse_BugFix2()
        {
            var name = "i-am-me";
            var input = $"@{name}";
            var d = UserDefinition.Parse(input);
            Assert.AreEqual(d.Name, name);
        }
    }
}
