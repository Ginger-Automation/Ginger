using System;
using System.Collections.Generic;
using System.Text;

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

    public interface IActivitiesGroup
    {
        Dictionary<Guid, DateTime> ExecutedActivities { get; set; }
        Guid Guid { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string AutomationPrecentage { get;  }
        DateTime StartTimeStamp { get; set; }
        DateTime EndTimeStamp { get; set; }
        Single? ElapsedSecs { get; set; }
        eActivitiesGroupRunStatus RunStatus { get; set; }
        string ExternalID { get; set; }
    }
}
