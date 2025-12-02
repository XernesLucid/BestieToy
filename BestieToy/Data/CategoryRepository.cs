using BestieToy.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BestieToy.Data
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DBContext _dbContext;

        public CategoryRepository(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "SELECT * FROM Categories ORDER BY CategoryName";
            var categories = await connection.QueryAsync<Category>(sql);
            return categories;
        }

        public async Task<Category?> GetCategoryByIdAsync(string categoryId)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "SELECT * FROM Categories WHERE CategoryId = @CategoryId";
            var category = await connection.QueryFirstOrDefaultAsync<Category>(sql, new { CategoryId = categoryId });
            return category;
        }

        public async Task<IEnumerable<Category>> GetCategoriesByPetTypeAsync(string petType)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "SELECT * FROM Categories WHERE PetType = @PetType AND IsActive = 1 ORDER BY CategoryName";
            var categories = await connection.QueryAsync<Category>(sql, new { PetType = petType });
            return categories;
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "SELECT * FROM Categories WHERE IsActive = 1 ORDER BY CategoryName";
            var categories = await connection.QueryAsync<Category>(sql);
            return categories;
        }

        public async Task<bool> AddCategoryAsync(Category category)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                INSERT INTO Categories (CategoryId, CategoryName, Description, PetType, IsActive)
                VALUES (@CategoryId, @CategoryName, @Description, @PetType, @IsActive)";

            var affectedRows = await connection.ExecuteAsync(sql, category);
            return affectedRows > 0;
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                UPDATE Categories 
                SET CategoryName = @CategoryName,
                    Description = @Description,
                    PetType = @PetType,
                    IsActive = @IsActive
                WHERE CategoryId = @CategoryId";

            var affectedRows = await connection.ExecuteAsync(sql, category);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteCategoryAsync(string categoryId)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "UPDATE Categories SET IsActive = 0 WHERE CategoryId = @CategoryId";
            var affectedRows = await connection.ExecuteAsync(sql, new { CategoryId = categoryId });
            return affectedRows > 0;
        }

        public async Task<int> CountCategoriesAsync()
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "SELECT COUNT(*) FROM Categories WHERE IsActive = 1";
            var count = await connection.ExecuteScalarAsync<int>(sql);
            return count;
        }

        public async Task<int> CountProductsInCategoryAsync(string categoryId)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "SELECT COUNT(*) FROM Products WHERE CategoryId = @CategoryId AND IsActive = 1";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { CategoryId = categoryId });
            return count;
        }
    }
}