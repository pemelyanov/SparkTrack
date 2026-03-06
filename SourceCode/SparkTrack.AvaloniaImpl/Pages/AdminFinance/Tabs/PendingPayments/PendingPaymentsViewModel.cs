namespace SparkTrack.AvaloniaImpl.Pages.AdminFinance.Tabs.PendingPayments;

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Controls.BonusForm;
using Controls.PaymentForm;
using Controls.ProjectsFilter;
using Controls.UsersFilter;
using Core.Client.Extensions;
using Core.Client.Services.Configuration;
using Core.Shared.Data;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Core.Shared.Services.PaymentBills;
using Core.Shared.Services.SubTasks;
using Data.Configurations;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using Reactive;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.DialogHost;
using ViewModels;

public class PendingPaymentsViewModel : ViewModelBase
{
    private readonly IPaymentBillsService                                  m_paymentBillsService;
    private readonly ISubTasksService                                      m_subTasksService;
    private readonly IDialogService                                        m_dialogService;
    private readonly Func<BonusFormViewModel>                              m_bonusFormViewModelFactory;
    private readonly IConfigurationService<AdminPendingPaymentsPageConfig> m_pageConfig;

    private readonly BehaviorObservableSubject<IReadOnlyList<PaymentBillViewModel>> m_selectedBills = new([]);

    public PendingPaymentsViewModel(
        IPaymentBillsService paymentBillsService,
        ProjectsFilterViewModel projectsFilterViewModel,
        ISubTasksService subTasksService,
        IDialogService dialogService,
        Func<BonusFormViewModel> bonusFormViewModelFactory,
        UserFilterViewModel employeeFilterViewModel,
        IConfigurationService<AdminPendingPaymentsPageConfig> pageConfig
    )
    {
        m_paymentBillsService = paymentBillsService;
        m_subTasksService = subTasksService;
        m_dialogService = dialogService;
        m_bonusFormViewModelFactory = bonusFormViewModelFactory;
        m_pageConfig = pageConfig;
        EmployeeFilterViewModel = employeeFilterViewModel;
        ProjectsFilterViewModel = projectsFilterViewModel;

        employeeFilterViewModel.UserRole = ERole.Employee;

        ReloadTableCommand = CreateReloadTableCommand();
        ToggleIsBonusApprovedCommand =
            ReactiveCommand.CreateFromTask<PaymentBillViewModel, Unit>(ToggleIsBonusApprovedAsync);

        PayForSelectionCommand = ReactiveCommand.CreateFromTask(
            PayForSelectionAsync,
            m_selectedBills.Select(it => it.Count > 0)
        );

        ApproveBonusForSelectionCommand = ReactiveCommand.CreateFromTask(
            () => SetBonusApprovementForSelectionAsync(true),
            m_selectedBills.Select(selection =>
                    selection.Select(it => it.WhenAnyValue(t => t.IsTimelyBonusApproved))
                        .CombineLatest()
                        .Select(it => it.Any(itTrue => !itTrue))
                )
                .Switch()
        );

        UnapproveBonusForSelectionCommand = ReactiveCommand.CreateFromTask(
            () => SetBonusApprovementForSelectionAsync(false),
            m_selectedBills.Select(selection =>
                    selection.Select(it => it.WhenAnyValue(t => t.IsTimelyBonusApproved))
                        .CombineLatest()
                        .Select(it => it.Any(itTrue => itTrue))
                )
                .Switch()
        );

        PayBonusCommand = ReactiveCommand.CreateFromTask(PayBonusAsync);
        
        if(m_pageConfig.Config.ProjectId is { } projectId) ProjectsFilterViewModel.AutoSelectOnceOnNextUpdate(projectId);

        if (m_pageConfig.Config.IsDatesFilterEnabled is { } isDatesFilterEnabled)
            DateRangeViewModel.IsSelected = isDatesFilterEnabled;

        if (m_pageConfig.Config.StartDate is { } startDate) DateRangeViewModel.Model.StartDate = startDate;
        
        if (m_pageConfig.Config.EndDate is { } endDate) DateRangeViewModel.Model.StartDate = endDate;
        
        if (m_pageConfig.Config.ShowOnlyMine is { } showOnlyMine) ShowOnlyMine = showOnlyMine;

        if (m_pageConfig.Config.ShowPaid is { } showPaid) ShowPaid = showPaid;
        
        if (m_pageConfig.Config.EmployeeId is { } employeeId) EmployeeFilterViewModel.AutoSelectOnceOnNextUpdate(employeeId);
        
        if (m_pageConfig.Config.ItemsPerPage is { } itemsPerPage) PaginatorViewModel.ItemsPerPage = itemsPerPage;
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        ProjectsFilterViewModel.WhenAnyValue(it => it.SelectedProject)
            .CombineLatest(PaginatorViewModel.WhenChanged())
            .CombineLatest(EmployeeFilterViewModel.WhenAnyValue(it => it.SelectedUser))
            .CombineLatest(DateRangeViewModel.GetChangingObservable())
            .CombineLatest(this.WhenAnyValue(it => it.ShowPaid))
            .CombineLatest(this.WhenAnyValue(it => it.ShowOnlyMine))
            .Throttle(TimeSpan.FromMilliseconds(50))
            .Select(_ => ReloadTableCommand.Execute())
            .Switch()
            .Subscribe()
            .DisposeWith(disposables);

        this.SetupSelectionList(it => it.CurrentPageData, m_selectedBills)
            .DisposeWith(disposables);

        m_selectedBills
            .CombineLatest(
                m_selectedBills.Select(list => list.Select(it => it.WhenAnyValue(t => t.IsTimelyBonusApproved))
                    .CombineLatest()
                ),
                (list, _) => list
            )
            .Select(it => it.Count == 0
                ? 0
                : it.Sum(b
                    => b.SubTask.Cost
                    - b.PaymentsList.Where(p => p.PaymentType is EPaymentType.Main).Sum(p => p.Payment)
                    + (b.SubTask.IsTimelyBonusApproved
                        ? b.SubTask.TimelyBonus - b.PaymentsList.Where(p => p.PaymentType is EPaymentType.TimelyBonus)
                            .Sum(p => p.Payment)
                        : 0)
                )
            )
            .Subscribe(value => TotalBill = value)
            .DisposeWith(disposables);

        this.WhenAnyValue(it => it.PendingPaymentsSummary)
            .Select(it => it is null ? 0 : it.RemainingPayments.Sum(p => p.Payment))
            .Subscribe(value => TotalRemainingPayments = value)
            .DisposeWith(disposables);
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        
        m_pageConfig.Update(it => it with
        {
            ProjectId = ProjectsFilterViewModel.SelectedProject?.Id,
            ShowOnlyMine = ShowOnlyMine,
            EmployeeId = EmployeeFilterViewModel.SelectedUser?.Id,
            StartDate = DateRangeViewModel.TryGetStartDate(),
            EndDate = DateRangeViewModel.TryGetEndDate(),
            IsDatesFilterEnabled = DateRangeViewModel.IsSelected,
            ShowPaid = ShowPaid,
            ItemsPerPage = PaginatorViewModel.ItemsPerPage
        });
    }

    [Reactive]
    public IReadOnlyList<SelectableViewModel<PaymentBillViewModel>> CurrentPageData { get; private set; } = [];

    [Reactive]
    public PendingPaymentsSummary? PendingPaymentsSummary { get; private set; }

    [Reactive]
    public float TotalRemainingPayments { get; private set; }

    [Reactive]
    public float TotalBill { get; private set; }

    [Reactive]
    public bool ShowPaid { get; set; }

    [Reactive]
    public bool ShowOnlyMine { get; set; } = true;

    public PaginatorViewModel PaginatorViewModel { get; } = new();

    public ProjectsFilterViewModel ProjectsFilterViewModel { get; }

    public UserFilterViewModel EmployeeFilterViewModel { get; }

    public SelectableViewModel<DateRangeViewModel> DateRangeViewModel { get; } = new(new DateRangeViewModel())
    {
        IsSelected = true
    };

    public ReactiveCommand<Unit, Unit> ReloadTableCommand { get; }

    public ReactiveCommand<PaymentBillViewModel, Unit> ToggleIsBonusApprovedCommand { get; }

    public ReactiveCommand<Unit, Unit> ApproveBonusForSelectionCommand { get; }

    public ReactiveCommand<Unit, Unit> UnapproveBonusForSelectionCommand { get; }

    public ReactiveCommand<Unit, Unit> PayForSelectionCommand { get; }

    public ReactiveCommand<Unit, Unit> PayBonusCommand { get; }

    private ReactiveCommand<Unit, Unit> CreateReloadTableCommand() =>
        ReactiveCommand.CreateFromTask(() => Task.WhenAll(ReloadTableAsync(), ReloadRemainingPaymentsAsync())
        );

    private async Task ReloadTableAsync()
    {
        var page = await m_paymentBillsService.GetPageAsync(
            ShowPaid,
            EmployeeFilterViewModel.SelectedUser?.Id,
            ProjectsFilterViewModel.SelectedProject?.Id,
            DateRangeViewModel.TryGetStartDate(),
            DateRangeViewModel.TryGetEndDate(),
            ShowOnlyMine,
            PaginatorViewModel.ToQuery()
        );

        CurrentPageData = page.Items
            .Select(it => new SelectableViewModel<PaymentBillViewModel>(new PaymentBillViewModel(it)))
            .ToArray();
        PaginatorViewModel.SetPagesQuantity(page.Total);
    }

    private async Task ReloadRemainingPaymentsAsync()
    {
        var payments =
            await m_paymentBillsService.GetPendingPaymentsSummaryAsync(ProjectsFilterViewModel.SelectedProject?.Id, ShowOnlyMine);

        PendingPaymentsSummary = payments;
    }

    private async Task<Unit> ToggleIsBonusApprovedAsync(PaymentBillViewModel paymentBillViewModel)
    {
        var value = !paymentBillViewModel.SubTask.IsTimelyBonusApproved;

        var updatedTask = await m_subTasksService.SetIsTimelyBonusApprovedAsync(
            paymentBillViewModel.SubTask.Id,
            value,
            paymentBillViewModel.SubTask.Version
        );

        if (updatedTask is null) return Unit.Default;

        paymentBillViewModel.Update(updatedTask);

        await ReloadRemainingPaymentsAsync();

        return Unit.Default;
    }

    private async Task PayForSelectionAsync()
    {
        var paymentViewModel = new PaymentFormViewModel(m_selectedBills.Value);

        if (await m_dialogService.ShowAsync(paymentViewModel) is not true) return;

        var taskIds = m_selectedBills.Value.Select(it => it.SubTask.Id).ToArray();

        await m_paymentBillsService.PayBillsAsync(
            taskIds,
            DefaultIfNaN(paymentViewModel.EnteredCost),
            DefaultIfNaN(paymentViewModel.EnteredBonus)
        );

        await ReloadTableCommand.Execute().ToTask();
    }

    private async Task SetBonusApprovementForSelectionAsync(bool value)
    {
        var selection = m_selectedBills.Value.ToDictionary(it => it.SubTask.Id);

        var updatedTasks = await m_subTasksService.SetIsTimelyBonusApprovedAsync(
            selection.Values.Select(it => new EditableEntityIdentity(it.SubTask.Id, it.SubTask.Version)).ToArray(),
            value
        );

        foreach (var task in updatedTasks)
            selection[task.Id].Update(task);

        await ReloadRemainingPaymentsAsync();
    }

    private async Task PayBonusAsync()
    {
        var bonusViewModel = m_bonusFormViewModelFactory.Invoke();

        await m_dialogService.ShowAsync(bonusViewModel);

        if (bonusViewModel.UserFilterViewModel.SelectedUser is null || bonusViewModel.Payment <= 0) return;

        await m_paymentBillsService.PayBonusAsync(
            bonusViewModel.UserFilterViewModel.SelectedUser.Id,
            bonusViewModel.Payment,
            bonusViewModel.Comment
        );

        await ReloadTableCommand.Execute().ToTask();
    }

    private float DefaultIfNaN(float number) => float.IsNaN(number) ? 0 : number;
}