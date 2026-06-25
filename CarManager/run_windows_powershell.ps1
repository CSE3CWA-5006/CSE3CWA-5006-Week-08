Write-Host "============================================================"
Write-Host "Week 8 CarManager ASP.NET Core demo"
Write-Host "============================================================"
Write-Host ""
Write-Host "This script runs the local Razor Pages + API + SQLite demo."
Write-Host "The first run creates cars.db automatically."
Write-Host ""

dotnet --version

Write-Host ""
Write-Host "Restoring packages..."
dotnet restore

Write-Host ""
Write-Host "Starting the application."
Write-Host "Open the URL shown in the terminal, then browse to /Cars."
Write-Host "For API testing, open /api/cars."
Write-Host "Press Ctrl + C to stop."
Write-Host "============================================================"
Write-Host ""

dotnet run
