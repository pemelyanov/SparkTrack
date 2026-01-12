namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

using Extensions;
using System.Diagnostics;

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

    public bool IsDownloaded => true;

    public string Uri { get; }

    public string Name { get; }

    public long Size { get; }

    public void Remove()
    {
        m_onRemove.Invoke(this);
    }

    public Task DownloadAsync() => throw new NotImplementedException();

    public void Open()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = Uri,
            UseShellExecute = true
        });
    }

    public void OpenInExplorer()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"/select,\"{Uri}\"",
            UseShellExecute = true
        });
    }
}