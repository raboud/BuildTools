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
    // The following are possible values for the new switch.
    public enum LoggingLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Fatal = 4,
		Off = 5,
	}


    class LoggerLevelSwitch : System.Diagnostics.Switch
    {
        protected LoggingLevel level;

        public LoggerLevelSwitch(string displayName, string description)
            : base(displayName, description)
        {
        }

		public LoggerLevelSwitch(LoggingLevel level)
			: base("", "")
		{
			this.level = level;
		}

        public LoggingLevel Level
        {
            get
            {
                return level;
            }
            set
            {
                level = value;
            }
        }

        new protected int SwitchSetting
        {
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                if (value > 5)
                {
                    value = 5;
                }

                level = (LoggingLevel)value;
            }
            get
            {
                return (int)this.Level;
            }
        }


        public bool LogDebug
        {
            get { return (int) this.level <= (int) LoggingLevel.Debug; }
        }

        public bool LogInfo
        {
            get { return (int)this.level <= (int)LoggingLevel.Info; }
        }

        public bool LogWarning
        {
            get { return (int) this.level <= (int)LoggingLevel.Warning; }
        }

        public bool LogError
        {
            get { return (int)this.level <= (int)LoggingLevel.Error; }
        }

        public bool LogFatal
        {
            get { return (int)this.level <= (int)LoggingLevel.Fatal; }
        }
    }

}
