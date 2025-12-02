using BestieToy.Models;
using BestieToy.Data;

namespace BestieToy.Services
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        void Login(User user, HttpContext context);
        void Logout(HttpContext context);
        bool HasRememberMeCookie(HttpContext context);
        string? GetUserIdFromCookie(HttpContext context);
        User? GetCurrentUser(HttpContext context);
        bool IsLoggedIn(HttpContext context);
        bool IsAdmin(HttpContext context);
        bool IsStaff(HttpContext context);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user != null && VerifyPassword(password, user.Password))
            {
                return user;
            }
            return null;
        }

        public void Login(User user, HttpContext context)
        {
            // Lưu session
            context.Session.SetString("UserId", user.UserId);
            context.Session.SetString("Username", user.Username);
            context.Session.SetString("RoleId", user.RoleId);
            context.Session.SetString("FullName", user.FullName ?? "");
            context.Session.SetString("Email", user.Email ?? "");

            // Lưu cookie "Remember me"
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Strict
            };

            context.Response.Cookies.Append("UserAuth", user.UserId, cookieOptions);
        }

        public void Logout(HttpContext context)
        {
            // Xóa session
            context.Session.Clear();

            // Xóa cookie
            context.Response.Cookies.Delete("UserAuth");
        }

        public bool HasRememberMeCookie(HttpContext context)
        {
            return context.Request.Cookies.ContainsKey("UserAuth");
        }

        public string? GetUserIdFromCookie(HttpContext context)
        {
            return context.Request.Cookies["UserAuth"];
        }

        public User? GetCurrentUser(HttpContext context)
        {
            var userId = context.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu không có session, kiểm tra cookie
                userId = GetUserIdFromCookie(context);
                if (!string.IsNullOrEmpty(userId))
                {
                    // Tải user từ database và tạo session mới
                    var user = _userRepository.GetUserByIdAsync(userId).Result;
                    if (user != null)
                    {
                        Login(user, context);
                        return user;
                    }
                }
                return null;
            }

            // Tạo user object từ session
            return new User
            {
                UserId = userId,
                Username = context.Session.GetString("Username") ?? "",
                RoleId = context.Session.GetString("RoleId") ?? "",
                FullName = context.Session.GetString("FullName"),
                Email = context.Session.GetString("Email") ?? ""
            };
        }

        public bool IsLoggedIn(HttpContext context)
        {
            return !string.IsNullOrEmpty(context.Session.GetString("UserId")) ||
                   HasRememberMeCookie(context);
        }

        public bool IsAdmin(HttpContext context)
        {
            var roleId = context.Session.GetString("RoleId");
            return roleId == "ROLE001";
        }

        public bool IsStaff(HttpContext context)
        {
            var roleId = context.Session.GetString("RoleId");
            return roleId == "ROLE002";
        }

        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            // TODO: Implement password hashing/verification
            // Hiện tại chỉ so sánh plain text (cho development)
            return inputPassword == storedPassword;
        }
    }
}