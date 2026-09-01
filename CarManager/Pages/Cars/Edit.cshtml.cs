using CarManager.Data;
using CarManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarManager.Pages.Cars;

public class EditModel(CarDbContext db) : PageModel
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
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var car = await db.Cars.FindAsync(Car.Id);
        if (car is null)
        {
            return NotFound();
        }

        car.Make = Car.Make;
        car.Model = Car.Model;
        car.Year = Car.Year;
        car.Price = Car.Price;

        await db.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
