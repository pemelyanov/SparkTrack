namespace SparkTrack.AvaloniaImpl.Controls.AttachmentsPanel;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

public partial class AttachmentsPanel : ReactiveUserControl<AttachmentsPanelViewModel>
{
    public AttachmentsPanel()
    {
        InitializeComponent();
    }

    #region IsReadOnly Property

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<AttachmentsPanel, bool>(nameof(IsReadOnly));

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    #endregion
    
    private void FilesPanel_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if(sender is not Control panel) return;
        
        panel.AddHandler(DragDrop.DropEvent, FilesPanel_OnDrop);
    }
    
    private void FilesPanel_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if(sender is not Control panel) return;
        
        panel.RemoveHandler(DragDrop.DropEvent, FilesPanel_OnDrop);
    }

    private void FilesPanel_OnDrop(object? sender, DragEventArgs e)
    {
        if(ViewModel is null || !e.DataTransfer.Contains(DataFormat.File)) return;

        var files = e.DataTransfer.TryGetFiles();
        
        if(files is null) return;

        foreach (var file in files)
            ViewModel.AddAttachment(file.Path.LocalPath);
    }

}