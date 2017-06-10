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

using System;
using System.Configuration;
using Voat.Configuration;
using Voat.Utilities;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class VoatPathHelperTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Formatting"), TestCategory("Formatting.Paths")]
        public void AvatarPath()
        {
            string username = "username";
            string avatarFileName = "username.jpg";
            string result = "";


            var originalSetting = VoatSettings.Instance.UseContentDeliveryNetwork;

            //CDN
            VoatSettings.Instance.UseContentDeliveryNetwork = true;

            result = VoatPathHelper.AvatarPath(username, avatarFileName, false, true, true);
            Assert.AreEqual(String.Format("~/avatars/{0}.jpg", username), result, "Condition:1");

            result = VoatPathHelper.AvatarPath(username, avatarFileName, true, false, true);
            Assert.AreEqual(String.Format("//cdn.voat.co/avatars/{0}.jpg", username), result, "Condition:2");

            result = VoatPathHelper.AvatarPath(username, avatarFileName, true, true, true);
            Assert.AreEqual($"http{(VoatSettings.Instance.ForceHTTPS ? "s" : "")}://cdn.voat.co/avatars/{username}.jpg", result, "Condition:3");


            //LOCAL
            VoatSettings.Instance.UseContentDeliveryNetwork = false;

            result = VoatPathHelper.AvatarPath(username, avatarFileName, false, true, true);
            Assert.AreEqual(String.Format("~/Storage/Avatars/{0}.jpg", username), result, "Condition:1.2");

            result = VoatPathHelper.AvatarPath(username, avatarFileName, true, false, true);
            Assert.AreEqual($"//{VoatSettings.Instance.SiteDomain}/Storage/Avatars/{username}.jpg", result, "Condition:2.2");

            result = VoatPathHelper.AvatarPath(username, avatarFileName, true, true, true);
            Assert.AreEqual($"http{(VoatSettings.Instance.ForceHTTPS ? "s" : "")}://{VoatSettings.Instance.SiteDomain}/Storage/Avatars/{username}.jpg", result, "Condition:3.2");

            //Reset original value
            VoatSettings.Instance.UseContentDeliveryNetwork = originalSetting;

        }

        [TestMethod]
        [TestCategory("Formatting"), TestCategory("Formatting.Paths")]
        public void BadgePath()
        {
            //Badges don't use the CDN right now, only the UI
            string filename = "developer.jpg";
            string result = "";
            //string domain = ConfigurationManager.AppSettings["ui.domain"];
            //domain = (String.IsNullOrEmpty(domain) ? "voat.co" : domain);

            result = VoatPathHelper.BadgePath(filename, false);
            Assert.AreEqual(String.Format("~/Graphics/Badges/{0}", filename), result, "Condition:1");

            result = VoatPathHelper.BadgePath(filename, true, true);
            Assert.AreEqual($"http{(VoatSettings.Instance.ForceHTTPS ? "s" : "")}://{VoatSettings.Instance.SiteDomain}/Graphics/Badges/{filename}", result, "Condition:2");

            result = VoatPathHelper.BadgePath(filename, true, false);
            Assert.AreEqual($"//{VoatSettings.Instance.SiteDomain}/Graphics/Badges/{filename}", result, "Condition:3");

            //Assert.Inconclusive();
        }

        [TestMethod]
        [TestCategory("Formatting"), TestCategory("Formatting.Paths")]
        public void ThumbnailPath()
        {
            string filename = Guid.NewGuid().ToString() + ".jpg";
            string result = "";



            var originalSetting = VoatSettings.Instance.UseContentDeliveryNetwork;

            //CDN
            VoatSettings.Instance.UseContentDeliveryNetwork = true;

            result = VoatPathHelper.ThumbnailPath(filename);
            Assert.AreEqual(String.Format("~/thumbs/{0}", filename), result, "Condition:1");

            result = VoatPathHelper.ThumbnailPath(filename, true);
            Assert.AreEqual(String.Format("//cdn.voat.co/thumbs/{0}", filename), result, "Condition:2");

            result = VoatPathHelper.ThumbnailPath(filename, true, true);
            Assert.AreEqual($"http{(VoatSettings.Instance.ForceHTTPS ? "s" : "")}://cdn.voat.co/thumbs/{filename}", result, "Condition:3");


            //LOCAL
            VoatSettings.Instance.UseContentDeliveryNetwork = false;

            result = VoatPathHelper.ThumbnailPath(filename);
            Assert.AreEqual(String.Format("~/thumbs/{0}", filename), result, "Condition:1");

            result = VoatPathHelper.ThumbnailPath(filename, true);
            Assert.AreEqual($"//{VoatSettings.Instance.SiteDomain}/thumbs/{filename}", result, "Condition:2");

            result = VoatPathHelper.ThumbnailPath(filename, true, true);
            Assert.AreEqual($"http{(VoatSettings.Instance.ForceHTTPS ? "s" : "")}://{VoatSettings.Instance.SiteDomain}/thumbs/{filename}", result, "Condition:3");


            //Reset original value
            VoatSettings.Instance.UseContentDeliveryNetwork = originalSetting;

            
        }
    }
}
