using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.GeneralLib;
using System;
using System.Reflection;

namespace Amdocs.Ginger.CoreNET.TelemetryLib
{
    class TelemetrySession
    {
        public Guid Guid { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Elapsed { get; set; }
        public int BusinessFlowsCounter { get; set; }
        public int ActivitiesCounter { get; set; }
        public int ActionsCounter { get; set; }
        public string TimeZone { get; set; }
        public string version { get; set; }
        public string Runtime { get; set; }

        public bool Debugger { get; set; }
        public bool Is64BitProcess { get; set; }
        public string OSVersion { get; set; }
        public bool dox { get; set; }
        public string Terminology { get; set; }
        public string exe { get; set; }

        public TelemetrySession(Guid guid)
        {
            Guid = guid;
            StartTime = Telemetry.Time;
            TimeZone = TimeZoneInfo.Local.DisplayName;
            version = ApplicationInfo.ApplicationVersion;

#if DEBUG
            Runtime = "Debug";
#else
            Runtime = "Release";
#endif

            Debugger = System.Diagnostics.Debugger.IsAttached;
            Is64BitProcess = Environment.Is64BitProcess;
            OSVersion = Environment.OSVersion.ToString();

            if (System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName == "corp.amdocs.com")
            {
                dox = true;
            }
            else
            {
                dox = false;
            }

            if (WorkSpace.Instance.UserProfile != null)
            {
                Terminology = WorkSpace.Instance.UserProfile.TerminologyDictionaryType.ToString();
            }

            Assembly assembly = Assembly.GetEntryAssembly();
            if (assembly ==null)
            {
                //running from unit tests
                exe = "Unit Test";
            }
            else
            {
                exe = assembly.GetName().Name;
            }
            
        }

    }
}
