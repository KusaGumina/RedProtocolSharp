using Newtonsoft.Json;
using RedProtocolSharp.Convert;
using RedProtocolSharp.Message;

namespace RedProtocolSharp.Oprator;

  public class BotSend
    {
        #region Echo
        public class SendEcho
        {
            public int? chatType { get; set; }
            public string? msgId { get; set; }
            public string? msgSeq { get; set; }
            public string? msgTime { get; set; }
            public string? senderUin { get; set; }
            public string? sendMemberName { get; set; }
            public string? sendNickName { get; set; }
            public string? peerUin { get; set; }
            public string? peerName { get; set; }
            [JsonIgnore]
            public ChatTypes chatTypes { get; set; }
        }
        #endregion
        
        private Bot bot;
        private string sendNode = "message/send";
        
        public BotSend(Bot sender)
        {
            bot = sender;
        }
        private MessageChain _chain = new MessageChain();
        public BotSend SetTarget(string target, ChatTypes chatTypes)
        {
            _chain.PeerUin = target;
            _chain.chatTypes = chatTypes;
            return this;
        }
        public BotSend AddText(string content)
        {
            _chain.Add(new TextElement()
            {
                content = content
            });
            return this;
        }
        public BotSend AddAt(string target)
        {
            _chain.Add(new AtElement()
            {
                target = target
            });
            return this;
        }
        public BotSend AddReply(string replayMsgSeq,string replyMsgId,string targetUin)
        {
            _chain.Add(new ReplyElement()
            {
                replyMsgSeq = replayMsgSeq,
                replyTargetUin = targetUin
            });
            return this;
        }
        public BotSend AddPic(string filePath)
        {
            _chain.Add(new ImageElement()
            {
                sourcePath = filePath
            });
            return this;
        }

        public BotSend AddVoice(string filePath)
        {
            try
            {
                var voiceInfo = VoiceConverter.Mp3ToSilk(filePath);
                if (voiceInfo != null)
                {
                    _chain.Add(new VoiceElement()
                    {
                        filePath = voiceInfo.filePath,
                        name = voiceInfo.name,
                        duration = voiceInfo.duration
                    });
                }
            }
            catch (Exception e)
            {
                return this;
            }
            return this;
        }
        public async Task<SendEcho> SendMessage()
        {
            try
            {
                var payload = new MsgType.MessageSend()
                {
                    elements = new List<MsgType.Elements>(),
                    peer = new MsgType.Peer()
                    {
                        chatType = _chain.chatTypes switch
                        {
                            ChatTypes.GroupMessage => 2,
                            ChatTypes.PrivateMessage => 1,
                            _ => 0
                        },
                        peerUin = _chain.PeerUin
                    }
                };
                foreach (var item in _chain)
                    switch (item)
                    {
                        case AtElement data:
                        {
                            payload.elements.Add(new MsgType.Elements
                            {
                                elementType = 1,
                                textElement = new MsgType.TextElement
                                {
                                    atType = 2,
                                    atNtUin = data.target
                                }
                            });
                            payload.elements.Add(new MsgType.Elements
                            {
                                elementType = 1,
                                textElement = new MsgType.TextElement
                                {
                                    content = " "
                                }
                            });
                            break;
                        }
                        case ReplyElement data:
                        {
                            payload.elements.Add(new MsgType.Elements
                            {
                                elementType = 7,
                                replyElement = new MsgType.ReplyElement
                                {
                                    replayMsgSeq = data.replyMsgSeq,
                                    senderUinStr = data.replyTargetUin
                                }
                            });
                            break;
                        }
                        case TextElement data:
                        {
                            payload.elements.Add(new MsgType.Elements
                            {
                                elementType = 1,
                                textElement = new MsgType.TextElement
                                {
                                    content = data.content
                                }
                            });
                            break;
                        }
                        case ImageElement data:
                        {
                            var uploadNode = "upload";
                            var fileName = Path.GetFileName(data.sourcePath);
                            var uploadReply = await bot.httpPostUpload(data.sourcePath, uploadNode);
                            var picReply = new MsgType.UploadData();
                            if (uploadReply != "")
                                picReply = JsonConvert.DeserializeObject<MsgType.UploadData>(uploadReply);
                            else
                                return null;
                            payload.elements.Add(new MsgType.Elements
                            {
                                elementType = 2,
                                picElement = new MsgType.PicElement
                                {
                                    md5HexStr = picReply.md5,
                                    fileSize = picReply.fileSize,
                                    fileName = fileName,
                                    sourcePath = picReply.ntFilePath,
                                    picHeight = picReply.imageInfo.height,
                                    picWidth = picReply.imageInfo.width
                                }
                            });
                            break;
                        }
                        case VoiceElement data:
                        {
                            var uploadNode = "upload";
                            var uploadReply = await bot.httpPostUpload(data.filePath, uploadNode);
                            var voiceReply = new MsgType.UploadData();
                            if (uploadReply != "")
                                voiceReply = JsonConvert.DeserializeObject<MsgType.UploadData>(uploadReply);
                            else
                                return null;
                            if (voiceReply == null) return null;
                            payload.elements.Add(new MsgType.Elements
                            {
                                elementType = 4,
                                pttElement = new MsgType.PttElement()
                                {
                                    md5HexStr = voiceReply.md5,
                                    duration = data.duration,
                                    fileName = Path.GetFileName(voiceReply.ntFilePath),
                                    filePath = voiceReply.ntFilePath,
                                    fileSize = voiceReply.fileSize,
                                    waveAmplitudes = new []{0,1,3,3,1,0}
                                }
                            });
                            break;
                        }
                    }

                var package = JsonConvert.SerializeObject(payload);
                var reply = await bot.httpPostRequest(package, sendNode);
                var echo = JsonConvert.DeserializeObject<SendEcho>(reply);
                if (echo != null)
                {
                    echo.chatTypes = echo.chatType switch
                    {
                        1 => ChatTypes.PrivateMessage,
                        2 => ChatTypes.GroupMessage,
                        _ => echo.chatTypes
                    };
                }

                return echo;
            }
            finally
            {
                _chain = new MessageChain();
            }
        }

        public async Task<SendEcho> SendMessage(MessageChain chain)
        {
            _chain = chain;
            return await SendMessage();
        }
    }