using System.ComponentModel.DataAnnotations;
using BlackburnCaravanServices.Data;
using BlackburnCaravanServices.Models;
using BlackburnCaravanServices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BlackburnCaravanServices.Pages.Admin.Caravans;

[Authorize]
public class EditModel(
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

    public IReadOnlyList<CaravanImage> ExistingImages { get; private set; } = [];

    public class InputModel
    {
        public int Id { get; set; }

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

    public async Task<IActionResult> OnGetAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var caravan = await dbContext.Caravans
            .AsNoTracking()
            .Include(x => x.Images)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (caravan is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = caravan.Id,
            Make = caravan.Make,
            Model = caravan.Model,
            Year = caravan.Year,
            Berths = caravan.Berths,
            Price = caravan.Price,
            Description = caravan.Description,
            IsPublished = caravan.IsPublished,
            IsSold = caravan.IsSold
        };

        ExistingImages = caravan.Images
            .OrderBy(x => x.SortOrder)
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        ValidateImages();

        if (!ModelState.IsValid)
        {
            await LoadExistingImagesAsync(
                Input.Id,
                cancellationToken
            );

            return Page();
        }

        var caravan = await dbContext.Caravans
            .Include(x => x.Images)
            .SingleOrDefaultAsync(
                x => x.Id == Input.Id,
                cancellationToken
            );

        if (caravan is null)
        {
            return NotFound();
        }

        caravan.Make = Input.Make.Trim();
        caravan.Model = Input.Model.Trim();
        caravan.Year = Input.Year;
        caravan.Berths = Input.Berths;
        caravan.Price = Input.Price;
        caravan.Description = Input.Description.Trim();
        caravan.IsPublished = Input.IsPublished;
        caravan.IsSold = Input.IsSold;

        var nextSortOrder = caravan.Images.Count == 0
            ? 0
            : caravan.Images.Max(x => x.SortOrder) + 1;

        foreach (var image in Images)
        {
            await using var stream = image.OpenReadStream();

            var upload = await imageStorageService.UploadAsync(
                stream,
                image.FileName,
                image.ContentType,
                cancellationToken
            );

            caravan.Images.Add(
                new CaravanImage
                {
                    BlobName = upload.BlobName,
                    ImageUrl = upload.ImageUrl,
                    SortOrder = nextSortOrder++
                }
            );
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToPage(
            "./Edit",
            new { id = caravan.Id }
        );
    }

    public async Task<IActionResult> OnPostDeleteImageAsync(
        int id,
        int imageId,
        CancellationToken cancellationToken)
    {
        var image = await dbContext.CaravanImages
            .SingleOrDefaultAsync(
                x => x.Id == imageId && x.CaravanId == id,
                cancellationToken
            );

        if (image is null)
        {
            return NotFound();
        }

        await imageStorageService.DeleteAsync(
            image.BlobName,
            cancellationToken
        );

        dbContext.CaravanImages.Remove(image);

        await dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToPage(
            "./Edit",
            new { id }
        );
    }

    private async Task LoadExistingImagesAsync(
        int caravanId,
        CancellationToken cancellationToken)
    {
        ExistingImages = await dbContext.CaravanImages
            .AsNoTracking()
            .Where(x => x.CaravanId == caravanId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);
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