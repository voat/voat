using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;
using Voat.IO.Email;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.Utility
{
    [TestClass]
    public class EmailTests : BaseUnitTest
    {

        [TestMethod]
        public async Task TestSendGrid()
        {
            var name = "SendGrid";
            var handler = EmailConfigurationSettings.Instance.Handlers.FirstOrDefault(x => x.Name.IsEqual(name));
            if (handler == null)
            {
                Assert.Inconclusive($"Can't find email sender: {name}");
            }
            var sender = handler.Construct<IEmailSender>();
            await sender.SendEmail("test@genericity.com", "Test Email", "<h1>Test</h1><br/><br/><b>Hi!</b><br/>Nice to see you. Here is a link: http://www.yahoo.com");


        }

    }
}
