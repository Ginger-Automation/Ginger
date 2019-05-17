using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.Drivers;
using GingerCore.Platforms;
using GingerCore.Platforms.PlatformsInfo;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for MainAddActionsNavigationPage.xaml
    /// </summary>
    public partial class MainAddActionsNavigationPage : Page
    {
        string TargetApplication { get; set; }
        Context mContext;
        IWindowExplorer mWindowExplorerDriver;
        RecordNavPage RecordPage = null;
        public bool IsAgentStarted { get; set; }

        public MainAddActionsNavigationPage(Context context, string targetApplication)
        {
            mContext = context;
            TargetApplication = targetApplication;
            InitializeComponent();
            xNavigationBarPnl.Visibility = Visibility.Collapsed;
            xSelectedItemFrame.ContentRendered += NavPnlActionFrame_ContentRendered;
            IsAgentStarted = false;
        }

        private void NavPnlActionFrame_ContentRendered(object sender, EventArgs e)
        {
            if ((sender as Frame).Content == null)
            {
                (sender as Frame).Visibility = Visibility.Collapsed;
                xNavigationBarPnl.Visibility = Visibility.Collapsed;
                xAddActionsOptionsPnl.Visibility = Visibility.Visible;
            }
            else
            {
                (sender as Frame).Visibility = Visibility.Visible;
                xNavigationBarPnl.Visibility = Visibility.Visible;
                xAddActionsOptionsPnl.Visibility = Visibility.Collapsed;
                //navBarTitle.Content = (navPnlActionFrame.Content as Page).Title;
            }
        }

        private void XNavSharedRepo_Click(object sender, RoutedEventArgs e)
        {
            //LoadActionFrame(new RepositoryPage(mContext.BusinessFlow));
            LoadActionFrame(new SharedRepositoryNavPage(mContext), "Shared Repository", eImageType.SharedRepositoryItem); // WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Act>()));
        }

        private void XNavPOM_Click(object sender, RoutedEventArgs e)
        {
            ApplicationPOMsTreeItem POMsRoot = new ApplicationPOMsTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>());
            LoadActionFrame(new POMNavPage("Page Objects Models", eImageType.Application, POMsRoot, POMsRoot.SaveAllTreeFolderItemsHandler, POMsRoot.AddPOM), "Page Objects Model", eImageType.ApplicationPOMModel);
        }

        private void XRecord_Click(object sender, RoutedEventArgs e)
        {
            if(mWindowExplorerDriver == null)
            {
                StartAgent();
            }
            RecordPage = new RecordNavPage(mContext, mWindowExplorerDriver);
            LoadActionFrame(RecordPage, "Record", eImageType.Camera);
        }

        private void XNavActLib_Click(object sender, RoutedEventArgs e)
        {
            LoadActionFrame(new ActionsLibraryNavPage(mContext), "Actions Library", eImageType.Action);
        }

        private void XNavSpy_Click(object sender, RoutedEventArgs e)
        {
            LoadActionFrame(new LiveSpyNavPage(mContext), "Live Spy", eImageType.Spy);
        }

        private void XNavWinExp_Click(object sender, RoutedEventArgs e)
        {
            LoadActionFrame(new WindowsExplorerNavPage(mContext), "Windows Explorer", eImageType.Search);
            ListViewItem lvi = new ListViewItem();
        }

        private void xGoBackBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadActionFrame(null);
        }

        private void LoadActionFrame(Page navigationPage, string titleText = "", eImageType titleImage = eImageType.Empty)
        {            
            xSelectedItemFrame.Content = navigationPage;

            if (navigationPage != null)
            {
                xSelectedItemTitlePnl.Visibility = Visibility.Visible;
                xSelectedItemTitleImage.ImageType = titleImage;
                xSelectedItemTitleText.Content = titleText;
            }
            else
            {
                xSelectedItemTitlePnl.Visibility = Visibility.Collapsed;
            }
        }

        public void StartAgent()
        {
            IsAgentStarted = false;
            xRecordItemBtn.IsEnabled = false;
            if (mContext == null)
                return;

            @AppAgentAct:
            Activity mActParentActivity = mContext.BusinessFlow.CurrentActivity;
            if (string.IsNullOrEmpty(mActParentActivity.TargetApplication))
            {
                if (mContext.BusinessFlow.TargetApplications.Count() == 1)
                {
                    mActParentActivity.TargetApplication = ((ApplicationAgent)mContext.Runner.ApplicationAgents[0]).AppName; 
                }
            }            
            ApplicationAgent appAgent = (ApplicationAgent)mContext.Runner.ApplicationAgents.Where(x => x.AppName == mActParentActivity.TargetApplication).FirstOrDefault();

            if (appAgent == null)
            {
                mContext.Runner.SolutionAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
                mContext.Runner.UpdateApplicationAgents();
                goto AppAgentAct;
            }

            PlatformInfoBase platform = PlatformInfoBase.GetPlatformImpl(appAgent.Agent.Platform);

            ObservableList<DataSourceBase> dSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            if (appAgent != null)
            {
                if (appAgent.Agent.Driver == null)
                {
                    appAgent.Agent.DSList = dSList;
                    appAgent.Agent.StartDriver();
                    xRecordItemBtn.IsEnabled = true;
                    IsAgentStarted = true;
                }
                else if (!appAgent.Agent.Driver.IsRunning())
                {
                    if (Reporter.ToUser(eUserMsgKey.PleaseStartAgent, eUserMsgOption.OKCancel, eUserMsgSelection.OK) == eUserMsgSelection.OK)
                    {
                        appAgent.Agent.StartDriver();
                        xRecordItemBtn.IsEnabled = true;
                        IsAgentStarted = true;
                    }
                    else
                    {
                        IsAgentStarted = false;
                        return;
                    }
                }
                DriverBase driver = appAgent.Agent.Driver;
                if (driver is IWindowExplorer)
                {
                    mWindowExplorerDriver = (IWindowExplorer)appAgent.Agent.Driver;
                }
            }
            else
            {
                IsAgentStarted = false;
            }         
        }

        public void StopRecording()
        {
            if(RecordPage != null && RecordPage.IsRecording)
            {
                RecordPage.StopRecording();
            }
        }
    }
}
