using Avalonia;
using Avalonia.Controls;

namespace SparkTrack.AvaloniaImpl.Controls.RemainingPaymentCell;

using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Pages.AdminFinance;

public partial class RemainingPaymentCell : UserControl
{
    public RemainingPaymentCell()
    {
        InitializeComponent();
    }

    #region PaymentType Property

    public static readonly StyledProperty<EPaymentType> PaymentTypeProperty =
        AvaloniaProperty.Register<RemainingPaymentCell, EPaymentType>(nameof(PaymentType));

    public EPaymentType PaymentType
    {
        get => GetValue(PaymentTypeProperty);
        set => SetValue(PaymentTypeProperty, value);
    }

    #endregion

    #region PaymentBill Property

    public static readonly StyledProperty<PaymentBillViewModel?> PaymentBillProperty =
        AvaloniaProperty.Register<RemainingPaymentCell, PaymentBillViewModel?>(nameof(PaymentBill));

    public PaymentBillViewModel? PaymentBill
    {
        get => GetValue(PaymentBillProperty);
        set => SetValue(PaymentBillProperty, value);
    }

    #endregion

    #region TotalPayment Property

    public static readonly StyledProperty<float> TotalPaymentProperty =
        AvaloniaProperty.Register<RemainingPaymentCell, float>(nameof(TotalPayment));

    public float TotalPayment
    {
        get => GetValue(TotalPaymentProperty);
        private set => SetValue(TotalPaymentProperty, value);
    }

    #endregion

    #region PaymentsList Property

    public static readonly StyledProperty<IReadOnlyList<PaymentInfo>> PaymentsListProperty =
        AvaloniaProperty.Register<RemainingPaymentCell, IReadOnlyList<PaymentInfo>>(nameof(PaymentsList));

    public IReadOnlyList<PaymentInfo> PaymentsList
    {
        get => GetValue(PaymentsListProperty);
        private set => SetValue(PaymentsListProperty, value);
    }

    #endregion

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if(change.Property != PaymentTypeProperty && change.Property != PaymentBillProperty || PaymentBill is null) return;

        TotalPayment = PaymentType switch
        {
            EPaymentType.Main => PaymentBill.SubTask.Cost,
            EPaymentType.TimelyBonus => PaymentBill.SubTask.TimelyBonus,
            _ => throw new NotSupportedException()
        };

        PaymentsList = PaymentBill.PaymentsList.Where(it => it.PaymentType == PaymentType).ToArray();
    }
}