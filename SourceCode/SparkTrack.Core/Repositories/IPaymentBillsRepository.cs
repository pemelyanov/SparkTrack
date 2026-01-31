namespace SparkTrack.Core.Repositories;

using Shared.Data;
using Shared.Data.Entities;

public interface IPaymentBillsRepository
{
    Task<PaymentInfo?> GetPaymentAsync(Guid id);

    Task<BonusPaymentInfo?> GetBonusPaymentAsync(Guid id);

    Task<IReadOnlyPagedData<PaymentBill>> GetPageAsync(
        bool isPaid,
        Guid? employeeId,
        Guid? projectId,
        PageQuery pageQuery
    );

    Task<IReadOnlyList<UserPayment>> GetUsersRemainingPaymentsAsync(Guid? projectId);

    Task<PendingPaymentsSummary> GetPendingPaymentsSummaryAsync(Guid? projectId);

    Task<IReadOnlyPagedData<PaymentDetails>> GetPaidPaymentsListAsync(
        Guid? adminId,
        Guid? employeeId,
        Guid? projectId,
        DateTime? startDate,
        DateTime? endDate,
        PageQuery pageQuery
    );

    Task<IReadOnlyPagedData<BonusPaymentInfo>> GetPaidBonusPaymentsListAsync(
        Guid? adminId,
        Guid? employeeId,
        DateTime? startDate,
        DateTime? endDate,
        PageQuery pageQuery
    );

    Task AddPaymentsRangeAsync(IReadOnlyList<PaymentInfo> paymentsList);

    Task AddBonusPaymentAsync(BonusPaymentInfo bonusPaymentInfo);

    Task DeletePaymentAsync(Guid id);

    Task DeleteBonusPaymentAsync(Guid id);

    Task<bool> IsPaymentPaidByThisUser(Guid paymentId, Guid userId);

    Task<bool> IsBonusPaymentPaidByThisUser(Guid paymentId, Guid userId);
}