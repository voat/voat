using Kaliko.ImageLibrary;
using Kaliko.ImageLibrary.Scaling;
using OpenGraph_Net;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Web;
using Message = Voat.Models.Message;

namespace Voat.Utils
{
    public static class ThumbGenerator
    {
        // public folder where thumbs should be saved
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

            var originalImage = new KalikoImage(response.GetResponseStream()) { BackgroundColor = Color.Black };
            originalImage.Scale(new PadScaling(MaxWidth, MaxHeight)).SaveJpg(DestinationPath + '\\' + randomFileName + ".jpg", 90);

            return randomFileName + ".jpg";
        }

        public static bool GenerateAvatar(Image inputImage, string userName, string mimetype)
        {
            try
            {
                string DestinationPath = HttpContext.Current.Server.MapPath("~/Storage/Avatars");

                var originalImage = new KalikoImage(inputImage);

                originalImage.Scale(new PadScaling(MaxWidth, MaxHeight)).SaveJpg(DestinationPath + '\\' + userName + ".jpg", 90);

                return true;
            }
            catch (Exception ex)
            {
                return false;
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
                    var graphUri = new Uri(submissionModel.MessageContent);
                    var graph = OpenGraph.ParseUrl(graphUri, userAgent: "Voat.co OpenGraph Parser");

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
                var graphUri = new Uri(submissionModel.MessageContent);
                var graph = OpenGraph.ParseUrl(graphUri, userAgent: "Voat.co OpenGraph Parser");

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