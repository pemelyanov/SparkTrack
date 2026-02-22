namespace SparkTrack.Telegram.DataAccess.LiteDb.Repositories;

using System.Linq.Expressions;
using Core.Data;
using Core.Repositories;
using Data;
using DatabaseProvider;

public class TelegramUsersRepository : ITelegramUsersRepository
{
    private readonly ILiteDatabaseProvider m_databaseProvider;

    public TelegramUsersRepository(ILiteDatabaseProvider databaseProvider)
    {
        m_databaseProvider = databaseProvider;
        
        var collection = 
            m_databaseProvider.GetDatabase()
                .UnderlyingDatabase
                .GetCollection<TelegramUserData>();
        
        collection.EnsureIndex(it => it.ChatId, unique: true);
        collection.EnsureIndex(it => it.Tag, unique: true);
    }

    public async Task<TelegramUser?> GetByIdAsync(Guid id) => await m_databaseProvider.GetDatabase()
        .GetCollection<TelegramUserData>()
        .Query()
        .Where(it => it.UserId == id)
        .Select(GetMapToDomainExpression())
        .FirstOrDefaultAsync();

    public async Task<TelegramUser?> GetByChatIdAsync(long chatId) => await m_databaseProvider.GetDatabase()
        .GetCollection<TelegramUserData>()
        .Query()
        .Where(it => it.ChatId == chatId)
        .Select(GetMapToDomainExpression())
        .FirstOrDefaultAsync();

    public async Task<TelegramUser?> GetByTagAsync(string tag) => await m_databaseProvider.GetDatabase()
        .GetCollection<TelegramUserData>()
        .Query()
        .Where(it => it.Tag == tag)
        .Select(GetMapToDomainExpression())
        .FirstOrDefaultAsync();

    public Task AddAsync(TelegramUser user) => m_databaseProvider
        .GetDatabase()
        .GetCollection<TelegramUserData>()
        .InsertAsync(MapToData(user));

    public Task EditAsync(TelegramUser user) => m_databaseProvider
        .GetDatabase()
        .GetCollection<TelegramUserData>()
        .UpdateAsync(MapToData(user));

    public Task RemoveAsync(long chatId) => m_databaseProvider
        .GetDatabase()
        .GetCollection<TelegramUserData>()
        .DeleteManyAsync(it => it.ChatId == chatId);

    private TelegramUserData MapToData(TelegramUser it) => new()
    {
        UserId = it.UserId,
        ChatId = it.ChatId,
        Tag = it.Tag,
        TimeZone = it.TimeZone,
        IsNotificationsEnabled = it.IsNotificationsEnabled,
        IsHelloSent = it.IsHelloSent
    };
    
    private static Expression<Func<TelegramUserData, TelegramUser>> GetMapToDomainExpression()
    {
        return it => new TelegramUser
        {
            UserId = it.UserId,
            ChatId = it.ChatId,
            Tag = it.Tag,
            IsNotificationsEnabled = it.IsNotificationsEnabled,
            TimeZone = it.TimeZone,
            IsHelloSent = it.IsHelloSent
        };
    }
}