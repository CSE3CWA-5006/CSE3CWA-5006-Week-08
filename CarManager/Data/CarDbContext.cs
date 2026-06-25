using CarManager.Models;
using Microsoft.EntityFrameworkCore;

namespace CarManager.Data;

public class CarDbContext(DbContextOptions<CarDbContext> options) : DbContext(options)
{
    public DbSet<Car> Cars => Set<Car>();
}
