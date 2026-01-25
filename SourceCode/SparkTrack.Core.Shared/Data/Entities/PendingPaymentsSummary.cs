namespace SparkTrack.Core.Shared.Data.Entities;

public record PendingPaymentsSummary
{
    public IReadOnlyList<UserPayment> RemainingPayments { get; init; } = [];

    public IReadOnlyList<UserPayment> AdminPayments { get; init; } = [];
}