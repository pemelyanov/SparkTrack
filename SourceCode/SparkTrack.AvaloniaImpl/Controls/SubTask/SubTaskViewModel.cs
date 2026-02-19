namespace SparkTrack.AvaloniaImpl.Controls.SubTask;

using Core.Shared.Data.Edit;
using SubTaskData = Core.Shared.Data.Entities.SubTask;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Core.Shared.Services.SubTasks;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.DialogHost;
using System.Reactive;
using System.Reactive.Disposables;
using Core.Shared.Extensions;
using Data.Templates;
using Exceptions;
using TemplateSaveForm;

public class SubTaskViewModel : ViewModelBase
{
    private          bool                                             m_isUserInitiallySet;
    private          SubTaskData?                                     m_subTask;
    private readonly IObservable<IReadOnlyList<User>>                 m_availableEmployees;
    private readonly Action<SubTaskViewModel>                         m_onRemove;
    private readonly ISubTasksService                                 m_subTasksService;
    private readonly IDialogService                                   m_dialogService;
    private readonly Func<TemplateSaveFormViewModel<SubTaskTemplate>> m_templateViewModelFactory;

    public SubTaskViewModel(SubTaskData? subTask,
                            IObservable<IReadOnlyList<User>> availableEmployees,
                            Action<SubTaskViewModel> onRemove,
                            ISubTasksService subTasksService,
                            IDialogService dialogService,
                            Func<TemplateSaveFormViewModel<SubTaskTemplate>> templateViewModelFactory)
    {
        m_subTask = subTask;
        m_availableEmployees = availableEmployees;
        m_onRemove = onRemove;
        m_subTasksService = subTasksService;
        m_dialogService = dialogService;
        m_templateViewModelFactory = templateViewModelFactory;
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
    public float TimelyBonus { get; set; }
    
    [Reactive]
    public bool IsCompleted { get; private set; }
    
    [Reactive]
    public EPaymentStatus PaymentStatus { get; private set; }
    
    public ReactiveCommand<Unit, Unit> ToggleCompletionStatusCommand { get; }
    
    public ReactiveCommand<Unit, Unit> TogglePaymentStatusCommand { get; }

    public async Task RemoveAsync()
    {
        if (!await m_dialogService.ConfirmAsync(
            "Вы уверены что хотите удалить задачу?",
            "Удаление задачи"
        )) return;
        
        m_onRemove(this);
    }

    public async Task SaveAsTemplateAsync()
    {
        var viewModel = m_templateViewModelFactory();

        await m_dialogService.ShowAsync(viewModel);
    }

    public SubTaskEdit MapToEdit() => new()
    {
        Id = m_subTask?.Id ?? Guid.Empty,
        Name = Name,
        ExecutorEmployeeId = SelectedEmployee?.Id ?? throw new NotifyUIException($"Выберите сотрудника для задачи {Name}"),
        Deadline = Deadline,
        Cost = Cost,
        Version = m_subTask?.Version ?? Guid.Empty,
        TimelyBonus = TimelyBonus
    };

    private async Task ToggleCompletionStatusAsync()
    {
        if(m_subTask is null) return;

        m_subTask = await m_subTasksService.SetIsCompletedAsync(m_subTask.Id, !IsCompleted, m_subTask.Version);
        
        UpdateProperties(m_subTask);
    }
    
    private async Task TogglePaymentStatusAsync()
    {
        if(m_subTask is null) return;

        var paymentStatus = PaymentStatus;

        if (paymentStatus is EPaymentStatus.None)
        {
            paymentStatus = EPaymentStatus.OnPayment;
        }
        else
        {
            paymentStatus = EPaymentStatus.None;
        }
        
        m_subTask = await m_subTasksService.SetPaymentStatusAsync(m_subTask.Id, paymentStatus, m_subTask.Version);
        
        UpdateProperties(m_subTask);
    }
    
    private void UpdateProperties(SubTaskData? subTask)
    {
        Name = subTask?.Name ?? string.Empty;
        Deadline = (subTask?.Deadline ?? DateTime.Now).EndOfTheDay();
        Cost = subTask?.Cost ?? 0;
        IsCompleted = subTask?.IsCompleted ?? false;
        PaymentStatus = subTask?.PaymentStatus ?? EPaymentStatus.None;
        TimelyBonus = subTask?.TimelyBonus ?? 0;
    }
}