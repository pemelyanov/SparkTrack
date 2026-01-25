using Avalonia.Controls;

namespace SparkTrack.AvaloniaImpl.Pages.AdminFinance.Tabs.PendingPayments;

using Avalonia.Input;
using Avalonia.ReactiveUI;
using ViewModels;

public partial class PendingPayments : ReactiveUserControl<PendingPaymentsViewModel>
{
    public PendingPayments()
    {
        InitializeComponent();
    }
    
    private void DataGrid_OnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        e.Row.Tapped += RowOnTapped;
    }
    
    private void DataGrid_OnUnloadingRow(object? sender, DataGridRowEventArgs e)
    {
        e.Row.Tapped -= RowOnTapped;
    }

    private void RowOnTapped(object? sender, TappedEventArgs e)
    {
        if(sender is not Control { DataContext: SelectableViewModel<PaymentBillViewModel> itemViewModel }) return;

        itemViewModel.IsSelected = !itemViewModel.IsSelected;
    }
}