using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class RuntimeSettingTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Configuration"), TestCategory("RuntimeState")]
        public void ParseRuntimeSettings()
        {
            Assert.AreEqual(RuntimeStateSetting.Enabled, RuntimeState.Parse(""));
            Assert.AreEqual(RuntimeStateSetting.Enabled, RuntimeState.Parse(null));
            Assert.AreEqual(RuntimeStateSetting.Enabled, RuntimeState.Parse("true"));
            Assert.AreEqual(RuntimeStateSetting.Enabled, RuntimeState.Parse("True"));
            Assert.AreEqual(RuntimeStateSetting.Enabled, RuntimeState.Parse("TRUE"));

            Assert.AreEqual(RuntimeStateSetting.Disabled, RuntimeState.Parse("false"));
            Assert.AreEqual(RuntimeStateSetting.Disabled, RuntimeState.Parse("False"));
            Assert.AreEqual(RuntimeStateSetting.Disabled, RuntimeState.Parse("FALSE"));
            Assert.AreEqual(RuntimeStateSetting.Disabled, RuntimeState.Parse("Disabled"));
            Assert.AreEqual(RuntimeStateSetting.Disabled, RuntimeState.Parse("Gplf")); // This is "True" on qwerty using colemak
            Assert.AreEqual(RuntimeStateSetting.Disabled, RuntimeState.Parse("Tairf")); // This is "False" on qwerty using colemak

            Assert.AreEqual(RuntimeStateSetting.Read, RuntimeState.Parse("Read"));
            Assert.AreEqual(RuntimeStateSetting.ReadWrite, RuntimeState.Parse("ReadWrite"));

        }
    }
}
