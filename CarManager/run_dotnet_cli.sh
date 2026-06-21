#!/usr/bin/env bash
set -e

echo "============================================================"
echo "Week 8 CarManager ASP.NET Core demo"
echo "============================================================"
echo
echo "This script runs the local Razor Pages + API + SQLite demo."
echo "The first run creates cars.db automatically."
echo

dotnet --version

echo
echo "Restoring packages..."
dotnet restore

echo
echo "Starting the application."
echo "Open the URL shown in the terminal, then browse to /Cars."
echo "For API testing, open /api/cars."
echo "Press Ctrl + C to stop."
echo "============================================================"
echo

dotnet run
