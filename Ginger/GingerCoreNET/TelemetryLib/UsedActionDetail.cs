using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Execution;
using GingerCore.Actions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.TelemetryLib
{
    public class UsedActionDetail
    {
        public string Name { get; set; }
        [DefaultValue(0)]
        public int Total { get; set; } = 0;
        [DefaultValue(0)]
        public int Passed { get; set; } = 0;
        [DefaultValue(0)]
        public int Failed { get; set; } = 0;
        [Setting, DefaultValue(default(HashSet<string>))]
        public HashSet<string> Errors { get; set; }

        public UsedActionDetail(string name, int countTotal, string error=null)
        {
            Name = name;
            Total = countTotal;
            if (!string.IsNullOrEmpty(error))
            {
                if (Errors == null)
                {
                    Errors = new HashSet<string>();
                }
                Errors.Add(error);
            }     
        }

        public static void AddOrModifyActionDetail(Act action)
        {
            UsedActionDetail usedActionDetail = WorkSpace.Instance.Telemetry.TelemetrySession.ExecutedActionTypes.Where(x => x.Name == action.ActionType).FirstOrDefault();
            if (usedActionDetail != null)
            {
                int index = WorkSpace.Instance.Telemetry.TelemetrySession.ExecutedActionTypes.IndexOf(usedActionDetail);
                usedActionDetail.Total += 1;
                if (action.Status == eRunStatus.Passed)
                {
                    WorkSpace.Instance.Telemetry.TelemetrySession.PassedActionsCount += 1;
                    usedActionDetail.Passed += 1;
                }
                if (action.Status == eRunStatus.Failed)
                {
                    WorkSpace.Instance.Telemetry.TelemetrySession.FailedActionsCount += 1;
                    usedActionDetail.Failed += 1;
                }
                if (!string.IsNullOrEmpty(action.Error))
                {
                    usedActionDetail.Errors.Add(action.Error);
                }
                WorkSpace.Instance.Telemetry.TelemetrySession.ExecutedActionTypes[index] = usedActionDetail;

            }
            else
            {
                usedActionDetail = new UsedActionDetail(action.ActionType, 1, action.Error);

                if (action.Status == eRunStatus.Passed)
                {
                    WorkSpace.Instance.Telemetry.TelemetrySession.PassedActionsCount += 1;
                    usedActionDetail.Passed += 1;
                }
                if (action.Status == eRunStatus.Failed)
                {
                    WorkSpace.Instance.Telemetry.TelemetrySession.FailedActionsCount += 1;
                    usedActionDetail.Failed += 1;
                }
                WorkSpace.Instance.Telemetry.TelemetrySession.ExecutedActionTypes.Add(usedActionDetail);
            }
        }
    }
}
