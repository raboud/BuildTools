using System;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace RandREng.MsBuildTasks
{
    public class XmlAppendChildTask : XmlElementTask
    {
        public XmlAppendChildTask()
        {
            this.XmlOperation = xmlOperation.AppendChild;
        }
    }
}
