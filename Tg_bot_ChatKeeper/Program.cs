using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var botClient = new TelegramBotClient("5622869579:AAGr07dfWYGgw-KWzQhALiZyq8TrVJnYelw");

using var cts = new CancellationTokenSource();

List <long> userIdSendMsg = new List<long>();
List<string> userSendMsgLinkName = new List<string>();
List<int> userSendMsgLinkCount = new List<int>();

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
    else if (update.ChatJoinRequest is { } joinRequest)
    {
        Console.WriteLine("Chat Join Request (10)");
        if (userSendMsgLinkName.Contains(joinRequest.InviteLink.InviteLink))
        {
            userSendMsgLinkCount[userSendMsgLinkName.IndexOf(joinRequest.InviteLink.InviteLink)]++;
            Console.WriteLine($"Count invited people {userSendMsgLinkCount[userSendMsgLinkName.IndexOf(joinRequest.InviteLink.InviteLink)]}," +
            $"Index {userSendMsgLinkName.IndexOf(joinRequest.InviteLink.InviteLink)}");
        }
        else { Console.WriteLine($"Not found {joinRequest.InviteLink.InviteLink}"); }

        botClient.ApproveChatJoinRequest(joinRequest.Chat.Id, joinRequest.From.Id);
    }
    else if (update.ChatMember is { })
    {
        Console.WriteLine("Chat ChatMember (11)");
    }
    else
    {
        Console.WriteLine($"Update ID - {update.Id}");
    }

    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
    {
        Console.WriteLine($"Not message 1, is {update.GetType}");
        return;
    }


    var chatId = message.Chat.Id;
    int j;
    Console.WriteLine("Message from " + update.Message.From.FirstName);

    //botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);

    if (message.NewChatMembers is { })
    {
        User[] newMembers = message.NewChatMembers;
        for (int i = 0; i < newMembers.Length; i++)
        {
            userIdSendMsg.Add(newMembers[i].Id);
            ChatInviteLink link = await botClient.CreateChatInviteLinkAsync(chatId, null, null, null, true);
            
            Console.WriteLine("New member\n Link: " + link.InviteLink);
            userSendMsgLinkName.Add(link.InviteLink);
            userSendMsgLinkCount.Add(0);
        }
    }
    else if (message.LeftChatMember is { })
    {
        if (userIdSendMsg.Contains(message.From.Id))
        {
            User leftMember = message.LeftChatMember;
            int userNumber = userIdSendMsg.IndexOf(leftMember.Id);
            if (userSendMsgLinkName[userNumber] is not null)
            {
                botClient.RevokeChatInviteLinkAsync(chatId, userSendMsgLinkName[userNumber]);
                userIdSendMsg.RemoveAt(userNumber);
                userSendMsgLinkName.RemoveAt(userNumber);
                userSendMsgLinkCount.RemoveAt(userNumber);
            }
        }
        Console.WriteLine("Left member");
    }

    // Only process text messages
    if (message.Text is not { } messageText)
    {
        Console.WriteLine($"Not text 2, is {update.GetType}, {message.Type}");
        return;
    }


    if (userIdSendMsg.Contains(message.From.Id))
    {
        string messageAnswer = "Your invite link: " + userSendMsgLinkName[userIdSendMsg.IndexOf(message.From.Id)];

        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: messageAnswer,
            cancellationToken: cancellationToken);
    }


    //    Message sentMessage = await botClient.SendTextMessageAsync(
    //    chatId: chatId,
    //    text: messageText,
    //    cancellationToken: cancellationToken);

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
    return
 Task.CompletedTask;
}