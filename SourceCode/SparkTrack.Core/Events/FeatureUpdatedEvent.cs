using SparkTrack.Core.Shared.Data.Entities;

namespace SparkTrack.Core.Events;

public record FeatureUpdatedEvent(Feature OldInfo, Feature NewInfo);