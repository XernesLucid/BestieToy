using BestieToy.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BestieToy.Data
{
    public class CartRepository : ICartRepository
    {
        private readonly DBContext _dbContext;

        public CartRepository(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Cart?> GetCartByUserIdAsync(string userId)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "SELECT * FROM Carts WHERE UserId = @UserId";
            var cart = await connection.QueryFirstOrDefaultAsync<Cart>(sql, new { UserId = userId });
            return cart;
        }

        public async Task<bool> CreateCartAsync(string userId)
        {
            using var connection = _dbContext.CreateConnection();

            var cartId = $"CART{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

            var sql = @"
                INSERT INTO Carts (CartId, UserId, CreatedAt)
                VALUES (@CartId, @UserId, GETDATE())";

            var affectedRows = await connection.ExecuteAsync(sql, new
            {
                CartId = cartId,
                UserId = userId
            });

            return affectedRows > 0;
        }

        public async Task<IEnumerable<CartItem>> GetCartItemsAsync(string cartId)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                SELECT ci.*, p.ProductName, p.Price, p.ImageUrl, p.StockQuantity
                FROM CartItems ci
                LEFT JOIN Products p ON ci.ProductId = p.ProductId
                WHERE ci.CartId = @CartId
                ORDER BY ci.AddedAt DESC";

            var cartItems = await connection.QueryAsync<CartItem>(sql, new { CartId = cartId });
            return cartItems;
        }

        public async Task<bool> AddToCartAsync(string cartId, string productId, int quantity = 1)
        {
            using var connection = _dbContext.CreateConnection();

            // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
            var checkSql = "SELECT CartItemId FROM CartItems WHERE CartId = @CartId AND ProductId = @ProductId";
            var existingItemId = await connection.QueryFirstOrDefaultAsync<string>(checkSql, new
            {
                CartId = cartId,
                ProductId = productId
            });

            if (existingItemId != null)
            {
                // Nếu đã có, cập nhật số lượng
                var updateSql = @"
                    UPDATE CartItems 
                    SET Quantity = Quantity + @Quantity
                    WHERE CartItemId = @CartItemId";

                var affectedRows = await connection.ExecuteAsync(updateSql, new
                {
                    CartItemId = existingItemId,
                    Quantity = quantity
                });

                return affectedRows > 0;
            }
            else
            {
                // Nếu chưa có, thêm mới
                var cartItemId = $"CARTI{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

                var insertSql = @"
                    INSERT INTO CartItems (CartItemId, CartId, ProductId, Quantity, AddedAt)
                    VALUES (@CartItemId, @CartId, @ProductId, @Quantity, GETDATE())";

                var affectedRows = await connection.ExecuteAsync(insertSql, new
                {
                    CartItemId = cartItemId,
                    CartId = cartId,
                    ProductId = productId,
                    Quantity = quantity
                });

                return affectedRows > 0;
            }
        }

        public async Task<bool> UpdateCartItemQuantityAsync(string cartItemId, int quantity)
        {
            using var connection = _dbContext.CreateConnection();

            if (quantity <= 0)
            {
                // Nếu quantity <= 0, xóa item
                return await RemoveFromCartAsync(cartItemId);
            }

            var sql = "UPDATE CartItems SET Quantity = @Quantity WHERE CartItemId = @CartItemId";
            var affectedRows = await connection.ExecuteAsync(sql, new
            {
                CartItemId = cartItemId,
                Quantity = quantity
            });

            return affectedRows > 0;
        }

        public async Task<bool> RemoveFromCartAsync(string cartItemId)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "DELETE FROM CartItems WHERE CartItemId = @CartItemId";
            var affectedRows = await connection.ExecuteAsync(sql, new { CartItemId = cartItemId });
            return affectedRows > 0;
        }

        public async Task<bool> ClearCartAsync(string cartId)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "DELETE FROM CartItems WHERE CartId = @CartId";
            var affectedRows = await connection.ExecuteAsync(sql, new { CartId = cartId });
            return affectedRows > 0;
        }

        public async Task<int> CountCartItemsAsync(string cartId)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "SELECT SUM(Quantity) FROM CartItems WHERE CartId = @CartId";
            var count = await connection.ExecuteScalarAsync<int?>(sql, new { CartId = cartId });
            return count ?? 0;
        }

        public async Task<decimal> CalculateCartTotalAsync(string cartId)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                SELECT SUM(ci.Quantity * p.Price)
                FROM CartItems ci
                LEFT JOIN Products p ON ci.ProductId = p.ProductId
                WHERE ci.CartId = @CartId";

            var total = await connection.ExecuteScalarAsync<decimal?>(sql, new { CartId = cartId });
            return total ?? 0;
        }

        public async Task<bool> ItemExistsInCartAsync(string cartId, string productId)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "SELECT COUNT(1) FROM CartItems WHERE CartId = @CartId AND ProductId = @ProductId";
            var count = await connection.ExecuteScalarAsync<int>(sql, new
            {
                CartId = cartId,
                ProductId = productId
            });

            return count > 0;
        }

        public async Task<CartItem?> GetCartItemByIdAsync(string cartItemId)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                SELECT ci.*, p.ProductName, p.Price, p.ImageUrl
                FROM CartItems ci
                LEFT JOIN Products p ON ci.ProductId = p.ProductId
                WHERE ci.CartItemId = @CartItemId";

            var cartItem = await connection.QueryFirstOrDefaultAsync<CartItem>(sql, new { CartItemId = cartItemId });
            return cartItem;
        }
    }
}