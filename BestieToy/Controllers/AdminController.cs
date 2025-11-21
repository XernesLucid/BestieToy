using BestieToy.Data;
using BestieToy.Models;
using BestieToy.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestieToy.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;

        public AdminController(IAuthService authService, IUserRepository userRepository)
        {
            _authService = authService;
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Dashboard()
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            // Thống kê cho dashboard
            var totalUsers = await _userRepository.GetAllUsersAsync();
            var adminUsers = await _userRepository.GetUsersByRoleAsync("ROLE001");
            var staffUsers = await _userRepository.GetUsersByRoleAsync("ROLE002");
            var customerUsers = await _userRepository.GetUsersByRoleAsync("ROLE003");

            ViewBag.TotalUsers = totalUsers.Count;
            ViewBag.AdminCount = adminUsers.Count;
            ViewBag.StaffCount = staffUsers.Count;
            ViewBag.CustomerCount = customerUsers.Count;

            return View();
        }

        public async Task<IActionResult> UserManagement()
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var users = await _userRepository.GetAllUsersAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            if (ModelState.IsValid)
            {
                // Generate UserId mới
                user.UserId = "UID" + DateTime.Now.ToString("yyyyMMddHHmmss");

                var result = await _userRepository.CreateUserAsync(user);
                if (result)
                {
                    return RedirectToAction("UserManagement");
                }
            }
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(User user)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            if (ModelState.IsValid)
            {
                var result = await _userRepository.UpdateUserAsync(user);
                if (result)
                {
                    return RedirectToAction("UserManagement");
                }
            }
            return View(user);
        }

        // Check authorization for all actions
        private IActionResult? CheckAdminAccess()
        {
            if (!_authService.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Account");

            if (!_authService.IsAdmin(HttpContext))
                return RedirectToAction("AccessDenied", "Home");

            return null;
        }

        public IActionResult Index()
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            return View();
        }

        public IActionResult Users()
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            // Logic quản lý users
            return View();
        }

        public IActionResult Reports()
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            // Logic báo cáo thống kê
            return View();
        }
    }
}