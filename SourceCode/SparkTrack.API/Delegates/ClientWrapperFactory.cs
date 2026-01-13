namespace SparkTrack.API.Delegates;

public delegate ClientWrapper<TClient> ClientFactory<TClient>() where TClient : ClientBase;