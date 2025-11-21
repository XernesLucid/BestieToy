using BestieToy.Models;
using Dapper;
using System.Data;

namespace BestieToy.Data
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(string userId);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> GetUsersByRoleAsync(string roleId);
    }

    public class UserRepository : IUserRepository
    {
        private readonly DBContext _context;

        public UserRepository(DBContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT u.*, r.RoleName, r.Description 
                FROM Users u 
                INNER JOIN Roles r ON u.RoleId = r.RoleId 
                WHERE u.Username = @Username AND u.IsActive = 1";

            var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT u.*, r.RoleName, r.Description 
                FROM Users u 
                INNER JOIN Roles r ON u.RoleId = r.RoleId 
                WHERE u.Email = @Email AND u.IsActive = 1";

            var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
            return user;
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT u.*, r.RoleName, r.Description 
                FROM Users u 
                INNER JOIN Roles r ON u.RoleId = r.RoleId 
                WHERE u.UserId = @UserId AND u.IsActive = 1";

            var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { UserId = userId });
            return user;
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                INSERT INTO Users (UserId, Username, Password, Email, FullName, Phone, Address, RoleId)
                VALUES (@UserId, @Username, @Password, @Email, @FullName, @Phone, @Address, @RoleId)";

            var result = await connection.ExecuteAsync(sql, user);
            return result > 0;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT u.*, r.RoleName, r.Description 
                FROM Users u 
                INNER JOIN Roles r ON u.RoleId = r.RoleId 
                WHERE u.IsActive = 1
                ORDER BY u.CreatedAt DESC";

            var users = await connection.QueryAsync<User>(sql);
            return users.ToList();
        }

        public async Task<List<User>> GetUsersByRoleAsync(string roleId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT u.*, r.RoleName, r.Description 
                FROM Users u 
                INNER JOIN Roles r ON u.RoleId = r.RoleId 
                WHERE u.RoleId = @RoleId AND u.IsActive = 1
                ORDER BY u.CreatedAt DESC";

            var users = await connection.QueryAsync<User>(sql, new { RoleId = roleId });
            return users.ToList();
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                UPDATE Users 
                SET Username = @Username, Email = @Email, FullName = @FullName, 
                    Phone = @Phone, Address = @Address, RoleId = @RoleId
                WHERE UserId = @UserId";

            var result = await connection.ExecuteAsync(sql, user);
            return result > 0;
        }
    }
}