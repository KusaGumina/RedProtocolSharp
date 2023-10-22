#region

using RedProtocolSharp.Convert;

#endregion

namespace RedProtocolSharp.Message;

public sealed class MessageBuilder
{
    private readonly MessageChain _chain = new ();
    public MessageBuilder SetTarget(string target, ChatTypes chatTypes)
    {
        _chain.PeerUin = target;
        _chain.chatTypes = chatTypes;
        return this;
    }

    public MessageBuilder AddText(string content)
    {
        _chain.Add(new TextElement
        {
            content = content
        });
        return this;
    }

    public MessageBuilder AddAt(string target)
    {
        _chain.Add(new AtElement
        {
            target = target
        });
        return this;
    }

    public MessageBuilder AddReply(string replayMsgSeq, string replyMsgId, string targetUin)
    {
        _chain.Add(new ReplyElement
        {
            replyMsgSeq = replayMsgSeq,
            replyTargetUin = targetUin
        });
        return this;
    }

    public MessageBuilder AddPic(string filePath)
    {
        _chain.Add(new ImageElement
        {
            sourcePath = filePath
        });
        return this;
    }

    public MessageBuilder AddVoice(string filePath)
    {
        try
        {
            var voiceInfo = VoiceConverter.Mp3ToSilk(filePath);
            if (voiceInfo != null)
                _chain.Add(new VoiceElement
                {
                    filePath = voiceInfo.filePath,
                    name = voiceInfo.name,
                    duration = voiceInfo.duration
                });
        }
        catch (Exception e)
        {
            return this;
        }
        return this;
    }

    public MessageChain Build() => _chain;
}