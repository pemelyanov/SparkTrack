namespace SparkTrack.AvaloniaImpl.Pages.Feature.DescriptionTemplates;

/// <summary>
/// TODO: В будущем подумать как переделать это в механизм.
/// Заказчику нужно делить описание на 3 поля и большое превью,
/// но я не хочу жертвовать гибкостью системы, поэтому будем хранить описание Json строкой вот такой структуры,
/// возможно потом можно будет сделать механизм подключения разных шаблонов для описания
/// </summary>
public record ReelWithPreviewTemplate
{
    public required string ReelLink { get; init; }
    
    public required string ReelDescription { get; init; }
    
    public required string PreviewDescription { get; init; }
}