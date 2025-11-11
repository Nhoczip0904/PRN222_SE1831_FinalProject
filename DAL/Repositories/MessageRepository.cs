using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly EvBatteryTrading2Context _context;

    public MessageRepository(EvBatteryTrading2Context context)
    {
        _context = context;
    }

    public async Task<Message?> GetByIdAsync(int id)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Include(m => m.Product)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Message>> GetConversationAsync(int userId, int otherUserId)
    {
        return await _context.Messages
            .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                       (m.SenderId == otherUserId && m.ReceiverId == userId))
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Include(m => m.Product)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetInboxAsync(int userId)
    {
        return await _context.Messages
            .Where(m => m.ReceiverId == userId)
            .Include(m => m.Sender)
            .Include(m => m.Product)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetSentMessagesAsync(int userId)
    {
        return await _context.Messages
            .Where(m => m.SenderId == userId)
            .Include(m => m.Receiver)
            .Include(m => m.Product)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Messages
            .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
    }

    public async Task<Message> CreateAsync(Message message)
    {
        message.CreatedAt = DateTime.Now;
        message.IsRead = false;
        
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        
        return message;
    }

    public async Task<Message> UpdateAsync(Message message)
    {
        _context.Messages.Update(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<bool> MarkAsReadAsync(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            return false;

        message.IsRead = true;
        message.ReadAt = DateTime.Now;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message == null)
            return false;

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();
        return true;
    }
}
