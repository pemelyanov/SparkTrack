namespace SparkTrack.Telegram.Core.Repositories;

using Data;

public interface ITelegramUsersRepository
{
    Task<TelegramUser?> GetByIdAsync(Guid id);
    
    Task<TelegramUser?> GetByChatIdAsync(long chatId);
    
    Task<TelegramUser?> GetByTagAsync(string tag);
    
    Task AddAsync(TelegramUser user);

    Task EditAsync(TelegramUser user);

    Task RemoveAsync(long chatId);
}