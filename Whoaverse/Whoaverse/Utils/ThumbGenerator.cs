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
        private static string destinationPath = HttpContext.Current.Server.MapPath("~/Thumbs");

        // setup default thumb resolution
        private static int maxHeight = 70;
        private static int maxWidth = 70;

        // generate a thumbnail while removing transparency and preserving aspect ratio
        public static string GenerateThumbFromUrl(string sourceUrl)
        {
            string randomFileName = GenerateRandomFilename();

            try
            {
                WebRequest request = WebRequest.Create(sourceUrl);
                request.Timeout = 300;
                WebResponse response = request.GetResponse();

                var originalImage = new KalikoImage(response.GetResponseStream());
                originalImage.BackgroundColor = Color.Black;
                originalImage.GetThumbnailImage(maxWidth, maxHeight, ThumbnailMethod.Pad).SaveJpg(@destinationPath + '\\' + randomFileName + ".jpg", 90);

                return randomFileName + ".jpg";
            }
            catch (Exception)
            {
                throw;
            }
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
            var location = Path.Combine(destinationPath, fileName);

            return (File.Exists(location));
        }
    }

}