using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using TangyAzureFunction.Data;

namespace TangyAzureFunction;

public class ResizeImageOnblobUpload
{
    private readonly ILogger<ResizeImageOnblobUpload> _logger;
    public ResizeImageOnblobUpload(ILogger<ResizeImageOnblobUpload> logger)
    {
        _logger = logger;
    }

    [Function(nameof(ResizeImageOnblobUpload))]
    [BlobOutput("functionsalesrep-final/{name}")]
    public async Task<Byte[]> Run([BlobTrigger("functionsalesrep/{name}", Connection = "AzureWebJobsStorage")] Byte[] myBlobByte, string name)
    {
        // Resize the image to 100x100 pixels
        using var menoryStream = new MemoryStream(myBlobByte);
        // Load the image
        using var image = Image.Load(menoryStream);
        // Resize the image
        image.Mutate(x => x.Resize(100, 100));
        // Save the resized image to a new memory stream
        using var outputStream = new MemoryStream();
        image.SaveAsJpeg(outputStream);
        // Reset the position of the output stream
        outputStream.Position = 0;

        _logger.LogInformation("C# Blob trigger function Processed blob\n Name: {name}", name);

        return outputStream.ToArray();
    }
}