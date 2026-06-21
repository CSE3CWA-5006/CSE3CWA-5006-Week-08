// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

namespace ProjectHub.Models;

/// <summary>A container for tasks, with its own icon. The project's members are
/// derived from the people assigned to its tasks (not managed directly).</summary>
public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "📁";
    public string ColorHex { get; set; } = "#6264A7";
    public ProjectStatus Status { get; set; } = ProjectStatus.NotStarted;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
