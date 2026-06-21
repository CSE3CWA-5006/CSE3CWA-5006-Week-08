// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectHub.Data;
using ProjectHub.Models;

namespace ProjectHub.Pages.Members;

public class EditModel : PageModel
{
    private readonly AppDbContext _db;
    public EditModel(AppDbContext db) => _db = db;

    [BindProperty] public AppUser Member { get; set; } = new();

    public IActionResult OnGet(int id)
    {
        var u = _db.Users.Find(id);
        if (u == null) return RedirectToPage("Index");
        Member = u;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (string.IsNullOrWhiteSpace(Member.Name))
        {
            ModelState.AddModelError("Member.Name", "Please enter a name.");
            return Page();
        }
        var u = _db.Users.Find(Member.Id);
        if (u == null) return RedirectToPage("Index");
        u.Name = Member.Name.Trim();
        u.Email = Member.Email;
        u.Title = Member.Title ?? "";
        u.Organization = Member.Organization ?? "";
        u.Role = Member.Role;
        u.ColorHex = string.IsNullOrWhiteSpace(Member.ColorHex) ? u.ColorHex : Member.ColorHex;
        u.Initials = AppUser.InitialsFrom(u.Name);
        _db.SaveChanges();
        return RedirectToPage("Index");
    }
}
