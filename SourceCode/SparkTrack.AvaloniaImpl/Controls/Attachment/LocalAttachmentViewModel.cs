namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

using Extensions;

public class LocalAttachmentViewModel : IAttachmentViewModel
{
    private readonly Action<IAttachmentViewModel> m_onRemove;

    public LocalAttachmentViewModel(string path, Action<IAttachmentViewModel> onRemove)
    {
        m_onRemove = onRemove;
        using var stream = File.OpenRead(path);

        IsImage = stream.IsImageBySignature();
        Uri = path;
        Name = Path.GetFileName(path);
        Size = stream.Length;
    }

    public bool IsImage { get; }

    public string Uri { get; }

    public string Name { get; }

    public long Size { get; }

    public void Remove()
    {
        m_onRemove.Invoke(this);
    }
}