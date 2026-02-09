namespace SparkTrack.AvaloniaImpl.Pages.Feature;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;

public partial class FeaturePage : ReactiveUserControl<FeaturePageViewModel>
{
    private CompositeDisposable? m_loadedDisposables;
    
    public FeaturePage()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        m_loadedDisposables = new CompositeDisposable();
        
        CommentCreationControl.GetObservable(IsVisibleProperty)
            .Where(isVisible => isVisible)
            .Throttle(TimeSpan.FromMilliseconds(50))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(
                _ =>
                {
                    MainScrollViewer.ScrollToEnd();
                }
            )
            .DisposeWith(m_loadedDisposables);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        
        m_loadedDisposables?.Dispose();
    }

    public async void OnPaste()
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;

        if (clipboard?.TryGetDataAsync() is not { } dataTask) return;
        
        var data = await dataTask;
            
        if(data is null) return;

        foreach (var asyncDataTransferItem in data.Items)
        {
            var imageFormat = asyncDataTransferItem.Formats.FirstOrDefault(it =>
                it.Identifier is "PNG" or "JPG" or "JPEG"
            );
                
            if(imageFormat is null) continue;

            var imageBytes = await asyncDataTransferItem.TryGetRawAsync(imageFormat) as byte[];
                
            if(imageBytes is null) continue;
            
            ViewModel?.OnImagePaste(imageBytes, imageFormat.Identifier.ToLower());
        }
    }

    private void InputElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if(sender is not ScrollViewer scrollViewer) return;

        scrollViewer.Offset = new Vector(scrollViewer.Offset.X + e.Delta.Y * 10, 0);
    }
}