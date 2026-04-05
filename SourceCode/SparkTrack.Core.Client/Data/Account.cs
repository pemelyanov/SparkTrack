namespace SparkTrack.Core.Client.Data;

using Shared.Enums;

public record Account(string Name, string Email, ERole Role, TokensConfiguration Credentials);