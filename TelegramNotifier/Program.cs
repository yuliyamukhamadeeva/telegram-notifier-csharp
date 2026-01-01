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

    var text = msg.Text.Trim();

    if (text == "/start")
    {
        await bot.SendMessage(msg.Chat, "Команды: /format /photo /audio /voice /video /group /location /contact /poll");
        return;
    }

    await bot.SendMessage(msg.Chat, $"Эхо: {msg.Text}");
};

Console.ReadLine();
cts.Cancel();
