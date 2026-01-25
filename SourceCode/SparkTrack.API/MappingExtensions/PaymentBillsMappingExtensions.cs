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

    public static UserPayment ToDomain(this UserRemainingPaymentDTO data) => new()
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
}