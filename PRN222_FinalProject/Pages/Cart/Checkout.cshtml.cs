using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Cart;

public class CheckoutModel : PageModel
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly IVNPayService _vnPayService;
    private readonly IWalletService _walletService;

    public CheckoutModel(ICartService cartService, IOrderService orderService, IVNPayService vnPayService, IWalletService walletService)
    {
        _cartService = cartService;
        _orderService = orderService;
        _vnPayService = vnPayService;
        _walletService = walletService;
    }

    [BindProperty]
    public CreateOrderDto CreateOrderDto { get; set; } = new CreateOrderDto();

    public UserDto? CurrentUser { get; set; }
    public List<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
    public decimal CurrentBalance { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        CurrentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        CartItems = _cartService.GetCart("ShoppingCart");

        if (!CartItems.Any())
        {
            return RedirectToPage("/Cart/Index");
        }

        // Get wallet balance
        var wallet = await _walletService.GetOrCreateWalletAsync(CurrentUser.Id);
        CurrentBalance = wallet.Balance;

        // Pre-fill shipping address
        CreateOrderDto.ShippingAddress = CurrentUser.Address ?? "";

        return Page();
    }

    public async Task<IActionResult> OnPostVNPayAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        CartItems = _cartService.GetCart("ShoppingCart");
        CurrentUser = currentUser;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!CartItems.Any())
        {
            TempData["ErrorMessage"] = "Giỏ hàng trống";
            return RedirectToPage("/Cart/Index");
        }

        // Save order info to session for later processing
        HttpContext.Session.SetObjectAsJson("PendingOrder", CreateOrderDto);

        // Create VNPay payment URL
        decimal totalAmount = CartItems.Sum(c => c.TotalPrice);
        string transactionId = $"ORDER_{currentUser.Id}_{DateTime.Now:yyyyMMddHHmmss}";
        string orderInfo = $"Thanh toan don hang - {currentUser.FullName}";
        string returnUrl = $"{Request.Scheme}://{Request.Host}/Payment/VNPayCallback";

        string paymentUrl = _vnPayService.CreatePaymentUrl(
            transactionId,
            totalAmount,
            orderInfo,
            returnUrl
        );

        return Redirect(paymentUrl);
    }

    public async Task<IActionResult> OnPostPlaceOrderAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        CartItems = _cartService.GetCart("ShoppingCart");
        CurrentUser = currentUser;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!CartItems.Any())
        {
            TempData["ErrorMessage"] = "Giỏ hàng trống";
            return RedirectToPage("/Cart/Index");
        }

        // Set payment method from form (wallet or vnpay only)
        string paymentMethod = Request.Form["PaymentMethod"].ToString();
        CreateOrderDto.PaymentMethod = paymentMethod == "wallet" ? "wallet" : "vnpay";

        var result = await _orderService.CreateOrderFromCartAsync(currentUser.Id, CreateOrderDto, CartItems);

        if (result.Success)
        {
            // Clear cart
            _cartService.ClearCart("ShoppingCart");
            
            TempData["SuccessMessage"] = "Đặt hàng thành công!";
            return RedirectToPage("/Orders/Index");
        }

        ModelState.AddModelError("", result.Message);
        return Page();
    }
}
