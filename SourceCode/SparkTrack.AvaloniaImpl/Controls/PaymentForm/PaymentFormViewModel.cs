using System.Reactive.Disposables;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SparkTrack.AvaloniaImpl.ViewModels;

namespace SparkTrack.AvaloniaImpl.Controls.PaymentForm;

public class PaymentFormViewModel : DialogViewModelBase
{
    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        this.WhenAnyValue(it => it.CostRatio)
            .Subscribe(ratio => EnteredCost = TotalCost * ratio)
            .DisposeWith(disposables);
        
        this.WhenAnyValue(it => it.TimelyBonusRatio)
            .Subscribe(ratio => EnteredTimelyBonus = TotalTimelyBonus * ratio)
            .DisposeWith(disposables);
        
        this.WhenAnyValue(it => it.EnteredCost)
            .Subscribe(value => CostRatio = value / TotalCost)
            .DisposeWith(disposables);
        
        this.WhenAnyValue(it => it.EnteredTimelyBonus)
            .Subscribe(value => TimelyBonusRatio = value / TotalTimelyBonus)
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
    public float CostRatio { get; set; } = 0.5f;

    [Reactive]
    public float TimelyBonusRatio { get; set; } = 1f;
}