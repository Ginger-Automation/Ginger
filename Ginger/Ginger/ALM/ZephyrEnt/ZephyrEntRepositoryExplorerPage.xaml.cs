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
using GingerCore.ALM;
using System.Windows.Input;
using System.Collections.Generic;
using GingerCore.ALM.ZephyrEnt.Bll;
using Newtonsoft.Json.Linq;
using ZephyrEntStdSDK.Models.Base;

namespace Ginger.ALM.ZephyrEnt.TreeViewItems
{
    /// <summary>
    /// Interaction logic for ZephyrEntRepositoryExplorerPage.xaml
    /// </summary>
    public partial class ZephyrEntRepositoryExplorerPage : Page
    {
        public string SelectedPath { get; set; }
        List<BaseResponseItem> treeData;
        GenericWindow genWin = null;

        public ZephyrEntRepositoryExplorerPage()
        {
            InitializeComponent();

            TestRepositoryExplorerTreeView.TreeTitle = "'" + ALMCore.DefaultAlmConfig.ALMDomain + " \\ " + ALMCore.DefaultAlmConfig.ALMProjectName + "' - Test Repository Explorer";
            TestRepositoryExplorerTreeView.SetTitleSection(2, 30, 15, FontWeights.Bold);
            GetTreeData();

            treeData.ForEach(folder =>
            {
                TestRepositoryFolderTreeItem tvv = new TestRepositoryFolderTreeItem(folder);
                tvv.entityType = EntityFolderType.Phase;
                tvv.Folder = tvv.Name;
                tvv.Path = tvv.Name;
                GetFolderChilds(tvv, (JToken)folder.TryGetItem("categories"));
                TestRepositoryExplorerTreeView.Tree.AddItem(tvv);
            });
            TestRepositoryExplorerTreeView.Tree.ItemSelected += TestPlanExplorerTreeView_ItemSelected;
        }
        private void GetTreeData()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            treeData = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetRepositoryTreeByReleaseId(ALMCore.DefaultAlmConfig.ALMProjectKey);
            Mouse.OverrideCursor = null;
        }
        private void GetFolderChilds(ITreeViewItem folder, JToken categories)
        {
            if (((TestRepositoryFolderTreeItem)folder).CurrentChildrens == null)
            {
                return;
            }
            foreach (var item in categories)
            {
                TestRepositoryFolderTreeItem tvv = new TestRepositoryFolderTreeItem();
                tvv.Id = item["id"].ToString();
                tvv.Name = item["name"].ToString();
                tvv.entityType = EntityFolderType.Module;
                tvv.Path = ((TestRepositoryFolderTreeItem)folder).Path + '\\' + tvv.Name;
                tvv.Folder = tvv.Name;
                if(((JArray)item["categories"]).Count > 0)
                {
                    tvv.CurrentChildrens = new List<ITreeViewItem>();
                    GetFolderChilds(tvv, item["categories"]);
                }
                ((TestRepositoryFolderTreeItem)folder).CurrentChildrens.Add(tvv);
            }
        }
        private void TestPlanExplorerTreeView_ItemSelected(object sender, EventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            ITreeViewItem iv = (ITreeViewItem)item.Tag;
            TestRepositoryFolderTreeItem QCTVI = (TestRepositoryFolderTreeItem)iv;

            SelectedPath = QCTVI.Id;
        }

        public string ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button selectBtn = new Button();
            selectBtn.Content = "Select Folder";
            selectBtn.Click += new RoutedEventHandler(selectBtn_Clicked);

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, "Browse ALM Test Repository", this, new ObservableList<Button> { selectBtn }, true, "Cancel", Cancel_Clicked);

            return SelectedPath;
        }

        private void selectBtn_Clicked(object sender, RoutedEventArgs e)
        {
            if (Convert.ToBoolean(xCreateBusinessFlowFolder.IsChecked))
            {
                TestRepositoryFolderTreeItem.IsCreateBusinessFlowFolder = true;
            }
            else
            {
                TestRepositoryFolderTreeItem.IsCreateBusinessFlowFolder = false;
            }
            genWin.Close();
        }

        private void Cancel_Clicked(object sender, EventArgs e)
        {
            SelectedPath = null;
            genWin.Close();
        }
        private void xCreateBusinessFlowFolder_Checked(object sender, RoutedEventArgs e)
        {
            if (Convert.ToBoolean(xCreateBusinessFlowFolder.IsChecked))
            {
                TestRepositoryFolderTreeItem.IsCreateBusinessFlowFolder = true;
            }
            else
            {
                TestRepositoryFolderTreeItem.IsCreateBusinessFlowFolder = false;
            }
        }

    }
}

