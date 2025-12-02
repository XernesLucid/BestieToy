using BestieToy.Data;
using BestieToy.Services;
using BestieToy.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BestieToy.Controllers
{
    public class CartController : Controller
    {
        private readonly ILogger<CartController> _logger;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IAuthService _authService;

        public CartController(
            ILogger<CartController> logger,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IAuthService authService)
        {
            _logger = logger;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _authService = authService;
        }

        // GET: /Cart
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!_authService.IsLoggedIn(HttpContext))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem giỏ hàng";
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Index", "Cart") });
            }

            try
            {
                var user = _authService.GetCurrentUser(HttpContext);
                if (user == null)
                {
                    return Unauthorized();
                }

                // Lấy giỏ hàng
                var cart = await _cartRepository.GetCartByUserIdAsync(user.UserId);
                if (cart == null)
                {
                    // Nếu chưa có giỏ hàng, tạo mới
                    await _cartRepository.CreateCartAsync(user.UserId);
                    cart = await _cartRepository.GetCartByUserIdAsync(user.UserId);
                }

                if (cart == null)
                {
                    return View(new CartViewModel());
                }

                // Lấy items trong giỏ hàng
                var cartItems = await _cartRepository.GetCartItemsAsync(cart.CartId);

                // Tạo view model
                var items = new List<CartItemViewModel>();
                foreach (var item in cartItems)
                {
                    var product = await _productRepository.GetProductByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        items.Add(new CartItemViewModel
                        {
                            CartItemId = item.CartItemId,
                            ProductId = item.ProductId,
                            ProductName = product.ProductName,
                            ImageUrl = product.ImageUrl,
                            Price = product.Price,
                            Quantity = item.Quantity,
                            StockQuantity = product.StockQuantity
                        });
                    }
                }

                // Tính tổng tiền
                var subtotal = await _cartRepository.CalculateCartTotalAsync(cart.CartId);
                var itemCount = await _cartRepository.CountCartItemsAsync(cart.CartId);
                var shippingFee = CalculateShippingFee(subtotal);
                var tax = CalculateTax(subtotal);
                var total = subtotal + shippingFee + tax;

                var model = new CartViewModel
                {
                    CartId = cart.CartId,
                    UserId = user.UserId,
                    Items = items,
                    Subtotal = subtotal,
                    ShippingFee = shippingFee,
                    Tax = tax,
                    Total = total,
                    ItemCount = itemCount,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    ShippingAddress = user.Address
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // POST: /Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string productId, int quantity = 1, string? returnUrl = null)
        {
            if (!_authService.IsLoggedIn(HttpContext))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập", redirect = Url.Action("Login", "Auth") });
            }

            try
            {
                var user = _authService.GetCurrentUser(HttpContext);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng" });
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
                    return Json(new { success = false, message = "Không thể tạo giỏ hàng" });
                }

                // Kiểm tra số lượng tồn kho
                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                }

                if (product.StockQuantity < quantity)
                {
                    return Json(new { success = false, message = "Số lượng sản phẩm không đủ" });
                }

                // Thêm vào giỏ hàng
                var success = await _cartRepository.AddToCartAsync(cart.CartId, productId, quantity);

                if (success)
                {
                    // Cập nhật số lượng trong giỏ hàng
                    var itemCount = await _cartRepository.CountCartItemsAsync(cart.CartId);
                    return Json(new
                    {
                        success = true,
                        message = "Đã thêm vào giỏ hàng",
                        itemCount = itemCount,
                        returnUrl = returnUrl
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể thêm vào giỏ hàng" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart: {ProductId}", productId);
                return Json(new { success = false, message = "Đã xảy ra lỗi" });
            }
        }

        // POST: /Cart/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string cartItemId, int quantity)
        {
            if (!_authService.IsLoggedIn(HttpContext))
            {
                return Unauthorized();
            }

            try
            {
                var user = _authService.GetCurrentUser(HttpContext);
                if (user == null)
                {
                    return Unauthorized();
                }

                // Cập nhật số lượng
                var success = await _cartRepository.UpdateCartItemQuantityAsync(cartItemId, quantity);

                if (success)
                {
                    // Tính lại tổng tiền
                    var cart = await _cartRepository.GetCartByUserIdAsync(user.UserId);
                    if (cart != null)
                    {
                        var subtotal = await _cartRepository.CalculateCartTotalAsync(cart.CartId);
                        var itemCount = await _cartRepository.CountCartItemsAsync(cart.CartId);
                        var shippingFee = CalculateShippingFee(subtotal);
                        var tax = CalculateTax(subtotal);
                        var total = subtotal + shippingFee + tax;

                        return Json(new
                        {
                            success = true,
                            subtotal = subtotal.ToString("N0"),
                            shippingFee = shippingFee.ToString("N0"),
                            tax = tax.ToString("N0"),
                            total = total.ToString("N0"),
                            itemCount = itemCount
                        });
                    }
                }

                return Json(new { success = false, message = "Không thể cập nhật số lượng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item: {CartItemId}", cartItemId);
                return Json(new { success = false, message = "Đã xảy ra lỗi" });
            }
        }

        // POST: /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(string cartItemId)
        {
            if (!_authService.IsLoggedIn(HttpContext))
            {
                return Unauthorized();
            }

            try
            {
                var user = _authService.GetCurrentUser(HttpContext);
                if (user == null)
                {
                    return Unauthorized();
                }

                // Xóa item khỏi giỏ hàng
                var success = await _cartRepository.RemoveFromCartAsync(cartItemId);

                if (success)
                {
                    // Tính lại tổng tiền
                    var cart = await _cartRepository.GetCartByUserIdAsync(user.UserId);
                    if (cart != null)
                    {
                        var subtotal = await _cartRepository.CalculateCartTotalAsync(cart.CartId);
                        var itemCount = await _cartRepository.CountCartItemsAsync(cart.CartId);
                        var shippingFee = CalculateShippingFee(subtotal);
                        var tax = CalculateTax(subtotal);
                        var total = subtotal + shippingFee + tax;

                        return Json(new
                        {
                            success = true,
                            subtotal = subtotal.ToString("N0"),
                            shippingFee = shippingFee.ToString("N0"),
                            tax = tax.ToString("N0"),
                            total = total.ToString("N0"),
                            itemCount = itemCount
                        });
                    }
                }

                return Json(new { success = false, message = "Không thể xóa sản phẩm" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item: {CartItemId}", cartItemId);
                return Json(new { success = false, message = "Đã xảy ra lỗi" });
            }
        }

        // POST: /Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            if (!_authService.IsLoggedIn(HttpContext))
            {
                return Unauthorized();
            }

            try
            {
                var user = _authService.GetCurrentUser(HttpContext);
                if (user == null)
                {
                    return Unauthorized();
                }

                var cart = await _cartRepository.GetCartByUserIdAsync(user.UserId);
                if (cart == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy giỏ hàng" });
                }

                var success = await _cartRepository.ClearCartAsync(cart.CartId);

                if (success)
                {
                    TempData["SuccessMessage"] = "Đã xóa tất cả sản phẩm khỏi giỏ hàng";
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Không thể xóa giỏ hàng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return Json(new { success = false, message = "Đã xảy ra lỗi" });
            }
        }

        // GET: /Cart/Checkout
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (!_authService.IsLoggedIn(HttpContext))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thanh toán";
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Checkout", "Cart") });
            }

            try
            {
                var user = _authService.GetCurrentUser(HttpContext);
                if (user == null)
                {
                    return Unauthorized();
                }

                var cart = await _cartRepository.GetCartByUserIdAsync(user.UserId);
                if (cart == null)
                {
                    TempData["ErrorMessage"] = "Giỏ hàng trống";
                    return RedirectToAction("Index");
                }

                // Lấy thông tin giỏ hàng
                var cartItems = await _cartRepository.GetCartItemsAsync(cart.CartId);
                var itemCount = await _cartRepository.CountCartItemsAsync(cart.CartId);

                if (itemCount == 0)
                {
                    TempData["ErrorMessage"] = "Giỏ hàng trống";
                    return RedirectToAction("Index");
                }

                // Tính tổng tiền
                var subtotal = await _cartRepository.CalculateCartTotalAsync(cart.CartId);
                var shippingFee = CalculateShippingFee(subtotal);
                var tax = CalculateTax(subtotal);
                var total = subtotal + shippingFee + tax;

                var model = new CheckoutViewModel
                {
                    FullName = user.FullName ?? "",
                    Email = user.Email,
                    Phone = user.Phone ?? "",
                    ShippingAddress = user.Address ?? "",
                    Subtotal = subtotal,
                    ShippingFee = shippingFee,
                    Total = total,
                    ItemCount = itemCount
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading checkout");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // POST: /Cart/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!_authService.IsLoggedIn(HttpContext))
            {
                return Unauthorized();
            }

            try
            {
                var user = _authService.GetCurrentUser(HttpContext);
                if (user == null)
                {
                    return Unauthorized();
                }

                // TODO: Xử lý thanh toán
                // 1. Tạo đơn hàng (Order)
                // 2. Tạo chi tiết đơn hàng (OrderDetails)
                // 3. Cập nhật số lượng tồn kho
                // 4. Xóa giỏ hàng
                // 5. Gửi email xác nhận

                // Mô phỏng thanh toán thành công
                TempData["SuccessMessage"] = "Đặt hàng thành công! Cảm ơn bạn đã mua sắm tại BestieToy.";

                // Xóa giỏ hàng sau khi thanh toán
                var cart = await _cartRepository.GetCartByUserIdAsync(user.UserId);
                if (cart != null)
                {
                    await _cartRepository.ClearCartAsync(cart.CartId);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi trong quá trình thanh toán");
                return View(model);
            }
        }

        // Helper methods
        private decimal CalculateShippingFee(decimal subtotal)
        {
            if (subtotal == 0) return 0;
            if (subtotal >= 500000) return 0; // Miễn phí vận chuyển cho đơn trên 500k
            return 30000; // Phí vận chuyển cố định 30k
        }

        private decimal CalculateTax(decimal subtotal)
        {
            return subtotal * 0.1m; // VAT 10%
        }
    }
}