using System;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace RandREng.MsBuildTasks
{
    public class XmlInsertAfterTask : XmlElementTask
    {
        public XmlInsertAfterTask()
        {
            this.XmlOperation = xmlOperation.InsertAfter;
        }
    }
}
