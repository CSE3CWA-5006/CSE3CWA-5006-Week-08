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

public class AddTaskModel : PageModel
{
    private readonly AppDbContext _db;
    public AddTaskModel(AppDbContext db) => _db = db;

    public Project Project { get; private set; } = new();
    public List<AppUser> Users { get; private set; } = new();   // global team
    public List<TaskItem> Added { get; private set; } = new();
    public bool JustAdded { get; private set; }

    [BindProperty] public int ProjectId { get; set; }
    [BindProperty] public string Title { get; set; } = "";
    [BindProperty] public string? Description { get; set; }
    [BindProperty] public Priority Priority { get; set; } = Priority.Medium;
    [BindProperty] public List<int> AssigneeIds { get; set; } = new();
    [BindProperty] public DateTime StartDate { get; set; } = DateTime.Today;
    [BindProperty] public DateTime DueDate { get; set; } = DateTime.Today.AddDays(3);
    [BindProperty] public bool IsMilestone { get; set; }

    public IActionResult OnGet(int projectId, bool added = false)
    {
        if (!Load(projectId)) return RedirectToPage("Index");
        ProjectId = projectId; JustAdded = added;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!Load(ProjectId)) return RedirectToPage("Index");
        if (string.IsNullOrWhiteSpace(Title))
        {
            ModelState.AddModelError(nameof(Title), "Please enter a task title.");
            return Page();
        }

        var nextOrder = (_db.Tasks.Where(t => t.ProjectId == ProjectId).Max(t => (int?)t.SortOrder) ?? 0) + 1;
        var task = new TaskItem
        {
            ProjectId = ProjectId, Title = Title.Trim(), Description = Description ?? "",
            Priority = Priority, StartDate = StartDate.Date,
            DueDate = DueDate.Date < StartDate.Date ? StartDate.Date : DueDate.Date,
            IsMilestone = IsMilestone, Status = WorkStatus.NotStarted, SortOrder = nextOrder
        };
        foreach (var u in _db.Users.Where(u => AssigneeIds.Contains(u.Id))) task.Assignees.Add(u);
        _db.Tasks.Add(task);
        _db.SaveChanges();
        return RedirectToPage("AddTask", new { projectId = ProjectId, added = true });
    }

    private bool Load(int projectId)
    {
        var p = _db.Projects.FirstOrDefault(x => x.Id == projectId);
        if (p == null) return false;
        Project = p;
        Users = _db.Users.OrderBy(u => u.Name).ToList();
        Added = _db.Tasks.Include(t => t.Assignees).Where(t => t.ProjectId == projectId)
                         .OrderByDescending(t => t.Id).ToList();
        return true;
    }
}
