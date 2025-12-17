namespace SparkTrack.DataAccess.EFCore.Data.Entities;

using Core.Shared.Enums;

public record UserData
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required ERole Role { get; init; }
    
    public required string Email { get; init; }
    
    public required string PasswordHash { get; init; }
}