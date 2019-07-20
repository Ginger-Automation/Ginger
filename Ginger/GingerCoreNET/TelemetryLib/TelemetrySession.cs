using Amdocs.Ginger.Common.GeneralLib;
using System;

namespace Amdocs.Ginger.CoreNET.TelemetryLib
{
    class TelemetrySession
    {
        public Guid Guid {get; set;}
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Elapsed { get; set; }
        public int BusinessFlowsCounter { get; set; }
        public int ActivitiesCounter { get; set; }
        public int ActionsCounter { get; set; }
        public string TimeZone { get; set; }
        public string version { get; set; }

        public TelemetrySession(Guid guid)
        {
            Guid = guid;
            StartTime = Telemetry.Time;
            TimeZone = TimeZoneInfo.Local.DisplayName;
            version = ApplicationInfo.ApplicationVersion;
        }
            
    }
}
