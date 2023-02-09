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
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Windows.Controls;
using GingerCoreNET.GeneralLib;

namespace GingerWPF.UserControlsLib
{
    /// <summary>
    /// Interaction logic for TreeViewExplorerPage.xaml
    /// </summary>
    public partial class TreeViewExplorerPage : Page
    {
        public TreeViewExplorerPage(ITreeViewItem Root)
        {
            Init();
            TreeViewItem r = MainTreeView.Tree.AddItem(Root);             
            r.IsExpanded = true;            
        }

        private void Init()
        {
            InitializeComponent();
            MainTreeView.Tree.ItemSelected += MainTreeView_ItemSelected;
        }

        public TreeViewExplorerPage(ObservableList<ITreeViewItem> RootItems)
        {
            InitializeComponent();

            MainTreeView.Tree.ItemSelected += MainTreeView_ItemSelected;

            foreach(ITreeViewItem TVI in RootItems)
            {
                TreeViewItem r = MainTreeView.Tree.AddItem(TVI);
            }            
        }

        private void MainTreeView_ItemSelected(object sender, EventArgs e)
        {
            TreeViewItem TVI = (TreeViewItem)sender;
            ITreeViewItem TVObj = (ITreeViewItem)TVI.Tag;
            if (TVObj is ITreeViewItem)
            {
                DetailsFrame.Content = TVObj.EditPage();                                
            }
            else
            {                
                DetailsFrame.Content = "Object doesn't have edit page yet - " + TVObj.GetType().Name;
            }
        }
    }
}
