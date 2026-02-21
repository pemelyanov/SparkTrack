using NLog;
using ILogger = NLog.ILogger;

namespace SparkTrack.WebAPI.BackgroundServices;

using Telegram.Core.Services;

public class TelegramBotBackgroundService(ITelegramBotService telegramBotService) : BackgroundService
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        s_logger.Info("Starting bot service...");
        
        telegramBotService.Start(stoppingToken);

        s_logger.Info("Bot service started");

        return Task.CompletedTask;
    }
}