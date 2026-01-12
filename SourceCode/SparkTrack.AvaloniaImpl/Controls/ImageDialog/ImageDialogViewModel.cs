namespace SparkTrack.AvaloniaImpl.Controls.ImageDialog;

using Fanatiki.MVVM.ViewModels;

public class ImageDialogViewModel(string name, string uri) : ViewModelBase
{
    public string Name { get; } = name;
    
    public string Uri { get; } = uri;
}