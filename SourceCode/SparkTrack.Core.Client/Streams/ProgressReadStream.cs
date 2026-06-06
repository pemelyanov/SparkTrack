namespace SparkTrack.Core.Client.Streams;

using Data;

public sealed class ProgressReadStream : Stream
{
    private          long            m_totalRead;
    private readonly Stream          m_inner;
    private readonly LoadingProgress m_progress;

    public ProgressReadStream(Stream inner, LoadingProgress progress)
    {
        m_inner = inner;
        m_progress = progress;

        progress.TotalProgress.OnNext(-1);
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        var read = await m_inner.ReadAsync(buffer, cancellationToken);
        
        if (read > 0)
        {
            if (m_progress.TotalProgress.Value < 1)
                m_progress.TotalProgress.OnNext(Length);
            
            m_totalRead += read;
            m_progress.CurrentProgress.OnNext(m_totalRead);
        }
        return read;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = m_inner.Read(buffer, offset, count);
        if (read > 0)
        {
            m_totalRead += read;
            m_progress.TotalProgress.OnNext(m_totalRead);
        }
        return read;
    }

    #region Proxy
    public override bool CanRead => m_inner.CanRead;
    public override bool CanSeek => m_inner.CanSeek;
    public override bool CanWrite => false;
    public override long Length => m_inner.Length;
    public override long Position
    {
        get => m_inner.Position;
        set => m_inner.Position = value;
    }
    public override void Flush() => m_inner.Flush();
    public override long Seek(long offset, SeekOrigin origin) => m_inner.Seek(offset, origin);
    public override void SetLength(long value) => m_inner.SetLength(value);
    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();
    protected override void Dispose(bool disposing)
    {
        if (disposing) m_inner.Dispose();
        base.Dispose(disposing);
    }
    #endregion
}