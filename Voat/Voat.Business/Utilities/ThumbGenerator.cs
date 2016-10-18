using Kaliko.ImageLibrary;
using Kaliko.ImageLibrary.Scaling;
using OpenGraph_Net;
using System;

using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Voat.Configuration;
using Voat.Data.Models;
using Voat.Utilities.Components;

namespace Voat.Utilities
{
    public static class ThumbGenerator
    {
        private static string _destinationPathThumbs = null;
        private static string _destinationPathAvatars = null;

        public static string DestinationPathThumbs { get { return _destinationPathThumbs; } }

        public static string DestinationPathAvatars { get { return _destinationPathAvatars; } }

        static ThumbGenerator()
        {
            //For UI/API
            if (HttpContext.Current != null)
            {
                _destinationPathThumbs = HttpContext.Current.Server.MapPath("~/Storage/Thumbs");
                _destinationPathAvatars = HttpContext.Current.Server.MapPath("~/Storage/Avatars");
            }

            //For Unit Testing
            else
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                _destinationPathThumbs = Path.Combine(baseDir, @"Storage\Thumbs");
                _destinationPathAvatars = Path.Combine(baseDir, @"Storage\Avatars");
                if (!Directory.Exists(_destinationPathThumbs))
                {
                    Directory.CreateDirectory(_destinationPathThumbs);
                }
                if (!Directory.Exists(_destinationPathAvatars))
                {
                    Directory.CreateDirectory(_destinationPathAvatars);
                }
            }
        }

        // setup default thumb resolution
        private const int MaxHeight = 70;

        private const int MaxWidth = 70;

        // generate a thumbnail while removing transparency and preserving aspect ratio
        public static async Task<string> GenerateThumbFromImageUrl(string imageUrl, int timeoutInMilliseconds = 3000)
        {
            var randomFileName = GenerateRandomFilename();
            var tempPath = Path.Combine(DestinationPathThumbs, $"{randomFileName}.jpg");

            var request = WebRequest.Create(imageUrl);
            request.Timeout = timeoutInMilliseconds; //Putts: extended this from 300 mills
            using (var response = request.GetResponse())
            {
                var originalImage = new KalikoImage(response.GetResponseStream()) { BackgroundColor = Color.Black };
                originalImage.Scale(new PadScaling(MaxWidth, MaxHeight)).SaveJpg(tempPath, 90);
            }

            // call upload to storage method if CDN config is enabled
            if (Settings.UseContentDeliveryNetwork)
            {
                if (FileSystemUtility.FileExists(tempPath, DestinationPathThumbs))
                {
                    await CloudStorageUtility.UploadBlobToStorageAsync(tempPath, "thumbs");

                    // delete local file after uploading to CDN
                    File.Delete(tempPath);
                }
            }

            return Path.GetFileName(tempPath);
        }

        // store uploaded avatar
        public static async Task<bool> GenerateAvatar(Image inputImage, string userName, string mimetype)
        {
            try
            {
                // store avatar locally
                var originalImage = new KalikoImage(inputImage);
                originalImage.Scale(new PadScaling(MaxWidth, MaxHeight)).SaveJpg(DestinationPathAvatars + '\\' + userName + ".jpg", 90);
                if (!Settings.UseContentDeliveryNetwork)
                    return true;

                // call upload to storage since CDN is enabled in config
                string tempAvatarLocation = DestinationPathAvatars + '\\' + userName + ".jpg";

                // the avatar file was not found at expected path, abort
                if (!FileSystemUtility.FileExists(tempAvatarLocation, DestinationPathAvatars))
                {
                    return false;
                }
                else if (Settings.UseContentDeliveryNetwork)
                {
                    // upload to CDN
                    await CloudStorageUtility.UploadBlobToStorageAsync(tempAvatarLocation, "avatars");

                    // delete local file after uploading to CDN
                    File.Delete(tempAvatarLocation);
                }
                return true;
            }
            catch (Exception ex)
            {
                EventLogger.Log(ex);
                return false;
            }
        }

        // Generate a random filename for a thumbnail and make sure that the file does not exist.
        private static string GenerateRandomFilename()
        {
            string rndFileName;

            // if CDN flag is active, check if file exists on CDN, otherwise check if file exists on local storage
            if (Settings.UseContentDeliveryNetwork)
            {
                // make sure blob with same name doesn't exist already
                do
                {
                    rndFileName = Guid.NewGuid().ToString();
                } while (CloudStorageUtility.BlobExists(rndFileName, "thumbs"));

                rndFileName = Guid.NewGuid().ToString();
            }
            else
            {
                do
                {
                    rndFileName = Guid.NewGuid().ToString();
                } while (FileSystemUtility.FileExists(rndFileName, DestinationPathThumbs));
            }

            return rndFileName;
        }

        // generate a thumbnail if submission is a direct link to image or video
        public static async Task<string> GenerateThumbFromWebpageUrl(string websiteUrl)
        {
            var extension = Path.GetExtension(websiteUrl);

            // this is a direct link to image
            if (extension != String.Empty)
            {
                if (extension == ".jpg" || extension == ".JPG" || extension == ".png" || extension == ".PNG" || extension == ".gif" || extension == ".GIF")
                {
                    try
                    {
                        var thumbFileName = await GenerateThumbFromImageUrl(websiteUrl);
                        return thumbFileName;
                    }
                    catch (Exception)
                    {
                        // thumnail generation failed, skip adding thumbnail
                        return null;
                    }
                }

                // try generating a thumbnail by using the Open Graph Protocol
                try
                {
                    var graphUri = new Uri(websiteUrl);
                    var graph = OpenGraph.ParseUrl(graphUri, userAgent: "Voat.co OpenGraph Parser");

                    // open graph failed to find og:image element, abort thumbnail generation
                    if (graph.Image == null)
                        return null;

                    var thumbFileName = await GenerateThumbFromImageUrl(graph.Image.ToString());
                    return thumbFileName;
                }
                catch (Exception ex)
                {
                    EventLogger.Log(ex);
                    // thumnail generation failed, skip adding thumbnail
                    return null;
                }
            }

            // this is not a direct link to an image, it could be a link to an article or video
            // try generating a thumbnail by using the Open Graph Protocol
            try
            {
                var graphUri = new Uri(websiteUrl);
                var graph = OpenGraph.ParseUrl(graphUri, userAgent: "Voat.co OpenGraph Parser");

                // open graph failed to find og:image element, abort thumbnail generation
                if (graph.Image == null)
                    return null;

                var thumbFileName = await GenerateThumbFromImageUrl(graph.Image.ToString());
                return thumbFileName;
            }
            catch (Exception ex)
            {
                EventLogger.Log(ex);
                // thumnail generation failed, skip adding thumbnail
                return null;
            }
        }
    }
}
