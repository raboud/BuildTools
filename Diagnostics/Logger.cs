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
	public class Logger
	{
		#region ILog Members

		static public void Debug(ILog logger, object message)
		{
			if (logger != null)
			{
				logger.Debug(message);
			}
		}

		static public void Debug(ILog logger, string message, Exception exception)
		{
			if (logger != null)
			{
				logger.Debug(message, exception);
			}
		}

		static public void Debug(ILog logger, string format, params object[] args)
		{
			if (logger != null)
			{
				logger.Debug(format, args);
			}
		}

		static public void Debug(ILog logger, IFormatProvider provider, string format, params object[] args)
		{
			if (logger != null)
			{
				logger.Debug(provider, format, args);
			}
		}

		static public void Info(ILog logger, object message)
		{
			if (logger != null)
			{
				logger.Info(message);
			}
		}

		static public void Info(ILog logger, string message, Exception exception)
		{
			if (logger != null)
			{
				logger.Info(message, exception);
			}
		}

		static public void Info(ILog logger, string format, params object[] args)
		{
			if (logger != null)
			{
				logger.Info(format, args);
			}
		}

		static public void Info(ILog logger, IFormatProvider provider, string format, params object[] args)
		{
			if (logger != null)
			{
				logger.Info(provider, format, args);
			}
		}

		static public void Warn(ILog logger, object message)
		{
			if (logger != null)
			{
				logger.Warn(message);
			}
		}

		static public void Warn(ILog logger, string message, Exception exception)
		{
			if (logger != null)
			{
				logger.Warn(message, exception);
			}
		}

		static public void Warn(ILog logger, string format, params object[] args)
		{
			if (logger != null)
			{
				logger.Warn(format, args);
			}
		}

		static public void Warn(ILog logger, IFormatProvider provider, string format, params object[] args)
		{
			if (logger != null)
			{
				logger.Warn(provider, format, args);
			}
		}

		static public void Error(ILog logger, object message)
		{
			if (logger != null)
			{
				logger.Error(message);
			}
		}

		static public void Error(ILog logger, string message, Exception exception)
		{
			if (logger != null)
			{
				logger.Error(message, exception);
			}
		}

		static public void Error(ILog logger, string format, params object[] args)
		{
			if (logger != null)
			{
				logger.Error(format, args);
			}
		}

		static public void Error(ILog logger, IFormatProvider provider, string format, params object[] args)
		{
			if (logger != null)
			{
				logger.Error(provider, format, args);
			}
		}

		static public void Fatal(ILog logger, object message)
		{
			if (logger != null)
			{
				logger.Fatal(message);
			}
		}

		static public void Fatal(ILog logger, string message, Exception exception)
		{
			if (logger != null)
			{
				logger.Fatal(message, exception);
			}
		}

		static public void Fatal(ILog logger, string format, params object[] args)
		{
			if (logger != null)
			{
				logger.Fatal(format, args);
			}
		}

		static public void Fatal(ILog logger, IFormatProvider provider, string format, params object[] args)
		{
			if (logger != null)
			{
				logger.Fatal(provider, format, args);
			}
		}

		static public bool IsDebugEnabled(ILog logger)
		{
			if (logger != null)
			{
				return logger.IsDebugEnabled;
			}
			return false;
		}

		static public bool IsInfoEnabled(ILog logger)
		{
			if (logger != null)
			{
				return logger.IsInfoEnabled;
			}
			return false;
		}

		static public bool IsWarnEnabled(ILog logger)
		{
			if (logger != null)
			{
				return logger.IsWarnEnabled;
			}
			return false;
		}

		static public bool IsErrorEnabled(ILog logger)
		{
			if (logger != null)
			{
				return logger.IsErrorEnabled;
			}
			return false;
		}

		static public bool IsFatalEnabled(ILog logger)
		{
			if (logger != null)
			{
				return logger.IsFatalEnabled;
			}
			return false;
		}

		#endregion
	}
}
