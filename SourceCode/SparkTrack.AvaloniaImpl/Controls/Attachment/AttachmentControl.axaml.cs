namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Converters;
using FluentIcons.Common;

public partial class AttachmentControl : ReactiveUserControl<IAttachmentViewModel>
{
    private IDisposable? m_dataContextDisposables;
    
    public AttachmentControl()
    {
        InitializeComponent();
    }

    #region IsReadOnly Property

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<AttachmentControl, bool>(nameof(IsReadOnly));

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    #endregion

    public static IValueConverter LoadingIconConverter { get; } = new FuncValueConverter<ELoadType, Symbol>(
        it => it switch
        {
            ELoadType.Download => Symbol.ArrowDownload,
            ELoadType.Upload => Symbol.ArrowUpload,
            _ => throw new NotSupportedException()
        }
    );

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        
        m_dataContextDisposables?.Dispose();
        m_dataContextDisposables = null;
        
        if(DataContext is not IAttachmentViewModel attachmentViewModel) return;
        
        attachmentViewModel.CopyToClipboardRequested += AttachmentViewModel_OnCopyToClipboardRequested;

        m_dataContextDisposables = Disposable.Create(() =>
            attachmentViewModel.CopyToClipboardRequested -= AttachmentViewModel_OnCopyToClipboardRequested
        );
    }

    private async void AttachmentViewModel_OnCopyToClipboardRequested()
    {
        if(DataContext is not IAttachmentViewModel attachmentViewModel) return;

        var bytes = UriToImageSourceConverter.ExtractBytes(attachmentViewModel.Uri);
        
        if(bytes is null) return;

        await CopyBytesToClipboardAsync(bytes, attachmentViewModel.Extension);
    }

    public async Task CopyBytesToClipboardAsync(byte[] bytes, string extension)
    {
        var clipboard = TopLevel.GetTopLevel(this).Clipboard;
        if (clipboard is null) return;

        var dataObject = new DataObject();
        
        dataObject.Set($"image/{extension}", bytes);
        
        await clipboard.SetDataObjectAsync(dataObject);
    }
}