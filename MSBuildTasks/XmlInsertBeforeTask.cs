using System;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace RandREng.MsBuildTasks
{
    public class XmlInsertBeforeTask : XmlElementTask
    {
        public XmlInsertBeforeTask()
        {
            this.XmlOperation = xmlOperation.InsertBefore;
        }
    }
}
