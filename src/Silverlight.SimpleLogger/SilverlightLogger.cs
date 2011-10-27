using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace Silverlight.SimpleLogger
{
    /// <summary>
    /// Simple logger for Silverlight (currently writes into file in isolated storage)
    /// </summary>
    public class SilverlightLogger
    {
        /// <summary>
        /// Logging level
        /// </summary>
        private static int _level;

        /// <summary>
        /// File name for log
        /// </summary>
        private static string _logFileName;

        /// <summary>
        /// File stream for isolated storage
        /// </summary>     
        private static IsolatedStorageFileStream _storageFileStream;

        /// <summary>
        /// Stream writer for log file
        /// </summary>
        private static StreamWriter _streamWriter;

        /// <summary>
        /// Closes log file...just in case
        /// </summary>
        ~SilverlightLogger()
        {
            if (_storageFileStream != null)
            {
                _storageFileStream.Close();
            }
        }

        /// <summary>
        /// Configures Silverlight logger.
        /// Call this method when Silverlight application initializes.
        /// </summary>
        /// <remarks>
        /// You can find the log file on a client PC in isolated storage which is usually somewhere inside c:\Users\username\AppData\LocalLow\Microsoft\Silverlight\ folder
        /// </remarks>
        /// <param name="level">Desired log level (see <see cref="SilverlightLogLevel"/>)</param>
        /// <param name="fileName">File name for log.</param>
        public static void Configure(int level, string fileName)
        {
            _level = level;
            _logFileName = fileName;
        }

        /// <summary>
        /// Writes informational message into log
        /// </summary>
        /// <param name="message">Possibly formatted message</param>
        /// <param name="parameters">Formatted parameters</param>
        public static void Info(string message, params object[] parameters)
        {
            if (SilverlightLogLevel.Info < _level) return;

            WriteToLog(String.Format(message, parameters));
        }

        /// <summary>
        /// Writes error message into log
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="exception">Exception occured</param>
        public static void Error(string message, Exception exception)
        {
            if (SilverlightLogLevel.Error < _level) return;

            var fullMessage = String.Format("{0} \n{1}", message, exception);
            WriteToLog(fullMessage);
        }

        /// <summary>
        /// Writes formatted error message into log
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="parameters">Format parameters list</param>        
        public static void ErrorFormat(string message, params object[] parameters)
        {
            if (SilverlightLogLevel.Error < _level) return;

            var fullMessage = String.Format(message, parameters);
            WriteToLog(fullMessage);
        }


        /// <summary>
        /// Opens logs file
        /// </summary>        
        /// <param name="fileName">Log file name</param>
        private static void OpenLog(string fileName)
        {
            try
            {
                var _storageFile = IsolatedStorageFile.GetUserStoreForApplication();
                _storageFileStream = _storageFile.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                _streamWriter = new StreamWriter(_storageFileStream);
                _streamWriter.AutoFlush = true;
            }
            catch
            {
                // we fail silently because logging trouble should not affect the end users
            }
        }

        /// <summary>
        /// Writes message into log file
        /// </summary>
        /// <param name="message">Message to be written</param>
        private static void WriteToLog(string message)
        {
            try
            {               
                if (_streamWriter == null)
                {
                    OpenLog(_logFileName);
                }

                if (_streamWriter != null)
                {
                    _streamWriter.WriteLine(String.Format("{0:yyyy-dd-MM hh:mm:ss.fff} {1}", DateTime.Now, message));
                }
            }
            catch
            {
                // we fail silently because logging trouble should not affect the end users
            }
        }
    }
}