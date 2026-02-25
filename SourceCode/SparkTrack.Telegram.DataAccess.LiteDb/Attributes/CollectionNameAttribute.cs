namespace SparkTrack.Telegram.DataAccess.LiteDb.Attributes;

public class CollectionNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}