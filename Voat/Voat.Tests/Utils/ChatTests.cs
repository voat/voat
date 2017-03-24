using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class ChatTests : BaseUnitTest
    {

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

        }
    }
}
