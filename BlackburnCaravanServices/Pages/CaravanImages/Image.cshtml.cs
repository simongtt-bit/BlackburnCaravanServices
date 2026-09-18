using Azure.Storage.Blobs;
using BlackburnCaravanServices.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BlackburnCaravanServices.Pages.CaravanImages;

public class ImageModel(
    CaravanDbContext dbContext,
    BlobServiceClient blobServiceClient,
    IConfiguration configuration
) : PageModel
{
    public async Task<IActionResult> OnGetAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var image = await dbContext.CaravanImages
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken
            );

        if (image is null)
        {
            return NotFound();
        }

        var containerName =
            configuration["AzureStorage:ContainerName"]
            ?? "caravan-images";

        var container = blobServiceClient
            .GetBlobContainerClient(containerName);

        var blob = container.GetBlobClient(image.BlobName);

        if (!await blob.ExistsAsync(cancellationToken))
        {
            return NotFound();
        }

        var download = await blob.DownloadContentAsync(
            cancellationToken
        );

        return File(
            download.Value.Content.ToArray(),
            download.Value.Details.ContentType
            ?? "application/octet-stream"
        );
    }
}