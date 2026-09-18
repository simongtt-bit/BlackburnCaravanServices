using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BlackburnCaravanServices.Services;

public record CaravanImageUploadResult(
    string BlobName,
    string ImageUrl
);

public class CaravanImageStorageService(
    BlobServiceClient blobServiceClient,
    IConfiguration configuration
)
{
    private readonly BlobContainerClient _container =
        blobServiceClient.GetBlobContainerClient(
            configuration["AzureStorage:ContainerName"] ?? "caravan-images"
        );

    public async Task<CaravanImageUploadResult> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default
    )
    {
        var blobName = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var blob = _container.GetBlobClient(blobName);

        await blob.UploadAsync(
            stream,
            new BlobHttpHeaders { ContentType = contentType },
            cancellationToken: cancellationToken
        );

        return new CaravanImageUploadResult(
            blobName,
            blob.Uri.ToString()
        );
    }

    public async Task DeleteAsync(
        string blobName,
        CancellationToken cancellationToken = default
    )
    {
        await _container.DeleteBlobIfExistsAsync(
            blobName,
            cancellationToken: cancellationToken
        );
    }
}