namespace SparkTrack.AvaloniaImpl.Behaviors;

public class IntBoxBehavior : NumberBoxBehaviorBase
{
    protected override bool CanInput(string? source, string? added) => true;
    
    protected override bool IsNumber(string? source)
    {
        return source is not null && int.TryParse(source, out _);
    }
}