using CarManager.Data;
using CarManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarManager.Pages.Cars;

public class DeleteModel(CarDbContext db) : PageModel
{
    [BindProperty]
    public Car Car { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var car = await db.Cars.FindAsync(id);
        if (car is null)
        {
            return NotFound();
        }

        Car = car;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var car = await db.Cars.FindAsync(Car.Id);
        if (car is null)
        {
            return NotFound();
        }

        db.Cars.Remove(car);
        await db.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
