namespace Fanatiki.Loading.Data;

using System.Collections.ObjectModel;
using System.Reactive.Subjects;

public class LoadingProgress
{
    #region Properties

    public required string StageName { get; init; }

    public BehaviorSubject<int> TotalTasksQuantity { get; init; } = new(-1);

    public BehaviorSubject<int> ProcessedTasksQuantity { get; init; } = new(0);

    public ObservableCollection<string> ProcessedTasks { get; } = [];

    #endregion
}