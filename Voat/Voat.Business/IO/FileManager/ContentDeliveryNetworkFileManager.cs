using System;
using System.IO;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Configuration;
using Voat.Utilities;

namespace Voat.IO
{
    public class ContentDeliveryNetworkFileManager : LocalNetworkFileManager
    {
        private string _connectionString = null;

        public ContentDeliveryNetworkFileManager(string connectionString)
        {
            this._connectionString = connectionString;
        }

        protected override string Domain => VoatSettings.Instance.ContentDeliveryDomain;

        public override void Delete(FileKey key)
        {
            throw new NotImplementedException();
        }
        public override bool Exists(FileKey key)
        {
            throw new NotImplementedException();
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
                    result = base.Uri(key, options);
                    break;
                case FileType.Avatar:
                case FileType.Thumbnail:
                    return VoatUrlFormatter.BuildUrlPath(null, options, (new string[] { ContentPath(key.FileType), key.ID }).ToPathParts());
                    break;
            }
            return result;
            
        }
        public override async Task Upload(FileKey key, Uri contentPath, HttpResourceOptions options = null, Func<Stream, Task<Stream>> preProcessor = null)
        {
            throw new NotImplementedException();
            //await base.Upload(key, contentPath);
        }
        public override Task Upload(FileKey key, Stream stream)
        {
            throw new NotImplementedException();
            return base.Upload(key, stream);
        }
        protected override string ContentPath(FileType type)
        {
            var path = "";
            switch (type)
            {
                case FileType.Badge:
                    path = base.ContentPath(type);
                    break;
                case FileType.Avatar:
                    path = "avatars";
                    break;
                case FileType.Thumbnail:
                    path = "thumbs";
                    break;
            }
            return path;
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
