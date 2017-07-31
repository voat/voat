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
using Voat.IO;

namespace Voat.Utilities
{
    //interface for dealing with writing files 

    public static class ThumbGenerator
    {
        public static async Task<string> GenerateAvatar(Stream imageStream, string fileName, string mimetype, bool purgeTempFile = true)
        {
            var fileManager = FileManager.Instance;
            if (fileManager.IsUploadPermitted(fileName, FileType.Avatar, mimetype, imageStream.Length))
            {

                var key = new FileKey();
                key.FileType = FileType.Avatar;
                key.ID = GenerateRandomFilename(Path.GetExtension(fileName), FileType.Avatar);

                await GenerateImageThumbnail(fileManager, key, imageStream, VoatSettings.Instance.AvatarSize);

                return key.ID;
            }

            return null;
        }
        // Generate a random filename for a thumbnail and make sure that the file does not exist.
        private static string GenerateRandomFilename(string extention, FileType fileType)
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
            } while (FileManager.Instance.Exists(new FileKey(fileName, fileType)));

            return fileName;
        }
        private static async Task GenerateImageThumbnail(FileManager fileManager, FileKey key, Stream stream, Size size)
        {
            using (var resizedStream = ImageHandler.Resize(stream, size))
            {
                await fileManager.Upload(key, resizedStream);
            }
        }
        public static async Task<string> GenerateThumbnail(string url, bool purgeTempFile = true)
        {
            return await GenerateThumbnail(new Uri(url), purgeTempFile);
        }
        public static async Task<string> GenerateThumbnail(Uri uri, bool purgeTempFile = true)
        {
            var url = uri.ToString();
            if (!String.IsNullOrEmpty(url) && UrlUtility.IsUriValid(url))
            {
                //Ok this all needs to be centralized, we should only make 1 request to a remote resource
                using (var httpResource = new HttpResource(url, new HttpResourceOptions() { AllowAutoRedirect = true }))
                {
                    await httpResource.GiddyUp();

                    if (httpResource.IsImage)
                    {
                        var fileManager = FileManager.Instance;
                        if (fileManager.IsUploadPermitted(url.ToString(), FileType.Thumbnail, null, httpResource.Stream.Length))
                        {
                            var key = new FileKey();
                            key.FileType = FileType.Thumbnail;
                            key.ID = GenerateRandomFilename(Path.GetExtension(url.ToString()), FileType.Thumbnail);
                            var stream = httpResource.Stream;

                            await GenerateImageThumbnail(fileManager, key, stream, VoatSettings.Instance.ThumbnailSize);

                            return key.ID;
                        }
                    }
                    else if (httpResource.Image != null)
                    {
                        //just do it. again.
                        return await GenerateThumbnail(httpResource.Image);
                    }
                }
            }
            return null;
        }
    }
}
