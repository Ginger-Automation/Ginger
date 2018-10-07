using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.BusinessFlowFolder;
using Ginger.SolutionWindows.TreeViewItems;
using GingerCore;
using GingerWPF.BusinessFlowsLib;
using GingerWPF.UserControlsLib;
using GingerWPF.UserControlsLib.UCTreeView;
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

namespace Ginger.BusinessFlowWindows
{
    /// <summary>
    /// Interaction logic for BusinessFlowsAutomatePage.xaml
    /// </summary>
    public partial class BusinessFlowsAutomatePage : Page
    {
        SingleItemTreeViewExplorerPage mBusFlowsPage;
        Page mAutomatePage;

        public BusinessFlowsAutomatePage()
        {
            InitializeComponent();

            App.AutomateBusinessFlowEvent += App_AutomateBusinessFlowEvent;
            App.UserProfile.PropertyChanged += UserProfile_PropertyChanged;

            Reset();
        }

        private void App_AutomateBusinessFlowEvent(AutomateEventArgs args)
        {
            if (args.EventType == AutomateEventArgs.eEventType.Automate)
            {
                ShiftToAutoMateView();
            }
        }

        private void UserProfile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserProfile.Solution))
            {
                Reset();
            }
        }

        private void Reset()
        {
            mBusFlowsPage = null;            
            //App.BusinessFlow = null;
            ShiftToBusinessFlowView();
        }

        private void ShiftToBusinessFlowView()
        {
            if(mBusFlowsPage == null && App.UserProfile.Solution != null)
            {
                BusinessFlowsFolderTreeItem busFlowsRootFolder = new BusinessFlowsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>());
                mBusFlowsPage = new SingleItemTreeViewExplorerPage(GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.BusinessFlows), eImageType.BusinessFlow, busFlowsRootFolder, busFlowsRootFolder.SaveAllTreeFolderItemsHandler, busFlowsRootFolder.AddItemHandler, treeItemDoubleClickHandler: BusinessFlowsTree_ItemDoubleClick);
            }
            xContentFrame.Content = mBusFlowsPage;
        }

        private void ShiftToAutoMateView()
        {
            if (mAutomatePage == null)
            {
                if (WorkSpace.Instance.BetaFeatures.ShowNewautomate)
                {
                    mAutomatePage = new NewAutomatePage(); 
                }
                else
                {
                    mAutomatePage = new AutomatePage();
                    ((AutomatePage)mAutomatePage).GoToBusFlowsListHandler = GoToBusinessFlowsList;
                }
                
            }
            xContentFrame.Content = mAutomatePage;
        }

        private void GoToBusinessFlowsList(object sender, RoutedEventArgs e)
        {
            ShiftToBusinessFlowView();
        }

        private void BusinessFlowsTree_ItemDoubleClick(object sender, EventArgs e)
        {
            TreeViewItem i = (TreeViewItem)sender;
            if (i != null)
            {
                ITreeViewItem iv = (ITreeViewItem)i.Tag;

                if (iv.NodeObject() != null && iv.NodeObject() is BusinessFlow)
                    App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.Automate, (BusinessFlow)iv.NodeObject());
            }
        }

    }
}
