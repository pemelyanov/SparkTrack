namespace SparkTrack.AvaloniaImpl.Pages.AdminFinance;

using Core.Shared.Data.Entities;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

public class PaymentBillViewModel(PaymentBill bill) : ReactiveObject
{
    public Feature Feature { get; } = bill.Feature;

    [Reactive]
    public SubTask SubTask { get; private set; } = bill.SubTask;

    [Reactive]
    public bool IsTimelyBonusApproved { get; private set; } = bill.SubTask.IsTimelyBonusApproved;

    [Reactive]
    public IReadOnlyList<PaymentInfo> PaymentsList { get; private set; } = bill.PaymentsList;

    public void Update(SubTask subTask)
    {
        SubTask = subTask;
        IsTimelyBonusApproved = subTask.IsTimelyBonusApproved;
    }
}