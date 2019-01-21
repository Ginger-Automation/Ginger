using Amdocs.Ginger.Common;
using System;

namespace Amdocs.Ginger.CoreNET.PlugInsLib
{
    public class PluginProcessWrapper
    {

        string mPluginId;
        string mServiceID;
        readonly System.Diagnostics.Process mProcess;

        public PluginProcessWrapper(string pluginId, string serviceID, System.Diagnostics.Process process)
        {
            mPluginId = pluginId;
            mServiceID = serviceID;
            mProcess = process;
        }

        public string PlugId { get { return mPluginId; } }
        public string ServiceId { get { return mServiceID; } }

        public int Id { get { return mProcess.Id; } }

        public string MainWindowTitle { get
            {
                if (mProcess.HasExited)
                {
                    return "process exited";
                }
                
                return mProcess.MainWindowTitle;                
            }
        }

        public long PrivateMemorySize
        {
            get
            {
                if (mProcess.HasExited)
                {
                    return 0;
                }

                return mProcess.WorkingSet64;
            }
        }

        public TimeSpan TotalProcessorTime { get { return mProcess.TotalProcessorTime; } }

        internal void Close()
        {
            try
            {
                mProcess.CloseMainWindow();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, "Failed to close window", ex);
               
            }
        }
    }
}
