using System.ComponentModel.DataAnnotations;
using BlackburnCaravanServices.Data;
using BlackburnCaravanServices.Models;
using BlackburnCaravanServices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlackburnCaravanServices.Pages.Admin.Caravans;

[Authorize]
public class CreateModel(
    CaravanDbContext dbContext,
    CaravanImageStorageService imageStorageService
) : PageModel
{
    private const long MaxImageSize = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedImageTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty]
    public List<IFormFile> Images { get; set; } = [];

    public class InputModel
    {
        [Required]
        [MaxLength(100)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Model { get; set; } = string.Empty;

        [Range(1900, 2100)]
        public int? Year { get; set; }

        [Range(0, 20)]
        public int? Berths { get; set; }

        [Range(0, 1000000)]
        public decimal Price { get; set; }

        public string Description { get; set; } = string.Empty;

        public bool IsPublished { get; set; }

        public bool IsSold { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        ValidateImages();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var caravan = new Caravan
        {
            Make = Input.Make.Trim(),
            Model = Input.Model.Trim(),
            Year = Input.Year,
            Berths = Input.Berths,
            Price = Input.Price,
            Description = Input.Description.Trim(),
            IsPublished = Input.IsPublished,
            IsSold = Input.IsSold,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Caravans.Add(caravan);

        var uploadedBlobNames = new List<string>();

        try
        {
            for (var index = 0; index < Images.Count; index++)
            {
                var image = Images[index];

                await using var stream = image.OpenReadStream();

                var upload = await imageStorageService.UploadAsync(
                    stream,
                    image.FileName,
                    image.ContentType,
                    cancellationToken
                );

                uploadedBlobNames.Add(upload.BlobName);

                caravan.Images.Add(
                    new CaravanImage
                    {
                        BlobName = upload.BlobName,
                        ImageUrl = upload.ImageUrl,
                        SortOrder = index
                    }
                );
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            foreach (var blobName in uploadedBlobNames)
            {
                try
                {
                    await imageStorageService.DeleteAsync(
                        blobName,
                        CancellationToken.None
                    );
                }
                catch
                {
                    // Preserve the original exception.
                }
            }

            throw;
        }

        return RedirectToPage("./Index");
    }

    private void ValidateImages()
    {
        foreach (var image in Images)
        {
            if (image.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(Images),
                    $"{image.FileName} is empty."
                );

                continue;
            }

            if (image.Length > MaxImageSize)
            {
                ModelState.AddModelError(
                    nameof(Images),
                    $"{image.FileName} is larger than 10 MB."
                );
            }

            if (!AllowedImageTypes.Contains(image.ContentType))
            {
                ModelState.AddModelError(
                    nameof(Images),
                    $"{image.FileName} is not a supported image type."
                );
            }
        }
    }
}