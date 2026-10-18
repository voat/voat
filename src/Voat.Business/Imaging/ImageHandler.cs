using System;
using System.IO;
using SkiaSharp;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Drawing.Size;

namespace Voat.Imaging
{
    public class ImageHandler
    {
        //This is a direct port for default thumbnails that Voat has used in the past
        //Modernization: implemented on SkiaSharp (System.Drawing.Common is Windows-only
        //from .NET 7 on). SkiaSharp is Apache-2.0 and builds without a license key;
        //ImageSharp 4.x (the security-fixed line) refuses Release builds without a
        //Six Labors license. Resize geometry and output quality (JPEG at 100) are
        //preserved.
        public static Stream Resize(Stream input, Size maximumSize, bool forceJpegFormat = false, bool square = true, bool scale = true, bool center = true)
        {
            //Buffer the source: the format sniff and the decode both read from the
            //start, and byte[] APIs avoid any stream-ownership questions.
            byte[] sourceBytes;
            using (var buffer = new MemoryStream())
            {
                input.CopyTo(buffer);
                sourceBytes = buffer.ToArray();
            }

            var sourceFormat = GetEncodedFormat(sourceBytes);

            using (var source = Decode(sourceBytes))
            {
                var destinationSize = new Size(maximumSize.Width, maximumSize.Height);
                //Sizing
                Size scaledSize = scale ? Scale(new Size(source.Width, source.Height), maximumSize) : maximumSize;

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

                using (var surface = SKSurface.Create(new SKImageInfo(destinationSize.Width, destinationSize.Height, SKColorType.Rgba8888, SKAlphaType.Premul)))
                {
                    var canvas = surface.Canvas;

                    //Assuming that if we are forcing jpeg we are creating site thumbnails which uses black bars
                    canvas.Clear(forceJpegFormat ? SKColors.Black : SKColors.Transparent);

                    //Scale the source to the computed size and place it at the
                    //destination offset in one draw (equivalent to the old
                    //resize-then-draw steps).
                    var destination = new SKRect(
                        destinationRectangle.X,
                        destinationRectangle.Y,
                        destinationRectangle.X + scaledSize.Width,
                        destinationRectangle.Y + scaledSize.Height);
                    canvas.DrawBitmap(source,
                        new SKRect(0, 0, source.Width, source.Height),
                        destination,
                        new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));

                    using (var image = surface.Snapshot())
                    {
                        var output = new MemoryStream();

                        //match legacy behavior: source format preserved, JPEG quality always 100
                        using (var data = (forceJpegFormat || sourceFormat == SKEncodedImageFormat.Jpeg)
                            ? image.Encode(SKEncodedImageFormat.Jpeg, 100)
                            : (sourceFormat != null
                                ? image.Encode(sourceFormat.Value, 100) ?? image.Encode(SKEncodedImageFormat.Png, 100)
                                : image.Encode(SKEncodedImageFormat.Png, 100)))
                        {
                            data.SaveTo(output);
                        }

                        //reset resized stream
                        output.Seek(0, SeekOrigin.Begin);
                        return output;
                    }
                }
            }
        }
        //SKBitmap.Decode throws ArgumentNullException (rather than returning
        //null) when the bytes are not a decodable image; normalize so callers
        //always get the InvalidDataException this method has long raised for
        //bad input.
        private static SKBitmap Decode(byte[] sourceBytes)
        {
            SKBitmap bitmap;
            try
            {
                bitmap = SKBitmap.Decode(sourceBytes);
            }
            catch (ArgumentNullException ex)
            {
                throw new InvalidDataException("Stream does not contain a supported image.", ex);
            }
            return bitmap ?? throw new InvalidDataException("Stream does not contain a supported image.");
        }
        private static SKEncodedImageFormat? GetEncodedFormat(byte[] sourceBytes)
        {
            using (var buffer = new MemoryStream(sourceBytes))
            using (var codec = SKCodec.Create(buffer))
            {
                return codec?.EncodedFormat;
            }
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
    }
}
