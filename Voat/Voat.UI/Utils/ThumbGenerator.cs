using Kaliko.ImageLibrary;
using Kaliko.ImageLibrary.Scaling;
using OpenGraph_Net;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Message = Voat.Models.Message;

namespace Voat.Utils
{
    public static class ThumbGenerator
    {
        // public folder where thumbs should be saved
        private static readonly string DestinationPath = HttpContext.Current.Server.MapPath("~/Thumbs");

        // setup default thumb resolution
        private const int MaxHeight = 70;
        private const int MaxWidth = 70;

        // generate a thumbnail while removing transparency and preserving aspect ratio
        public static async Task<string> GenerateThumbFromUrl(string sourceUrl)
        {
            var randomFileName = GenerateRandomFilename();

            var request = WebRequest.Create(sourceUrl);
            request.Timeout = 300;
            var response = request.GetResponse();

            var originalImage = new KalikoImage(response.GetResponseStream()) { BackgroundColor = Color.Black };
            
            originalImage.Scale(new PadScaling(MaxWidth, MaxHeight)).SaveJpg(DestinationPath + '\\' + randomFileName + ".jpg", 90);
            
            // call upload to storage method if CDN config is enabled
            if (MvcApplication.UseContentDeliveryNetwork)
            {
                string tempThumbLocation = DestinationPath + '\\' + randomFileName + ".jpg";

                if (FileExists(tempThumbLocation))
                {
                    await UploadBlobToStorageAsync(tempThumbLocation);

                    // delete local file after uploading to CDN
                    File.Delete(tempThumbLocation);
                }
            }

            return randomFileName + ".jpg";
        }

        // TODO: implement CDN routing for avatars
        public static bool GenerateAvatar(Image inputImage, string userName, string mimetype)
        {
            try
            {
                string DestinationPath = HttpContext.Current.Server.MapPath("~/Storage/Avatars");

                var originalImage = new KalikoImage(inputImage);

                originalImage.Scale(new PadScaling(MaxWidth, MaxHeight)).SaveJpg(DestinationPath + '\\' + userName + ".jpg", 90);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // Generate a random filename for a thumbnail and make sure that the file does not exist.
        private static string GenerateRandomFilename()
        {
            string rndFileName;

            // if CDN flag is active, check if file exists on CDN, otherwise check if file exists on local storage
            if (MvcApplication.UseContentDeliveryNetwork)
            {
                // make sure blob with same name doesn't exist already
                do
                {
                    rndFileName = Guid.NewGuid().ToString();
                } while (BlobExists(rndFileName));

                rndFileName = Guid.NewGuid().ToString();
            }
            else
            {
                do
                {
                    rndFileName = Guid.NewGuid().ToString();
                } while (FileExists(rndFileName));
            }

            return rndFileName;
        }

        // Check if a file exists at given location.
        private static bool FileExists(string fileName)
        {
            var location = Path.Combine(DestinationPath, fileName);

            return (File.Exists(location));
        }

        public static async Task<string> ThumbnailFromSubmissionModel(Message submissionModel)
        {
            var extension = Path.GetExtension(submissionModel.MessageContent);

            // this is a direct link to image
            if (extension != String.Empty)
            {
                if (extension == ".jpg" || extension == ".JPG" || extension == ".png" || extension == ".PNG" || extension == ".gif" || extension == ".GIF")
                {
                    try
                    {
                        var thumbFileName = await GenerateThumbFromUrl(submissionModel.MessageContent);
                        return thumbFileName;
                    }
                    catch (Exception)
                    {
                        // thumnail generation failed, skip adding thumbnail
                        return null;
                    }
                }

                // try generating a thumbnail by using the Open Graph Protocol
                try
                {
                    var graphUri = new Uri(submissionModel.MessageContent);
                    var graph = OpenGraph.ParseUrl(graphUri, userAgent: "Voat.co OpenGraph Parser");

                    // open graph failed to find og:image element, abort thumbnail generation
                    if (graph.Image == null) return null;

                    var thumbFileName = await GenerateThumbFromUrl(graph.Image.ToString());
                    return thumbFileName;
                }
                catch (Exception)
                {
                    // thumnail generation failed, skip adding thumbnail
                    return null;
                }
            }

            // this is not a direct link to an image, it could be a link to an article or video
            // try generating a thumbnail by using the Open Graph Protocol
            try
            {
                var graphUri = new Uri(submissionModel.MessageContent);
                var graph = OpenGraph.ParseUrl(graphUri, userAgent: "Voat.co OpenGraph Parser");

                // open graph failed to find og:image element, abort thumbnail generation
                if (graph.Image == null) return null;

                var thumbFileName = await GenerateThumbFromUrl(graph.Image.ToString());
                return thumbFileName;
            }
            catch (Exception)
            {
                // thumnail generation failed, skip adding thumbnail
                return null;
            }
        }

        // upload a blob to storage, requires full path
        private static async Task UploadBlobToStorageAsync(string blobToUpload)
        {
            // Retrieve storage account information from connection string
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create a blob client for interacting with the blob service.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Create a container for organizing blobs within the storage account.
            CloudBlobContainer container = blobClient.GetContainerReference("thumbs");
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

            // Upload a BlockBlob to the newly created container
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(Path.GetFileName(blobToUpload));
            await blockBlob.UploadFromFileAsync(blobToUpload, FileMode.Open);
        }

        private static bool BlobExists(string blobName)
        {
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            return blobClient.GetContainerReference("thumbs").GetBlockBlobReference(blobName).Exists();  
        }

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
    }
}
