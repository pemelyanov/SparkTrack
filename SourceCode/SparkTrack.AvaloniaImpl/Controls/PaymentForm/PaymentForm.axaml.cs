using SparkTrack.AvaloniaImpl.Windows;

namespace SparkTrack.AvaloniaImpl.Controls.PaymentForm;

using FluentAvalonia.UI.Controls;

public partial class PaymentForm : ReactiveContentDialog<PaymentFormViewModel>
{
    public PaymentForm()
    {
        InitializeComponent();
    }

    protected override void OnPrimaryButtonClick(ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;
    }
}