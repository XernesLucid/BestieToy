using BestieToy.Services;
using BestieToy.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BestieToy.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;
        private readonly BestieToy.Data.IUserRepository _userRepository; // Thêm dependency

        public AccountController(IAuthService authService, ILogger<AccountController> logger, BestieToy.Data.IUserRepository userRepository)
        {
            _authService = authService;
            _logger = logger;
            _userRepository = userRepository; // Inject repository
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Nếu đã đăng nhập, chuyển hướng về trang chủ
            if (_authService.IsLoggedIn(HttpContext))
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Xác thực người dùng
                var user = await _authService.AuthenticateAsync(model.Username, model.Password);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng");
                    return View(model);
                }

                // Đăng nhập và lưu session/cookie
                _authService.Login(user, HttpContext);

                // Ghi log đăng nhập thành công
                _logger.LogInformation($"User {model.Username} logged in successfully");

                // Chuyển hướng về returnUrl hoặc trang chủ
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                // Phân quyền chuyển hướng
                if (_authService.IsAdmin(HttpContext))
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (_authService.IsStaff(HttpContext))
                {
                    return RedirectToAction("Index", "Staff");
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user {Username}", model.Username);
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi trong quá trình đăng nhập");
                return View(model);
            }
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            // Nếu đã đăng nhập, chuyển hướng về trang chủ
            if (_authService.IsLoggedIn(HttpContext))
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model) // Đã thêm async
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Kiểm tra username đã tồn tại chưa
                var existingUser = await _userRepository.GetUserByUsernameAsync(model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã được sử dụng");
                    return View(model);
                }

                // Kiểm tra email đã tồn tại chưa
                var existingEmail = await _userRepository.GetUserByEmailAsync(model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    return View(model);
                }

                // Tạo user mới
                var newUser = new Models.User
                {
                    UserId = $"UID{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                    Username = model.Username,
                    Password = model.Password, // Lưu ý: Cần hash password trong thực tế
                    Email = model.Email,
                    FullName = model.FullName,
                    Phone = model.Phone,
                    Address = model.Address,
                    RoleId = model.RoleId,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                // Lưu user vào database
                var success = await _userRepository.AddUserAsync(newUser);

                if (!success)
                {
                    ModelState.AddModelError(string.Empty, "Không thể tạo tài khoản. Vui lòng thử lại.");
                    return View(model);
                }

                // Đăng nhập tự động sau khi đăng ký
                _authService.Login(newUser, HttpContext);

                _logger.LogInformation($"New user registered: {model.Username}");

                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for user {Username}", model.Username);
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi trong quá trình đăng ký");
                return View(model);
            }
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            _authService.Logout(HttpContext);
            TempData["SuccessMessage"] = "Đã đăng xuất thành công";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // Model cho Error View
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}