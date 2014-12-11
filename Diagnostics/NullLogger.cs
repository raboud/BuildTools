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
    public class NullLogger : BaseLogger
    {
        public override bool IsDebugEnabled
        {
            get
            {
                return false;
            }
        }

        public override bool IsErrorEnabled
        {
            get
            {
                return false;
            }
        }

        public override bool IsFatalEnabled
        {
            get
            {
                return false;
            }
        }

        public override bool IsInfoEnabled
        {
            get
            {
                return false;
            }
        }

        public override bool IsWarnEnabled
        {
            get
            {
                return false;
            }
        }
    }
}
