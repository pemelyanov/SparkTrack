namespace SparkTrack.API.Services.PaymentBills;

using Core.Shared.Data;
using Core.Shared.Data.Entities;
using Core.Shared.Services.PaymentBills;
using Delegates;
using MappingExtensions;

public class PaymentBillsService(ClientFactory<FinanceClient> financeClientFactory) : IPaymentBillsService
{
    public async Task<IReadOnlyPagedData<PaymentBill>> GetPageAsync(bool isPaid, Guid? projectId, PageQuery pageQuery)
    {
        using var wrapper = financeClientFactory.Invoke();

        var page = await wrapper.Client.GetBillsPageAsync(isPaid, projectId, pageQuery.Page, pageQuery.ItemsPerPage);

        return new ReadOnlyPagedData<PaymentBill>(page.Items.Select(it => it.ToDomain()).ToArray(), page.Total);
    }

    public async Task<IReadOnlyList<UserRemainingPayment>> GetUsersRemainingPaymentsAsync(Guid? projectId)
    {
        using var wrapper = financeClientFactory.Invoke();

        var list = await wrapper.Client.GetUsersRemainingPaymentsAsync(projectId);

        return list.Select(it => it.ToDomain()).ToArray();
    }
}