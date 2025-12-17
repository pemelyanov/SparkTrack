namespace SparkTrack.WebAPI.DTO.Edit;

public record UserEditDTO
{
    public required string Name { get; init; }
    
    public required string Email { get; init; }
}