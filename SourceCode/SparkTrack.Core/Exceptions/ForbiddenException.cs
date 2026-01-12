namespace SparkTrack.Core.Exceptions;

public class ForbiddenException(string? message = null) : Exception(message);