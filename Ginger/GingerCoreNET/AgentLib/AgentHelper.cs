using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Ginger.Run;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.Drivers;
using GingerCore.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCoreNET
{
    public class AgentHelper
    {
        /// <summary>
        /// This methods sets the default target applciation to acitvity
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static Activity SetDefaultTargetApplication(Activity activity, Context context)
        {
            if(activity != null)
            {
                activity.TargetApplication = ((ApplicationAgent)context.Runner.ApplicationAgents[0]).AppName;
            }
            return activity;
        }

        /// <summary>
        /// This method is used to get the app agent
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="runner"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static ApplicationAgent GetAppAgent(Activity activity, GingerRunner runner, Context context)
        {
            ApplicationAgent appAgent = null;
            if (context != null && context.BusinessFlow.CurrentActivity != null)
            {
                @AppAgentAct:
                if (string.IsNullOrEmpty(activity.TargetApplication))
                {
                    if (context.BusinessFlow.TargetApplications.Count() == 1)
                    {
                        activity = SetDefaultTargetApplication(activity, context); 
                    }
                }
                appAgent = (ApplicationAgent)runner.ApplicationAgents.Where(x => x.AppName == activity.TargetApplication).FirstOrDefault();
                if (appAgent == null)
                {
                    context.Runner.SolutionAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
                    context.Runner.UpdateApplicationAgents();
                    goto AppAgentAct;
                }
                else
                {
                    ObservableList<DataSourceBase> dSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
                    if (appAgent != null)
                    {
                        if (appAgent.Agent.Driver == null)
                        {
                            appAgent.Agent.DSList = dSList;
                        }
                    }
                }
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
