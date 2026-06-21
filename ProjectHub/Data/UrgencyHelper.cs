// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

using ProjectHub.Models;

namespace ProjectHub.Data;

/// <summary>Deadline helpers and status text. Status is the single source of
/// truth; "overdue" / "due soon" are deadline signals layered on top.</summary>
public static class UrgencyHelper
{
    public static bool IsOverdue(TaskItem t) =>
        t.Status != WorkStatus.Completed && t.DueDate.Date < DateTime.Today;

    public static bool IsDueSoon(TaskItem t) =>
        t.Status != WorkStatus.Completed &&
        t.DueDate.Date >= DateTime.Today && t.DueDate.Date <= DateTime.Today.AddDays(3);

    public static string StatusText(WorkStatus s) => s switch
    {
        WorkStatus.NotStarted => "Not started",
        WorkStatus.InProgress => "In progress",
        WorkStatus.Delayed => "Delayed",
        _ => "Completed"
    };

    public static string DueClass(TaskItem t) => IsOverdue(t) ? "over" : IsDueSoon(t) ? "soon" : "";

    public static string DueText(TaskItem t) =>
        (IsOverdue(t) ? "Overdue · " : "Due ") + t.DueDate.ToString("MMM d");
}

/// <summary>The reason a task needs attention (a reason, not a work status).</summary>
public static class ReasonHelper
{
    public static string Text(TaskItem t)
    {
        if (UrgencyHelper.IsOverdue(t) || t.Status == WorkStatus.Delayed) return "Delayed";
        if (UrgencyHelper.IsDueSoon(t)) return "Due soon";
        return UrgencyHelper.StatusText(t.Status);
    }
    public static string Css(TaskItem t)
    {
        if (UrgencyHelper.IsOverdue(t) || t.Status == WorkStatus.Delayed) return "rs-over";
        if (UrgencyHelper.IsDueSoon(t)) return "rs-soon";
        return "st-" + (int)t.Status;
    }
}

/// <summary>Readable labels for the unified four-state status (projects + tasks).</summary>
public static class StatusLabels
{
    private static readonly string[] Text = { "Not started", "In progress", "Delayed", "Completed" };
    public static string Of(ProjectStatus s) => Text[(int)s];
    public static string Of(WorkStatus s) => Text[(int)s];
}
