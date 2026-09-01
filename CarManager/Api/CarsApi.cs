using System.ComponentModel.DataAnnotations;
using CarManager.Data;
using CarManager.Models;
using Microsoft.EntityFrameworkCore;

namespace CarManager.Api;

public static class CarsApi
{
    public static RouteGroupBuilder MapCarApi(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/cars").WithTags("Cars");

        group.MapGet("/", async (CarDbContext db) =>
        {
            var cars = await db.Cars
                .OrderBy(car => car.Make)
                .ThenBy(car => car.Model)
                .ToListAsync();

            return Results.Ok(cars);
        });

        group.MapGet("/{id:int}", async (int id, CarDbContext db) =>
        {
            var car = await db.Cars.FindAsync(id);
            return car is null ? Results.NotFound() : Results.Ok(car);
        });

        group.MapPost("/", async (Car car, CarDbContext db) =>
        {
            car.Id = 0;

            var errors = Validate(car);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            db.Cars.Add(car);
            await db.SaveChangesAsync();

            return Results.Created($"/api/cars/{car.Id}", car);
        });

        group.MapPut("/{id:int}", async (int id, Car updatedCar, CarDbContext db) =>
        {
            var car = await db.Cars.FindAsync(id);
            if (car is null)
            {
                return Results.NotFound();
            }

            car.Make = updatedCar.Make;
            car.Model = updatedCar.Model;
            car.Year = updatedCar.Year;
            car.Price = updatedCar.Price;

            var errors = Validate(car);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            await db.SaveChangesAsync();

            return Results.Ok(car);
        });

        group.MapDelete("/{id:int}", async (int id, CarDbContext db) =>
        {
            var car = await db.Cars.FindAsync(id);
            if (car is null)
            {
                return Results.NotFound();
            }

            db.Cars.Remove(car);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        return group;
    }

    private static Dictionary<string, string[]> Validate(Car car)
    {
        var context = new ValidationContext(car);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(car, context, results, validateAllProperties: true);

        return results
            .SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty)
                .Select(memberName => new { memberName, result.ErrorMessage }))
            .GroupBy(item => item.memberName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(item => item.ErrorMessage ?? "Invalid value.")
                    .ToArray()
            );
    }
}
