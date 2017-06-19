using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;
using Voat.Tests.Infrastructure;
using Voat.Utilities;

namespace Voat.Tests.Utility
{
    [TestClass]
    public class HttpResponseTests : BaseUnitTest
    {

        [TestMethod]
        public async Task Test()
        {
            var x = new HttpResource("http://www.microsoft.com/en/us/default.aspx?redir=true");
            await x.Execute();

            //var w = x.Response.Headers.AcceptRanges.First();
            var s = new StreamReader(x.Stream);
            var contents = s.ReadToEnd();

            var ss = new MemoryStream();
            await x.Response.Content.CopyToAsync(ss);

        }

    }
}
