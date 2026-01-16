namespace SparkTrack.Core.Exceptions;

public class ConflictException(string? message = null, Exception? innerException = null)
    : Exception(message, innerException);