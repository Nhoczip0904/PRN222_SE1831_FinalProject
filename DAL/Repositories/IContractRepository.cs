using DAL.Entities;

namespace DAL.Repositories;

public interface IContractRepository
{
    Task<Contract?> GetByIdAsync(int id);
    Task<Contract?> GetByOrderIdAsync(int orderId);
    Task<IEnumerable<Contract>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Contract>> GetPendingContractsAsync();
    Task<bool> ExistsByOrderIdAsync(int orderId);
    Task<Contract> CreateAsync(Contract contract);
    Task<Contract> UpdateAsync(Contract contract);
    Task<bool> DeleteAsync(int id);
    Task<string> GenerateContractNumberAsync(int orderId);
    Task<ContractConfirmation> AddConfirmationAsync(ContractConfirmation confirmation);
}
