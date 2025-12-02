using BestieToy.Models;

namespace BestieToy.ViewModels
{
    public class CartViewModel
    {
        public string CartId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public IEnumerable<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal Subtotal { get; set; } = 0;
        public decimal ShippingFee { get; set; } = 0;
        public decimal Tax { get; set; } = 0;
        public decimal Total { get; set; } = 0;
        public int ItemCount { get; set; } = 0;

        // User info for checkout
        public string? ShippingAddress { get; set; }
        public string? Phone { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }

    public class CartItemViewModel
    {
        public string CartItemId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
        public int StockQuantity { get; set; }
        public bool IsAvailable => StockQuantity > 0;
    }
}