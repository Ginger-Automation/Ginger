using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.BusinessFlowPages_New;
using Ginger.Drivers.Common;
using Ginger.Drivers.PowerBuilder;
using Ginger.UserControls;
using Ginger.WindowExplorer;
using Ginger.WindowExplorer.Appium;
using Ginger.WindowExplorer.HTMLCommon;
using Ginger.WindowExplorer.Java;
using Ginger.WindowExplorer.Windows;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.UIAutomation;
using GingerCore.Drivers.Appium;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Drivers.PBDriver;
using GingerCore.Platforms;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.UserControlsLib.UCTreeView;

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
            LoadWindowExplorerPage(mContext);
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
                CurrentLoadedPage.SetWindowExplorerForNewPanel(mWindowExplorerDriver);
            }
            else if (e.PropertyName == nameof(Context.Agent))
            {
                LoadWindowExplorerPage(mContext);
                SetFrameEnableDisable();
                CurrentLoadedPage.SetWindowExplorerForNewPanel(mWindowExplorerDriver);
            }
        }

        /// <summary>
        /// This method will check if agent is running then it will enable the frame
        /// </summary>
        private void SetFrameEnableDisable()
        {
            bool isAgentRunning = AgentHelper.CheckIfAgentIsRunning(mContext.BusinessFlow.CurrentActivity, mContext.Runner, mContext, out mWindowExplorerDriver);
            if (isAgentRunning)
            {
                xSelectedItemFrame.IsEnabled = true;
            }
            else
            {
                xSelectedItemFrame.IsEnabled = false;
            }
        }
        
        /// <summary>
        /// This method is used to get the new WindowExplorerPage based on Context and Agent
        /// </summary>
        /// <returns></returns>
        private void LoadWindowExplorerPage(Context context)
        {
            bool isLoaded = false;
            if (mLiveSpyPageDictonary != null && mLiveSpyPageDictonary.Count > 0 && context.Agent != null)
            {
                AgentPageMappingHelper objHelper = mLiveSpyPageDictonary.Where(x => x.ObjectAgent.DriverType == context.Agent.DriverType &&
                                                                                x.ObjectAgent.ItemName == context.Agent.ItemName).FirstOrDefault();
                if (objHelper != null && objHelper.ObjectWindowPage != null)
                {
                    CurrentLoadedPage = (LiveSpyPage)objHelper.ObjectWindowPage;
                    isLoaded = true;
                }
            }

            if (!isLoaded)
            {
                ApplicationAgent appAgent = AgentHelper.GetAppAgent(context.BusinessFlow.CurrentActivity, context.Runner, context);
                if (appAgent != null)
                {
                    CurrentLoadedPage = new LiveSpyPage(context);
                    CurrentLoadedPage.SetWindowExplorerForNewPanel(mWindowExplorerDriver);
                    if (mLiveSpyPageDictonary == null)
                    {
                        mLiveSpyPageDictonary = new List<AgentPageMappingHelper>();
                    }
                    mLiveSpyPageDictonary.Add(new AgentPageMappingHelper(context.Agent, CurrentLoadedPage));
                }
            }

            xSelectedItemFrame.Content = CurrentLoadedPage;
        }
    }
}
