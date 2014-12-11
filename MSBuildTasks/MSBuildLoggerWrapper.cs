using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Build.Utilities;

using RandREng.Common.Diagnostics;

namespace RandREng.MsBuildTasks
{
    public class MSBuildLoggerWrapper : ILog 
    {
        TaskLoggingHelper Log;
        MSBuildLoggerWrapper(TaskLoggingHelper log)
        {
            this.Log = log;
        }

        #region ILog Members

        public void Debug(object message)
        {
            Log.LogMessage(Microsoft.Build.Framework.MessageImportance.Low, message.ToString());
        }

        public void Debug(string message, Exception exception)
        {
            Log.LogMessage(Microsoft.Build.Framework.MessageImportance.Low, exception.ToString());
        }

        public void Debug(string format, params object[] args)
        {
            Log.LogMessage(Microsoft.Build.Framework.MessageImportance.Low, format, args);
        }

        public void Debug(IFormatProvider provider, string format, params object[] args)
        {
            Log.LogMessage(Microsoft.Build.Framework.MessageImportance.Low, format, args);
        }

        public void Info(object message)
        {
            Log.LogMessage(Microsoft.Build.Framework.MessageImportance.Normal, message.ToString());
        }

        public void Info(string message, Exception exception)
        {
            Log.LogMessage(Microsoft.Build.Framework.MessageImportance.Normal, exception.ToString());
        }

        public void Info(string format, params object[] args)
        {
            Log.LogMessage(Microsoft.Build.Framework.MessageImportance.Normal, format, args);
        }

        public void Info(IFormatProvider provider, string format, params object[] args)
        {
            Log.LogMessage(Microsoft.Build.Framework.MessageImportance.Normal, format, args);
        }

        public void Warn(object message)
        {
            Log.LogWarning(message.ToString());
        }

        public void Warn(string message, Exception exception)
        {
            Log.LogWarningFromException(exception, true);
        }

        public void Warn(string format, params object[] args)
        {
            Log.LogWarning(format, args);
        }

        public void Warn(IFormatProvider provider, string format, params object[] args)
        {
            Log.LogWarning(format, args);
        }

        public void Error(object message)
        {
            Log.LogError(message.ToString());
        }

        public void Error(string message, Exception exception)
        {
            Log.LogErrorFromException(exception, true);
        }

        public void Error(string format, params object[] args)
        {
            Log.LogError(format, args);
        }

        public void Error(IFormatProvider provider, string format, params object[] args)
        {
            Log.LogError(format, args);
        }

        public void Fatal(object message)
        {
            Log.LogError(message.ToString());
        }

        public void Fatal(string message, Exception exception)
        {
            Log.LogErrorFromException(exception, true);
        }

        public void Fatal(string format, params object[] args)
        {
            Log.LogError(format, args);
        }

        public void Fatal(IFormatProvider provider, string format, params object[] args)
        {
            Log.LogError(format, args);
        }

        public bool IsDebugEnabled
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsInfoEnabled
        {
            get { return true; }
        }

        public bool IsWarnEnabled
        {
            get { return true; }
        }

        public bool IsErrorEnabled
        {
            get { return true; }
        }

        public bool IsFatalEnabled
        {
            get { return true; }
        }

        #endregion
    }
}
