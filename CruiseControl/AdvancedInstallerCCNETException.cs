using System;
using System.Collections.Generic;
using System.Text;

namespace ThoughtWorks.CruiseControl.Core.Tasks
{
    public class AdvancedInstallerCCNETException : BuilderException
    {
        public AdvancedInstallerCCNETException(ITask runner, string message)
            : base(runner, message)
        {

        }

        public AdvancedInstallerCCNETException(ITask runner, string message, Exception innerException)
            : base(runner, message, innerException)
        {

        }
    }
}
