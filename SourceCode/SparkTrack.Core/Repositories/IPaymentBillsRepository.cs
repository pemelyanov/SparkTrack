namespace SparkTrack.Core.Repositories;

using Shared.Data;
using Shared.Data.Entities;

public interface IPaymentBillsRepository
{
    public Task<IReadOnlyPagedData<PaymentBill>> GetPageAsync(bool isPaid, Guid? employeeId, Guid? projectId, PageQuery pageQuery);

    public Task<IReadOnlyList<UserPayment>> GetUsersRemainingPaymentsAsync(Guid? projectId);
    
    public Task<PendingPaymentsSummary> GetPendingPaymentsSummaryAsync(Guid? projectId);

    public Task AddPaymentsRangeAsync(IReadOnlyList<PaymentInfo> paymentsList);

    public Task AddBonusPaymentAsync(BonusPaymentInfo bonusPaymentInfo);

    public Task DeletePaymentAsync(Guid id);

    public Task DeleteBonusPaymentAsync(Guid id);
}