using Kaliko.ImageLibrary;
using Kaliko.ImageLibrary.Scaling;
using OpenGraph_Net;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Voat.Business.Utilities;
using Voat.Configuration;

namespace Voat.Utilities
{
    public static class ThumbGenerator
    {
        // public folder where thumbs should be saved
        private static readonly string DestinationPathThumbs = HttpContext.Current.Server.MapPath("~/Thumbs");
        private static readonly string DestinationPathAvatars = HttpContext.Current.Server.MapPath("~/Storage/Avatars");

        // setup default thumb resolution
        private const int MaxHeight = 70;
        private const int MaxWidth = 70;

        // generate a thumbnail while removing transparency and preserving aspect ratio
        public static async Task<string> GenerateThumbFromUrl(string sourceUrl)
        {
            var randomFileName = GenerateRandomFilename();

            var request = WebRequest.Create(sourceUrl);
            request.Timeout = 300;
            using (var response = request.GetResponse())
            {

                var originalImage = new KalikoImage(response.GetResponseStream()) { BackgroundColor = Color.Black };


                originalImage.Scale(new PadScaling(MaxWidth, MaxHeight)).SaveJpg(DestinationPathThumbs + '\\' + randomFileName + ".jpg", 90);
            }
            // call upload to storage method if CDN config is enabled
            if (Settings.UseContentDeliveryNetwork)
            {
                string tempThumbLocation = DestinationPathThumbs + '\\' + randomFileName + ".jpg";

                if (FileSystemUtility.FileExists(tempThumbLocation, DestinationPathThumbs))
                {
                    await CloudStorageUtility.UploadBlobToStorageAsync(tempThumbLocation, "thumbs");

                    // delete local file after uploading to CDN
                    File.Delete(tempThumbLocation);
                }
            }

            return randomFileName + ".jpg";
        }

        // store uploaded avatar
        public static async Task<bool> GenerateAvatar(Image inputImage, string userName, string mimetype)
        {
            try
            {
                // store avatar locally
                var originalImage = new KalikoImage(inputImage);
                originalImage.Scale(new PadScaling(MaxWidth, MaxHeight)).SaveJpg(DestinationPathAvatars + '\\' + userName + ".jpg", 90);
                if (!Settings.UseContentDeliveryNetwork) return true;

                // call upload to storage since CDN is enabled in config
                string tempAvatarLocation = DestinationPathAvatars + '\\' + userName + ".jpg";

                // the avatar file was not found at expected path, abort
                if (!FileSystemUtility.FileExists(tempAvatarLocation, DestinationPathAvatars)) return false;

                // upload to CDN
                await CloudStorageUtility.UploadBlobToStorageAsync(tempAvatarLocation, "avatars");

                // delete local file after uploading to CDN
                File.Delete(tempAvatarLocation);
                return true;
            }
            catch (Exception ex)
            {
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
        public static async Task<string> ThumbnailFromSubmissionModel(Data.Models.Submission submissionModel)
        {
            var extension = Path.GetExtension(submissionModel.Content);

            // this is a direct link to image
            if (extension != String.Empty)
            {
                if (extension == ".jpg" || extension == ".JPG" || extension == ".png" || extension == ".PNG" || extension == ".gif" || extension == ".GIF")
                {
                    try
                    {
                        var thumbFileName = await GenerateThumbFromUrl(submissionModel.Content);
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
                    var graphUri = new Uri(submissionModel.Content);
                    var graph = OpenGraph.ParseUrl(graphUri, userAgent: "Voat.co OpenGraph Parser");

                    // open graph failed to find og:image element, abort thumbnail generation
                    if (graph.Image == null) return null;

                    var thumbFileName = await GenerateThumbFromUrl(graph.Image.ToString());
                    return thumbFileName;
                }
                catch (Exception)
                {
                    // thumnail generation failed, skip adding thumbnail
                    return null;
                }
            }

            // this is not a direct link to an image, it could be a link to an article or video
            // try generating a thumbnail by using the Open Graph Protocol
            try
            {
                var graphUri = new Uri(submissionModel.Content);
                var graph = OpenGraph.ParseUrl(graphUri, userAgent: "Voat.co OpenGraph Parser");

                // open graph failed to find og:image element, abort thumbnail generation
                if (graph.Image == null) return null;

                var thumbFileName = await GenerateThumbFromUrl(graph.Image.ToString());
                return thumbFileName;
            }
            catch (Exception)
            {
                // thumnail generation failed, skip adding thumbnail
                return null;
            }
        }
    }
}
