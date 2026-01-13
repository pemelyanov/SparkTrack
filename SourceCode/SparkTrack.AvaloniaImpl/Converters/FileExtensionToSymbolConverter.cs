namespace SparkTrack.AvaloniaImpl.Converters;

using Avalonia.Data.Converters;
using FluentIcons.Common;
using System.Globalization;

public class FileExtensionToSymbolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string extension) return null;
        
        extension = extension.TrimStart('.').ToLowerInvariant();

        return extension switch
        {
            "mp4" or "mkv" or "avi" or "mov" or "wmv" or "webm" or "flv" or "m4v"
                => Symbol.Video,
            
            "zip" or "rar" or "7z" or "tar" or "gz" or "bz2" or "xz" or "tgz"
                or "iso" or "cab" or "arj" or "lz" or "zst" or "dra"
                => Symbol.FolderZip,
            
            "mp3" or "wav" or "flac" or "aac" or "ogg" or "opus" or "m4a" or "wma"
                => Symbol.MusicNote1,
            
            "pdf" or "doc" or "docx" or "xls" or "xlsx" or "ppt" or "pptx" or "odt" or "ods"
                or "txt" or "rtf" or "md" or "csv"
                => Symbol.Document,
            _ => Symbol.Attach
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}