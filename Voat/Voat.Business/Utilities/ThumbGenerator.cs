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
using System.Threading.Tasks;
using Voat.Common;
using Voat.IO;

namespace Voat.Utilities
{
    //interface for dealing with writing files 

    public static class ThumbGenerator
    {
        //CORE_PORT: Image handling has changed in core, commenting out method until we know what we are doing
        public static async Task<bool> GenerateAvatar(object inputImage, string userName, string mimetype, bool purgeTempFile = true)
        {
            throw new NotImplementedException("Core Port CacheHandler.ExpireItem");
        }
        /*
        // store uploaded avatar
        public static async Task<bool> GenerateAvatar(Image inputImage, string userName, string mimetype, bool purgeTempFile = true)
        {
            try
            {
                // store avatar locally
                var originalImage = new KalikoImage(inputImage);
                originalImage.Scale(new PadScaling(MaxWidth, MaxHeight)).SaveJpg(DestinationPathAvatars + '\\' + userName + ".jpg", 90);
                if (!VoatSettings.Instance.UseContentDeliveryNetwork)
                {
                    return true;
                }

                // call upload to storage since CDN is enabled in config
                string tempAvatarLocation = DestinationPathAvatars + '\\' + userName + ".jpg";

                // the avatar file was not found at expected path, abort
                if (!FileSystemUtility.FileExists(tempAvatarLocation, DestinationPathAvatars))
                {
                    return false;
                }
                else if (VoatSettings.Instance.UseContentDeliveryNetwork)
                {
                    // upload to CDN
                    await CloudStorageUtility.UploadBlobToStorageAsync(tempAvatarLocation, "avatars");
                    if (purgeTempFile)
                    {
                        // delete local file after uploading to CDN
                        File.Delete(tempAvatarLocation);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                EventLogger.Log(ex);
                return false;
            }
        }
        */
        // Generate a random filename for a thumbnail and make sure that the file does not exist.
        private static string GenerateRandomFilename(string extention)
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
            } while (FileManager.Instance.Exists(new FileKey(fileName, FileType.Thumbnail)));

            return fileName;
        }

        public static async Task<string> GenerateThumbnail(string url, bool purgeTempFile = true)
        {
            return await GenerateThumbnail(new Uri(url), purgeTempFile);
        }
        public static async Task<string> GenerateThumbnail(Uri url, bool purgeTempFile = true)
        {
            //Ok this all needs to be centralized, we should only make 1 request to a remote resource
            using (var httpResource = new HttpResource(url))
            {
                await httpResource.GiddyUp();

                if (httpResource.IsImage)
                {
                    var key = new FileKey();
                    key.FileType = FileType.Thumbnail;
                    key.ID = GenerateRandomFilename("jpg");

                    await FileManager.Instance.Upload(key, httpResource.Stream);

                    return key.ID;
                }
                else if (httpResource.Image != null)
                {
                    //just do it. again.
                    return await GenerateThumbnail(httpResource.Image);
                }
                return null;
            }
        }
    }
}
