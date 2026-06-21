// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

namespace ProjectHub.Models;

/// <summary>A timestamped comment attached to a task.</summary>
public class TaskNote
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }

    public string Author { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
