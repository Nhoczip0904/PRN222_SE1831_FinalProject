using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class SupportTicketRepository : ISupportTicketRepository
{
    private readonly EvBatteryTrading2Context _context;

    public SupportTicketRepository(EvBatteryTrading2Context context)
    {
        _context = context;
    }

    public async Task<SupportTicket?> GetByIdAsync(int id)
    {
        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedAdmin)
            .Include(t => t.Messages)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<SupportTicket?> GetByTicketNumberAsync(string ticketNumber)
    {
        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedAdmin)
            .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber);
    }

    public async Task<IEnumerable<SupportTicket>> GetByUserIdAsync(int userId)
    {
        return await _context.SupportTickets
            .Where(t => t.UserId == userId)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedAdmin)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> GetAllAsync()
    {
        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedAdmin)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> GetByStatusAsync(string status)
    {
        return await _context.SupportTickets
            .Where(t => t.Status == status)
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedAdmin)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> GetByAssignedAdminAsync(int adminId)
    {
        return await _context.SupportTickets
            .Where(t => t.AssignedTo == adminId)
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<SupportTicket> CreateAsync(SupportTicket ticket)
    {
        ticket.CreatedAt = DateTime.Now;
        ticket.UpdatedAt = DateTime.Now;
        
        _context.SupportTickets.Add(ticket);
        await _context.SaveChangesAsync();
        
        return ticket;
    }

    public async Task<SupportTicket> UpdateAsync(SupportTicket ticket)
    {
        ticket.UpdatedAt = DateTime.Now;
        
        _context.SupportTickets.Update(ticket);
        await _context.SaveChangesAsync();
        
        return ticket;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var ticket = await _context.SupportTickets.FindAsync(id);
        if (ticket == null)
            return false;

        _context.SupportTickets.Remove(ticket);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> GenerateTicketNumberAsync()
    {
        var date = DateTime.Now.ToString("yyyyMMdd");
        var count = await _context.SupportTickets
            .CountAsync(t => t.TicketNumber.StartsWith($"TK{date}"));
        
        return $"TK{date}{(count + 1):D4}";
    }
}
