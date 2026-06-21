// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectHub.Data;
using ProjectHub.Models;

namespace ProjectHub.Pages.Projects;

public class CreateModel : PageModel
{
    private readonly AppDbContext _db;
    public CreateModel(AppDbContext db) => _db = db;

    [BindProperty]
    public Project Project { get; set; } = new()
    {
        StartDate = DateTime.Today,
        EndDate = DateTime.Today.AddDays(30),
        ColorHex = "#6264A7"
    };

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (string.IsNullOrWhiteSpace(Project.Name))
            ModelState.AddModelError("Project.Name", "Please enter a project name.");
        if (!ModelState.IsValid) return Page();

        Project.CreatedAt = DateTime.Now;
        _db.Projects.Add(Project);
        _db.SaveChanges();
        // Step 2 of creation: add tasks one by one.
        return RedirectToPage("AddTask", new { projectId = Project.Id });
    }
}
