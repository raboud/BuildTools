using System;
using System.Collections.Generic;
using System.Text;

namespace ThoughtWorks.CruiseControl.Core.Tasks
{
    public class AdvancedInstallerException : Exception
    {
        public AdvancedInstallerException(string aipFile) : base()
        {
            this.advancedInstallerProjectFile = aipFile;
        }

        public AdvancedInstallerException(string aipFile, string message) : base(message)
        {
            this.advancedInstallerProjectFile = aipFile;
        }

        string advancedInstallerProjectFile;
        public string AdvancedInstallerProjectFile
        {
            get { return advancedInstallerProjectFile; }
            set { advancedInstallerProjectFile = value; }
        }
    }
}
