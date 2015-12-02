using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NLog.Targets;
using NLog.Config;
using System.Reflection;
using System.IO;

namespace Relational.Octapus.Persistence
{
    public class OctapusLog 
    {

        private Logger logger;
        private string loggerName;
        private IHookOctapusLogger hookLogger;

        public OctapusLog(string loggerName, IHookOctapusLogger hookLogger = null)
        {
            this.loggerName = loggerName;
            this.logger = LogManager.GetLogger(loggerName);
            this.hookLogger = hookLogger;
        }

        
        public Logger GetLogger()
        {
            return this.logger ;

        }
        public void LogWarning(string message)
        {
            HookCall("Warning", message);
            this.logger.Warn(message);
        }
        public void LogError(string message)
        {
            HookCall("Error", message);
            this.logger.Error(message);
        }
        public void LogTrace(string message)
        {
            HookCall("Trace", message);
            this.logger.Trace(message);
        }
        public  void LogInfo(string message)
        {
            HookCall("Info", message);
            this.logger.Info(message + Environment.NewLine);
        }
        public void LogFatal(string message)
        {
            HookCall("Fatal", message);
            this.logger.Fatal(message);
        }

        public void SetLogTarget()
        {
            
        }

        private void HookCall(string type, string message)
        {
            if (hookLogger != null)
                hookLogger.LogMessage(loggerName, type, message + " - " + DateTime.Now);
        }

    }
}
