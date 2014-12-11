/*---------------------------------------------------------------------------
	Copyright 2011 - 2012
	R & R Engineering, LLC
	4291 Communication Dr
    Norcross, GA 30093
----------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;

namespace RandREng.Common.Diagnostics
{
    public class TraceLogger : BaseLogger
    {
        static LoggerLevelSwitch loggerSwitch = new LoggerLevelSwitch("TraceLoggerLevel", "Entire Application");

        public override bool IsDebugEnabled
        {
            get{ return TraceLogger.loggerSwitch.LogDebug; }
        }

        public override bool IsInfoEnabled
        {
            get { return TraceLogger.loggerSwitch.LogInfo; }
        }

        public override bool IsWarnEnabled
        {
            get { return TraceLogger.loggerSwitch.LogWarning; }
        }

        public override bool IsErrorEnabled
        {
            get { return TraceLogger.loggerSwitch.LogError; }
        }

        public override bool IsFatalEnabled
        {
            get { return TraceLogger.loggerSwitch.LogFatal; }
        }

        protected override void WriteMessage(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(this.FormatMessage(format, args)) ;
        }

        protected override void WriteMessage(IFormatProvider provider, string format, params object[] args)
        {
			System.Diagnostics.Trace.WriteLine(this.FormatMessage(provider, format, args));
        }

        protected override void WriteMessage(object message)
        {
			System.Diagnostics.Trace.WriteLine(this.FormatMessage(message));
        }

        protected override void WriteMessage(string message, Exception exception)
        {
			System.Diagnostics.Trace.WriteLine(this.FormatMessage(message, exception));
        }
    }
}
