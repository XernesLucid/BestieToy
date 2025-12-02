// Trong file ViewModels/ProductListViewModel.cs (hoặc cùng file với các ViewModels)
namespace BestieToy.ViewModels
{
    public class ProductListViewModel
    {
        public IEnumerable<Models.Product> Products { get; set; } = new List<Models.Product>();
        public IEnumerable<Models.Category> Categories { get; set; } = new List<Models.Category>();

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalProducts { get; set; } = 0;
        public int PageSize { get; set; } = 12;

        // Filtering
        public string? SearchKeyword { get; set; }
        public string? CategoryId { get; set; }
        public string? PetType { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsActive { get; set; } // ✅ THÊM DÒNG NÀY

        // Sorting
        public string SortBy { get; set; } = "newest"; // newest, price_asc, price_desc, name

        // UI
        public string PageTitle { get; set; } = "Tất cả sản phẩm";
    }
}