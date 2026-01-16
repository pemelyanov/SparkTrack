namespace SparkTrack.AvaloniaImpl.Controls.SubTask;

using Core.Shared.Data.Edit;
using SubTaskData = Core.Shared.Data.Entities.SubTask;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.Reactive.Disposables;

public class SubTaskViewModel : ViewModelBase
{
    private          bool                             m_isUserInitiallySet;
    private readonly SubTaskData?                     m_subTask;
    private readonly IObservable<IReadOnlyList<User>> m_availableEmployees;
    private readonly Action<SubTaskViewModel>         m_onDelete;

    public SubTaskViewModel(SubTaskData? subTask,
                            IObservable<IReadOnlyList<User>> availableEmployees,
                            Action<SubTaskViewModel> onDelete)
    {
        m_subTask = subTask;
        m_availableEmployees = availableEmployees;
        m_onDelete = onDelete;
        UpdateProperties(subTask);

        ToggleCompletionStatusCommand = ReactiveCommand.CreateFromTask(ToggleCompletionStatusAsync);
        TogglePaymentStatusCommand = ReactiveCommand.CreateFromTask(TogglePaymentStatusAsync);
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        m_availableEmployees.Subscribe(
                it =>
                {
                    AvailableEmployees = it;

                    if (m_isUserInitiallySet) return;
                    m_isUserInitiallySet = true;

                    if (SelectedEmployee is not null || m_subTask?.ExecutorEmployee is null) return;

                    SelectedEmployee = it.FirstOrDefault(u => u.Id == m_subTask.ExecutorEmployee.Id);
                }
            )
            .DisposeWith(disposables);
    }

    [Reactive]
    public bool IsInEditMode { get; set; }

    [Reactive]
    public string Name { get; set; } = string.Empty;

    [Reactive]
    public IReadOnlyList<User> AvailableEmployees { get; private set; } = [];

    [Reactive]
    public User? SelectedEmployee { get; set; }

    [Reactive]
    public DateTime Deadline { get; set; }

    [Reactive]
    public float Cost { get; set; }
    
    [Reactive]
    public bool IsCompleted { get; private set; }
    
    [Reactive]
    public EPaymentStatus PaymentStatus { get; private set; }
    
    public ReactiveCommand<Unit, Unit> ToggleCompletionStatusCommand { get; }
    
    public ReactiveCommand<Unit, Unit> TogglePaymentStatusCommand { get; }

    public void Delete() => m_onDelete(this);

    public SubTaskEdit MapToEdit() => new()
    {
        Id = m_subTask?.Id ?? Guid.Empty,
        Name = Name,
        ExecutorEmployeeId = SelectedEmployee?.Id ?? throw new NullReferenceException("Select employee"),
        Deadline = Deadline,
        Cost = Cost,
        Version = m_subTask?.Version ?? Guid.Empty
    };

    private async Task ToggleCompletionStatusAsync()
    {
        await Task.Delay(1000);

        if (IsCompleted)
        {
            IsCompleted = false;
            PaymentStatus = EPaymentStatus.None;
            return;
        }

        IsCompleted = true;
        PaymentStatus = EPaymentStatus.OnPayment;
    }
    
    private async Task TogglePaymentStatusAsync()
    {
        await Task.Delay(1000);

        if (PaymentStatus is EPaymentStatus.None)
        {
            PaymentStatus = EPaymentStatus.OnPayment;
            return;
        }
        
        PaymentStatus = EPaymentStatus.None;
    }
    
    private void UpdateProperties(SubTaskData? subTask)
    {
        Name = subTask?.Name ?? string.Empty;
        Deadline = subTask?.Deadline ?? DateTime.Now;
        Cost = subTask?.Cost ?? 0;
        IsCompleted = subTask?.IsCompleted ?? false;
        PaymentStatus = EPaymentStatus.Paid;
    }
}