using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Silverlight.SimpleLogger
{
    /// <summary>
    /// Simple logger for Silverlight (currently writes into file in isolated storage)
    /// </summary>
    public class SilverlightLogger
    {
        /// <summary>
        /// Background queue of logging operations
        /// </summary>
        private static readonly BackgroundActionsProcessor LogActions = new BackgroundActionsProcessor();

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
        /// True if log file is open
        /// </summary>
        private static bool _isLogFileOpen;

        /// <summary>
        /// Maximum log size
        /// </summary>
        private static int _maxLogSize;

        /// <summary>
        /// Minimum free space left in isolated storage
        /// </summary>
        private static int _minIsolatedStorageSpaceLeft;

        /// <summary>
        /// Size of log contents to keep when log exceeds max size and we shrink it
        /// </summary>
        private static int _previousLogSizeToKeep;

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
        /// Configures Silverlight logger
        /// </summary>
        /// <param name="level">Desired log level</param>
        /// <param name="fileName">File name for log</param>
        /// <param name="maxLogSize">Maximum log size</param>
        /// <param name="minIsolatedStorageSpaceLeft">Minimum isolated storage space left for other consumers</param>
        /// <param name="previousLogSizeToKeep">Size of log contents to keep when log exceeds max size and we shrink it</param>
        public static void Configure(int level, string fileName, int maxLogSize = 500000, int minIsolatedStorageSpaceLeft = 100000, int previousLogSizeToKeep = 20000)
        {
            _level = level;
            _logFileName = fileName;
            _maxLogSize = maxLogSize;
            _minIsolatedStorageSpaceLeft = minIsolatedStorageSpaceLeft;
            _previousLogSizeToKeep = previousLogSizeToKeep;

            if (_previousLogSizeToKeep >= _maxLogSize)
            {
                _previousLogSizeToKeep = _maxLogSize / 10;
            }

            if (_maxLogSize < 0)
            {
                _maxLogSize = 0;
            }
        }

        /// <summary>
        /// Writes informational message into log
        /// </summary>
        /// <param name="message">Possibly formatted message</param>
        /// <param name="parameters">Formatted parameters</param>
        public static void Info(string message, params object[] parameters)
        {
            if (SilverlightLogLevel.Info < _level) return;

            var logEntry = new LogEntry { Message = message, Parameters = parameters };
            LogActions.Enqueue(() => WriteToLog(logEntry));
        }

        /// <summary>
        /// Writes error message into log
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="exception">Exception occured</param>
        public static void Error(string message, Exception exception)
        {
            if (SilverlightLogLevel.Error < _level) return;

            var logEntry = new LogEntry { Message = message, Exception = exception };
            LogActions.Enqueue(() => WriteToLog(logEntry));
        }

        /// <summary>
        /// Writes formatted error message into log
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="parameters">Format parameters list</param>        
        public static void ErrorFormat(string message, params object[] parameters)
        {
            if (SilverlightLogLevel.Error < _level) return;

            var logEntry = new LogEntry { Message = message, Parameters = parameters };
            LogActions.Enqueue(() => WriteToLog(logEntry));
        }

        /// <summary>
        /// Shows log file in a separate child window on top
        /// </summary>
        public static void ShowLog()
        {
            var popup = new Popup();

            var text = new TextBlock();

            text.Width = 800;
            text.TextWrapping = TextWrapping.Wrap;

            try
            {
                text.Text = GetLogContents();
            }
            catch (Exception exception)
            {
                text.Text = string.Format("Cannot open log file, exception is\n {0}", exception.ToString());
            }
            

            var closeButton = new Button();
            closeButton.Content = "Close";
            closeButton.Click += (s, e) => popup.IsOpen = false;

            var scroll = new ScrollViewer();
            scroll.Background = new SolidColorBrush(Colors.White);
            scroll.Content = text;
            scroll.Height = 400;

            var content = new StackPanel();
            content.Children.Add(scroll);
            content.Children.Add(closeButton);

            popup.Child = content;
            popup.IsOpen = true;

            // scroll to the bottom
            scroll.UpdateLayout();
            scroll.ScrollToVerticalOffset(double.MaxValue);            
        }

        /// <summary>
        /// Retrieves full contents of a log file
        /// </summary>
        /// <returns>String with log file contents</returns>
        public static string GetLogContents()
        {
            var storageFile = IsolatedStorageFile.GetUserStoreForApplication();
            using (var fileStream = storageFile.OpenFile(_logFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var reader = new StreamReader(fileStream))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Opens logs file
        /// </summary>                
        private static void OpenLog()
        {
            try
            {
                var storageFile = GetIsolatedStorageFile();
                _storageFileStream = storageFile.OpenFile(_logFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
                _storageFileStream.Seek(0, SeekOrigin.End);
                _streamWriter = new StreamWriter(_storageFileStream);
                _streamWriter.AutoFlush = true;
                _isLogFileOpen = true;
            }
            catch
            {
                // we fail silently because logging trouble should not affect the end users
            }
        }

        /// <summary>
        /// Retrieves isolated storage file for logs
        /// </summary>
        /// <returns>Isolated storage file</returns>
        private static IsolatedStorageFile GetIsolatedStorageFile()
        {
            return IsolatedStorageFile.GetUserStoreForApplication();
        }

        /// <summary>
        /// Writes log entry into file
        /// </summary>
        /// <param name="entry">Log entry to be written</param>
        private static void WriteToLog(LogEntry entry)
        {
            try
            {
                if (!_isLogFileOpen)
                {
                    OpenLog();
                }

                if (_isLogFileOpen)
                {
                    if (_streamWriter != null)
                    {
                        var message = entry + Environment.NewLine;
                        lock (_streamWriter)
                        {
                            var allowedMessageLength = CheckLogSize(message);

                            if (allowedMessageLength > 0)
                            {
                                _streamWriter.Write(message.Substring(0, allowedMessageLength));
                            }
                        }
                    }
                }
            }
            catch
            {
                // we fail silently because logging trouble should not affect the end users
            }
        }

        /// <summary>
        /// Checks log size before adding new message and adjusts it if it exceeded the bounds
        /// </summary>
        /// <param name="newMessage">New message to be added to log</param>       
        /// <returns>Maximum number of characters in a message string to fit the log</returns>
        private static int CheckLogSize(string newMessage)
        {
            var storageFile = GetIsolatedStorageFile();
           
            // calculate new log size and allowed log size            
            var quota = storageFile.Quota;
            var freeSpaceAllowedLogSize = quota - _minIsolatedStorageSpaceLeft;
            var maxAllowedLogSize = (_maxLogSize > freeSpaceAllowedLogSize) ? freeSpaceAllowedLogSize : _maxLogSize;

            var messageLength = Encoding.UTF8.GetByteCount(newMessage);
            var newLength = _storageFileStream.Length + messageLength;

            // if new length would exceed maximum size, shrink the log
            if (newLength > maxAllowedLogSize)
            {                
                // take the last piece of log to save bit of context in the new log file
                var contentSizeToKeep = (_previousLogSizeToKeep + messageLength > maxAllowedLogSize)
                                            ? maxAllowedLogSize - messageLength
                                            : _previousLogSizeToKeep;

                byte[] contentsToKeep = null;
                if (contentSizeToKeep > 0)
                {
                    contentsToKeep = new byte[_previousLogSizeToKeep];
                    _storageFileStream.Seek(-contentSizeToKeep, SeekOrigin.End);
                    contentSizeToKeep = _storageFileStream.Read(contentsToKeep, 0, (int)contentSizeToKeep);
                }

                // shrink the log
                _storageFileStream.SetLength(0);
                _storageFileStream.Position = 0;

                // write the tail of the previous log
                if (contentsToKeep != null)
                {
                    _storageFileStream.Write(contentsToKeep, 0, (int)contentSizeToKeep);
                    _storageFileStream.Position = contentSizeToKeep;
                }
            }

            // calculate max message size to fit the log
            if (messageLength > maxAllowedLogSize)
            {
                int currentSize = 0;
                var chars = newMessage.ToCharArray();
                for (int i = 0; i < messageLength; i++)
                {
                    currentSize += Encoding.UTF8.GetByteCount(chars, i, 1);
                    if (currentSize > maxAllowedLogSize)
                    {
                        return i;
                    }
                }

                return messageLength;
            }
            else
            {
                return messageLength;
            }
        }
    }
}