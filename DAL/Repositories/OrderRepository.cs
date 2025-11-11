using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly EvBatteryTrading2Context _context;

    public OrderRepository(EvBatteryTrading2Context context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Seller)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Transactions)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Seller)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetAllWithDetailsAsync()
    {
        return await _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Seller)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByBuyerIdAsync(int buyerId)
    {
        return await _context.Orders
            .Include(o => o.Seller)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.BuyerId == buyerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetBySellerIdAsync(int sellerId)
    {
        return await _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.SellerId == sellerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(string status)
    {
        return await _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Seller)
            .Include(o => o.OrderItems)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetByBuyerIdWithPaginationAsync(int buyerId, int pageNumber, int pageSize)
    {
        var query = _context.Orders
            .Include(o => o.Seller)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.BuyerId == buyerId)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync();
        var orders = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, totalCount);
    }

    public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetBySellerIdWithPaginationAsync(int sellerId, int pageNumber, int pageSize)
    {
        var query = _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.SellerId == sellerId)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync();
        var orders = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, totalCount);
    }

    public async Task<Order> CreateAsync(Order order)
    {
        order.CreatedAt = DateTime.Now;
        order.UpdatedAt = DateTime.Now;
        order.Status = "pending";

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        order.UpdatedAt = DateTime.Now;

        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<bool> UpdateStatusAsync(int orderId, string status)
    {
        var order = await GetByIdAsync(orderId);
        if (order == null)
            return false;

        order.Status = status;
        order.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var order = await GetByIdAsync(id);
        if (order == null)
            return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Orders.CountAsync();
    }
}
