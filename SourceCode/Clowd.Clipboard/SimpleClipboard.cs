namespace Clowd.Clipboard;

public class SimpleClipboard : ClipboardStaticBase<SimpleClipboardHandle>
{
    public static byte[]? GetImage()
    {
        using var ch = Open();
        return ch.GetImageBytes();
    }

    public static bool ContainsImage()
    {
        using var ch = Open();
        return ch.ContainsImage();
    }
}
