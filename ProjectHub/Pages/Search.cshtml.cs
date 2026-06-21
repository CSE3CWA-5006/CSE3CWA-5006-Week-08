// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Data;
using ProjectHub.Models;

namespace ProjectHub.Pages;

public class SearchModel : PageModel
{
    private readonly AppDbContext _db;
    public SearchModel(AppDbContext db) => _db = db;

    public string Q { get; private set; } = "";
    public List<Project> Projects { get; private set; } = new();
    public List<AppUser> Members { get; private set; } = new();

    public void OnGet(string? q)
    {
        Q = (q ?? "").Trim();
        if (Q.Length == 0) return;
        var needle = Q;
        bool Has(string? s) => !string.IsNullOrEmpty(s) && s.Contains(needle, StringComparison.OrdinalIgnoreCase);

        Projects = _db.Projects.Include(p => p.Tasks)
            .ToList()
            .Where(p => Has(p.Name) || Has(p.Description))
            .OrderBy(p => p.Name).ToList();

        Members = _db.Users.ToList()
            .Where(u => Has(u.Name) || Has(u.Title) || Has(u.Organization) || Has(u.Email))
            .OrderBy(u => u.Name).ToList();
    }
}
