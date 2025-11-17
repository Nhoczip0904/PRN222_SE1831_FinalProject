using DAL.Entities;

namespace DAL.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<Order?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<IEnumerable<Order>> GetAllWithDetailsAsync();
    Task<IEnumerable<Order>> GetByBuyerIdAsync(int buyerId);
    Task<IEnumerable<Order>> GetBySellerIdAsync(int sellerId);
    Task<IEnumerable<Order>> GetByStatusAsync(string status);
    Task<(IEnumerable<Order> Orders, int TotalCount)> GetByBuyerIdWithPaginationAsync(int buyerId, int pageNumber, int pageSize);
    Task<(IEnumerable<Order> Orders, int TotalCount)> GetBySellerIdWithPaginationAsync(int sellerId, int pageNumber, int pageSize);
    Task<Order> CreateAsync(Order order);
    Task<Order> UpdateAsync(Order order);
    Task<bool> UpdateStatusAsync(int orderId, string status);
    Task<bool> UpdateShippingAddressAsync(int orderId, string shippingAddress);
    Task<bool> DeleteAsync(int id);
    Task<int> GetTotalCountAsync();
}
