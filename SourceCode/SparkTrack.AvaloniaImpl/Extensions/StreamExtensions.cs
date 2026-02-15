namespace SparkTrack.AvaloniaImpl.Extensions;

public static class StreamExtensions
{
    public static bool IsImageBySignature(this Stream stream)
    {
        Span<byte> header = stackalloc byte[12];
        if (stream.Read(header) < 12)
            return false;

        return GetImageExtensionBySignature(header) is not null;
    }
    
    public static string? GetImageExtensionBySignature(this ReadOnlySpan<byte> stream)
    {
        if (stream.Length < 2)
            return null;

        // PNG
        if (stream.Length >= 8 &&
            stream[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            return "png";

        // JPEG / JPG (SOI)
        if (stream[0] == 0xFF && stream[1] == 0xD8)
        {
            // Доп. проверка на JFIF / EXIF
            if (stream.Length >= 4)
            {
                // FF D8 FF E0 — JFIF
                if (stream[2] == 0xFF && stream[3] == 0xE0)
                    return "jpg";

                // FF D8 FF E1 — EXIF
                if (stream[2] == 0xFF && stream[3] == 0xE1)
                    return "jpeg";
            }

            return "jpg"; // fallback
        }

        // GIF
        if (stream.Length >= 6 &&
            (stream[..6].SequenceEqual("GIF89a"u8) ||
                stream[..6].SequenceEqual("GIF87a"u8)))
            return "gif";

        // BMP
        if (stream.Length >= 2 &&
            stream[..2].SequenceEqual("BM"u8))
            return "bmp";

        return null;
    }
}