using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

public enum eLogLevel
{
    Error,
    Warning,
    Info,
    Debug
}

namespace PluginsManagement.Helpers
{
    public class LogHelper : IDisposable
    {
        private string _logBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");//Log日志的默认路径

        private string _fileName;//Log文件名
        private eLogLevel _logLevel;//Log等级
        private Int32 _buffSize;//缓存大小
        private readonly Thread _logThread;//执行线程
        private readonly ConcurrentQueue<string> _logQueue; //自定义线程安全的Queue
        private string _currentDate;
        private string _currentFilePath;
        private bool _disposed;
        private long _maxLogFileSize = 1024 * 1024 * 10; // 10 MB
        private int _fileNumber = 0;
        private readonly object _lockObject = new object();

        /// <summary>
        /// 设置文档的保存路径,默认路径是程序路径\Logs\yyyy-MM-dd;
        /// </summary>
        /// <param name="logFolder"></param>
        private void SetLogFilePath(string logFolder) => _logBasePath = logFolder;

        /// <summary>
        /// 初始化Log日志
        /// </summary>
        /// <param name="FileName">文件名称，默认后缀为.log，不需要带后缀</param>
        /// <param name="logLevel">日志等级:Debug-Info-Warning-Error-Exception</param>
        /// <param name="BuffSize">缓存大小，默认1M</param>
        /// <param name="FilePath">文件目录：为空则为默认路径</param>
        /// <param name="MaxLogFileSize">文件大小：默认20M</param>
        public LogHelper(string FileName, eLogLevel logLevel = eLogLevel.Debug, int BuffSize = 1024, string FilePath = "", long MaxLogFileSize = 1024 * 1024 * 10)
        {
            _logQueue = new ConcurrentQueue<string>();

            if (!string.IsNullOrEmpty(FilePath))
                SetLogFilePath(FilePath);
            this._maxLogFileSize = MaxLogFileSize;
            this._fileName = FileName;
            this._logLevel = logLevel;
            this._buffSize = BuffSize;

            _currentDate = DateTime.Now.ToString("yyyyMMdd");

            _fileNumber = 0;

            while (File.Exists(GetNextFilePath()))// 保证在最后一个不足指定大小的文件上加追加
            {
                _fileNumber++;
                if (!File.Exists(GetNextFilePath()))
                {
                    _fileNumber--;
                    if (new FileInfo(GetNextFilePath()).Length >= _maxLogFileSize)
                    { 
                        _fileNumber++;
                        break;
                    }
                    else
                        break;
                }
            }

            _currentFilePath = GetNextFilePath();


            _logThread = new Thread(WriteLog);
            _logThread.IsBackground = true;//后台线程，程序关闭时线程同步关闭；
            _logThread.Start();
        }

        private string GetNextFilePath()
        {
            string folderPath = Path.Combine(_logBasePath, _currentDate);
            return Path.Combine(folderPath, $"{_fileName}_{_fileNumber}.log");
        }

        /// <summary>
        /// 写入队列
        /// </summary>
        private void WriteLog()
        {
            StringBuilder strBuilder = new StringBuilder();
            lock (_lockObject)
            {
                _currentDate = DateTime.Now.ToString("yyyyMMdd");
                _currentFilePath = GetNextFilePath();
            }
            CreateDirectory(Path.Combine(_logBasePath, _currentDate));//创建日志文件夹目录
            CreateFile(_currentFilePath);

            while (!_disposed)
            {
                if (_logQueue.TryDequeue(out string _msg))//取队列里的第一条
                {
                    strBuilder.Append(_msg);
                    if (strBuilder.Length >= _buffSize || _logQueue.IsEmpty)//日志数据长度达到_buffSize大小时先记录下，防止出现异常时丢失过多的数据
                    {
                        ProcessWriteLog(_currentFilePath, strBuilder.ToString()); //写入日志到文本
                        strBuilder.Clear();

                        // Check if the date has changed to create a new log file
                        string currentDate = DateTime.Now.ToString("yyyyMMdd");
                        lock (_lockObject)
                        {
                            if (currentDate != _currentDate)
                            {
                                _currentDate = currentDate;
                                _fileNumber = 0;
                                _currentFilePath = GetNextFilePath();
                                CreateDirectory(Path.Combine(_logBasePath, _currentDate));
                                CreateFile(_currentFilePath);
                            }
                            else if (new FileInfo(_currentFilePath).Length >= _maxLogFileSize)
                            {
                                _fileNumber++;
                                _currentFilePath = GetNextFilePath();
                                CreateFile(_currentFilePath);
                            }
                        }
                    }
                }
                else
                {
                    Thread.Sleep(100); // Sleep for a while if no log messages are available
                }
            }
        }

        /// <summary>
        /// 写入文件动作
        /// </summary>
        /// <param name="path">文件路径返回文件名</param>
        /// <param name="msg">写入内容</param>
        private void ProcessWriteLog(string path, string msg)
        {
            try
            {
                using (StreamWriter _sw = new StreamWriter(path, true, Encoding.UTF8))
                {
                    _sw.Write(msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write log to file: {ex.Message}");
                string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", $"ErrorLog_{DateTime.Now.ToString("yyyyMMdd")}.log");
                using (StreamWriter _sw = new StreamWriter(LogFilePath, true, Encoding.UTF8))
                {
                    _sw.Write(ex.ToString() + Environment.NewLine);
                    _sw.Write(msg);
                    _sw.Flush();
                }
            }
        }

        /// <summary>
        /// 创建日志文件夹
        /// </summary>
        /// <returns></returns>
        private bool CreateDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create directory: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 创建日志文件
        /// </summary>
        /// <param name="path">文件地址</param>
        /// <returns></returns>
        private bool CreateFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    using (File.Create(path)) { }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 记录日志，受日志等级限制
        /// </summary>
        /// <param name="msg">日志内容</param>
        /// <param name="type">日志类型</param>
        private void Log(string msg, eLogLevel type)
        {
            if (this._logLevel >= type && !_disposed)
            {
                string logMessage = $"{DateTime.Now.ToString("HH:mm:ss.fff")}|{type}|{msg}{Environment.NewLine}";
                _logQueue.Enqueue(logMessage);
                // _autoReset.Set();
            }
        }

        /// <summary>
        /// 记录异常日志，不受日志等级限制
        /// </summary>
        /// <param name="ex">异常</param>
        public void LogException(Exception ex)
        {
            if (ex != null)
            {
                StringBuilder _builder = new StringBuilder();
                _builder.AppendFormat($"{DateTime.Now.ToString("HH:mm:ss.fff")}|Exception|{ex.Message};{ex.GetType()};{ex.Source};{ex.TargetSite};{ex.StackTrace}{Environment.NewLine}");
                _logQueue.Enqueue(_builder.ToString());
                // _autoReset.Set();
            }
        }

        /// <summary>
        /// Error等级的日志
        /// </summary>
        /// <param name="msg"></param>
        public void LogError(string msg)
        {
            Log(msg, eLogLevel.Error);
        }

        /// <summary>
        /// Warning等级的日志
        /// </summary>
        /// <param name="msg"></param>
        public void LogWarn(string msg)
        {
            Log(msg, eLogLevel.Warning);
        }

        /// <summary>
        /// Info等级的日志
        /// </summary>
        /// <param name="msg"></param>
        public void LogInfo(string msg)
        {
            Log(msg, eLogLevel.Info);
        }

        /// <summary>
        /// Debug等级的日志
        /// </summary>
        /// <param name="msg"></param>
        public void LogDebug(string msg)
        {
            Log(msg, eLogLevel.Debug);
        }

        public void Dispose()
        {
            _disposed = true;
            //_autoReset.Set();
            _logThread.Join();
        }
    }
}