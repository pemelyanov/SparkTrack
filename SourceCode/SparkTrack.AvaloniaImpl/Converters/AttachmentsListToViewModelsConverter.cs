namespace SparkTrack.AvaloniaImpl.Converters;

using Avalonia.Data.Converters;
using Core.Shared.Data.Entities;
using Delegates;
using Splat;
using System.Globalization;

public class AttachmentsListToViewModelsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<Attachment> attachmentsList) return null;

        var factory = Locator.Current.GetService<RemoteAttachmentViewModelFactory>()!;

        return attachmentsList.Select(it => factory(it, _ => { }));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}