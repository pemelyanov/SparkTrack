namespace SparkTrack.Core.Services.PaymentBills;

using Authorization;
using Extensions;
using Repositories;
using Shared.Data;
using Shared.Data.Entities;
using Shared.Enums;
using Shared.Services.PaymentBills;

public class PaymentBillsService(
    IPaymentBillsRepository paymentBillsRepository,
    ISubTasksRepository subTasksRepository,
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

        foreach (var task in tasks)
        {
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
            }
        }

        await paymentBillsRepository.AddPaymentsRangeAsync(paymentsList);
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