using Avalonia.Input;

namespace SparkTrack.AvaloniaImpl.Controls.ManualCheckBox;

using Avalonia.Controls;

public class ManualCheckBox : CheckBox
{
    protected override void OnClick()
    {
        var valueBeforeClick = IsChecked;

        try
        {
            base.OnClick();
        }
        catch
        {
            // ignore
        }
        
        IsChecked = valueBeforeClick;
    }
}