using BestieToy.Models;

namespace BestieToy.Data
{
    public interface ICategoryRepository
    {
        /// <summary>
        /// Lấy tất cả danh mục
        /// </summary>
        Task<IEnumerable<Category>> GetAllCategoriesAsync();

        /// <summary>
        /// Lấy danh mục theo ID
        /// </summary>
        Task<Category?> GetCategoryByIdAsync(string categoryId);

        /// <summary>
        /// Lấy danh mục theo loại thú cưng
        /// </summary>
        Task<IEnumerable<Category>> GetCategoriesByPetTypeAsync(string petType);

        /// <summary>
        /// Lấy danh mục đang active
        /// </summary>
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();

        /// <summary>
        /// Thêm danh mục mới
        /// </summary>
        Task<bool> AddCategoryAsync(Category category);

        /// <summary>
        /// Cập nhật danh mục
        /// </summary>
        Task<bool> UpdateCategoryAsync(Category category);

        /// <summary>
        /// Xóa (ẩn) danh mục
        /// </summary>
        Task<bool> DeleteCategoryAsync(string categoryId);

        /// <summary>
        /// Đếm tổng số danh mục
        /// </summary>
        Task<int> CountCategoriesAsync();

        /// <summary>
        /// Đếm số sản phẩm trong danh mục
        /// </summary>
        Task<int> CountProductsInCategoryAsync(string categoryId);
    }
}