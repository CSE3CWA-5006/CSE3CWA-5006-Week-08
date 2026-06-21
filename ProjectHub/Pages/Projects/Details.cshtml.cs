// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Data;
using ProjectHub.Models;

namespace ProjectHub.Pages.Projects;

public class DetailsModel : PageModel
{
    private readonly AppDbContext _db;
    public DetailsModel(AppDbContext db) => _db = db;

    public Project Project { get; private set; } = new();
    public List<TaskItem> Tasks { get; private set; } = new();        // active (non-archived)
    public List<AppUser> Members { get; private set; } = new();       // derived from assignees
    public List<AppUser> AllUsers { get; private set; } = new();
    public List<TaskItem> OverdueTasks { get; private set; } = new();
    public int ArchivedCount { get; private set; }
    public int? OpenTaskId { get; private set; }
    public string BoardJson { get; private set; } = "[]";
    public string MembersJson { get; private set; } = "[]";

    public IActionResult OnGet(int id, int? task)
    {
        var project = _db.Projects
            .Include(p => p.Tasks).ThenInclude(t => t.Assignees)
            .Include(p => p.Tasks).ThenInclude(t => t.Notes)
            .Include(p => p.Tasks).ThenInclude(t => t.DependsOn)
            .FirstOrDefault(p => p.Id == id);
        if (project == null) return RedirectToPage("Index");

        Project = project;
        var active = project.Tasks.Where(t => !t.IsArchived).ToList();
        Tasks = active.OrderBy(t => t.SortOrder).ThenBy(t => t.Id).ToList();
        ArchivedCount = project.Tasks.Count(t => t.IsArchived);
        Members = active.SelectMany(t => t.Assignees).GroupBy(u => u.Id).Select(g => g.First())
                        .OrderBy(u => u.Name).ToList();
        AllUsers = _db.Users.OrderBy(u => u.Name).ToList();
        OverdueTasks = active.Where(UrgencyHelper.IsOverdue).OrderBy(t => t.DueDate).ToList();
        OpenTaskId = task;

        BoardJson = JsonSerializer.Serialize(Tasks.Select(Dto));
        MembersJson = JsonSerializer.Serialize(Members.Select(Mini));
        return Page();
    }

    private static object Mini(AppUser u) => new { id = u.Id, name = u.Name, initials = u.Initials, color = u.ColorHex };

    public IActionResult OnGetTask(int taskId)
    {
        var t = _db.Tasks.Include(x => x.Assignees).Include(x => x.DependsOn).Include(x => x.Notes).FirstOrDefault(x => x.Id == taskId);
        if (t == null) return NotFound();
        return new JsonResult(new
        {
            task = Dto(t),
            notes = t.Notes.OrderBy(n => n.CreatedAt).Select(n => new { author = n.Author, body = n.Body, createdAt = n.CreatedAt.ToString("MMM d, yyyy HH:mm") })
        });
    }

    public IActionResult OnGetArchived(int projectId)
    {
        var list = _db.Tasks.Include(x => x.Assignees).Include(x => x.DependsOn)
            .Where(t => t.ProjectId == projectId && t.IsArchived)
            .OrderByDescending(t => t.Id).ToList();
        return new JsonResult(new { tasks = list.Select(Dto) });
    }

    public IActionResult OnPostCreateTask(int projectId, string title, string? description,
        int status, int priority, int progress, bool isMilestone, string start, string due, string? assigneeIds, string? dependsOnIds)
    {
        if (string.IsNullOrWhiteSpace(title)) return BadRequest();
        var nextOrder = (_db.Tasks.Where(t => t.ProjectId == projectId).Max(t => (int?)t.SortOrder) ?? 0) + 1;
        var t = new TaskItem
        {
            ProjectId = projectId, Title = title.Trim(), Description = description ?? "",
            Status = (WorkStatus)status, Priority = (Priority)priority,
            Progress = Math.Clamp(progress, 0, 100), IsMilestone = isMilestone,
            StartDate = ParseDate(start, DateTime.Today), DueDate = ParseDate(due, DateTime.Today.AddDays(3)),
            SortOrder = nextOrder
        };
        if (t.DueDate < t.StartDate) t.DueDate = t.StartDate;
        if (t.Status == WorkStatus.Completed) { t.Progress = 100; t.CompletedAt = DateTime.Now; }
        SetAssignees(t, assigneeIds);
        _db.Tasks.Add(t); _db.SaveChanges();
        SetDepends(t.Id, dependsOnIds);
        return new JsonResult(new { ok = true, task = Dto(Reload(t.Id)) });
    }

    public IActionResult OnPostSaveTask(int taskId, string title, string? description,
        int status, int priority, int progress, bool isMilestone,
        string start, string due, string? assigneeIds, string? dependsOnIds)
    {
        var t = _db.Tasks.Include(x => x.Assignees).FirstOrDefault(x => x.Id == taskId);
        if (t == null) return NotFound();
        t.Title = string.IsNullOrWhiteSpace(title) ? t.Title : title.Trim();
        t.Description = description ?? "";
        t.Status = (WorkStatus)status;
        t.Priority = (Priority)priority;
        t.Progress = Math.Clamp(progress, 0, 100);
        t.IsMilestone = isMilestone;
        t.StartDate = ParseDate(start, t.StartDate);
        t.DueDate = ParseDate(due, t.DueDate);
        if (t.DueDate < t.StartDate) t.DueDate = t.StartDate;
        if (t.Status == WorkStatus.Completed) { t.Progress = 100; t.CompletedAt ??= DateTime.Now; }
        else t.CompletedAt = null;
        SetAssignees(t, assigneeIds);
        _db.SaveChanges();
        SetDepends(taskId, dependsOnIds);
        return new JsonResult(new { ok = true, task = Dto(Reload(taskId)) });
    }

    public IActionResult OnPostMove(int taskId, string group, string value, string ids)
    {
        var moved = _db.Tasks.Include(x => x.Assignees).FirstOrDefault(x => x.Id == taskId);
        if (moved == null) return NotFound();
        ApplyGroup(moved, group, value);
        var ordered = (ids ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out var n) ? n : 0).Where(n => n > 0).ToList();
        for (int i = 0; i < ordered.Count; i++) { var t = _db.Tasks.Find(ordered[i]); if (t != null) t.SortOrder = i; }
        _db.SaveChanges();
        return new JsonResult(new { ok = true, task = Dto(Reload(taskId)) });
    }

    private void ApplyGroup(TaskItem t, string group, string value)
    {
        switch (group)
        {
            case "assignee":
                t.Assignees.Clear();
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out var uid))
                { var u = _db.Users.Find(uid); if (u != null) t.Assignees.Add(u); }
                break;
            case "priority":
                if (int.TryParse(value, out var p)) t.Priority = (Priority)p;
                break;
            default:
                if (int.TryParse(value, out var s))
                {
                    var ns = (WorkStatus)s; t.Status = ns;
                    if (ns == WorkStatus.Completed) { t.Progress = 100; t.CompletedAt = DateTime.Now; }
                    else { t.CompletedAt = null; if (ns == WorkStatus.NotStarted) t.Progress = 0; }
                }
                break;
        }
    }

    private void SetAssignees(TaskItem t, string? csv)
    {
        t.Assignees.Clear();
        foreach (var u in _db.Users.Where(u => ParseIds(csv).Contains(u.Id))) t.Assignees.Add(u);
    }

    private void SetDepends(int taskId, string? csv)
    {
        var t = _db.Tasks.Include(x => x.DependsOn).First(x => x.Id == taskId);
        t.DependsOn.Clear();
        var ids = ParseIds(csv).Where(i => i != taskId).ToList();
        foreach (var d in _db.Tasks.Where(x => ids.Contains(x.Id))) t.DependsOn.Add(d);
        _db.SaveChanges();
    }

    private static List<int> ParseIds(string? csv) => (csv ?? "")
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(s => int.TryParse(s, out var n) ? n : 0).Where(n => n > 0).Distinct().ToList();

    public IActionResult OnPostUpdateDates(int taskId, string start, string due)
    {
        var t = _db.Tasks.Find(taskId);
        if (t == null) return NotFound();
        t.StartDate = ParseDate(start, t.StartDate);
        t.DueDate = ParseDate(due, t.DueDate);
        if (t.DueDate < t.StartDate) t.DueDate = t.StartDate;
        _db.SaveChanges();
        return new JsonResult(new { ok = true });
    }

    public IActionResult OnPostAddNote(int taskId, string author, string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return BadRequest();
        var note = new TaskNote { TaskItemId = taskId, Author = string.IsNullOrWhiteSpace(author) ? "You" : author.Trim(), Body = body.Trim(), CreatedAt = DateTime.Now };
        _db.TaskNotes.Add(note); _db.SaveChanges();
        return new JsonResult(new { ok = true, note = new { note.Author, note.Body, createdAt = note.CreatedAt.ToString("MMM d, yyyy HH:mm") } });
    }

    // ---- archive lifecycle ----
    public IActionResult OnPostArchiveTask(int taskId)
    {
        var t = _db.Tasks.Find(taskId);
        if (t != null) { t.IsArchived = true; _db.SaveChanges(); }
        return new JsonResult(new { ok = true });
    }
    public IActionResult OnPostRecoverTask(int taskId)
    {
        var t = _db.Tasks.Include(x => x.Assignees).Include(x => x.DependsOn).FirstOrDefault(x => x.Id == taskId);
        if (t == null) return NotFound();
        t.IsArchived = false; _db.SaveChanges();
        return new JsonResult(new { ok = true, task = Dto(t) });
    }
    public IActionResult OnPostPurgeTask(int taskId)
    {
        var t = _db.Tasks.Find(taskId);
        if (t != null && t.IsArchived) { _db.Tasks.Remove(t); _db.SaveChanges(); }
        return new JsonResult(new { ok = true });
    }
    public IActionResult OnPostEmptyArchive(int projectId)
    {
        var archived = _db.Tasks.Where(t => t.ProjectId == projectId && t.IsArchived).ToList();
        _db.Tasks.RemoveRange(archived); _db.SaveChanges();
        return new JsonResult(new { ok = true, removed = archived.Count });
    }

    private TaskItem Reload(int id) => _db.Tasks.Include(x => x.Assignees).Include(x => x.DependsOn).First(x => x.Id == id);
    private static DateTime ParseDate(string s, DateTime fallback) =>
        DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d.Date : fallback;

    private object Dto(TaskItem t)
    {
        var assignees = t.Assignees.OrderBy(u => u.Name)
            .Select(u => new { id = u.Id, name = u.Name, initials = u.Initials, color = u.ColorHex }).ToList();
        return new
        {
            id = t.Id, title = t.Title, description = t.Description,
            status = (int)t.Status, statusText = UrgencyHelper.StatusText(t.Status),
            priority = (int)t.Priority, priorityName = t.Priority.ToString(),
            progress = t.Progress, isMilestone = t.IsMilestone,
            start = t.StartDate.ToString("yyyy-MM-dd"), due = t.DueDate.ToString("yyyy-MM-dd"),
            dueText = UrgencyHelper.DueText(t), dueClass = UrgencyHelper.DueClass(t),
            assignees, assigneeIds = assignees.Select(a => a.id).ToArray(),
            dependsOnIds = t.DependsOn.Select(d => d.Id).ToArray(),
            notesCount = _db.TaskNotes.Count(n => n.TaskItemId == t.Id)
        };
    }
}
