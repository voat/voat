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

namespace Voat.Tests.ControllerTests
{
    [TestClass]
    public class HomeControllerTests
    {
        [TestMethod]
        public void Index()
        {
            //CORE_PORT: Will probably not test controllers like this
            /*

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
            */

        }

    }
}
