namespace SparkTrack.WebAPI.DTO;

public record ChangePasswordDTO
{
    public required string OldPassword { get; init; }
    
    public required string NewPassword { get; init; }
}