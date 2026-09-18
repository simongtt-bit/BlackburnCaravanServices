namespace BlackburnCaravanServices.Models;

public class Caravan
{
    public int Id { get; set; }

    public string Make { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int? Year { get; set; }

    public decimal Price { get; set; }

    public int? Berths { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsSold { get; set; }

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<CaravanImage> Images { get; set; } = [];
}