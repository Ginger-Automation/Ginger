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
using Amdocs.Ginger.Common.UIElement;
using Ginger.Run;
using GingerCore;
using GingerCore.Drivers;
using GingerCore.Platforms;
using System.Linq;

namespace GingerCoreNET
{
    public class AgentHelper
    {
        /// <summary>
        /// This method is used to get the app agent
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="runner"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ApplicationAgent GetAppAgent(Activity activity, GingerExecutionEngine runner, Context context)
        {
            ApplicationAgent appAgent = null;
            if (context != null && activity != null)
            {
                appAgent = (ApplicationAgent)runner.GingerRunner.ApplicationAgents.Where(x => x.AppName == activity.TargetApplication).FirstOrDefault();                
            }
            return appAgent;
        }

        /// <summary>
        /// This method is used to check if the agent is running or not
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool CheckIfAgentIsRunning(Activity activity, GingerExecutionEngine runner, Context context, out IWindowExplorer windowExplorerDriver)
        {
            bool isRunning = false;
            windowExplorerDriver = null;
            ApplicationAgent appAgent = GetAppAgent(activity, runner, context);
            if (appAgent != null && appAgent.Agent != null && ((AgentOperations)appAgent.Agent.AgentOperations).Driver != null && ((AgentOperations)appAgent.Agent.AgentOperations).Driver.IsRunning())
            {
                if (((AgentOperations)appAgent.Agent.AgentOperations).Driver is IWindowExplorer)
                {
                    windowExplorerDriver = (IWindowExplorer)((AgentOperations)appAgent.Agent.AgentOperations).Driver;
                }
                isRunning = true;
            }
            return isRunning;
        }

        /// <summary>
        /// This method is used to check if the agent is running or not
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Agent GetDriverAgent(Activity activity, GingerExecutionEngine runner, Context context)
        {
            Agent agent = null;
            ApplicationAgent appAgent = GetAppAgent(activity, runner, context);
            if (appAgent != null && appAgent.Agent != null)
            {
                agent = appAgent.Agent;
            }
            return agent;
        }

        /// <summary>
        /// This method is used to Start the agent
        /// </summary>
        public static bool StartAgent(Activity activity, GingerExecutionEngine runner, Context context, out IWindowExplorer windowExplorerDriver)
        {
            bool isAgentStarted = false;
            windowExplorerDriver = null;
            ApplicationAgent appAgent = GetAppAgent(activity, runner, context);
            if (appAgent != null)
            {
                if (((AgentOperations)appAgent.Agent.AgentOperations).Driver == null)
                {
                    appAgent.Agent.AgentOperations.StartDriver();
                    isAgentStarted = true;
                }
                else if (!((AgentOperations)appAgent.Agent.AgentOperations).Driver.IsRunning())
                {
                    if (Reporter.ToUser(eUserMsgKey.PleaseStartAgent, eUserMsgOption.OKCancel, eUserMsgSelection.OK) == eUserMsgSelection.OK)
                    {
                        appAgent.Agent.AgentOperations.StartDriver();
                        isAgentStarted = true;
                    }
                    else
                    {
                        isAgentStarted = false;
                    }
                }
                DriverBase driver = ((AgentOperations)appAgent.Agent.AgentOperations).Driver;
                if (driver is IWindowExplorer)
                {
                    windowExplorerDriver = (IWindowExplorer)((AgentOperations)appAgent.Agent.AgentOperations).Driver;
                }
            }
            else
            {
                isAgentStarted = false;
            }
            return isAgentStarted;
        }
    }
}
