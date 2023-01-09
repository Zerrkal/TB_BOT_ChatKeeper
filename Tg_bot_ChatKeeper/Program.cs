using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var botClient = new TelegramBotClient("5622869579:AAGr07dfWYGgw-KWzQhALiZyq8TrVJnYelw");

using var cts = new CancellationTokenSource();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = new UpdateType[] { UpdateType.ChatJoinRequest } // receive all update types
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

    if(update.ChatJoinRequest is { })
    {
        botClient.ApproveChatJoinRequest(-1001702257678, update.ChatJoinRequest.From.Id);
        ChatJoinRequest chatJoinRequest = update.ChatJoinRequest;
        Console.WriteLine($"Join request detected\n " +
            $"Master — {chatJoinRequest.From} invited some slave." +
            $" all stats: 1 {chatJoinRequest.Chat} \n 2 {chatJoinRequest.Bio} \n 3 {chatJoinRequest.InviteLink}");
        return;
    }

    if (update.ChosenInlineResult is { })
    {
        var chosenInlineResult = update.ChosenInlineResult;
        Console.WriteLine($"Chosen inline result {chosenInlineResult}\n " +
            $"Averysing I interesting in \n From — {chosenInlineResult.From.Username}." +
            $" all stats: 111 {chosenInlineResult.Query} \n 112 {chosenInlineResult.InlineMessageId}");
    }

    //Try all updates
    if (update.InlineQuery is { })
    {
        var inlineQuery = update.InlineQuery;
        Console.WriteLine($"inlineQuery {inlineQuery.From.Username} (1)");
    }
    else if (update.ChosenInlineResult is { })
    {
        Console.WriteLine("Chosen Inline Result (2)");
    }
    else if (update.ChatMember is { })
    {
        Console.WriteLine("chat member (9)");
    }
    else if (update.CallbackQuery is { })
    {
        Console.WriteLine("call back query (3)");
    }
    else if (update.ShippingQuery is { })
    {
        Console.WriteLine("Shipping query (4)");
    }
    else if (update.PreCheckoutQuery is { })
    {
        Console.WriteLine("preChackout query (5)");
    }
    else if (update.MyChatMember is { })
    {
        Console.WriteLine("My chat member (8)");
    }
    else if (update.Poll is { })
    {
        Console.WriteLine("Poll (6)");
    }
    else if (update.PollAnswer is { })
    {
        Console.WriteLine("Poll Answer (7)");
    }
    else if (update.ChatJoinRequest is { })
    {
        Console.WriteLine("Chat Join Request (10)");
    }
    else
    {
        Console.WriteLine($"Update ID - {update.Id}");
    }


    // Only process Message
    if (update.Message is not { } message)
    {
        Console.WriteLine($"Not message, is type {update.GetType}, update Id {update.Id}");
        return;
    }

    //botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);

    // Only process text messages
    if (message.Text is not { } messageText)
    {
        if (update.Message.NewChatMembers is { } )
        {
            //var updatesArray = botClient.GetUpdatesAsync(null,null, null, "ChatJoinRequest" , cts.Token);

            var newCatMember = update.Message.NewChatMembers;

            Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"New slave in the gym \ngreetings {update.Message.From.Username} go take 300 buks" ,
            cancellationToken: cancellationToken);

            Console.WriteLine($"New slaves in the gym\n " +
                $"Averysing I interesting in \n New chat member JoinGoup 10 — {newCatMember}." +
                $" all stats: 11 {newCatMember.ToString} \n 12 {newCatMember.GetType}");
        }

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
        //Message sentMessage = await botClient.SendTextMessageAsync(
        //chatId: chatId,
        //text: $"{message.From.FirstName} say \n" + messageText,
        //cancellationToken: cancellationToken);
        if (message.From.Id == 385246590)
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Mr {message.From.Username} say \n" + messageText,
            cancellationToken: cancellationToken);
        }
        else
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Slave {message.From.FirstName} say \n" + messageText,
            cancellationToken: cancellationToken);
        }
        
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
