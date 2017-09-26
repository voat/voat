using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace Voat.Imaging
{
    public class ImageHandler
    {
        //This is a direct port for default thumbnails that Voat has used in the past
        public static Stream Resize(Stream input, Size maximumSize, bool forceJpegFormat = false, bool square = true, bool scale = true, bool center = true)
        {
            MemoryStream output = null;
            using (var image = Image.FromStream(input))
            {
                using (var bitmapImage = new Bitmap(image))
                {
                    var destinationSize = new Size(maximumSize.Width, maximumSize.Height);
                    //Sizing
                    Size scaledSize = scale ? Scale(bitmapImage.Size, maximumSize) : maximumSize;
                    //The original image size is our source
                    var sourceRectangle = new Rectangle(0, 0, bitmapImage.Size.Width, bitmapImage.Size.Height);

                    //default destination
                    var destinationRectangle = new Rectangle(0, 0, scaledSize.Width, scaledSize.Height);
                    //centered destination
                    if (square && center)
                    {
                        destinationRectangle = Center(maximumSize, scaledSize);
                    }
                    //allow non-squared output
                    if (!square)
                    {
                        destinationSize = destinationRectangle.Size;
                    }

                    var resized = new Bitmap(destinationSize.Width, destinationSize.Height);

                    using (var graphics = Graphics.FromImage(resized))
                    {
                        graphics.CompositingQuality = CompositingQuality.HighSpeed;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.CompositingMode = CompositingMode.SourceCopy;

                        //Assuming that if we are forcing jpeg we are creating site thumbnails which uses black bars
                        if (forceJpegFormat)
                        {
                            //Draw black background
                            graphics.DrawRectangle(new Pen(new SolidBrush(Color.Black)), new Rectangle(0, 0, maximumSize.Width, maximumSize.Height));
                        }

                        graphics.DrawImage(bitmapImage, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);

                        output = new MemoryStream();
                        var qualityParamId = System.Drawing.Imaging.Encoder.Quality;
                        var encoderParameters = new EncoderParameters(1);
                        encoderParameters.Param[0] = new EncoderParameter(qualityParamId, 100);

                        var codec = ImageCodecInfo.GetImageDecoders().FirstOrDefault(x =>
                            //Force JPEG
                            (forceJpegFormat && x.FormatID == ImageFormat.Jpeg.Guid)
                            //Allow source format
                            || (!forceJpegFormat && x.FormatID == image.RawFormat.Guid)
                        );

                        resized.Save(output, codec, encoderParameters);

                        //reset resized stream
                        output.Seek(0, SeekOrigin.Begin);
                    }
                }
            }
            

            return output;
        }
        public static Rectangle Center(Size destinationSize, Size scaledSize)
        {
            var shiftedWidthStart = 0;
            var shiftedHeightStart = 0;

            shiftedWidthStart = (destinationSize.Width - scaledSize.Width) / 2;
            shiftedHeightStart = (destinationSize.Height - scaledSize.Height) / 2;

            return new Rectangle(shiftedWidthStart, shiftedHeightStart, scaledSize.Width, scaledSize.Height);
        }
        public static Size Scale(Size currentSize, Size maxSize, float scalePercentage = 0.0f)
        {
            var scaleWidth = currentSize.Width;
            var scaleHeight = currentSize.Height;

            if (scalePercentage == 0.0f && (maxSize.Width > 0 || maxSize.Height > 0))
            {
                if (currentSize.Width > currentSize.Height)
                {
                    scaleWidth = maxSize.Width;
                    scaleHeight = (int)(currentSize.Height * ((float)maxSize.Width / currentSize.Width));
                }
                else
                {
                    scaleHeight = maxSize.Height;
                    scaleWidth = (int)(currentSize.Width * ((float)maxSize.Height / currentSize.Height));
                }
            }
            if (scalePercentage > 0.0f)
            {
                scaleWidth = (int)(scaleWidth * (scalePercentage / 100F));
                scaleHeight = (int)(scaleHeight * (scalePercentage / 100F));
            }
            return new Size(scaleWidth, scaleHeight);
        }

        #region Port From Gen Libs
        private static ImageFormat GetImageFormatFromString(string Format)
        {
            ImageFormat format = null;
            if (!String.IsNullOrEmpty(Format))
            {
                switch (Format.ToLower())
                {
                    case ".png":
                    case "png":
                    case "image/png":
                        format = ImageFormat.Png;
                        break;

                    case ".gif":
                    case ".giff":
                    case "gif":
                    case "image/gif":
                        format = ImageFormat.Gif;
                        break;

                    case ".jpg":
                    case ".jpeg":
                    case ".jpe":
                    case ".jfif":
                    case ".pjpeg":
                    case ".pjp":
                    case "jpg":
                    case "jpeg":
                    case "image/jpeg":
                    case "image/pjpeg":
                    case "image/pipeg":
                        format = ImageFormat.Jpeg;
                        break;

                    case ".tif":
                    case ".tiff":
                    case "tiff":
                    case "tif":
                    case "image/tiff":
                        format = ImageFormat.Tiff;
                        break;

                    case ".bmp":
                    case "bmp":
                    case "image/bmp":
                        format = ImageFormat.Bmp;
                        break;

                    case ".ico":
                    case "ico":
                    case "icon":
                    case "image/x-icon":
                        format = ImageFormat.Icon;
                        break;
                }
            }
            else
            {
                throw new ArgumentException("Format can not be null or Empty", Format);
            }
            return format;
        }

        private static string GetMimeType(System.Drawing.Imaging.ImageFormat format)
        {
            if (format.Equals(ImageFormat.Bmp))
            {
                return "image/bmp";
            }
            if (format.Equals(ImageFormat.Gif))
            {
                return "image/gif";
            }
            if (format.Equals(ImageFormat.Jpeg))
            {
                return "image/jpeg";
            }
            if (format.Equals(ImageFormat.Tiff))
            {
                return "image/tiff";
            }
            if (format.Equals(ImageFormat.Png))
            {
                return "image/png";
            }
            if (format.Equals(ImageFormat.Icon))
            {
                return "image/x-icon";
            }
            if (format.Equals(ImageFormat.Wmf))
            {
                return "windows/metafile";
            }

            return "";
        }
        private static string GetMimeType(Image image)
        {
            return GetMimeType(image.RawFormat);
        }
        private static ImageCodecInfo GetEncoder(string mimeType)
        {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageDecoders();
            for (int i = 0; i < encoders.Length; ++i)
            {
                if (encoders[i].MimeType == mimeType)
                {
                    return encoders[i];
                }
            }
            return null;
        }
        public static ImageCodecInfo GetEncoder(ImageFormat imageFormat)
        {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageDecoders();
            for (int i = 0; i < encoders.Length; ++i)
            {
                if (encoders[i].FormatID == imageFormat.Guid)
                {
                    return encoders[i];
                }
            }
            return null;
        }
        #endregion
    }
}
