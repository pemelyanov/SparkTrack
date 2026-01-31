namespace SparkTrack.AvaloniaImpl.Pages.AdminFinance;

using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using Tabs.PaymentsHistory;
using Tabs.PendingPayments;

public class AdminFinancePageViewModel(
    Lazy<IScreen> hostScreen,
    PendingPaymentsViewModel pendingPaymentsViewModel,
    PaymentsHistoryViewModel paymentsHistoryViewModel
) : ViewModelBase, IRoutableViewModel
{
    public string UrlPathSegment => "admin-finance";

    public IScreen HostScreen => hostScreen.Value;

    public PendingPaymentsViewModel PendingPaymentsViewModel { get; } = pendingPaymentsViewModel;

    public PaymentsHistoryViewModel PaymentsHistoryViewModel { get; } = paymentsHistoryViewModel;
}