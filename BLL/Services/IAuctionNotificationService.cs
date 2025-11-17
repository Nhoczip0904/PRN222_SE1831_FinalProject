namespace BLL.Services;

public interface IAuctionNotificationService
{
    Task SendAuctionUpdateAsync(int auctionId, object data);
}
