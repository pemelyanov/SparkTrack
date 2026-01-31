namespace SparkTrack.AvaloniaImpl.Pages.AdminFinance.Tabs.PaymentsHistory;

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Controls.ProjectsFilter;
using Core.Shared.Data.Entities;
using Core.Shared.Services.PaymentBills;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViewModels;

public class PaymentsHistoryViewModel : ViewModelBase
{
    private readonly IPaymentBillsService m_paymentBillsService;

    public PaymentsHistoryViewModel(ProjectsFilterViewModel projectsFilterViewModel, IPaymentBillsService paymentBillsService)
    {
        m_paymentBillsService = paymentBillsService;
        ProjectsFilterViewModel = projectsFilterViewModel;

        ReloadTableCommand = ReactiveCommand.CreateFromTask(ReloadTableAsync);
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        ProjectsFilterViewModel.WhenAnyValue(it => it.SelectedProject)
            .CombineLatest(this.WhenAnyValue(it => it.SelectedPaymentKind))
            .CombineLatest(PaginatorViewModel.WhenChanged())
            .Throttle(TimeSpan.FromMilliseconds(50))
            .Select(_ => ReloadTableCommand.Execute())
            .Switch()
            .Subscribe()
            .DisposeWith(disposables);
    }

    public PaginatorViewModel PaginatorViewModel { get; } = new();

    public ProjectsFilterViewModel ProjectsFilterViewModel { get; }

    public IReadOnlyList<PaymentKind> PaymentKinds { get; } = [PaymentKind.Primary.Instance, PaymentKind.Bonus.Instance];

    [Reactive]
    public PaymentKind SelectedPaymentKind { get; set; } = PaymentKind.Primary.Instance;

    [Reactive]
    public IReadOnlyList<PaymentDetails> PaymentDetailsPageData { get; private set; } = [];

    [Reactive]
    public IReadOnlyList<BonusPaymentInfo> BonusPaymentsPageData { get; private set; } = [];
    
    public ReactiveCommand<Unit, Unit> ReloadTableCommand { get; }
    
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
            null,
            ProjectsFilterViewModel.SelectedProject?.Id,
            PaginatorViewModel.ToQuery()
        );

        PaymentDetailsPageData = page.Items;
        PaginatorViewModel.SetPagesQuantity(page.Total);
    }
    
    private async Task LoadBonusHistory()
    {
        var page = await m_paymentBillsService.GetPaidBonusPaymentsListAsync(
            null,
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