// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

using Microsoft.EntityFrameworkCore;
using ProjectHub.Models;

namespace ProjectHub.Data;

/// <summary>EF Core database context. One SQLite file holds every table.</summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskNote> TaskNotes => Set<TaskNote>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<TaskItem>()
            .HasOne(t => t.Project).WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<TaskNote>()
            .HasOne(n => n.TaskItem).WithMany(t => t.Notes)
            .HasForeignKey(n => n.TaskItemId).OnDelete(DeleteBehavior.Cascade);

        // Task <-> assignees (many-to-many).
        b.Entity<TaskItem>().HasMany(t => t.Assignees).WithMany(u => u.Tasks);

        // Task -> depends-on tasks (many-to-many, self-referencing).
        b.Entity<TaskItem>()
            .HasMany(t => t.DependsOn).WithMany(t => t.Dependents)
            .UsingEntity(j => j.ToTable("TaskDependencies"));
    }
}
