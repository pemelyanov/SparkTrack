namespace SparkTrack.API;

using Interceptors;

public class SparkHttpClient : HttpClient
{
    public SparkHttpClient(string baseUrl, RetryAuthHandler retryAuthHandler) : base(retryAuthHandler)
    {
        BaseAddress = new Uri(baseUrl);
    }
}