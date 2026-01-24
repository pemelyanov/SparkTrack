namespace SparkTrack.Core.Shared.Services.PaymentBills;

using Data;
using Data.Entities;

public interface IPaymentBillsService
{
    Task<IReadOnlyPagedData<PaymentBill>> GetPageAsync(bool isPaid, Guid? projectId, PageQuery pageQuery);

    Task<IReadOnlyList<UserRemainingPayment>> GetUsersRemainingPaymentsAsync(Guid? projectId);

    Task PayBillsAsync(IReadOnlyList<Guid> tasksIdList, float payment, float timelyBonusPayment);

    Task PayBonusAsync(Guid employeeId, float payment, string? comment);

    Task DeleteBillAsync(Guid id);

    Task DeleteBonusAsync(Guid id);
}