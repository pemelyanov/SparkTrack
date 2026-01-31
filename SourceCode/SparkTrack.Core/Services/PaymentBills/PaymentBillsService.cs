namespace SparkTrack.Core.Services.PaymentBills;

using Authorization;
using Exceptions;
using Extensions;
using Repositories;
using Shared.Data;
using Shared.Data.Entities;
using Shared.Enums;
using Shared.Services.PaymentBills;
using Transactions;

public class PaymentBillsService(
    IPaymentBillsRepository paymentBillsRepository,
    ISubTasksRepository subTasksRepository,
    IAuthorizationService authorizationService,
    ITransactionWrapper transactionWrapper
) : IPaymentBillsService
{
    public Task<IReadOnlyPagedData<PaymentBill>> GetPageAsync(bool isPaid, Guid? projectId, PageQuery pageQuery)
    {
        var currentUser = authorizationService.GetUserOrThrowIfUnauthorized();

        var employeeId = currentUser.GetEmployeeIdOrNull();

        return paymentBillsRepository.GetPageAsync(isPaid, employeeId, projectId, pageQuery);
    }

    public Task<IReadOnlyList<UserPayment>> GetUsersRemainingPaymentsAsync(Guid? projectId) =>
        paymentBillsRepository.GetUsersRemainingPaymentsAsync(projectId);

    public Task<PendingPaymentsSummary> GetPendingPaymentsSummaryAsync(Guid? projectId) =>
        paymentBillsRepository.GetPendingPaymentsSummaryAsync(projectId);

    public Task<IReadOnlyPagedData<PaymentDetails>> GetPaidPaymentsListAsync(
        Guid? adminId,
        Guid? employeeId,
        Guid? projectId,
        DateTime? startDate,
        DateTime? endDate,
        PageQuery pageQuery
    ) => paymentBillsRepository.GetPaidPaymentsListAsync(adminId, employeeId, projectId, startDate, endDate, pageQuery);

    public Task<IReadOnlyPagedData<BonusPaymentInfo>> GetPaidBonusPaymentsListAsync(
        Guid? adminId,
        Guid? employeeId,
        DateTime? startDate,
        DateTime? endDate,
        PageQuery pageQuery
    ) => paymentBillsRepository.GetPaidBonusPaymentsListAsync(adminId, employeeId, startDate, endDate, pageQuery);

    public async Task PayBillsAsync(IReadOnlyList<Guid> tasksIdList, float payment, float timelyBonusPayment)
    {
        var admin = authorizationService.GetUserOrThrowIfNotInRole(ERole.Admin);

        var tasks = await subTasksRepository.GetListAsync(tasksIdList);

        var totalMainPayments = tasks.Sum(it => it.RemainingMainPayment
        );

        var totalTimelyBonusPayments = tasks.Where(it => it is { IsTimelyBonusApproved: true, TimelyBonus: > 0 })
            .Sum(it => it.RemainingTimelyBonusPayment
            );

        var mainPaymentsRatio = payment / totalMainPayments;
        var timelyBonusPaymentRatio = totalTimelyBonusPayments is 0 ? 0 : timelyBonusPayment / totalTimelyBonusPayments;

        var paymentsList = new List<PaymentInfo>();
        var paidTasks = new List<SubTask>();

        foreach (var task in tasks)
        {
            var currentMainPayment = task.RemainingMainPayment * mainPaymentsRatio;

            if (currentMainPayment > 0)
            {
                paymentsList.Add(
                    new PaymentInfo
                    {
                        Admin = admin,
                        Payment = currentMainPayment,
                        PaymentType = EPaymentType.Main,
                        TaskId = task.Id,
                        CreatedAt = DateTime.UtcNow
                    }
                );
            }

            bool isMainPaymentCompleted = task.RemainingMainPayment - currentMainPayment <= 0;

            var currentTimelyBonusPayment = task.RemainingTimelyBonusPayment * timelyBonusPaymentRatio;

            if (currentTimelyBonusPayment > 0)
            {
                paymentsList.Add(
                    new PaymentInfo
                    {
                        Admin = admin,
                        Payment = currentTimelyBonusPayment,
                        PaymentType = EPaymentType.TimelyBonus,
                        TaskId = task.Id,
                        CreatedAt = DateTime.UtcNow
                    }
                );
            }

            bool isTimelyBonusPaymentCompleted = task.RemainingTimelyBonusPayment - currentTimelyBonusPayment <= 0;

            if (isMainPaymentCompleted && isTimelyBonusPaymentCompleted)
                paidTasks.Add(
                    task with
                    {
                        PaymentStatus = EPaymentStatus.Paid
                    }
                );
        }

        await transactionWrapper.ExecuteInTransactionAsync(async () =>
            {
                await paymentBillsRepository.AddPaymentsRangeAsync(paymentsList);

                await subTasksRepository.EditRangeAsync(paidTasks);
            }
        );
    }

    public Task PayBonusAsync(Guid employeeId, float payment, string? comment)
    {
        if (payment <= 0) return Task.CompletedTask;

        var admin = authorizationService.GetUserOrThrowIfNotInRole(ERole.Admin);

        return paymentBillsRepository.AddBonusPaymentAsync(
            new BonusPaymentInfo
            {
                Comment = comment,
                Payment = payment,
                Admin = admin,
                Employee = User.Empty(employeeId),
                CreatedAt = DateTime.UtcNow
            }
        );
    }

    public async Task DeleteBillAsync(Guid id)
    {
        var currentUser = authorizationService.GetUserOrThrowIfUnauthorized();

        if (!await paymentBillsRepository.IsPaymentPaidByThisUser(id, currentUser.Id))
            throw new ForbiddenException($"Payment isn't paid by user {currentUser.Id}");

        await transactionWrapper.ExecuteInTransactionAsync(async () =>
            {
                var payment = await paymentBillsRepository.GetPaymentAsync(id);

                if (payment is null) return;

                var task = await subTasksRepository.GetAsync(payment.TaskId);

                if (task is null) return;

                if (task.PaymentStatus is EPaymentStatus.Paid)
                {
                    await subTasksRepository.EditAsync(
                        task with
                        {
                            PaymentStatus = EPaymentStatus.OnPayment
                        }
                    );
                }

                await paymentBillsRepository.DeletePaymentAsync(id);
            }
        );
    }

    public async Task DeleteBonusAsync(Guid id)
    {
        var currentUser = authorizationService.GetUserOrThrowIfUnauthorized();

        if (!await paymentBillsRepository.IsBonusPaymentPaidByThisUser(id, currentUser.Id))
            throw new ForbiddenException($"Bonus isn't paid by user {currentUser.Id}");

        await paymentBillsRepository.DeleteBonusPaymentAsync(id);
    }
}