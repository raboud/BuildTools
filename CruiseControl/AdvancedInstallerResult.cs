using System;
using System.Collections.Generic;
using System.Text;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.Core.Tasks
{
    public class AdvancedInstallerResult : ITaskResult
    {
        public AdvancedInstallerResult()
        {
            
        }               

        #region ITaskResult Members

        string data;
        public string Data
        {
            get { return data; }
            set { data = value; }
        }

        bool hasFailed = false;
        public bool HasFailed
        {
            get { return hasFailed; }
            set { hasFailed = value; }
        }

        public bool CheckIfSuccess()
        {
            return !hasFailed;
        }

        #endregion
    }
}
