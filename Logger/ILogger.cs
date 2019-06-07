namespace Logger
{
    public interface ILogger
    {
        void Info(object source, ILoggable loggable);
        void Warn(object source, ILoggable loggable);
        void Error(object source, ILoggable loggable);
        void FlushLogs();
    }
}