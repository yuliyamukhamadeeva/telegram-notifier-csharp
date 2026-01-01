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

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(token, cancellationToken: cts.Token);

var me = await bot.GetMe();
Console.WriteLine($"@{me.Username} запущен. Нажми Enter для остановки");

bot.OnError += (exception, source) =>
{
    Console.WriteLine(exception);
    return Task.CompletedTask;
};

bot.OnMessage += async (msg, type) =>
{
    if (msg.Text is null) return;

    var chatId = msg.Chat.Id;
    var text = msg.Text.Trim();
    var command = text.Split(' ', 2)[0];

    switch (command)
    {
        case "/start":
            await bot.SendMessage(chatId, "Команды: /format /photo /audio /voice /video /group /location /contact /poll");
            break;

        case "/format":
            await bot.SendMessage(chatId, "Ок, дальше сделаем форматирование");
            break;

        case "/photo":
            await bot.SendMessage(chatId, "Ок, дальше отправим фото");
            break;

        case "/audio":
            await bot.SendMessage(chatId, "Ок, дальше отправим аудио");
            break;

        case "/voice":
            await bot.SendMessage(chatId, "Ок, дальше отправим голосовое");
            break;

        case "/video":
            await bot.SendMessage(chatId, "Ок, дальше отправим видео");
            break;

        case "/group":
            await bot.SendMessage(chatId, "Ок, дальше отправим медиа группу");
            break;

        case "/location":
            await bot.SendMessage(chatId, "Ок, дальше отправим локацию");
            break;

        case "/contact":
            await bot.SendMessage(chatId, "Ок, дальше отправим контакт");
            break;

        case "/poll":
            await bot.SendMessage(chatId, "Ок, дальше отправим опрос");
            break;

        default:
            await bot.SendMessage(chatId, $"Эхо: {text}");
            break;
    }
};


Console.ReadLine();
cts.Cancel();
