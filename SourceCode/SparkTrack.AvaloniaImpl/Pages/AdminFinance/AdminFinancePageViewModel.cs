namespace SparkTrack.AvaloniaImpl.Pages.AdminFinance;

using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using Tabs.PendingPayments;

public class AdminFinancePageViewModel(Lazy<IScreen> hostScreen, PendingPaymentsViewModel pendingPaymentsViewModel) : ViewModelBase, IRoutableViewModel
{
    public string UrlPathSegment => "admin-finance";

    public IScreen HostScreen => hostScreen.Value;
    
    public PendingPaymentsViewModel PendingPaymentsViewModel { get; } = pendingPaymentsViewModel;
}