namespace RedProtocolSharp;

public class BotAction
{
    private Bot bot;

    public BotAction(Bot sender)
    {
        bot = sender;
    }
    public async Task<string> MuteEveryOne(string groupUin, bool enablement)
    {
        var content = $"{{\n    \"group\": {groupUin},\n    \"enable\": {enablement}\n}}";
        var reply = await bot.httpPostRequest(content, "group/muteEveryone");
        return reply;
    }

    //禁言所有人
    public async Task<string> Kick(string groupUin, string userUin, bool refuseForever, string reason)
    {
        var content =
            $"{{\n    \"uidList\": [{userUin}],\n    \"group\": {groupUin}, \n    \"refuseForever\": {refuseForever}, \n    \"reason\": \"{reason}\"\n}}";
        var reply = await bot.httpPostRequest(content, "group/kick");
        return reply;
    }

    //踢人
    public async Task<string> MuteMember(string groupUin, string userUin, int seconds)
    {
        var content =
            $"{{\n  \"group\": {groupUin},\n  \"memList\":\n  [\n      {{\n          \"uin\": \"{userUin}\",\n          \"timeStamp\": \"{seconds}\"\n      }}\n  ]\n}}";
        var reply = await bot.httpPostRequest(content, "group/muteMember");
        return reply;
    }

    //禁言指定人
}