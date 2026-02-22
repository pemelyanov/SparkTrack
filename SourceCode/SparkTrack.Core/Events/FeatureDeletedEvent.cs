namespace SparkTrack.Core.Events;

using Shared.Data.Entities;
using Shared.Enums;

/// <summary>
/// Событие удаления идеи
/// </summary>
/// <param name="Feature">Удаленная идея</param>
/// <param name="Reason">Причина архивации, если идея была удалена не полностью, а заархивирована</param>
public record FeatureDeletedEvent(Feature Feature, EArchiveSource? Reason = null);