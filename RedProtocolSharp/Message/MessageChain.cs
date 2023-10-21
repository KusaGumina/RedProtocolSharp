namespace RedProtocolSharp.Message;

public class MessageChain : List<IMessageElement>
{
    public enum Role
    {
        Member = 2,
        Admin = 3,
        Owner = 4
    }
    public ChatTypes chatTypes { get; set; }
    public string SenderUin { get; set; } = "";
    public string SenderName { get; set; } = "";
    public string PeerUin { get; set; } = "";
    public string GroupName { get; set; } = "";
    public Role? RoleType { get; set; }
    public DateTime Time { get; set; }
    public string MsgSeq { get; set; }
    public string MsgId { get; set; }
    internal void Parse(MsgType.Payload<MsgType.MessageRecv> originPayload)
    {
        foreach (var item in originPayload.payload.elements)
        {
            switch (item.elementType)
            {
                //文本|at
                case 1:
                    if (item.textElement.atType == 2)
                    {
                        Add(new AtElement()
                        {
                            target = item.textElement.atNtUin,
                            content = item.textElement.content
                        });
                    }
                    else
                    {
                        Add(new TextElement()
                        {
                            content = item.textElement.content
                        });
                    }
                    break;
                //图片
                case 2:
                    Add(new ImageElement()
                    {
                        md5 = item.picElement.md5HexStr,
                        name = item.picElement.fileName,
                        sourcePath = item.picElement.sourcePath,
                        size = item.picElement.fileSize,
                        url = $@"https://gchat.qpic.cn/gchatpic_new/0/0-0-{item.picElement.md5HexStr}/0"
                    });
                    break;
                //语音
                case 4:
                    Add(new VoiceElement()
                    {
                        md5 = item.pttElement.md5HexStr,
                        name = item.pttElement.fileName,
                        filePath = item.pttElement.filePath,
                        size = item.pttElement.fileSize,
                        duration = item.pttElement.duration
                    });
                    break;
                //回复
                case 7:
                    Add(new ReplyElement()
                    {
                        replyTargetUin = item.replyElement.senderUid,
                        replyMsgSeq = item.replyElement.replayMsgSeq,
                    });
                    break;
                //合并转发
                case 16:
                    Add(new MultiForwardElement()
                    {
                        resId = item.multiForwardMsgElement.resId,
                        xmlContent = item.multiForwardMsgElement.xmlContent
                    });
                    break;
            }
        }
    }

    public string ToSummary()
    {
        string summary = $"[From{GroupName}({PeerUin}) {SenderName}({SenderUin}):";
        foreach (var item in this)
        {
            summary += item.ToSummary();
        }

        summary += "]";
        return summary;
    }
}