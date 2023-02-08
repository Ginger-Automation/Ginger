#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

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

        public bool IsProcessExited { get { return mProcess != null ? mProcess.HasExited : false; } }
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
