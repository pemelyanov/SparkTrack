namespace SparkTrack.API;

/// <summary>
/// Обертка для использования автоматически сгенерированных классов API.
/// Создавать через фабрику для каждого запроса, после использования обязательно вызывать Dispose.
/// </summary>
public class ClientWrapper<TClient> : IDisposable
    where TClient : ClientBase
{
    private readonly HttpClient m_httpClient;
    
    public ClientWrapper(HttpClient httpClient)
    {
        m_httpClient = httpClient;

        var constructor = typeof(TClient).GetConstructor([typeof(HttpClient)])
            ?? throw new InvalidOperationException(
                $"Cannot find constructor with http client parameter for {typeof(TClient)}"
            );

        Client = (TClient)constructor.Invoke([httpClient]);
    }

    public TClient Client { get; }
    
    public void Dispose()
    {
        m_httpClient.Dispose();
    }
}