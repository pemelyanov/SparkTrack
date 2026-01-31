namespace SparkTrack.WebAPI.MappingExtensions;

using Core.Shared.Data.Entities;
using DTO;

public static class PaymentBillsMappingExtensions
{
    public static PaymentBillDTO ToDTO(this PaymentBill data) => new()
    {
        Feature = data.Feature.ToDTO(),
        SubTask = data.SubTask.ToDTO(),
        PaymentsList = data.PaymentsList.Select(it => it.ToDTO()).ToArray()
    };

    public static UserPaymentDTO ToDTO(this UserPayment data) => new()
    {
        User = data.User.ToDTO(),
        RemainingPayment = data.Payment
    };

    public static PaymentDTO ToDTO(this PaymentInfo data) => new()
    {
        Id = data.Id,
        Admin = data.Admin.ToDTO(),
        Payment = data.Payment,
        PaymentType = data.PaymentType,
        TaskId = data.TaskId,
        CreatedAt = data.CreatedAt
    };
    
    public static PaymentDetailsDTO ToDTO(this PaymentDetails data) => new()
    {
        Id = data.Id,
        Admin = data.Admin.ToDTO(),
        Payment = data.Payment,
        PaymentType = data.PaymentType,
        TaskId = data.TaskId,
        CreatedAt = data.CreatedAt,
        Task = data.Task.ToDTO(),
        Project = data.Project.ToDTO()
    };
    
    public static BonusPaymentDTO ToDTO(this BonusPaymentInfo data) => new()
    {
        Id = data.Id,
        Admin = data.Admin.ToDTO(),
        Payment = data.Payment,
        CreatedAt = data.CreatedAt,
        Employee = data.Employee.ToDTO(),
        Comment = data.Comment
    };
    
    public static PendingPaymentsSummaryDTO ToDTO(this PendingPaymentsSummary data) => new()
    {
        AdminPayments = data.AdminPayments.Select(it => it.ToDTO()).ToArray(),
        RemainingPayments = data.RemainingPayments.Select(it => it.ToDTO()).ToArray()
    };
    

}