using DAL.Entities;

namespace DAL.Repositories;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(int id);
    Task<IEnumerable<Message>> GetConversationAsync(int userId, int otherUserId);
    Task<IEnumerable<Message>> GetInboxAsync(int userId);
    Task<IEnumerable<Message>> GetSentMessagesAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<Message> CreateAsync(Message message);
    Task<Message> UpdateAsync(Message message);
    Task<bool> MarkAsReadAsync(int messageId);
    Task<bool> DeleteAsync(int id);
}
