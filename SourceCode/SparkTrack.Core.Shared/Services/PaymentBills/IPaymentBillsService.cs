namespace SparkTrack.Core.Shared.Services.PaymentBills;

using Data;
using Data.Entities;

public interface IPaymentBillsService
{
    public Task<IReadOnlyPagedData<PaymentBill>> GetPageAsync(bool isPaid, Guid? projectId, PageQuery pageQuery);

    public Task<IReadOnlyList<UserRemainingPayment>> GetUsersRemainingPaymentsAsync(Guid? projectId);
}