namespace SparkTrack.AvaloniaImpl.Behaviors;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;

public class FocusBehaviors
{
    static FocusBehaviors()
    {
        FocusOnLoadedProperty.Changed.AddClassHandler<Control>(OnFocusOnLoadedChanged);
    }

    private static void OnFocusOnLoadedChanged(Control obj, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is false)
        {
            obj.Loaded -= Obj_OnLoaded;
            
            return;
        }

        if (args.NewValue is true && args.OldValue != args.NewValue)
        {
            obj.Loaded += Obj_OnLoaded;
        }
    }

    private static void Obj_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if(sender is not Control control) return;

        control.Focus();
    }

    public static readonly AttachedProperty<bool> FocusOnLoadedProperty = AvaloniaProperty.RegisterAttached<FocusBehaviors, Control, bool>(
        "FocusOnLoaded", false, false, BindingMode.OneTime);

    public static bool GetFocusOnLoaded(Control obj) => obj.GetValue(FocusOnLoadedProperty);

    public static void SetFocusOnLoaded(Control obj, bool value) => obj.SetValue(FocusOnLoadedProperty, value);
}