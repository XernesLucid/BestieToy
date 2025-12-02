using BestieToy.Data;
using BestieToy.Services;
using BestieToy.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BestieToy.Controllers
{
    [Authorize]
    public class StaffController : Controller
    {
        private readonly ILogger<StaffController> _logger;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAuthService _authService;

        public StaffController(
            ILogger<StaffController> logger,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IAuthService authService)
        {
            _logger = logger;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _authService = authService;
        }

        // GET: /Staff
        [HttpGet]
        public IActionResult Index()
        {
            if (!_authService.IsStaff(HttpContext) && !_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            return View();
        }

        // GET: /Staff/Products
        [HttpGet]
        public async Task<IActionResult> Products(string? search = null, string? categoryId = null,
            int page = 1, bool? isActive = true)
        {
            if (!_authService.IsStaff(HttpContext) && !_authService.IsAdmin(HttpContext))
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
                var categories = await _categoryRepository.GetActiveCategoriesAsync();

                var model = new ProductListViewModel
                {
                    Products = products,
                    Categories = categories,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalProducts = totalProducts,
                    SearchKeyword = search,
                    CategoryId = categoryId,
                    IsActive = isActive ?? true
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading staff products page");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // GET: /Staff/Products/Create
        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            if (!_authService.IsStaff(HttpContext) && !_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var categories = await _categoryRepository.GetActiveCategoriesAsync();
            ViewBag.Categories = categories;
            return View();
        }

        // POST: /Staff/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Models.Product product)
        {
            if (!_authService.IsStaff(HttpContext) && !_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    var categories = await _categoryRepository.GetActiveCategoriesAsync();
                    ViewBag.Categories = categories;
                    return View(product);
                }

                // Tạo ProductId mới
                product.ProductId = $"PID{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
                product.CreatedAt = DateTime.Now;
                product.IsActive = true;

                // Nếu không có stock quantity, mặc định là 0
                if (product.StockQuantity < 0)
                {
                    product.StockQuantity = 0;
                }

                var success = await _productRepository.AddProductAsync(product);

                if (success)
                {
                    TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                    return RedirectToAction("Products");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Không thể thêm sản phẩm. Vui lòng thử lại.");
                    var categories = await _categoryRepository.GetActiveCategoriesAsync();
                    ViewBag.Categories = categories;
                    return View(product);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi thêm sản phẩm");
                var categories = await _categoryRepository.GetActiveCategoriesAsync();
                ViewBag.Categories = categories;
                return View(product);
            }
        }

        // GET: /Staff/Products/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> EditProduct(string id)
        {
            if (!_authService.IsStaff(HttpContext) && !_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                var product = await _productRepository.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                var categories = await _categoryRepository.GetActiveCategoriesAsync();
                ViewBag.Categories = categories;

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product for edit: {ProductId}", id);
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // POST: /Staff/Products/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(string id, Models.Product product)
        {
            if (!_authService.IsStaff(HttpContext) && !_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                if (id != product.ProductId)
                {
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    var categories = await _categoryRepository.GetActiveCategoriesAsync();
                    ViewBag.Categories = categories;
                    return View(product);
                }

                var success = await _productRepository.UpdateProductAsync(product);

                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction("Products");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Không thể cập nhật sản phẩm. Vui lòng thử lại.");
                    var categories = await _categoryRepository.GetActiveCategoriesAsync();
                    ViewBag.Categories = categories;
                    return View(product);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductId}", id);
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi cập nhật sản phẩm");
                var categories = await _categoryRepository.GetActiveCategoriesAsync();
                ViewBag.Categories = categories;
                return View(product);
            }
        }

        // POST: /Staff/Products/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            if (!_authService.IsStaff(HttpContext) && !_authService.IsAdmin(HttpContext))
            {
                return Unauthorized();
            }

            try
            {
                var success = await _productRepository.DeleteProductAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa sản phẩm. Vui lòng thử lại.";
                }

                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {ProductId}", id);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa sản phẩm";
                return RedirectToAction("Products");
            }
        }

        // GET: /Staff/Orders
        [HttpGet]
        public IActionResult Orders()
        {
            if (!_authService.IsStaff(HttpContext) && !_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            // TODO: Implement order management for staff
            return View();
        }

        // GET: /Staff/LowStock
        [HttpGet]
        public async Task<IActionResult> LowStock(int threshold = 10)
        {
            if (!_authService.IsStaff(HttpContext) && !_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                var lowStockProducts = await _productRepository.GetLowStockProductsAsync(threshold);
                return View(lowStockProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading low stock products");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // GET: /Staff/Profile
        [HttpGet]
        public IActionResult Profile()
        {
            if (!_authService.IsStaff(HttpContext) && !_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var user = _authService.GetCurrentUser(HttpContext);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(user);
        }

        // POST: /Staff/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(Models.User user)
        {
            if (!_authService.IsStaff(HttpContext) && !_authService.IsAdmin(HttpContext))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                var currentUser = _authService.GetCurrentUser(HttpContext);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Cập nhật thông tin user
                // TODO: Implement user update logic
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff profile");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi cập nhật thông tin");
                return View(user);
            }
        }
    }
}