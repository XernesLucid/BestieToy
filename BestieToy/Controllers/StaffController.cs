using BestieToy.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestieToy.Controllers
{
    public class StaffController : Controller
    {
        private readonly IAuthService _authService;

        public StaffController(IAuthService authService)
        {
            _authService = authService;
        }

        private IActionResult? CheckStaffAccess()
        {
            if (!_authService.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Account");

            if (!_authService.IsStaff(HttpContext) && !_authService.IsAdmin(HttpContext))
                return RedirectToAction("AccessDenied", "Home");

            return null;
        }

        public IActionResult Index()
        {
            var accessCheck = CheckStaffAccess();
            if (accessCheck != null) return accessCheck;

            return View();
        }

        public IActionResult ProductManagement()
        {
            var accessCheck = CheckStaffAccess();
            if (accessCheck != null) return accessCheck;

            // Logic quản lý sản phẩm
            return View();
        }

        public IActionResult OrderManagement()
        {
            var accessCheck = CheckStaffAccess();
            if (accessCheck != null) return accessCheck;

            // Logic quản lý đơn hàng
            return View();
        }

        public IActionResult Profile()
        {
            var accessCheck = CheckStaffAccess();
            if (accessCheck != null) return accessCheck;

            var currentUser = _authService.GetCurrentUser(HttpContext);
            return View(currentUser);
        }
    }
}
