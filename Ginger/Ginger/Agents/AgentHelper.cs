using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.Drivers;
using GingerCore.Platforms;
using GingerCore.Platforms.PlatformsInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.Agents
{
    public class AgentHelper
    {
        public IWindowExplorer WindowExplorerDriver;

        /// <summary>
        /// Default Constructor
        /// </summary>
        private AgentHelper()
        {
        }

        /// <summary>
        /// Single Instance of class
        /// </summary>
        private static AgentHelper mInstance;
        public static AgentHelper Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new AgentHelper();
                }
                return mInstance;
            }
        }

        /// <summary>
        /// This method is used to get the app agent
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private ApplicationAgent GetAppAgent(Context context)
        {
            ApplicationAgent appAgent = null;
            if (context != null && context.BusinessFlow.CurrentActivity != null)
            {
                @AppAgentAct:
                Activity mActParentActivity = context.BusinessFlow.CurrentActivity;
                if (string.IsNullOrEmpty(mActParentActivity.TargetApplication))
                {
                    if (context.BusinessFlow.TargetApplications.Count() == 1)
                    {
                        mActParentActivity.TargetApplication = ((ApplicationAgent)context.Runner.ApplicationAgents[0]).AppName;
                    }
                }
                appAgent = (ApplicationAgent)context.Runner.ApplicationAgents.Where(x => x.AppName == mActParentActivity.TargetApplication).FirstOrDefault();
                if (appAgent == null)
                {
                    context.Runner.SolutionAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
                    context.Runner.UpdateApplicationAgents();
                    goto AppAgentAct;
                }
                else
                {
                    PlatformInfoBase platform = PlatformInfoBase.GetPlatformImpl(appAgent.Agent.Platform);

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
        public bool CheckIfAgentIsRunning(Context context)
        {
            bool isRunning = false;
            ApplicationAgent appAgent = GetAppAgent(context);
            if (appAgent != null && appAgent.Agent.Driver != null && appAgent.Agent.Driver.IsRunning())
            {
                WindowExplorerDriver = (IWindowExplorer)appAgent.Agent.Driver;
                isRunning = true;
            }
            return isRunning;
        }

        /// <summary>
        /// This method is used to Start the agent
        /// </summary>
        public bool StartAgent(Context context)
        {
            bool isAgentStarted = false;
            ApplicationAgent appAgent = GetAppAgent(context);
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
                    WindowExplorerDriver = (IWindowExplorer)appAgent.Agent.Driver;
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
