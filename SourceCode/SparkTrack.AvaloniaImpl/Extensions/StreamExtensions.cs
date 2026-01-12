namespace SparkTrack.AvaloniaImpl.Extensions;

public static class StreamExtensions
{
    public static bool IsImageBySignature(this Stream stream)
    {
        Span<byte> header = stackalloc byte[12];
        if (stream.Read(header) < 12)
            return false;

        return
            header[..8].SequenceEqual(new byte[] { 137,80,78,71,13,10,26,10 }) || // PNG
            (header[0] == 0xFF && header[1] == 0xD8) ||                          // JPG
            header[..6].SequenceEqual("GIF89a"u8) ||
            header[..6].SequenceEqual("GIF87a"u8) ||
            header[..2].SequenceEqual("BM"u8);                                  // BMP
    }
}