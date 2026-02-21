namespace SparkTrack.Telegram.Core.Services;

public interface ITelegramBotService
{
    public void Start(CancellationToken cancellationToken);
}