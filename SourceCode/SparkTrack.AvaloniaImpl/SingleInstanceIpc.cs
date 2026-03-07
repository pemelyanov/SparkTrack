namespace SparkTrack.AvaloniaImpl;

using System.IO.Pipes;

public static class SingleInstanceIpc
{
    private const  string                  PipeName                  = "SparkTrack";
    private static CancellationTokenSource s_cancellationTokenSource = new();
    private static Task?                   s_task;

    public static void StartListening(Action<string> onSignal)
    {
        if (s_task is not null) return;

        s_cancellationTokenSource = new();

        s_task = Task.Run(async () =>
            {
                while (!s_cancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        using var server = new NamedPipeServerStream(
                            PipeName,
                            PipeDirection.In
                        );

                        await server.WaitForConnectionAsync(s_cancellationTokenSource.Token);
                        
                        using var reader = new StreamReader(server);
                        var deeplink = await reader.ReadToEndAsync();
                        
                        onSignal(deeplink);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }, s_cancellationTokenSource.Token
        );
    }

    public static void SignalFirstInstance(string deeplink = "")
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(200);
            
            if (!string.IsNullOrEmpty(deeplink))
            {
                using var writer = new StreamWriter(client);
                writer.Write(deeplink);
                writer.Flush();
            }
        }
        catch
        {
            // Первый инстанс ещё не поднялся — игнорируем
        }
    }

    public static void Stop()
    {
        s_cancellationTokenSource.Cancel();
        s_task = null;
    }
}