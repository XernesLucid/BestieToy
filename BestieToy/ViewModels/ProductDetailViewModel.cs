using BestieToy.Models;

namespace BestieToy.ViewModels
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = new Product();
        public Category? Category { get; set; }
        public IEnumerable<Product> RelatedProducts { get; set; } = new List<Product>();

        // Cart
        public int CartQuantity { get; set; } = 1;
        public bool InCart { get; set; } = false;

        // Reviews (có thể thêm sau)
        public double AverageRating { get; set; } = 4.5;
        public int ReviewCount { get; set; } = 0;
    }
}