namespace RedProtocolSharp;

public class BotLogger
{
    public enum Levels
    {
        Debug,
        Info,
        Warn,
        Error
    }
    public class BotLogQueue : EventArgs
    {
        public delegate void BotLogHandler(Levels levels, string content);
        public event BotLogHandler OnLogger;

        public void LoggedBot(Levels levels, string content)
        {
            OnLogger?.Invoke(levels,content);
        }
    }
    public BotLogQueue BotLog = new();
    public void Debug(string content)
    {
        BotLog.LoggedBot(Levels.Debug,content);
    }
    public void Info(string content)
    {
        BotLog.LoggedBot(Levels.Info,content);
    }
    public void Warn(string content)
    {
        BotLog.LoggedBot(Levels.Warn,content);
    }
    public void Error(string content)
    {
        BotLog.LoggedBot(Levels.Error,content);
    }
}