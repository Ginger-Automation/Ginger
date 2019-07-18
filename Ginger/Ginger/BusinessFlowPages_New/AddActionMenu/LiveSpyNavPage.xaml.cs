using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Ginger.BusinessFlowPages;
using GingerCore;
using GingerCore.Platforms;
using GingerCoreNET;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for LiveSpyNavAction.xaml
    /// </summary>
    public partial class LiveSpyNavPage : Page
    {
        Context mContext;
        IWindowExplorer mWindowExplorerDriver;
        List<AgentPageMappingHelper> mLiveSpyPageDictonary = null;
        LiveSpyPage CurrentLoadedPage = null;

        public LiveSpyNavPage(Context context)
        {
            InitializeComponent();
            mContext = context;
            context.PropertyChanged += Context_PropertyChanged;
            LoadLiveSpyPage(mContext);
            SetFrameEnableDisable();
        }

        /// <summary>
        /// Context Property changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Context.AgentStatus))
            {
                SetFrameEnableDisable();
                CurrentLoadedPage.SetLiveSpyForNewPanel(mWindowExplorerDriver);
            }
            else if (e.PropertyName == nameof(Context.Agent) && mContext.Agent != null)
            {
                LoadLiveSpyPage(mContext);
                SetFrameEnableDisable();
                CurrentLoadedPage.SetLiveSpyForNewPanel(mWindowExplorerDriver);
            }
        }

        /// <summary>
        /// This method will check if agent is running then it will enable the frame
        /// </summary>
        private void SetFrameEnableDisable()
        {
            this.Dispatcher.Invoke(() =>
            {
                bool isAgentRunning = mContext.Agent.Status == Agent.eStatus.Running;                  //  AgentHelper.CheckIfAgentIsRunning(mContext.BusinessFlow.CurrentActivity, mContext.Runner, mContext, out mWindowExplorerDriver);
                if (mContext.Agent != null)
                    mWindowExplorerDriver = mContext.Agent.Driver as IWindowExplorer;

                if (isAgentRunning)
                {
                    xSelectedItemFrame.IsEnabled = true;
                }
                else
                {
                    xSelectedItemFrame.IsEnabled = false;
                }
            });
        }

        /// <summary>
        /// This method is used to get the new LiveSpyPage based on Context and Agent
        /// </summary>
        /// <returns></returns>
        private void LoadLiveSpyPage(Context context)
        {
            this.Dispatcher.Invoke(() =>
            {
                bool isLoaded = false;
                if (mLiveSpyPageDictonary != null && mLiveSpyPageDictonary.Count > 0 && context.Agent != null)
                {
                    AgentPageMappingHelper objHelper = mLiveSpyPageDictonary.Find(x => x.ObjectAgent.DriverType == context.Agent.DriverType &&
                                                                                    x.ObjectAgent.ItemName == context.Agent.ItemName);
                    if (objHelper != null && objHelper.ObjectWindowPage != null)
                    {
                        CurrentLoadedPage = (LiveSpyPage)objHelper.ObjectWindowPage;
                        isLoaded = true;
                    }
                }

                if (!isLoaded)
                {
                    ApplicationAgent appAgent = AgentHelper.GetAppAgent(mContext.BusinessFlow.CurrentActivity, mContext.Runner, mContext);
                    if (appAgent != null)
                    {
                        CurrentLoadedPage = new LiveSpyPage(mContext);
                        CurrentLoadedPage.SetLiveSpyForNewPanel(mWindowExplorerDriver);
                        if (mLiveSpyPageDictonary == null)
                        {
                            mLiveSpyPageDictonary = new List<AgentPageMappingHelper>();
                        }
                        mLiveSpyPageDictonary.Add(new AgentPageMappingHelper(context.Agent, CurrentLoadedPage));
                    }
                }

                xSelectedItemFrame.Content = CurrentLoadedPage;
            });
        }
    }
}
