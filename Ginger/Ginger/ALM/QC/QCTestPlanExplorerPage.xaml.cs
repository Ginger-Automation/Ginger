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

using Amdocs.Ginger.Common;
using System;
using System.Windows;
using System.Windows.Controls;
using GingerWPF.UserControlsLib.UCTreeView;
using Ginger.ALM.QC.TreeViewItems;
using GingerCore.ALM.QC;
using GingerCore.ALM.QCRestAPI;
using GingerCore.ALM;

namespace Ginger.ALM.QC
{
    public partial class QCTestPlanExplorerPage : Page
    {
        public string SelectedPath {get;set;}

        GenericWindow genWin = null;

        public QCTestPlanExplorerPage()
        {
            InitializeComponent();

            TestPlanExplorerTreeView.TreeTitle = "'" + ALMCore.AlmConfig.ALMDomain + " \\ " + ALMCore.AlmConfig.ALMProjectName + "' - Test Plan Explorer";
            TestPlanExplorerTreeView.TreeTitleStyle = (Style)TryFindResource("@ucTitleStyle_3");

            //set root item
            QCTestPlanFolderTreeItem tvi = new QCTestPlanFolderTreeItem();
            tvi.Folder = "Subject";
            tvi.Path = @"Subject";
            TestPlanExplorerTreeView.Tree.AddItem(tvi);

            TestPlanExplorerTreeView.Tree.ItemSelected += TestPlanExplorerTreeView_ItemSelected;
        }

        private void TestPlanExplorerTreeView_ItemSelected(object sender, EventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            ITreeViewItem iv = (ITreeViewItem)item.Tag;
            QCTestPlanFolderTreeItem QCTVI = (QCTestPlanFolderTreeItem)iv;

            SelectedPath = QCTVI.Path;
        }
 
        public string ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button selectBtn = new Button();
            selectBtn.Content = "Select Folder";
            selectBtn.Click += new RoutedEventHandler(selectBtn_Clicked);

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, "Browse QC/ALM Test Plan", this, new ObservableList<Button> { selectBtn }, true, "Cancel", Cancel_Clicked);

            return SelectedPath;
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
