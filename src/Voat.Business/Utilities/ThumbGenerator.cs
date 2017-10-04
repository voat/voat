#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Configuration;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.IO;
using Voat.Utilities.Components;
using Voat.Imaging;
using Voat.Logging;
using Voat.Common.Models;

namespace Voat.Utilities
{
    //interface for dealing with writing files 

    public static class ThumbGenerator
    {
        public static async Task<CommandResponse<string>> GenerateAvatar(Stream imageStream, string fileName, string mimetype, bool purgeTempFile = true)
        {
            var fileManager = FileManager.Instance;
            var fileCheck = fileManager.IsUploadPermitted(fileName, FileType.Avatar, mimetype, imageStream.Length);

            if (fileCheck.Success)
            {
                var key = new FileKey();
                key.FileType = FileType.Avatar;
                key.ID = await GenerateRandomFilename(Path.GetExtension(fileName), FileType.Avatar);

                await GenerateImageThumbnail(fileManager, key, imageStream, VoatSettings.Instance.AvatarSize, false, false);

                return CommandResponse.Successful(key.ID);
            }

            return CommandResponse.FromStatus<string>(null, fileCheck.Status, fileCheck.Message);
        }
        // Generate a random filename for a thumbnail and make sure that the file does not exist.
        private static async Task<string> GenerateRandomFilename(string extention, FileType fileType)
        {
            string fileName = null;
            if (String.IsNullOrEmpty(extention))
            {
                throw new ArgumentException("A file extention must be provided", nameof(extention));
            }
            extention = extention.TrimSafe(".");
            do
            {
                fileName = $"{Guid.NewGuid().ToString()}.{extention.ToLower()}";
            } while (await FileManager.Instance.Exists(new FileKey(fileName, fileType)));

            return fileName;
        }
        private static async Task GenerateImageThumbnail(FileManager fileManager, FileKey key, Stream stream, Size size, bool forceJpegFormat = true, bool square = true)
        {
            using (var resizedStream = ImageHandler.Resize(stream, size, forceJpegFormat, square))
            {
                await fileManager.Upload(key, resizedStream);
            }
        }
        public static async Task<CommandResponse<string>> GenerateThumbnail(string url, bool purgeTempFile = true)
        {
            return await GenerateThumbnail(new Uri(url), purgeTempFile);
        }
        public static async Task<CommandResponse<string>> GenerateThumbnail(Uri uri, bool purgeTempFile = true)
        {
            if (VoatSettings.Instance.OutgoingTraffic.Enabled)
            {
                var url = uri.ToString();
                if (!String.IsNullOrEmpty(url) && UrlUtility.IsUriValid(url))
                {
                    try
                    {
                        //Ok this all needs to be centralized, we should only make 1 request to a remote resource
                        using (var httpResource = new HttpResource(
                            new Uri(url), 
                            new HttpResourceOptions() { AllowAutoRedirect = true }, 
                            VoatSettings.Instance.OutgoingTraffic.Proxy.ToWebProxy()))
                        {
                            var result = await httpResource.GiddyUp();

                            if (httpResource.IsImage)
                            {
                                var fileManager = FileManager.Instance;
                                var fileCheck = fileManager.IsUploadPermitted(url, FileType.Thumbnail, httpResource.Response.Content.Headers.ContentType.MediaType, httpResource.Stream.Length);

                                if (fileCheck.Success)
                                {
                                    var key = new FileKey();
                                    key.FileType = FileType.Thumbnail;
                                    key.ID = await GenerateRandomFilename(".jpg", FileType.Thumbnail);
                                    var stream = httpResource.Stream;

                                    await GenerateImageThumbnail(fileManager, key, stream, VoatSettings.Instance.ThumbnailSize, true);

                                    return CommandResponse.Successful(key.ID);
                                }

                            }
                            else if (httpResource.Image != null)
                            {
                                //just do it. again.
                                return await GenerateThumbnail(httpResource.Image);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.Instance.Log(ex, new { url = url, type = FileType.Thumbnail });
                        var response = CommandResponse.Error<CommandResponse<string>>(ex);
                        response.Response = ""; //Make sure this returns string.empty as other failures do.
                        return response;
                    }
                }
                EventLogger.Instance.Log(new LogInformation()
                {
                    Type = LogType.Debug,
                    Category = "Thumbnail Diag",
                    Message = "Default Response",
                    Data = new { url = url },
                    Origin = VoatSettings.Instance.Origin
                });
            }
            return CommandResponse.FromStatus<string>("", Status.Invalid);
        }
    }
}
