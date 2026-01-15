namespace SparkTrack.AvaloniaImpl.Behaviors;

using System.Linq;

public class FloatBoxBehavior : NumberBoxBehaviorBase
{
    protected override bool CanInput(string? source, string? added)
    {
        source ??= string.Empty;
        added ??= string.Empty;

        var sourceHasComma = source.Any(s => s == ',');
        var addedCommaQuantity = added.Count(s => s == ',');
            
        if (sourceHasComma && addedCommaQuantity > 0) return false;
        if (addedCommaQuantity > 1) return false;
        
        return true;
    }
    
    protected override bool IsNumber(string? source)
    {
        return source is not null && (float.TryParse(source, out _) || source == ",");
    }
}