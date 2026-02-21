using Message = Telegram.Bot.Types.Message;
using ReplyMarkup = Telegram.Bot.Types.ReplyMarkups.ReplyMarkup;
using TelegramBotClient = Telegram.Bot.TelegramBotClient;
using Update = Telegram.Bot.Types.Update;
using UpdateType = Telegram.Bot.Types.Enums.UpdateType;
using NLog;
using BotCommand = Telegram.Bot.Types.BotCommand;
using BotCommandScopeChat = Telegram.Bot.Types.BotCommandScopeChat;
using ChatType = Telegram.Bot.Types.Enums.ChatType;
using HandleErrorSource = Telegram.Bot.Polling.HandleErrorSource;
using InlineKeyboardButton = Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton;
using InlineKeyboardMarkup = Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup;
using ITelegramBotClient = Telegram.Bot.ITelegramBotClient;
using ParseMode = Telegram.Bot.Types.Enums.ParseMode;
using ReceiverOptions = Telegram.Bot.Polling.ReceiverOptions;
using TelegramBotClientExtensions = Telegram.Bot.TelegramBotClientExtensions;

namespace SparkTrack.Telegram.Core.Services;

using Data;
using Repositories;
using SparkTrack.Core.Services.Users;

public class TelegramBotService(
    string botToken,
    Func<IUsersService> usersServiceFactory,
    Func<ITelegramUsersRepository> telegramUsersRepositoryFactory
) : ITelegramBotService, ITelegramMessageSender
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    private const string ActionsCommand = "actions";

    private const string HelloText =
        "Привет! Это бот уведомлений трекера SparkTrack. Для получения доступных действий используй команду /"
        + ActionsCommand;

    private const string EnableNotificationsCommand  = "enable_notif";
    private const string DisableNotificationsCommand = "disable_notif";
    private const string ShowTimeZonesCommand        = "show_timezones";
    private const string SetTimeZonesCommand         = "set_timezones";

    private static readonly InlineKeyboardMarkup s_timezoneButtons;

    private TelegramBotClient? m_botClient;

    private readonly Dictionary<string, Func<TelegramBotClient, string, string?, Message, CancellationToken, Task>>
        m_commandHandlers = [];

    static TelegramBotService()
    {
        (TimeSpan Offset, string DisplayName)[] timeZones =
        {
            (TimeSpan.FromHours(2), "Калининград"),
            (TimeSpan.FromHours(3), "Москва, Санкт-Петербург, Минск, Астрахань"),
            (TimeSpan.FromHours(4), "Самара, Ижевск"),
            (TimeSpan.FromHours(5), "Екатеринбург, Челябинск, Тюмень"),
            (TimeSpan.FromHours(6), "Омск"),
            (TimeSpan.FromHours(7), "Новосибирск, Барнаул, Красноярск, Кемерово, Абакан"),
            (TimeSpan.FromHours(8), "Иркутск, Улан-Удэ, Чита"),
            (TimeSpan.FromHours(9), "Якутск, Благовещенск, Нерюнгри"),
            (TimeSpan.FromHours(10), "Владивосток, Хабаровск, Южно-Сахалинск"),
            (TimeSpan.FromHours(11), "Магадан"),
            (TimeSpan.FromHours(12), "Анадырь, Петропавловск-Камчатский"),

            (TimeSpan.FromHours(0), "Лондон, Дублин, Лиссабон (зимой)"),
            (TimeSpan.FromHours(-3), "Бразилиа, Буэнос-Айрес, Монтевидео"),
            (TimeSpan.FromHours(-6), "Чикаго, Мехико, Гватемала"),
            (TimeSpan.FromHours(-9), "Анкоридж, Джуно, Фэрбанкс"),
            (TimeSpan.FromHours(-12), "остров Бейкер, остров Хауленд")
        };

        s_timezoneButtons = timeZones
            .Select(offset =>
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"UTC{FormatOffset(offset.Offset)} {offset.DisplayName}",
                        $"{SetTimeZonesCommand}:{offset.Offset}"
                    )
                }
            )
            .ToArray();
    }

    public void Start(CancellationToken cancellationToken)
    {
        if (m_botClient is not null)
        {
            s_logger.Warn("Attemp to start already started bot");
            return;
        }

        s_logger.Info("Initializing bot...");

        m_commandHandlers[EnableNotificationsCommand] = EnableNotificationsAsync;
        m_commandHandlers[DisableNotificationsCommand] = DisableNotificationsAsync;
        m_commandHandlers[ShowTimeZonesCommand] = ShowTimeZonesAsync;
        m_commandHandlers[SetTimeZonesCommand] = SetTimeZoneAsync;

        m_botClient = new TelegramBotClient(botToken, cancellationToken: cancellationToken);

        s_logger.Info("TelegramBot initialized");

        TelegramBotClientExtensions.StartReceiving(
            m_botClient,
            UpdateHandler,
            ErrorHandler,
            new ReceiverOptions
            {
                DropPendingUpdates = true,
                AllowedUpdates = [UpdateType.CallbackQuery, UpdateType.Message]
            },
            cancellationToken
        );
    }

    private Task ErrorHandler(
        ITelegramBotClient bot,
        Exception exception,
        HandleErrorSource errorSource,
        CancellationToken cancellationToken
    )
    {
        s_logger.Debug("Error occured ({source}): {exception}", errorSource, exception.Message);

        return Task.CompletedTask;
    }

    private async Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message is { } message)
        {
            await OnMessageAsync(message, update.Type, cancellationToken);
            return;
        }

        if (update.Type == UpdateType.CallbackQuery)
        {
            await OnUpdateAsync(update, cancellationToken);
            return;
        }

        s_logger.Warn("Unsupported update: {update}", update.Type);
    }

    public Task SendAsync(
        long chatId,
        string message,
        ParseMode parseMode = ParseMode.None,
        ReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default
    ) => TelegramBotClientExtensions.SendMessage(
        EnsureBot(),
        chatId,
        message,
        parseMode,
        replyMarkup: replyMarkup,
        cancellationToken: cancellationToken
    );

    private async Task OnMessageAsync(Message message, UpdateType type, CancellationToken cancellationToken)
    {
        s_logger.Debug(
            "[{type}] Message received (user: {user}; chat: {chat} ({chatType})): {message}",
            type,
            message.Chat.Username,
            message.Chat.Id,
            message.Chat.Type,
            message.Text
        );

        if (message.Chat.Type is not ChatType.Private)
        {
            s_logger.Warn("Only {type} supported", ChatType.Private);
            return;
        }

        if (message.Chat.Username is not { } username)
        {
            s_logger.Warn("Cannot get username of user");
            return;
        }

        var telegramUsersRepository = telegramUsersRepositoryFactory();

        var telegramUser = await telegramUsersRepository.GetByChatIdAsync(message.Chat.Id);

        if (telegramUser is null)
        {
            s_logger.Warn(
                "User not found in telegram database, searching users with specified tag ({tag}) in main database...",
                username
            );

            var usersService = usersServiceFactory();

            var existingUser = await usersService.GetByTelegramTagAsync(username);

            if (existingUser is null)
            {
                s_logger.Warn("No users with tag {tag} found, ignoring", username);

                return;
            }

            s_logger.Info(
                "User founded: {user} ({role}). Saving to telegram database...",
                existingUser.Name,
                existingUser.Role
            );

            telegramUser = new TelegramUser
            {
                ChatId = message.Chat.Id,
                UserId = existingUser.Id,
            };

            await telegramUsersRepository.AddAsync(telegramUser);

            s_logger.Info("User added to telegram database");
        }

        if (message.Text?.StartsWith($"/{ActionsCommand}") is true)
        {
            await TelegramBotClientExtensions.SendMessage(
                EnsureBot(),
                message.Chat.Id,
                "Выберите действие",
                replyMarkup: ResolveActions(telegramUser),
                cancellationToken: cancellationToken
            );

            return;
        }

        await TelegramBotClientExtensions.SetMyCommands(
            EnsureBot(),
            [
                new BotCommand(ActionsCommand, "Получить список доступных действий")
            ],
            new BotCommandScopeChat
            {
                ChatId = message.Chat.Id
            },
            cancellationToken: cancellationToken
        );

        if (!telegramUser.IsHelloSent)
        {
            s_logger.Info("Hello message not sent, sending...");
            await TelegramBotClientExtensions.SendMessage(
                EnsureBot(),
                message.Chat.Id,
                HelloText,
                cancellationToken: cancellationToken
            );
            
            await telegramUsersRepository.EditAsync(
                telegramUser with
                {
                    IsHelloSent = true
                }
            );
        }
    }

    private async Task OnUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        s_logger.Debug("Update received ({type})", update.Type);

        if (update.Type is not UpdateType.CallbackQuery || update.CallbackQuery?.Data is not { } query
            || update.CallbackQuery.Message is not { } message) return;

        var firstSeparatorIndex = query.IndexOf(':');

        var command = firstSeparatorIndex == -1 ? query : query[..firstSeparatorIndex];
        var args = firstSeparatorIndex == -1 ? null : query[(firstSeparatorIndex + 1)..];

        if (!m_commandHandlers.TryGetValue(command, out var handler)) return;

        s_logger.Info(
            "Handling qery - {query}; User - {user}; ChatId: {chatId}",
            query,
            message.Chat.Username,
            message.Chat.Id
        );

        await handler(EnsureBot(), command, args, message, cancellationToken);

        await TelegramBotClientExtensions.DeleteMessage(EnsureBot(), message.Chat.Id, message.Id, cancellationToken);
    }

    private TelegramBotClient EnsureBot()
    {
        if (m_botClient is null)
            throw new InvalidOperationException($"Start {nameof(ITelegramBotService)} before using bot");

        return m_botClient;
    }

    private InlineKeyboardMarkup ResolveActions(TelegramUser user)
    {
        if (!user.IsNotificationsEnabled)
        {
            return new InlineKeyboardMarkup(
                InlineKeyboardButton.WithCallbackData(
                    "🔔 Подключить уведомления",
                    EnableNotificationsCommand
                )
            );
        }

        return new InlineKeyboardMarkup(
            [
                [
                    InlineKeyboardButton.WithCallbackData(
                        "🔕 Отключить уведомления",
                        DisableNotificationsCommand
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        "🌍 Указать часовой пояс",
                        ShowTimeZonesCommand
                    ),
                ],
            ]
        );
    }

    private async Task DisableNotificationsAsync(
        TelegramBotClient bot,
        string command,
        string? args,
        Message message,
        CancellationToken cancellationToken
    )
    {
        var telegramUsersRepository = telegramUsersRepositoryFactory();
        var telegramUser = await telegramUsersRepository.GetByChatIdAsync(message.Chat.Id);

        if (telegramUser is null)
        {
            s_logger.Warn("Cannot disable notifications. User with chat id {id} not found", message.Chat.Id);
            return;
        }

        await telegramUsersRepository.EditAsync(
            telegramUser with
            {
                IsNotificationsEnabled = false
            }
        );

        s_logger.Info("Disabling notifications for user {user}", message.Chat.Username);
        await TelegramBotClientExtensions.SendMessage(
            bot,
            message.Chat.Id,
            "Уведомления отключены",
            cancellationToken: cancellationToken
        );
    }

    private async Task EnableNotificationsAsync(
        TelegramBotClient bot,
        string command,
        string? args,
        Message message,
        CancellationToken cancellationToken
    )
    {
        var telegramUsersRepository = telegramUsersRepositoryFactory();
        var telegramUser = await telegramUsersRepository.GetByChatIdAsync(message.Chat.Id);

        if (telegramUser is null)
        {
            s_logger.Warn("Cannot enable notifications. User with chat id {id} not found", message.Chat.Id);
            return;
        }

        await telegramUsersRepository.EditAsync(
            telegramUser with
            {
                IsNotificationsEnabled = true
            }
        );

        s_logger.Info("Enabling notifications for user {user}", message.Chat.Username);
        await TelegramBotClientExtensions.SendMessage(
            bot,
            message.Chat.Id,
            "Уведомления подключены",
            cancellationToken: cancellationToken
        );
    }

    private async Task ShowTimeZonesAsync(
        TelegramBotClient bot,
        string command,
        string? args,
        Message message,
        CancellationToken cancellationToken
    )
    {
        await TelegramBotClientExtensions.SendMessage(
            bot,
            message.Chat.Id,
            "Выберите часовой пояс",
            replyMarkup: s_timezoneButtons,
            cancellationToken: cancellationToken
        );
    }

    private async Task SetTimeZoneAsync(
        TelegramBotClient bot,
        string command,
        string? args,
        Message message,
        CancellationToken cancellationToken
    )
    {
        var telegramUsersRepository = telegramUsersRepositoryFactory();
        var telegramUser = await telegramUsersRepository.GetByChatIdAsync(message.Chat.Id);

        if (telegramUser is null)
        {
            s_logger.Warn("Cannot set timezone. User with chat id {id} not found", message.Chat.Id);
            return;
        }
        
        if (!TimeSpan.TryParse(args, out var timeZone))
        {
            s_logger.Warn("Unable to parse timezone: {timezone}", args);
            return;
        }

        await telegramUsersRepository.EditAsync(
            telegramUser with
            {
                TimeZone = timeZone
            }
        );

        s_logger.Info("Updating timezone to {timezone} for user {user}", timeZone, message.Chat.Username);
        await TelegramBotClientExtensions.SendMessage(
            bot,
            message.Chat.Id,
            "Часовой пояс обновлен",
            cancellationToken: cancellationToken
        );
    }

    private static string FormatOffset(TimeSpan offset)
    {
        var sign = offset >= TimeSpan.Zero ? "+" : "-";
        return $"{sign}{offset:hh\\:mm}";
    }
}