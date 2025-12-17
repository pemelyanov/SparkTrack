namespace SparkTrack.WebAPI.DTO;

using Core.Shared.Enums;

public record AuthorizationDTO
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; set; }
    public required Guid UserId { get; set; }
    public required ERole UserRole { get; set; }
}