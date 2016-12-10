using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Voat.Controllers;

namespace Voat.Tests.ControllerTests
{
    [TestClass]
    public class HomeControllerTests
    {
        [TestMethod]
        public void Index()
        {
            var requestContext = new RequestContext(ControllerMock.MockControllerContext(null, false).Object, new RouteData());
            var controller = new SubversesController()
            {
                Url = new UrlHelper(requestContext)
            };
            controller.ControllerContext = new ControllerContext()
            {
                Controller = controller,
                RequestContext = requestContext
            };

            
            var result = controller.SubverseIndex(null, "_front");


        }

    }
}
