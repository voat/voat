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

using System;
using System.Configuration;
using Voat.Configuration;
using Voat.Utilities;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class VoatPathHelperTests
    {
        [TestMethod]
        [TestCategory("Formatting"), TestCategory("Formatting.Paths")]
        public void AvatarPath()
        {
            string username = "username";
            string result = "";

            result = VoatPathHelper.AvatarPath(username, false, true, true);

            if (Settings.UseContentDeliveryNetwork)
            {
                Assert.IsTrue(result == String.Format("~/avatars/{0}.jpg", username), "Condition:1");
            }
            else
            {
                Assert.IsTrue(result == String.Format("~/Storage/Avatars/{0}.jpg", username), "Condition:1.2");
            }

            result = VoatPathHelper.AvatarPath(username, true, false, true);
            if (Settings.UseContentDeliveryNetwork)
            {
                Assert.IsTrue(result == String.Format("//cdn.voat.co/avatars/{0}.jpg", username), "Condition:2");
            }
            else
            {
                Assert.IsTrue(result == String.Format("//voat.co/Storage/Avatars/{0}.jpg", username), "Condition:2.2");
            }

            result = VoatPathHelper.AvatarPath(username, true, true, true);
            if (Settings.UseContentDeliveryNetwork)
            {
                Assert.IsTrue(result == String.Format("https://cdn.voat.co/avatars/{0}.jpg", username), "Condition:3");
            }
            else
            {
                Assert.IsTrue(result == String.Format("https://voat.co/Storage/Avatars/{0}.jpg", username), "Condition:3.2");
            }

            //Assert.Inconclusive();
        }

        [TestMethod]
        [TestCategory("Formatting"), TestCategory("Formatting.Paths")]
        public void BadgePath()
        {
            //Badges don't use the CDN right now, only the UI
            string filename = "developer.jpg";
            string result = "";
            string domain = ConfigurationManager.AppSettings["ui.domain"];
            domain = (String.IsNullOrEmpty(domain) ? "voat.co" : domain);

            result = VoatPathHelper.BadgePath(filename, false);
            Assert.IsTrue(result == String.Format("~/Graphics/Badges/{0}", filename), "Condition:1");

            result = VoatPathHelper.BadgePath(filename, true, true);
            Assert.IsTrue(result == String.Format("https://{1}/Graphics/Badges/{0}", filename, domain), "Condition:2");

            result = VoatPathHelper.BadgePath(filename, true, true);
            Assert.IsTrue(result == String.Format("https://{1}/Graphics/Badges/{0}", filename, domain), "Condition:3");

            //Assert.Inconclusive();
        }

        [TestMethod]
        [TestCategory("Formatting"), TestCategory("Formatting.Paths")]
        public void ThumbnailPath()
        {
            string filename = Guid.NewGuid().ToString() + ".jpg";
            string result = "";

            result = VoatPathHelper.ThumbnailPath(filename);
            if (Settings.UseContentDeliveryNetwork)
            {
                Assert.IsTrue(result == String.Format("~/thumbs/{0}", filename), "Condition:1");
            }
            else
            {
                Assert.IsTrue(result == String.Format("~/thumbs/{0}", filename), "Condition:1");
            }

            result = VoatPathHelper.ThumbnailPath(filename, true);
            if (Settings.UseContentDeliveryNetwork)
            {
                Assert.IsTrue(result == String.Format("//cdn.voat.co/thumbs/{0}", filename), "Condition:2");
            }
            else
            {
                Assert.IsTrue(result == String.Format("//voat.co/thumbs/{0}", filename), "Condition:2");
            }

            result = VoatPathHelper.ThumbnailPath(filename, true, true);
            if (Settings.UseContentDeliveryNetwork)
            {
                Assert.IsTrue(result == String.Format("https://cdn.voat.co/thumbs/{0}", filename), "Condition:3");
            }
            else
            {
                Assert.IsTrue(result == String.Format("https://voat.co/thumbs/{0}", filename), "Condition:3");
            }
        }
    }
}
