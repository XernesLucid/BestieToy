using BestieToy.Models;
using BestieToy.Data;

namespace BestieToy.Services
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        void Login(User user, HttpContext context);
        void Logout(HttpContext context);
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

            context.Session.SetString("UserId", user.UserId);
            context.Session.SetString("Username", user.Username);
            context.Session.SetString("RoleId", user.RoleId);
            context.Session.SetString("FullName", user.FullName ?? "");
            context.Session.SetString("Email", user.Email);


            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7), 
                HttpOnly = true, 
                Secure = true, 
                SameSite = SameSiteMode.Strict
            };

            context.Response.Cookies.Append("UserAuth", user.UserId, cookieOptions);
        }

        public void Logout(HttpContext context)
        {
            // Clear Session
            context.Session.Clear();

            // Clear Cookies
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
            if (string.IsNullOrEmpty(userId)) return null;

            return new User
            {
                UserId = userId,
                Username = context.Session.GetString("Username") ?? "",
                RoleId = context.Session.GetString("RoleId") ?? "",
                FullName = context.Session.GetString("FullName"),
                Email = context.Session.GetString("Email") ?? "" // Vẫn cần vì Session.GetString có thể trả về null
            };
        }

        public bool IsLoggedIn(HttpContext context)
            => !string.IsNullOrEmpty(context.Session.GetString("UserId"));

        public bool IsAdmin(HttpContext context)
            => context.Session.GetString("RoleId") == "ROLE001";

        public bool IsStaff(HttpContext context)
            => context.Session.GetString("RoleId") == "ROLE002";

        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            return inputPassword == storedPassword;
        }
    }
}