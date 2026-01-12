namespace SparkTrack.Core.Exceptions;

public class UnauthorizedException(string? message = null) : Exception(message);