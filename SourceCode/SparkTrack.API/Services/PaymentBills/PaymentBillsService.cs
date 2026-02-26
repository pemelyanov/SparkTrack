namespace SparkTrack.API.Services.PaymentBills;

using Core.Shared.Data;
using Core.Shared.Data.Entities;
using Core.Shared.Extensions;
using Core.Shared.Services.PaymentBills;
using Delegates;
using MappingExtensions;

public class PaymentBillsService(ClientFactory<FinanceClient> financeClientFactory) : IPaymentBillsService
{
    public async Task<IReadOnlyPagedData<PaymentBill>> GetPageAsync(
        bool isPaid,
        Guid? employeeId,
        Guid? projectId,
        DateTime? startDate,
        DateTime? endDate, 
        bool showOnlyMine,
        PageQuery pageQuery
    )
    {
        using var wrapper = financeClientFactory.Invoke();

        var page = await wrapper.Client.GetBillsPageAsync(
            isPaid,
            employeeId,
            projectId,
            startDate?.ToUniversalTime(),
            endDate?.ToUniversalTime(),
            showOnlyMine,
            pageQuery.Page,
            pageQuery.ItemsPerPage
        );

        return new ReadOnlyPagedData<PaymentBill>(page.Items.Select(it => it.ToDomain()).ToArray(), page.Total);
    }

    public async Task<IReadOnlyList<UserPayment>> GetUsersRemainingPaymentsAsync(Guid? projectId, bool showOnlyMine)
    {
        using var wrapper = financeClientFactory.Invoke();

        var list = await wrapper.Client.GetUsersRemainingPaymentsAsync(projectId, showOnlyMine);

        return list.Select(it => it.ToDomain()).ToArray();
    }

    public async Task<PendingPaymentsSummary> GetPendingPaymentsSummaryAsync(Guid? projectId, bool showOnlyMine)
    {
        using var wrapper = financeClientFactory.Invoke();

        var dto = await wrapper.Client.GetPendingPaymentsSummaryAsync(projectId, showOnlyMine);

        return dto.ToDomain();
    }

    public async Task<IReadOnlyPagedData<PaymentDetails>> GetPaidPaymentsListAsync(
        Guid? adminId,
        Guid? employeeId,
        Guid? projectId,
        DateTime? startDate,
        DateTime? endDate,
        PageQuery pageQuery
    )
    {
        using var wrapper = financeClientFactory.Invoke();

        var page = await wrapper.Client.GetAdminPaymentsHistoryAsync(
            adminId,
            employeeId,
            projectId,
            startDate?.ToUniversalTime(),
            endDate?.ToUniversalTime(),
            pageQuery.Page,
            pageQuery.ItemsPerPage
        );

        return page.ReflectionConvert<PaymentDetailsDTO, PaymentDetails>(it => it.ToDomain());
    }

    public async Task<IReadOnlyPagedData<BonusPaymentInfo>> GetPaidBonusPaymentsListAsync(
        Guid? adminId,
        Guid? employeeId,
        DateTime? startDate,
        DateTime? endDate,
        PageQuery pageQuery
    )
    {
        using var wrapper = financeClientFactory.Invoke();

        var page = await wrapper.Client.GetAdminBonusPaymentsHistoryAsync(
            adminId,
            employeeId,
            startDate?.ToUniversalTime(),
            endDate?.ToUniversalTime(),
            pageQuery.Page,
            pageQuery.ItemsPerPage
        );

        return page.ReflectionConvert<BonusPaymentDTO, BonusPaymentInfo>(it => it.ToDomain());
    }

    public async Task PayBillsAsync(IReadOnlyList<Guid> tasksIdList, float payment, float timelyBonusPayment)
    {
        using var wrapper = financeClientFactory.Invoke();

        await wrapper.Client.PayBillsAsync(tasksIdList, payment, timelyBonusPayment);
    }

    public async Task PayBonusAsync(Guid employeeId, float payment, string? comment)
    {
        using var wrapper = financeClientFactory.Invoke();

        await wrapper.Client.PayBonusAsync(employeeId, payment, comment);
    }

    public async Task DeleteBillAsync(Guid id)
    {
        using var wrapper = financeClientFactory.Invoke();

        await wrapper.Client.DeleteBillAsync(id);
    }

    public async Task DeleteBonusAsync(Guid id)
    {
        using var wrapper = financeClientFactory.Invoke();

        await wrapper.Client.DeleteBonusAsync(id);
    }
}