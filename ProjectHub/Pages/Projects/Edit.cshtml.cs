// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectHub.Data;
using ProjectHub.Models;

namespace ProjectHub.Pages.Projects;

public class EditModel : PageModel
{
    private readonly AppDbContext _db;
    public EditModel(AppDbContext db) => _db = db;

    [BindProperty]
    public Project Project { get; set; } = new();

    public IActionResult OnGet(int id)
    {
        var project = _db.Projects.Find(id);
        if (project == null) return RedirectToPage("/Index");
        Project = project;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (string.IsNullOrWhiteSpace(Project.Name))
            ModelState.AddModelError("Project.Name", "Please enter a project name.");
        if (!ModelState.IsValid) return Page();

        var project = _db.Projects.Find(Project.Id);
        if (project == null) return RedirectToPage("/Index");

        project.Name = Project.Name;
        project.Description = Project.Description;
        project.ColorHex = Project.ColorHex;
        project.Status = Project.Status;
        project.StartDate = Project.StartDate;
        project.EndDate = Project.EndDate;
        _db.SaveChanges();

        return RedirectToPage("Details", new { id = project.Id });
    }
}
