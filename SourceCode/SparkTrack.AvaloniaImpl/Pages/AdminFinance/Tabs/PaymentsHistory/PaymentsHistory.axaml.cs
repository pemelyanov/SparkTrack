namespace SparkTrack.AvaloniaImpl.Pages.AdminFinance.Tabs.PaymentsHistory;

using Avalonia.Data.Converters;
using Avalonia.ReactiveUI;

public partial class PaymentsHistory : ReactiveUserControl<PaymentsHistoryViewModel>
{
    public PaymentsHistory()
    {
        InitializeComponent();
    }

    public static IValueConverter PaymentTypeConverter { get; } = new FuncValueConverter<EPaymentKind, string>(type =>
        type switch
        {
            EPaymentKind.Bonus => "Премия",
            EPaymentKind.Primary => "Постоянные платежи",
            _ => string.Empty
        }
    );
}