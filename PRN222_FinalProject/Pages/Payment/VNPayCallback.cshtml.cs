using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Payment;

public class VNPayCallbackModel : PageModel
{
    private readonly IVNPayService _vnPayService;
    private readonly IWalletService _walletService;
    private readonly IOrderService _orderService;

    public VNPayCallbackModel(
        IVNPayService vnPayService,
        IWalletService walletService,
        IOrderService orderService)
    {
        _vnPayService = vnPayService;
        _walletService = walletService;
        _orderService = orderService;
    }

    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string OrderInfo { get; set; } = string.Empty;
    public string PayDate { get; set; } = string.Empty;
    public bool IsWalletDeposit { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var queryParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
        
        if (!queryParams.ContainsKey("vnp_SecureHash"))
        {
            Success = false;
            Message = "Dữ liệu không hợp lệ";
            return Page();
        }

        string secureHash = queryParams["vnp_SecureHash"];
        
        // Validate signature
        bool isValidSignature = _vnPayService.ValidateSignature(queryParams, secureHash);
        
        if (!isValidSignature)
        {
            Success = false;
            Message = "Chữ ký không hợp lệ";
            return Page();
        }

        // Get response data
        string responseCode = queryParams.ContainsKey("vnp_ResponseCode") ? queryParams["vnp_ResponseCode"] : "";
        TransactionId = queryParams.ContainsKey("vnp_TxnRef") ? queryParams["vnp_TxnRef"] : "";
        string amountStr = queryParams.ContainsKey("vnp_Amount") ? queryParams["vnp_Amount"] : "0";
        Amount = decimal.Parse(amountStr) / 100; // VNPay trả về số tiền * 100
        OrderInfo = queryParams.ContainsKey("vnp_OrderInfo") ? queryParams["vnp_OrderInfo"] : "";
        PayDate = queryParams.ContainsKey("vnp_PayDate") ? queryParams["vnp_PayDate"] : "";

        // Check if wallet deposit or order payment
        IsWalletDeposit = TransactionId.StartsWith("WALLET_");

        if (responseCode == "00")
        {
            Success = true;
            Message = "Giao dịch thành công";

            var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
            if (currentUser == null)
            {
                Success = false;
                Message = "Phiên đăng nhập hết hạn";
                return Page();
            }

            if (IsWalletDeposit)
            {
                // Process wallet deposit
                var result = await _walletService.AddBalanceAsync(
                    currentUser.Id,
                    Amount,
                    $"Nạp tiền qua VNPay - {TransactionId}",
                    null,
                    "vnpay_deposit"
                );

                if (!result.Success)
                {
                    Success = false;
                    Message = result.Message;
                }
            }
            else
            {
                // Process order payment
                var pendingOrder = HttpContext.Session.GetObjectFromJson<CreateOrderDto>("PendingOrder");
                if (pendingOrder != null)
                {
                    // Set payment method to VNPay
                    pendingOrder.PaymentMethod = "VNPay";
                    
                    // Get cart items from session
                    var cartItems = HttpContext.Session.GetObjectFromJson<List<CartItemDto>>("ShoppingCart");
                    if (cartItems != null && cartItems.Any())
                    {
                        // Create order
                        var orderResult = await _orderService.CreateOrderFromCartAsync(
                            currentUser.Id,
                            pendingOrder,
                            cartItems
                        );

                        if (orderResult.Success)
                        {
                            // Clear cart and pending order
                            HttpContext.Session.Remove("ShoppingCart");
                            HttpContext.Session.Remove("PendingOrder");
                        }
                        else
                        {
                            Success = false;
                            Message = orderResult.Message;
                        }
                    }
                }
            }
        }
        else
        {
            Success = false;
            Message = GetResponseMessage(responseCode);
        }

        return Page();
    }

    private string GetResponseMessage(string responseCode)
    {
        return responseCode switch
        {
            "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
            "09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
            "10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
            "11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch.",
            "12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa.",
            "13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).",
            "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
            "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
            "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
            "75" => "Ngân hàng thanh toán đang bảo trì.",
            "79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định.",
            _ => "Giao dịch thất bại"
        };
    }
}
