using NLog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = NLog.ILogger;

namespace SparkTrack.WebAPI.BackgroundServices;

public class TelegramBotService(IConfiguration configuration) : BackgroundService
{
    private static readonly ILogger           s_logger    = LogManager.GetCurrentClassLogger();
    private                 TelegramBotClient m_botClient = null!;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var botToken = configuration.GetRequiredSection("TelegramBot").GetSection("Token").Get<string>()
                       ?? throw new InvalidOperationException("Specify bot token in appsettings.json");

        s_logger.Info("Initializing bot...");

        m_botClient = new TelegramBotClient(botToken, cancellationToken: stoppingToken);

        m_botClient.OnMessage += OnMessage;
        m_botClient.OnUpdate += OnUpdate;
        m_botClient.OnError += OnError;

        s_logger.Info("TelegramBot initialized");

        return Task.CompletedTask;
    }

    private Task OnMessage(Message message, UpdateType type)
    {
        s_logger.Info("[{type}] Message received (user: {user}; chat: {chat}): {message}", type, message.From?.Username,
            message.Chat.Id, message.Text);

        return Task.CompletedTask;
    }

    private Task OnUpdate(Update update)
    {
        s_logger.Info("Update received");

        return Task.CompletedTask;
    }

    private Task OnError(Exception exception, HandleErrorSource source)
    {
        s_logger.Warn("Error occured ({source}): {exception}", source, exception.Message);

        return Task.CompletedTask;
    }
}