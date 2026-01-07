using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using TangyAzureFunction.Data;
using TangyAzureFunction.Models;

namespace TangyAzureFunction;

public class BlobResizeUpdateDbStatus
{
    private readonly ILogger<BlobResizeUpdateDbStatus> _logger;
    private readonly ApplicationDbContext _dbContext;
    public BlobResizeUpdateDbStatus(ILogger<BlobResizeUpdateDbStatus> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [Function(nameof(BlobResizeUpdateDbStatus))]
    public async Task Run([BlobTrigger("functionsalesrep-final/{name}", Connection = "AzureWebJobsStorage")] Byte[] myBlobByte, string name)
    {
        var fileName = Path.GetFileNameWithoutExtension(name);

        SalesRequest? salesRequest = await _dbContext.SalesRequests.FirstOrDefaultAsync(x => x.Id == fileName);

        if(salesRequest != null)
        {
            salesRequest.Status = "Image Processed";
            await _dbContext.SaveChangesAsync();
        }
        _logger.LogInformation("BlobResize update DB status has been completed.");
    }
}