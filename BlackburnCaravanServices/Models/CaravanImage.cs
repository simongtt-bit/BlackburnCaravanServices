namespace BlackburnCaravanServices.Models;

public class CaravanImage
{
    public int Id { get; set; }

    public int CaravanId { get; set; }

    public string BlobName { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public Caravan Caravan { get; set; } = null!;
}