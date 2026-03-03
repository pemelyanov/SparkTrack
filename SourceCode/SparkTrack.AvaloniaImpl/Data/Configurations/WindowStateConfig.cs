using System.Drawing;

namespace SparkTrack.AvaloniaImpl.Data.Configurations;

public record WindowStateConfig
{
    public Point? Position { get; init; }
    
    public Size? Size { get; init; }
}