using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var botClient = new TelegramBotClient("token");

using var cts = new CancellationTokenSource();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};
botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);


var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
    {
        Console.WriteLine($"Not message 1, is {update.GetType}");
        return;
    }

    botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
    //if (message.Sticker is not { } messageSticker)
    //{
    //    Message sentMessage = await botClient.SendStickerAsync(
    //    chatId: message.Chat.Id,
    //    sticker: message.Sticker.FileId,
    //    cancellationToken: cancellationToken);
    //}

    // Only process text messages
    if (message.Text is not { } messageText)
    {
        Console.WriteLine($"Not text 2, is {update.GetType}, {message.Type}");
        return;
    }



    var chatId = message.Chat.Id;

    string[] gachiStickers = new string[] {
        "https://tlgrm.eu/_/stickers/06d/991/06d991f7-564f-47cd-8180-585cd0056a42/2.webp",
        "https://tlgrm.eu/_/stickers/06d/991/06d991f7-564f-47cd-8180-585cd0056a42/5.webp",
        "https://tlgrm.eu/_/stickers/06d/991/06d991f7-564f-47cd-8180-585cd0056a42/6.webp"
    };

    if (messageText.Contains("gachi") || messageText.Contains("Gachi"))
    {
        Message sentMessage = await botClient.SendStickerAsync(
        chatId: chatId,
        sticker: gachiStickers[new Random().Next(gachiStickers.Length)],
        cancellationToken: cancellationToken);

    }
    else if (messageText.Contains("?"))
    {
        Message sentMessage = await botClient.SendAnimationAsync(
        chatId: chatId,
        animation: "https://tenor.com/view/nodding-kermit-uh-huh-gif-19083131",
        caption: "",
        cancellationToken: cancellationToken);

    }
    else
    {
        Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: messageText,
        cancellationToken: cancellationToken);
    }

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

    // Echo received message text

}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}
