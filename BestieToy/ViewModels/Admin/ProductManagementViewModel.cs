using BestieToy.Models;

namespace BestieToy.ViewModels.Admin
{
    public class ProductManagementViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalProducts { get; set; } = 0;
        public int PageSize { get; set; } = 20;

        // Filter
        public string? SearchKeyword { get; set; }
        public string? CategoryId { get; set; }
        public string? PetType { get; set; }
        public bool? IsActive { get; set; }

        // Stats
        public int TotalActiveProducts { get; set; }
        public int TotalOutOfStock { get; set; }
        public decimal TotalInventoryValue { get; set; }
    }
}