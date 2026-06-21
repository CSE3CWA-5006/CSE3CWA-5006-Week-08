# ProjectHub

A self-contained, offline **project & task management** web application built with
**ASP.NET Core 8 (Razor Pages)** and **SQLite**, styled after Microsoft Teams / Fluent UI.
It ships with realistic demo data and needs no internet connection, no CDNs and no
external services.

> **Copyright (c) 2026 Dr Shuo Ding · <shuoding@outlook.com>**
> This software belongs to the author and is released under the
> **GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later)** — see [`LICENSE`](LICENSE).
> It is **free to use**. Any **copy, modification, or distribution** (including hosted/network
> use) **must retain the author copyright notice** and remain under the AGPL.

---

## Features

- **Overview dashboard** — projects grouped by the four unified statuses
  (Not started / In progress / Delayed / Completed); hover a card to reveal and jump to
  the projects it counts; an *Attention needed* panel lists delayed projects.
- **Projects** — create, edit, delete (delete lives in a separate *danger zone*).
- **Project workspace** — three synchronized views over one live data set:
  - **Board** with drag-and-drop between columns (status / assignee / priority),
  - **Gantt** chart with Day / Week / Month zoom, faint date gridlines and draggable bars,
  - **List** with sortable columns.
- **Tasks** — multiple assignees, multiple dependencies, priority, progress, milestones,
  notes, and a recoverable **Archive** (deleting a task moves it to the Archive; you can
  recover it or empty the archive permanently).
- **Team** — 12 seeded members with job title and organization; click a row to expand a
  profile and the projects that person works on (derived from task assignments).
- **Search** — find projects (by name/description) and people (by name/title/organization).
- Custom CSS (glassmorphism), vanilla JavaScript, no front-end frameworks.

## Tech stack

| Layer       | Technology                                        |
|-------------|---------------------------------------------------|
| Runtime     | .NET 8 / ASP.NET Core                             |
| UI          | Razor Pages + custom CSS + vanilla JS             |
| Data        | Entity Framework Core 8 + SQLite                  |
| Packaging   | Single project, one NuGet dependency (EF SQLite)  |

## Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) or newer
  (check with `dotnet --version`).

## Run it

```bash
# from the folder that contains ProjectHub.csproj
dotnet run
```

Then open the URL printed in the console (for example `http://localhost:5000`).
On first run the app creates `projecthub.db` (SQLite) via `EnsureCreated()` and fills it
with demo data.

> **Re-seeding / schema changes:** `EnsureCreated()` does **not** migrate an existing
> database. If you change the entity model (or pull a new version that does), stop the app,
> delete `projecthub.db`, and run `dotnet run` again to rebuild the schema and demo data.

## Project structure

```
ProjectHub/
├─ Program.cs               # Hosting, DI, EnsureCreated + seeding
├─ ProjectHub.csproj        # net8.0 + EF Core SQLite
├─ appsettings.json         # SQLite connection string
├─ Models/                  # Entities + enums (Project, TaskItem, AppUser, TaskNote)
├─ Data/                    # AppDbContext, DbSeeder, helpers
├─ Pages/                   # Razor Pages (Home, Projects, Members, Search) + shared layout
└─ wwwroot/                 # css/site.css and js/ph-core.js, ph-board.js, ph-gantt.js
```

## License

GNU Affero General Public License v3.0 or later — see [`LICENSE`](LICENSE).

You may use, study, run and modify this software for free, including for commercial and
educational purposes. **If you copy, modify, distribute, or run a modified version over a
network, you must keep the author's copyright notice (Dr Shuo Ding, <shuoding@outlook.com>),
release your changes under the same AGPL license, and make the corresponding source
available.** Every source file in this project carries an AGPL copyright header; please do
not remove it.
