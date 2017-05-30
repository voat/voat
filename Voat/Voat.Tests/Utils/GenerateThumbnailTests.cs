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

namespace Voat.Tests.Utils
{
    [TestClass]
    public class GenerateThumbnailTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Thumbnail")]
        public async Task GenerateThumbFromWebsiteUrl()
        {
            var result = await ThumbGenerator.GenerateThumbFromWebpageUrl("http://www.yahoo.com", false);
            string path = Path.Combine(ThumbGenerator.DestinationPathThumbs, result);
            Assert.IsTrue(File.Exists(path), "Thumb did not get generated from site html");
            File.Delete(path);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Thumbnail")]
        public async Task GenerateThumbFromWebsiteUrl_Failure()
        {
            var result = await ThumbGenerator.GenerateThumbFromWebpageUrl("http://www.idontexistimprettysuremaybeIlladdrandom3243242.com", false);
            Assert.AreEqual(null, result, "Expecting no thumb");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Thumbnail")]
        //[ExpectedException(typeof(WebException))]
        public async Task GenerateThumbFromImageUrl_Failure()
        {
            await VoatAssert.ThrowsAsync<WebException>(() => {
                return ThumbGenerator.GenerateThumbFromImageUrl("https://idontexistimprettysuremaybeIlladdrandom3243242.co/graphics/voat-goat.png", 5000, false);
            });

            //var result = await ThumbGenerator.GenerateThumbFromImageUrl("https://idontexistimprettysuremaybeIlladdrandom3243242.co/graphics/voat-goat.png", 5000, false);
            //Assert.AreEqual(null, result, "Expecting no thumb");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Thumbnail")]
        public async Task GenerateThumbFromImageUrl()
        {
            var result = await ThumbGenerator.GenerateThumbFromImageUrl("https://voat.co/graphics/voat-goat.png", 5000, false);
            string path = Path.Combine(ThumbGenerator.DestinationPathThumbs, result);
            Assert.IsTrue(File.Exists(path), "Thumb did not get generated from image url");
            File.Delete(path);
        }
    }
}
