using System;
using System.Threading;

namespace Silverlight.SimpleLogger
{
    /// <summary>
    /// Log entry for Silverlight logger
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Initializes log entty
        /// </summary>
        public LogEntry()
        {
            When = DateTime.Now;
            ThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// Message to be logged
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Exception occured, if any
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Additional parameters for formatted log messages
        /// </summary>
        public object[] Parameters { get; set; }

        /// <summary>
        /// Thread identifier
        /// </summary>
        public int ThreadId { get; set; }

        /// <summary>
        /// Date and time for log entry
        /// </summary>
        public DateTime When { get; set; }

        /// <summary>
        /// Provides string representation of log entry
        /// </summary>
        /// <returns>String to be put in log file</returns>
        public override string ToString()
        {
            string fullMessage;
            if(Exception != null)
            {
                fullMessage = string.Format("{0} \n{1}", Message, Exception);
            }
            else
                if (Parameters != null)
                {
                    fullMessage = string.Format(Message, Parameters);
                }
                else
                {
                    fullMessage = Message;
                }

            return string.Format("{0:yyyy-MM-dd HH:mm:ss.fff} [{2:D3}] {1}", When, fullMessage, ThreadId);
        }
    }
}
