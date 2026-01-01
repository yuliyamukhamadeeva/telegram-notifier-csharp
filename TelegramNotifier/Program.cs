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

bot.OnMessage += async (msg, type) =>
{
    if (msg.Text is null) return;

    var chatId = msg.Chat.Id;
    var text = msg.Text.Trim();

    if (text == "/start")
    {
        await bot.SendMessage(chatId,
            "Команды:\n/format\n/photo\n/audio\n/voice\n/video\n/group\n/location\n/contact\n/poll");
        return;
    }

    if (text == "/format")
    {
        var html = "<b>Подтверждение брони</b>\n" +
                   "Маршрут: <i>Москва → Сочи</i>\n" +
                   "Статус: <u>Ожидаем оплату</u>\n" +
                   "Сумма: <code>52 400 ₽</code>";
        await bot.SendMessage(chatId, html, parseMode: ParseMode.Html);
        return;
    }

    if (text == "/photo")
    {
        await using var stream = System.IO.File.OpenRead(AssetPath("photo.jpg"));
        await bot.SendPhoto(chatId, InputFile.FromStream(stream, "photo.jpg"), caption: "Фото для теста");
        return;
    }

    if (text == "/audio")
    {
        await using var stream = System.IO.File.OpenRead(AssetPath("audio.mp3"));
        await bot.SendAudio(chatId, InputFile.FromStream(stream, "audio.mp3"), caption: "Аудио для теста");
        return;
    }

    if (text == "/voice")
    {
        await using var stream = System.IO.File.OpenRead(AssetPath("voice.ogg"));
        await bot.SendVoice(chatId, InputFile.FromStream(stream, "voice.ogg"));
        return;
    }

    if (text == "/video")
    {
        await using var stream = System.IO.File.OpenRead(AssetPath("video.mp4"));
        await bot.SendVideo(chatId, InputFile.FromStream(stream, "video.mp4"), caption: "Видео для теста");
        return;
    }

    if (text == "/group")
    {
        await using var s1 = System.IO.File.OpenRead(AssetPath("group1.jpg"));
        await using var s2 = System.IO.File.OpenRead(AssetPath("group2.jpg"));
        await using var s3 = System.IO.File.OpenRead(AssetPath("group3.jpg"));

        var media = new IAlbumInputMedia[]
        {
            new InputMediaPhoto(InputFile.FromStream(s1, "group1.jpg")) { Caption = "Фото 1" },
            new InputMediaPhoto(InputFile.FromStream(s2, "group2.jpg")) { Caption = "Фото 2" },
            new InputMediaPhoto(InputFile.FromStream(s3, "group3.jpg")) { Caption = "Фото 3" }
        };

        await bot.SendMediaGroup(chatId, media);
        return;
    }

    if (text == "/location")
    {
        await bot.SendLocation(chatId, latitude: 55.751244f, longitude: 37.618423f);
        return;
    }

    if (text == "/contact")
    {
        await bot.SendContact(chatId, phoneNumber: "+79990000000", firstName: "Travel", lastName: "Notifier");
        return;
    }

    if (text == "/poll")
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

    await bot.SendMessage(chatId, $"Эхо: {text}");
};

bot.OnError += (exception, source) =>
{
    Console.WriteLine(exception);
    return Task.CompletedTask;
};


Console.WriteLine("Бот слушает сообщения. Остановить: Ctrl+C");
await Task.Delay(-1);
