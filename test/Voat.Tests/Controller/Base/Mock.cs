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

//CORE_PORT: Will probably not test controllers like this
/*

using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Voat.Tests.ControllerTests
{
    public static class ControllerMock
    {
        public static string BasePath = "????";

        public static Mock<HttpContextBase> MockControllerContext(string userName, bool isAjaxRequest)
        {
            var request = new Mock<HttpRequestBase>();
            request.SetupGet(r => r.HttpMethod).Returns("GET");
            //request.SetupGet(r => r.IsAuthenticated).Returns(authenticated);
            request.SetupGet(r => r.ApplicationPath).Returns("/");
            request.SetupGet(r => r.ServerVariables).Returns((NameValueCollection)null);
            request.SetupGet(r => r.Url).Returns(new Uri("http://localhost/app", UriKind.Absolute));
            //request.SetupGet(r => r.QueryString).Returns()
            if (isAjaxRequest)
            {
                request.SetupGet(x => x.Headers).Returns(new System.Net.WebHeaderCollection { { "X-Requested-With", "XMLHttpRequest" } });
            }

            var server = new Mock<HttpServerUtilityBase>();
            server.Setup(x => x.MapPath(It.IsAny<string>())).Returns(BasePath);

            var response = new Mock<HttpResponseBase>();
            response.Setup(r => r.ApplyAppPathModifier(It.IsAny<string>())).Returns((String url) => url);

            var session = new MockHttpSession();

            var mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(c => c.Request).Returns(request.Object);
            mockHttpContext.Setup(c => c.Response).Returns(response.Object);
            mockHttpContext.Setup(c => c.Server).Returns(server.Object);
            mockHttpContext.Setup(x => x.Session).Returns(session);
            if (!String.IsNullOrEmpty(userName))
            {
                mockHttpContext.Setup(x => x.User).Returns(new GenericPrincipal(new GenericIdentity(userName), new string[0]));
            }
            else
            {
                mockHttpContext.Setup(x => x.User).Returns(new GenericPrincipal(new GenericIdentity(""), new string[0]));
            }

            return mockHttpContext;
        }

        public class MockHttpSession : HttpSessionStateBase
        {
            private readonly Dictionary<string, object> sessionStorage = new Dictionary<string, object>();

            public override object this[string name]
            {
                get { return sessionStorage.ContainsKey(name) ? sessionStorage[name] : null; }
                set { sessionStorage[name] = value; }
            }

            public override void Remove(string name)
            {
                sessionStorage.Remove(name);
            }
        }
    }
}
*/