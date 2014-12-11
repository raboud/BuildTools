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
	public class ConsoleLogger : BaseLogger
	{
		public ConsoleLogger(LoggingLevel level)
			: base(level)
		{
		}

		protected override void WriteMessage(IFormatProvider provider, string format, params object[] args)
		{
			System.Console.WriteLine(this.FormatMessage(provider, format, args));
		}

		protected override void WriteMessage(object message)
		{
			System.Console.WriteLine(this.FormatMessage(message));
		}

		protected override void WriteMessage(string message, Exception exception)
		{
			System.Console.WriteLine(this.FormatMessage(message, exception));
		}

		protected override void WriteMessage(string format, params object[] args)
		{
			System.Console.WriteLine(this.FormatMessage(format, args));
		}
	}
}
