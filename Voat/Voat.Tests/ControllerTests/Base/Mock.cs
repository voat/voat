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
