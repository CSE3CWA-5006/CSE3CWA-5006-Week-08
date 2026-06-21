// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

using ProjectHub.Models;

namespace ProjectHub.Data;

/// <summary>Fills an empty database with realistic demo data: a 12-person team
/// (with titles and organizations) and eight commercial-style projects whose
/// tasks have one or more assignees, may depend on several other tasks, and are
/// spread across statuses (a few overdue). At most three projects are delayed,
/// and every project has at least one completed task.</summary>
public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Projects.Any() || db.Users.Any()) return;
        var today = DateTime.Today;

        var users = new[]
        {
            new AppUser { Name = "Wei Chen",        Email = "wei.chen@example.com",        Title = "Project Lead",         Organization = "La Trobe University", Role = MemberRole.Owner,  Initials = "WC", ColorHex = "#6264A7" },
            new AppUser { Name = "Grace Liu",       Email = "grace.liu@example.com",       Title = "UX Designer",          Organization = "La Trobe University", Role = MemberRole.Member, Initials = "GL", ColorHex = "#0F6CBD" },
            new AppUser { Name = "Daniel Smith",    Email = "daniel.smith@example.com",    Title = "Software Engineer",    Organization = "Acme Corp",           Role = MemberRole.Member, Initials = "DS", ColorHex = "#107C10" },
            new AppUser { Name = "Emily Johnson",   Email = "emily.johnson@example.com",   Title = "Data Analyst",         Organization = "Acme Corp",           Role = MemberRole.Member, Initials = "EJ", ColorHex = "#D83B01" },
            new AppUser { Name = "Kevin Wang",      Email = "kevin.wang@example.com",      Title = "Backend Engineer",     Organization = "La Trobe University", Role = MemberRole.Member, Initials = "KW", ColorHex = "#8764B8" },
            new AppUser { Name = "Olivia Brown",    Email = "olivia.brown@example.com",    Title = "Marketing Specialist", Organization = "BrightMedia",         Role = MemberRole.Guest,  Initials = "OB", ColorHex = "#C239B3" },
            new AppUser { Name = "James Wilson",    Email = "james.wilson@example.com",    Title = "DevOps Engineer",      Organization = "Acme Corp",           Role = MemberRole.Member, Initials = "JW", ColorHex = "#008272" },
            new AppUser { Name = "Sophia Martinez", Email = "sophia.martinez@example.com", Title = "Product Manager",      Organization = "Northwind Traders",   Role = MemberRole.Member, Initials = "SM", ColorHex = "#5C2E91" },
            new AppUser { Name = "Liam Nguyen",     Email = "liam.nguyen@example.com",     Title = "Frontend Engineer",    Organization = "La Trobe University", Role = MemberRole.Member, Initials = "LN", ColorHex = "#C19C00" },
            new AppUser { Name = "Ava Patel",       Email = "ava.patel@example.com",       Title = "QA Engineer",          Organization = "Acme Corp",           Role = MemberRole.Member, Initials = "AP", ColorHex = "#B146C2" },
            new AppUser { Name = "Noah Kim",        Email = "noah.kim@example.com",        Title = "Solutions Architect",  Organization = "Northwind Traders",   Role = MemberRole.Member, Initials = "NK", ColorHex = "#0E7A0D" },
            new AppUser { Name = "Mia Garcia",      Email = "mia.garcia@example.com",      Title = "Content Strategist",   Organization = "BrightMedia",         Role = MemberRole.Guest,  Initials = "MG", ColorHex = "#CA5010" },
        };
        db.Users.AddRange(users);
        db.SaveChanges();

        var pWeb    = new Project { Name = "Website Redesign",    Icon = "🌐", ColorHex = "#6264A7", Description = "Rebuild the public marketing site with a new design system.", Status = ProjectStatus.InProgress,    StartDate = today.AddDays(-25), EndDate = today.AddDays(30) };
        var pBank   = new Project { Name = "Mobile Banking App",  Icon = "📱", ColorHex = "#0F6CBD", Description = "Ship v2.0 of the customer mobile banking application.",       Status = ProjectStatus.Delayed,    StartDate = today.AddDays(-20), EndDate = today.AddDays(40) };
        var pMkt    = new Project { Name = "Q3 Marketing Launch", Icon = "📣", ColorHex = "#C239B3", Description = "Plan and execute the Q3 product launch campaign.",            Status = ProjectStatus.InProgress,    StartDate = today.AddDays(-10), EndDate = today.AddDays(45) };
        var pCloud  = new Project { Name = "Cloud Migration",     Icon = "☁️", ColorHex = "#008272", Description = "Migrate on-prem services to the cloud with zero downtime.",   Status = ProjectStatus.Delayed,    StartDate = today.AddDays(-15), EndDate = today.AddDays(50) };
        var pCRM    = new Project { Name = "CRM Rollout",         Icon = "🧩", ColorHex = "#5C2E91", Description = "Implement and roll out the new customer-relationship platform.", Status = ProjectStatus.NotStarted, StartDate = today.AddDays(-3),  EndDate = today.AddDays(60) };
        var pDW     = new Project { Name = "Data Warehouse",      Icon = "🗄️", ColorHex = "#107C10", Description = "Stand up a central analytics warehouse and dashboards.",      Status = ProjectStatus.Completed,     StartDate = today.AddDays(-40), EndDate = today.AddDays(-2) };
        var pSec    = new Project { Name = "Security Audit",      Icon = "🔒", ColorHex = "#D83B01", Description = "End-to-end security review and penetration testing.",        Status = ProjectStatus.Delayed,    StartDate = today.AddDays(-12), EndDate = today.AddDays(20) };
        var pPortal = new Project { Name = "Customer Portal",     Icon = "💻", ColorHex = "#8764B8", Description = "Self-service customer portal with billing and SSO.",          Status = ProjectStatus.InProgress,    StartDate = today.AddDays(-8),  EndDate = today.AddDays(55) };
        db.Projects.AddRange(pWeb, pBank, pMkt, pCloud, pCRM, pDW, pSec, pPortal);
        db.SaveChanges();

        var order = 0;
        TaskItem T(Project p, string title, WorkStatus s, Priority pr,
                   int startOff, int dueOff, int[] who, int progress = 0, bool milestone = false)
        {
            var t = new TaskItem
            {
                ProjectId = p.Id, Title = title, Status = s, Priority = pr,
                Progress = milestone ? (s == WorkStatus.Completed ? 100 : 0) : progress,
                IsMilestone = milestone,
                StartDate = today.AddDays(startOff), DueDate = today.AddDays(dueOff),
                CompletedAt = s == WorkStatus.Completed ? today.AddDays(dueOff) : (DateTime?)null,
                SortOrder = order++,
            };
            foreach (var i in who) t.Assignees.Add(users[i]);
            return t;
        }

        var tasks = new List<TaskItem>
        {
            T(pWeb,  "Stakeholder interviews",     WorkStatus.Completed,  Priority.Medium, -25, -18, new[]{0},     100),
            T(pWeb,  "Information architecture",    WorkStatus.Completed,  Priority.High,   -18, -10, new[]{1,7},   100),
            T(pWeb,  "Wireframes & design system",  WorkStatus.InProgress, Priority.High,   -10,   4, new[]{1,8},    60),
            T(pWeb,  "Homepage build",              WorkStatus.InProgress, Priority.Urgent,  -3,  -1, new[]{8,2},    40),
            T(pWeb,  "Accessibility audit",         WorkStatus.NotStarted, Priority.High,     5,  14, new[]{9}),
            T(pWeb,  "Go-live",                     WorkStatus.NotStarted, Priority.Urgent,  28,  28, new[]{0,2},     0, true),

            T(pBank, "Define v2 scope",             WorkStatus.Completed,  Priority.High,   -20, -15, new[]{7,0},   100),
            T(pBank, "API contract",                WorkStatus.InProgress, Priority.High,   -12,  -2, new[]{2,4},    70),
            T(pBank, "Offline sync engine",         WorkStatus.Delayed,    Priority.Urgent,  -8,  -1, new[]{4,2},    45),
            T(pBank, "Biometric login",             WorkStatus.InProgress, Priority.High,    -2,   6, new[]{8,9},    30),
            T(pBank, "Beta release",                WorkStatus.NotStarted, Priority.High,    20,  20, new[]{3,0},     0, true),

            T(pMkt,  "Messaging & positioning",     WorkStatus.Completed,  Priority.High,   -10,  -4, new[]{5},     100),
            T(pMkt,  "Landing page copy",           WorkStatus.InProgress, Priority.Urgent,  -3,   1, new[]{5,11},   50),
            T(pMkt,  "Launch video",                WorkStatus.NotStarted, Priority.Medium,   4,  16, new[]{11}),
            T(pMkt,  "Email campaign",              WorkStatus.NotStarted, Priority.High,     8,  18, new[]{5}),
            T(pMkt,  "Launch day",                  WorkStatus.NotStarted, Priority.Urgent,  40,  40, new[]{5,1},     0, true),

            T(pCloud,"Cloud readiness assessment",  WorkStatus.Completed,  Priority.High,   -15,  -9, new[]{10,6},  100),
            T(pCloud,"Network architecture",        WorkStatus.InProgress, Priority.High,    -8,  -1, new[]{10,4},   55),
            T(pCloud,"Data migration",              WorkStatus.Delayed,    Priority.Urgent,  -5,  -2, new[]{3,6},    35),
            T(pCloud,"Security hardening",          WorkStatus.NotStarted, Priority.High,     3,  12, new[]{6}),
            T(pCloud,"Production cutover",          WorkStatus.NotStarted, Priority.Urgent,  30,  30, new[]{10,0},    0, true),

            T(pCRM,  "Vendor selection",            WorkStatus.Completed,  Priority.Medium,  -3,  -1, new[]{7,0},   100),
            T(pCRM,  "Requirements workshop",       WorkStatus.NotStarted, Priority.High,     2,   9, new[]{7,3}),
            T(pCRM,  "Data model design",           WorkStatus.NotStarted, Priority.High,     6,  16, new[]{4,10}),
            T(pCRM,  "Pilot rollout",               WorkStatus.NotStarted, Priority.Medium,  18,  30, new[]{2,9},     0, true),

            T(pDW,   "Source system inventory",     WorkStatus.Completed,  Priority.High,   -38, -30, new[]{3,10},  100),
            T(pDW,   "ETL pipeline design",         WorkStatus.Completed,  Priority.High,   -30, -22, new[]{10,4},  100),
            T(pDW,   "Dashboard build",             WorkStatus.Completed,  Priority.Medium, -22, -10, new[]{3},     100),
            T(pDW,   "Data quality checks",         WorkStatus.Completed,  Priority.High,   -12,  -3, new[]{4},     100),

            T(pSec,  "Scope & rules of engagement", WorkStatus.Completed,  Priority.Medium, -12,  -7, new[]{6,0},   100),
            T(pSec,  "Vulnerability scan",          WorkStatus.InProgress, Priority.Urgent,  -5,  -1, new[]{6,9},    60),
            T(pSec,  "Penetration testing",         WorkStatus.Delayed,    Priority.Urgent,  -3,  -2, new[]{6},      30),
            T(pSec,  "Remediation plan",            WorkStatus.NotStarted, Priority.High,     2,  12, new[]{6,0}),

            T(pPortal,"Portal requirements",        WorkStatus.Completed,  Priority.High,    -8,  -3, new[]{7,1},   100),
            T(pPortal,"Auth & SSO",                 WorkStatus.InProgress, Priority.High,    -2,   5, new[]{4,2},    40),
            T(pPortal,"Self-service UI",            WorkStatus.NotStarted, Priority.Medium,   4,  16, new[]{8,1}),
            T(pPortal,"Billing integration",        WorkStatus.NotStarted, Priority.High,     8,  20, new[]{2}),
            T(pPortal,"Portal launch",              WorkStatus.NotStarted, Priority.Urgent,  35,  35, new[]{7,0},     0, true),
        };
        db.Tasks.AddRange(tasks);
        db.SaveChanges();

        TaskItem? Find(string title) => tasks.FirstOrDefault(t => t.Title == title);
        void Dep(string task, params string[] deps)
        {
            var t = Find(task); if (t == null) return;
            foreach (var d in deps) { var x = Find(d); if (x != null) t.DependsOn.Add(x); }
        }
        Dep("Homepage build", "Wireframes & design system");
        Dep("Go-live", "Homepage build", "Accessibility audit");
        Dep("Offline sync engine", "API contract");
        Dep("Beta release", "Offline sync engine", "Biometric login");
        Dep("Email campaign", "Landing page copy");
        Dep("Launch day", "Launch video", "Email campaign");
        Dep("Data migration", "Network architecture");
        Dep("Production cutover", "Data migration", "Security hardening");
        Dep("Data model design", "Requirements workshop");
        Dep("Pilot rollout", "Data model design");
        Dep("Dashboard build", "ETL pipeline design");
        Dep("Penetration testing", "Vulnerability scan");
        Dep("Remediation plan", "Penetration testing");
        Dep("Self-service UI", "Auth & SSO");
        Dep("Portal launch", "Self-service UI", "Billing integration");

        var hp = Find("Homepage build");
        if (hp != null)
            db.TaskNotes.AddRange(
                new TaskNote { TaskItemId = hp.Id, Author = "Grace Liu", Body = "Hero section done; blocked on final logo from design.", CreatedAt = today.AddDays(-1).AddHours(9) },
                new TaskNote { TaskItemId = hp.Id, Author = "Wei Chen",  Body = "Logo delivered. Please wrap up by Friday.",            CreatedAt = today.AddHours(11) });

        db.SaveChanges();
    }
}
