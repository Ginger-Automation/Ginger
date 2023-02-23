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
using Amdocs.Ginger.Common.Enums;
using GingerCoreNET.SolutionRepositoryLib;
using GingerWPF.TreeViewItemsLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GingerWPF.UserControlsLib.UCTreeView
{
    /// <summary>
    /// Interaction logic for TreeView1.xaml
    /// </summary>
    public partial class TreeView1 : UserControl, ITreeView
    {
        public event EventHandler SearchStarted;

        public event EventHandler SearchCancelled;
        public event EventHandler SearchCompleted;

        private Task mSearchTask = null;
        private CancellationTokenSource mCancellationTokenSource = null;
        private string mSearchString;

        public UCTreeView Tree
        {
            get
            {
                return xTreeViewTree;
            }
        }

        public Brush Background
        {
            get
            {
                return xGrid.Background;
            }
            set
            {
                xGrid.Background = value;
            }
        }

        public Grid TreeGrid
        {
            get { return xGrid; }
        }
        public string EnableRightClick
        {
            get
            {
                return Tree.EnableRightClick.ToString();
            }
            set
            {
                Tree.EnableRightClick = Convert.ToBoolean(value);
            }
        }

        public string EnableDragDrop
        {
            get
            {
                return Tree.EnableDragDrop.ToString();
            }
            set
            {
                Tree.EnableDragDrop = Convert.ToBoolean(value);
            }
        }

        public string TreeTitle
        {
            get { return xTreeTitle.Content.ToString(); }
            set { xTreeTitle.Content = value; }
        }

        public eImageType TreeIcon
        {
            get { return xTreeIcon.ImageType; }
            set { xTreeIcon.ImageType = value; }
        }

        public object TreeTooltip
        {
            get { return xTreeViewTree.ToolTip; }
            set { xTreeViewTree.ToolTip = value; xTreeTitle.ToolTip = value; }
        }

        public Style TreeTitleStyle
        {
            get { return xTreeTitle.Style; }
            set { xTreeTitle.Style = value; }
        }

        public eImageType AddButtonIcon
        {
            get { return xAddButton.ButtonImageType; }
            set { xAddButton.ButtonImageType = value; }
        }

        bool mAllowTreeTools = true;
        public bool AllowTreeTools { get { return mAllowTreeTools; } set { mAllowTreeTools = value; } }

        public TreeView1()
        {
            InitializeComponent();

            //Tree Style
            xTreeViewTree.Tree.BorderThickness = new Thickness(0);

            xTreeViewTree.ItemSelected += xTreeViewTree_ItemSelected;
            xTreeViewTree.ItemAdded += XTreeViewTree_ItemAdded;
        }


        public bool IsSearchRunning()
        {
            if (mSearchTask == null)
            {
                return false;
            }
            else if (mSearchTask.IsCompleted == false && mSearchTask.IsCanceled == false)
            {
                return true;
            }
            return false;
        }

        private void XTreeViewTree_ItemAdded(object sender, EventArgs e)
        {
            if (sender is ITreeViewItem)
            {
                ((ITreeViewItem)sender).TreeView = this;
            }
        }

        public void SetTopToolBarTools(RoutedEventHandler saveAllHandler = null, RoutedEventHandler addHandler = null, RoutedEventHandler refreshHandler = null, RoutedEventHandler addActionHandler = null, bool isSaveButtonHidden = false)
        {
            if (saveAllHandler != null && !isSaveButtonHidden)
            {
                xSaveAllButton.Visibility = Visibility.Visible;
                xSaveAllButton.Click += saveAllHandler;
            }
            else
            {
                xSaveAllButton.Visibility = Visibility.Collapsed;
            }

            if (addHandler != null)
            {
                xAddButton.Visibility = Visibility.Visible;
                xAddButton.Click += addHandler;
            }
            else
            {
                xAddButton.Visibility = Visibility.Collapsed;
            }

            if (refreshHandler != null)
            {
                xRefreshButton.Visibility = Visibility.Visible;
                xRefreshButton.Click += refreshHandler;
            }
            else
            {
                xRefreshButton.Visibility = Visibility.Collapsed;
            }

            if (addActionHandler != null)
            {
                xAddActionButton.Visibility = Visibility.Visible;
                xAddActionButton.Click += addActionHandler;
            }
            else
            {
                xAddActionButton.Visibility = Visibility.Collapsed;
            }
        }

        private void xTreeViewTree_ItemSelected(object sender, EventArgs e)
        {
            if (xTreeViewTree.CurrentSelectedTreeViewItem != null & AllowTreeTools == true)
            {
                xTreeViewTree.CurrentSelectedTreeViewItem.SetTools(this);
            }
        }

        public void SetTitleSection(double margin, double GridLength, double TitleFontSize, FontWeight TitleFontWeight, Visibility titleSectionVisibility = Visibility.Visible)
        {
            xTreeItemHeaderPnl.Margin = new Thickness(margin);
            xTreeTitle.FontSize = TitleFontSize;
            xTreeTitle.FontWeight = TitleFontWeight;
            xTitleSection.HorizontalAlignment = HorizontalAlignment.Center;
            xTitleSection.Visibility = titleSectionVisibility;
            TreeGrid.RowDefinitions[0].Height = new GridLength(GridLength);
            xTreeActionsIconsPnl.Visibility = Visibility.Collapsed;
        }

        private async void xSearchTextBox_TextChangedAsync(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(xSearchTextBox.Text))
            {
                await ClearSearch();
                return;
            }
            // this inner method checks if user is still typing
            async Task<bool> UserKeepsTyping()
            {
                string txt = xSearchTextBox.Text;
                await Task.Delay(1000);
                return txt != xSearchTextBox.Text;
            }
            if (await UserKeepsTyping() || xSearchTextBox.Text == mSearchString) return;

            mSearchString = xSearchTextBox.Text;
            await SearchAsync();

        }

        private async void xSearchClearBtn_Click(object sender, RoutedEventArgs e)
        {
            await ClearSearch();
        }

        private async Task ClearSearch()
        {
            xSearchClearBtn.Visibility = Visibility.Collapsed;
            xSearchBtn.Visibility = Visibility.Visible;


            if (mSearchTask?.IsCompleted == false && mSearchTask?.IsCanceled == false)
            {
                await CancelSearchAsync();
            }
            else
            {
                //if search is already complete and user trying to clear text we collapse the unselected nodes
                List<TreeViewItem> pathNodes = new List<TreeViewItem>();
                if (xTreeViewTree.LastSelectedTVI != null)
                {
                    pathNodes = UCTreeView.getSelecetdItemPathNodes(xTreeViewTree.LastSelectedTVI);
                }
                UCTreeView.CollapseUnselectedTreeNodes(xTreeViewTree.TreeItemsCollection, pathNodes);
            }
            xSearchTextBox.Text = "";
            mSearchString = null;
        }



        public void SearchTree(string txt)
        {
            xSearchTextBox.Text = txt;
        }

        private void xGroupBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        public void AddToolbarTool(string toolImage, string toolTip = "", RoutedEventHandler clickHandler = null, Visibility toolVisibility = Visibility.Visible, object CommandParameter = null)
        {
            //no tool bar to add to in this View type
        }
        public void AddToolbarTool(eImageType imageType, string toolTip = "", RoutedEventHandler clickHandler = null, Visibility toolVisibility = System.Windows.Visibility.Visible, object CommandParameter = null)
        {
            //no tool bar to add to in this View type
        }

        private async void xSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(xSearchTextBox.Text))
            {
                await SearchAsync();
            }
        }


        private async Task SearchAsync()
        {

            if (string.IsNullOrEmpty(mSearchString))
            {
                return;
            }

            if (mSearchTask?.IsCanceled == false && mSearchTask?.IsCompleted == false)
            {
                //Cancel if previous search is running 
                await CancelSearchAsync();
            }

            xSearchBtn.Visibility = Visibility.Collapsed;
            xSearchClearBtn.Visibility = Visibility.Visible;
            mCancellationTokenSource = new CancellationTokenSource();
            mSearchTask = new Task(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        mCancellationTokenSource.Token.ThrowIfCancellationRequested();

                        if (SearchStarted == null)
                        {
                            //If event is not hooked we say searching status on main window
                            Reporter.ToStatus(eStatusMsgKey.Search, null, ": " + mSearchString);
                        }
                        else
                        {
                            //If event is hookded then no point in showing status on main window. 
                            //child window need to handle it in the window. E.g. Windows Explorer
                            SearchStarted.Invoke(Tree, new EventArgs());
                        }
                        Mouse.OverrideCursor = Cursors.Wait;
                        xTreeViewTree.FilterItemsByText(xTreeViewTree.TreeItemsCollection, mSearchString, mCancellationTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to search : ", ex);
                    }
                    finally
                    {

                        if (SearchStarted == null)
                        {
                            Reporter.HideStatusMessage();
                        }
                        else
                        {
                            SearchCompleted.Invoke(Tree, new EventArgs());
                        }

                        Mouse.OverrideCursor = null;
                        mCancellationTokenSource.Dispose();
                    }
                });
            }, mCancellationTokenSource.Token, TaskCreationOptions.LongRunning);

            mSearchTask.Start();

        }

        public async Task CancelSearchAsync()
        {

            mCancellationTokenSource?.Cancel();
            Stopwatch st = new Stopwatch();
            st.Start();
            while (mSearchTask.IsCompleted == false && mSearchTask.IsCanceled == false && mSearchTask.IsFaulted == false)
            {
                await Task.Delay(1000);
                if (st.ElapsedMilliseconds > 5000)
                {
                    break;
                }
            }

            mCancellationTokenSource?.Dispose();
            mSearchTask = null;
            if (SearchCancelled != null)
            {
                SearchCancelled.Invoke(Tree, new EventArgs());
            }
            else
            {
                Reporter.HideStatusMessage();
            }
            Mouse.OverrideCursor = null;
        }

        public void SetAddButtonToArrow()
        {
            this.xAddButton.ButtonImageType = eImageType.MoveLeft;
        }
    }
}