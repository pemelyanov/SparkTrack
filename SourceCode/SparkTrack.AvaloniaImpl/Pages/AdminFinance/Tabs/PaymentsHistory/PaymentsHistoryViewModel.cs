namespace SparkTrack.AvaloniaImpl.Pages.AdminFinance.Tabs.PaymentsHistory;

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Controls.ProjectsFilter;
using Controls.UsersFilter;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Core.Shared.Services.PaymentBills;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.DialogHost;
using ViewModels;

public class PaymentsHistoryViewModel : ViewModelBase
{
    private readonly IPaymentBillsService m_paymentBillsService;
    private readonly IDialogService       m_dialogService;

    public PaymentsHistoryViewModel(
        ProjectsFilterViewModel projectsFilterViewModel,
        IPaymentBillsService paymentBillsService,
        IDialogService dialogService,
        UserFilterViewModel adminFilterViewModel,
        UserFilterViewModel employeeFilterViewModel
    )
    {
        m_paymentBillsService = paymentBillsService;
        m_dialogService = dialogService;
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
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        ProjectsFilterViewModel.WhenAnyValue(it => it.SelectedProject)
            .CombineLatest(this.WhenAnyValue(it => it.SelectedPaymentKind))
            .CombineLatest(PaginatorViewModel.WhenChanged())
            .CombineLatest(AdminFilterViewModel.WhenAnyValue(it => it.SelectedUser))
            .CombineLatest(EmployeeFilterViewModel.WhenAnyValue(it => it.SelectedUser))
            .CombineLatest(DateRangeViewModel.WhenAnyValue(it => it.IsSelected))
            .CombineLatest(DateRangeViewModel.Model.WhenAnyValue(it => it.StartDate))
            .CombineLatest(DateRangeViewModel.Model.WhenAnyValue(it => it.EndDate))
            .Throttle(TimeSpan.FromMilliseconds(50))
            .Select(_ => ReloadTableCommand.Execute())
            .Switch()
            .Subscribe()
            .DisposeWith(disposables);
    }

    public PaginatorViewModel PaginatorViewModel { get; } = new();

    public ProjectsFilterViewModel ProjectsFilterViewModel { get; }

    public UserFilterViewModel AdminFilterViewModel { get; }

    public UserFilterViewModel EmployeeFilterViewModel { get; }

    public SelectableViewModel<DateRangeViewModel> DateRangeViewModel { get; } = new(new DateRangeViewModel())
    {
        IsSelected = true
    };

    public IReadOnlyList<PaymentKind> PaymentKinds { get; } =
        [PaymentKind.Primary.Instance, PaymentKind.Bonus.Instance];

    [Reactive]
    public PaymentKind SelectedPaymentKind { get; set; } = PaymentKind.Primary.Instance;

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
        if (SelectedPaymentKind is PaymentKind.Bonus)
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
            DateRangeViewModel.IsSelected ? DateRangeViewModel.Model.StartDate : null,
            DateRangeViewModel.IsSelected ? DateRangeViewModel.Model.EndDate : null,
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

public abstract record PaymentKind(string Name)
{
    public record Primary() : PaymentKind("Постоянные платежи")
    {
        public static Primary Instance { get; } = new();
    }

    public record Bonus() : PaymentKind("Премия")
    {
        public static Bonus Instance { get; } = new();
    }
}