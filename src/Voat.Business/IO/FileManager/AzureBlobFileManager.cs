using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Common.Models;
using Voat.Configuration;
using Voat.Utilities;

namespace Voat.IO
{
    public class AzureBlobFileManager : LocalNetworkFileManager
    {
        private string _connectionString = null;
        private string _containerNameSuffix = null;

        public AzureBlobFileManager(string connectionString, string containerNameSuffix = "")
        {
            this._connectionString = connectionString;
            this._containerNameSuffix = String.IsNullOrEmpty(containerNameSuffix) ? "" : $"-{containerNameSuffix}";
        }

        protected override string Domain => VoatSettings.Instance.ContentDeliveryDomain;

        private string GetContainerName(FileKey key)
        {
            var name = "default";
            switch (key.FileType)
            {
                case FileType.Thumbnail:
                    name = "thumbs";
                    break;
                case FileType.Avatar:
                    name = "avatars";
                    break;
                case FileType.Badge:
                default:
                    name = key.FileType.ToString();
                    break;
            }
            //azure blobs are 3-63 chars, lower case, only alphanumeric and -
            return $"{name}{_containerNameSuffix}".SubstringMax(63).ToNormalized(Normalization.Lower);
        }

        public override async Task<bool> Delete(FileKey key)
        {
            var blockBlob = await GetBlock(key, false);

            return (await blockBlob.DeleteIfExistsAsync()).Value;
        }
        public override async Task<bool> Exists(FileKey key)
        {
            var blockBlob = await GetBlock(key, false);

            return (await blockBlob.ExistsAsync()).Value;
        }
        public override string Uri(FileKey key, PathOptions options = null)
        {
            if (key == null || String.IsNullOrEmpty(key.ID))
            {
                return null;
            }

            options = options == null ? new PathOptions() : options;
            options.ForceDomain = Domain;

            var result = "";
            switch (key.FileType)
            {
                case FileType.Badge:
                    options.ForceDomain = VoatSettings.Instance.SiteDomain;
                    result = base.Uri(key, options);
                    break;
                case FileType.Avatar:
                case FileType.Thumbnail:
                    return VoatUrlFormatter.BuildUrlPath(null, options, (new string[] { ContentPath(key), key.ID }).ToPathParts());
                    break;
            }
            return result;

        }
        public override async Task Upload(FileKey key, Stream stream)
        {
            var blockBlob = await GetBlock(key);

            // default mode: overwrite existing
            await blockBlob.UploadAsync(stream, overwrite: true);

        }
        private async Task<BlobClient> GetBlock(FileKey key, bool ensureCreated = true)
        {
            // Retrieve storage account information from connection string
            var blobServiceClient = new BlobServiceClient(_connectionString);

            // Create a container for organizing blobs within the storage account.
            var container = blobServiceClient.GetBlobContainerClient(GetContainerName(key));

            if (ensureCreated)
            {
                // create the container if it does not exist and allow public access to blobs in this container
                await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
            }

            var blockBlob = container.GetBlobClient(key.ID);

            return blockBlob;
        }
        protected override string ContentPath(FileKey key)
        {
            //container is part of path for azureblobs. We can have folders in a container but currenty we aren't using this
            //so we just return container-name
            if (key.FileType == FileType.Badge)
            {
                return base.ContentPath(key);
            }
            else
            {
                return GetContainerName(key);
            }
        }
    }
}
