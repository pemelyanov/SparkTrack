namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

public interface IAttachmentViewModel
{
    bool IsImage { get; }
    
    string Uri { get; }
    
    string Name { get; }
    
    long Size { get; }

    void Remove();
}