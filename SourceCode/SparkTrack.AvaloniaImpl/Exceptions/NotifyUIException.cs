namespace SparkTrack.AvaloniaImpl.Exceptions;

public class NotifyUIException(string message, Exception? source = null) : Exception(message, source);