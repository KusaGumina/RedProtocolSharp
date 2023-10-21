# RedProtocolSharp

--基于RedProtocol的C# SDK,应用于某猫

## 警告:当前RedProtocol与RedProtocolSharp均处于开发阶段,未来结构可能产生较大变化,请做好随时变更代码的准备

### 介绍

RedProtocolSharp将RedProtocol基础的消息格式进行了封装,并将收到的消息封为Event,可供开发者快速开发Bot框架

### 功能表

- [x] 接收源消息

- [x] 源消息文本解析

- [x] 源消息图片解析

- [x] 源消息at解析

- [x] 源消息回复解析

- [x] 源消息发送者信息解析

- [ ] json消息解析(Red尚未提供实现)

  ---

- [x] 发送文本消息

- [x] 发送图片消息

- [x] 发送at消息

- [x] 发送回复消息

- [ ] 发送合并转发消息

- [x] 发送语音消息 无需安装其他外置库,感谢[silk-rs](https://github.com/lz1998/silk-rs)提供的dll,语音仅支持mp3格式,直接填写fileName即可

- [x] 撤回消息

  ---
  
- [x] 获取Bot自身消息

- [x] 获取Bot好友列表(Bug)

- [x] 获取Bot所在某群群员列表(Bug)

  ---
  
- [x] 踢人

- [x] 禁言人

- [x] 全体禁言

### 已知问题/feature

1. GetMemberList返回空数组(打开UI可能可以解决问题)

### 安装

Nuget搜索RedProtocolSharp安装

或

引入Release下的Dll,并自行安装WebSocketSharp,NewtonSoft.JSON相关依赖(不推荐)

### 示例

#### 可去Sample查看基础用法,下面的内容可能并不会及时更新

###### 启动Bot

```c#
var bot = new Bot("localhost:16530","yourToken");
bot.Start();
```

###### 注册Logger

```c#
bot.Logger.BotLog.OnLogger += BotLogOnHandler;
private static void BotLogOnHandler(BotLogger.Levels levels, string content)
{
    Console.WriteLine($"{levels} : {content}");
}
```

###### 订阅消息

```c#
bot.Invoker.OnGroupMessageReceived += InvokerOnOnGroupMessageReceived;
    
private static void InvokerOnOnGroupMessageReceived(MessageEventArgs e)
{
    Console.WriteLine(e.chain.GroupUin);
    foreach (var item in e.chain)
    {
        if (item is TextElement textElement)
        {
            Console.WriteLine(textElement.content);
        }
    }
}
```

###### 发送消息

```c#
var reply = await bot.Send
        .SetTarget("1919810", ChatTypes.GroupMessage)
        .AddText("testMessage")
        .AddAt("114514")
        .AddPic("test.png")
        .SendMessage();
//向群1919810发送内容为testMessage,艾特用户114514,附带图片test.png的消息
//Extra
var chain = new MessageChain()
{
    PeerUin = "1919810",
    chatTypes = ChatTypes.GroupMessage
};
chain.Add(new TextElement()
{
    content = "text"
});
chain.Add(new AtElement()
{
    target = "114514"
});
chain.Add(new ImageElement()
{
    sourcePath = "test.png"
});
await bot.Send.SendMessage(chain);
//该方法等效于上面的链式调用方法
```

###### 撤回消息

```c#
//假设上一步发送返回了reply
await bot.Action.Revoke(new []{reply.msgId}, reply.chatTypes, reply.peerUin);
```

###### 获取Bot自身信息

```c#
var selfInfo = bot.Get.SelfInfo();
```

###### 踢人

```c#
await bot.Action.Kick("114514", "1919810", false, "test");
//从群114514踢出用户1919810,不永拒,理由为test
```

其他操作请自行探索/类比

### 案例

LlisBot(尚在开发中,暂未提供)