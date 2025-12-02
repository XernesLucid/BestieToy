using BestieToy.Models;

namespace BestieToy.Data
{
    public interface IProductRepository
    {
        /// <summary>
        /// Lấy tất cả sản phẩm (có phân trang)
        /// </summary>
        Task<IEnumerable<Product>> GetAllProductsAsync(int page = 1, int pageSize = 12);

        /// <summary>
        /// Lấy sản phẩm theo ID
        /// </summary>
        Task<Product?> GetProductByIdAsync(string productId);

        /// <summary>
        /// Lấy sản phẩm theo danh mục
        /// </summary>
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(string categoryId, int page = 1, int pageSize = 12);

        /// <summary>
        /// Lấy sản phẩm theo loại thú cưng (Dog/Cat)
        /// </summary>
        Task<IEnumerable<Product>> GetProductsByPetTypeAsync(string petType, int page = 1, int pageSize = 12);

        /// <summary>
        /// Tìm kiếm sản phẩm theo tên/mô tả
        /// </summary>
        Task<IEnumerable<Product>> SearchProductsAsync(string keyword, int page = 1, int pageSize = 12);

        /// <summary>
        /// Lấy sản phẩm nổi bật (còn hàng, đang active)
        /// </summary>
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int limit = 8);

        /// <summary>
        /// Lấy sản phẩm mới nhất
        /// </summary>
        Task<IEnumerable<Product>> GetNewestProductsAsync(int limit = 8);

        /// <summary>
        /// Lấy sản phẩm liên quan (cùng danh mục)
        /// </summary>
        Task<IEnumerable<Product>> GetRelatedProductsAsync(string productId, int limit = 4);

        /// <summary>
        /// Lọc sản phẩm theo giá
        /// </summary>
        Task<IEnumerable<Product>> FilterProductsByPriceAsync(decimal minPrice, decimal maxPrice, int page = 1, int pageSize = 12);

        /// <summary>
        /// Thêm sản phẩm mới
        /// </summary>
        Task<bool> AddProductAsync(Product product);

        /// <summary>
        /// Cập nhật sản phẩm
        /// </summary>
        Task<bool> UpdateProductAsync(Product product);

        /// <summary>
        /// Xóa (ẩn) sản phẩm
        /// </summary>
        Task<bool> DeleteProductAsync(string productId);

        /// <summary>
        /// Cập nhật số lượng tồn kho
        /// </summary>
        Task<bool> UpdateStockQuantityAsync(string productId, int quantity);

        /// <summary>
        /// Đếm tổng số sản phẩm
        /// </summary>
        Task<int> CountProductsAsync();

        /// <summary>
        /// Đếm số sản phẩm theo danh mục
        /// </summary>
        Task<int> CountProductsByCategoryAsync(string categoryId);

        /// <summary>
        /// Lấy sản phẩm sắp hết hàng
        /// </summary>
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10);
    }
}