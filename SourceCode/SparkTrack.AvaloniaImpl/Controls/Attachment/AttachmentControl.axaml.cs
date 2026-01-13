namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

using Avalonia.Data.Converters;
using Avalonia.ReactiveUI;
using FluentIcons.Common;

public partial class AttachmentControl : ReactiveUserControl<IAttachmentViewModel>
{
    public AttachmentControl()
    {
        InitializeComponent();
    }

    public static IValueConverter LoadingIconConverter { get; } = new FuncValueConverter<ELoadType, Symbol>(
        it => it switch
        {
            ELoadType.Download => Symbol.ArrowDownload,
            ELoadType.Upload => Symbol.ArrowUpload,
            _ => throw new NotSupportedException()
        }
    );
}