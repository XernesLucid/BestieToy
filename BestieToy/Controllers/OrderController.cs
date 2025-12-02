using BestieToy.Data;
using BestieToy.Services;
using BestieToy.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BestieToy.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IAuthService _authService;
        // TODO: Thêm OrderRepository khi tạo

        public OrderController(
            ILogger<OrderController> logger,
            IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        // GET: /Order/History
        [HttpGet]
        public IActionResult History(int page = 1)
        {
            if (!_authService.IsLoggedIn(HttpContext))
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("History", "Order") });
            }

            try
            {
                // TODO: Lấy lịch sử đơn hàng từ OrderRepository
                var mockOrders = GetMockOrders();

                // Phân trang
                var pageSize = 10;
                var totalOrders = mockOrders.Count();
                var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);
                var orders = mockOrders.Skip((page - 1) * pageSize).Take(pageSize);

                var model = new OrderHistoryViewModel
                {
                    Orders = orders,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalOrders = totalOrders
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order history");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // GET: /Order/Detail/{id}
        [HttpGet]
        public IActionResult Detail(string id)
        {
            if (!_authService.IsLoggedIn(HttpContext))
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Detail", "Order", new { id }) });
            }

            try
            {
                // TODO: Lấy chi tiết đơn hàng từ OrderRepository
                var mockOrder = GetMockOrderDetail(id);
                if (mockOrder == null)
                {
                    return NotFound();
                }

                return View(mockOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order detail: {OrderId}", id);
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // POST: /Order/Cancel/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(string id)
        {
            if (!_authService.IsLoggedIn(HttpContext))
            {
                return Unauthorized();
            }

            try
            {
                // TODO: Implement cancel order logic
                TempData["SuccessMessage"] = "Đã hủy đơn hàng thành công!";
                return RedirectToAction("History");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling order: {OrderId}", id);
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng. Vui lòng liên hệ hỗ trợ.";
                return RedirectToAction("History");
            }
        }

        // GET: /Order/Track/{id}
        [HttpGet]
        public IActionResult Track(string id)
        {
            if (!_authService.IsLoggedIn(HttpContext))
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Track", "Order", new { id }) });
            }

            try
            {
                // TODO: Lấy thông tin tracking từ OrderRepository
                var trackingInfo = GetMockTrackingInfo(id);
                if (trackingInfo == null)
                {
                    return NotFound();
                }

                return View(trackingInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tracking info: {OrderId}", id);
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // Helper methods for mock data (tạm thời)
        private IEnumerable<OrderViewModel> GetMockOrders()
        {
            return new List<OrderViewModel>
            {
                new OrderViewModel
                {
                    OrderId = "ORD2024120001",
                    OrderDate = DateTime.Now.AddDays(-5),
                    TotalAmount = 865000,
                    Status = "Đã giao hàng",
                    ItemCount = 3
                },
                new OrderViewModel
                {
                    OrderId = "ORD2024120002",
                    OrderDate = DateTime.Now.AddDays(-3),
                    TotalAmount = 285000,
                    Status = "Đang giao hàng",
                    ItemCount = 2
                },
                new OrderViewModel
                {
                    OrderId = "ORD2024120003",
                    OrderDate = DateTime.Now.AddDays(-1),
                    TotalAmount = 320000,
                    Status = "Đã xác nhận",
                    ItemCount = 1
                }
            };
        }

        private OrderDetailViewModel? GetMockOrderDetail(string orderId)
        {
            var orders = GetMockOrders();
            var order = orders.FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                return null;

            return new OrderDetailViewModel
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = "123 Nguyễn Huệ, Quận 1, TP.HCM",
                Phone = "0903000101",
                Notes = "Giao giờ hành chính",
                Items = new List<OrderItemViewModel>
                {
                    new OrderItemViewModel
                    {
                        ProductName = "Bóng Thông Minh Kong Classic",
                        Quantity = 1,
                        UnitPrice = 185000,
                        Total = 185000
                    },
                    new OrderItemViewModel
                    {
                        ProductName = "Hạt Khô Royal Canin Mini Adult",
                        Quantity = 2,
                        UnitPrice = 450000,
                        Total = 900000
                    }
                }
            };
        }

        private TrackingViewModel? GetMockTrackingInfo(string orderId)
        {
            var orders = GetMockOrders();
            var order = orders.FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                return null;

            return new TrackingViewModel
            {
                OrderId = order.OrderId,
                Status = order.Status,
                EstimatedDelivery = DateTime.Now.AddDays(2),
                TrackingNumber = "VN123456789",
                TrackingSteps = new List<TrackingStep>
                {
                    new TrackingStep { Step = "Đơn hàng đã đặt", Time = order.OrderDate, IsCompleted = true },
                    new TrackingStep { Step = "Đơn hàng đã xác nhận", Time = order.OrderDate.AddHours(1), IsCompleted = true },
                    new TrackingStep { Step = "Đang đóng gói", Time = order.OrderDate.AddHours(3), IsCompleted = true },
                    new TrackingStep { Step = "Đang giao hàng", Time = order.OrderDate.AddDays(1), IsCompleted = order.Status == "Đang giao hàng" || order.Status == "Đã giao hàng" },
                    new TrackingStep { Step = "Đã giao hàng", Time = order.Status == "Đã giao hàng" ? order.OrderDate.AddDays(2) : (DateTime?)null, IsCompleted = order.Status == "Đã giao hàng" }
                }
            };
        }
    }

    // ViewModels cho Order
    public class OrderHistoryViewModel
    {
        public IEnumerable<OrderViewModel> Orders { get; set; } = new List<OrderViewModel>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalOrders { get; set; } = 0;
    }

    public class OrderViewModel
    {
        public string OrderId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ItemCount { get; set; }
    }

    public class OrderDetailViewModel
    {
        public string OrderId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public IEnumerable<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
    }

    public class OrderItemViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }

    public class TrackingViewModel
    {
        public string OrderId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime EstimatedDelivery { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public IEnumerable<TrackingStep> TrackingSteps { get; set; } = new List<TrackingStep>();
    }

    public class TrackingStep
    {
        public string Step { get; set; } = string.Empty;
        public DateTime? Time { get; set; }
        public bool IsCompleted { get; set; }
    }
}