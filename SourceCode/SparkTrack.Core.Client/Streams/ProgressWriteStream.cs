namespace SparkTrack.Core.Client.Streams;

using Data;

public sealed class ProgressWriteStream(Stream inner, LoadingProgress progress) : Stream
{
    private long m_totalWritten;

    public override async ValueTask WriteAsync(
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken = default
    )
    {
        await inner.WriteAsync(buffer, cancellationToken);
        m_totalWritten += buffer.Length;
        progress.CurrentProgress.OnNext(m_totalWritten);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        inner.Write(buffer, offset, count);
        m_totalWritten += count;
        progress.CurrentProgress.OnNext(m_totalWritten);
    }

    #region Proxy

    public override bool CanWrite => inner.CanWrite;

    public override bool CanRead => false;

    public override bool CanSeek => inner.CanSeek;

    public override long Length => inner.Length;

    public override long Position
    {
        get => inner.Position;
        set => inner.Position = value;
    }

    public override void Flush() => inner.Flush();

    public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

    public override void SetLength(long value) => inner.SetLength(value);

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing) inner.Dispose();
        base.Dispose(disposing);
    }

    #endregion
}