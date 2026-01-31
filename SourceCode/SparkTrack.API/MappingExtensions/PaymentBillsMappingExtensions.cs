namespace SparkTrack.API.MappingExtensions;

using Core.Shared.Enums;
using SparkTrack.Core.Shared.Data.Entities;

public static class PaymentBillsMappingExtensions
{
    public static PaymentBill ToDomain(this PaymentBillDTO data) => new()
    {
        Feature = data.Feature.ToDomain(),
        SubTask = data.SubTask.ToDomain(),
        PaymentsList = data.PaymentsList.Select(it => it.ToDomain()).ToArray()
    };

    public static UserPayment ToDomain(this UserPaymentDTO data) => new()
    {
        User = data.User.ToDomain(),
        Payment = data.RemainingPayment
    };
    
    public static PaymentInfo ToDomain(this PaymentDTO data) => new()
    {
        Id = data.Id,
        Admin = data.Admin.ToDomain(),
        Payment = data.Payment,
        PaymentType = data.PaymentType.Cast<EPaymentType>(),
        TaskId = data.TaskId,
        CreatedAt = data.CreatedAt
    };
    
    public static PendingPaymentsSummary ToDomain(this PendingPaymentsSummaryDTO data) => new()
    {
        AdminPayments = data.AdminPayments.Select(it => it.ToDomain()).ToArray(),
        RemainingPayments = data.RemainingPayments.Select(it => it.ToDomain()).ToArray()
    };
    
    public static PaymentDetails ToDomain(this PaymentDetailsDTO data) => new()
    {
        Id = data.Id,
        Admin = data.Admin.ToDomain(),
        Payment = data.Payment,
        PaymentType = data.PaymentType.Cast<EPaymentType>(),
        TaskId = data.TaskId,
        CreatedAt = data.CreatedAt,
        Task = data.Task.ToDomain(),
        Feature = data.Feature.ToDomain()
    };
    
    public static BonusPaymentInfo ToDomain(this BonusPaymentDTO data) => new()
    {
        Id = data.Id,
        Admin = data.Admin.ToDomain(),
        Payment = data.Payment,
        CreatedAt = data.CreatedAt,
        Employee = data.Employee.ToDomain(),
        Comment = data.Comment
    };
}