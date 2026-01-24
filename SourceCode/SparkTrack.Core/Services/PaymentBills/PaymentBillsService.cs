namespace SparkTrack.Core.Services.PaymentBills;

using Authorization;
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

    public Task<IReadOnlyList<UserRemainingPayment>> GetUsersRemainingPaymentsAsync(Guid? projectId) =>
        paymentBillsRepository.GetUsersRemainingPaymentsAsync(projectId);

    public async Task PayBillsAsync(IReadOnlyList<Guid> tasksIdList, float payment, float timelyBonusPayment)
    {
        var admin = authorizationService.GetUserOrThrowIfNotInRole(ERole.Admin);
        
        var tasks = await subTasksRepository.GetListAsync(tasksIdList);

        var totalMainPayments = tasks.Sum(
            it => it.RemainingMainPayment
        );

        var totalTimelyBonusPayments = tasks.Where(it => it is { IsTimelyBonusApproved: true, TimelyBonus: > 0 })
            .Sum(
                it => it.RemainingTimelyBonusPayment
            );

        var mainPaymentsRatio = payment / totalMainPayments;
        var timelyBonusPaymentRatio = timelyBonusPayment / totalTimelyBonusPayments;

        var paymentsList = new List<PaymentInfo>();
        var paidTasks = new List<SubTask>();

        foreach (var task in tasks)
        {
            bool isMainPaymentCompleted = false;
            bool isTimelyBonusPaymentCompleted = false;
            
            var currentMainPayment = task.RemainingMainPayment * mainPaymentsRatio;

            if (currentMainPayment > 0)
            {
                paymentsList.Add(new PaymentInfo
                {
                    Admin = admin,
                    Payment = currentMainPayment,
                    PaymentType = EPaymentType.Main,
                    TaskId = task.Id
                });

                isMainPaymentCompleted = task.RemainingMainPayment - currentMainPayment <= 0;
            }

            var currentTimelyBonusPayment = task.RemainingTimelyBonusPayment * timelyBonusPaymentRatio;

            if (currentTimelyBonusPayment > 0)
            {
                paymentsList.Add(new PaymentInfo
                {
                    Admin = admin,
                    Payment = currentTimelyBonusPayment,
                    PaymentType = EPaymentType.TimelyBonus,
                    TaskId = task.Id
                }); 
                
                isTimelyBonusPaymentCompleted = task.RemainingTimelyBonusPayment - currentTimelyBonusPayment <= 0;
            }

            if (isMainPaymentCompleted && isTimelyBonusPaymentCompleted)
                paidTasks.Add(task with
                {
                    PaymentStatus = EPaymentStatus.Paid
                });
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
        if(payment <= 0) return Task.CompletedTask;
        
        var admin = authorizationService.GetUserOrThrowIfNotInRole(ERole.Admin);

        return paymentBillsRepository.AddBonusPaymentAsync(
            new BonusPaymentInfo
            {
                Comment = comment,
                Payment = payment,
                Admin = admin,
                EmployeeId = employeeId
            }
        );
    }

    public Task DeleteBillAsync(Guid id) => paymentBillsRepository.DeletePaymentAsync(id);

    public Task DeleteBonusAsync(Guid id) => paymentBillsRepository.DeleteBonusPaymentAsync(id);
}