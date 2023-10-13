# RedProtocolSharp

--基于RedProtocol的C# SDK,应用于ChronoCat

## 警告:当前Red与RedProtocolSharp均处于开发阶段,未来结构可能产生较大变化,请做好随时变更代码的准备

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

- [ ] 发送语音消息

- [ ] 撤回消息

  ---
  
- [x] 获取Bot自身消息

- [x] 获取Bot好友列表(Bug)

- [x] 获取Bot所在某群群员列表(Bug)

  ---
  
- [x] 踢人

- [x] 禁言人

- [x] 全体禁言

### 已知问题/feature

1. 发送者解析不包含用户权限信息
1. GetMemberList返回空数组(打开UI可能可以解决问题)

### 安装

Nuget搜索RedProtocolSharp安装

或

引入Release下的Dll,并自行安装WebSocketSharp,NewtonSoft.JSON相关依赖(不推荐)

### 示例

###### 启动Bot

```c#
var bot = new Bot("localhost:16530","yourToken");
bot.Start();
```

###### 注册Logger,MessageReceive订阅

```c#
bot.Logger.BotLog.OnLogger += BotLogOnHandler;
bot.MessageReceive.OnMessageReceived += MessageReceiveOnHandler;

private static void MessageReceiveOnHandler(MsgType.Payload payload)
{
}

private static void BotLogOnHandler(BotLogger.Levels levels, string content)
{
    Console.WriteLine($"{levels} : {content}");
}
```

###### 提取消息内容

```c#
if (payload is MsgType.Payload<MsgType.MessageRecv> data)
{
    string text = data.GetMsgText();
}
```

###### 发送消息

```c#
bool statue = await bot.Send
    .SetTarget("114514", BotSend.ChatType.Group)
    .AddAt("1919810")
    .AddText("123")
    .AddPic(@"D:\pic.jpg")
    .SendMessage();
//向群114514发送内容为123,艾特用户1919810,附带图片pic.jpg的消息
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