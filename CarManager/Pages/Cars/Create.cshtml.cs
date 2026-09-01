using CarManager.Data;
using CarManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarManager.Pages.Cars;

public class CreateModel(CarDbContext db) : PageModel
{
    [BindProperty]
    public Car Car { get; set; } = new() { Year = DateTime.Now.Year };

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        db.Cars.Add(Car);
        await db.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
