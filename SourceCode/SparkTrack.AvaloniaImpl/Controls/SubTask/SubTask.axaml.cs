namespace SparkTrack.AvaloniaImpl.Controls.SubTask;

using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

public partial class SubTask : ReactiveUserControl<SubTaskViewModel>
{
    public SubTask()
    {
        InitializeComponent();
    }

    #region IsForEmployee Property

    public static readonly StyledProperty<bool> IsForEmployeeProperty =
        AvaloniaProperty.Register<SubTask, bool>(nameof(IsForEmployee));

    public bool IsForEmployee
    {
        get => GetValue(IsForEmployeeProperty);
        set => SetValue(IsForEmployeeProperty, value);
    }

    #endregion

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
    }
}