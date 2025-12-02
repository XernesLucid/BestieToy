using BestieToy.Data;
using BestieToy.Services;
using BestieToy.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BestieToy.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAuthService _authService;

        public AdminController(
            ILogger<AdminController> logger,
            IUserRepository userRepository,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IAuthService authService)
        {
            _logger = logger;
            _userRepository = userRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _authService = authService;
        }

        // GET: /Admin
        [HttpGet]
        public IActionResult Index()
        {
            if (!_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            return View();
        }

        // GET: /Admin/Users
        [HttpGet]
        public async Task<IActionResult> Users(string? search = null, string? roleId = null,
            bool? isActive = null, int page = 1)
        {
            if (!_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                IEnumerable<Models.User> users;

                if (!string.IsNullOrEmpty(search))
                {
                    // TODO: Cần thêm method search users
                    users = await _userRepository.GetAllUsersAsync();
                    users = users.Where(u =>
                        u.Username.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (u.FullName != null && u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)));
                }
                else if (!string.IsNullOrEmpty(roleId))
                {
                    users = await _userRepository.GetUsersByRoleAsync(roleId);
                }
                else
                {
                    users = await _userRepository.GetAllUsersAsync();
                }

                // Lọc theo trạng thái active
                if (isActive.HasValue)
                {
                    users = users.Where(u => u.IsActive == isActive.Value);
                }

                // Phân trang
                var totalUsers = users.Count();
                var pageSize = 20;
                var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
                users = users.Skip((page - 1) * pageSize).Take(pageSize);

                // Lấy danh sách roles
                var allUsers = await _userRepository.GetAllUsersAsync();
                var roles = new List<Models.Roles>
                {
                    new Models.Roles { RoleId = "ROLE001", RoleName = "Admin" },
                    new Models.Roles { RoleId = "ROLE002", RoleName = "Staff" },
                    new Models.Roles { RoleId = "ROLE003", RoleName = "Customer" }
                };

                // Tính statistics
                var totalAdmins = allUsers.Count(u => u.RoleId == "ROLE001");
                var totalStaff = allUsers.Count(u => u.RoleId == "ROLE002");
                var totalCustomers = allUsers.Count(u => u.RoleId == "ROLE003");
                var totalActiveUsers = allUsers.Count(u => u.IsActive);

                var model = new UserManagementViewModel
                {
                    Users = users,
                    Roles = roles,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalUsers = totalUsers,
                    SearchKeyword = search,
                    RoleId = roleId,
                    IsActive = isActive,
                    TotalAdmins = totalAdmins,
                    TotalStaff = totalStaff,
                    TotalCustomers = totalCustomers,
                    TotalActiveUsers = totalActiveUsers
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin users page");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // GET: /Admin/Products
        [HttpGet]
        public async Task<IActionResult> Products(string? search = null, string? categoryId = null,
            string? petType = null, bool? isActive = null, int page = 1)
        {
            if (!_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                IEnumerable<Models.Product> products;

                if (!string.IsNullOrEmpty(search))
                {
                    products = await _productRepository.SearchProductsAsync(search, page, 20);
                }
                else if (!string.IsNullOrEmpty(categoryId))
                {
                    products = await _productRepository.GetProductsByCategoryAsync(categoryId, page, 20);
                }
                else if (!string.IsNullOrEmpty(petType))
                {
                    products = await _productRepository.GetProductsByPetTypeAsync(petType, page, 20);
                }
                else
                {
                    products = await _productRepository.GetAllProductsAsync(page, 20);
                }

                // Lọc theo trạng thái active
                if (isActive.HasValue)
                {
                    products = products.Where(p => p.IsActive == isActive.Value);
                }

                // Lấy tổng số sản phẩm
                var totalProducts = await _productRepository.CountProductsAsync();
                var pageSize = 20;
                var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

                // Lấy danh sách categories
                var categories = await _categoryRepository.GetAllCategoriesAsync();

                // Tính statistics
                var allProducts = await _productRepository.GetAllProductsAsync(1, int.MaxValue);
                var totalActiveProducts = allProducts.Count(p => p.IsActive);
                var totalOutOfStock = allProducts.Count(p => p.StockQuantity <= 0);
                var totalInventoryValue = allProducts.Sum(p => p.Price * p.StockQuantity);

                var model = new ProductManagementViewModel
                {
                    Products = products,
                    Categories = categories,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalProducts = totalProducts,
                    SearchKeyword = search,
                    CategoryId = categoryId,
                    PetType = petType,
                    IsActive = isActive,
                    TotalActiveProducts = totalActiveProducts,
                    TotalOutOfStock = totalOutOfStock,
                    TotalInventoryValue = totalInventoryValue
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin products page");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // GET: /Admin/Categories
        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            if (!_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                var categories = await _categoryRepository.GetAllCategoriesAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin categories page");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // GET: /Admin/Orders
        [HttpGet]
        public IActionResult Orders()
        {
            if (!_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            // TODO: Implement order management
            return View();
        }

        // GET: /Admin/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (!_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                // TODO: Lấy thống kê cho dashboard
                var totalUsers = await _userRepository.CountUsersAsync();
                var totalProducts = await _productRepository.CountProductsAsync();
                var totalCategories = await _categoryRepository.CountCategoriesAsync();

                ViewBag.TotalUsers = totalUsers;
                ViewBag.TotalProducts = totalProducts;
                ViewBag.TotalCategories = totalCategories;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
}