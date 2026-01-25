using System.Reactive.Disposables;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SparkTrack.AvaloniaImpl.ViewModels;

namespace SparkTrack.AvaloniaImpl.Controls.PaymentForm;

using Core.Shared.Enums;
using Pages.AdminFinance;

public class PaymentFormViewModel : DialogViewModelBase
{
    public PaymentFormViewModel(IReadOnlyList<PaymentBillViewModel> selectedBills)
    {
        var totalCost = selectedBills.Sum(it => it.SubTask.Cost);
        var totalTimelyBonus = selectedBills.Where(it => it.SubTask.IsTimelyBonusApproved)
            .Sum(it => it.SubTask.TimelyBonus);

        var paidCost = selectedBills.Sum(it =>
            it.PaymentsList.Where(p => p.PaymentType == EPaymentType.Main).Sum(p => p.Payment)
        );

        var paidTimelyBonus = selectedBills.Sum(it =>
            it.PaymentsList.Where(p => p.PaymentType == EPaymentType.TimelyBonus).Sum(p => p.Payment)
        );

        TotalCost = Math.Max(totalCost - paidCost, 0);
        TotalTimelyBonus = Math.Max(totalTimelyBonus - paidTimelyBonus, 0);

        EnteredCost = Math.Min(totalCost / 2, TotalCost);
        EnteredBonus = TotalTimelyBonus;

        CanEnterBonus = selectedBills.Select(it => it.SubTask.ExecutorEmployee.Id).Distinct().Count() == 1;
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);
        
        this.WhenAnyValue(it => it.EnteredCost)
            .Subscribe(value => CostRatio = value / TotalCost)
            .DisposeWith(disposables);

        this.WhenAnyValue(it => it.EnteredTimelyBonus)
            .Subscribe(value => TimelyBonusRatio = value / TotalTimelyBonus)
            .DisposeWith(disposables);

        this.WhenAnyValue(it => it.CostRatio)
            .Subscribe(ratio => EnteredCost = TotalCost * ratio)
            .DisposeWith(disposables);

        this.WhenAnyValue(it => it.TimelyBonusRatio)
            .Subscribe(ratio => EnteredTimelyBonus = TotalTimelyBonus * ratio)
            .DisposeWith(disposables);
    }

    [Reactive]
    public float TotalCost { get; set; }

    [Reactive]
    public float TotalTimelyBonus { get; set; }

    [Reactive]
    public float EnteredCost { get; set; }

    [Reactive]
    public float EnteredTimelyBonus { get; set; }

    [Reactive]
    public float EnteredBonus { get; set; }

    [Reactive]
    public float CostRatio { get; set; }

    [Reactive]
    public float TimelyBonusRatio { get; set; }
    
    public  bool CanEnterBonus { get; }
}