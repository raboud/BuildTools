/*---------------------------------------------------------------------------
	Copyright 2011 - 2012
	R & R Engineering, LLC
	4291 Communication Dr
    Norcross, GA 30093
----------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RandREng.Common.Diagnostics
{
    public class FileLogger: BaseLogger
	{
        string fileName;

        public FileLogger(LoggingLevel level, string fileName, bool append)
			: base(level)
		{
            this.fileName = fileName;
            if (!append)
            {
                System.IO.File.Delete(fileName);
            }

		}

		protected override void WriteMessage(IFormatProvider provider, string format, params object[] args)
		{
            using (TextWriter sw = new StreamWriter(this.fileName, true))
            {
                sw.WriteLine(this.FormatMessage(provider, format, args));
            }
		}

		protected override void WriteMessage(object message)
		{
            using (TextWriter sw = new StreamWriter(this.fileName, true))
            {
                sw.WriteLine(this.FormatMessage(message));
            }
		}

		protected override void WriteMessage(string message, Exception exception)
		{
            using (TextWriter sw = new StreamWriter(this.fileName, true))
            {
                sw.WriteLine(this.FormatMessage(message, exception));
            }
		}

		protected override void WriteMessage(string format, params object[] args)
		{
            using (TextWriter sw = new StreamWriter(this.fileName, true))
            {
                sw.WriteLine(this.FormatMessage(format, args));
            }
		}
	}
}
