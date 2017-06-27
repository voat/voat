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
        public async Task HttpResource_Get_NoRedirect()
        {
            var httpRehorse = new HttpResource("http://www.microsoft.com/en/us/default.aspx?redir=true");
            await httpRehorse.GiddyUp();

            //var w = x.Response.Headers.AcceptRanges.First();
            var s = new StreamReader(httpRehorse.Stream);
            var contents = s.ReadToEnd();

            var ss = new MemoryStream();
            await httpRehorse.Response.Content.CopyToAsync(ss);

        }

    }
}
