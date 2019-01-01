using System;
using System.Collections.Generic;
using System.Text;
using GingerCore.Activities;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public enum eActivitiesGroupRunStatus
    {
        Pending,
        Running,
        Passed,
        Failed,
        Stopped,
        Blocked,
        Skipped
    }
    public enum executionLoggerStatus
    {
        NotStartedYet,
        StartedNotFinishedYet,
        Finished
    }

}
