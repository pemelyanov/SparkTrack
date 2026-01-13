namespace SparkTrack.Core.Client.Data;

using System.Collections.ObjectModel;
using System.Reactive.Subjects;

public class LoadingProgress
{
    #region Properties

    public string StageName { get; init; } = string.Empty;

    public BehaviorSubject<long> TotalProgress { get; init; } = new(-1);

    public BehaviorSubject<long> CurrentProgress { get; init; } = new(0);

    public ObservableCollection<string> ProcessedTasks { get; } = [];

    #endregion
}