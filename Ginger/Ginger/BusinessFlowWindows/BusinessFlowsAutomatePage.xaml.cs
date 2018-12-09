#region License
/*
Copyright © 2014-2018 European Support Limited

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
                    ((AutomatePage)mAutomatePage).GoToBusFlowsListHandler(GoToBusinessFlowsList);
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
                {
                    App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.Automate, (BusinessFlow)iv.NodeObject());
                }
            }
        }

    }
}
