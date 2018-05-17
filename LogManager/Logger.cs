using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogManager {
    public enum LogType {
        Error,
        Warning,
        Info
    }

    public class Logger {
        #region Singleton
        private static Logger Instance { get; set; }
        public static Logger GetInstance() {
            if (Instance == null) {
                Instance = new Logger();
            }

            return Instance;
        }
        #endregion

        private string LogFilePath { get; set; }
        private bool IsLogging { get; set; }

        private Logger() {
            if ((this.IsLogging = AppController.GetInstance().GetConfigvalue<bool>("isLogging"))) {
                this.LogFilePath = AppController.GetInstance().GetConfigvalue<string>("logFile");
            }
        }

        public void LogInfo(string content) {
            Log(content, LogType.Info);
        }

        public void LogWarning(string content) {
            Log(content, LogType.Warning);
        }

        public void LogError(string content) {
            Log(content, LogType.Error);
        }

        private void Log(string content, LogType type) {
            if (this.IsLogging && type == LogType.Info) {
                StringBuilder sb = new StringBuilder();
                sb.Append("[");
                sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.Append("]");

                if (type == LogType.Error) {
                    sb.Append("[ERROR]");
                } else if (type == LogType.Warning) {
                    sb.Append("[WARNING]");
                }
                sb.Append(content);
                sb.Append(System.Environment.NewLine);

                File.AppendAllText(this.LogFilePath, sb.ToString());
            }
        }
    }
}
