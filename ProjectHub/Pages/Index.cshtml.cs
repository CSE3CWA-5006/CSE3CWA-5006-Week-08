// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Data;
using ProjectHub.Models;

namespace ProjectHub.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<Project> Projects { get; private set; } = new();

    public void OnGet()
    {
        Projects = _db.Projects
            .Include(p => p.Tasks).ThenInclude(t => t.Assignees)
            .OrderBy(p => p.Name).ToList();
    }
}
