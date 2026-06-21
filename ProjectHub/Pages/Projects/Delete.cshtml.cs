// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Data;
using ProjectHub.Models;

namespace ProjectHub.Pages.Projects;

public class DeleteModel : PageModel
{
    private readonly AppDbContext _db;
    public DeleteModel(AppDbContext db) => _db = db;

    public Project? Project { get; set; }

    public IActionResult OnGet(int id)
    {
        Project = _db.Projects.Include(p => p.Tasks).FirstOrDefault(p => p.Id == id);
        if (Project == null) return RedirectToPage("Index");
        return Page();
    }

    public IActionResult OnPost(int id)
    {
        var project = _db.Projects.Find(id);
        if (project != null) { _db.Projects.Remove(project); _db.SaveChanges(); }
        return RedirectToPage("Index");
    }
}
