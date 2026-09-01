using System.ComponentModel.DataAnnotations;

namespace CarManager.Models;

public class Car
{
    public int Id { get; set; }

    [Required]
    [StringLength(60)]
    public string Make { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Model { get; set; } = string.Empty;

    [Range(1886, 2100)]
    public int Year { get; set; }

    [Range(0, 10000000)]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }
}
