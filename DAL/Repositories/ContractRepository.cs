using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly EvBatteryTrading2Context _context;

    public ContractRepository(EvBatteryTrading2Context context)
    {
        _context = context;
    }

    public async Task<Contract?> GetByIdAsync(int id)
    {
        return await _context.Contracts
            .Include(c => c.Order)
            .Include(c => c.Buyer)
            .Include(c => c.Seller)
            .Include(c => c.AdminApprover)
            .Include(c => c.Confirmations)
                .ThenInclude(cf => cf.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Contract?> GetByOrderIdAsync(int orderId)
    {
        return await _context.Contracts
            .Include(c => c.Order)
            .Include(c => c.Buyer)
            .Include(c => c.Seller)
            .Include(c => c.AdminApprover)
            .Include(c => c.Confirmations)
                .ThenInclude(cf => cf.User)
            .FirstOrDefaultAsync(c => c.OrderId == orderId);
    }

    public async Task<IEnumerable<Contract>> GetByUserIdAsync(int userId)
    {
        return await _context.Contracts
            .Where(c => c.BuyerId == userId || c.SellerId == userId)
            .Include(c => c.Order)
            .Include(c => c.Buyer)
            .Include(c => c.Seller)
            .Include(c => c.AdminApprover)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Contract>> GetPendingContractsAsync()
    {
        return await _context.Contracts
            .Where(c => c.Status == "confirmed" && !c.AdminApproved)
            .Include(c => c.Order)
            .Include(c => c.Buyer)
            .Include(c => c.Seller)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsByOrderIdAsync(int orderId)
    {
        return await _context.Contracts.AnyAsync(c => c.OrderId == orderId);
    }

    public async Task<Contract> CreateAsync(Contract contract)
    {
        contract.CreatedAt = DateTime.Now;
        contract.UpdatedAt = DateTime.Now;
        
        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();
        
        return contract;
    }

    public async Task<Contract> UpdateAsync(Contract contract)
    {
        contract.UpdatedAt = DateTime.Now;
        
        _context.Contracts.Update(contract);
        await _context.SaveChangesAsync();
        
        return contract;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null)
            return false;

        _context.Contracts.Remove(contract);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> GenerateContractNumberAsync(int orderId)
    {
        return await Task.FromResult($"HD{DateTime.Now:yyyyMMdd}{orderId:D6}");
    }

    public async Task<ContractConfirmation> AddConfirmationAsync(ContractConfirmation confirmation)
    {
        confirmation.CreatedAt = DateTime.Now;
        
        _context.ContractConfirmations.Add(confirmation);
        await _context.SaveChangesAsync();
        
        return confirmation;
    }
}
