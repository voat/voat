using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Models;

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
