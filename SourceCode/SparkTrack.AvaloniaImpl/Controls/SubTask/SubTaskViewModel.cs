namespace SparkTrack.AvaloniaImpl.Controls.SubTask;

using SubTaskData = Core.Shared.Data.Entities.SubTask;
using Core.Shared.Data.Entities;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Disposables;

public class SubTaskViewModel(
    SubTaskData? subTask,
    IObservable<IReadOnlyList<User>> availableEmployees,
    Action<SubTaskViewModel> onDelete
) : ViewModelBase
{
    private bool m_isUserInitiallySet;

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        availableEmployees.Subscribe(
                it =>
                {
                    AvailableEmployees = it;

                    if (m_isUserInitiallySet) return;
                    m_isUserInitiallySet = true;

                    if (SelectedEmployee is not null || subTask?.ExecutorEmployee is null) return;

                    SelectedEmployee = it.FirstOrDefault(u => u.Id == subTask.ExecutorEmployee.Id);
                }
            )
            .DisposeWith(disposables);
    }

    [Reactive]
    public bool IsInEditMode { get; set; }

    [Reactive]
    public string Name { get; set; } = subTask?.Name ?? string.Empty;

    [Reactive]
    public IReadOnlyList<User> AvailableEmployees { get; private set; } = [];

    [Reactive]
    public User? SelectedEmployee { get; set; }

    [Reactive]
    public DateTime Deadline { get; set; } = subTask?.Deadline ?? DateTime.Now;

    [Reactive]
    public float Cost { get; set; } = subTask?.Cost ?? 0;

    public void Delete() => onDelete(this);
}