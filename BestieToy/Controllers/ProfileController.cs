using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using BestieToy.Data;
using BestieToy.Services;
using BestieToy.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BestieToy.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;

        public ProfileController(
            ILogger<ProfileController> logger,
            IAuthService authService,
            IUserRepository userRepository)
        {
            _logger = logger;
            _authService = authService;
            _userRepository = userRepository;
        }

        // GET: /Profile
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!_authService.IsLoggedIn(HttpContext))
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Index", "Profile") });
            }

            try
            {
                var user = _authService.GetCurrentUser(HttpContext);
                if (user == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Lấy thông tin đầy đủ từ database
                var fullUser = await _userRepository.GetUserByIdAsync(user.UserId);
                if (fullUser == null)
                {
                    return NotFound();
                }

                var model = new ProfileViewModel
                {
                    UserId = fullUser.UserId,
                    Username = fullUser.Username,
                    Email = fullUser.Email,
                    FullName = fullUser.FullName ?? "",
                    Phone = fullUser.Phone ?? "",
                    Address = fullUser.Address ?? "",
                    CreatedAt = fullUser.CreatedAt
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // GET: /Profile/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            if (!_authService.IsLoggedIn(HttpContext))
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Edit", "Profile") });
            }

            try
            {
                var user = _authService.GetCurrentUser(HttpContext);
                if (user == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Lấy thông tin đầy đủ từ database
                var fullUser = await _userRepository.GetUserByIdAsync(user.UserId);
                if (fullUser == null)
                {
                    return NotFound();
                }

                var model = new EditProfileViewModel
                {
                    FullName = fullUser.FullName ?? "",
                    Phone = fullUser.Phone ?? "",
                    Address = fullUser.Address ?? ""
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile edit page");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!_authService.IsLoggedIn(HttpContext))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = _authService.GetCurrentUser(HttpContext);
                if (user == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Lấy thông tin user từ database
                var fullUser = await _userRepository.GetUserByIdAsync(user.UserId);
                if (fullUser == null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin
                fullUser.FullName = model.FullName;
                fullUser.Phone = model.Phone;
                fullUser.Address = model.Address;

                var success = await _userRepository.UpdateUserAsync(fullUser);

                if (success)
                {
                    // Cập nhật session
                    _authService.Login(fullUser, HttpContext);

                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Không thể cập nhật thông tin. Vui lòng thử lại.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi cập nhật thông tin");
                return View(model);
            }
        }

        // GET: /Profile/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (!_authService.IsLoggedIn(HttpContext))
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("ChangePassword", "Profile") });
            }

            return View(new ChangePasswordViewModel());
        }

        // POST: /Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!_authService.IsLoggedIn(HttpContext))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = _authService.GetCurrentUser(HttpContext);
                if (user == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Lấy thông tin user từ database
                var fullUser = await _userRepository.GetUserByIdAsync(user.UserId);
                if (fullUser == null)
                {
                    return NotFound();
                }

                // Kiểm tra mật khẩu cũ (đơn giản - trong thực tế cần hash)
                if (fullUser.Password != model.OldPassword)
                {
                    ModelState.AddModelError("OldPassword", "Mật khẩu cũ không đúng");
                    return View(model);
                }

                // Cập nhật mật khẩu mới
                fullUser.Password = model.NewPassword; // Lưu ý: Cần hash password trong thực tế

                var success = await _userRepository.UpdateUserAsync(fullUser);

                if (success)
                {
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Không thể đổi mật khẩu. Vui lòng thử lại.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi đổi mật khẩu");
                return View(model);
            }
        }

        // GET: /Profile/Orders
        [HttpGet]
        public IActionResult Orders()
        {
            // Chuyển hướng đến OrderController
            return RedirectToAction("History", "Order");
        }
    }

    // ViewModels cho Profile
    public class ProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(255, ErrorMessage = "Họ tên không quá 255 ký tự")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Địa chỉ không quá 500 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } = string.Empty;
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu cũ")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-100 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}