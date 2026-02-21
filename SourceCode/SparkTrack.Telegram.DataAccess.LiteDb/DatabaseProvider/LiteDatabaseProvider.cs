namespace SparkTrack.Telegram.DataAccess.LiteDb.DatabaseProvider;

using System.Reflection;
using Attributes;
using LiteDB;
using LiteDB.Async;
using NLog;

internal class LiteDatabaseProvider : ILiteDatabaseProvider, IDisposable
{
    private static readonly ILogger            s_logger = LogManager.GetCurrentClassLogger();
    private readonly        ILiteDatabaseAsync m_liteDatabaseAsync;
    
    public LiteDatabaseProvider(string databasePath, string password)
    {
        s_logger.Info("Initializing LiteDB by path: {path}", databasePath);

        if (Path.GetDirectoryName(databasePath) is { } directory) Directory.CreateDirectory(directory);
        
        BsonMapper.Global.ResolveCollectionName = ResolveCollectionName; 
        m_liteDatabaseAsync = new LiteDatabaseAsync(
            new ConnectionString
            {
                Filename = databasePath,
                Password = password
            }
        );
    }

    private string ResolveCollectionName(Type type)
    {
        return (type.GetCustomAttribute(typeof(CollectionNameAttribute)) as CollectionNameAttribute)?.Name ?? type.Name;
    }

    public ILiteDatabaseAsync GetDatabase() => m_liteDatabaseAsync;

    public void Dispose()
    {
        m_liteDatabaseAsync.Dispose();
    }
}