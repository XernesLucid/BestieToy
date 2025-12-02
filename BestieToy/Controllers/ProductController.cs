using BestieToy.Data;
using BestieToy.Services;
using BestieToy.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BestieToy.Controllers
{
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IAuthService _authService;

        public ProductController(
            ILogger<ProductController> logger,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            ICartRepository cartRepository,
            IAuthService authService)
        {
            _logger = logger;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _cartRepository = cartRepository;
            _authService = authService;
        }

        // GET: /Product
        public async Task<IActionResult> Index(string? categoryId = null, string? petType = null,
            string? sortBy = null, decimal? minPrice = null, decimal? maxPrice = null,
            int page = 1, string? search = null)
        {
            try
            {
                IEnumerable<Models.Product> products;
                int totalProducts = 0;
                string pageTitle = "Tất cả sản phẩm";

                // Xử lý tìm kiếm
                if (!string.IsNullOrEmpty(search))
                {
                    products = await _productRepository.SearchProductsAsync(search, page, 12);
                    totalProducts = await _productRepository.CountProductsAsync();
                    pageTitle = $"Kết quả tìm kiếm: '{search}'";
                }
                // Xử lý lọc theo danh mục
                else if (!string.IsNullOrEmpty(categoryId))
                {
                    var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
                    if (category == null)
                    {
                        return NotFound();
                    }

                    products = await _productRepository.GetProductsByCategoryAsync(categoryId, page, 12);
                    totalProducts = await _productRepository.CountProductsByCategoryAsync(categoryId);
                    pageTitle = category.CategoryName;
                }
                // Xử lý lọc theo loại thú cưng
                else if (!string.IsNullOrEmpty(petType))
                {
                    products = await _productRepository.GetProductsByPetTypeAsync(petType, page, 12);
                    totalProducts = await _productRepository.CountProductsAsync();
                    pageTitle = petType == "Dog" ? "Sản phẩm cho Chó" : "Sản phẩm cho Mèo";
                }
                // Xử lý lọc theo giá
                else if (minPrice.HasValue || maxPrice.HasValue)
                {
                    decimal min = minPrice ?? 0;
                    decimal max = maxPrice ?? decimal.MaxValue;

                    products = await _productRepository.FilterProductsByPriceAsync(min, max, page, 12);
                    totalProducts = await _productRepository.CountProductsAsync();
                    pageTitle = $"Sản phẩm từ {min:0,0}đ - {max:0,0}đ";
                }
                // Mặc định: tất cả sản phẩm
                else
                {
                    products = await _productRepository.GetAllProductsAsync(page, 12);
                    totalProducts = await _productRepository.CountProductsAsync();
                }

                // Sắp xếp (mặc định là "newest" nếu sortBy null)
                products = SortProducts(products, sortBy ?? "newest");

                var categories = await _categoryRepository.GetActiveCategoriesAsync();
                var totalPages = (int)Math.Ceiling(totalProducts / 12.0);

                var model = new ProductListViewModel
                {
                    Products = products,
                    Categories = categories,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalProducts = totalProducts,
                    SearchKeyword = search,
                    CategoryId = categoryId,
                    PetType = petType,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    SortBy = sortBy ?? "newest",
                    PageTitle = pageTitle
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products page");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // GET: /Product/{id}
        [Route("Product/{id}")]
        public async Task<IActionResult> Detail(string id)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                // Lấy danh mục của sản phẩm
                var category = await _categoryRepository.GetCategoryByIdAsync(product.CategoryId);

                // Lấy sản phẩm liên quan
                var relatedProducts = await _productRepository.GetRelatedProductsAsync(id, 4);

                // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa (nếu đã đăng nhập)
                bool inCart = false;
                if (_authService.IsLoggedIn(HttpContext))
                {
                    var user = _authService.GetCurrentUser(HttpContext);
                    if (user != null)
                    {
                        var cart = await _cartRepository.GetCartByUserIdAsync(user.UserId);
                        if (cart != null)
                        {
                            inCart = await _cartRepository.ItemExistsInCartAsync(cart.CartId, id);
                        }
                    }
                }

                var model = new ProductDetailViewModel
                {
                    Product = product,
                    Category = category,
                    RelatedProducts = relatedProducts,
                    InCart = inCart
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product detail for id: {ProductId}", id);
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // POST: /Product/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(string productId, int quantity = 1)
        {
            if (!_authService.IsLoggedIn(HttpContext))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng";
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Detail", "Product", new { id = productId }) });
            }

            try
            {
                var user = _authService.GetCurrentUser(HttpContext);
                if (user == null)
                {
                    return Unauthorized();
                }

                // Lấy hoặc tạo giỏ hàng
                var cart = await _cartRepository.GetCartByUserIdAsync(user.UserId);
                if (cart == null)
                {
                    await _cartRepository.CreateCartAsync(user.UserId);
                    cart = await _cartRepository.GetCartByUserIdAsync(user.UserId);
                }

                if (cart == null)
                {
                    throw new Exception("Không thể tạo giỏ hàng");
                }

                // Kiểm tra số lượng tồn kho
                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound();
                }

                if (product.StockQuantity < quantity)
                {
                    TempData["ErrorMessage"] = "Số lượng sản phẩm không đủ";
                    return RedirectToAction("Detail", new { id = productId });
                }

                // Thêm vào giỏ hàng
                var success = await _cartRepository.AddToCartAsync(cart.CartId, productId, quantity);

                if (success)
                {
                    TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể thêm sản phẩm vào giỏ hàng";
                }

                return RedirectToAction("Detail", new { id = productId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product to cart: {ProductId}", productId);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi thêm vào giỏ hàng";
                return RedirectToAction("Detail", new { id = productId });
            }
        }

        // GET: /Product/ByCategory/{categoryId}
        [Route("Product/Category/{categoryId}")]
        public IActionResult ByCategory(string categoryId, int page = 1)
        {
            return RedirectToAction("Index", new { categoryId, page });
        }

        // GET: /Product/Pet/{petType}
        [Route("Product/Pet/{petType}")]
        public IActionResult ByPetType(string petType, int page = 1)
        {
            if (petType != "Dog" && petType != "Cat")
            {
                return NotFound();
            }

            return RedirectToAction("Index", new { petType, page });
        }

        // Helper method để sắp xếp sản phẩm (đã sửa null reference)
        private IEnumerable<Models.Product> SortProducts(IEnumerable<Models.Product> products, string sortBy)
        {
            return sortBy switch
            {
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                "name" => products.OrderBy(p => p.ProductName),
                _ => products.OrderByDescending(p => p.CreatedAt) // newest (default)
            };
        }
    }
}