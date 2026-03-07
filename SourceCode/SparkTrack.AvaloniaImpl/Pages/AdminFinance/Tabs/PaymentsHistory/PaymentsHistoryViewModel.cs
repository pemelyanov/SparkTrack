namespace SparkTrack.AvaloniaImpl.Pages.AdminFinance.Tabs.PaymentsHistory;

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Controls.ProjectsFilter;
using Controls.UsersFilter;
using Core.Client.Extensions;
using Core.Client.Services.Configuration;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Core.Shared.Services.PaymentBills;
using Data.Configurations;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.DialogHost;
using ViewModels;

public class PaymentsHistoryViewModel : ViewModelBase
{
    private readonly IPaymentBillsService                                  m_paymentBillsService;
    private readonly IDialogService                                        m_dialogService;
    private readonly IConfigurationService<AdminPaymentsHistoryPageConfig> m_pageConfig;

    public PaymentsHistoryViewModel(
        ProjectsFilterViewModel projectsFilterViewModel,
        IPaymentBillsService paymentBillsService,
        IDialogService dialogService,
        UserFilterViewModel adminFilterViewModel,
        UserFilterViewModel employeeFilterViewModel,
        IConfigurationService<AdminPaymentsHistoryPageConfig> pageConfig
    )
    {
        m_paymentBillsService = paymentBillsService;
        m_dialogService = dialogService;
        m_pageConfig = pageConfig;
        AdminFilterViewModel = adminFilterViewModel;
        EmployeeFilterViewModel = employeeFilterViewModel;
        ProjectsFilterViewModel = projectsFilterViewModel;

        adminFilterViewModel.UserRole = ERole.Admin;
        employeeFilterViewModel.UserRole = ERole.Employee;

        ReloadTableCommand = ReactiveCommand.CreateFromTask(ReloadTableAsync);
        DeleteEntryCommand = ReactiveCommand.CreateFromTask<object, Unit>(async (entry) =>
            {
                await DeleteEntryAsync(entry);
                return Unit.Default;
            }
        );
        
        if(m_pageConfig.Config.ProjectId is { } projectId) ProjectsFilterViewModel.AutoSelectOnceOnNextUpdate(projectId);

        if (m_pageConfig.Config.IsDatesFilterEnabled is { } isDatesFilterEnabled)
            DateRangeViewModel.IsSelected = isDatesFilterEnabled;

        if (m_pageConfig.Config.StartDate is { } startDate) DateRangeViewModel.Model.StartDate = startDate;
        
        if (m_pageConfig.Config.EndDate is { } endDate) DateRangeViewModel.Model.EndDate = endDate;

        if (m_pageConfig.Config.PaymentKind is { } paymentKind) SelectedPaymentKind = paymentKind;
        
        if (m_pageConfig.Config.EmployeeId is { } employeeId) EmployeeFilterViewModel.AutoSelectOnceOnNextUpdate(employeeId);
        
        if (m_pageConfig.Config.AdminId is { } adminId) AdminFilterViewModel.AutoSelectOnceOnNextUpdate(adminId);
        
        if (m_pageConfig.Config.ItemsPerPage is { } itemsPerPage) PaginatorViewModel.ItemsPerPage = itemsPerPage;
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        ProjectsFilterViewModel.WhenAnyValue(it => it.SelectedProject)
            .CombineLatest(this.WhenAnyValue(it => it.SelectedPaymentKind))
            .CombineLatest(PaginatorViewModel.WhenChanged())
            .CombineLatest(AdminFilterViewModel.WhenAnyValue(it => it.SelectedUser))
            .CombineLatest(EmployeeFilterViewModel.WhenAnyValue(it => it.SelectedUser))
            .CombineLatest(DateRangeViewModel.GetChangingObservable())
            .Throttle(TimeSpan.FromMilliseconds(50))
            .Select(_ => ReloadTableCommand.Execute())
            .Switch()
            .Subscribe()
            .DisposeWith(disposables);
    }
    
    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        
        m_pageConfig.Update(it => it with
        {
            ProjectId = ProjectsFilterViewModel.SelectedProject?.Id,
            PaymentKind = SelectedPaymentKind,
            EmployeeId = EmployeeFilterViewModel.SelectedUser?.Id,
            AdminId = AdminFilterViewModel.SelectedUser?.Id,
            StartDate = DateRangeViewModel.TryGetStartDate(),
            EndDate = DateRangeViewModel.TryGetEndDate(),
            IsDatesFilterEnabled = DateRangeViewModel.IsSelected,
            ItemsPerPage = PaginatorViewModel.ItemsPerPage
        });
    }

    public PaginatorViewModel PaginatorViewModel { get; } = new();

    public ProjectsFilterViewModel ProjectsFilterViewModel { get; }

    public UserFilterViewModel AdminFilterViewModel { get; }

    public UserFilterViewModel EmployeeFilterViewModel { get; }

    public SelectableViewModel<DateRangeViewModel> DateRangeViewModel { get; } = new(new DateRangeViewModel())
    {
        IsSelected = true
    };

    public IReadOnlyList<EPaymentKind> PaymentKinds { get; } =
        [EPaymentKind.Primary, EPaymentKind.Bonus];

    [Reactive]
    public EPaymentKind SelectedPaymentKind { get; set; } = EPaymentKind.Primary;

    [Reactive]
    public IReadOnlyList<PaymentDetails> PaymentDetailsPageData { get; private set; } = [];

    [Reactive]
    public IReadOnlyList<BonusPaymentInfo> BonusPaymentsPageData { get; private set; } = [];

    public ReactiveCommand<Unit, Unit> ReloadTableCommand { get; }

    public ReactiveCommand<Object, Unit> DeleteEntryCommand { get; }

    private async Task DeleteEntryAsync(object entry)
    {
        if (!await m_dialogService.ConfirmAsync("Вы уверены что хотите удалить платеж?")) return;

        if (entry is PaymentDetails paymentDetails) await m_paymentBillsService.DeleteBillAsync(paymentDetails.Id);
        if (entry is BonusPaymentInfo bonusPaymentInfo)
            await m_paymentBillsService.DeleteBonusAsync(bonusPaymentInfo.Id);

        await ReloadTableCommand.Execute().ToTask();
    }

    private async Task ReloadTableAsync()
    {
        if (SelectedPaymentKind is EPaymentKind.Bonus)
        {
            await LoadBonusHistory();
            return;
        }

        await LoadPrimaryHistory();
    }

    private async Task LoadPrimaryHistory()
    {
        var page = await m_paymentBillsService.GetPaidPaymentsListAsync(
            AdminFilterViewModel.SelectedUser?.Id,
            EmployeeFilterViewModel.SelectedUser?.Id,
            ProjectsFilterViewModel.SelectedProject?.Id,
            DateRangeViewModel.TryGetStartDate(),
            DateRangeViewModel.TryGetEndDate(),
            PaginatorViewModel.ToQuery()
        );

        PaymentDetailsPageData = page.Items;
        PaginatorViewModel.SetPagesQuantity(page.Total);
    }

    private async Task LoadBonusHistory()
    {
        var page = await m_paymentBillsService.GetPaidBonusPaymentsListAsync(
            AdminFilterViewModel.SelectedUser?.Id,
            EmployeeFilterViewModel.SelectedUser?.Id,
            DateRangeViewModel.IsSelected ? DateRangeViewModel.Model.StartDate : null,
            DateRangeViewModel.IsSelected ? DateRangeViewModel.Model.EndDate : null,
            PaginatorViewModel.ToQuery()
        );

        BonusPaymentsPageData = page.Items;
        PaginatorViewModel.SetPagesQuantity(page.Total);
    }
}