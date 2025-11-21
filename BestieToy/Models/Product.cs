using BestieToy.Models;

public class Product
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; } // Có thể null
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public string? ImageUrl { get; set; } // Có thể null
    public string? Color { get; set; } // Có thể null
    public string? Size { get; set; } // Có thể null
    public string? Material { get; set; } // Có thể null
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public Category? Category { get; set; }
}