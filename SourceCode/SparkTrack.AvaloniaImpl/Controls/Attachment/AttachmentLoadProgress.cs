namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

using Core.Client.Data;

public class AttachmentLoadProgress(ELoadType loadType, LoadingProgress progress)
{
    public ELoadType LoadType { get; } = loadType;

    public LoadingProgress Progress { get; } = progress;
}