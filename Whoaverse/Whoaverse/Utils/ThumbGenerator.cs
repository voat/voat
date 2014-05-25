using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Whoaverse.Utils
{
    public static class ThumbGenerator
    {
        //public folder where thumbs should be saved
        private static string destinationPath = System.Web.HttpContext.Current.Server.MapPath("~/Thumbs");

        // setup default thumb resolution
        private static int maxHeight = 70;
        private static int maxWidth = 70;

        // generate a thumbnail while removing transparency and preserving aspect ratio
        public static string GenerateThumbFromUrl(string sourceUrl)
        {
            string randomFileName = GenerateRandomFilename();
            string fileExtension = Path.GetExtension(sourceUrl);

            try
            {
                WebRequest request = WebRequest.Create(sourceUrl);
                request.Timeout = 300;
                WebResponse response = request.GetResponse();
                Image originalImage = Image.FromStream(response.GetResponseStream());

                using (Bitmap b = new Bitmap(originalImage.Width, originalImage.Height))
                {
                    using (Graphics g = Graphics.FromImage(b))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                        if (fileExtension == ".png" || fileExtension == ".PNG")
                        {
                            //only for PNGs
                            g.Clear(Color.White);
                        }                        

                        g.DrawImageUnscaled(originalImage, 0, 0);
                    }

                    Save(b, maxHeight, maxWidth, 100, (destinationPath + '\\' + randomFileName + ".jpg"));
                    return randomFileName + ".jpg";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        // generate a thumbnail from youtube video
        public static string GenerateThumbFromYoutubeVideo(string sourceUrl)
        {
            string randomFileName = GenerateRandomFilename();
            
            try
            {
                // get youtube video id from url
                Uri tmpUri = new Uri(sourceUrl);
                string videoId = HttpUtility.ParseQueryString(tmpUri.Query).Get("v");               
                WebRequest request = WebRequest.Create("http://img.youtube.com/vi/" + videoId + "/hqdefault.jpg");
                request.Timeout = 300;
                WebResponse response = request.GetResponse();
                Image originalImage = Image.FromStream(response.GetResponseStream());

                using (Bitmap b = new Bitmap(originalImage.Width, originalImage.Height))
                {
                    using (Graphics g = Graphics.FromImage(b))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImageUnscaled(originalImage, 0, 0);
                    }

                    Save(b, maxHeight, maxWidth, 100, (destinationPath + '\\' + randomFileName + ".jpg"));
                    return randomFileName + ".jpg";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Credits: John-ph, http://www.codeproject.com/Tips/552141/Csharp-Image-resize-convert-and-save
        /// <summary>
        /// Method to resize, convert and save the image.
        /// </summary>
        /// <param name="image">Bitmap image.</param>
        /// <param name="maxWidth">resize width.</param>
        /// <param name="maxHeight">resize height.</param>
        /// <param name="quality">quality setting value.</param>
        /// <param name="filePath">file path.</param> 
        public static void Save(Bitmap image, int maxWidth, int maxHeight, int quality, string filePath)
        {
            // Get the image's original width and height
            int originalWidth = image.Width;
            int originalHeight = image.Height;

            // To preserve the aspect ratio
            float ratioX = (float)maxWidth / (float)originalWidth;
            float ratioY = (float)maxHeight / (float)originalHeight;
            float ratio = Math.Min(ratioX, ratioY);

            // New width and height based on aspect ratio
            int newWidth = (int)(originalWidth * ratio);
            int newHeight = (int)(originalHeight * ratio);

            // Convert other formats (including CMYK) to RGB.
            Bitmap newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);

            // Draws the image in the specified size with quality mode set to HighQuality
            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            // Get an ImageCodecInfo object that represents the JPEG codec.
            ImageCodecInfo imageCodecInfo = GetEncoderInfo(ImageFormat.Jpeg);

            // Create an Encoder object for the Quality parameter.
            Encoder encoder = Encoder.Quality;

            // Create an EncoderParameters object. 
            EncoderParameters encoderParameters = new EncoderParameters(1);

            // Save the image as a JPEG file with quality level.
            EncoderParameter encoderParameter = new EncoderParameter(encoder, quality);
            encoderParameters.Param[0] = encoderParameter;
            newImage.Save(filePath, imageCodecInfo, encoderParameters);
        }

        // Credits: John-ph, http://www.codeproject.com/Tips/552141/Csharp-Image-resize-convert-and-save
        /// <summary>
        /// Method to get encoder infor for given image format.
        /// </summary>
        /// <param name="format">Image format</param>
        /// <returns>image codec info.</returns>
        private static ImageCodecInfo GetEncoderInfo(ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().SingleOrDefault(c => c.FormatID == format.Guid);
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
            var location = System.IO.Path.Combine(destinationPath, fileName);

            return (System.IO.File.Exists(location));
        }
    }

}