namespace SparkTrack.Core.Shared.Services.PaymentBills;

using Data;
using Data.Entities;

public interface IPaymentBillsService
{
    Task<IReadOnlyPagedData<PaymentBill>> GetPageAsync(bool isPaid, Guid? projectId, PageQuery pageQuery);

    Task<IReadOnlyList<UserRemainingPayment>> GetUsersRemainingPaymentsAsync(Guid? projectId);

    Task PayBillsAsync(IReadOnlyList<Guid> tasksIdList, float payment);

    Task PayBonusAsync(Guid employeeId, float payment);
}