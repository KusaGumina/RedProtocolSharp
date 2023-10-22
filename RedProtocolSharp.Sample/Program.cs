#region

using RedProtocolSharp;
using RedProtocolSharp.Invoker;
using RedProtocolSharp.Message;

#endregion

public class Sample
{
    public static async Task Main()
    {
        var bot = new Bot("localhost:16531", "650ab2f2e5aa80ff890a2bd66c021bb7518cb0077d15f1f6a8890de3800329dd");
        bot.Logger.BotLog.OnLogger += BotLogOnHandler;
        bot.Invoker.OnGroupMessageReceived += InvokerOnOnGroupMessageReceived;
        bot.Invoker.OnPrivateMessageReceived += InvokerOnOnPrivateMessageReceived;
        bot.Invoker.OnGroupMemberAdd += InvokerOnOnGroupMemberAdd;
        await bot.Start();
        await bot.Send.SendMessageChain(new MessageBuilder().SetTarget("114514", ChatTypes.GroupMessage).AddText("test").Build());
        while (true)
        {
        }
    }

    private static void InvokerOnOnGroupMemberAdd(GroupMemberAddEventArgs e)
    {
        Console.WriteLine();
    }

    private static void InvokerOnOnPrivateMessageReceived(MessageEventArgs e)
    {
        Console.WriteLine();
    }

    private static void InvokerOnOnGroupMessageReceived(MessageEventArgs e)
    {
        Console.WriteLine(e.chain.PeerUin);
        foreach (var item in e.chain)
        {
            if (item is TextElement textElement)
            {
                Console.WriteLine(textElement.content);
            }
        }
    }

    private static void BotLogOnHandler(BotLogger.Levels levels, string content)
    {
        //Console.WriteLine($"{levels} : {content}");
    }
}