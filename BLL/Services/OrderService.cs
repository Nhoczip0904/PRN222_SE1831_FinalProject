using BLL.DTOs;
using DAL;
using DAL.Entities;
using DAL.Repositories;

namespace BLL.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IContractService _contractService;
    private readonly IWalletService _walletService;
    private readonly EvBatteryTrading2Context _context;
    private const decimal COMMISSION_RATE = 0.25m; // 25% commission

    public OrderService(IOrderRepository orderRepository, IProductRepository productRepository, IContractService contractService, IWalletService walletService, EvBatteryTrading2Context context)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _contractService = contractService;
        _walletService = walletService;
        _context = context;
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(id);
        return order != null ? MapToOrderDto(order) : null;
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByBuyerIdAsync(int buyerId)
    {
        var orders = await _orderRepository.GetByBuyerIdAsync(buyerId);
        return orders.Select(MapToOrderDto);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersBySellerIdAsync(int sellerId)
    {
        var orders = await _orderRepository.GetBySellerIdAsync(sellerId);
        return orders.Select(MapToOrderDto);
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllWithDetailsAsync();
        return orders.Select(MapToOrderDto);
    }

    public async Task<(IEnumerable<OrderDto> Orders, int TotalCount, int TotalPages)> GetOrdersByBuyerIdWithPaginationAsync(int buyerId, int pageNumber, int pageSize)
    {
        var result = await _orderRepository.GetByBuyerIdWithPaginationAsync(buyerId, pageNumber, pageSize);
        var orderDtos = result.Orders.Select(MapToOrderDto);
        var totalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);

        return (orderDtos, result.TotalCount, totalPages);
    }

    public async Task<(IEnumerable<OrderDto> Orders, int TotalCount, int TotalPages)> GetOrdersBySellerIdWithPaginationAsync(int sellerId, int pageNumber, int pageSize)
    {
        var result = await _orderRepository.GetBySellerIdWithPaginationAsync(sellerId, pageNumber, pageSize);
        var orderDtos = result.Orders.Select(MapToOrderDto);
        var totalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);

        return (orderDtos, result.TotalCount, totalPages);
    }

    public async Task<(bool Success, string Message, int? OrderId)> CreateOrderFromCartAsync(int buyerId, CreateOrderDto createDto, List<CartItemDto> cartItems)
    {
        Console.WriteLine($"[OrderService] CreateOrderFromCartAsync called - BuyerId: {buyerId}, PaymentMethod: {createDto.PaymentMethod}");
        
        if (cartItems == null || !cartItems.Any())
        {
            Console.WriteLine($"[OrderService] ERROR: Cart is empty");
            return (false, "Giỏ hàng trống", null);
        }

        // Calculate total amount
        var totalAmount = cartItems.Sum(i => i.TotalPrice);
        Console.WriteLine($"[OrderService] Total amount: {totalAmount}, Items count: {cartItems.Count}");

        // Skip wallet check for auction orders (already paid)
        bool isAuctionOrder = createDto.PaymentMethod == "auction";
        Console.WriteLine($"[OrderService] Is auction order: {isAuctionOrder}");
        
        if (!isAuctionOrder)
        {
            // Check buyer wallet balance
            var buyerWallet = await _walletService.GetOrCreateWalletAsync(buyerId);
            if (buyerWallet.Balance < totalAmount)
            {
                return (false, $"Số dư ví không đủ. Cần {totalAmount:N0} đ, hiện có {buyerWallet.Balance:N0} đ. Vui lòng nạp thêm tiền.", null);
            }
        }

        // Group items by seller
        var itemsBySeller = cartItems.GroupBy(c => c.SellerId);

        // Create separate order for each seller
        var createdOrderIds = new List<int>();

        foreach (var sellerGroup in itemsBySeller)
        {
            var sellerId = sellerGroup.Key;
            var items = sellerGroup.ToList();

            // Verify all products are still available, not sold, and have enough stock
            // Skip validation for auction orders (product already reserved)
            if (!isAuctionOrder)
            {
                foreach (var item in items)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product == null || product.IsActive == false || product.IsSold == true)
                    {
                        return (false, $"Sản phẩm '{item.ProductName}' không còn khả dụng hoặc đã được bán", null);
                    }
                    
                    if (product.Quantity < item.Quantity)
                    {
                        return (false, $"Sản phẩm '{item.ProductName}' chỉ còn {product.Quantity} trong kho", null);
                    }
                }
            }

            // Calculate order total
            var orderTotal = items.Sum(i => i.TotalPrice);

            // Deduct from buyer wallet (skip for auction orders - already paid)
            if (!isAuctionOrder)
            {
                var deductResult = await _walletService.DeductBalanceAsync(buyerId, orderTotal, $"Thanh toán đơn hàng");
                if (!deductResult.Success)
                {
                    return (false, deductResult.Message, null);
                }
            }

            // Create order
            var order = new Order
            {
                BuyerId = buyerId,
                SellerId = sellerId,
                OrderDate = DateTime.Now,
                TotalAmount = orderTotal,
                Status = "pending",
                ShippingAddress = createDto.ShippingAddress,
                PaymentMethod = createDto.PaymentMethod,
                Note = createDto.Note,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var createdOrder = await _orderRepository.CreateAsync(order);

            // Create order items and mark products as sold
            foreach (var item in items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = createdOrder.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    CreatedAt = DateTime.Now
                };

                createdOrder.OrderItems.Add(orderItem);

                // Deduct quantity and mark as sold if quantity reaches 0
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity -= item.Quantity;
                    
                    // Mark as sold if no stock left
                    if (product.Quantity <= 0)
                    {
                        product.IsSold = true;
                        product.IsActive = false;
                        product.Quantity = 0; // Ensure it doesn't go negative
                    }
                    
                    await _productRepository.UpdateAsync(product);
                }
            }

            await _orderRepository.UpdateAsync(createdOrder);

            // Calculate and record commission (25%)
            var commissionAmount = orderTotal * COMMISSION_RATE;
            var sellerAmount = orderTotal - commissionAmount;

            var revenue = new SystemRevenue
            {
                OrderId = createdOrder.Id,
                OrderAmount = orderTotal,
                CommissionRate = COMMISSION_RATE,
                CommissionAmount = commissionAmount,
                CreatedAt = DateTime.Now
            };
            _context.SystemRevenues.Add(revenue);
            await _context.SaveChangesAsync();

            // Tự động tạo hợp đồng số
            try
            {
                Console.WriteLine($"[OrderService] Creating contract for order #{createdOrder.Id}");
                await _contractService.CreateContractFromOrderAsync(createdOrder.Id);
                Console.WriteLine($"[OrderService] Contract created successfully for order #{createdOrder.Id}");
            }
            catch (Exception ex)
            {
                // Log error nhưng không fail order creation
                Console.WriteLine($"[OrderService] ERROR creating contract: {ex.Message}");
                Console.WriteLine($"[OrderService] Stack trace: {ex.StackTrace}");
            }

            createdOrderIds.Add(createdOrder.Id);
            Console.WriteLine($"[OrderService] Order #{createdOrder.Id} added to result list");
        }

        Console.WriteLine($"[OrderService] SUCCESS: Created {createdOrderIds.Count} order(s). First OrderId: {createdOrderIds.FirstOrDefault()}");
        return (true, $"Đã tạo {createdOrderIds.Count} đơn hàng thành công. Tiền đã được trừ từ ví của bạn.", createdOrderIds.FirstOrDefault());
    }

    public async Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, string status, int userId, string userRole)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        
        if (order == null)
        {
            return (false, "Đơn hàng không tồn tại");
        }

        // Check permissions
        if (userRole != "admin" && order.SellerId != userId && order.BuyerId != userId)
        {
            return (false, "Bạn không có quyền cập nhật đơn hàng này");
        }

        // Validate status transitions
        var validStatuses = new[] { "pending", "confirmed", "shipped", "delivered", "cancelled" };
        if (!validStatuses.Contains(status))
        {
            return (false, "Trạng thái không hợp lệ");
        }

        await _orderRepository.UpdateStatusAsync(orderId, status);

        return (true, "Đã cập nhật trạng thái đơn hàng");
    }

    public async Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, string status)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        
        if (order == null)
        {
            return (false, "Đơn hàng không tồn tại");
        }

        // Validate status transitions
        var validStatuses = new[] { "pending", "confirmed", "shipped", "delivered", "cancelled" };
        if (!validStatuses.Contains(status))
        {
            return (false, "Trạng thái không hợp lệ");
        }

        await _orderRepository.UpdateStatusAsync(orderId, status);

        return (true, $"Đã cập nhật trạng thái đơn hàng thành '{status}'");
    }

    public async Task<(bool Success, string Message)> CancelOrderAsync(int orderId, int userId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        
        if (order == null)
        {
            return (false, "Đơn hàng không tồn tại");
        }

        if (order.BuyerId != userId && order.SellerId != userId)
        {
            return (false, "Bạn không có quyền hủy đơn hàng này");
        }

        if (order.Status == "delivered")
        {
            return (false, "Không thể hủy đơn hàng đã giao");
        }

        await _orderRepository.UpdateStatusAsync(orderId, "cancelled");
        return (true, "Đã hủy đơn hàng thành công");
    }

    public async Task<(bool Success, string Message)> UpdateShippingAddressAsync(int orderId, string shippingAddress)
    {
        if (string.IsNullOrWhiteSpace(shippingAddress))
        {
            return (false, "Địa chỉ giao hàng không được để trống");
        }

        var order = await _orderRepository.GetByIdAsync(orderId);
        
        if (order == null)
        {
            return (false, "Đơn hàng không tồn tại");
        }

        // Only allow address update for pending or confirmed orders
        if (order.Status != "pending" && order.Status != "confirmed")
        {
            return (false, "Không thể cập nhật địa chỉ cho đơn hàng đã được xử lý");
        }

        await _orderRepository.UpdateShippingAddressAsync(orderId, shippingAddress);
        return (true, "Cập nhật địa chỉ giao hàng thành công");
    }

    public async Task<ContractDto?> GetContractDetailsAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order == null)
            return null;

        return MapToContractDto(order);
    }

    private OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            BuyerId = order.BuyerId,
            BuyerName = order.Buyer?.FullName,
            SellerId = order.SellerId,
            SellerName = order.Seller?.FullName,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            ShippingAddress = order.ShippingAddress,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            OrderItems = order.OrderItems?.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? "Unknown",
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                CreatedAt = oi.CreatedAt
            }).ToList() ?? new List<OrderItemDto>()
        };
    }

    private ContractDto MapToContractDto(Order order)
    {
        return new ContractDto
        {
            Id = order.Id,
            BuyerId = order.BuyerId,
            BuyerName = order.Buyer?.FullName,
            BuyerEmail = order.Buyer?.Email,
            BuyerPhone = order.Buyer?.Phone,
            BuyerAddress = order.Buyer?.Address,
            SellerId = order.SellerId,
            SellerName = order.Seller?.FullName,
            SellerEmail = order.Seller?.Email,
            SellerPhone = order.Seller?.Phone,
            SellerAddress = order.Seller?.Address,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            ShippingAddress = order.ShippingAddress,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            OrderDate = order.OrderDate,
            Note = order.Note,
            OrderItems = order.OrderItems?.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? "Unknown",
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                CreatedAt = oi.CreatedAt
            }).ToList() ?? new List<OrderItemDto>()
        };
    }
}
