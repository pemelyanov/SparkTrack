namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

public interface IUploadableAttachment
{
    public Guid? UploadedFileId { get; }
    
    Task UploadAsync();
}