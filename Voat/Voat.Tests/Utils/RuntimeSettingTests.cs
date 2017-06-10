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

namespace Voat.Tests.Utils
{
    [TestClass]
    public class RuntimeSettingTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Configuration"), TestCategory("RuntimeState")]
        public void ParseRuntimeSettings()
        {
            Assert.Fail("Fix this");

            //Assert.AreEqual(RuntimeStateSetting.Enabled, RuntimeState.Parse(""));
            //Assert.AreEqual(RuntimeStateSetting.Enabled, RuntimeState.Parse(null));
            //Assert.AreEqual(RuntimeStateSetting.Enabled, RuntimeState.Parse("true"));
            //Assert.AreEqual(RuntimeStateSetting.Enabled, RuntimeState.Parse("True"));
            //Assert.AreEqual(RuntimeStateSetting.Enabled, RuntimeState.Parse("TRUE"));

            //Assert.AreEqual(RuntimeStateSetting.Disabled, RuntimeState.Parse("false"));
            //Assert.AreEqual(RuntimeStateSetting.Disabled, RuntimeState.Parse("False"));
            //Assert.AreEqual(RuntimeStateSetting.Disabled, RuntimeState.Parse("FALSE"));
            //Assert.AreEqual(RuntimeStateSetting.Disabled, RuntimeState.Parse("Disabled"));
            //Assert.AreEqual(RuntimeStateSetting.Disabled, RuntimeState.Parse("Gplf")); // This is "True" on qwerty using colemak
            //Assert.AreEqual(RuntimeStateSetting.Disabled, RuntimeState.Parse("Tairf")); // This is "False" on qwerty using colemak

            //Assert.AreEqual(RuntimeStateSetting.Read, RuntimeState.Parse("Read"));
            //Assert.AreEqual(RuntimeStateSetting.ReadWrite, RuntimeState.Parse("ReadWrite"));

        }
    }
}
