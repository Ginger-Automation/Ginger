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

using Amdocs.Ginger.Common.Enums;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.UserControlsLib
{
    /// <summary>
    /// Interaction logic for TreeViewExploreItemPage.xaml
    /// </summary>
    public partial class SingleItemTreeViewExplorerPage : Page
    {
        public TreeView1 TreeView
        {
            get { return xTreeView; }
        }

        public SingleItemTreeViewExplorerPage(string itemTypeName, eImageType itemTypeIcon, ITreeViewItem itemTypeRootNode, RoutedEventHandler saveAllHandler = null, RoutedEventHandler addHandler = null)
        {
            InitializeComponent();

            xTreeView.TreeTitle = itemTypeName;
            xTreeView.TreeIcon = itemTypeIcon;
            
            TreeViewItem r = xTreeView.Tree.AddItem(itemTypeRootNode);            
            r.IsExpanded = true;

            itemTypeRootNode.SetTools(xTreeView);
            xTreeView.SetTopToolBarTools(saveAllHandler, addHandler);

            xTreeView.Tree.ItemSelected += MainTreeView_ItemSelected;
        }

        private void MainTreeView_ItemSelected(object sender, EventArgs e)
        {
            TreeViewItem TVI = (TreeViewItem)sender;
            object tvItem = TVI.Tag;            

            if (tvItem is ITreeViewItem)
            {
                DetailsFrame.Content = ((ITreeViewItem)tvItem).EditPage();            
            }
            else
            {
                DetailsFrame.Content = "View/Edit page is not available yet for the tree item '" + tvItem.GetType().Name + "'";
            }
        }
    }
}
