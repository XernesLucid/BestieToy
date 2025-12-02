using BestieToy.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BestieToy.Data
{
    public class ProductRepository : IProductRepository
    {
        private readonly DBContext _dbContext;

        public ProductRepository(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync(int page = 1, int pageSize = 12)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                SELECT p.*, c.CategoryName, c.PetType 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE p.IsActive = 1
                ORDER BY p.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var offset = (page - 1) * pageSize;
            var products = await connection.QueryAsync<Product>(sql, new
            {
                Offset = offset,
                PageSize = pageSize
            });

            return products;
        }

        public async Task<Product?> GetProductByIdAsync(string productId)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                SELECT p.*, c.CategoryName, c.PetType 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE p.ProductId = @ProductId";

            var product = await connection.QueryFirstOrDefaultAsync<Product>(sql, new { ProductId = productId });
            return product;
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string categoryId, int page = 1, int pageSize = 12)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                SELECT p.*, c.CategoryName, c.PetType 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE p.CategoryId = @CategoryId AND p.IsActive = 1
                ORDER BY p.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var offset = (page - 1) * pageSize;
            var products = await connection.QueryAsync<Product>(sql, new
            {
                CategoryId = categoryId,
                Offset = offset,
                PageSize = pageSize
            });

            return products;
        }

        public async Task<IEnumerable<Product>> GetProductsByPetTypeAsync(string petType, int page = 1, int pageSize = 12)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                SELECT p.*, c.CategoryName, c.PetType 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE c.PetType = @PetType AND p.IsActive = 1
                ORDER BY p.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var offset = (page - 1) * pageSize;
            var products = await connection.QueryAsync<Product>(sql, new
            {
                PetType = petType,
                Offset = offset,
                PageSize = pageSize
            });

            return products;
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string keyword, int page = 1, int pageSize = 12)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                SELECT p.*, c.CategoryName, c.PetType 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE (p.ProductName LIKE @Keyword OR p.Description LIKE @Keyword) 
                AND p.IsActive = 1
                ORDER BY p.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var offset = (page - 1) * pageSize;
            var products = await connection.QueryAsync<Product>(sql, new
            {
                Keyword = $"%{keyword}%",
                Offset = offset,
                PageSize = pageSize
            });

            return products;
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int limit = 8)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                SELECT TOP (@Limit) p.*, c.CategoryName, c.PetType 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE p.IsActive = 1 AND p.StockQuantity > 0
                ORDER BY p.CreatedAt DESC";

            var products = await connection.QueryAsync<Product>(sql, new { Limit = limit });
            return products;
        }

        public async Task<IEnumerable<Product>> GetNewestProductsAsync(int limit = 8)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                SELECT TOP (@Limit) p.*, c.CategoryName, c.PetType 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE p.IsActive = 1
                ORDER BY p.CreatedAt DESC";

            var products = await connection.QueryAsync<Product>(sql, new { Limit = limit });
            return products;
        }

        public async Task<IEnumerable<Product>> GetRelatedProductsAsync(string productId, int limit = 4)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                SELECT TOP (@Limit) p.*, c.CategoryName, c.PetType 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE p.CategoryId = (
                    SELECT CategoryId FROM Products WHERE ProductId = @ProductId
                ) 
                AND p.ProductId != @ProductId 
                AND p.IsActive = 1
                ORDER BY NEWID()"; // Random order for variety

            var products = await connection.QueryAsync<Product>(sql, new
            {
                ProductId = productId,
                Limit = limit
            });

            return products;
        }

        public async Task<IEnumerable<Product>> FilterProductsByPriceAsync(decimal minPrice, decimal maxPrice, int page = 1, int pageSize = 12)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                SELECT p.*, c.CategoryName, c.PetType 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE p.Price BETWEEN @MinPrice AND @MaxPrice 
                AND p.IsActive = 1
                ORDER BY p.Price
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var offset = (page - 1) * pageSize;
            var products = await connection.QueryAsync<Product>(sql, new
            {
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Offset = offset,
                PageSize = pageSize
            });

            return products;
        }

        public async Task<bool> AddProductAsync(Product product)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                INSERT INTO Products (ProductId, ProductName, Description, Price, StockQuantity, 
                                     CategoryId, ImageUrl, Color, Size, Material, IsActive, CreatedAt)
                VALUES (@ProductId, @ProductName, @Description, @Price, @StockQuantity, 
                        @CategoryId, @ImageUrl, @Color, @Size, @Material, @IsActive, @CreatedAt)";

            var affectedRows = await connection.ExecuteAsync(sql, product);
            return affectedRows > 0;
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                UPDATE Products 
                SET ProductName = @ProductName,
                    Description = @Description,
                    Price = @Price,
                    StockQuantity = @StockQuantity,
                    CategoryId = @CategoryId,
                    ImageUrl = @ImageUrl,
                    Color = @Color,
                    Size = @Size,
                    Material = @Material,
                    IsActive = @IsActive
                WHERE ProductId = @ProductId";

            var affectedRows = await connection.ExecuteAsync(sql, product);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteProductAsync(string productId)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "UPDATE Products SET IsActive = 0 WHERE ProductId = @ProductId";
            var affectedRows = await connection.ExecuteAsync(sql, new { ProductId = productId });
            return affectedRows > 0;
        }

        public async Task<bool> UpdateStockQuantityAsync(string productId, int quantity)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "UPDATE Products SET StockQuantity = @Quantity WHERE ProductId = @ProductId";
            var affectedRows = await connection.ExecuteAsync(sql, new
            {
                ProductId = productId,
                Quantity = quantity
            });
            return affectedRows > 0;
        }

        public async Task<int> CountProductsAsync()
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "SELECT COUNT(*) FROM Products WHERE IsActive = 1";
            var count = await connection.ExecuteScalarAsync<int>(sql);
            return count;
        }

        public async Task<int> CountProductsByCategoryAsync(string categoryId)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "SELECT COUNT(*) FROM Products WHERE CategoryId = @CategoryId AND IsActive = 1";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { CategoryId = categoryId });
            return count;
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                SELECT p.*, c.CategoryName, c.PetType 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE p.StockQuantity <= @Threshold AND p.IsActive = 1
                ORDER BY p.StockQuantity ASC";

            var products = await connection.QueryAsync<Product>(sql, new { Threshold = threshold });
            return products;
        }
    }
}