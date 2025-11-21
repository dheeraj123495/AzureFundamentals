using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AzureBlobProject.Services
{
    public class ContainerService : IContainerService
    {
        private readonly BlobServiceClient _blobClient;
        public ContainerService(BlobServiceClient blobClient) { 
            _blobClient = blobClient;
        }
        public async Task CreateContainer(string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);
        }
        public async Task DeleteContainer(string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            await blobContainerClient.DeleteIfExistsAsync();
        }
        public async Task<List<string>> ListAllContainers()
        {
            List<string> containersName = new List<string>();

            await foreach(BlobContainerItem blobContainerItem in _blobClient.GetBlobContainersAsync())
            {
                containersName.Add(blobContainerItem.Name);
            }

            return containersName;
        }
        public async Task<List<string>> GetAllContainerAndBlobs()
        {
            List<string> containersAndBlobName = new List<string>();
            containersAndBlobName.Add("------Account Name : " + _blobClient.AccountName +"-------");

            containersAndBlobName.Add("-----------------------------------------------------------");

            await foreach (BlobContainerItem blobContainerItem in _blobClient.GetBlobContainersAsync())
            {
                containersAndBlobName.Add("-----"+blobContainerItem.Name);
                BlobContainerClient blobContainer = _blobClient.GetBlobContainerClient(blobContainerItem.Name);

                await foreach(BlobItem blobItem in blobContainer.GetBlobsAsync())
                {
                    // Get metadata
                    var blobClient = blobContainer.GetBlobClient(blobItem.Name);
                    BlobProperties blobProperties = await blobClient.GetPropertiesAsync();
                    string tempBlobToAdd = blobItem.Name;
                    if (blobProperties.Metadata.ContainsKey("title"))
                    {
                        tempBlobToAdd += "(" + blobProperties.Metadata["title"] + ")";
                    }

                    containersAndBlobName.Add("->>" + tempBlobToAdd);
                }
                containersAndBlobName.Add("-----------------------------------------------------------");
            }
            return containersAndBlobName;
        }
    }
}
