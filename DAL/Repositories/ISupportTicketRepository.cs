using DAL.Entities;

namespace DAL.Repositories;

public interface ISupportTicketRepository
{
    Task<SupportTicket?> GetByIdAsync(int id);
    Task<SupportTicket?> GetByTicketNumberAsync(string ticketNumber);
    Task<IEnumerable<SupportTicket>> GetByUserIdAsync(int userId);
    Task<IEnumerable<SupportTicket>> GetAllAsync();
    Task<IEnumerable<SupportTicket>> GetByStatusAsync(string status);
    Task<IEnumerable<SupportTicket>> GetByAssignedAdminAsync(int adminId);
    Task<SupportTicket> CreateAsync(SupportTicket ticket);
    Task<SupportTicket> UpdateAsync(SupportTicket ticket);
    Task<bool> DeleteAsync(int id);
    Task<string> GenerateTicketNumberAsync();
}
