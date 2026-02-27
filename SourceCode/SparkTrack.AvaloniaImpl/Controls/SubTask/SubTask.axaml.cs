using Avalonia.Controls;
using SparkTrack.Core.Shared.Extensions;

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

    #region IsInEditMode

    public static readonly StyledProperty<bool> IsInEditModeProperty =
        AvaloniaProperty.Register<SubTask, bool>(
            nameof(IsInEditMode));

    public bool IsInEditMode
    {
        get => GetValue(IsInEditModeProperty);
        set => SetValue(IsInEditModeProperty, value);
    }

    #endregion


    private void CalendarDatePicker_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        if(ViewModel is null || ViewModel.Deadline.TimeOfDay != TimeSpan.Zero) return;

        ViewModel.Deadline = ViewModel.Deadline.EndOfTheDay();
    }
}