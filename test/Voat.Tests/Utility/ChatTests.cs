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
using Voat.Tests.Infrastructure;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class ChatTests : BaseUnitTest
    {
        [TestMethod]
        public void TestPermissions()
        {
            ChatRoom room = null;
            room = new ChatRoom() { ID = "Public", IsPrivate = false };

            Assert.IsTrue(room.IsAccessAllowed(null, String.Empty));
            Assert.IsTrue(room.IsAccessAllowed(null, null));
            Assert.IsTrue(room.IsAccessAllowed(String.Empty, null));
            Assert.IsTrue(room.IsAccessAllowed("TOM", "SOMETHING"));


            room = new ChatRoom() { ID = "Private", IsPrivate = true };

            Assert.IsFalse(room.IsAccessAllowed(null, String.Empty));
            Assert.IsFalse(room.IsAccessAllowed(null, null));
            Assert.IsFalse(room.IsAccessAllowed(null, null));

            var userName = "TestUser01";
            var user = TestHelper.SetPrincipal(userName);
            Assert.IsTrue(room.IsAccessAllowed(userName, "SOMETHING"));
            Assert.IsTrue(room.IsAccessAllowed(userName, ""));


            room = new ChatRoom() { ID = "Passphrase", IsPrivate = false, Passphrase = "hello" };

            Assert.IsFalse(room.IsAccessAllowed(null, String.Empty));
            Assert.IsFalse(room.IsAccessAllowed(null, null));

            userName = "TestUser01";
            user = TestHelper.SetPrincipal(userName);
            var hash = ChatRoom.GetAccessHash(userName, "hello");
            Assert.IsTrue(room.IsAccessAllowed(userName, hash));

            Assert.IsFalse(room.IsAccessAllowed(userName, ""));
            Assert.IsFalse(room.IsAccessAllowed(userName, null));
            Assert.IsFalse(room.IsAccessAllowed(userName + "2", hash));


        }
        [TestMethod]
        public void TestSanitization()
        {
            Assert.AreEqual("This is my message", ChatMessage.SanitizeInput(@"This 
                        is 
                        my   
                          message 

                        "));

            Assert.AreEqual("This is my message", ChatMessage.SanitizeInput(@"This 
                        is 
                        - - - 
                        my  
                        * * *
                          message 

                        "));

            Assert.AreEqual("This is my message", ChatMessage.SanitizeInput(" This is my   message "));

            Assert.AreEqual("This is my message", ChatMessage.SanitizeInput(">This is my message"));
            Assert.AreEqual("This is my message", ChatMessage.SanitizeInput(" >> > >This is my message "));
            Assert.AreEqual("This is my message", ChatMessage.SanitizeInput("       >> > >This is my message "));

            Assert.AreEqual("This is my message", ChatMessage.SanitizeInput("   ## This is my message"));
            Assert.AreEqual("This is my message > ast", ChatMessage.SanitizeInput(">This is my message > ast"));

            Assert.AreEqual("This is my message", ChatMessage.SanitizeInput("   * * * This is my message"));
            Assert.AreEqual("This is my message", ChatMessage.SanitizeInput("   *  *   * * This is my message"));
            Assert.AreEqual("*** This is my message", ChatMessage.SanitizeInput(" *** This is my message"));
            Assert.AreEqual("This is my message", ChatMessage.SanitizeInput("   - - -  This is my message"));


            Assert.AreEqual("This is my message", ChatMessage.SanitizeInput("This is my message   * * * "));
            Assert.AreEqual("This is my message", ChatMessage.SanitizeInput("This is my message   *  *   * * "));
            Assert.AreEqual("This is my message ***", ChatMessage.SanitizeInput("This is my message *** "));
            Assert.AreEqual("This is my message", ChatMessage.SanitizeInput("This is my message   - - -  "));
            Assert.AreEqual("This is my message", ChatMessage.SanitizeInput("This is my message - - -"));


            Assert.AreEqual("This is my message", ChatMessage.SanitizeInput(">#This is my message"));
            Assert.AreEqual("This is my message", ChatMessage.SanitizeInput(">#>#>#This is my message"));
        }
    }
}
