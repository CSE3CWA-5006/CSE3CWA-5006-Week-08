# CarManager

CarManager is a small ASP.NET Core web application for managing a car inventory. It demonstrates Razor Pages, Minimal APIs, Entity Framework Core, model validation, and a local SQLite database in a single project.

Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>

Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later). See [LICENSE](LICENSE) for the full license terms.

## Features

- ASP.NET Core 10 / .NET 10
- Razor Pages user interface
- Minimal API CRUD endpoints
- Entity Framework Core 10
- SQLite local database
- Server-side model validation
- Automatic database creation and initial sample data
- No external database server required

## Requirements

### Visual Studio

For the easiest Windows development experience, install:

- Visual Studio Community 2026 or a compatible Visual Studio release with .NET 10 support
- **ASP.NET and web development** workload
- .NET 10 SDK

### Command line

If you do not use Visual Studio, install the .NET 10 SDK and verify it with:

bash
dotnet --version


The project targets `net10.0`.

## Installation

Clone or download the project, then enter the `CarManager` directory.

Restore the NuGet packages:

bash
dotnet restore


Build the application:

bash
dotnet build


A successful build should complete without errors.

## Running with Visual Studio Community 2026

1. Open `CarManager.csproj` in Visual Studio Community 2026.
2. Allow Visual Studio to restore the NuGet packages if prompted.
3. Select either the **https** or **http** launch profile.
4. Press **F5** to run with the debugger, or **Ctrl+F5** to run without the debugger.
5. Visual Studio should open the application in your default browser.

The development ports currently configured in `Properties/launchSettings.json` are:

- HTTPS: `https://localhost:7059`
- HTTP: `http://localhost:5088`

These are development configuration values, not ports required by CarManager. They can be changed in `Properties/launchSettings.json` if necessary.

If the browser reports a local HTTPS certificate warning, trust the .NET development certificate with:

bash
dotnet dev-certs https --trust


## Running from the Command Line

From the project directory, run:

bash
dotnet restore
dotnet run


By default, `dotnet run` uses the project's launch profile. You can also explicitly choose the HTTP profile:

bash
dotnet run --launch-profile http


Or specify your own port:

bash
dotnet run --urls http://localhost:5088


Stop the application with **Ctrl+C**.

## Using the Application

After the application starts, the main routes are:

| Route | Purpose |
| --- | --- |
| `/` | Home page |
| `/Cars` | Car inventory and Razor Pages CRUD interface |
| `/Cars/Create` | Add a car |
| `/api/cars` | JSON API for car records |

The application automatically creates `cars.db` in the project working directory when the database does not already exist. On a new database, it also inserts several sample car records.

## REST API

The application exposes the following endpoints:

| Method | Endpoint | Description |
| --- | --- | --- |
| `GET` | `/api/cars` | List all cars |
| `GET` | `/api/cars/{id}` | Get one car |
| `POST` | `/api/cars` | Create a car |
| `PUT` | `/api/cars/{id}` | Update a car |
| `DELETE` | `/api/cars/{id}` | Delete a car |

Example request body for `POST /api/cars`:


{
  "make": "Toyota",
  "model": "Corolla",
  "year": 2026,
  "price": 30000
}


Invalid values are rejected with an HTTP `400 Bad Request` validation response.

## Project Structure

text
CarManager/
|-- Api/
|   `-- CarsApi.cs
|-- Data/
|   `-- CarDbContext.cs
|-- Models/
|   `-- Car.cs
|-- Pages/
|   |-- Cars/
|   `-- Shared/
|-- Properties/
|   `-- launchSettings.json
|-- wwwroot/
|-- Program.cs
|-- CarManager.csproj
|-- appsettings.json
|-- README.md
`-- LICENSE


Important components:

- `Program.cs` configures dependency injection, Razor Pages, SQLite, database initialization, and API routes.
- `Models/Car.cs` defines the car entity and validation rules.
- `Data/CarDbContext.cs` defines the Entity Framework Core database context.
- `Pages/Cars/` contains the browser-based CRUD interface.
- `Api/CarsApi.cs` implements the JSON CRUD API.
- `appsettings.json` contains the SQLite connection string.

## Database

The default connection string is:

text
Data Source=cars.db


It is configured under `ConnectionStrings:CarDatabase` in `appsettings.json`.

To start with a fresh development database, stop the application first and then remove `cars.db`. The application will create a new database and seed the sample records the next time it starts.

Do not remove a database containing data you need to keep. Back it up first.

## Dependency Security

To check NuGet dependencies for known vulnerabilities, run:

bash
dotnet list package --vulnerable --include-transitive


The project explicitly uses a current `SQLitePCLRaw.bundle_e_sqlite3` package so that the SQLite native dependency is not resolved to the previously vulnerable `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 package.

To inspect available package updates, run:

bash
dotnet list package --outdated


After changing package versions, restore and rebuild the project before running it.

## Troubleshooting

### Port already in use

Change the development port in `Properties/launchSettings.json`, or run the project with another port:

bash
dotnet run --urls http://localhost:5090


### HTTPS development certificate problems

Run:

bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust


Then restart Visual Studio and the application.

### NuGet restore problems

Try:

bash
dotnet nuget locals all --clear
dotnet restore


Then rebuild the project.

## License

CarManager is free software licensed under the **GNU Affero General Public License, version 3 or any later version (AGPL-3.0-or-later)**.

Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>

The AGPL permits use, study, modification, and redistribution under its terms. In particular, if you modify the program and make that modified version available for users to interact with over a network, the AGPL requires that those users be offered the corresponding source code as specified by the license.

See [LICENSE](LICENSE) for the complete legal text.
