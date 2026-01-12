namespace SparkTrack.AvaloniaImpl.Pages.Feature;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

public partial class FeaturePage : ReactiveUserControl<FeaturePageViewModel>
{
    public FeaturePage()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if(sender is not ScrollViewer scrollViewer) return;

        scrollViewer.Offset = new Vector(scrollViewer.Offset.X + e.Delta.Y * 10, 0);
    }

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