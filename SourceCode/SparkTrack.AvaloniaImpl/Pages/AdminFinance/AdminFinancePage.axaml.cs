using Avalonia.Controls;
using Avalonia.Input;
using SparkTrack.AvaloniaImpl.ViewModels;

namespace SparkTrack.AvaloniaImpl.Pages.AdminFinance;

using Avalonia.ReactiveUI;
using ReactiveUI;

[SingleInstanceView]
public partial class AdminFinancePage : ReactiveUserControl<AdminFinancePageViewModel>
{
    public AdminFinancePage()
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