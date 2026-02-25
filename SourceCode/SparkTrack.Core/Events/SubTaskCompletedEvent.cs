using SparkTrack.Core.Shared.Data.Entities;

namespace SparkTrack.Core.Events;

public record SubTaskCompletedEvent(SubTask SubTask, Feature ParentFeature);