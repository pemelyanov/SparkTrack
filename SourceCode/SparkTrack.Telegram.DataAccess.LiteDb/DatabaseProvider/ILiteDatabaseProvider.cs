namespace SparkTrack.Telegram.DataAccess.LiteDb.DatabaseProvider;

using LiteDB.Async;

public interface ILiteDatabaseProvider
{
    ILiteDatabaseAsync GetDatabase();
}