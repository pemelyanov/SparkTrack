namespace SparkTrack.AvaloniaImpl.Pages.Feature;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls.Primitives;
using Extensions;

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

    private void InputElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if(sender is not ScrollViewer scrollViewer) return;

        scrollViewer.Offset = new Vector(scrollViewer.Offset.X + e.Delta.Y * 10, 0);
    }

    private async void TextBox_OnPastingFromClipboard(object? sender, RoutedEventArgs e)
    {
        await this.HandleImagePastingFromClipboard((data, extension) => ViewModel?.OnImagePaste(data, extension));
    }
    
    private void SaveAndClose_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;

        StyledElement? currentParent = button.Parent;

        while (currentParent is not null)
        {
            if (currentParent is Popup popup)
            {
                ViewModel?.SaveCommand.Execute(true);
                
                popup.Close();
                break;
            }

            currentParent = currentParent.Parent;
        }
    }
}