using BlackburnCaravanServices.Data;
using BlackburnCaravanServices.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BlackburnCaravanServices.Pages.CaravansForSale;

public class CaravansForSaleModel(CaravanDbContext dbContext) : PageModel
{
    public IReadOnlyList<Caravan> Caravans { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Caravans = await dbContext.Caravans
            .AsNoTracking()
            .Include(x => x.Images)
            .Where(x => x.IsPublished)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}