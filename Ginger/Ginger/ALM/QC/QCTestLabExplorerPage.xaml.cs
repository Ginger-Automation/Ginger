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
using GingerCore.ALM.QC;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GingerCore.ALM.QCRestAPI;
using GingerCore.ALM;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.ALM.QC {
    public partial class QCTestLabExplorerPage : Page
    {
        public enum eExplorerTestLabPageUsageType { Import, Select, BrowseFolders }
                
        private eExplorerTestLabPageUsageType mExplorerTestLabPageUsageType;
        ObservableList<BusinessFlow> mBizFlows;
        private List<string[]> mExecDetailNames;

        private ITreeViewItem mCurrentSelectedTreeItem = null;
        public string CurrentSelectedPath { get; set; }
        ObservableList<QCTestSetTreeItem> mCurrentSelectedTestSets= new ObservableList<QCTestSetTreeItem>();
        public ObservableList<QCTestSetTreeItem> CurrentSelectedTestSets
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
        
        public QCTestLabExplorerPage(eExplorerTestLabPageUsageType explorerTestLabPageUsageType, string importDestinationPath="")
        {
            InitializeComponent();

            mExplorerTestLabPageUsageType = explorerTestLabPageUsageType;
            mImportDestinationPath = importDestinationPath;

            //root item
            QCTestLabFolderTreeItem tvi = new QCTestLabFolderTreeItem();
            tvi.Folder = ALMCore.DefaultAlmConfig.AlmType == GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Octane ? "Application Modules" : "Root";
            tvi.Path = ALMCore.DefaultAlmConfig.AlmType == GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Octane ? @"Application Modules" : @"Root";
            TestLabExplorerTreeView.Tree.AddItem(tvi);

            TestLabExplorerTreeView.TreeTitle = "'" + ALMCore.DefaultAlmConfig.ALMDomain + " \\ " + ALMCore.DefaultAlmConfig.ALMProjectName + "' - Test Lab Explorer";
            TestLabExplorerTreeView.TreeTitleStyle = (Style)TryFindResource("@ucTitleStyle_3");
            TestLabExplorerTreeView.Tree.ItemSelected += TestLabExplorerTreeView_ItemSelected;

            LoadDataBizFlows();

            mExecDetailNames = new List<string[]>();

            ShowTestSetDetailsPanel(false);
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
                    total += number;
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
        }

        private void TestLabExplorerTreeView_ItemSelected(object sender, EventArgs e)
        {
            mCurrentSelectedTestSets.Clear();

            TreeViewItem item = (TreeViewItem)sender;
            mCurrentSelectedTreeItem = (ITreeViewItem)item.Tag;

            if (mCurrentSelectedTreeItem is QCTestSetTreeItem)
            {
                if (mExplorerTestLabPageUsageType != eExplorerTestLabPageUsageType.BrowseFolders)
                {
                    mCurrentSelectedTestSets.Add((QCTestSetTreeItem)mCurrentSelectedTreeItem);
                    GetTestSetDetails(mCurrentSelectedTestSets[0]);
                    CurrentSelectedPath = mCurrentSelectedTestSets[0].Path;
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
                CurrentSelectedPath = ((QCTestLabFolderTreeItem)mCurrentSelectedTreeItem).Path;
                ShowTestSetDetailsPanel(false);
            }
        }

        private void GetFolderChildTestSets(ITreeViewItem folder)
        {
            if (((QCTestLabFolderTreeItem)folder).CurrentChildrens == null)
                folder.Childrens();
            foreach (ITreeViewItem item in ((QCTestLabFolderTreeItem)folder).CurrentChildrens)
            {
                if (item is QCTestSetTreeItem)
                    mCurrentSelectedTestSets.Add((QCTestSetTreeItem)item);
                else
                    GetFolderChildTestSets(item);
            }
        }

        private void GetTestSetDetails(QCTestSetTreeItem testSetItem)
        {            
            mExecDetailNames = testSetItem.TestSetStatuses;
            LoadExecutionDetails();
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
            switch (mExplorerTestLabPageUsageType)
            {
                case (eExplorerTestLabPageUsageType.Import):
                    Button importBtn = new Button();
                    importBtn.Content = "Import Selected";
                    importBtn.Click += new RoutedEventHandler(ImportSelected);
                    GingerCore.General.LoadGenericWindow(ref _GenericWin, App.MainWindow, windowStyle, "Browse ALM Test Lab", this, new ObservableList<Button> { importBtn }, true, "Cancel", Cancel_Clicked);
                    return CurrentSelectedTestSets;

                case (eExplorerTestLabPageUsageType.Select):
                    Button selectBtn = new Button();
                    selectBtn.Content = "Select";
                    selectBtn.Click += new RoutedEventHandler(Select);
                    GingerCore.General.LoadGenericWindow(ref _GenericWin, App.MainWindow, windowStyle, "Browse ALM Test Lab", this, new ObservableList<Button> { selectBtn }, true,"Cancel", Cancel_Clicked);
                    return CurrentSelectedTestSets;

                case (eExplorerTestLabPageUsageType.BrowseFolders):
                    Button selectFolderBtn = new Button();
                    selectFolderBtn.Content = "Select Folder";
                    selectFolderBtn.Click += new RoutedEventHandler(SelectFolder);
                    GingerCore.General.LoadGenericWindow(ref _GenericWin, App.MainWindow, windowStyle, "Browse ALM Test Lab", this, new ObservableList<Button> { selectFolderBtn }, true, "Cancel", Cancel_Clicked);
                    return CurrentSelectedPath;
            }

            return null;
        }

        private void Select(object sender, RoutedEventArgs e)
        {
            if (mCurrentSelectedTreeItem == null)
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
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
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Please select a folder item");
            else
            {
                _GenericWin.Close();
            }
        }  

        private void ImportSelected(object sender, RoutedEventArgs e)
        {
            if (mCurrentSelectedTreeItem != null)
            {
                if (mCurrentSelectedTreeItem is QCTestLabFolderTreeItem)
                {
                    //get all child test sets
                    Mouse.OverrideCursor = Cursors.Wait;
                    GetFolderChildTestSets(mCurrentSelectedTreeItem);
                    Mouse.OverrideCursor = null;
                }
    
                if (ALMIntegration.Instance.ImportSelectedTestSets(mImportDestinationPath, (IEnumerable<object>)CurrentSelectedTestSets) == true)
                {
                    //Refresh the explorer selected tree items import status
                    LoadDataBizFlows();
                    foreach (QCTestSetTreeItem testSet in CurrentSelectedTestSets)
                        testSet.IsTestSetAlreadyImported();
                    ShowTestSetDetailsPanel(false);
                }
            }
            else
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
        }

        private void Cancel_Clicked(object sender, EventArgs e)
        {
            CurrentSelectedTestSets = null;
            CurrentSelectedPath = null;
            _GenericWin.Close();
        }
    }
}
