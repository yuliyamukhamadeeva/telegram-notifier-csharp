using Telegram.Bot;
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

bot.OnMessage += async (msg, type) =>
{
    if (msg.Text is null) return;

    var chatId = msg.Chat.Id;
    var text = msg.Text.Trim();
    var cmd = ExtractCommand(text);

    if (cmd is "/start" or "/help")
    {
        await bot.SendMessage(chatId, helpText);
        return;
    }

    if (cmd == "/format")
    {
        var html =
            "<b>Подтверждение брони</b>\n" +
            "Маршрут: <i>Москва → Сочи</i>\n" +
            "Статус: <u>Ожидаем оплату</u>\n" +
            "Сумма: <code>52 400 ₽</code>";

        await bot.SendMessage(chatId, html, parseMode: ParseMode.Html);
        return;
    }

    if (cmd == "/photo")
    {
        await using var stream = File.OpenRead(AssetPath("photo.jpg"));
        await bot.SendPhoto(chatId, InputFile.FromStream(stream, "photo.jpg"), caption: "Фото: вариант для поездки");
        return;
    }

    if (cmd == "/audio")
    {
        await using var stream = File.OpenRead(AssetPath("audio.mp3"));
        await bot.SendAudio(chatId, InputFile.FromStream(stream, "audio.mp3"), caption: "Аудио: промо/объявление");
        return;
    }

    if (cmd == "/voice")
    {
        await using var stream = File.OpenRead(AssetPath("voice.ogg"));
        await bot.SendVoice(chatId, InputFile.FromStream(stream, "voice.ogg"), caption: "Голосовое: короткое уведомление");
        return;
    }

    if (cmd == "/video")
    {
        await using var stream = File.OpenRead(AssetPath("video.mp4"));
        await bot.SendVideo(chatId, InputFile.FromStream(stream, "video.mp4"), caption: "Видео: обзор места");
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

        await bot.SendMediaGroup(chatId, media);
        return;
    }

    if (cmd == "/location")
    {
        await bot.SendLocation(chatId, latitude: 55.751244f, longitude: 37.618423f);
        return;
    }

    if (cmd == "/contact")
    {
        await bot.SendContact(chatId, phoneNumber: "+79990000000", firstName: "Travel", lastName: "Notifier");
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

        await bot.SendPoll(
            chatId: chatId,
            question: "Что добавить в поездку?",
            options: options,
            isAnonymous: true
        );

        return;
    }

    if (text.StartsWith("/"))
    {
        await bot.SendMessage(chatId, "Не знаю такую команду. Напиши /help, чтобы увидеть список.");
        return;
    }

    await bot.SendMessage(chatId, $"Эхо: {text}");
};

bot.OnError += (exception, source) =>
{
    Console.WriteLine(exception);
    return Task.CompletedTask;
};


Console.WriteLine("Бот слушает сообщения. Остановить: Ctrl+C");
await Task.Delay(-1);
