using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;

public class CreateAuctionDto
{
    [Required(ErrorMessage = "Sản phẩm là bắt buộc")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Giá khởi điểm là bắt buộc")]
    [Range(0.01, 999999999999, ErrorMessage = "Giá khởi điểm phải lớn hơn 0")]
    public decimal StartingPrice { get; set; }

    [Range(0.01, 999999999999, ErrorMessage = "Giá dự trữ phải lớn hơn 0")]
    public decimal? ReservePrice { get; set; }

    [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
    public DateTime StartTime { get; set; }

    [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc")]
    public DateTime EndTime { get; set; }
}
