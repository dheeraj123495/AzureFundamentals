namespace AzureBlobProject.Services
{
    public interface IContainerService
    {
        Task<List<string>> GetAllContainerAndBlobs();
        Task<List<string>> ListAllContainers();
        Task CreateContainer(string containerName);
        Task DeleteContainer(string containerName);
    }
}
