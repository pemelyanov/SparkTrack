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
using System.Reactive.Linq;
using Data.Templates;
using DynamicData;
using Exceptions;
using TemplateSaveForm;

public class SubTaskViewModel : ViewModelBase
{
    private          bool                                                              m_isUserInitiallySet;
    private          bool                                                              m_isDependenciesInitiallySet;
    private          SubTaskData?                                                      m_subTask;
    private readonly IObservable<IReadOnlyList<User>>                                  m_availableEmployees;
    private readonly IObservable<IReadOnlyList<SubTaskViewModel>>                      m_availableSubTasks;
    private readonly Action<SubTaskViewModel>                                          m_onRemove;
    private readonly ISubTasksService                                                  m_subTasksService;
    private readonly IDialogService                                                    m_dialogService;
    private readonly Func<SubTaskTemplate, TemplateSaveFormViewModel<SubTaskTemplate>> m_templateViewModelFactory;

    public SubTaskViewModel(
        SubTaskData? subTask,
        IObservable<IReadOnlyList<User>> availableEmployees,
        IObservable<IReadOnlyList<SubTaskViewModel>> availableSubTasks,
        Action<SubTaskViewModel> onRemove,
        ISubTasksService subTasksService,
        IDialogService dialogService,
        Func<SubTaskTemplate, TemplateSaveFormViewModel<SubTaskTemplate>> templateViewModelFactory
    )
    {
        m_subTask = subTask;
        m_availableEmployees = availableEmployees;
        m_availableSubTasks = availableSubTasks;
        m_onRemove = onRemove;
        m_subTasksService = subTasksService;
        m_dialogService = dialogService;
        m_templateViewModelFactory = templateViewModelFactory;

        Id = m_subTask?.Id ?? Guid.NewGuid();
        IsNew = m_subTask is null;
        UpdateProperties(subTask);

        ToggleCompletionStatusCommand = ReactiveCommand.CreateFromTask(ToggleCompletionStatusAsync);
        TogglePaymentStatusCommand = ReactiveCommand.CreateFromTask(TogglePaymentStatusAsync);
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        m_availableEmployees
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(list =>
                {
                    AvailableEmployees = list;

                    if (list.Count == 0) return;

                    if (m_isUserInitiallySet) return;
                    m_isUserInitiallySet = true;

                    if (EmployeeToSelectOnNextLoad is not null || m_subTask?.ExecutorEmployee is null)
                    {
                        SelectedEmployee =
                            AvailableEmployees.FirstOrDefault(u => u.Id == EmployeeToSelectOnNextLoad?.Id);
                        EmployeeToSelectOnNextLoad = null;

                        return;
                    }

                    SelectedEmployee = list.FirstOrDefault(u => u.Id == m_subTask?.ExecutorEmployee.Id);
                }
            )
            .DisposeWith(disposables);

        m_availableSubTasks.Subscribe(list =>
                {
                    if (list.Count == 0) return;

                    if (m_isDependenciesInitiallySet) return;
                    m_isDependenciesInitiallySet = true;

                    if (m_subTask is null) return;

                    using (DependsOnList.SuspendNotifications())
                    {
                        DependsOnList.AddRange(list.Where(it => m_subTask.DependsOnIdList.Contains(it.Id)));
                    }
                }
            )
            .DisposeWith(disposables);

        m_availableSubTasks.Select(list =>
                list.Select(it => it.DependsOnList.GetListObservable()).CombineLatest().Select(_ => list)
            )
            .Switch()
            .Subscribe(list =>
                {
                    AvailableDependencyTasksList =
                        list.Where(it => it != this && !it.DependsOnList.Contains(this) && !DependsOnList.Contains(it))
                            .ToArray();
                }
            )
            .DisposeWith(disposables);
    }

    public Guid Id { get; }

    public bool IsNew { get; }

    [Reactive]
    public IReadOnlyList<SubTaskViewModel> AvailableDependencyTasksList { get; private set; } = [];

    public SuspendableObservableCollection<SubTaskViewModel> DependsOnList { get; } = [];

    [Reactive]
    public string Name { get; set; } = string.Empty;

    [Reactive]
    public IReadOnlyList<User> AvailableEmployees { get; private set; } = [];

    [Reactive]
    public User? SelectedEmployee { get; set; }

    public UserSelectionTemplate? EmployeeToSelectOnNextLoad { get; set; }

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

    public void AddDependency(SubTaskViewModel dependency) => DependsOnList.Add(dependency);

    public void RemoveDependency(SubTaskViewModel dependency) => DependsOnList.Remove(dependency);

    public async Task RemoveAsync()
    {
        if (!await m_dialogService.ConfirmAsync(
            "Вы уверены что хотите удалить задачу?",
            "Удаление задачи"
        )) return;

        m_onRemove(this);
    }

    public SubTaskTemplate GetTemplate() => new()
    {
        Name = Name,
        Deadline = Deadline.Date - DateTime.Now.Date,
        ExecutorEmployee = SelectedEmployee is null
            ? null
            : new UserSelectionTemplate
            {
                Id = SelectedEmployee.Id,
                Name = SelectedEmployee.Name
            },
        Cost = Cost,
        TimelyBonus = TimelyBonus
    };

    public async Task CreateTemplateAsync()
    {
        var template = GetTemplate();

        var viewModel = m_templateViewModelFactory(template);

        await m_dialogService.ShowAsync(viewModel);
    }

    public SubTaskEdit MapToEdit() => new()
    {
        Id = Id,
        Name = Name,
        ExecutorEmployeeId =
            SelectedEmployee?.Id ?? throw new NotifyUIException($"Выберите сотрудника для задачи {Name}"),
        DependsOnIdList = DependsOnList.Select(it => it.Id).ToArray(),
        Deadline = Deadline,
        Cost = Cost,
        Version = m_subTask?.Version ?? Guid.Empty,
        TimelyBonus = TimelyBonus
    };

    private async Task ToggleCompletionStatusAsync()
    {
        if (m_subTask is null) return;

        m_subTask = await m_subTasksService.SetIsCompletedAsync(m_subTask.Id, !IsCompleted, m_subTask.Version);

        UpdateProperties(m_subTask);
    }

    private async Task TogglePaymentStatusAsync()
    {
        if (m_subTask is null) return;

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
        Deadline = subTask?.Deadline ?? DateTime.Now;
        Cost = subTask?.Cost ?? 0;
        IsCompleted = subTask?.IsCompleted ?? false;
        PaymentStatus = subTask?.PaymentStatus ?? EPaymentStatus.None;
        TimelyBonus = subTask?.TimelyBonus ?? 0;
    }
}