#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.Help;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.UserControlsLib.UCTreeView
{
    public class SelectionTreeEventArgs : EventArgs
    {
        List<object> mSelectedItems;
        public List<object> SelectedItems
        {
            get { return mSelectedItems; }
            set { this.mSelectedItems = value; }
        }

        // Constructor.
        public SelectionTreeEventArgs(List<object> selectedItems)
        {
            this.mSelectedItems = selectedItems;
        }       
    }

    public delegate void SelectionTreeEventHandler(object sender, SelectionTreeEventArgs e);

    /// <summary>
    /// Interaction logic for SingleItemTreeViewSelectionPage.xaml
    /// </summary>
    public partial class SingleItemTreeViewSelectionPage : Page
    {
        public enum eItemSelectionType
        {
            Single, Multi, MultiStayOpenOnDoubleClick, Folder
        }
       
        eItemSelectionType mItemSelectionType;        
        GenericWindow mPageGenericWin = null;
        List<object> mSelectedItems = null;
        bool bOpenasWindow = false;
        string mitemTypeName;
        public event SelectionTreeEventHandler SelectionDone;
        public event SelectionTreeEventHandler OnSelect;

        bool mShowAlerts;

        protected virtual void OnSelectionDone(SelectionTreeEventArgs e)
        {
            if (SelectionDone != null)
                SelectionDone(this, e);
        }

        protected virtual void OnSelectItem(SelectionTreeEventArgs e)
        {
            if (OnSelect != null)
            { 
                OnSelect(this, e);
            }
        }


        public TreeView1 TreeView
        {
            get { return xTreeView; }
        }

        public SingleItemTreeViewSelectionPage(string itemTypeName, eImageType itemTypeIcon, ITreeViewItem itemTypeRootNode, eItemSelectionType itemSelectionType = eItemSelectionType.Single, bool allowTreeTools = false, Tuple<string, string> propertyValueFilter = null, UCTreeView.eFilteroperationType filterType = UCTreeView.eFilteroperationType.Equals, bool showAlerts = true)
        {
            InitializeComponent();

            GingerHelpProvider.SetHelpString(this, itemTypeName.TrimEnd(new char[] { 's' }));

            xTreeView.Tree.TreeNodesFilterByField = propertyValueFilter;
            xTreeView.Tree.FilterType = filterType;
            xTreeView.AllowTreeTools = allowTreeTools;
            if(itemSelectionType == eItemSelectionType.Folder)
            {
                xTreeView.Tree.TreeChildFolderOnly = true;                
            }

            TreeViewItem r = xTreeView.Tree.AddItem(itemTypeRootNode);            
            r.IsExpanded = true;

            xTreeView.Tree.ItemDoubleClick += Tree_ItemDoubleClick;
            xTreeView.Tree.ItemSelected += Tree_ItemSelected;

            mitemTypeName = itemTypeName;
            xTreeView.TreeTitle = itemTypeName;
            xTreeView.TreeIcon = itemTypeIcon;

            mItemSelectionType = itemSelectionType;
            if (mItemSelectionType == eItemSelectionType.MultiStayOpenOnDoubleClick)
                xTipLabel.Visibility = Visibility.Visible;
            else
                xTipLabel.Visibility = Visibility.Collapsed;

            mShowAlerts = showAlerts;
        }        

        public List<object> ShowAsWindow(string windowTitle="", Window ownerWindow = null, eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset = false)
        {
            bOpenasWindow = true;
            ObservableList<Button> winButtons = new ObservableList<Button>();
            
            Button selectBtn = new Button();
            selectBtn.Content = "Select";
            selectBtn.Click += new RoutedEventHandler(selectBtn_Click);
            winButtons.Add(selectBtn);

            if (windowTitle == string.Empty)            
                windowTitle = mitemTypeName + " Selection";

            if (ownerWindow == null)
            {
                ownerWindow = App.MainWindow;
            }
              
            GenericWindow.LoadGenericWindow(ref mPageGenericWin, ownerWindow, windowStyle, windowTitle, this, winButtons, true, "Close", CloseWinClicked,
                                                    startupLocationWithOffset: startupLocationWithOffset);
            
            return mSelectedItems;
        }

        private void CloseWinClicked(object sender, EventArgs e)
        {
            mSelectedItems = null;
            mPageGenericWin.Close();
        }

        private bool SelectCurrentItem()
        {
            if (bOpenasWindow == true)
            { 
                return true;
            }

            ITreeViewItem itvItem = xTreeView.Tree.CurrentSelectedTreeViewItem;

            if (itvItem != null &&  mItemSelectionType != eItemSelectionType.Folder)
            {
                mSelectedItems = new List<object>();
                if (itvItem.IsExpandable())
                {
                    if (mShowAlerts && mItemSelectionType == eItemSelectionType.Single)
                    {                        
                        Reporter.ToUser(eUserMsgKey.ItemSelection, "Please select single node item (not a folder).");
                        return false;
                    }                      
                    
                        //get all children's objects of direct and sub folders
                    foreach (ITreeViewItem subItvItem in xTreeView.Tree.GetTreeNodeChildsIncludingSubChilds(itvItem))
                        if (subItvItem.NodeObject() != null && subItvItem.NodeObject().GetType().BaseType != typeof(RepositoryFolderBase))
                            mSelectedItems.Add(subItvItem.NodeObject());                                        
                }
                else
                {
                    mSelectedItems.Add(itvItem.NodeObject());
                }
            }


            if (mShowAlerts && (mSelectedItems == null || mSelectedItems.Count == 0))
            {                
                Reporter.ToUser(eUserMsgKey.ItemSelection, "No item was selected.");
                return false;
            }

            return true;
        }

        private void selectBtn_Click(object sender, RoutedEventArgs e)
        {
            bOpenasWindow = false;
            if (SelectCurrentItem())
            {
                if (mPageGenericWin != null)
                {
                    mPageGenericWin.Close();
                }
            }
            bOpenasWindow = true;
        }

        private void Tree_ItemDoubleClick(object sender, EventArgs e)
        {
            bOpenasWindow = false;
            if (SelectCurrentItem())
            {
                if (mItemSelectionType == eItemSelectionType.MultiStayOpenOnDoubleClick)
                {
                    OnSelectionDone(new SelectionTreeEventArgs(mSelectedItems));
                    mSelectedItems.Clear();
                }
                else if (mPageGenericWin != null)
                {
                    mPageGenericWin.Close();
                }
            }
            bOpenasWindow = true;
        }
        private void Tree_ItemSelected(object sender, EventArgs e)
        {
            mSelectedItems = new List<object>();
            ITreeViewItem itvItem = xTreeView.Tree.CurrentSelectedTreeViewItem;            
            if(mItemSelectionType == eItemSelectionType.Folder)
            { 
                mSelectedItems.Add(itvItem);
            }
            else
            { 
                mSelectedItems.Add(itvItem.NodeObject());
            }

            if (SelectCurrentItem())
            {
                OnSelectItem(new SelectionTreeEventArgs(mSelectedItems));
            }
        }
    }
}
