namespace SparkTrack.WebAPI.MappingExtensions;

using Core.Shared.Data.Entities;
using DTO;

public static class PaymentBillsMappingExtensions
{
    public static PaymentBillDTO ToDTO(this PaymentBill data) => new()
    {
        Feature = data.Feature.ToDTO(),
        SubTask = data.SubTask.ToDTO()
    };

    public static UserRemainingPaymentDTO ToDTO(this UserRemainingPayment data) => new()
    {
        User = data.User.ToDTO(),
        RemainingPayment = data.RemainingPayment
    };
}