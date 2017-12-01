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
using Voat.Common;
using Voat.Configuration;
using Voat.Tests.Infrastructure;
using System.Threading.Tasks;
using Voat.IO;
using Voat.Utilities;
using System.Linq;
using Voat.Common.Models;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class FileManagerTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Formatting"), TestCategory("Formatting.Paths")]
        public void MiscPathTests()
        {
            var options = new PathOptions(true, true, "Somesite.com");
            string[] parts = new string[] { "", " ", "white space ", "x.jpg" };
            var expectedUrl = $"http{(VoatSettings.Instance.ForceHTTPS ? "s" : "")}://Somesite.com/white space/x.jpg";

            options.EscapeUrl = false;
            var url = VoatUrlFormatter.BuildUrlPath(null, options, parts);

            Assert.AreEqual(expectedUrl, url, "Condition:1.1");

            options.EscapeUrl = true;
            url = VoatUrlFormatter.BuildUrlPath(null, options, parts);

            Assert.AreEqual(Uri.EscapeUriString(expectedUrl), url, "Condition:1.2");

            options.EscapeUrl = false;
            options.Normalization = Normalization.Lower;
            url = VoatUrlFormatter.BuildUrlPath(null, options, parts);
            Assert.AreEqual(expectedUrl.ToLower(), url, "Condition:1.3");

            options.EscapeUrl = false;
            options.Normalization = Normalization.Upper;
            url = VoatUrlFormatter.BuildUrlPath(null, options, parts);
            Assert.AreEqual(expectedUrl.ToUpper(), url, "Condition:1.4");

        }
        public void VerifyPath(FileManager fm, FileKey key, string partialPath, string domain = null)
        {
            string url = null;
            string domainPart = (domain == null || (fm is AzureBlobFileManager && key.FileType == FileType.Badge) ? VoatSettings.Instance.SiteDomain : domain);

            if (key != null && !String.IsNullOrEmpty(key.ID))
            {

                url = fm.Uri(key, new PathOptions() { FullyQualified = false, ProvideProtocol = false });
                Assert.AreEqual($"/{partialPath}", url, "Condition:1.1");

                url = fm.Uri(key, new PathOptions() { FullyQualified = true, ProvideProtocol = false });
                Assert.AreEqual($"//{domainPart}/{partialPath}", url, "Condition:1.2");

                url = fm.Uri(key, new PathOptions() { FullyQualified = true, ProvideProtocol = true });
                Assert.AreEqual($"http{(VoatSettings.Instance.ForceHTTPS ? "s" : "")}://{domainPart}/{partialPath}", url, "Condition:1.3");

            }
            else
            {
                url = fm.Uri(key, new PathOptions() { FullyQualified = false, ProvideProtocol = false });
                Assert.IsNull(url, "Condition:1.4");
            }

        }
        [TestMethod]
        [TestCategory("Formatting"), TestCategory("Formatting.Paths")]
        public void Local_ExceptionPaths()
        {
            var fm = new LocalNetworkFileManager();
            
            var key = new FileKey() { ID = null, FileType = FileType.Avatar };
            VerifyPath(fm, key, "");

            key = new FileKey() { ID = "", FileType = FileType.Avatar };
            VerifyPath(fm, key, "");
        }
        [TestMethod]
        [TestCategory("Formatting"), TestCategory("Formatting.Paths")]
        public void Local_AvatarPath()
        {
            var fileName = "puttitout.jpg";
            var fm = new LocalNetworkFileManager();
            var key = new FileKey() { ID = fileName, FileType = FileType.Avatar };
            string partialPath = String.Join("/", new string[] { VoatSettings.Instance.DestinationPathAvatars, fileName }.ToPathParts());

            VerifyPath(fm, key, partialPath);

            

            ////LOCAL
            //VoatSettings.Instance.UseContentDeliveryNetwork = false;

            //var localPath = String.Join('/', VoatSettings.Instance.DestinationPathAvatars.ToPathParts());

            //result = VoatUrlFormatter.AvatarPath(username, avatarFileName, false, true, true);
            //Assert.AreEqual($"~/{localPath}/{username}.jpg", result, "Condition:1.2");

            //result = VoatUrlFormatter.AvatarPath(username, avatarFileName, true, false, true);
            //Assert.AreEqual($"//{VoatSettings.Instance.SiteDomain}/{localPath}/{username}.jpg", result, "Condition:2.2");

            //result = VoatUrlFormatter.AvatarPath(username, avatarFileName, true, true, true);
            //Assert.AreEqual($"http{(VoatSettings.Instance.ForceHTTPS ? "s" : "")}://{VoatSettings.Instance.SiteDomain}/{localPath}/{username}.jpg", result, "Condition:3.2");

            ////Reset original value
            //VoatSettings.Instance.UseContentDeliveryNetwork = originalSetting;


            //string username = "username";
            //string avatarFileName = "username.jpg";
            //string result = "";


            //var originalSetting = VoatSettings.Instance.UseContentDeliveryNetwork;

            ////CDN
            //VoatSettings.Instance.UseContentDeliveryNetwork = true;

            //result = VoatUrlFormatter.AvatarPath(username, avatarFileName, false, true, true);
            //Assert.AreEqual(String.Format("~/avatars/{0}.jpg", username), result, "Condition:1");

            //result = VoatUrlFormatter.AvatarPath(username, avatarFileName, true, false, true);
            //Assert.AreEqual(String.Format("//cdn.voat.co/avatars/{0}.jpg", username), result, "Condition:2");

            //result = VoatUrlFormatter.AvatarPath(username, avatarFileName, true, true, true);
            //Assert.AreEqual($"http{(VoatSettings.Instance.ForceHTTPS ? "s" : "")}://cdn.voat.co/avatars/{username}.jpg", result, "Condition:3");


        }

        [TestMethod]
        [TestCategory("Formatting"), TestCategory("Formatting.Paths")]
        public void Local_BadgePath()
        {
            var fileName = "Donor.jpg";
            var fm = new LocalNetworkFileManager();
            var key = new FileKey() { ID = fileName, FileType = FileType.Badge };
            string partialPath = String.Join("/", new string[] { "~/images/badges", fileName }.ToPathParts());

            VerifyPath(fm, key, partialPath);

        }

        [TestMethod]
        [TestCategory("Formatting"), TestCategory("Formatting.Paths")]
        public void Local_ThumbnailPath()
        {
            var fileName = "SOMEQUIDHERE.jpg";
            var fm = new LocalNetworkFileManager();
            var key = new FileKey() { ID = fileName, FileType = FileType.Thumbnail };
            string partialPath = String.Join("/", new string[] { VoatSettings.Instance.DestinationPathThumbs, fileName }.ToPathParts());

            VerifyPath(fm, key, partialPath);

        }
        [TestMethod]
        [TestCategory("Formatting"), TestCategory("Formatting.Paths")]
        public async Task Local_FileLifeCycle()
        {
            var fileName = Guid.NewGuid().ToString() + ".png";
            var fm = new LocalNetworkFileManager();
            var key = new FileKey() { ID = fileName, FileType = FileType.Thumbnail };
            string partialPath = String.Join("/", new string[] { VoatSettings.Instance.DestinationPathThumbs, fileName }.ToPathParts());

            Assert.IsFalse(await fm.Exists(key));

            using (var httpRehorse = new HttpResource("https://voat.co/Graphics/voat-goat.png"))
            {
                await httpRehorse.GiddyUp();

                await fm.Upload(key, httpRehorse.Stream);
            }

            Assert.IsTrue(await fm.Exists(key));

            var url = fm.Uri(key, new PathOptions() { FullyQualified = false, ProvideProtocol = false });
            Assert.AreEqual($"/{partialPath}", url, "Condition:1.1");

            fm.Delete(key);

            Assert.IsFalse(await fm.Exists(key));
        }

        [TestMethod]
        [TestCategory("Formatting"), TestCategory("Formatting.Paths")]
        public void ContentDelivery_AvatarPath()
        {
            var fileName = "puttitout.jpg";
            var fm = new AzureBlobFileManager("");
            var key = new FileKey() { ID = fileName, FileType = FileType.Avatar };
            string partialPath = String.Join("/", new string[] { "avatars", fileName }.ToPathParts());

            VerifyPath(fm, key, partialPath, VoatSettings.Instance.ContentDeliveryDomain);



            ////LOCAL
            //VoatSettings.Instance.UseContentDeliveryNetwork = false;

            //var localPath = String.Join('/', VoatSettings.Instance.DestinationPathAvatars.ToPathParts());

            //result = VoatUrlFormatter.AvatarPath(username, avatarFileName, false, true, true);
            //Assert.AreEqual($"~/{localPath}/{username}.jpg", result, "Condition:1.2");

            //result = VoatUrlFormatter.AvatarPath(username, avatarFileName, true, false, true);
            //Assert.AreEqual($"//{VoatSettings.Instance.SiteDomain}/{localPath}/{username}.jpg", result, "Condition:2.2");

            //result = VoatUrlFormatter.AvatarPath(username, avatarFileName, true, true, true);
            //Assert.AreEqual($"http{(VoatSettings.Instance.ForceHTTPS ? "s" : "")}://{VoatSettings.Instance.SiteDomain}/{localPath}/{username}.jpg", result, "Condition:3.2");

            ////Reset original value
            //VoatSettings.Instance.UseContentDeliveryNetwork = originalSetting;


            //string username = "username";
            //string avatarFileName = "username.jpg";
            //string result = "";


            //var originalSetting = VoatSettings.Instance.UseContentDeliveryNetwork;

            ////CDN
            //VoatSettings.Instance.UseContentDeliveryNetwork = true;

            //result = VoatUrlFormatter.AvatarPath(username, avatarFileName, false, true, true);
            //Assert.AreEqual(String.Format("~/avatars/{0}.jpg", username), result, "Condition:1");

            //result = VoatUrlFormatter.AvatarPath(username, avatarFileName, true, false, true);
            //Assert.AreEqual(String.Format("//cdn.voat.co/avatars/{0}.jpg", username), result, "Condition:2");

            //result = VoatUrlFormatter.AvatarPath(username, avatarFileName, true, true, true);
            //Assert.AreEqual($"http{(VoatSettings.Instance.ForceHTTPS ? "s" : "")}://cdn.voat.co/avatars/{username}.jpg", result, "Condition:3");


        }

        [TestMethod]
        [TestCategory("Formatting"), TestCategory("Formatting.Paths")]
        public void ContentDelivery_BadgePath()
        {
            var fileName = "Donor.jpg";
            var fm = new AzureBlobFileManager("");
            var key = new FileKey() { ID = fileName, FileType = FileType.Badge };
            string partialPath = String.Join("/", new string[] { "~/images/badges", fileName }.ToPathParts());

            VerifyPath(fm, key, partialPath, VoatSettings.Instance.ContentDeliveryDomain);

        }

        [TestMethod]
        [TestCategory("Formatting"), TestCategory("Formatting.Paths")]
        public void ContentDelivery_ThumbnailPath()
        {
            var fileName = "SOMEQUIDHERE.jpg";
            var fm = new AzureBlobFileManager("");
            var key = new FileKey() { ID = fileName, FileType = FileType.Thumbnail };
            string partialPath = String.Join("/", new string[] { "thumbs", fileName }.ToPathParts());

            VerifyPath(fm, key, partialPath, VoatSettings.Instance.ContentDeliveryDomain);

        }
        //CORE_PORT: Not fully ported
        [TestMethod]
        [TestCategory("Formatting"), TestCategory("Formatting.Paths")]
        public async Task ContentDelivery_FileLifeCycle()
        {
            string fmType = "AzureBlob";

            var handler = FileManagerConfigurationSettings.Instance.Handlers.FirstOrDefault(x => x.Name.IsEqual(fmType));

            if (handler == null)
            {
                Assert.Inconclusive($"Can't find {fmType}");
            }
            var fm = handler.Construct<AzureBlobFileManager>();
            if (fm == null)
            {
                Assert.Inconclusive($"Can't construct {fmType}");
            }
            var fileName = Guid.NewGuid().ToString() + ".png";
            var key = new FileKey() { ID = fileName, FileType = FileType.Thumbnail };
            //string partialPath = String.Join("/", new string[] { VoatSettings.Instance.DestinationPathThumbs, fileName }.ToPathParts());

            Assert.IsFalse(await fm.Exists(key));

            using (var httpRehorse = new HttpResource("https://voat.co/Graphics/voat-goat.png"))
            {
                await httpRehorse.GiddyUp();

                await fm.Upload(key, httpRehorse.Stream);
            }
            
            Assert.IsTrue(await fm.Exists(key));

            var url = fm.Uri(key, new PathOptions() { FullyQualified = true, ProvideProtocol = true });
            //Assert.AreEqual($"/{partialPath}", url, "Condition:1.1");

            await fm.Delete(key);

            Assert.IsFalse(await fm.Exists(key));
        }
        [TestMethod]
        [TestCategory("Formatting"), TestCategory("Formatting.Paths")]
        public void ContentDelivery_ExceptionPaths()
        {
            var fm = new AzureBlobFileManager("");
            
            var key = new FileKey() { ID = null, FileType = FileType.Avatar };
            VerifyPath(fm, key, "");

            key = new FileKey() { ID = "", FileType = FileType.Avatar };
            VerifyPath(fm, key, "");
        }
    }
}
