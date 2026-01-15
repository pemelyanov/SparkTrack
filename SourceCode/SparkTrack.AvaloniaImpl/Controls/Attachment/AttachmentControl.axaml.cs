namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.ReactiveUI;
using FluentIcons.Common;

public partial class AttachmentControl : ReactiveUserControl<IAttachmentViewModel>
{
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
}