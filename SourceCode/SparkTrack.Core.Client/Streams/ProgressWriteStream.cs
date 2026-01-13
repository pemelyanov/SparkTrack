namespace SparkTrack.Core.Client.Streams;

using Data;

public sealed class ProgressWriteStream : Stream
{
    private readonly Stream          m_inner;
    private readonly LoadingProgress m_progress;
    private          long            m_totalWritten;

    public ProgressWriteStream(Stream inner, LoadingProgress progress)
    {
        m_inner = inner;
        m_progress = progress;
        m_progress.TotalProgress.OnNext(Length);
    }

    public override async ValueTask WriteAsync(
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        await m_inner.WriteAsync(buffer, cancellationToken);
        m_totalWritten += buffer.Length;
        m_progress.CurrentProgress.OnNext(m_totalWritten);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        m_inner.Write(buffer, offset, count);
        m_totalWritten += count;
        m_progress.CurrentProgress.OnNext(m_totalWritten);
    }

    #region Proxy
    public override bool CanWrite => m_inner.CanWrite;
    public override bool CanRead => false;
    public override bool CanSeek => m_inner.CanSeek;
    public override long Length => m_inner.Length;
    public override long Position
    {
        get => m_inner.Position;
        set => m_inner.Position = value;
    }
    public override void Flush() => m_inner.Flush();
    public override long Seek(long offset, SeekOrigin origin) => m_inner.Seek(offset, origin);
    public override void SetLength(long value) => m_inner.SetLength(value);
    public override int Read(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();
    protected override void Dispose(bool disposing)
    {
        if (disposing) m_inner.Dispose();
        base.Dispose(disposing);
    }
    #endregion
}
