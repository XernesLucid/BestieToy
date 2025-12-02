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
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAuthService _authService;
        // TODO: Thêm OrderRepository, CartRepository khi cần

        public DashboardController(
            ILogger<DashboardController> logger,
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

        // GET: /Dashboard (Admin)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                // Lấy thống kê tổng quan
                var totalUsers = await _userRepository.CountUsersAsync();
                var totalProducts = await _productRepository.CountProductsAsync();
                var totalCategories = await _categoryRepository.CountCategoriesAsync();

                // Lấy sản phẩm sắp hết hàng
                var lowStockProducts = await _productRepository.GetLowStockProductsAsync(10);

                // Lấy sản phẩm mới nhất
                var newestProducts = await _productRepository.GetNewestProductsAsync(5);

                // Lấy danh sách users mới nhất
                var recentUsers = await GetRecentUsersAsync(5);

                var model = new DashboardViewModel
                {
                    TotalUsers = totalUsers,
                    TotalProducts = totalProducts,
                    TotalCategories = totalCategories,
                    LowStockProducts = lowStockProducts,
                    NewestProducts = newestProducts,
                    RecentUsers = recentUsers,
                    TotalRevenue = 0, // TODO: Tính từ OrderRepository
                    TotalOrders = 0, // TODO: Tính từ OrderRepository
                    TodayOrders = 0 // TODO: Tính từ OrderRepository
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // GET: /Dashboard/Stats
        [HttpGet]
        public IActionResult Stats(string period = "monthly")
        {
            if (!_authService.IsAdmin(HttpContext))
            {
                return Unauthorized();
            }

            try
            {
                // TODO: Lấy thống kê theo period (daily, weekly, monthly, yearly)
                var stats = GetMockStats(period);

                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stats for period: {Period}", period);
                return Json(new { error = "Không thể tải thống kê" });
            }
        }

        // GET: /Dashboard/Reports
        [HttpGet]
        public IActionResult Reports()
        {
            if (!_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            return View();
        }

        // POST: /Dashboard/GenerateReport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerateReport(ReportRequestViewModel model)
        {
            if (!_authService.IsAdmin(HttpContext))
            {
                return Unauthorized();
            }

            try
            {
                // TODO: Generate report based on model parameters
                TempData["SuccessMessage"] = "Đã tạo báo cáo thành công!";
                return RedirectToAction("Reports");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                TempData["ErrorMessage"] = "Không thể tạo báo cáo. Vui lòng thử lại.";
                return RedirectToAction("Reports");
            }
        }

        // GET: /Dashboard/Sales
        [HttpGet]
        public IActionResult Sales()
        {
            if (!_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            // TODO: Implement sales dashboard
            return View();
        }

        // GET: /Dashboard/Inventory
        [HttpGet]
        public async Task<IActionResult> Inventory()
        {
            if (!_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                // Lấy tất cả sản phẩm để tính giá trị tồn kho
                var allProducts = await _productRepository.GetAllProductsAsync(1, int.MaxValue);
                var inventoryValue = allProducts.Sum(p => p.Price * p.StockQuantity);
                var outOfStockCount = allProducts.Count(p => p.StockQuantity <= 0);
                var lowStockCount = allProducts.Count(p => p.StockQuantity > 0 && p.StockQuantity <= 10);

                var model = new InventoryViewModel
                {
                    TotalProducts = allProducts.Count(),
                    TotalValue = inventoryValue,
                    OutOfStockCount = outOfStockCount,
                    LowStockCount = lowStockCount,
                    Products = allProducts.OrderByDescending(p => p.Price * p.StockQuantity).Take(20)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inventory dashboard");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // Helper methods
        private async Task<IEnumerable<UserViewModel>> GetRecentUsersAsync(int limit)
        {
            var allUsers = await _userRepository.GetAllUsersAsync();
            return allUsers
                .OrderByDescending(u => u.CreatedAt)
                .Take(limit)
                .Select(u => new UserViewModel
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    CreatedAt = u.CreatedAt
                });
        }

        private StatsViewModel GetMockStats(string period)
        {
            // Mock data - trong thực tế cần query từ database
            return new StatsViewModel
            {
                Period = period,
                RevenueData = new List<decimal> { 5000000, 7500000, 6200000, 8900000, 9500000, 8200000 },
                OrderData = new List<int> { 45, 62, 58, 71, 68, 65 },
                UserData = new List<int> { 120, 135, 142, 158, 165, 172 },
                TopCategories = new List<CategoryStats>
                {
                    new CategoryStats { CategoryName = "Đồ chơi thông minh", Revenue = 25000000, Orders = 45 },
                    new CategoryStats { CategoryName = "Thức ăn cao cấp", Revenue = 18000000, Orders = 38 },
                    new CategoryStats { CategoryName = "Phụ kiện", Revenue = 12000000, Orders = 32 }
                }
            };
        }
    }

    // ViewModels cho Dashboard
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TodayOrders { get; set; }
        public IEnumerable<Models.Product> LowStockProducts { get; set; } = new List<Models.Product>();
        public IEnumerable<Models.Product> NewestProducts { get; set; } = new List<Models.Product>();
        public IEnumerable<UserViewModel> RecentUsers { get; set; } = new List<UserViewModel>();
    }

    public class UserViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class StatsViewModel
    {
        public string Period { get; set; } = string.Empty;
        public List<decimal> RevenueData { get; set; } = new List<decimal>();
        public List<int> OrderData { get; set; } = new List<int>();
        public List<int> UserData { get; set; } = new List<int>();
        public List<CategoryStats> TopCategories { get; set; } = new List<CategoryStats>();
    }

    public class CategoryStats
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }

    public class ReportRequestViewModel
    {
        [Required(ErrorMessage = "Loại báo cáo là bắt buộc")]
        [Display(Name = "Loại báo cáo")]
        public string ReportType { get; set; } = string.Empty; // sales, inventory, users

        [Required(ErrorMessage = "Từ ngày là bắt buộc")]
        [Display(Name = "Từ ngày")]
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; } = DateTime.Now.AddMonths(-1);

        [Required(ErrorMessage = "Đến ngày là bắt buộc")]
        [Display(Name = "Đến ngày")]
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; } = DateTime.Now;

        [Display(Name = "Định dạng")]
        public string Format { get; set; } = "pdf"; // pdf, excel, csv
    }

    public class InventoryViewModel
    {
        public int TotalProducts { get; set; }
        public decimal TotalValue { get; set; }
        public int OutOfStockCount { get; set; }
        public int LowStockCount { get; set; }
        public IEnumerable<Models.Product> Products { get; set; } = new List<Models.Product>();
    }
}