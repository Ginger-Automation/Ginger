#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Ginger.Help;
using GingerCore.GeneralLib;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        public object SelectedItemObject
        {
            get
            {
                if (TreeView.Tree.CurrentSelectedTreeViewItem != null)
                {
                    return TreeView.Tree.CurrentSelectedTreeViewItem.NodeObject();
                }
                else
                {
                    return null;
                }
            }
        }

        public SingleItemTreeViewExplorerPage(string itemTypeName, eImageType itemTypeIcon, ITreeViewItem itemTypeRootNode, RoutedEventHandler saveAllHandler = null, RoutedEventHandler addHandler = null, EventHandler treeItemDoubleClickHandler = null, bool isSaveButtonHidden = false, bool showTitle = true)
        {
            InitializeComponent();

            GingerHelpProvider.SetHelpString(this, itemTypeName.TrimEnd(['s']));

            xTreeView.TreeTitle = itemTypeName;
            xTreeView.TreeIcon = itemTypeIcon;
            xTreeView.Background = (Brush)FindResource("$BackgroundColor_White");

            TreeViewItem r = xTreeView.Tree.AddItem(itemTypeRootNode);
            r.IsExpanded = true;

            itemTypeRootNode.SetTools(xTreeView);
            xTreeView.SetTopToolBarTools(saveAllHandler, addHandler, isSaveButtonHidden: isSaveButtonHidden);
            xTreeView.Tree.ItemSelected -= MainTreeView_ItemSelected;
            xTreeView.Tree.ItemSelected += MainTreeView_ItemSelected;

            if (treeItemDoubleClickHandler != null)
            {
                xTreeView.Tree.ItemDoubleClick -= treeItemDoubleClickHandler;
                xTreeView.Tree.ItemDoubleClick += treeItemDoubleClickHandler;
            }

            if (!showTitle)
            {
                TreeView.TreeTitleVisibility = Visibility.Collapsed;
            }
        }

        private void MainTreeView_ItemSelected(object sender, EventArgs e)
        {
            TreeViewItem TVI = (TreeViewItem)sender;
            object tvItem = TVI.Tag;

            if (tvItem is ITreeViewItem ITVItem)
            {
                DetailsFrame.ClearAndSetContent(ITVItem.EditPage());
                if (tvItem is NewTreeViewItemBase newTreeViewItemBase)
                {
                    newTreeViewItemBase.PrepareItemForEdit();
                }
            }
            else
            {
                DetailsFrame.Content = "View/Edit page is not available yet for the tree item '" + tvItem.GetType().Name + "'";
            }
        }

    }
}
