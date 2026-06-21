// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

namespace ProjectHub.Models;

/// <summary>A single unit of work. A milestone is a task with IsMilestone = true.
/// A task can have several assignees and can depend on several other tasks.
/// Deleting a task archives it (IsArchived) so it can be recovered later.</summary>
public class TaskItem
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public string Title { get; set; } = "";
    public string Description { get; set; } = "";

    public WorkStatus Status { get; set; } = WorkStatus.NotStarted;
    public Priority Priority { get; set; } = Priority.Medium;
    public int Progress { get; set; }
    public bool IsMilestone { get; set; }
    public bool IsArchived { get; set; }

    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(3);
    public DateTime? CompletedAt { get; set; }

    public int SortOrder { get; set; }

    /// <summary>Tasks this one depends on (many-to-many, self-referencing).</summary>
    public ICollection<TaskItem> DependsOn { get; set; } = new List<TaskItem>();
    public ICollection<TaskItem> Dependents { get; set; } = new List<TaskItem>();

    /// <summary>People assigned to this task (many-to-many).</summary>
    public ICollection<AppUser> Assignees { get; set; } = new List<AppUser>();
    public ICollection<TaskNote> Notes { get; set; } = new List<TaskNote>();
}
