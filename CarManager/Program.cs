using CarManager.Api;
using CarManager.Data;
using CarManager.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<CarDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("CarDatabase")
        ?? "Data Source=cars.db";

    options.UseSqlite(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CarDbContext>();
    db.Database.EnsureCreated();

    if (!db.Cars.Any())
    {
        db.Cars.AddRange(
            new Car { Make = "Toyota", Model = "Corolla", Year = 2022, Price = 24500m },
            new Car { Make = "Tesla", Model = "Model 3", Year = 2024, Price = 62900m },
            new Car { Make = "Hyundai", Model = "Ioniq 5", Year = 2023, Price = 58500m }
        );

        db.SaveChanges();
    }
}

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapCarApi();

app.Run();
