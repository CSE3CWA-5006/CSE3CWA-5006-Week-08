// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

namespace ProjectHub.Models;

/// <summary>How urgent a task is. Higher value = more urgent.</summary>
public enum Priority { Low = 0, Medium = 1, High = 2, Urgent = 3 }

/// <summary>The single, unified status used for BOTH tasks and projects.</summary>
public enum WorkStatus { NotStarted = 0, InProgress = 1, Delayed = 2, Completed = 3 }

/// <summary>Health of a whole project — same four states as a task.</summary>
public enum ProjectStatus { NotStarted = 0, InProgress = 1, Delayed = 2, Completed = 3 }

/// <summary>Permission level of a team member (Teams-style).</summary>
public enum MemberRole { Owner = 0, Member = 1, Guest = 2 }
