namespace SparkTrack.Core.Repositories;

using Shared.Data;
using Shared.Data.Entities;

public interface IPaymentBillsRepository
{
    Task<IReadOnlyPagedData<PaymentBill>> GetPageAsync(bool isPaid, Guid? employeeId, Guid? projectId, PageQuery pageQuery);

    Task<IReadOnlyList<UserPayment>> GetUsersRemainingPaymentsAsync(Guid? projectId);
    
    Task<PendingPaymentsSummary> GetPendingPaymentsSummaryAsync(Guid? projectId);

    Task<IReadOnlyPagedData<PaymentDetails>> GetPaidPaymentsListAsync(Guid? adminId, Guid? projectId, PageQuery pageQuery);
    
    Task<IReadOnlyPagedData<BonusPaymentInfo>> GetPaidBonusPaymentsListAsync(Guid? adminId, PageQuery pageQuery);

    Task AddPaymentsRangeAsync(IReadOnlyList<PaymentInfo> paymentsList);

    Task AddBonusPaymentAsync(BonusPaymentInfo bonusPaymentInfo);

    Task DeletePaymentAsync(Guid id);

    Task DeleteBonusPaymentAsync(Guid id);
    
    Task<bool> IsPaymentPaidByThisUser(Guid paymentId, Guid userId);
    
    Task<bool> IsBonusPaymentPaidByThisUser(Guid paymentId, Guid userId);
}