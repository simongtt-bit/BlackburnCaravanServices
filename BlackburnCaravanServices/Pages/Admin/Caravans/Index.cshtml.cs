using BlackburnCaravanServices.Data;
using BlackburnCaravanServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BlackburnCaravanServices.Pages.Admin.Caravans;

[Authorize]
public class IndexModel(CaravanDbContext dbContext) : PageModel
{
    public IReadOnlyList<Caravan> Caravans { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Caravans = await dbContext.Caravans
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}