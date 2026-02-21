namespace SparkTrack.AvaloniaImpl.Services.AttachmentsPathCache;

using System.Text.Json;
using Core.Shared.Eventing;
using Events;

public class JsonAttachmentsPathCache : IAttachmentsPathCache, IEventHandler<StartupEvent>
{
    private static string s_cachePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SparkTrack",
        "attachments_path_cache.json"
    );

    private PathCache m_pathCache = new();

    private static readonly object s_sync = new();
    
    public string? Resolve(Guid attachmentId) => m_pathCache.Cache.GetValueOrDefault(attachmentId);

    public void Save(Guid attachmentId, string localPath)
    {
        lock (s_sync)
        {
            m_pathCache.Cache[attachmentId] = localPath;

            var json = JsonSerializer.Serialize(m_pathCache);
            
            File.WriteAllText(s_cachePath, json);
        }
    }

    public Task HandleAsync(StartupEvent eventData, CancellationToken cancellationToken = default)
    {
        var cacheDirectory = Path.GetDirectoryName(s_cachePath)!;

        Directory.CreateDirectory(cacheDirectory);

        if (File.Exists(s_cachePath))
        {
            m_pathCache = JsonSerializer.Deserialize<PathCache>(File.ReadAllText(s_cachePath))!;
        }

        return Task.CompletedTask;
    }
}

record PathCache
{
    public Dictionary<Guid, string> Cache { get; } = [];
}