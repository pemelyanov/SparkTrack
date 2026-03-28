namespace SparkTrack.AvaloniaImpl.Data.Configurations;

using Core.Shared.Enums;
using Pages.AdminFinance.Tabs.PaymentsHistory;

public struct AdminPaymentsHistoryPageConfig
{
    public Guid? ProjectId { get; init; }
    
    public EPaymentKind? PaymentKind { get; init; }
    
    public Guid? AdminId { get; init; }
    
    public Guid? EmployeeId { get; init; }
    
    public bool? IsDatesFilterEnabled { get; init; }
    
    public DateTime? StartDate { get; init; }
    
    public DateTime? EndDate { get; init; }
    
    public int? ItemsPerPage { get; init; }
}