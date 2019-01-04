using Amdocs.Ginger.Common;
using GingerCoreNET.ReporterLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.PlugInsLib
{
    public class PluginProcessWrapper
    {

        string mPuginId;
        string mServiceID;
        readonly System.Diagnostics.Process mProcess;

        public PluginProcessWrapper(string pluginId, string serviceID, System.Diagnostics.Process process)
        {
            mPuginId = pluginId;
            mServiceID = serviceID;
            mProcess = process;
        }

        public string PlugId { get { return mPuginId; } }
        public string ServiceId { get { return mServiceID; } }

        public int Id { get { return mProcess.Id; } }

        public string MainWindowTitle { get { return mProcess.MainWindowTitle; } }

        public long PrivateMemorySize { get { return mProcess.WorkingSet64; } }

        public TimeSpan TotalProcessorTime { get { return mProcess.TotalProcessorTime; } }

        internal void Close()
        {
            try
            {
                mProcess.CloseMainWindow();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.INFO, "Failed to close window", ex);
               
            }
        }
    }
}
