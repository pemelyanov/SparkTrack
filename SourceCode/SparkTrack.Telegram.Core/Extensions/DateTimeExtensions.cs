namespace SparkTrack.Telegram.Core.Extensions;

public static class DateTimeExtensions
{
    public static DateTime ApplyTimeZone(this DateTime dateTime,TimeSpan? timeZone)
    {
        if (timeZone is null) return dateTime;

        return TimeZoneInfo.ConvertTimeFromUtc(
            dateTime,
            TimeZoneInfo.CreateCustomTimeZone(
                $"custom-user-timezone-{(int)timeZone.Value.TotalMinutes}",
                timeZone.Value,
                "",
                ""
            )
        );
    }
    
    public static string AsUtcOffset(this TimeSpan offset)
    {
        var sign = offset >= TimeSpan.Zero ? "+" : "-";
        return $"UTC{sign}{offset:hh\\:mm}";
    }
}