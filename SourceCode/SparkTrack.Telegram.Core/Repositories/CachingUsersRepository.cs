namespace SparkTrack.Telegram.Core.Repositories;

using Data;

using System.Collections.Concurrent;

public class CachingUsersRepository(ITelegramUsersRepository sourceRepository) : ITelegramUsersRepository
{
    private readonly ConcurrentDictionary<Guid, TelegramUser> m_cache = new();
    
    private readonly SemaphoreSlim m_sync = new(1, 1);

    public async Task<TelegramUser?> GetByIdAsync(Guid id)
    {
        if (m_cache.TryGetValue(id, out var cachedUser))
            return cachedUser;
        
        await m_sync.WaitAsync();
        try
        {
            // Double-check
            if (m_cache.TryGetValue(id, out cachedUser))
                return cachedUser;

            var user = await sourceRepository.GetByIdAsync(id);

            if (user != null)
                m_cache[id] = user;

            return user;
        }
        finally
        {
            m_sync.Release();
        }
    }

    public async Task<TelegramUser?> GetByChatIdAsync(long chatId)
    {
        var cachedUser = m_cache.Values.FirstOrDefault(x => x.ChatId == chatId);
        if (cachedUser != null)
            return cachedUser;

        await m_sync.WaitAsync();
        try
        {
            // Double-check
            cachedUser = m_cache.Values.FirstOrDefault(x => x.ChatId == chatId);
            if (cachedUser != null)
                return cachedUser;

            var user = await sourceRepository.GetByChatIdAsync(chatId);

            if (user != null)
                m_cache[user.UserId] = user;

            return user;
        }
        finally
        {
            m_sync.Release();
        }
    }

    public async Task AddAsync(TelegramUser user)
    {
        await m_sync.WaitAsync();
        try
        {
            await sourceRepository.AddAsync(user);
            m_cache[user.UserId] = user;
        }
        finally
        {
            m_sync.Release();
        }
    }

    public async Task EditAsync(TelegramUser user)
    {
        await m_sync.WaitAsync();
        try
        {
            await sourceRepository.EditAsync(user);
            m_cache[user.UserId] = user;
        }
        finally
        {
            m_sync.Release();
        }
    }

    public async Task RemoveAsync(long chatId)
    {
        await m_sync.WaitAsync();
        try
        {
            await sourceRepository.RemoveAsync(chatId);
            
            var item = m_cache.Values.FirstOrDefault(x => x.ChatId == chatId);
            
            if(item is null) return;
            
            m_cache.TryRemove(item.UserId, out _);
        }
        finally
        {
            m_sync.Release();
        }
    }
}