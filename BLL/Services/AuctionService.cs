using BLL.DTOs;
using DAL.Entities;
using DAL.Repositories;

namespace BLL.Services;

public class AuctionService : IAuctionService
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IProductRepository _productRepository;
    private readonly IBidRepository _bidRepository;
    private readonly IWalletService _walletService;
    private readonly IOrderService _orderService;

    public AuctionService(IAuctionRepository auctionRepository, IProductRepository productRepository, IBidRepository bidRepository, IWalletService walletService, IOrderService orderService)
    {
        _auctionRepository = auctionRepository;
        _productRepository = productRepository;
        _bidRepository = bidRepository;
        _walletService = walletService;
        _orderService = orderService;
    }

    public async Task<AuctionDto?> GetAuctionByIdAsync(int id)
    {
        var auction = await _auctionRepository.GetByIdWithDetailsAsync(id);
        if (auction == null) return null;

        var bidCount = await _bidRepository.GetBidCountAsync(id);

        return MapToAuctionDto(auction, bidCount);
    }

    public async Task<IEnumerable<AuctionDto>> GetAllAuctionsAsync()
    {
        var auctions = await _auctionRepository.GetAllAsync();
        var auctionDtos = new List<AuctionDto>();

        foreach (var auction in auctions)
        {
            var bidCount = await _bidRepository.GetBidCountAsync(auction.Id);
            auctionDtos.Add(MapToAuctionDto(auction, bidCount));
        }

        return auctionDtos;
    }

    public async Task<IEnumerable<AuctionDto>> GetActiveAuctionsAsync()
    {
        var auctions = await _auctionRepository.GetActiveAuctionsAsync();
        var auctionDtos = new List<AuctionDto>();

        foreach (var auction in auctions)
        {
            var bidCount = await _bidRepository.GetBidCountAsync(auction.Id);
            auctionDtos.Add(MapToAuctionDto(auction, bidCount));
        }

        return auctionDtos;
    }

    public async Task<IEnumerable<AuctionDto>> GetPendingAuctionsAsync()
    {
        var auctions = await _auctionRepository.GetPendingAuctionsAsync();
        var auctionDtos = new List<AuctionDto>();

        foreach (var auction in auctions)
        {
            var bidCount = await _bidRepository.GetBidCountAsync(auction.Id);
            auctionDtos.Add(MapToAuctionDto(auction, bidCount));
        }

        return auctionDtos;
    }

    public async Task<IEnumerable<AuctionDto>> GetAuctionsBySellerIdAsync(int sellerId)
    {
        var auctions = await _auctionRepository.GetBySellerIdAsync(sellerId);
        var auctionDtos = new List<AuctionDto>();

        foreach (var auction in auctions)
        {
            var bidCount = await _bidRepository.GetBidCountAsync(auction.Id);
            auctionDtos.Add(MapToAuctionDto(auction, bidCount));
        }

        return auctionDtos;
    }

    public async Task<(IEnumerable<AuctionDto> Auctions, int TotalCount, int TotalPages)> GetAuctionsWithPaginationAsync(int pageNumber, int pageSize, string? status = null)
    {
        var result = await _auctionRepository.GetWithPaginationAsync(pageNumber, pageSize, status);
        var auctionDtos = new List<AuctionDto>();

        foreach (var auction in result.Auctions)
        {
            var bidCount = await _bidRepository.GetBidCountAsync(auction.Id);
            auctionDtos.Add(MapToAuctionDto(auction, bidCount));
        }

        var totalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);

        return (auctionDtos, result.TotalCount, totalPages);
    }

    public async Task<(bool Success, string Message, int? AuctionId)> CreateAuctionAsync(int sellerId, CreateAuctionDto createDto)
    {
        // Validate product exists and belongs to seller
        var product = await _productRepository.GetByIdAsync(createDto.ProductId);
        if (product == null)
        {
            return (false, "Sản phẩm không tồn tại", null);
        }

        if (product.SellerId != sellerId)
        {
            return (false, "Bạn không có quyền tạo đấu giá cho sản phẩm này", null);
        }

        // Validate times
        if (createDto.StartTime < DateTime.Now)
        {
            return (false, "Thời gian bắt đầu phải sau thời điểm hiện tại", null);
        }

        if (createDto.EndTime <= createDto.StartTime)
        {
            return (false, "Thời gian kết thúc phải sau thời gian bắt đầu", null);
        }

        // Validate prices
        if (createDto.StartingPrice <= 0)
        {
            return (false, "Giá khởi điểm phải lớn hơn 0", null);
        }

        if (createDto.ReservePrice.HasValue && createDto.ReservePrice <= createDto.StartingPrice)
        {
            return (false, "Giá dự trữ phải lớn hơn giá khởi điểm", null);
        }

        var auction = new Auction
        {
            ProductId = createDto.ProductId,
            SellerId = sellerId,
            StartingPrice = createDto.StartingPrice,
            CurrentPrice = createDto.StartingPrice,
            ReservePrice = createDto.ReservePrice,
            BidIncrement = createDto.BidIncrement,
            StartTime = createDto.StartTime,
            EndTime = createDto.EndTime,
            Status = "pending",
            ApprovalStatus = "pending"
        };

        var createdAuction = await _auctionRepository.CreateAsync(auction);

        // Lock product - cannot edit or delete while in auction
        product.IsActive = false;
        await _productRepository.UpdateAsync(product);

        return (true, "Tạo đấu giá thành công. Đang chờ phê duyệt từ admin/staff. Sản phẩm đã bị khóa trong thời gian chờ duyệt", createdAuction.Id);
    }

    public async Task<(bool Success, string Message)> CloseAuctionAsync(int auctionId, int adminId)
    {
        var auction = await _auctionRepository.GetByIdWithDetailsAsync(auctionId);
        
        if (auction == null)
        {
            return (false, "Đấu giá không tồn tại");
        }

        if (auction.Status != "active")
        {
            return (false, "Đấu giá đã được đóng hoặc hủy");
        }

        // Get winning bid
        var winningBid = await _bidRepository.GetWinningBidAsync(auctionId);
        
        if (winningBid == null)
        {
            return (false, "Không có lượt đặt giá nào");
        }

        // Check reserve price if set
        if (auction.ReservePrice.HasValue && winningBid.BidAmount < auction.ReservePrice.Value)
        {
            await _auctionRepository.CancelAuctionAsync(auctionId);
            return (false, "Giá đặt cao nhất không đạt giá dự trữ. Đấu giá đã bị hủy");
        }

        // Close auction and set winner
        await _auctionRepository.CloseAuctionAsync(auctionId, winningBid.BidderId);

        return (true, $"Đã chốt đấu giá. Người thắng: {winningBid.Bidder?.FullName}");
    }

    public async Task<(bool Success, string Message)> SelectWinnerAsync(int auctionId, int sellerId, int winnerId)
    {
        var auction = await _auctionRepository.GetByIdWithDetailsAsync(auctionId);
        
        if (auction == null)
        {
            return (false, "Đấu giá không tồn tại");
        }

        // Check seller permission
        if (auction.SellerId != sellerId)
        {
            return (false, "Bạn không có quyền chọn người thắng cho đấu giá này");
        }

        // Check auction status
        if (auction.Status == "closed")
        {
            return (false, "Đấu giá đã được chốt");
        }

        if (auction.Status == "cancelled")
        {
            return (false, "Đấu giá đã bị hủy");
        }

        // Seller can close auction anytime, no need to wait for end time
        // Get all bids
        var allBids = await _bidRepository.GetByAuctionIdAsync(auctionId);
        
        if (!allBids.Any())
        {
            return (false, "Chưa có ai đặt giá cho đấu giá này");
        }

        // Get winner's bid (must be the highest bidder)
        var highestBid = allBids.OrderByDescending(b => b.BidAmount).FirstOrDefault();
        
        if (highestBid == null || highestBid.BidderId != winnerId)
        {
            return (false, "Người được chọn không phải người đặt giá cao nhất");
        }

        var selectedBid = highestBid;

        if (selectedBid == null)
        {
            return (false, "Người dùng này chưa đặt giá trong đấu giá");
        }

        // Check winner's wallet balance
        var balanceCheck = await _walletService.CheckBalanceAsync(winnerId, selectedBid.BidAmount);
        if (!balanceCheck.Success)
        {
            // Cancel auction and unlock product since winner cannot pay
            await _auctionRepository.CancelAuctionAsync(auctionId);
            
            // Unlock product
            var product = await _productRepository.GetByIdAsync(auction.ProductId);
            if (product != null)
            {
                product.IsActive = true;
                await _productRepository.UpdateAsync(product);
            }
            
            return (false, $"Người thắng không đủ số dư. Đấu giá đã bị hủy và sản phẩm đã được mở khóa. {balanceCheck.Message}");
        }

        // Deduct money from winner's wallet (hold in system until delivery confirmed)
        var deductResult = await _walletService.DeductBalanceAsync(
            winnerId, 
            selectedBid.BidAmount, 
            $"Thanh toán đấu giá #{auctionId} - Chờ xác nhận giao hàng",
            auctionId,
            "auction_payment"
        );

        if (!deductResult.Success)
        {
            // Cancel auction and unlock product since payment failed
            await _auctionRepository.CancelAuctionAsync(auctionId);
            
            // Unlock product
            var product = await _productRepository.GetByIdAsync(auction.ProductId);
            if (product != null)
            {
                product.IsActive = true;
                await _productRepository.UpdateAsync(product);
            }
            
            return (false, $"Thanh toán thất bại. Đấu giá đã bị hủy và sản phẩm đã được mở khóa. {deductResult.Message}");
        }

        // Money will be split 75/25 after buyer confirms delivery
        // 75% to seller, 25% commission to system

        // Update auction
        await _auctionRepository.CloseAuctionAsync(auctionId, winnerId);

        // Get product details for order
        var auctionProduct = await _productRepository.GetByIdWithDetailsAsync(auction.ProductId);
        if (auctionProduct == null)
        {
            return (false, "Không tìm thấy sản phẩm");
        }

        // Mark product as sold
        auctionProduct.IsSold = true;
        auctionProduct.UpdatedAt = DateTime.Now;
        await _productRepository.UpdateAsync(auctionProduct);

        // Create order automatically with auction price
        var createOrderDto = new CreateOrderDto
        {
            ShippingAddress = "Địa chỉ từ đấu giá - Vui lòng cập nhật",
            PaymentMethod = "auction", // Special payment method for auction
            Note = $"Đơn hàng từ đấu giá #{auctionId}. Đã thanh toán {selectedBid.BidAmount:N0} VND qua ví."
        };

        var cartItems = new List<CartItemDto>
        {
            new CartItemDto
            {
                ProductId = auction.ProductId,
                ProductName = auctionProduct.Name,
                Price = selectedBid.BidAmount, // Use actual auction price
                Quantity = 1,
                Image = auctionProduct.Images?.Split(',').FirstOrDefault(),
                SellerId = auction.SellerId,
                SellerName = auctionProduct.Seller?.FullName ?? "Unknown",
                IsAvailable = true
            }
        };

        Console.WriteLine($"[AuctionService] ===== CREATING ORDER FOR AUCTION =====");
        Console.WriteLine($"[AuctionService] AuctionId: {auctionId}");
        Console.WriteLine($"[AuctionService] WinnerId (BuyerId): {winnerId}");
        Console.WriteLine($"[AuctionService] ProductId: {cartItems[0].ProductId}");
        Console.WriteLine($"[AuctionService] ProductName: {cartItems[0].ProductName}");
        Console.WriteLine($"[AuctionService] Price: {cartItems[0].Price}");
        Console.WriteLine($"[AuctionService] Quantity: {cartItems[0].Quantity}");
        Console.WriteLine($"[AuctionService] TotalPrice: {cartItems[0].TotalPrice}");
        Console.WriteLine($"[AuctionService] SellerId: {cartItems[0].SellerId}");
        Console.WriteLine($"[AuctionService] PaymentMethod: {createOrderDto.PaymentMethod}");
        
        var orderResult = await _orderService.CreateOrderFromCartAsync(winnerId, createOrderDto, cartItems);
        
        Console.WriteLine($"[AuctionService] Order creation result: Success={orderResult.Success}, Message={orderResult.Message}, OrderId={orderResult.OrderId}");
        
        if (!orderResult.Success)
        {
            // Refund if order creation fails
            Console.WriteLine($"[AuctionService] Order creation failed, refunding {selectedBid.BidAmount} to user {winnerId}");
            await _walletService.AddBalanceAsync(
                winnerId,
                selectedBid.BidAmount,
                $"Hoàn tiền đấu giá #{auctionId} - Tạo đơn hàng thất bại",
                auctionId,
                "auction_refund"
            );
            return (false, $"Tạo đơn hàng thất bại: {orderResult.Message}");
        }
        
        // Auto-confirm order from seller side (skip pending status)
        if (orderResult.OrderId.HasValue)
        {
            Console.WriteLine($"[AuctionService] Auto-confirming order #{orderResult.OrderId.Value}");
            await _orderService.UpdateOrderStatusAsync(orderResult.OrderId.Value, "confirmed");
        }
        else
        {
            Console.WriteLine($"[AuctionService] WARNING: Order created but OrderId is null!");
        }

        return (true, "Đã chốt đấu giá và tạo đơn hàng thành công");
    }

    public async Task<(bool Success, string Message)> CloseAuctionWithHighestBidderAsync(int auctionId, int sellerId)
    {
        var auction = await _auctionRepository.GetByIdWithDetailsAsync(auctionId);
        
        if (auction == null)
        {
            return (false, "Đấu giá không tồn tại");
        }

        // Check seller permission
        if (auction.SellerId != sellerId)
        {
            return (false, "Bạn không có quyền đóng đấu giá này");
        }

        // Check auction status
        if (auction.Status == "closed")
        {
            return (false, "Đấu giá đã được chốt");
        }

        if (auction.Status == "cancelled")
        {
            return (false, "Đấu giá đã bị hủy");
        }

        // Get highest bidder
        var allBids = await _bidRepository.GetByAuctionIdAsync(auctionId);
        
        if (!allBids.Any())
        {
            return (false, "Chưa có ai đặt giá cho đấu giá này");
        }

        var highestBid = allBids.OrderByDescending(b => b.BidAmount).FirstOrDefault();
        
        if (highestBid == null)
        {
            return (false, "Không tìm thấy người đặt giá");
        }

        // Use SelectWinnerAsync to process the winner
        return await SelectWinnerAsync(auctionId, sellerId, highestBid.BidderId);
    }

    public async Task<(bool Success, string Message)> CancelAuctionAsync(int auctionId, int sellerId)
    {
        var auction = await _auctionRepository.GetByIdAsync(auctionId);
        
        if (auction == null)
        {
            return (false, "Đấu giá không tồn tại");
        }

        if (auction.SellerId != sellerId)
        {
            return (false, "Bạn không có quyền hủy đấu giá này");
        }

        if (auction.Status != "active")
        {
            return (false, "Đấu giá đã được đóng hoặc hủy");
        }

        // Check if auction has started
        if (auction.StartTime <= DateTime.Now)
        {
            return (false, "Không thể hủy đấu giá đã bắt đầu");
        }

        await _auctionRepository.CancelAuctionAsync(auctionId);

        return (true, "Đã hủy đấu giá");
    }

    public async Task ProcessExpiredAuctionsAsync()
    {
        var expiredAuctions = await _auctionRepository.GetExpiredAuctionsAsync();

        foreach (var auction in expiredAuctions)
        {
            var winningBid = await _bidRepository.GetWinningBidAsync(auction.Id);
            
            if (winningBid != null)
            {
                // Check reserve price
                if (auction.ReservePrice.HasValue && winningBid.BidAmount < auction.ReservePrice.Value)
                {
                    await _auctionRepository.CancelAuctionAsync(auction.Id);
                }
                else
                {
                    await _auctionRepository.CloseAuctionAsync(auction.Id, winningBid.BidderId);
                }
            }
            else
            {
                // No bids, cancel auction
                await _auctionRepository.CancelAuctionAsync(auction.Id);
            }
        }
    }

    public async Task<(bool Success, string Message)> ApproveAuctionAsync(int auctionId, int approvedById)
    {
        var auction = await _auctionRepository.GetByIdAsync(auctionId);
        
        if (auction == null)
        {
            return (false, "Đấu giá không tồn tại");
        }

        if (auction.ApprovalStatus != "pending")
        {
            return (false, "Đấu giá đã được phê duyệt hoặc từ chối");
        }

        // Update auction
        auction.ApprovalStatus = "approved";
        auction.ApprovedById = approvedById;
        auction.ApprovalReason = null;
        auction.Status = "active"; // Activate auction
        auction.UpdatedAt = DateTime.Now;

        await _auctionRepository.UpdateAsync(auction);

        return (true, "Đã phê duyệt đấu giá thành công");
    }

    public async Task<(bool Success, string Message)> RejectAuctionAsync(int auctionId, int approvedById, string reason)
    {
        var auction = await _auctionRepository.GetByIdAsync(auctionId);
        
        if (auction == null)
        {
            return (false, "Đấu giá không tồn tại");
        }

        if (auction.ApprovalStatus != "pending")
        {
            return (false, "Đấu giá đã được phê duyệt hoặc từ chối");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return (false, "Vui lòng cung cấp lý do từ chối");
        }

        // Update auction
        auction.ApprovalStatus = "cancelled";
        auction.ApprovedById = approvedById;
        auction.ApprovalReason = reason;
        auction.Status = "cancelled";
        auction.UpdatedAt = DateTime.Now;

        await _auctionRepository.UpdateAsync(auction);

        // Unlock product
        var product = await _productRepository.GetByIdAsync(auction.ProductId);
        if (product != null)
        {
            product.IsActive = true;
            await _productRepository.UpdateAsync(product);
        }

        return (true, "Đã từ chối đấu giá");
    }

    private AuctionDto MapToAuctionDto(Auction auction, int bidCount)
    {
        var firstImage = auction.Product?.Images?.Split(',').FirstOrDefault();

        return new AuctionDto
        {
            Id = auction.Id,
            ProductId = auction.ProductId,
            ProductName = auction.Product?.Name ?? "Unknown",
            ProductImage = firstImage,
            SellerId = auction.SellerId,
            SellerName = auction.Seller?.FullName ?? "Unknown",
            StartingPrice = auction.StartingPrice,
            CurrentPrice = auction.CurrentPrice,
            ReservePrice = auction.ReservePrice,
            BidIncrement = auction.BidIncrement,
            StartTime = auction.StartTime,
            EndTime = auction.EndTime,
            Status = auction.Status,
            ApprovalStatus = auction.ApprovalStatus,
            ApprovedById = auction.ApprovedById,
            ApprovedByName = auction.ApprovedBy?.FullName,
            ApprovalReason = auction.ApprovalReason,
            WinnerId = auction.WinnerId,
            WinnerName = auction.Winner?.FullName,
            TotalBids = bidCount,
            CreatedAt = auction.CreatedAt
        };
    }
}
