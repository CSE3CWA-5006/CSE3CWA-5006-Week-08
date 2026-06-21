// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectHub.Data;
using ProjectHub.Models;

namespace ProjectHub.Pages.Members;

public class CreateModel : PageModel
{
    private readonly AppDbContext _db;
    public CreateModel(AppDbContext db) => _db = db;

    [BindProperty] public AppUser Member { get; set; } = new();

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (string.IsNullOrWhiteSpace(Member.Name))
        {
            ModelState.AddModelError("Member.Name", "Please enter a name.");
            return Page();
        }
        Member.Initials = AppUser.InitialsFrom(Member.Name);
        if (string.IsNullOrWhiteSpace(Member.ColorHex) || Member.ColorHex == "#000000")
            Member.ColorHex = AppUser.ColorFor(Member.Name);
        _db.Users.Add(Member);
        _db.SaveChanges();
        return RedirectToPage("Index");
    }
}
