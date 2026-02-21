namespace SparkTrack.Telegram.DataAccess.LiteDb.Repositories;

using System.Linq.Expressions;
using Core.Data;
using Core.Repositories;
using Data;
using DatabaseProvider;

public class TelegramUsersRepository(ILiteDatabaseProvider databaseProvider) : ITelegramUsersRepository
{
    public async Task<TelegramUser?> GetByIdAsync(Guid id) => await databaseProvider.GetDatabase()
        .GetCollection<TelegramUserData>()
        .Query()
        .Where(it => it.UserId == id)
        .Select(GetMapToDomainExpression())
        .FirstOrDefaultAsync();

    public async Task<TelegramUser?> GetByChatIdAsync(long chatId) => await databaseProvider.GetDatabase()
        .GetCollection<TelegramUserData>()
        .Query()
        .Where(it => it.ChatId == chatId)
        .Select(GetMapToDomainExpression())
        .FirstOrDefaultAsync();

    public Task AddAsync(TelegramUser user) => databaseProvider
        .GetDatabase()
        .GetCollection<TelegramUserData>()
        .InsertAsync(MapToData(user));

    public Task EditAsync(TelegramUser user) => databaseProvider
        .GetDatabase()
        .GetCollection<TelegramUserData>()
        .UpdateAsync(MapToData(user));

    public Task RemoveAsync(long chatId) => databaseProvider
        .GetDatabase()
        .GetCollection<TelegramUserData>()
        .DeleteManyAsync(it => it.ChatId == chatId);

    private TelegramUserData MapToData(TelegramUser it) => new()
    {
        UserId = it.UserId,
        ChatId = it.ChatId,
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
            IsNotificationsEnabled = it.IsNotificationsEnabled,
            TimeZone = it.TimeZone,
            IsHelloSent = it.IsHelloSent
        };
    }
}