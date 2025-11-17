using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.DTOs;
using BLL.Helpers;

namespace PRN222_FinalProject.Pages.Orders
{
    public class UpdateAddressModel : PageModel
    {
        private readonly IOrderService _orderService;

        public UpdateAddressModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [BindProperty]
        public string ShippingAddress { get; set; } = string.Empty;

        public OrderDto? Order { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }

        public async Task<IActionResult> OnGetAsync(int orderId)
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Get order details
            Order = await _orderService.GetOrderByIdAsync(orderId);
            
            if (Order == null)
            {
                Message = "Đơn hàng không tồn tại";
                return Page();
            }

            // Check if this order belongs to current user and is from auction
            if (Order.BuyerId != currentUser.Id || Order.PaymentMethod != "auction")
            {
                Message = "Bạn không có quyền cập nhật địa chỉ cho đơn hàng này";
                return Page();
            }

            // Check if address can be updated (only for pending/confirmed orders)
            if (Order.Status != "pending" && Order.Status != "confirmed")
            {
                Message = "Đơn hàng đã được xử lý, không thể cập nhật địa chỉ";
                return Page();
            }

            // Pre-fill current address
            ShippingAddress = Order.ShippingAddress;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int orderId)
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            if (string.IsNullOrWhiteSpace(ShippingAddress))
            {
                Message = "Vui lòng nhập địa chỉ giao hàng";
                return Page();
            }

            // Get order to verify ownership
            Order = await _orderService.GetOrderByIdAsync(orderId);
            
            if (Order == null)
            {
                Message = "Đơn hàng không tồn tại";
                return Page();
            }

            if (Order.BuyerId != currentUser.Id || Order.PaymentMethod != "auction")
            {
                Message = "Bạn không có quyền cập nhật địa chỉ cho đơn hàng này";
                return Page();
            }

            if (Order.Status != "pending" && Order.Status != "confirmed")
            {
                Message = "Đơn hàng đã được xử lý, không thể cập nhật địa chỉ";
                return Page();
            }

            // Update shipping address
            var result = await _orderService.UpdateShippingAddressAsync(orderId, ShippingAddress);
            
            if (result.Success)
            {
                IsSuccess = true;
                Message = "Cập nhật địa chỉ giao hàng thành công!";
                
                // Refresh order data
                Order = await _orderService.GetOrderByIdAsync(orderId);
            }
            else
            {
                Message = result.Message;
            }

            return Page();
        }
    }
}
