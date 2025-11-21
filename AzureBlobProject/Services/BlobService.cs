using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using AzureBlobProject.Models;

namespace AzureBlobProject.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobClient;
        public BlobService(BlobServiceClient blobClient)
        {
            _blobClient = blobClient;
        }
        public async Task<bool> CreateBlob(string name, IFormFile file, string containerName, BlobModel blobModel)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainerClient.GetBlobClient(name);

            var httpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders()
            {
                ContentType = file.ContentType
            };

            IDictionary<string, string> metaData = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(blobModel.Title))
            {
                metaData.Add("title", blobModel.Title);
            }
            if (!string.IsNullOrEmpty(blobModel.Comments))
            {
                metaData.Add("comment", blobModel.Comments);
            }       
            var result = await blobClient.UploadAsync(file.OpenReadStream(), httpHeaders, metaData);

            //Below is used to delete metadata
            //IDictionary<string, string> deleteMetaData = new Dictionary<string, string>();
            //await blobClient.SetMetadataAsync(deleteMetaData);

            metaData.Remove("title");
            await blobClient.SetMetadataAsync(metaData);

            if (result != null)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteBlob(string name, string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainerClient.GetBlobClient(name);

            return await blobClient.DeleteIfExistsAsync();
        }

        public async Task<List<string>> GetAllBlobs(string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            var blobs = blobContainerClient.GetBlobsAsync();

            List<string> blobNames = new List<string>();

            await foreach(var blobItem in blobs)
            {
                blobNames.Add(blobItem.Name);
            }
            return blobNames; 
        }

        public async Task<List<BlobModel>> GetAllBlobWithUrl(string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            var blobs = blobContainerClient.GetBlobsAsync();

            List<BlobModel> blobList = new List<BlobModel>();

            //Start : Below is for adding the SAS security at container level

            //string sasContainerSignature = "";

            //if (blobContainerClient.CanGenerateSasUri)
            //{
            //    BlobSasBuilder blobSasBuilder = new BlobSasBuilder()
            //    {
            //        BlobContainerName = blobContainerClient.Name,
            //        Resource = "c",
            //        ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            //    };
            //    blobSasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

            //    sasContainerSignature = blobContainerClient.GenerateSasUri(blobSasBuilder).AbsoluteUri.Split('?')[1].ToString();
            //}

            //End : Below is for adding the SAS security at container level

            await foreach (var blobItem in blobs)
            {
                var blobClient = blobContainerClient.GetBlobClient(blobItem.Name);

                BlobModel blobModel = new BlobModel()
                {
                    //Title = blobItem.Metadata.ContainsKey("title") ? blobItem.Metadata["title"] : string.Empty,
                    //Comments = blobItem.Metadata.ContainsKey("comment") ? blobItem.Metadata["comment"] : string.Empty,
                    Uri = blobClient.Uri.AbsoluteUri //+ "?" + sasContainerSignature
                };

                // Start:  Below is for adding the SAS security at blob level

                //if (blobClient.CanGenerateSasUri)
                //{
                //    BlobSasBuilder blobSasBuilder = new BlobSasBuilder()
                //    {
                //        BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                //        BlobName = blobClient.Name,
                //        Resource = "b",
                //        ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                //    };
                //    blobSasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

                //    blobModel.Uri = blobClient.GenerateSasUri(blobSasBuilder).AbsoluteUri;
                //}

                // End: Below is for adding the SAS security at blob level

                BlobProperties blobProperties = await blobClient.GetPropertiesAsync();
                if(blobProperties.Metadata.ContainsKey("title"))
                {
                    blobModel.Title = blobProperties.Metadata["title"];
                }
                if (blobProperties.Metadata.ContainsKey("comment"))
                {
                    blobModel.Comments = blobProperties.Metadata["comment"];
                }
                blobList.Add(blobModel);
            }
            return blobList;
        }

        public async Task<string> GetBlob(string name, string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(name);

            if(blobClient != null)
            {
                return blobClient.Uri.AbsoluteUri.ToString();
            }
            return string.Empty;
        }
    }
}
