namespace SparkTrack.AvaloniaImpl.Pages.AdminFinance;

using Controls.ProjectsFilter;
using Core.Shared.Data.Entities;
using Core.Shared.Services.PaymentBills;
using Core.Shared.Services.SubTasks;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using ViewModels;

public class AdminFinancePageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly Lazy<IScreen>        m_hostScreen;
    private readonly IPaymentBillsService m_paymentBillsService;
    private readonly ISubTasksService     m_subTasksService;

    public AdminFinancePageViewModel(
        Lazy<IScreen> hostScreen,
        IPaymentBillsService paymentBillsService,
        ProjectsFilterViewModel projectsFilterViewModel,
        ISubTasksService subTasksService
    )
    {
        m_hostScreen = hostScreen;
        m_paymentBillsService = paymentBillsService;
        m_subTasksService = subTasksService;
        ProjectsFilterViewModel = projectsFilterViewModel;

        ReloadTableCommand = CreateReloadTableCommand();
        ToggleIsBonusApprovedCommand =
            ReactiveCommand.CreateFromTask<PaymentBillViewModel, Unit>(ToggleIsBonusApprovedAsync);

        var selectedItemsPipe = this.WhenAnyValue(it => it.CurrentPageData)
            .Select(
                list => list.Select(
                        it => it.WhenAnyValue(v => v.IsSelected)
                            .Select(
                                isSelected => new
                                {
                                    it,
                                    isSelected
                                }
                            )
                    )
                    .CombineLatest()
            )
            .Switch()
            .Select(list => list.Where(it => it.isSelected).Select(it => it.it).ToArray())
            .Throttle(TimeSpan.FromMilliseconds(50))
            .ObserveOn(RxApp.MainThreadScheduler);

        PayForSelectionCommand = ReactiveCommand.CreateFromTask(
            PayForSelectionAsync,
            selectedItemsPipe.Select(it => it.Length > 0)
        );
        ApproveBonusForSelectionCommand = ReactiveCommand.CreateFromTask(
            () => SetBonusApprovementForSelectionAsync(true),
            selectedItemsPipe.Select(selection => selection.Any(it => !it.Model.IsTimelyBonusApproved))
        );
        UnapproveBonusForSelectionCommand = ReactiveCommand.CreateFromTask(
            () => SetBonusApprovementForSelectionAsync(false),
            selectedItemsPipe.Select(selection => selection.Any(it => it.Model.IsTimelyBonusApproved))
        );
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        ProjectsFilterViewModel.WhenAnyValue(it => it.SelectedProject)
            .CombineLatest(PaginatorViewModel.WhenChanged())
            .Throttle(TimeSpan.FromMilliseconds(50))
            .Select(_ => ReloadTableCommand.Execute())
            .Switch()
            .Subscribe()
            .DisposeWith(disposables);
    }

    public string UrlPathSegment => "admin-finance";

    public IScreen HostScreen => m_hostScreen.Value;

    [Reactive]
    public IReadOnlyList<SelectableViewModel<PaymentBillViewModel>> CurrentPageData { get; set; } = [];

    public PaginatorViewModel PaginatorViewModel { get; } = new();

    public ProjectsFilterViewModel ProjectsFilterViewModel { get; }

    public ReactiveCommand<Unit, Unit> ReloadTableCommand { get; }

    public ReactiveCommand<PaymentBillViewModel, Unit> ToggleIsBonusApprovedCommand { get; }

    public ReactiveCommand<Unit, Unit> ApproveBonusForSelectionCommand { get; }

    public ReactiveCommand<Unit, Unit> UnapproveBonusForSelectionCommand { get; }

    public ReactiveCommand<Unit, Unit> PayForSelectionCommand { get; }

    private ReactiveCommand<Unit, Unit> CreateReloadTableCommand() => ReactiveCommand.CreateFromTask(
        async () =>
        {
            var page = await m_paymentBillsService.GetPageAsync(
                false,
                ProjectsFilterViewModel.SelectedProject?.Id,
                PaginatorViewModel.ToQuery()
            );

            CurrentPageData = page.Items
                .Select(it => new SelectableViewModel<PaymentBillViewModel>(new PaymentBillViewModel(it)))
                .ToArray();
            PaginatorViewModel.SetPagesQuantity(page.Total);
        }
    );

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

        return Unit.Default;
    }

    private async Task PayForSelectionAsync()
    {
        await ReloadTableCommand.Execute().ToTask();
    }

    private async Task SetBonusApprovementForSelectionAsync(bool value) { }
}