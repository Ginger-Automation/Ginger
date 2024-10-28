#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Ginger.BusinessFlowWindows;
using Ginger.GeneralWindows;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.TwoLevelMenuLib;
using GingerCore;
using GingerWPF.BusinessFlowsLib;
using GingerWPF.UserControlsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.MenusLib
{
    public static class BusinessFlowsMenu
    {
        private static TwoLevelMenu twoLevelMenu;
        private static TwoLevelMenuPage mMenusPage = null;

        static SingleItemTreeViewExplorerPage mBusFlowsPage;
        static NewAutomatePage mNewAutomatePage;

        public static TwoLevelMenuPage MenusPage
        {
            get
            {
                if (mMenusPage == null)
                {
                    mMenusPage = new TwoLevelMenuPage(GetMenu());
                    WorkSpace.Instance.PropertyChanged -= WorkSpacePropertyChanged;
                    WorkSpace.Instance.PropertyChanged += WorkSpacePropertyChanged;
                }
                return mMenusPage;
            }
        }

        private static void WorkSpacePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WorkSpace.Solution))
            {
                if (WorkSpace.Instance.Solution == null)
                {
                    MenusPage.Reset();
                }
            }
        }

        private static TwoLevelMenu GetMenu()
        {
            twoLevelMenu = new TwoLevelMenu();

            TopMenuItem businessFlowsMenu = new TopMenuItem(eImageType.BusinessFlow, GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.BusinessFlows), ConsoleKey.B, "Business FLows AID", "Solution Automation Flows");
            businessFlowsMenu.Add(eImageType.BusinessFlow, "", GetBusinessFlowsPage, ConsoleKey.B, "", "AID");
            twoLevelMenu.Add(businessFlowsMenu);

            TopMenuItem automateMenu = new TopMenuItem(eImageType.Automate, "Automate", ConsoleKey.A, "Automate AID", "Design Automation Flow");
            automateMenu.Add(eImageType.Automate, "", GetAutomatePage, ConsoleKey.A, "", "AID");
            twoLevelMenu.Add(automateMenu);

            App.AutomateBusinessFlowEvent -= App_AutomateBusinessFlowEvent;
            App.AutomateBusinessFlowEvent += App_AutomateBusinessFlowEvent;

            return twoLevelMenu;
        }

        private static async void App_AutomateBusinessFlowEvent(AutomateEventArgs args)
        {
            if (args.EventType == AutomateEventArgs.eEventType.Automate)
            {
                BusinessFlow bf = (BusinessFlow)args.Object;
                try
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    Reporter.ToStatus(eStatusMsgKey.StaticStatusProcess, null, "Loading Automate Page...");
                    if (mNewAutomatePage == null)
                    {
                        mNewAutomatePage = new NewAutomatePage(bf);
                    }
                    await mNewAutomatePage.LoadBusinessFlowToAutomate(bf);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                    Reporter.HideStatusMessage();
                }
            }
        }

        private static Page GetBusinessFlowsPage()
        {
            BusinessFlowsFolderTreeItem busFlowsRootFolder = new BusinessFlowsFolderTreeItem(WorkSpace.Instance.SolutionRepository?.GetRepositoryItemRootFolder<BusinessFlow>());
            mBusFlowsPage = new SingleItemTreeViewExplorerPage(GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.BusinessFlows), eImageType.BusinessFlow, busFlowsRootFolder, busFlowsRootFolder.SaveAllTreeFolderItemsHandler, busFlowsRootFolder.AddItemHandler, treeItemDoubleClickHandler: BusinessFlowsTree_ItemDoubleClick, true, showTitle: true);
            return mBusFlowsPage;
        }

        private static void BusinessFlowsTree_ItemDoubleClick(object sender, EventArgs e)
        {
            TreeViewItem i = (TreeViewItem)sender;
            if (i != null)
            {
                ITreeViewItem iv = (ITreeViewItem)i.Tag;

                if (iv.NodeObject() is not null and BusinessFlow)
                {
                    App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.Automate, (BusinessFlow)iv.NodeObject());
                }
            }
        }

        private static BusinessFlow GetBusinessFlowToAutomate()
        {
            if (mBusFlowsPage.SelectedItemObject is not null and BusinessFlow)
            {
                return (BusinessFlow)mBusFlowsPage.SelectedItemObject;
            }
            else if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Count > 0)
            {
                return WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>()[0];
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, prefixString: "Please first add and select"));
                return null;
            }

        }

        private static Page GetAutomatePage()
        {
            BusinessFlow bf = GetBusinessFlowToAutomate();
            if (bf != null)
            {
                mNewAutomatePage = new NewAutomatePage(bf);
                App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.Automate, bf);
            }
            else
            {
                return null;
            }
            return mNewAutomatePage;
        }
    }
}
