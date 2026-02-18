namespace SparkTrack.Core.Shared.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Возвращает конец дня для указанной даты (23:59:59.999) с сохранением DateTimeKind
    /// </summary>
    public static DateTime EndOfTheDay(this DateTime dateTime)
    {
        return new DateTime(
            dateTime.Year, 
            dateTime.Month, 
            dateTime.Day, 
            23, 59, 59, 999, 
            dateTime.Kind
        );
    }
}