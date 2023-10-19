using Newtonsoft.Json;

namespace RedProtocolSharp.Oprator;

  public class BotSend
    {
        internal abstract class SendMsgHelper
        {
        }
        internal class atHelper : SendMsgHelper
        {
            public string atTarget { get; set; }
        }
        internal class replyHelper : SendMsgHelper
        {
            public string replayMsgSeq { get; set; }
            public string replyMsgId { get; set; }
            public string senderUin { get; set; }
        }
        internal class textHelper : SendMsgHelper
        {
            public string content { get; set; }
        }
        internal class picHelper : SendMsgHelper
        {
            public string filePath { get; set; }
        }

        public enum ChatType
        {
            Group,
            Private
        }
        private string sendNode = "message/send";
        private int chatType;
        private string sendTarget;
        private Bot bot;
        public BotSend(Bot sender)
        {
            bot = sender;
        }
        private List<SendMsgHelper> helper = new ();

        public BotSend SetTarget(string target, ChatType chatTypes)
        {
            chatType = chatTypes switch
            {
                ChatType.Group => 2,
                ChatType.Private => 1,
                _ => 0
            };
            sendTarget = target;
            return this;
        }
        public BotSend AddText(string content)
        {
            helper.Add(new textHelper()
            {
                content = content
            });
            return this;
        }
        public BotSend AddAt(string target)
        {
            helper.Add(new atHelper()
            {
                atTarget = target
            });
            return this;
        }
        public BotSend AddReply(string replayMsgSeq,string replyMsgId,string senderUin)
        {
            helper.Add(new replyHelper()
            {
                replayMsgSeq = replayMsgSeq,
                replyMsgId = replyMsgId,
                senderUin = senderUin
            });
            return this;
        }
        public BotSend AddPic(string filePath)
        {
            helper.Add(new picHelper()
            {
                filePath = filePath
            });
            return this;
        }
        public async Task<bool> SendMessage()
        {
            var payload = new MsgType.MessageSend()
            {
                elements = new List<MsgType.Elements>(),
                peer = new MsgType.Peer()
                {
                    chatType = chatType,
                    peerUin = sendTarget
                }
            };
            foreach (var item in helper)
                switch (item)
                {
                    case atHelper data:
                    {
                        payload.elements.Add(new MsgType.Elements
                        {
                            elementType = 1,
                            textElement = new MsgType.TextElement
                            {
                                atType = 2,
                                atNtUin = data.atTarget
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
                    case replyHelper data:
                    {
                        var sendUinInt = long.Parse(data.senderUin);
                        payload.elements.Add(new MsgType.Elements
                        {
                            elementType = 7,
                            replyElement = new MsgType.ReplyElement
                            {
                                replayMsgSeq = data.replayMsgSeq,
                                replyMsgId = data.replyMsgId,
                                senderUinStr = data.senderUin
                            }
                        });
                        break;
                    }
                    case textHelper data:
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
                    case picHelper data:
                    {
                        var uploadNode = "upload";
                        var fileName = Path.GetFileName(data.filePath);
                        var uploadReply = await bot.httpPostUpload(data.filePath, uploadNode);
                        var picReply = new MsgType.UploadPic();
                        if (uploadReply != "")
                            picReply = JsonConvert.DeserializeObject<MsgType.UploadPic>(uploadReply);
                        else
                            return false;
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
                }
            var package = JsonConvert.SerializeObject(payload);
            var reply = await bot.httpPostRequest(package, sendNode);
            if (reply != "")
            {
                return true;
            }
            return false;
        }
    }