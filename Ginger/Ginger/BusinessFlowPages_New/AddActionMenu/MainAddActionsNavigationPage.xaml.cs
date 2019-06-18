using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using Ginger.Agents;
using Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.Drivers;
using GingerCore.Platforms;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.TreeViewItemsLib.ApplicationModelsTreeItems;
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
        Context mContext;
        RecordNavPage mRecordPage = null;
        SharedRepositoryNavPage mSharedRepositoryNavPage = null;
        POMNavPage mPOMNavPage = null;
        ActionsLibraryNavPage mActionsLibraryNavPage = null;
        LiveSpyNavPage mLiveSpyNavPage = null;
        WindowsExplorerNavPage mWindowsExplorerNavPage = null;
        APINavPage mAPINavPage = null;
        private bool applicationModelView;
        private Agent mAgent = null;

        public MainAddActionsNavigationPage(Context context)
        {
            mContext = context;
            InitializeComponent();
            context.PropertyChanged += Context_PropertyChanged;
            xNavigationBarPnl.Visibility = Visibility.Collapsed;
            xSelectedItemFrame.ContentRendered += NavPnlActionFrame_ContentRendered;
            SetRecordButtonAccessebility();
        }

        private void SetRecordButtonAccessebility()
        {
            mAgent = AgentHelper.GetDriverAgent(mContext.BusinessFlow.CurrentActivity, mContext.Runner, mContext);
            if (mAgent != null && (mAgent.IsSupportRecording() || mAgent.Driver is IRecord))
            {
                xRecordItemBtn.IsEnabled = true;
            }
            else
            {
                xRecordItemBtn.IsEnabled = false;
            }
        }

        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e != null && (e.PropertyName == nameof(BusinessFlow) || e.PropertyName == nameof(Activity)))
            {
                SetRecordButtonAccessebility();
                LoadActionFrame(null);
            }
        }

        private void NavPnlActionFrame_ContentRendered(object sender, EventArgs e)
        {
            if ((sender as Frame).Content == null)
            {
                if(applicationModelView)
                {
                    (sender as Frame).Visibility = Visibility.Collapsed;
                    xNavigationBarPnl.Visibility = Visibility.Visible;
                    xAddActionsOptionsPnl.Visibility = Visibility.Collapsed;
                    xApplicationModelsPnl.Visibility = Visibility.Visible;
                }
                else
                {
                    (sender as Frame).Visibility = Visibility.Collapsed;
                    xNavigationBarPnl.Visibility = Visibility.Collapsed;
                    xAddActionsOptionsPnl.Visibility = Visibility.Visible;
                    xApplicationModelsPnl.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                (sender as Frame).Visibility = Visibility.Visible;
                xNavigationBarPnl.Visibility = Visibility.Visible;
                xAddActionsOptionsPnl.Visibility = Visibility.Collapsed;
                xApplicationModelsPnl.Visibility = Visibility.Collapsed;
            }
        }

        private void XNavSharedRepo_Click(object sender, RoutedEventArgs e)
        {
            if(mSharedRepositoryNavPage == null)
            {
                mSharedRepositoryNavPage = new SharedRepositoryNavPage(mContext);
            }
            LoadActionFrame(mSharedRepositoryNavPage, "Shared Repository", eImageType.SharedRepositoryItem); // WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Act>()));
        }

        private void XNavPOM_Click(object sender, RoutedEventArgs e)
        {
            ApplicationPOMsTreeItem POMsRoot = new ApplicationPOMsTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>());
            if(mPOMNavPage == null)
            {
                mPOMNavPage = new POMNavPage(mContext,"Page Objects Models", eImageType.Application, POMsRoot, POMsRoot.SaveAllTreeFolderItemsHandler, POMsRoot.AddPOM);
                mPOMNavPage.SetAgent(mAgent);
            }
            LoadActionFrame(mPOMNavPage, "Page Objects Model", eImageType.ApplicationPOMModel);
        }

        private void XRecord_Click(object sender, RoutedEventArgs e)
        {
            if (mRecordPage == null)
            {
                mRecordPage = new RecordNavPage(mContext);
            }

            LoadActionFrame(mRecordPage, "Record", eImageType.Camera);
        }

        private void XNavActLib_Click(object sender, RoutedEventArgs e)
        {
            if(mActionsLibraryNavPage == null)
            {
                mActionsLibraryNavPage = new ActionsLibraryNavPage(mContext);
            }
            LoadActionFrame(mActionsLibraryNavPage, "Actions Library", eImageType.Action);
        }

        private void XNavSpy_Click(object sender, RoutedEventArgs e)
        {
            if (mLiveSpyNavPage == null)
            {
                mLiveSpyNavPage = new LiveSpyNavPage(mContext);
            }
            LoadActionFrame(mLiveSpyNavPage, "Live Spy", eImageType.Spy);
        }

        private void XNavWinExp_Click(object sender, RoutedEventArgs e)
        {
            if (mWindowsExplorerNavPage == null)
            {
                mWindowsExplorerNavPage = new WindowsExplorerNavPage(mContext);
            }
            LoadActionFrame(mWindowsExplorerNavPage, "Windows Explorer", eImageType.Search);
        }

        private void XAPIBtn_Click(object sender, RoutedEventArgs e)
        {
            if(mAPINavPage == null)
            {
                AppApiModelsFolderTreeItem apiRoot = new AppApiModelsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationAPIModel>());
                mAPINavPage = new APINavPage(mContext, "API Models", eImageType.APIModel, apiRoot, apiRoot.SaveAllTreeFolderItemsHandler, apiRoot.AddAPIModelFromDocument);
            }
            LoadActionFrame(mAPINavPage, "API Models", eImageType.APIModel);
        }

        private void xGoBackBtn_Click(object sender, RoutedEventArgs e)
        {
            if(xSelectedItemFrame.Content is APINavPage || xSelectedItemFrame.Content is POMNavPage)
            {
                applicationModelView = true;
                LoadActionFrame(null, "Application Models", eImageType.ApplicationModel);
            }
            else if(xSelectedItemFrame.Content is null)
            {
                applicationModelView = false;
                xNavigationBarPnl.Visibility = Visibility.Collapsed;
                xAddActionsOptionsPnl.Visibility = Visibility.Visible;
                xApplicationModelsPnl.Visibility = Visibility.Collapsed;
            }
            else
                LoadActionFrame(null);
        }

        private void LoadActionFrame(Page navigationPage, string titleText = "", eImageType titleImage = eImageType.Empty)
        {
            xSelectedItemFrame.Content = navigationPage;

            if (navigationPage != null || titleImage is eImageType.ApplicationModel)
            {
                xNavigationBarPnl.Visibility = Visibility.Visible;
                xSelectedItemTitlePnl.Visibility = Visibility.Visible;
                xSelectedItemTitleImage.ImageType = titleImage;
                xSelectedItemTitleText.Content = titleText;
            }
            else
            {
                xSelectedItemTitlePnl.Visibility = Visibility.Collapsed;
            }
        }

        private void XApplicationModelsBtn_Click(object sender, RoutedEventArgs e)
        {
            xApplicationModelsPnl.Visibility = Visibility.Visible;
            xAddActionsOptionsPnl.Visibility = Visibility.Collapsed;
            applicationModelView = true;

            LoadActionFrame(null, "Application Models", eImageType.ApplicationModel);
        }
    }
}
