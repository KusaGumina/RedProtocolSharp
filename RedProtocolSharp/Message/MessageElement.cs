namespace RedProtocolSharp.Message;

public interface IMessageElement
{
    public string ToSummary();
}

public class TextElement : IMessageElement
{
    public string content { get; set; } = "";
    public string ToSummary()
    {
        return $@"[Message:Text,Content=""{content}""]";
    }
}
public class AtElement : IMessageElement
{
    public string target { get; set; } = "";
    public string content { get; set; } = "";
    public string ToSummary()
    {
        return $@"[Message:At,Target=""{target}"",Content=""{content}""]";
    }
}
public class ImageElement : IMessageElement
{
    public string md5 { get; set; } = "";
    public string name { get; set; } = "";
    public string url { get; set; } = "";
    public string sourcePath { get; set; } = "";
    public string size { get; set; } = "";
    public string ToSummary()
    {
        return $@"[Message:Image,Url=""{url}""]";
    }
}

public class VoiceElement : IMessageElement
{
    public string md5 { get; set; } = "";
    public string name { get; set; } = "";
    public string filePath { get; set; } = "";
    public string size { get; set; } = "";
    public int duration { get; set; }
    public int[]? waveAmplitudes { get; set; }
    public string ToSummary()
    {
        return $@"[Message:Voice,Name=""{name}""]";
    }
}
public class ReplyElement : IMessageElement
{
    public string replyTargetUin { get; set; }
    public string replyMsgSeq { get; set; }
    
    public string ToSummary()
    {
        return $@"[Message:Reply,Uin=""{replyTargetUin}"",Seq=""{replyMsgSeq}""]";
    }
}

public class MultiForwardElement : IMessageElement
{
    public string resId { get; set; }
    public string xmlContent { get; set; }
    public string ToSummary()
    {
        return $@"[Message:Forward,ResId=""{resId}""]";
    }
}
