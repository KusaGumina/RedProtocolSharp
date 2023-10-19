using Newtonsoft.Json;

namespace RedProtocolSharp.Oprator;

public class BotGet
{
    private Bot bot;
    public BotGet(Bot sender)
    {
        bot = sender;
    }
    public async Task<MsgType.InfoUser> SelfInfo()
    {
        var info = new MsgType.InfoUser();
        var reply = await bot.httpGetRequest("getSelfProfile");
        if (reply != "")
        {
            info = JsonConvert.DeserializeObject<MsgType.InfoUser>(reply);
        }
        return info;
    }
    //获取Bot自身信息
    public async Task<List<MsgType.InfoGroup>> GroupInfo()
    {
        var info = new List<MsgType.InfoGroup>();
        var reply = await bot.httpGetRequest("bot/groups");
        if (reply != "")
        {
            info = JsonConvert.DeserializeObject<List<MsgType.InfoGroup>>(reply);
        }
        return info;
    }
    //获取Bot加入的群
    public async Task<List<MsgType.InfoGroupUser>> MemberList(string groupUin,int number)
    {
        var info = new List<MsgType.InfoGroupUser>();
        var content = $"{{\n    \"group\": {groupUin},\n    \"size\": {number}\n}}";
        var reply = await bot.httpPostRequest(content,"group/getMemberList");
        if (reply != "")
        {
            info = JsonConvert.DeserializeObject<List<MsgType.InfoGroupUser>>(reply);
        }
        return info;
    }
    //获取指定群群员信息
}