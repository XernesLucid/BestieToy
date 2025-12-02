using BestieToy.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BestieToy.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DBContext _dbContext;

        public UserRepository(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                SELECT u.*, r.RoleName 
                FROM Users u
                LEFT JOIN Roles r ON u.RoleId = r.RoleId
                WHERE u.Username = @Username AND u.IsActive = 1";

            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "SELECT * FROM Users WHERE Email = @Email AND IsActive = 1";
            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
            return user;
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                SELECT u.*, r.RoleName 
                FROM Users u
                LEFT JOIN Roles r ON u.RoleId = r.RoleId
                WHERE u.UserId = @UserId";

            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                SELECT u.*, r.RoleName 
                FROM Users u
                LEFT JOIN Roles r ON u.RoleId = r.RoleId
                ORDER BY u.CreatedAt DESC";

            var users = await connection.QueryAsync<User>(sql);
            return users;
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleId)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                SELECT u.*, r.RoleName 
                FROM Users u
                LEFT JOIN Roles r ON u.RoleId = r.RoleId
                WHERE u.RoleId = @RoleId
                ORDER BY u.CreatedAt DESC";

            var users = await connection.QueryAsync<User>(sql, new { RoleId = roleId });
            return users;
        }

        public async Task<bool> AddUserAsync(User user)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                INSERT INTO Users (UserId, Username, Password, Email, FullName, Phone, Address, RoleId, IsActive, CreatedAt)
                VALUES (@UserId, @Username, @Password, @Email, @FullName, @Phone, @Address, @RoleId, @IsActive, @CreatedAt)";

            var affectedRows = await connection.ExecuteAsync(sql, user);
            return affectedRows > 0;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                UPDATE Users 
                SET Username = @Username, 
                    Email = @Email, 
                    FullName = @FullName, 
                    Phone = @Phone, 
                    Address = @Address, 
                    RoleId = @RoleId, 
                    IsActive = @IsActive
                WHERE UserId = @UserId";

            var affectedRows = await connection.ExecuteAsync(sql, user);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "UPDATE Users SET IsActive = 0 WHERE UserId = @UserId";
            var affectedRows = await connection.ExecuteAsync(sql, new { UserId = userId });
            return affectedRows > 0;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Username = username });
            return count > 0;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
            return count > 0;
        }

        public async Task<int> CountUsersAsync()
        {
            using var connection = _dbContext.CreateConnection();

            var sql = "SELECT COUNT(*) FROM Users WHERE IsActive = 1";
            var count = await connection.ExecuteScalarAsync<int>(sql);
            return count;
        }

        public async Task<Roles?> GetUserRoleAsync(string userId)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                SELECT r.* 
                FROM Roles r
                INNER JOIN Users u ON r.RoleId = u.RoleId
                WHERE u.UserId = @UserId";

            var role = await connection.QueryFirstOrDefaultAsync<Roles>(sql, new { UserId = userId });
            return role;
        }
    }
}