# RedProtocolSharp

--基于RedProtocol的C# SDK,应用于某猫

## 警告:当前RedProtocol与RedProtocolSharp均处于开发阶段,未来结构可能产生较大变化,请做好随时变更代码的准备
**我们强烈建议您升级至0.1.0,该版本弃用了直接链式调用发送消息在高并发的情况下可能会导致消息元素重叠的Bug,并添加了对应的ChainBuilder作为新的消息发送方式**

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

- [x] 发送语音消息 无需安装其他外置库,感谢[silk-rs](https://github.com/lz1998/silk-rs)提供的dll,语音仅支持mp3格式,直接填写filePath即可

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
2. 现在不会,未来也不可能支持发送Json,xml等消息,也不可能支持主动添加群/好友

### 安装

Nuget搜索RedProtocolSharp安装

或

引入Release下的Dll,并自行安装WebSocketSharp,NewtonSoft.JSON,NAudio相关依赖(不推荐)

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
    Console.WriteLine(e.chain.PeerUin);
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

**原链式调用的方法已弃用!改为Builder创建chain发送消息!**

```c#
await bot.Send.SendMessageChain(new MessageBuilder()
.SetTarget("114514", ChatTypes.GroupMessage)
.AddText("test")
.Build());
//请使用Builder创建chain对象,不要也不能直接new MessageChain
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

控制台项目RedProtocolSharp.Sample