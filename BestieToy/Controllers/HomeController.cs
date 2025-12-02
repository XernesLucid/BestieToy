using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using BestieToy.Data;
using BestieToy.Services;
using BestieToy.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BestieToy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAuthService _authService;

        public HomeController(
            ILogger<HomeController> logger,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IAuthService authService)
        {
            _logger = logger;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _authService = authService;
        }

        // GET: /
        public async Task<IActionResult> Index()
        {
            try
            {
                var model = new HomeViewModel
                {
                    FeaturedProducts = await _productRepository.GetFeaturedProductsAsync(8),
                    NewestProducts = await _productRepository.GetNewestProductsAsync(8),
                    Categories = await _categoryRepository.GetActiveCategoriesAsync(),
                    IsLoggedIn = _authService.IsLoggedIn(HttpContext),
                    CurrentUser = _authService.GetCurrentUser(HttpContext)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // GET: /Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }


        // GET: /Home/About
        public IActionResult About()
        {
            return View();
        }

        // GET: /Home/Contact
        public IActionResult Contact()
        {
            return View(new ContactViewModel());
        }

        // POST: /Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // TODO: Xử lý gửi contact form
            TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể.";
            return RedirectToAction("Contact");
        }

        // GET: /Home/Search
        public async Task<IActionResult> Search(string keyword, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return RedirectToAction("Index");
            }

            try
            {
                var products = await _productRepository.SearchProductsAsync(keyword, page, 12);
                var totalProducts = await _productRepository.CountProductsAsync();
                var totalPages = (int)Math.Ceiling(totalProducts / 12.0);

                var model = new ProductListViewModel
                {
                    Products = products,
                    SearchKeyword = keyword,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalProducts = totalProducts,
                    PageTitle = $"Kết quả tìm kiếm: '{keyword}'"
                };

                return View("~/Views/Product/Index.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with keyword: {Keyword}", keyword);
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // ViewModel cho Home
    public class HomeViewModel
    {
        public IEnumerable<Models.Product> FeaturedProducts { get; set; } = new List<Models.Product>();
        public IEnumerable<Models.Product> NewestProducts { get; set; } = new List<Models.Product>();
        public IEnumerable<Models.Category> Categories { get; set; } = new List<Models.Category>();
        public bool IsLoggedIn { get; set; }
        public Models.User? CurrentUser { get; set; }
    }

    // ViewModel cho Contact
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ tên")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không quá 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        [StringLength(2000, ErrorMessage = "Nội dung không quá 2000 ký tự")]
        [Display(Name = "Nội dung")]
        public string Message { get; set; } = string.Empty;
    }
}