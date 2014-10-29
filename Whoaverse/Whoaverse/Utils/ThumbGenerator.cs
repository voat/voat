using System.IO;
using System.Web;
using Kaliko.ImageLibrary;
using System;
using System.Drawing;
using System.Net;

namespace Whoaverse.Utils
{
    public static class ThumbGenerator
    {
        //public folder where thumbs should be saved
        private static readonly string DestinationPath = HttpContext.Current.Server.MapPath("~/Thumbs");

        // setup default thumb resolution
        private const int MaxHeight = 70;
        private const int MaxWidth = 70;

        // generate a thumbnail while removing transparency and preserving aspect ratio
        public static string GenerateThumbFromUrl(string sourceUrl)
        {
            var randomFileName = GenerateRandomFilename();

            var request = WebRequest.Create(sourceUrl);
            request.Timeout = 300;
            var response = request.GetResponse();

            var originalImage = new KalikoImage(response.GetResponseStream()) {BackgroundColor = Color.Black};
            originalImage.GetThumbnailImage(MaxWidth, MaxHeight, ThumbnailMethod.Pad).SaveJpg(DestinationPath + '\\' + randomFileName + ".jpg", 90);

            return randomFileName + ".jpg";
        }

        // Generate a random filename and make sure that the file does not exist.        
        private static string GenerateRandomFilename()
        {
            string rndFileName;

            do
            {
                rndFileName = Guid.NewGuid().ToString();
            } while (FileExists(rndFileName));

            return rndFileName;
        }

        // Check if a file exists at given location.
        private static bool FileExists(string fileName)
        {
            var location = Path.Combine(DestinationPath, fileName);

            return (File.Exists(location));
        }
    }

}