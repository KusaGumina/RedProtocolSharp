using System.Text.RegularExpressions;
using RedProtocolSharp.Message;

namespace RedProtocolSharp.Invoker;

public class MessageEventArgs : EventArgs
{
    internal MsgType.Payload<MsgType.MessageRecv> originPayload { get; set; }
    public Bot bot { get; set; }
    public MessageChain chain { get; set; }

    public MessageEventArgs(MsgType.Payload<MsgType.MessageRecv> originPayload, MessageChain chain,Bot bot)
    {
        this.originPayload = originPayload;
        this.chain = chain;
        this.bot = bot;
    }
}

public class GroupMemberAddEventArgs : EventArgs
{
    internal MsgType.Payload<MsgType.MessageRecv> originPayload { get; set; }
    public Bot bot { get; set; }
    public MemberAddMessage memberAddMessage { get; set; }
    public GroupMemberAddEventArgs(MsgType.Payload<MsgType.MessageRecv> originPayload, MemberAddMessage memberAddMessage,Bot bot)
    {
        this.originPayload = originPayload;
        this.memberAddMessage = memberAddMessage;
        this.bot = bot;
    }
}
public class Events
{
    internal Bot bot;
    public Events(Bot bot)
    {
        this.bot = bot;
    }
    //群组消息
    public delegate void GroupMessageReceived(MessageEventArgs e);
    public event GroupMessageReceived OnGroupMessageReceived;
    internal bool OnGroupMessageReceivedTriggered(MsgType.Payload<MsgType.MessageRecv> originPayload)
    {
        var senderInfo = originPayload.GetMessageInfo();
        if(senderInfo.ChatType != 2) return false;
        MessageChain chain = new MessageChain()
        {
            MessageType = MessageChain.Message.GroupMessage,
            MsgId = senderInfo.MsgId,
            MsgSeq = senderInfo.ReplayMsgSeq,
            Time = senderInfo.Time,
            SenderUin = senderInfo.Uin,
            SenderName = senderInfo.SenderName,
            GroupUin = senderInfo.PeerUin,
            GroupName = senderInfo.PeerName,
        };
        switch (senderInfo.RoleType)
        {
            case 2:
                chain.RoleType = MessageChain.Role.Member;
                break;
            case 3:
                chain.RoleType = MessageChain.Role.Admin;
                break;
            case 4:
                chain.RoleType = MessageChain.Role.Owner;
                break;
        }
        chain.Parse(originPayload);
        if (chain.Count == 0)
        {
            bot.Logger.Error("不支持的消息!");
            return false;
        }
        MessageEventArgs eventArgs = new MessageEventArgs(originPayload,chain,bot);
        OnGroupMessageReceived?.Invoke(eventArgs);
        return true;
    }
    //私聊消息
    public delegate void PrivateMessageReceived(MessageEventArgs e);
    public event PrivateMessageReceived OnPrivateMessageReceived;
    internal bool OnPrivateMessageReceivedTriggered(MsgType.Payload<MsgType.MessageRecv> originPayload)
    {
        var senderInfo = originPayload.GetMessageInfo();
        if(senderInfo.ChatType != 1) return false;
        MessageChain chain = new MessageChain()
        {
            MessageType = MessageChain.Message.PrivateMessage,
            MsgId = senderInfo.MsgId,
            MsgSeq = senderInfo.ReplayMsgSeq,
            Time = senderInfo.Time,
            SenderUin = senderInfo.Uin
        };
        chain.Parse(originPayload);
        if (chain.Count == 0)
        {
            bot.Logger.Error("不支持的消息!");
            return false;
        }
        MessageEventArgs eventArgs = new MessageEventArgs(originPayload,chain,bot);
        OnPrivateMessageReceived?.Invoke(eventArgs);
        return true;
    }
    //加群事件
    public delegate void GroupMemberAdd(GroupMemberAddEventArgs e);
    public event GroupMemberAdd OnGroupMemberAdd;
    internal bool OnGroupMemberAddTriggered(MsgType.Payload<MsgType.MessageRecv> originPayload)
    {
        var senderInfo = originPayload.GetMessageInfo();
        if(senderInfo.ChatType != 2) return false;
        if (originPayload.payload.elements[0].elementType!=8) return false;
        MemberAddMessage memberAddMessage = new MemberAddMessage();
        switch (originPayload.payload.elements[0].grayTipElement.subElementType)
        {
            case 4:
                memberAddMessage.MemberUin = originPayload.payload.elements[0].grayTipElement.groupElement.memberAdd
                    .otherAdd.uin;
                memberAddMessage.MemberName = originPayload.payload.elements[0].grayTipElement.groupElement.memberAdd
                    .otherAdd.name;
                memberAddMessage.GroupUin = originPayload.payload.peerUin;
                break;
            case 12:
                var regex = new Regex(
                    @"<gtip align=\\""""center\\""""><qq uin=\\""""(.*?)\\"""" col=\\""""3\\"""" jp=\\""""(.*?)\\"""" /><nor txt=\\""""邀请\\""""/><qq uin=\\""""(.*?)\\"""" col=\\""""3\\"""" jp=\\""""(.*?)\\"""" /> <nor txt=\\""""加入了群聊。\\""""/> </gtip>");
                var match = regex.Match(originPayload.payload.elements[0].grayTipElement.xmlElement.content);
                if (!match.Success) return false;
                memberAddMessage.MemberUin = match.Groups[4].Value;
                memberAddMessage.MemberUin = "";
                memberAddMessage.GroupUin = originPayload.payload.peerUin;
                break;
            default:
                return false;
        }

        GroupMemberAddEventArgs eventArgs = new GroupMemberAddEventArgs(originPayload, memberAddMessage, bot);
        OnGroupMemberAdd?.Invoke(eventArgs);
        return true;
    }
}