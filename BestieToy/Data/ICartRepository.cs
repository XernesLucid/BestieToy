using BestieToy.Models;

namespace BestieToy.Data
{
    public interface ICartRepository
    {
        /// <summary>
        /// Lấy giỏ hàng của user
        /// </summary>
        Task<Cart?> GetCartByUserIdAsync(string userId);

        /// <summary>
        /// Tạo giỏ hàng mới cho user
        /// </summary>
        Task<bool> CreateCartAsync(string userId);

        /// <summary>
        /// Lấy tất cả items trong giỏ hàng
        /// </summary>
        Task<IEnumerable<CartItem>> GetCartItemsAsync(string cartId);

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng
        /// </summary>
        Task<bool> AddToCartAsync(string cartId, string productId, int quantity = 1);

        /// <summary>
        /// Cập nhật số lượng sản phẩm trong giỏ hàng
        /// </summary>
        Task<bool> UpdateCartItemQuantityAsync(string cartItemId, int quantity);

        /// <summary>
        /// Xóa sản phẩm khỏi giỏ hàng
        /// </summary>
        Task<bool> RemoveFromCartAsync(string cartItemId);

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        Task<bool> ClearCartAsync(string cartId);

        /// <summary>
        /// Đếm số sản phẩm trong giỏ hàng
        /// </summary>
        Task<int> CountCartItemsAsync(string cartId);

        /// <summary>
        /// Tính tổng tiền giỏ hàng
        /// </summary>
        Task<decimal> CalculateCartTotalAsync(string cartId);

        /// <summary>
        /// Kiểm tra sản phẩm đã có trong giỏ hàng chưa
        /// </summary>
        Task<bool> ItemExistsInCartAsync(string cartId, string productId);

        /// <summary>
        /// Lấy cart item bằng ID
        /// </summary>
        Task<CartItem?> GetCartItemByIdAsync(string cartItemId);
    }
}