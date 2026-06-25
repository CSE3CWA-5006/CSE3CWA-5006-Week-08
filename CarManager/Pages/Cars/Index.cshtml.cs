using CarManager.Data;
using CarManager.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarManager.Pages.Cars;

public class IndexModel(CarDbContext db) : PageModel
{
    public IList<Car> Cars { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Cars = await db.Cars
            .OrderBy(car => car.Make)
            .ThenBy(car => car.Model)
            .ToListAsync();
    }
}
