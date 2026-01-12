namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

using Extensions;
using ImageDialog;
using ReactiveUI;
using Services.DialogHost;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Input;

public class LocalAttachmentViewModel : IAttachmentViewModel
{
    private readonly Action<IAttachmentViewModel> m_onRemove;
    private readonly IDialogHost                  m_dialogHost;

    public LocalAttachmentViewModel(string path, Action<IAttachmentViewModel> onRemove, IDialogHost dialogHost)
    {
        m_onRemove = onRemove;
        m_dialogHost = dialogHost;
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

    public ICommand SaveAsCommand { get; } = ReactiveCommand.Create(() => { }, Observable.Return(false));

    public void Remove()
    {
        m_onRemove.Invoke(this);
    }

    public Task DownloadAsync() => throw new NotImplementedException();

    public void Open()
    {
        if (!IsImage)
        {
         
            Process.Start(new ProcessStartInfo
            {
                FileName = Uri,
                UseShellExecute = true
            });
            
            return;
        }

        var imageViewModel = new ImageDialogViewModel(Name, Uri);

        m_dialogHost.ShowAsync(imageViewModel);
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