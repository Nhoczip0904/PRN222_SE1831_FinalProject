using BLL.DTOs;

namespace BLL.Services;

public interface IOrderService
{
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<IEnumerable<OrderDto>> GetOrdersByBuyerIdAsync(int buyerId);
    Task<IEnumerable<OrderDto>> GetOrdersBySellerIdAsync(int sellerId);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<(IEnumerable<OrderDto> Orders, int TotalCount, int TotalPages)> GetOrdersByBuyerIdWithPaginationAsync(int buyerId, int pageNumber, int pageSize);
    Task<(IEnumerable<OrderDto> Orders, int TotalCount, int TotalPages)> GetOrdersBySellerIdWithPaginationAsync(int sellerId, int pageNumber, int pageSize);
    Task<(bool Success, string Message, int? OrderId)> CreateOrderFromCartAsync(int buyerId, CreateOrderDto createDto, List<CartItemDto> cartItems);
    Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, string status, int userId, string userRole);
    Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, string status);
    Task<(bool Success, string Message)> UpdateShippingAddressAsync(int orderId, string shippingAddress);
    Task<(bool Success, string Message)> CancelOrderAsync(int orderId, int userId);
    Task<ContractDto?> GetContractDetailsAsync(int orderId);
}
