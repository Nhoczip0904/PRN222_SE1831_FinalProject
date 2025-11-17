using BLL.DTOs;
using BLL.Services;
using DAL.Entities;
using DAL.Repositories;

namespace BLL.Services;

public class BidService : IBidService
{
    private readonly IBidRepository _bidRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly IWalletService _walletService;
    private readonly IAuctionNotificationService _notificationService;

    public BidService(IBidRepository bidRepository, IAuctionRepository auctionRepository, IWalletService walletService, IAuctionNotificationService notificationService)
    {
        _bidRepository = bidRepository;
        _auctionRepository = auctionRepository;
        _walletService = walletService;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<BidDto>> GetBidsByAuctionIdAsync(int auctionId)
    {
        var bids = await _bidRepository.GetByAuctionIdAsync(auctionId);
        return bids.Select(MapToBidDto);
    }

    public async Task<IEnumerable<BidDto>> GetBidsByBidderIdAsync(int bidderId)
    {
        var bids = await _bidRepository.GetByBidderIdAsync(bidderId);
        return bids.Select(MapToBidDto);
    }

    public async Task<BidDto?> GetWinningBidAsync(int auctionId)
    {
        var bid = await _bidRepository.GetWinningBidAsync(auctionId);
        return bid != null ? MapToBidDto(bid) : null;
    }

    public async Task<(bool Success, string Message)> PlaceBidAsync(int bidderId, PlaceBidDto placeBidDto)
    {
        // Get auction
        var auction = await _auctionRepository.GetByIdAsync(placeBidDto.AuctionId);
        
        if (auction == null)
        {
            return (false, "Đấu giá không tồn tại");
        }

        // Check auction status
        if (auction.Status != "active")
        {
            return (false, "Đấu giá đã kết thúc hoặc bị hủy");
        }

        // Check if auction has started
        if (auction.StartTime > DateTime.Now)
        {
            return (false, "Đấu giá chưa bắt đầu");
        }

        // Check if auction has ended
        if (auction.EndTime <= DateTime.Now)
        {
            return (false, "Đấu giá đã kết thúc");
        }

        // Check if bidder is the seller
        if (auction.SellerId == bidderId)
        {
            return (false, "Người bán không thể đặt giá cho sản phẩm của mình");
        }

        // Check bid amount
        var currentPrice = auction.CurrentPrice ?? auction.StartingPrice;
        if (placeBidDto.BidAmount <= currentPrice)
        {
            return (false, $"Giá đặt phải lớn hơn giá hiện tại ({currentPrice:N0} VND)");
        }

        // Check bid increment
        var minimumIncrement = auction.BidIncrement;
        var requiredMinimumBid = currentPrice + minimumIncrement;
        if (placeBidDto.BidAmount < requiredMinimumBid)
        {
            return (false, $"Giá đặt phải tăng ít nhất {minimumIncrement:N0} VND. Giá tối thiểu: {requiredMinimumBid:N0} VND");
        }

        // Check wallet balance
        var balanceCheck = await _walletService.CheckBalanceAsync(bidderId, placeBidDto.BidAmount);
        if (!balanceCheck.Success)
        {
            return (false, balanceCheck.Message);
        }

        // Get highest bid from this bidder
        var bidderBids = await _bidRepository.GetByAuctionIdAsync(placeBidDto.AuctionId);
        var bidderHighestBid = bidderBids.Where(b => b.BidderId == bidderId).OrderByDescending(b => b.BidAmount).FirstOrDefault();
        
        if (bidderHighestBid != null && placeBidDto.BidAmount <= bidderHighestBid.BidAmount)
        {
            return (false, $"Bạn đã đặt giá {bidderHighestBid.BidAmount:N0} VND. Giá mới phải cao hơn");
        }

        // Create bid
        var bid = new Bid
        {
            AuctionId = placeBidDto.AuctionId,
            BidderId = bidderId,
            BidAmount = placeBidDto.BidAmount,
            BidTime = DateTime.Now
        };

        await _bidRepository.CreateAsync(bid);

        // Update auction current price
        auction.CurrentPrice = placeBidDto.BidAmount;
        auction.UpdatedAt = DateTime.Now;
        await _auctionRepository.UpdateAsync(auction);

        // Send real-time update to all clients
        var auctionUpdateData = new
        {
            currentPrice = auction.CurrentPrice,
            totalBids = (await _bidRepository.GetByAuctionIdAsync(placeBidDto.AuctionId)).Count(),
            lastBidAmount = placeBidDto.BidAmount,
            lastBidTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
        };
        
        await _notificationService.SendAuctionUpdateAsync(placeBidDto.AuctionId, auctionUpdateData);

        return (true, "Đặt giá thành công");
    }

    private BidDto MapToBidDto(Bid bid)
    {
        return new BidDto
        {
            Id = bid.Id,
            AuctionId = bid.AuctionId,
            BidderId = bid.BidderId,
            BidderName = bid.Bidder?.FullName ?? "Unknown",
            BidAmount = bid.BidAmount,
            BidTime = bid.BidTime,
            IsWinning = bid.IsWinning
        };
    }
}
