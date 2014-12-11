using System;
using System.Collections.Generic;
using System.Text;

namespace RandREng.MsBuildTasks.Acceptance
{
    public enum TaskState
    {
        Complete,
        Running,
        Queued,
        Failed,
        LastRunPending,
        LastRunComplete,
        LastRunNever
    }
}
