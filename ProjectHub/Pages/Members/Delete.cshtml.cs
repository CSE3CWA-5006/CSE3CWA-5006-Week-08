// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectHub.Data;
using ProjectHub.Models;

namespace ProjectHub.Pages.Members;

public class DeleteModel : PageModel
{
    private readonly AppDbContext _db;
    public DeleteModel(AppDbContext db) => _db = db;

    public AppUser? Member { get; set; }

    public IActionResult OnGet(int id)
    {
        Member = _db.Users.Find(id);
        if (Member == null) return RedirectToPage("Index");
        return Page();
    }

    public IActionResult OnPost(int id)
    {
        var u = _db.Users.Find(id);
        if (u != null) { _db.Users.Remove(u); _db.SaveChanges(); }   // assigned tasks become unassigned
        return RedirectToPage("Index");
    }
}
