using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using RedProtocolSharp.Invoker;
using RedProtocolSharp.Oprator;
using WebSocketSharp;
using MessageEventArgs = WebSocketSharp.MessageEventArgs;

namespace RedProtocolSharp;

public class Bot : IDisposable
{
    public Bot(string url,string authorizationToken)
    {
        //Bot自身信息初始化
        Url = url;
        AuthorizationToken = authorizationToken;
        HttpServerUrl = $@"http://{url}/api/";
        //Bot扩展初始化
        Send = new BotSend(this);
        Get = new BotGet(this);
        Action = new BotAction(this);
        Invoker = new Events(this);
        //cache文件夹初始化
        string cachePath = AppDomain.CurrentDomain.BaseDirectory + @"Cache\RedProtocolSharp";
        Directory.CreateDirectory(cachePath);
    }
    public BotLogger Logger = new ();
    public BotSend Send;
    public BotGet Get;
    public BotAction Action;
    public Events Invoker;
    public async Task Start()
    {

        //连接
        var ws = new WebSocket($@"ws://{Url}/");
        ws.OnMessage += ReceiveConnectMsg;
        ws.Connect();
        if (!ws.IsAlive) Reconnect(ws);
        ws.Send(SendConnectMsg(AuthorizationToken));
        //等待连接成功
        while (!ConnectSucceed)
        {
        }
        ws.OnMessage -= ReceiveConnectMsg;
        ws.OnClose += (sender, e) => Reconnect(ws);
        ws.OnMessage += (sender,e) => MessageReceive.ReceiveMessage(sender,e,this);
        //debug直接输出包
        ws.OnMessage += MessageReceiveLog;
        //消息分发
        MessageReceive.OnMessageReceived += DevideMessage;
        //INFO输出接收消息
        Invoker.OnGroupMessageReceived += GroupMessageLog;
        Invoker.OnPrivateMessageReceived += PrivateMessageLog;
    }

    private void DevideMessage(MsgType.Payload payload)
    {
        if (payload is MsgType.Payload<List<MsgType.MessageRecv>> dt)
        {
            var data = new MsgType.Payload<MsgType.MessageRecv>();
            data.payload = dt.payload[0];
            if(Invoker.OnGroupMessageReceivedTriggered(data)) return;
            if(Invoker.OnPrivateMessageReceivedTriggered(data)) return;
            if(Invoker.OnGroupMemberAddTriggered(data)) return;
        }
    }

    #region BotInfo
    private readonly string Url;
    private readonly string AuthorizationToken;
    private readonly string HttpServerUrl;
    public string ChronoCatVersion = "";
    public bool ConnectSucceed; 
    #endregion
    #region Connect
    private string SendConnectMsg(string authorizationToken)
    {
        var payload = new MsgType.Payload<MsgType.ConnectSend>
        {
            type = "meta::connect",
            payload = new MsgType.ConnectSend
            {
                token = authorizationToken
            }
        };
        var package = JsonConvert.SerializeObject(payload);
        return package;
    }
    private void ReceiveConnectMsg(object? sender,MessageEventArgs e)
    {
        var payload = JsonConvert.DeserializeObject<MsgType.Payload>(e.Data);
        try
        {
            if (payload.type == "meta::connect")
            {
                var connectData = JsonConvert.DeserializeObject<MsgType.Payload<MsgType.ConnectRecv>>(e.Data);
                ChronoCatVersion = connectData.payload.version;
                Logger.Info("成功连接到ChronoCat!");
                ConnectSucceed = true;
            }
            throw new Exception();
        }
        catch
        {
        }
    }
    private void Reconnect(WebSocket ws)
    {
        ws.OnMessage += (sender, e) => ReceiveConnectMsg(sender,e);
        Reconnect:
        Logger.Info("ChronoCat连接已关闭!请检查你的ChronoCat!将在五秒后尝试自动重连");
        Thread.Sleep(5000);
        ws.Connect();
        if(!ws.IsAlive) goto Reconnect;
    }
    #endregion
    #region MessageQueue
    internal class MessageReceiveQueue : EventArgs
    {
        internal delegate void MessageReceiveHandler(MsgType.Payload payload);
        internal event MessageReceiveHandler OnMessageReceived;
        internal void ReceiveMessage(object? sender,MessageEventArgs e,Bot bot)
        {
            var payload = bot.parseMsg(e.Data);
            OnMessageReceived?.Invoke(payload);
        }
    }
    internal MessageReceiveQueue MessageReceive = new();
    private MsgType.Payload parseMsg(string originMsg)
    {
        var payload = new MsgType.Payload();
        payload = JsonConvert.DeserializeObject<MsgType.Payload>(originMsg);
        try
        {
            if (payload.type == "message::recv")
            {
                var messageData = JsonConvert.DeserializeObject<MsgType.Payload<List<MsgType.MessageRecv>>>(originMsg);
                if (messageData != null && messageData.payload.Count != 0)
                    return messageData;
            }

            throw new FormatException();
        }
        catch (JsonException)
        {
            throw new FormatException();
        }
        catch (FormatException)
        {
            Logger.Warn("不受支持的消息!(可忽视)");
        }
        return null;
    }
    #endregion
    #region SendMessage
    internal async Task<string> httpGetRequest(string node)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", AuthorizationToken);
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                var response = await httpClient.GetAsync(HttpServerUrl + node);
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
            }
            catch (Exception e)
            {
            }

            Logger.Error("Http请求出错!请求失败/请求超时");
            return null;
        }
    }
    internal async Task<string> httpPostRequest(string content, string node)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", AuthorizationToken);
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                var postContent = new StringContent(content, Encoding.UTF8);
                var response = await httpClient.PostAsync(HttpServerUrl + node, postContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
            }
            catch (Exception e)
            {
            }

            return "";
        }
    }
    internal async Task<string> httpPostUpload(string filePath, string node)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", AuthorizationToken);
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                using (var formData = new MultipartFormDataContent())
                {
                    using (var fs = File.OpenRead(filePath))
                    {
                        formData.Add(new StreamContent(fs), "file", Path.GetFileName(filePath));
                        var response = await httpClient.PostAsync(HttpServerUrl + node, formData);
                        if (response.IsSuccessStatusCode)
                        {
                            var responseBody = await response.Content.ReadAsStringAsync();
                            return responseBody;
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }

            return null;
        }
    }
    #endregion
    #region Log

    private void MessageReceiveLog(object? sender,MessageEventArgs e)
    {
        Logger.Debug(e.Data);
    }
    private void GroupMessageLog(Invoker.MessageEventArgs e)
    {
        Logger.Info(@$"收到群聊消息:{e.chain.ToSummary()}");
    }
    private void PrivateMessageLog(Invoker.MessageEventArgs e)
    {
        Logger.Info(@$"收到私聊消息:{e.chain.ToSummary()}");
    }
    #endregion

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Invoker.Dispose();
    }
}