namespace SparkTrack.AvaloniaImpl.Services.AttachmentsPathCache;

public interface IAttachmentsPathCache
{
    public string? Resolve(Guid attachmentId);

    public void Save(Guid attachmentId, string localPath);
}