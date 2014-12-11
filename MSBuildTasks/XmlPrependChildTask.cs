using System;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace RandREng.MsBuildTasks
{
    public class XmlPrependChildTask : XmlElementTask
    {
        public XmlPrependChildTask()
        {
            this.XmlOperation = xmlOperation.PrependChild;
        }
    }
}
