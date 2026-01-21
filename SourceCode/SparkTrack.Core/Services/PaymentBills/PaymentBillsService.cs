namespace SparkTrack.Core.Services.PaymentBills;

using Authorization;
using Extensions;
using Repositories;
using Shared.Data;
using Shared.Data.Entities;
using Shared.Services.PaymentBills;

public class PaymentBillsService(
    IPaymentBillsRepository paymentBillsRepository,
    IAuthorizationService authorizationService
) : IPaymentBillsService
{
    public Task<IReadOnlyPagedData<PaymentBill>> GetPageAsync(bool isPaid, Guid? projectId, PageQuery pageQuery)
    {
        var currentUser = authorizationService.GetUserOrThrowIfUnauthorized();

        var employeeId = currentUser.GetEmployeeIdOrNull();

        return paymentBillsRepository.GetPageAsync(isPaid, employeeId, projectId, pageQuery);
    }

    public Task<IReadOnlyList<UserRemainingPayment>> GetUsersRemainingPaymentsAsync(Guid? projectId) =>
        paymentBillsRepository.GetUsersRemainingPaymentsAsync(projectId);

    public Task PayBillsAsync(IReadOnlyList<Guid> tasksIdList, float payment)
    {
        throw new NotImplementedException();
    }

    public Task PayBonusAsync(Guid employeeId, float payment) => throw new NotImplementedException();
}