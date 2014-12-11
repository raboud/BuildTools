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
    public class BaseLogger : ILog
    {
		LoggerLevelSwitch loggerSwitch;
		public BaseLogger()
		{
		}

		public BaseLogger(LoggingLevel level)
		{
			this.loggerSwitch = new LoggerLevelSwitch(level);
		}

		#region ILog Members

        virtual public void Debug(object message)
        {
            if (this.IsDebugEnabled)
            {
                this.WriteMessage(message);
            }
        }

        virtual public void Debug(string message, Exception exception)
        {
            if (this.IsDebugEnabled)
            {
                this.WriteMessage(message, exception);
            }
        }

        virtual public void Debug(string format, params object[] args)
        {
            if (this.IsDebugEnabled)
            {
                this.WriteMessage(format, args) ;
            }
        }

        virtual public void Debug(IFormatProvider provider, string format, params object[] args)
        {
            if (this.IsDebugEnabled)
            {
                this.WriteMessage(provider, format, args) ;
            }
        }

        virtual public void Info(object message)
        {
            if (this.IsInfoEnabled)
            {
                this.WriteMessage(message);
            }
        }

        virtual public void Info(string message, Exception exception)
        {
            if (this.IsInfoEnabled)
            {
                this.WriteMessage(message, exception);
            }
        }

        virtual public void Info(string format, params object[] args)
        {
            if (this.IsInfoEnabled)
            {
                this.WriteMessage(format, args);
            }
        }

        virtual public void Info(IFormatProvider provider, string format, params object[] args)
        {
            if (this.IsInfoEnabled)
            {
                this.WriteMessage(provider, format, args);
            }
        }

        virtual public void Warn(object message)
        {
            this.WriteMessage(message);
            {
                this.WriteMessage(message);
            }
        }

        virtual public void Warn(string message, Exception exception)
        {
            if (this.IsWarnEnabled)
            {
                this.WriteMessage(message, exception);
            }
        }

        virtual public void Warn(string format, params object[] args)
        {
            if (this.IsWarnEnabled)
            {
                this.WriteMessage(format, args);
            }
        }

        virtual public void Warn(IFormatProvider provider, string format, params object[] args)
        {
            if (this.IsWarnEnabled)
            {
                this.WriteMessage(provider, format, args);
            }
        }

        virtual public void Error(object message)
        {
            if (this.IsErrorEnabled)
            {
                this.WriteMessage(message);
            }
        }

        virtual public void Error(string message, Exception exception)
        {
            if (this.IsErrorEnabled)
            {
                this.WriteMessage(message, exception);
            }
        }

        virtual public void Error(string format, params object[] args)
        {
            if (this.IsErrorEnabled)
            {
                this.WriteMessage(format, args);
            }
        }

        virtual public void Error(IFormatProvider provider, string format, params object[] args)
        {
            if (this.IsErrorEnabled)
            {
                this.WriteMessage(provider, format, args);
            }
        }

        virtual public void Fatal(object message)
        {
            if (this.IsFatalEnabled)
            {
                this.WriteMessage(message);
            }
        }

        virtual public void Fatal(string message, Exception exception)
        {
            if (this.IsFatalEnabled)
            {
                this.WriteMessage(message, exception);
            }
        }

        virtual public void Fatal(string format, params object[] args)
        {
            if (this.IsFatalEnabled)
            {
                this.WriteMessage(format, args);
            }
        }

        virtual public void Fatal(IFormatProvider provider, string format, params object[] args)
        {
            if (this.IsFatalEnabled)
            {
                this.WriteMessage(provider, format, args);
            }
        }

        virtual public bool IsDebugEnabled
        {
			get 
			{ 
				if (this.loggerSwitch != null)
				{
					return this.loggerSwitch.LogDebug;
				}
				throw new NotImplementedException();
			}
        }

        virtual public bool IsInfoEnabled
        {
			get 
			{ 
				if (this.loggerSwitch != null)
				{
					return this.loggerSwitch.LogInfo;
				}
				throw new NotImplementedException();
			}
        }

        virtual public bool IsWarnEnabled
        {
			get 
			{ 
				if (this.loggerSwitch != null)
				{
					return this.loggerSwitch.LogWarning;
				}
				throw new NotImplementedException();
			}
        }

        virtual public bool IsErrorEnabled
        {
			get 
			{ 
				if (this.loggerSwitch != null)
				{
					return this.loggerSwitch.LogError;
				}
				throw new NotImplementedException();
			}
        }

        virtual public bool IsFatalEnabled
        {
			get 
			{ 
				if (this.loggerSwitch != null)
				{
					return this.loggerSwitch.LogFatal;
				}
				throw new NotImplementedException();
			}
        }

        #endregion

        virtual protected void WriteMessage(string message, Exception exception)
        {
            throw new NotImplementedException();
        }

        virtual protected void WriteMessage(object message)
        {
            throw new NotImplementedException();
        }

        virtual protected void WriteMessage(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        virtual protected void WriteMessage(IFormatProvider provider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

		virtual protected string FormatMessage(string message, Exception exception)
		{
			return "";
		}

		virtual protected string FormatMessage(object message)
		{
			return string.Format("{0}", message);
		}

		virtual protected string FormatMessage(string format, params object[] args)
		{
			return string.Format(format, args);
		}

		virtual protected string FormatMessage(IFormatProvider provider, string format, params object[] args)
		{
			return string.Format(provider, format, args);
		}
	}
}
