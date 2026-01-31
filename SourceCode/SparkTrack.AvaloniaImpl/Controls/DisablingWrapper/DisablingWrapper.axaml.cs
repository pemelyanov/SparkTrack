using Avalonia.Controls;

namespace SparkTrack.AvaloniaImpl.Controls.DisablingWrapper;

using Avalonia;
using Avalonia.Data;

public partial class DisablingWrapper : UserControl
{
    public DisablingWrapper()
    {
        InitializeComponent();
    }

    #region IsContentEnabled Property

    public static readonly StyledProperty<bool> IsContentEnabledProperty =
        AvaloniaProperty.Register<DisablingWrapper, bool>(
            nameof(IsContentEnabled),
            defaultValue: true,
            defaultBindingMode: BindingMode.TwoWay
        );

    public bool IsContentEnabled
    {
        get => GetValue(IsContentEnabledProperty);
        set => SetValue(IsContentEnabledProperty, value);
    }

    #endregion
}