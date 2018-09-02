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

using Ginger.Environments;
using Ginger.SolutionWindows.TreeViewItems;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.Environments;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Reflection;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.SolutionWindows
{
    /// <summary>
    /// Interaction logic for SolutionPage.xaml
    /// </summary>
    public partial class SolutionExplorerPage : Page
    {
        public enum eRefreshSolutionType
        {
            InitAllPage,RefreshTreeFolder
        }

        GridLength mlastsolutionColWidth = new GridLength(300);
        GridLength mMinColsExpanderSize = new GridLength(35);

        public SolutionExplorerPage()
        {
            InitializeComponent();

            SolutionTreeExpander.IsExpanded = true;
            SolutionTreeView.Tree.ItemSelected += SolutionTreeView_ItemSelected;
            SolutionTreeView.Tree.ItemDoubleClick +=SolutionTreeView_ItemDoubleClick;

            Refresh();

            // Listen for solution change
            App.UserProfile.PropertyChanged += UserProfile_PropertyChanged;
        }

        public void RefreshTreeItemFolder(Type treeViewItemFolderType)
        {
            object o = SolutionTreeView.Tree.GetItemAt(0);

            TreeViewItem TreeItemFolder = SolutionTreeView.Tree.FindMatchingTreeItemByType((TreeViewItem)o, treeViewItemFolderType);
            TreeItemFolder.IsSelected = true;
            SolutionTreeView.Tree.RefreshSelectedTreeNodeChildrens();
        }

        private void SolutionTreeView_ItemDoubleClick(object sender, EventArgs e)
        {
            TreeViewItem i = (TreeViewItem)sender;
            if (i != null)
            {
                ITreeViewItem iv = (ITreeViewItem)i.Tag;
                App.CurrentRepositoryItem = (RepositoryItem)iv.NodeObject();
            }

            //if (App.CurrentRepositoryItem != null)
            //{
            //    if (App.UserProfile.UserTypeHelper.IsSupportAutomate && App.CurrentRepositoryItem.GetType().IsAssignableFrom(typeof(BusinessFlow)))
            //    {
            //        App.MainWindow.AutomateBusinessFlow((BusinessFlow)App.CurrentRepositoryItem);
            //    }
            //}
        }

        private void SolutionTreeView_ItemSelected(object sender, EventArgs e)
        {
            // Show the Edit page in the frame
            TreeViewItem i = (TreeViewItem)sender;
            if (i != null)
            {
                ITreeViewItem iv = (ITreeViewItem)i.Tag;
                Page contentPage = iv.EditPage();
                ItemDetailsFrame.Content = contentPage;
                if (contentPage != null)
                {
                    //fit content to window size
                    Binding widthBinding = new Binding();
                    widthBinding.Source = ItemDetailsFrame;
                    widthBinding.Path = new PropertyPath(Frame.WidthProperty);
                    widthBinding.Mode = System.Windows.Data.BindingMode.OneWay;
                    widthBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    contentPage.SetBinding(Page.WidthProperty, widthBinding);
                    Binding heightBinding = new Binding();
                    heightBinding.Source = ItemDetailsFrame;
                    heightBinding.Path = new PropertyPath(Frame.HeightProperty);
                    heightBinding.Mode = System.Windows.Data.BindingMode.OneWay;
                    heightBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    contentPage.SetBinding(Page.HeightProperty, heightBinding);
                }

                Object obj = iv.NodeObject();
                if (obj is RepositoryItem)
                {
                    App.CurrentRepositoryItem = (RepositoryItem)iv.NodeObject();
                }
                else
                {
                    App.CurrentRepositoryItem = null;
                }
                App.CurrentSelectedTreeItem = iv;
                App.AddItemToSaveAll();
            }
            else
            {
                ItemDetailsFrame.Content = null;
                App.CurrentRepositoryItem = null;
            }                                       
        }

        void Refresh()
        {
            Init(App.UserProfile.Solution);
        }

        public void Init(Solution s, bool RefreshSol=true)
        {            
            if (s != null)
            {
                NoSolutionLabel.Visibility = System.Windows.Visibility.Collapsed;
                ExplorerTreeGrid.Visibility = System.Windows.Visibility.Visible;                

                ItemDetailsFrame.Content = null;                                
                LoadSoultionTree2();

                //show tips for business flows / agents
                if(WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Count == 0)
                {
                    //show tip to start with creating a business flow
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.CreateBusinessFlowTip);
                }
            }
            else
            {
                NoSolutionLabel.Visibility = System.Windows.Visibility.Visible;
                ExplorerTreeGrid.Visibility = System.Windows.Visibility.Collapsed;                
            }
        }

        public void LoadSoultionTree2()
        {
            //Clear all
            SolutionTreeView.Tree.ClearTreeItems();

            SolutionTreeView.TreeTitle = App.UserProfile.Solution.Name;
            SolutionTreeView.TreeTooltip = App.UserProfile.Solution.Folder;
            SolutionTreeView.TreeTitleStyle = (Style)TryFindResource("@ucTitleStyle_1");

            // Add Solution root item
            SolutionTreeItem STI = new SolutionTreeItem();
            STI.Solution = App.UserProfile.Solution;
            TreeViewItem Root = SolutionTreeView.Tree.AddItem(STI);

            // and autoexapnd for user
            try
            {
            Root.IsExpanded = true;

            }catch(Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}");
            }
        }

        private void UserProfile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Solution")
            {
                Refresh();
            }
        }

        private void SolutionTreeExpander_ExpandedCollapsed(object sender, RoutedEventArgs e)
        {
            if (SolutionTreeExpander.IsExpanded)
            {
                SolutionTreeExpanderLabel.Content = null;
                SolutionTreeColumn.Width = mlastsolutionColWidth;
            }
            else
            {
                mlastsolutionColWidth = SolutionTreeColumn.Width;

                SolutionTreeExpanderLabel.Content = App.UserProfile.Solution.Name;
                SolutionTreeExpanderLabel.ToolTip = App.UserProfile.Solution.Folder;
                SolutionTreeColumn.Width = mMinColsExpanderSize;
            }
        }

        private void SolutionTreeExpander_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (SolutionTreeExpander.IsExpanded == false && e.NewSize.Width > mMinColsExpanderSize.Value)
            {
                SolutionTreeColumn.Width = mMinColsExpanderSize;
            }
        }

        private void SolutionTreeView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (SolutionTreeExpander.IsExpanded && e.NewSize.Width <= 50)
            {
                SolutionTreeExpander.IsExpanded = false;
                mlastsolutionColWidth = new GridLength(300);
                SolutionTreeColumn.Width = mMinColsExpanderSize;
            }
        }

        private void SolutionTreeExpander_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SolutionTreeExpander.Content != null)
                SolutionTreeExpander.IsExpanded = true;
            else
                SolutionTreeExpander.IsExpanded = false;
        }
    }
}
