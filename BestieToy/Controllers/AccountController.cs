using BestieToy.Data;
using BestieToy.Models;
using BestieToy.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestieToy.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;

        public AccountController(IAuthService authService, IUserRepository userRepository)
        {
            _authService = authService;
            _userRepository = userRepository;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (_authService.IsLoggedIn(HttpContext))
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe)
        {
            var user = await _authService.AuthenticateAsync(username, password);
            if (user != null)
            {
                _authService.Login(user, HttpContext);

                // Nếu chọn "Remember Me", sẽ tạo cookie
                if (rememberMe)
                {
                    // Cookie đã được tạo trong AuthService.Login
                }

                // Redirect based on role
                return user.RoleId switch
                {
                    "ROLE001" => RedirectToAction("Dashboard", "Admin"),
                    "ROLE002" => RedirectToAction("Index", "Staff"),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng";
            return View();
        }

        public IActionResult Logout()
        {
            _authService.Logout(HttpContext);
            return RedirectToAction("Index", "Home");
        }
    }
}