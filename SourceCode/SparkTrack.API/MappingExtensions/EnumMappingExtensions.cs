namespace SparkTrack.API.MappingExtensions;

public static class EnumMappingExtensions
{
    public static TResultEnum Cast<TResultEnum>(this Enum sourceEnum) where TResultEnum : struct =>
        Enum.Parse<TResultEnum>(sourceEnum.ToString());
}