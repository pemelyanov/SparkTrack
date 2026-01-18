namespace SparkTrack.Core.Repositories;

using Shared.Data;
using Shared.Data.Entities;

public interface IPaymentBillsRepository
{
    public Task<IReadOnlyPagedData<PaymentBill>> GetPageAsync(bool isPaid, Guid? employeeId, Guid? projectId, PageQuery pageQuery);

    public Task<IReadOnlyList<UserRemainingPayment>> GetUsersRemainingPaymentsAsync(Guid? projectId);
}