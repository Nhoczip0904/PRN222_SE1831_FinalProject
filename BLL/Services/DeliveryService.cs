using DAL.Entities;
using DAL.Repositories;

namespace BLL.Services;

public interface IDeliveryService
{
    Task<(bool Success, string Message)> ConfirmDeliveryAsync(int orderId, int buyerId, string? notes);
    Task<bool> CanConfirmDeliveryAsync(int orderId, int buyerId);
    Task<Order?> GetOrderForDeliveryConfirmationAsync(int orderId, int buyerId);
}

public class DeliveryService : IDeliveryService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly ISystemRevenueRepository _systemRevenueRepository;

    public DeliveryService(
        IOrderRepository orderRepository, 
        IWalletRepository walletRepository, 
        IWalletTransactionRepository transactionRepository,
        ISystemRevenueRepository systemRevenueRepository)
    {
        _orderRepository = orderRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _systemRevenueRepository = systemRevenueRepository;
    }

    public async Task<(bool Success, string Message)> ConfirmDeliveryAsync(int orderId, int buyerId, string? notes)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);

        if (order == null)
        {
            return (false, "Đơn hàng không tồn tại");
        }

        if (order.BuyerId != buyerId)
        {
            return (false, "Bạn không có quyền xác nhận đơn hàng này");
        }

        if (order.Status != "shipped")
        {
            return (false, "Đơn hàng chưa được giao hoặc đã được xác nhận rồi");
        }

        if (order.Status == "delivered")
        {
            return (false, "Đơn hàng đã được xác nhận nhận hàng rồi");
        }

        // Update order status
        order.Status = "delivered";
        order.UpdatedAt = DateTime.Now;
        // Note: Delivery notes stored in order.Note field

        await _orderRepository.UpdateAsync(order);

        // Release funds to seller (chuyển tiền cho người bán)
        try
        {
            await ReleaseFundsToSellerAsync(order);
        }
        catch (Exception ex)
        {
            // Log error but don't fail the delivery confirmation
            Console.WriteLine($"Lỗi chuyển tiền cho người bán: {ex.Message}");
        }

        return (true, "Đã xác nhận nhận hàng thành công. Tiền đã được chuyển cho người bán.");
    }

    public async Task<bool> CanConfirmDeliveryAsync(int orderId, int buyerId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null) return false;
        if (order.BuyerId != buyerId) return false;
        if (order.Status != "shipped") return false;
        if (order.Status == "delivered") return false;

        return true;
    }

    public async Task<Order?> GetOrderForDeliveryConfirmationAsync(int orderId, int buyerId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order != null && order.BuyerId != buyerId)
        {
            return null;
        }
        return order;
    }

    private async Task ReleaseFundsToSellerAsync(Order order)
    {
        // Tìm wallet của seller
        var sellerWallet = await _walletRepository.GetByUserIdAsync(order.SellerId ?? 0);

        if (sellerWallet == null)
        {
            throw new Exception("Không tìm thấy ví của người bán");
        }

        // Calculate seller amount (75% after 25% commission)
        const decimal COMMISSION_RATE = 0.25m;
        var sellerAmount = order.TotalAmount * (1 - COMMISSION_RATE);
        var commissionAmount = order.TotalAmount * COMMISSION_RATE;

        // Cộng tiền vào ví seller (75% of order total)
        sellerWallet.Balance += sellerAmount;

        // Tạo transaction record (chỉ hiển thị số tiền nhận được 75%)
        var transaction = new WalletTransaction
        {
            WalletId = sellerWallet.Id,
            TransactionType = "payment",
            Amount = sellerAmount,
            BalanceAfter = sellerWallet.Balance,
            Description = $"Thanh toán từ đơn hàng #{order.Id}",
            ReferenceId = order.Id,
            ReferenceType = "order",
            CreatedAt = DateTime.Now
        };

        await _walletRepository.UpdateAsync(sellerWallet);
        await _transactionRepository.CreateAsync(transaction);

        // Record system commission (25%)
        var systemRevenue = new SystemRevenue
        {
            OrderId = order.Id,
            OrderAmount = order.TotalAmount,
            CommissionRate = COMMISSION_RATE,
            CommissionAmount = commissionAmount,
            CreatedAt = DateTime.Now
        };
        await _systemRevenueRepository.CreateAsync(systemRevenue);
    }
}
