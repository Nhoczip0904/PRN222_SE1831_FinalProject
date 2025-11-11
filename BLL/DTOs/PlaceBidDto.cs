using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;

public class PlaceBidDto
{
    [Required]
    public int AuctionId { get; set; }

    [Required(ErrorMessage = "Số tiền đặt giá là bắt buộc")]
    [Range(0.01, 999999999999, ErrorMessage = "Số tiền phải lớn hơn 0")]
    public decimal BidAmount { get; set; }
}
