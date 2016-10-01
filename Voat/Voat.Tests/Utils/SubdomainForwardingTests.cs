#region LICENSE

/*

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All portions of the code written by Voat, Inc. are Copyright(c) Voat, Inc.

    All Rights Reserved.

*/

#endregion LICENSE

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Voat.Tests.Utils
{
    public class SubdomainForwardingTests
    {
        [TestMethod]
        public void SubdomainToSubverseForward()
        {
            //XmlDocument x = new XmlDocument();
            //x.LoadXml("<customForward type=\"Voat.Web.SubdomainToSubverseForward, Voat.Web.UrlForwad\" regEx=\"(?'protocol'http(?:s)?)://(?'subverse'[a-zA-Z0-9]*)\\.(?'domain'[\\w-_]*\\.(?:com|co|net|org|us|info|mobi|biz|tv))(?'path'[\\w./_-]*)(?:(?'query'\\?[\\w=.\\-&amp;]*))?\" forwardUrl=\"{protocol}://{domain}/v/{subverse}{path}{query}\" />");
            //XmlNode node = x.DocumentElement;

            //Voat.Web.SubdomainToSubverseForward forward = new Voat.Web.SubdomainToSubverseForward();
            //forward.Load(node);

            //var response = forward.Process(new Voat.Web.UriForward(new Uri("https://news.voat.co")));

            //Assert.IsTrue(response.Changed, response.NewUri.ToString());
            //Assert.IsTrue(response.NewUri.ToString() == "https://voat.co/v/news/", response.NewUri.ToString());

            ////No match - we need to make sure that if it doesn't match we don't process
            //response = forward.Process(new Voat.Web.UriForward(new Uri("https://voat.co")));
            //Assert.IsTrue(!response.Changed, "Should have matched");

            ////Match with a path and querystring
            //response = forward.Process(new Voat.Web.UriForward(new Uri("https://news.voat.co/new?time=week")));
            //Assert.IsTrue(response.Changed, response.NewUri.ToString());
            //Assert.IsTrue(response.NewUri.ToString() == "https://voat.co/v/news/new?time=week", response.NewUri.ToString());
        }
    }
}
