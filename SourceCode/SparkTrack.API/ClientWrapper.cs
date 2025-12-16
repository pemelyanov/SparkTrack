namespace SparkTrack.API;

/// <summary>
/// Обертка для использования автоматически сгенерированных классов API.
/// Создавать через фабрику для каждого запроса, после использования обязательно вызывать Dispose.
/// </summary>
public class ClientWrapper<TClient>(TClient client, HttpClient httpClient) : IDisposable
{
    public TClient Client { get; } = client;
    
    public void Dispose()
    {
        httpClient.Dispose();
    }
}