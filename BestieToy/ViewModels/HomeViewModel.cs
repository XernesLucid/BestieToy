using BestieToy.Models;

namespace BestieToy.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Product> FeaturedProducts { get; set; } = new List<Product>();
        public IEnumerable<Product> NewestProducts { get; set; } = new List<Product>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public bool IsLoggedIn { get; set; }
        public User? CurrentUser { get; set; }
    }
}