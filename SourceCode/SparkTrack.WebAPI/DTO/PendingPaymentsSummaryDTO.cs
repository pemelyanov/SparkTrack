namespace SparkTrack.WebAPI.DTO;

public class PendingPaymentsSummaryDTO
{
    public IReadOnlyList<UserPaymentDTO> RemainingPayments { get; init; } = [];

    public IReadOnlyList<UserPaymentDTO> AdminPayments { get; init; } = [];
}