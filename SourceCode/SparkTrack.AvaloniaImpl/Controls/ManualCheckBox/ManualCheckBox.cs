namespace SparkTrack.AvaloniaImpl.Controls.ManualCheckBox;

using Avalonia.Controls;

public class ManualCheckBox : CheckBox
{
    protected override void OnClick()
    {
        var valueBeforeClick = IsChecked;
        
        base.OnClick();

        IsChecked = valueBeforeClick;
    }
}