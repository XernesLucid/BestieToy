using System.Data;
using BestieToy.Models;

namespace BestieToy.Data
{
    public interface IUserRepository
    {
        /// <summary>
        /// Lấy người dùng bằng Username
        /// </summary>
        Task<User?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Lấy người dùng bằng Email
        /// </summary>
        Task<User?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Lấy người dùng bằng UserId
        /// </summary>
        Task<User?> GetUserByIdAsync(string userId);

        /// <summary>
        /// Lấy tất cả người dùng
        /// </summary>
        Task<IEnumerable<User>> GetAllUsersAsync();

        /// <summary>
        /// Lấy người dùng theo RoleId
        /// </summary>
        Task<IEnumerable<User>> GetUsersByRoleAsync(string roleId);

        /// <summary>
        /// Thêm người dùng mới
        /// </summary>
        Task<bool> AddUserAsync(User user);

        /// <summary>
        /// Cập nhật thông tin người dùng
        /// </summary>
        Task<bool> UpdateUserAsync(User user);

        /// <summary>
        /// Xóa người dùng
        /// </summary>
        Task<bool> DeleteUserAsync(string userId);

        /// <summary>
        /// Kiểm tra username đã tồn tại chưa
        /// </summary>
        Task<bool> UsernameExistsAsync(string username);

        /// <summary>
        /// Kiểm tra email đã tồn tại chưa
        /// </summary>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>
        /// Đếm tổng số người dùng
        /// </summary>
        Task<int> CountUsersAsync();

        /// <summary>
        /// Lấy role của người dùng
        /// </summary>
        Task<Roles?> GetUserRoleAsync(string userId);
    }
}