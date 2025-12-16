namespace SparkTrack.API;

public class SparkHttpClient : HttpClient
{
    public SparkHttpClient(string baseUrl)
    {
        BaseAddress = new Uri(baseUrl);
    }
}