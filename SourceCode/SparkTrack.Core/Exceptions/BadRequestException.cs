namespace SparkTrack.Core.Exceptions;

public class BadRequestException(string? message = null) : Exception(message);