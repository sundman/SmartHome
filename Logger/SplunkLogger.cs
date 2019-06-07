using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Splunk.Logging;

namespace Logger
{
    public class SplunkLogger : ILogger
    {
        private Uri splunkServerUri;
        private string token = "e35460ee-bbe1-4347-9dac-292dc336627b";
        private HttpEventCollectorResendMiddleware middleware;
        private HttpEventCollectorSender sender;
        public SplunkLogger()
        {
            splunkServerUri = new Uri("http://splunk.sundman.at:8088");
            middleware = new HttpEventCollectorResendMiddleware(100);
            sender = new HttpEventCollectorSender(splunkServerUri, token, null, HttpEventCollectorSender.SendMode.Parallel, 0, 0, 0, middleware.Plugin, Formatter);
            sender.OnError += onError;
           
        }

        private object Formatter(HttpEventCollectorEventInfo eventInfo)
        {
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            settings.Converters.Add(new StringEnumConverter());

            return JsonConvert.SerializeObject(eventInfo.Event.Data, settings);
        }


        private void onError(HttpEventCollectorException exception)
        {
            Console.WriteLine(exception.Message);
        }

        public void Info(object source, ILoggable loggable)
        {
           Log(source, loggable, LogLevel.Info);
        }

        public void Warn(object source, ILoggable loggable)
        {
            Log(source, loggable, LogLevel.Warn);
        }

        public void Error(object source, ILoggable loggable)
        {
            Log(source, loggable, LogLevel.Error);
        }

        public void FlushLogs()
        {
            sender.FlushSync();
        }

        private Guid RequestGuid { get; }


        private static string GetLoggingContext(object sender)
        {
            var loggingContext = "null";

            if (sender != null)
            {
                if (sender is Type type)
                {
                    loggingContext = type.FullName;
                }
                else
                {
                    loggingContext = sender.GetType().FullName;
                }
            }
            return loggingContext;
        }


        private void Log(object source, ILoggable loggable, LogLevel level)
        {

            var wrapper = new SplunkLogObject
            {
                Data = loggable,
                LogContext = RequestGuid,
                Source = GetLoggingContext(source),
                Level = level,
                Application = Assembly.GetEntryAssembly()?.GetName().Name,
                Machine = Environment.MachineName,
                TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                Event = loggable.GetType().Name

            };

            sender.Send(Guid.NewGuid().ToString(), level.ToString(), null, wrapper,null );
        }
    }

    public class SplunkLogObject
    {
        public string TimeStamp { get; set; }
        public Guid LogContext { get; set; }
        public LogLevel Level { get; set; }
        public string Machine { get; set; }
        public string Application { get; set; }
        public string Source { get; set; }
        public string Event { get; set; }
        public object Data { get; set; }
    }

    public enum LogLevel
    {
        Info,
        Warn,
        Error,
        Debug
    }
}
