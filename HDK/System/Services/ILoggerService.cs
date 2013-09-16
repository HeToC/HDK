using System;
using System.Collections;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Services
{
    public interface ILoggerService : IService
    {
        /// <summary>
        ///     Sets the severity 
        /// </summary>
        /// <param name="minimumLevel">Minimum level</param>
        void SetSeverity(LogSeverity minimumLevel);

        /// <summary>
        ///     Log with an exception
        /// </summary>
        /// <param name="severity">The severity</param>
        /// <param name="source">The source</param>
        /// <param name="message">additional message</param>
        /// <param name="exception">The exception</param>
        //void Log(LogSeverity severity, object source, string message, Exception exception);

        /// <summary>
        ///     Log with formatting
        /// </summary>
        /// <param name="severity">The severity</param>
        /// <param name="source">The source</param>
        /// <param name="messageTemplate">The message template</param>
        /// <param name="arguments">The lines to log</param>
        void Log(LogSeverity severity, object source, string messageTemplate, params object[] arguments);

    }

    /// <summary>
    ///     Severity for the log message
    /// </summary>
    public enum LogSeverity : short
    {
        Verbose = 0,
        Information = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }


    /// <summary>
    ///     The default (debug) logger
    /// </summary>
    [Shared]
    [ExportService("Default Logger Service", "description", typeof(ILoggerService))]
    public class DefaultLogger : ILoggerService
    {
        /// <summary>
        ///     Template for logged messages
        /// </summary>
        private const string TEMPLATE = "{0} {1} {2} :: {3}";

        private LogSeverity _severityLevel = LogSeverity.Verbose;

        public List<string> LogEntries { get; set; }


        public DefaultLogger()
        {
            LogEntries = new List<string>();
        }

        /// <summary>
        ///     Sets the severity 
        /// </summary>
        /// <param name="minimumLevel">Minimum level</param>
        public void SetSeverity(LogSeverity minimumLevel)
        {
            _severityLevel = minimumLevel;
        }

        ///// <summary>
        /////     Log with a message
        ///// </summary>
        ///// <param name="severity">The severity</param>
        ///// <param name="source">The source</param>
        ///// <param name="message">The message</param>
        //public void Log(LogSeverity severity, object source, string message)
        //{
        //    if (!Debugger.IsAttached || (int)severity < (int)_severityLevel)
        //    {
        //        return;
        //    }

        //    string logMessage = string.Format(TEMPLATE, DateTime.Now, severity, source, message);

        //    LogEntries.Add(logMessage);
        //    Debug.WriteLine(logMessage);
        //}

        /// <summary>
        ///     Log with an exception
        /// </summary>
        /// <param name="severity">The severity</param>
        /// <param name="source">The source</param>
        /// <param name="exception">The exception</param>
        //public void Log(LogSeverity severity, object source, string message, Exception exception)
        //{
        //    if (!Debugger.IsAttached || (int)severity < (int)_severityLevel)
        //    {
        //        return;
        //    }

        //    var sb = new StringBuilder();
        //    sb.Append(exception);

        //    var ex = exception.InnerException;

        //    while (ex != null)
        //    {
        //        sb.AppendFormat("{0}{1}", Environment.NewLine, ex);
        //        ex = ex.InnerException;
        //    }

        //    Log(severity, source, sb.ToString());
        //}

        /// <summary>
        ///     Log with formatting
        /// </summary>
        /// <param name="severity">The severity</param>
        /// <param name="source">The source</param>
        /// <param name="messageTemplate">The message template</param>
        /// <param name="arguments">The lines to log</param>
        public void Log(LogSeverity severity, object source, string messageTemplate, params object[] arguments)
        {
            if (!Debugger.IsAttached || (int)severity < (int)_severityLevel)
            {
                return;
            }


            string innerText = string.Empty;
            Exception lastPropExc = arguments.Last() as Exception;
            if (arguments != null && lastPropExc != null)
            {
                StringBuilder exceptionMessageBuilder = new StringBuilder();
                exceptionMessageBuilder.AppendFormat(messageTemplate, arguments.TakeWhile(o => !(o is Exception)).ToArray());
                exceptionMessageBuilder.AppendLine();
                foreach(DictionaryEntry excData in lastPropExc.Data)
                {
                    exceptionMessageBuilder.AppendFormat(" Exception.Data -> {0} = '{1}'", excData.Key, excData.Value);
                    exceptionMessageBuilder.AppendLine();
                }
                exceptionMessageBuilder.AppendLine();
                exceptionMessageBuilder.AppendLine(lastPropExc.ToString());

                innerText = exceptionMessageBuilder.ToString();
            }
            else
                innerText = string.Format(messageTemplate, arguments);

            string logMessage = string.Format(TEMPLATE, DateTime.Now, severity, source, innerText);

            LogEntries.Add(logMessage);
            Debug.WriteLine(logMessage);
        }

        public void StartService()
        {
        }

        public void StopService()
        {
        }

        public void Dispose()
        {
        }
    }
}
