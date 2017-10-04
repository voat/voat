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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Voat.Utilities;
using System.IO;
using System.Net;
using Voat.Tests.Infrastructure;
using Voat.IO;
using System.Net.Http;
using Voat.Domain.Command;
using Voat.Common.Models;
using Voat.Configuration;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class GenerateThumbnailTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Thumbnail")]
        public async Task GenerateThumbFromWebsiteUrl()
        {
            var result = await ThumbGenerator.GenerateThumbnail("https://www.yahoo.com", false);

            var key = new FileKey(result.Response, FileType.Thumbnail);
            Assert.AreEqual(true, await FileManager.Instance.Exists(key), "Thumb did not get generated from image url");
            await FileManager.Instance.Delete(key);
            Assert.AreEqual(false, await FileManager.Instance.Exists(key), "Thumb did not delete");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Thumbnail")]
        public async Task GenerateThumbFromWebsiteUrl_Failure()
        {
            var result = await ThumbGenerator.GenerateThumbnail("http://www.idontexistimprettysuremaybeIlladdrandom3243242.com", false);
            Assert.AreNotEqual(Status.Success, result.Status, "Expecting no thumb");
            Assert.AreEqual("", result.Response);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Thumbnail")]
        //[ExpectedException(typeof(WebException))]
        public async Task GenerateThumbFromImageUrl_Failure()
        {
            //await VoatAssert.ThrowsAsync<TaskCanceledException>(() => {
            //    return ThumbGenerator.GenerateThumbnail("https://idontexistimprettysuremaybeIlladdrandom3243242.co/graphics/voat-goat.png", false);
            //});
            var result = await ThumbGenerator.GenerateThumbnail("https://idontexistimprettysuremaybeIlladdrandom3243242.co/graphics/voat-goat.png", false);
            Assert.AreNotEqual(Status.Success, result.Status, "Expecting no thumb");
            Assert.AreEqual("", result.Response);

            //var result = await ThumbGenerator.GenerateThumbFromImageUrl("https://idontexistimprettysuremaybeIlladdrandom3243242.co/graphics/voat-goat.png", 5000, false);
            //Assert.AreEqual(null, result, "Expecting no thumb");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Thumbnail")]
        public async Task GenerateThumbFromImageUrl()
        {
            var result = await ThumbGenerator.GenerateThumbnail("https://voat.co/graphics/voat-goat.png", false);
            var key = new FileKey(result.Response, FileType.Thumbnail);


            Assert.AreEqual(!VoatSettings.Instance.OutgoingTraffic.Enabled, await FileManager.Instance.Exists(key), "Thumb did not get generated from image url");
            await FileManager.Instance.Delete(key);

            Assert.AreEqual(false, await FileManager.Instance.Exists(key), "Thumb did not delete");
        }
    }
}
