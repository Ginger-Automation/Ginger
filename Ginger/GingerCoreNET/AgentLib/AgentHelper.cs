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
        public static ApplicationAgent GetAppAgent(Activity activity, GingerRunner runner, Context context)
        {
            ApplicationAgent appAgent = null;
            if (context != null && activity != null)
            {
                appAgent = (ApplicationAgent)runner.ApplicationAgents.Where(x => x.AppName == activity.TargetApplication).FirstOrDefault();                
            }
            return appAgent;
        }

        /// <summary>
        /// This method is used to check if the agent is running or not
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool CheckIfAgentIsRunning(Activity activity, GingerRunner runner, Context context, out IWindowExplorer windowExplorerDriver)
        {
            bool isRunning = false;
            windowExplorerDriver = null;
            ApplicationAgent appAgent = GetAppAgent(activity, runner, context);
            if (appAgent != null && appAgent.Agent.Driver != null && appAgent.Agent.Driver.IsRunning())
            {
                windowExplorerDriver = (IWindowExplorer)appAgent.Agent.Driver;
                isRunning = true;
            }
            return isRunning;
        }

        /// <summary>
        /// This method is used to check if the agent is running or not
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Agent GetDriverAgent(Activity activity, GingerRunner runner, Context context)
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
        public static bool StartAgent(Activity activity, GingerRunner runner, Context context, out IWindowExplorer windowExplorerDriver)
        {
            bool isAgentStarted = false;
            windowExplorerDriver = null;
            ApplicationAgent appAgent = GetAppAgent(activity, runner, context);
            if (appAgent != null)
            {
                if (appAgent.Agent.Driver == null)
                {
                    appAgent.Agent.StartDriver();
                    isAgentStarted = true;
                }
                else if (!appAgent.Agent.Driver.IsRunning())
                {
                    if (Reporter.ToUser(eUserMsgKey.PleaseStartAgent, eUserMsgOption.OKCancel, eUserMsgSelection.OK) == eUserMsgSelection.OK)
                    {
                        appAgent.Agent.StartDriver();
                        isAgentStarted = true;
                    }
                    else
                    {
                        isAgentStarted = false;
                    }
                }
                DriverBase driver = appAgent.Agent.Driver;
                if (driver is IWindowExplorer)
                {
                    windowExplorerDriver = (IWindowExplorer)appAgent.Agent.Driver;
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
