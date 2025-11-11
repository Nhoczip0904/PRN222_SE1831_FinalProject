using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public interface ISystemRevenueRepository
{
    Task<SystemRevenue?> GetByIdAsync(int id);
    Task<IEnumerable<SystemRevenue>> GetAllAsync();
    Task<SystemRevenue> CreateAsync(SystemRevenue systemRevenue);
    Task<decimal> GetTotalRevenueAsync();
}

public class SystemRevenueRepository : ISystemRevenueRepository
{
    private readonly EvBatteryTrading2Context _context;

    public SystemRevenueRepository(EvBatteryTrading2Context context)
    {
        _context = context;
    }

    public async Task<SystemRevenue?> GetByIdAsync(int id)
    {
        return await _context.SystemRevenues.FindAsync(id);
    }

    public async Task<IEnumerable<SystemRevenue>> GetAllAsync()
    {
        return await _context.SystemRevenues
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<SystemRevenue> CreateAsync(SystemRevenue systemRevenue)
    {
        _context.SystemRevenues.Add(systemRevenue);
        await _context.SaveChangesAsync();
        return systemRevenue;
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await _context.SystemRevenues.SumAsync(r => r.CommissionAmount);
    }
}
