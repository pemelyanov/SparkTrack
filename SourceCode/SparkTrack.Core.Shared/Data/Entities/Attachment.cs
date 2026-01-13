namespace SparkTrack.Core.Shared.Data.Entities;

public record Attachment
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required string Extension { get; init; }
    
    public long Size { get; init; }
    
    public Guid FileId { get; init; }
    
    // TODO: Возможно стоит использовать что-то абстрактное, типа IsAutoLoadNeeded,
    // т.к. по этому полю на фронте определяем надо при предзагружать это дело
    public bool IsImage { get; init; }

    public byte[] Checksum { get; init; } = [];
}