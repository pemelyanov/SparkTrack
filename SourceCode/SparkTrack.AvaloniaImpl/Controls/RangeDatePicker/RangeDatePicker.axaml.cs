using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SparkTrack.AvaloniaImpl.Controls.RangeDatePicker;

using Avalonia.Data;

public partial class RangeDatePicker : UserControl
{
    public RangeDatePicker()
    {
        InitializeComponent();
    }

    #region StartDate Property

    public static readonly StyledProperty<DateTime?> StartDateProperty =
        AvaloniaProperty.Register<RangeDatePicker, DateTime?>(nameof(StartDate), defaultBindingMode: BindingMode.TwoWay);

    public DateTime? StartDate
    {
        get => GetValue(StartDateProperty);
        set => SetValue(StartDateProperty, value);
    }

    #endregion

    #region EndDate Property

    public static readonly StyledProperty<DateTime?> EndDateProperty =
        AvaloniaProperty.Register<RangeDatePicker, DateTime?>(nameof(EndDate), defaultBindingMode: BindingMode.TwoWay);

    public DateTime? EndDate
    {
        get => GetValue(EndDateProperty);
        set => SetValue(EndDateProperty, value);
    }

    #endregion

    #region Label Property

    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<RangeDatePicker, string?>(nameof(Label));

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    #endregion
}