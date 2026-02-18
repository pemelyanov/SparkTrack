namespace SparkTrack.API.Delegates;

public delegate ClientWrapper<TClient> ClientFactory<TClient>() where TClient : ClientBase;

public delegate ClientWrapper<TClient> CustomClientFactory<TClient>(HttpClient httpClient) where TClient : ClientBase;