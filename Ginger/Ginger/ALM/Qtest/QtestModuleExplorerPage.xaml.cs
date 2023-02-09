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
using System;
using System.Windows;
using System.Windows.Controls;
using GingerWPF.UserControlsLib.UCTreeView;
using Ginger.ALM.Qtest.TreeViewItems;
using GingerCore.ALM.Qtest;
using GingerCore.ALM.QC;
using GingerCore.ALM.QCRestAPI;
using GingerCore.ALM;
using System.Windows.Input;

namespace Ginger.ALM.Qtest
{
    public partial class QtestModuleExplorerPage : Page
    {
        public string SelectedPath {get;set;}

        GenericWindow genWin = null;

        ObservableList<QTestAPIStdModel.ModuleResource> treeData;
        object mCurrentSelectedObject = new object();
        private ITreeViewItem mCurrentSelectedTreeItem = null;

        public QtestModuleExplorerPage()
        {
            InitializeComponent();

            TestPlanExplorerTreeView.TreeTitle = "'" + ALMCore.DefaultAlmConfig.ALMDomain + " \\ " + ALMCore.DefaultAlmConfig.ALMProjectName + "' - Module Explorer";
            TestPlanExplorerTreeView.SetTitleSection(2, 30, 15, FontWeights.Bold);
            //set root item
            GetTreeData();
            foreach (QTestAPIStdModel.ModuleResource moduleResource in treeData)
            {
                QtestModuleTreeItem tvi = new QtestModuleTreeItem(moduleResource.Children);
                tvi.ID = moduleResource.Id.ToString();
                tvi.Path = moduleResource.Path;
                tvi.Name = moduleResource.Name;
                TestPlanExplorerTreeView.Tree.AddItem(tvi);
            }
            TestPlanExplorerTreeView.Tree.ItemSelected += TestPlanExplorerTreeView_ItemSelected;
        }

        private void GetTreeData()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            treeData = QtestConnect.Instance.GetQTestModulesByProject(ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMProjectKey);
            Mouse.OverrideCursor = null;
        }

        private void TestPlanExplorerTreeView_ItemSelected(object sender, EventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            ITreeViewItem iv = (ITreeViewItem)item.Tag;
            mCurrentSelectedTreeItem = (QtestModuleTreeItem)iv;
            mCurrentSelectedObject = mCurrentSelectedTreeItem;
        }

        public object ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button selectBtn = new Button();
            selectBtn.Content = "Select Folder";
            selectBtn.Click += new RoutedEventHandler(selectBtn_Clicked);

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, "Browse Qtest/ALM Test Plan", this, new ObservableList<Button> { selectBtn }, true, "Cancel", Cancel_Clicked);

            return mCurrentSelectedObject;
        }

        private void selectBtn_Clicked(object sender, RoutedEventArgs e)
        {
            genWin.Close();
        }

        private void Cancel_Clicked(object sender, EventArgs e)
        {
            SelectedPath = null;
            genWin.Close();
        }
        
        
    }
}
