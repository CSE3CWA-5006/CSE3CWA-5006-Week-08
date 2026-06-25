# CarManager
A self-contained, offline web application built with
**ASP.NET Core 10 (Razor Pages + Minimal API)** and **SQLite**.
It ships with realistic demo data and needs no internet connection, no CDNs and no
external services.

> **Copyright (c) 2026 Dr Shuo Ding · <shuoding@outlook.com>**
> This software belongs to the author and is released under the
> **GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later)** — see [`LICENSE`](LICENSE).
> It is **free to use**. Any **copy, modification, or distribution** (including hosted/network
> use) **must retain the author copyright notice** and remain under the AGPL.

---

This Week 8 teaching project demonstrates an ASP.NET Core Razor Pages CRUD app with a local SQLite database and simple API endpoints.

## Run with Visual Studio

1. Open `CarManager.csproj` in Visual Studio Community.
2. Confirm that the `ASP.NET and web development` workload is installed.
3. Press the green Start button.
4. Open `/Cars` to use the Razor Pages interface.
5. Open `/api/cars` to view the same records as JSON.

## Run with the .NET CLI

```bash
dotnet restore
dotnet run --urls http://localhost:5088
```

Then open these URLs in your browser:

- `http://localhost:5088/` — the default home page.
- `http://localhost:5088/Cars` — the Razor Pages CRUD interface.
- `http://localhost:5088/api/cars` — the same car data as JSON.

The application creates `cars.db` automatically on first run.

## Key learning points

- `Models/Car.cs` defines the car data and validation rules.
- `Data/CarDbContext.cs` gives EF Core a database context.
- `Program.cs` registers Razor Pages, SQLite and the API endpoints.
- `Pages/Cars` contains the human-facing CRUD pages.
- `Api/CarsApi.cs` exposes JSON CRUD endpoints for API clients.
