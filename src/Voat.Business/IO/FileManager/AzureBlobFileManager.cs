using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
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
            //var storageAccount = CloudStorageAccount.Parse(_connectionString);
            //var blobClient = storageAccount.CreateCloudBlobClient();
            //var containerReference = blobClient.GetContainerReference(GetContainerName(key));
            //var blockReference = containerReference.GetBlockBlobReference(key.ID);
            var blockBlob = await GetBlock(key, false);

            return await blockBlob.DeleteIfExistsAsync();
        }
        public override async Task<bool> Exists(FileKey key)
        {
            var blockBlob = await GetBlock(key, false);

            return await blockBlob.ExistsAsync();
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

            await blockBlob.UploadFromStreamAsync(stream);

        }
        private async Task<CloudBlockBlob> GetBlock(FileKey key, bool ensureCreated = true)
        {
            // Retrieve storage account information from connection string
            var storageAccount = CloudStorageAccount.Parse(_connectionString);

            // Create a blob client for interacting with the blob service.
            var blobClient = storageAccount.CreateCloudBlobClient();

            // Create a container for organizing blobs within the storage account.
            var container = blobClient.GetContainerReference(GetContainerName(key));

            if (ensureCreated)
            {
                await container.CreateIfNotExistsAsync();

                // allow public access to blobs in this container
                await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            }

            // Upload a BlockBlob to the newly created container, default mode: overwrite existing
            var blockBlob = container.GetBlockBlobReference(key.ID);

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
            //var path = "";
            //switch (type)
            //{
            //    case FileType.Badge:
            //        path = base.ContentPath(type);
            //        break;
            //    case FileType.Avatar:
            //        path = "avatars";
            //        break;
            //    case FileType.Thumbnail:
            //        path = "thumbs";
            //        break;
            //}
            //return path;
        }
    }
    /*
     * Original CDN Interaction code
     * 
        public static class CloudStorageUtility
        {
            // validate the connection string information
            private static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
            {
                CloudStorageAccount storageAccount;
                try
                {
                    storageAccount = CloudStorageAccount.Parse(storageConnectionString);
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                    throw;
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                    throw;
                }

                return storageAccount;
            }

            // check if a blob exists
            public static bool BlobExists(string blobName, string containerName)
            {
                CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                return blobClient.GetContainerReference(containerName).GetBlockBlobReference(blobName).Exists();
            }

            // upload a blob to storage, requires full path
            public static async Task UploadBlobToStorageAsync(string blobToUpload, string containerName)
            {
                // Retrieve storage account information from connection string
                CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(CloudConfigurationManager.GetSetting("StorageConnectionString"));

                // Create a blob client for interacting with the blob service.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Create a container for organizing blobs within the storage account.
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);
                try
                {
                    await container.CreateIfNotExistsAsync();
                }
                catch (StorageException)
                {
                    throw;
                }

                // allow public access to blobs in this container
                await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

                // Upload a BlockBlob to the newly created container, default mode: overwrite existing
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(Path.GetFileName(blobToUpload));
                await blockBlob.UploadFromFileAsync(blobToUpload, FileMode.Open);
            }

            // delete a blob from storage
            public static bool DeleteBlob(string blobName, string containerName)
            {
                CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                return blobClient.GetContainerReference(containerName).GetBlockBlobReference(blobName).DeleteIfExists();
            }
        }
    */
     
}
