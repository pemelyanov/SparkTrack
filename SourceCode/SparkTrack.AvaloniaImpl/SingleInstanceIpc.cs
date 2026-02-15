namespace SparkTrack.AvaloniaImpl;

using System.IO.Pipes;

public static class SingleInstanceIpc
{
    private const  string                  PipeName                  = "SparkTrack";
    private static CancellationTokenSource s_cancellationTokenSource = new();
    private static Task?                   s_task;

    public static void StartListening(Action onSignal)
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
                        onSignal();
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }, s_cancellationTokenSource.Token
        );
    }

    public static void SignalFirstInstance()
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(200);
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