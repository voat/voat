using Kaliko.ImageLibrary;
using OpenGraph_Net;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Web;
using Message = Whoaverse.Models.Message;

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

        public static string ThumbnailFromSubmissionModel(Message submissionModel)
        {
            var extension = Path.GetExtension(submissionModel.MessageContent);

            // this is a direct link to image
            if (extension != String.Empty)
            {
                if (extension == ".jpg" || extension == ".JPG" || extension == ".png" || extension == ".PNG" || extension == ".gif" || extension == ".GIF")
                {
                    try
                    {
                        var thumbFileName = GenerateThumbFromUrl(submissionModel.MessageContent);
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
                    var graph = OpenGraph.ParseUrl(submissionModel.MessageContent);

                    // open graph failed to find og:image element, abort thumbnail generation
                    if (graph.Image == null) return null;

                    var thumbFileName = GenerateThumbFromUrl(graph.Image.ToString());
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
                var graph = OpenGraph.ParseUrl(submissionModel.MessageContent);
                
                // open graph failed to find og:image element, abort thumbnail generation
                if (graph.Image == null) return null;

                var thumbFileName = GenerateThumbFromUrl(graph.Image.ToString());
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