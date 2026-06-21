// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

namespace ProjectHub.Models;

/// <summary>
/// A team member: name, email, role, plus profile fields (job title and
/// organization/unit). Members are assigned to tasks (many-to-many); a project's
/// membership and a member's projects are both derived from those assignments.
/// </summary>
public class AppUser
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Title { get; set; } = "";          // e.g. "Data Analyst", "Software Engineer"
    public string Organization { get; set; } = "";   // e.g. "La Trobe University"
    public MemberRole Role { get; set; } = MemberRole.Member;
    public string Initials { get; set; } = "";
    public string ColorHex { get; set; } = "#6264A7";

    /// <summary>Tasks this member is assigned to (many-to-many).</summary>
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    public static string InitialsFrom(string name)
    {
        var parts = (name ?? "").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "?";
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
        return (parts[0].Substring(0, 1) + parts[^1].Substring(0, 1)).ToUpper();
    }

    public static string ColorFor(string name)
    {
        string[] palette = { "#6264A7", "#0F6CBD", "#107C10", "#D83B01", "#8764B8", "#C239B3", "#008272", "#5C2E91" };
        int h = 0; foreach (var c in name ?? "") h = (h * 31 + c) & 0x7fffffff;
        return palette[h % palette.Length];
    }
}
