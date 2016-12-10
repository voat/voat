using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Voat.Utilities;
using System.IO;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class GenerateThumbnailTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Thumbnail")]
        public async Task GenerateThumbFromWebsiteUrl()
        {
            var result = await ThumbGenerator.GenerateThumbFromWebpageUrl("http://www.yahoo.com");
            string path = Path.Combine(ThumbGenerator.DestinationPathThumbs, result);
            Assert.IsTrue(File.Exists(path), "Thumb did not get generated from site html");
            File.Delete(path);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Thumbnail")]
        public async Task GenerateThumbFromImageUrl()
        {
            var result = await ThumbGenerator.GenerateThumbFromImageUrl("https://i.sli.mg/W2fsxZ.jpg", 5000);
            string path = Path.Combine(ThumbGenerator.DestinationPathThumbs, result);
            Assert.IsTrue(File.Exists(path), "Thumb did not get generated from image url");
            File.Delete(path);
        }
    }
}
