using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Common.Components;
using Voat.Common.Models;
using Voat.Configuration;
using Voat.Logging;
using Voat.Utilities;
using Voat.Utilities.Components;

namespace Voat.IO
{
    public class LocalNetworkFileManager : FileManager
    {
        protected virtual string ContentPath(FileKey key)
        {
            var path = "";
            switch (key.FileType)
            {
                case FileType.Avatar:
                    path = VoatSettings.Instance.DestinationPathAvatars;
                    break;
                case FileType.Badge:
                    path = "~/images/badges/";
                    break;
                case FileType.Thumbnail:
                    path = VoatSettings.Instance.DestinationPathThumbs;
                    break;
            }
            return path;
        }
        protected override string Domain { get => VoatSettings.Instance.SiteDomain; }

        protected void EnsureLocalDirectoryExists(FileKey key)
        {
            var dir = FilePather.Instance.LocalPath(ContentPath(key));
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        public override async Task<bool> Delete(FileKey key)
        {
            if (await Exists(key))
            {
                File.Delete(FilePather.Instance.LocalPath(ContentPath(key), key.ID));
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        public override Task<bool> Exists(FileKey key)
        {
            var exists = File.Exists(FilePather.Instance.LocalPath(ContentPath(key), key.ID));
            return Task.FromResult(exists);
        }

        public override string Uri(FileKey key, PathOptions options = null)
        {
            if (key == null || String.IsNullOrEmpty(key.ID))
            {
                return null;
            }

            return VoatUrlFormatter.BuildUrlPath(null, options, (new string[] { ContentPath(key), key.ID }).ToPathParts());
        }

        public override async Task Upload(FileKey key, Stream stream)
        {
            EnsureLocalDirectoryExists(key);

            string destinationFile = FilePather.Instance.LocalPath(ContentPath(key), key.ID);

            using (var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write))
            {
                await stream.CopyToAsync(destinationStream);
            }
        }
    }
}
