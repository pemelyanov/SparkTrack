namespace SparkTrack.API.MappingExtensions;

using SparkTrack.Core.Shared.Data.Entities;

public static class PaymentBillsMappingExtensions
{
    public static PaymentBill ToDomain(this PaymentBillDTO data) => new()
    {
        Feature = data.Feature.ToDomain(),
        SubTask = data.SubTask.ToDomain()
    };

    public static UserRemainingPayment ToDomain(this UserRemainingPaymentDTO data) => new()
    {
        User = data.User.ToDomain(),
        RemainingPayment = data.RemainingPayment
    };
}