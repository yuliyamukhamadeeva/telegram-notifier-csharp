using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
if (string.IsNullOrWhiteSpace(token))
{
    Console.WriteLine("Нет TELEGRAM_BOT_TOKEN в переменных окружения");
    return;
}

var bot = new TelegramBotClient(token);
var me = await bot.GetMe();
var webhook = await bot.GetWebhookInfo();
Console.WriteLine($"Webhook url: {webhook.Url}");
Console.WriteLine($"Pending update count: {webhook.PendingUpdateCount}");

if (webhook.PendingUpdateCount > 0)
{
    await bot.DeleteWebhook(dropPendingUpdates: true);
    Console.WriteLine("Сбросил pending updates (dropPendingUpdates=true)");
}


Console.WriteLine($"Запущен бот: @{me.Username}");

var assetsDir = Path.Combine(AppContext.BaseDirectory, "assets");
string AssetPath(string fileName) => Path.Combine(assetsDir, fileName);

var helpText =
"""
Команды:
/start — справка
/help — справка
/format — форматированное сообщение
/photo — фотография
/audio — аудио
/voice — голосовое сообщение
/video — видео
/group — медиа-группа
/location — местоположение
/contact — контакт
/poll — опрос
""";

static string? ExtractCommand(string? text)
{
    if (string.IsNullOrWhiteSpace(text)) return null;
    var first = text.Trim().Split(' ', '\n', '\t')[0];
    var cmd = first.Split('@')[0];
    return cmd;
}

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
{
    if (update.Type != UpdateType.Message) return;
    if (update.Message?.Text is null) return;

    var chatId = update.Message.Chat.Id;
    var text = update.Message.Text.Trim();
    var cmd = ExtractCommand(text);

    if (cmd is "/start" or "/help")
    {
        await botClient.SendMessage(chatId, helpText, cancellationToken: ct);
        return;
    }

    if (cmd == "/format")
    {
        var html =
            "<b>Подтверждение брони</b>\n" +
            "Маршрут: <i>Москва → Сочи</i>\n" +
            "Статус: <u>Ожидаем оплату</u>\n" +
            "Сумма: <code>52 400 ₽</code>";

        await botClient.SendMessage(chatId, html, parseMode: ParseMode.Html, cancellationToken: ct);
        return;
    }

    if (cmd == "/photo")
    {
        await using var stream = File.OpenRead(AssetPath("photo.jpg"));
        await botClient.SendPhoto(chatId, InputFile.FromStream(stream, "photo.jpg"), caption: "Фото: вариант для поездки", cancellationToken: ct);
        return;
    }

    if (cmd == "/audio")
    {
        await using var stream = File.OpenRead(AssetPath("audio.mp3"));
        await botClient.SendAudio(chatId, InputFile.FromStream(stream, "audio.mp3"), caption: "Аудио: промо/объявление", cancellationToken: ct);
        return;
    }

    if (cmd == "/voice")
    {
        await using var stream = File.OpenRead(AssetPath("voice.ogg"));
        await botClient.SendVoice(chatId, InputFile.FromStream(stream, "voice.ogg"), caption: "Голосовое: короткое уведомление", cancellationToken: ct);
        return;
    }

    if (cmd == "/video")
    {
        await using var stream = File.OpenRead(AssetPath("video.mp4"));
        await botClient.SendVideo(chatId, InputFile.FromStream(stream, "video.mp4"), caption: "Видео: обзор места", cancellationToken: ct);
        return;
    }

    if (cmd == "/group")
    {
        await using var s1 = File.OpenRead(AssetPath("group1.jpg"));
        await using var s2 = File.OpenRead(AssetPath("group2.jpg"));
        await using var s3 = File.OpenRead(AssetPath("group3.jpg"));

        var media = new IAlbumInputMedia[]
        {
            new InputMediaPhoto(InputFile.FromStream(s1, "group1.jpg")) { Caption = "Отель: фасад" },
            new InputMediaPhoto(InputFile.FromStream(s2, "group2.jpg")) { Caption = "Отель: номер" },
            new InputMediaPhoto(InputFile.FromStream(s3, "group3.jpg")) { Caption = "Отель: бассейн" }
        };

        await botClient.SendMediaGroup(chatId, media, cancellationToken: ct);
        return;
    }

    if (cmd == "/location")
    {
        await botClient.SendLocation(chatId, latitude: 55.751244f, longitude: 37.618423f, cancellationToken: ct);
        return;
    }

    if (cmd == "/contact")
    {
        await botClient.SendContact(chatId, phoneNumber: "+79990000000", firstName: "Travel", lastName: "Notifier", cancellationToken: ct);
        return;
    }

    if (cmd == "/poll")
    {
        var options = new[]
        {
            new InputPollOption("Отель"),
            new InputPollOption("Экскурсии"),
            new InputPollOption("Трансфер")
        };

        await botClient.SendPoll(
            chatId: chatId,
            question: "Что добавить в поездку?",
            options: options,
            isAnonymous: true,
            cancellationToken: ct
        );

        return;
    }

    if (text.StartsWith("/"))
    {
        await botClient.SendMessage(chatId, "Не знаю такую команду. Напиши /help, чтобы увидеть список.", cancellationToken: ct);
        return;
    }

    await botClient.SendMessage(chatId, $"Эхо: {text}", cancellationToken: ct);
}

Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
{
    Console.WriteLine(exception);
    return Task.CompletedTask;
}

var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = new[] { UpdateType.Message }
};

bot.StartReceiving(
    updateHandler: new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
    receiverOptions: receiverOptions
);

Console.WriteLine("Бот слушает сообщения. Остановить: Ctrl+C");
await Task.Delay(-1);
