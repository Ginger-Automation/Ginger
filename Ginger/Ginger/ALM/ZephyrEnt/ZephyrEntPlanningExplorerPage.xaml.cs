#region License
/*
Copyright © 2014-2023 European Support Limited

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

using Amdocs.Ginger.Common;
using Ginger.ALM.QC.TreeViewItems;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GingerCore.ALM;
using amdocs.ginger.GingerCoreNET;
using ZephyrEntStdSDK.Models.Base;
using Ginger.ALM.ZephyrEnt.TreeViewItems;
using Newtonsoft.Json.Linq;
using GingerCore.ALM.ZephyrEnt.Bll;
using System.IO;

namespace Ginger.ALM.ZephyrEnt
{
    /// <summary>
    /// Interaction logic for ZephyrEntPlanningExplorerPage.xaml
    /// </summary>
    public partial class ZephyrEntPlanningExplorerPage : Page
    {
        public enum eExplorerTestPlanningPageUsageType { Import, Select, BrowseFolders, Map }
        private eExplorerTestPlanningPageUsageType mExplorerTestPlanningPageUsageType;
        ObservableList<BusinessFlow> mBizFlows;
        Dictionary<string, BusinessFlow> bfsExternalIds = new Dictionary<string, BusinessFlow>();
        private List<string[]> mExecDetailNames;
        List<BaseResponseItem> treeData;
        private ITreeViewItem mCurrentSelectedTreeItem = null;
        public string CurrentSelectedPath { get; set; }
        ObservableList<ZephyrEntPhaseTreeItem> mCurrentSelectedTestSets = new ObservableList<ZephyrEntPhaseTreeItem>();
        public ObservableList<ZephyrEntPhaseTreeItem> CurrentSelectedTestSets
        {
            get
            {
                return mCurrentSelectedTestSets;
            }
            set
            {
                mCurrentSelectedTestSets = value;
            }
        }
        private string mImportDestinationPath = string.Empty;
        GenericWindow _GenericWin = null;

        public ZephyrEntPlanningExplorerPage(eExplorerTestPlanningPageUsageType explorerTestPlanningPageUsageType, string importDestinationPath = "")
        {
            InitializeComponent();

            mExplorerTestPlanningPageUsageType = explorerTestPlanningPageUsageType;
            mImportDestinationPath = importDestinationPath;
            LoadDataBizFlows();
            GetTreeData();

            TestPlanningExplorerTreeView.TreeTitle = "'" + ALMCore.DefaultAlmConfig.ALMProjectName + "' - Zephyr Ent. Cycles Explorer";
            TestPlanningExplorerTreeView.TreeTitleStyle = (Style)TryFindResource("@ucTitleStyle_3");
            TestPlanningExplorerTreeView.Tree.ItemSelected += TestPlanningExplorerTreeView_ItemSelected;
            treeData.ForEach(folder =>
            {
                TestPlanningFolderTreeItem tvv = new TestPlanningFolderTreeItem(folder);
                tvv.entityType = EntityFolderType.Cycle;
                tvv.Folder = tvv.Name;
                tvv.Path = tvv.Name;
                tvv.FolderOnly = explorerTestPlanningPageUsageType.Equals(eExplorerTestPlanningPageUsageType.BrowseFolders);
                tvv.CurrentChildrens = new List<ITreeViewItem>();
                foreach (var item in (JArray)folder.TryGetItem("cyclePhases"))
                {
                    dynamic d = JObject.Parse(item.ToString());
                    tvv.CurrentChildrens.Add(new TestPlanningFolderTreeItem()
                    { Id = d.id.ToString(), CycleId = d.id,  Name = d.name.ToString(), entityType = EntityFolderType.Phase
                    , Folder = d.name.ToString(), Path = tvv.Path +  @"\" + d.name.ToString(), FolderOnly = tvv.FolderOnly, RevisionId = Convert.ToInt32(folder.TryGetItem("revision")) });
                }
                TestPlanningExplorerTreeView.Tree.AddItem(tvv);
            });

            mExecDetailNames = new List<string[]>();

            ShowTestSetDetailsPanel(false);
        }

        private void GetTreeData()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            treeData = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetZephyrEntTreeData(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), EntityFolderType.Cycle.ToString(), false);
            Mouse.OverrideCursor = null;
        }

        private void LoadExecutionDetails()
        {
            TSExecDetails.Children.Clear();
            //Total TC's
            int total = 0;
            foreach (string[] detail in mExecDetailNames)
            {
                short number;
                if (Int16.TryParse(detail[1], out number))
                {
                    total += number;
                }
            }
            Label totalTcsNum = new Label();
            totalTcsNum.Content = "Total Number of TC's: " + total;
            totalTcsNum.Style = this.FindResource("@SmallerInputFieldLabelStyle") as Style;
            totalTcsNum.Margin = new Thickness(0, 0, 0, 0);
            TSExecDetails.Children.Add(totalTcsNum);

            foreach (string[] detail in mExecDetailNames)
            {
                Label StatusName = new Label();
                StatusName.Content = detail[1] + " TC's in Status '" + detail[0] + "'";
                StatusName.Style = this.FindResource("@SmallerInputFieldLabelStyle") as Style;
                StatusName.Margin = new Thickness(0, 0, 0, 0);
                TSExecDetails.Children.Add(StatusName);
            }
        }

        /// <summary>
        /// load BF for Import ExternalID check if exists
        /// </summary>
        private void LoadDataBizFlows()
        {
            mBizFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            if(mBizFlows != null)
            {
                mBizFlows.ToList().ForEach(bf =>
                {
                    if (!String.IsNullOrEmpty(bf.ExternalID) && !bfsExternalIds.ContainsKey(bf.ExternalID))
                    {
                        bfsExternalIds.Add(bf.ExternalID, bf);
                    }
                });
            }
        }

        private void TestPlanningExplorerTreeView_ItemSelected(object sender, EventArgs e)
        {
            mCurrentSelectedTestSets.Clear();

            TreeViewItem item = (TreeViewItem)sender;
            mCurrentSelectedTreeItem = (ITreeViewItem)item.Tag;

            if (mCurrentSelectedTreeItem is ZephyrEntPhaseTreeItem)
            {
                if (mExplorerTestPlanningPageUsageType != eExplorerTestPlanningPageUsageType.BrowseFolders)
                {
                    mCurrentSelectedTestSets.Add((ZephyrEntPhaseTreeItem)mCurrentSelectedTreeItem);
                    GetTestSetDetails(mCurrentSelectedTestSets[0]);
                    ShowTestSetDetailsPanel(true);
                }
                else
                {
                    //don't count as selected
                    mCurrentSelectedTreeItem = null;
                    mCurrentSelectedTestSets.Clear();
                }
            }
            else
            {
                //probably a folder 
                string[] returnedValues = new string[] { ((TestPlanningFolderTreeItem)mCurrentSelectedTreeItem).entityType.ToString(),
                    ((TestPlanningFolderTreeItem)mCurrentSelectedTreeItem).Id,
                    ((TestPlanningFolderTreeItem)mCurrentSelectedTreeItem).ParentId.ToString(),
                    ((TestPlanningFolderTreeItem)mCurrentSelectedTreeItem).CycleId.ToString()};
                CurrentSelectedPath = String.Join("#", returnedValues); 
                ShowTestSetDetailsPanel(false);
            }
        }

        private void GetFolderChildTestSets(ITreeViewItem folder)
        {
            if (((TestPlanningFolderTreeItem)folder).CurrentChildrens == null ||
                ((TestPlanningFolderTreeItem)folder).entityType is EntityFolderType.Cycle)
            {
                folder.Childrens();
            }
            foreach (ITreeViewItem item in ((TestPlanningFolderTreeItem)folder).CurrentChildrens)
            {
                if (item is ZephyrEntPhaseTreeItem)
                {
                    mCurrentSelectedTestSets.Add((ZephyrEntPhaseTreeItem)item);
                }
                else
                {
                    GetFolderChildTestSets(item);
                }
            }
        }

        private void GetTestSetDetails(ZephyrEntPhaseTreeItem testSetItem)
        {
            mExecDetailNames = testSetItem.TestSetStatuses;
            LoadExecutionDetails();
            UpdateIfAlreadyImported(testSetItem);
            if (testSetItem.AlreadyImported)
            {
                lblTSAlreadyImported.Content = "Yes";
                txtBoxTSMappedBF.Text = "'" + testSetItem.MappedBusinessFlowPath + "'";
            }
            else
            {
                lblTSAlreadyImported.Content = "No";
                txtBoxTSMappedBF.Text = "None";
            }
        }

        private void ShowTestSetDetailsPanel(bool toShow)
        {
            if (toShow)
            {
                TestSetDetailsPanel.Visibility = System.Windows.Visibility.Visible;
                testSetDetailsColumn.Width = new GridLength(200);
            }
            else
            {
                TestSetDetailsPanel.Visibility = System.Windows.Visibility.Collapsed;
                testSetDetailsColumn.Width = new GridLength(0);
            }
        }

        public object ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            switch (mExplorerTestPlanningPageUsageType)
            {
                case (eExplorerTestPlanningPageUsageType.Import):
                    Button importBtn = new Button();
                    importBtn.Content = "Import Selected";
                    importBtn.Click += new RoutedEventHandler(ImportSelected);
                    GingerCore.General.LoadGenericWindow(ref _GenericWin, App.MainWindow, windowStyle, "Browse ALM Test Planning", this, new ObservableList<Button> { importBtn }, true, "Cancel", Cancel_Clicked);
                    return CurrentSelectedTestSets;

                case (eExplorerTestPlanningPageUsageType.Select):
                    Button selectBtn = new Button();
                    selectBtn.Content = "Select";
                    selectBtn.Click += new RoutedEventHandler(Select);
                    GingerCore.General.LoadGenericWindow(ref _GenericWin, App.MainWindow, windowStyle, "Browse ALM Test Planning", this, new ObservableList<Button> { selectBtn }, true, "Cancel", Cancel_Clicked);
                    return CurrentSelectedTestSets;

                case (eExplorerTestPlanningPageUsageType.BrowseFolders):
                    Button selectFolderBtn = new Button();
                    selectFolderBtn.Content = "Select Folder";
                    selectFolderBtn.Click += new RoutedEventHandler(SelectFolder);
                    GingerCore.General.LoadGenericWindow(ref _GenericWin, App.MainWindow, windowStyle, "Browse ALM Test Planning", this, new ObservableList<Button> { selectFolderBtn }, true, "Cancel", Cancel_Clicked);
                    return CurrentSelectedPath;
                case (eExplorerTestPlanningPageUsageType.Map):
                    //Button importBtn = new Button();
                    //importBtn.Content = "Import Selected";
                    //importBtn.Click += new RoutedEventHandler(ImportSelected);
                    //GingerCore.General.LoadGenericWindow(ref _GenericWin, App.MainWindow, windowStyle, "Browse ALM Test Planning", this, new ObservableList<Button> { importBtn }, true, "Cancel", Cancel_Clicked);
                    return CurrentSelectedTestSets;
                default:
                    return "";
            }

            return null;
        }

        private void Select(object sender, RoutedEventArgs e)
        {
            if (mCurrentSelectedTreeItem == null)
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
            else
            {
                if (mCurrentSelectedTreeItem is QCTestLabFolderTreeItem)
                {
                    //get all child test sets
                    Mouse.OverrideCursor = Cursors.Wait;
                    GetFolderChildTestSets(mCurrentSelectedTreeItem);
                    Mouse.OverrideCursor = null;
                }
                _GenericWin.Close();
            }
        }

        private void SelectFolder(object sender, RoutedEventArgs e)
        {
            if (mCurrentSelectedTreeItem == null)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Please select a folder item");
            }
            else
            {
                _GenericWin.Close();
            }
        }

        private void ImportSelected(object sender, RoutedEventArgs e)
        {
            if (mCurrentSelectedTreeItem != null)
            {
                if (mCurrentSelectedTreeItem is TestPlanningFolderTreeItem)
                {
                    //get all child test sets
                    Mouse.OverrideCursor = Cursors.Wait;
                    GetFolderChildTestSets(mCurrentSelectedTreeItem);
                    Mouse.OverrideCursor = null;
                }
                if(CurrentSelectedTestSets.Count == 0)
                {
                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Please select Test Set or Folder contains Test Set");
                    return;
                }
                CurrentSelectedTestSets.ToList().ForEach(ts =>
                {
                    if (!ts.AlreadyImported)
                    {
                        UpdateIfAlreadyImported(ts);
                    }
                });
                if (ALMIntegration.Instance.ImportSelectedTestSets(mImportDestinationPath, (IEnumerable<object>)CurrentSelectedTestSets))
                {
                    //Refresh the explorer selected tree items import status
                    LoadDataBizFlows();
                    foreach (ZephyrEntPhaseTreeItem testSet in CurrentSelectedTestSets)
                    {
                        testSet.IsTestSetAlreadyImported();
                    }
                    ShowTestSetDetailsPanel(false);
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        private void UpdateIfAlreadyImported(ZephyrEntPhaseTreeItem ts)
        {
            if (bfsExternalIds.ContainsKey(ts.Id))
            {
                ts.AlreadyImported = true;
                ts.MappedBusinessFlow = bfsExternalIds[ts.Id];
                ts.MappedBusinessFlowPath = Path.Combine(bfsExternalIds[ts.Id].ContainingFolder, bfsExternalIds[ts.Id].Name);
            }
        }

        private void Cancel_Clicked(object sender, EventArgs e)
        {
            CurrentSelectedTestSets = null;
            CurrentSelectedPath = null;
            _GenericWin.Close();
        }
    }
}
