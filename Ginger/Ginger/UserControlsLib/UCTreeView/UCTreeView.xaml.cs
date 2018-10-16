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

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Amdocs.Ginger.Repository;
using GingerWPF.DragDropLib;
using System.Reflection;
using System.Linq;
using Amdocs.Ginger.Repository;

namespace GingerWPF.UserControlsLib.UCTreeView
{
    /// <summary>
    /// Interaction logic for UCTreeView.xaml
    /// </summary>
    public partial class UCTreeView : UserControl, IDragDrop
    {
        public event EventHandler ItemSelected;
        public event EventHandler ItemDoubleClick;
        public event EventHandler ItemDropped;
        public delegate void ItemDroppedEventHandler(DragInfo DI);
        public bool TreeItemDoubleClicked = false;
        public bool TreeChildFolderOnly { get; set; }

        public Tuple<string, string> TreeNodesFilterByField { get; set; } 

        
        private TreeViewItem mlastSelectedTVI;

        public TreeViewItem  MlastSelectedTVI
        {
            get { return mlastSelectedTVI; }
            set { mlastSelectedTVI = value; }

        }


        public ItemCollection TreeItemsCollection
        {
            get { return Tree.Items; }
        }

        public ITreeViewItem CurrentSelectedTreeViewItem
        {
            get
            {
                if (Tree.SelectedItem != null && ((TreeViewItem)Tree.SelectedItem).Tag is ITreeViewItem)
                    return (ITreeViewItem)((TreeViewItem)Tree.SelectedItem).Tag;
                else
                    return null;
            }
        }
       
        public void ClearTreeItems()
        {
            Tree.Items.Clear();
        }

        public UCTreeView()
        {
            InitializeComponent();

            Tree.SelectedItemChanged += Tree_SelectedItemChanged;
            Tree.PreviewMouseRightButtonDown += Tree_PreviewMouseRightButtonDown;
            Tree.MouseDoubleClick += Tree_MouseDoubleClick;

            //Hook Drag Drop handler
            //TODO: add flag to decide if Drag and drop is needed
            DragDrop2.HookEventHandlers(this);
        }
        
        private void Tree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeItemDoubleClicked = true;
            if (ItemDoubleClick != null)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    ItemDoubleClick(Tree.SelectedItem, e);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void Tree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Create hot menu for item right click
            DependencyObject obj = e.OriginalSource as DependencyObject;
            TreeViewItem item = (TreeViewItem)TreeViewUtils.GetDependencyObjectFromVisualTree(obj, typeof(TreeViewItem));

            if (item == null)
            {
                e.Handled = true;   
            }
            else
            {
                item.Focus();
                ITreeViewItem o = (ITreeViewItem)item.Tag;
                item.ContextMenu = o.Menu();                
                if (item.ContextMenu == null)
                {
                    e.Handled = true;   
                }
            }
        }

        private void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (Tree.SelectedItem != null)
            {
                if (mlastSelectedTVI != null)
                    mlastSelectedTVI.Header = SetUnSelectedTreeNodeHeaderStyle((StackPanel)mlastSelectedTVI.Header);
                ((TreeViewItem)Tree.SelectedItem).Header= SetSelectedTreeNodeHeaderStyle((StackPanel)((TreeViewItem)Tree.SelectedItem).Header);

                //Check if there is event hooked
                if (ItemSelected != null)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {                        
                        ItemSelected(Tree.SelectedItem, e);                        
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                }

                mlastSelectedTVI = (TreeViewItem)Tree.SelectedItem;
            }
        }
        
        // TODO: remove temp code after cleanup 
        public TreeViewItem AddItem(ITreeViewItem item, TreeViewItem Parent = null)
        {
            TreeViewItem TVI = new TreeViewItem();
            TVI.Tag = item;
                TVI.Header = item.Header();
                if (Parent == null)
                {
                    Tree.Items.Add(TVI);
                }
                else
                {
                    Parent.Items.Add(TVI);
                }

                if (item.IsExpandable())
                {
                    TVI.Expanded += TVI_Expanded;
                    TVI.Collapsed += TVI_Collapsed;

                TreeViewItem TVDummy = new TreeViewItem() { Header = "DUMMY" };
                    TVI.Items.Add(TVDummy);
                }
                            
            return TVI;
        }

       
        private void TVI_Expanded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            TreeViewItem TVI = (TreeViewItem)e.Source;
            RemoveDummyNode(TVI);
            SetRepositoryFolderIsExpanded(TVI, true);
            SetTreeNodeItemChilds(TVI);

            // remove the handler as expand data is cached now on tree
            TVI.Expanded -= TVI_Expanded;
            TVI.Expanded += TVI_ExtraExpanded;

            Mouse.OverrideCursor = null;
        }

        private void TVI_Collapsed(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)e.Source;
            SetRepositoryFolderIsExpanded(tvi, false);
        }

        private void TVI_ExtraExpanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)e.Source;
            SetRepositoryFolderIsExpanded(tvi, true);
        }

        private void SetRepositoryFolderIsExpanded(TreeViewItem tvi, bool isExpanded)
        {
            ITreeViewItem itvi = (ITreeViewItem)tvi.Tag;
            object itviObject = itvi.NodeObject();
            if (itviObject is RepositoryFolderBase)
            { 
                ((RepositoryFolderBase)itviObject).IsFolderExpanded = isExpanded;
            }
        }

        private void SetTreeNodeItemChilds(TreeViewItem TVI)
        {
            // TODO: remove temp code after cleanup 
            if (TVI.Tag is ITreeViewItem)
            {
                ITreeViewItem ITVI = (ITreeViewItem)TVI.Tag;

                List<ITreeViewItem> Childs = null;
                Childs = ITVI.Childrens();
                    
                TVI.Items.Clear();
                if (Childs != null)
                {
                    foreach (ITreeViewItem item in Childs)
                    {
                        if (TreeChildFolderOnly == true && item.IsExpandable() == false)
                        {
                            continue;
                        }
                        if (TreeNodesFilterByField != null)
                        {
                            if (IsTreeItemFitsFilter(item))
                            {
                                AddItem(item, TVI);
                            }
                        }
                        else
                        {
                            AddItem(item, TVI);
                        }

                    }
                }
            }
        }

        private bool IsTreeItemFitsFilter(ITreeViewItem treeItemToCheck)
        {
            object treeItemToCheckObject = treeItemToCheck.NodeObject();
            if (treeItemToCheckObject is RepositoryFolderBase)
            {
                return true;
            }            
                
            //get the object to filter by
            List<string> filterByfieldHierarchyList = TreeNodesFilterByField.Item1.ToString().Split('.').ToList();
            object filterByObject = treeItemToCheckObject;
            foreach (string hierarchyElement in filterByfieldHierarchyList)
            {
                PropertyInfo pInfo = filterByObject.GetType().GetProperty(hierarchyElement);
                if (pInfo is null)
                {
                    break;
                }
                else
                {
                    filterByObject = pInfo.GetValue(filterByObject, null);
                }
            }

            //compare the value
            string filterbyValue = TreeNodesFilterByField.Item2.ToString();
            if (filterbyValue == filterByObject.ToString())
                return true;

            return false;
        }

        private void RemoveDummyNode(TreeViewItem node)
        {
            if (node.Items.Count > 0)
            {
                if (((TreeViewItem)node.Items[0]).Header.ToString().IndexOf("DUMMY") >= 0)
                {
                    node.Items.Clear();
                }
            }
        }

        public void RefreshSelectedTreeNodeChildrens(object sender, System.Windows.RoutedEventArgs e)
        {
            RefreshSelectedTreeNodeChildrens();
        }

        public void RefreshSelectedTreeNodeChildrens()
        {
            TreeViewItem TVI = (TreeViewItem)Tree.SelectedItem;
            if (TVI != null)
            {
                TVI.Items.Clear();
                SetTreeNodeItemChilds(TVI);
                TVI.IsExpanded = true;
            }
        }

        public object GetItemAt(int indx)
        {
            return Tree.Items.GetItemAt(indx);
        }

        public void RefreshTreeViewItemChildrens(TreeViewItem TVI)
        {
            TVI.Items.Clear();
            SetTreeNodeItemChilds(TVI);
            TVI.IsExpanded = true;
        }

        public void RefresTreeNodeChildrens(ITreeViewItem NodeItem)
        {
            TreeViewItem TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], NodeItem);
            if (TVI != null)
            {
                TVI.Items.Clear();
                SetTreeNodeItemChilds(TVI);
                TVI.IsExpanded = true;
            }
        }

        public void RefreshSelectedTreeNodeParent()
        {
            TreeViewItem TVI = null;
            if (((TreeViewItem)Tree.SelectedItem).Parent is TreeView)
                TVI = (TreeViewItem)((TreeViewItem)Tree.SelectedItem);
            else
                TVI = (TreeViewItem)((TreeViewItem)Tree.SelectedItem).Parent;
            TVI.Items.Clear();
            SetTreeNodeItemChilds(TVI);
            TVI.IsExpanded = true;
        }

        public void ExpandTreeNodeByName(string NodeName, bool Refresh = false, bool ExpandAll = false)
        {
            //Expand and Refresh and if needed expand all children by searching Node by name
            ExpandNodeByNameTVIRecursive((TreeViewItem)Tree.Items[0], NodeName, Refresh, ExpandAll);
        }

        public void RefreshTreeNodeParent(ITreeViewItem NodeItem, bool expandAfterRefresh = true)
        {
            TreeViewItem TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], NodeItem);
            if (TVI != null)
            {
                TreeViewItem TVIparent = (TreeViewItem)(TVI.Parent);
                TVIparent.Items.Clear();
                SetTreeNodeItemChilds(TVIparent);
                TVIparent.IsExpanded = expandAfterRefresh;
            }
        }

        public ITreeViewItem GetRootItem()
        {
            if (Tree.Items.Count > 0)
                return (ITreeViewItem)((TreeViewItem)Tree.Items[0]).Tag;
            else
                return null;
        }

        public List<ITreeViewItem> GetTreeNodeChilds(ITreeViewItem treeNode)
        {
            List<ITreeViewItem> childs = new List<ITreeViewItem>();
            TreeViewItem TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], treeNode);
            if (TVI != null)
            {
                GetNodeChilds(TVI, childs, false);
            }
            return childs;
        }

        public List<ITreeViewItem> GetTreeNodeChildsIncludingSubChilds(ITreeViewItem treeNode)
        {
            List<ITreeViewItem> childs = new List<ITreeViewItem>();
            TreeViewItem TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], treeNode);
            if (TVI != null)
            {
                GetNodeChilds(TVI, childs, true);
            }
            return childs;
        }

        private void GetNodeChilds(TreeViewItem TVI, List<ITreeViewItem> childsList, bool isRecursive)
        {
            if (TVI == null) return;
            if (TVI.Items.Count == 0 || (TVI.Items.Count == 1 && ((TreeViewItem)TVI.Items[0]).Tag == null))
                SetTreeNodeItemChilds(TVI);
            foreach (TreeViewItem childTVI in TVI.Items)
            {
                if (childTVI.Tag != null)
                {
                    childsList.Add((ITreeViewItem)childTVI.Tag);
                    if (isRecursive)
                        GetNodeChilds(childTVI, childsList, true);
                }
            }
        }

        public string GetSelectedTreeNodeName()
        {
            TreeViewItem TVI = (TreeViewItem)Tree.SelectedItem;
            if (TVI != null)
            {
                 foreach (object ctrl in ((StackPanel)TVI.Header).Children)
                    if (ctrl.GetType() == typeof(Label))
                        if (ctrl != null)
                            return ((Label)ctrl).Content.ToString();
                        else
                            return "";
            }

            return string.Empty;
        }

        private StackPanel SetSelectedTreeNodeHeaderStyle(StackPanel header)
        {
            if (header != null)
            {
                foreach (object ctrl in header.Children)
                    if (ctrl.GetType() == typeof(Label))
                    {
                        if (ctrl != null)
                        {                           
                            ((Label)ctrl).Foreground = FindResource("$SelectionColor_Pink") as Brush;
                            ((Label)ctrl).FontWeight = FontWeights.Bold;
                            break;
                        }
                    }                
            }

            return header;
        }

        private StackPanel SetUnSelectedTreeNodeHeaderStyle(StackPanel header)
        {
            if (header != null)
            {
                foreach (object ctrl in header.Children)
                    if (ctrl.GetType() == typeof(Label))
                    {
                        if (ctrl != null)
                        {
                            ((Label)ctrl).Foreground = Brushes.Black;
                            ((Label)ctrl).FontWeight = FontWeights.Normal;
                            break;
                        }
                    }
            }

            return header;
        }

        /// <summary>
        /// Recursive method to go over all tree nodes
        /// </summary>
        /// <param name="itemCollection"></param>
        /// <param name="txt"></param>
        public void FilterItemsByText(ItemCollection itemCollection, string txt)
        {
            // Filter not working for new TVI            
            foreach (TreeViewItem tvi in itemCollection)
            {
                // Need to expand to get all lazy loading
                tvi.IsExpanded = true;

                ITreeViewItem ITVI = (ITreeViewItem)tvi.Tag;

                // Find the label in the header, this is label child of the Header Stack Panel
                StackPanel SP = (StackPanel)tvi.Header;                     

                //Ccombine text of all label childs of the header Stack panel
                string HeaderTXT = "";
                foreach (var v in SP.Children)
                {
                    if (v.GetType() == typeof(Label))
                    {
                        Label l = (Label)v;
                        if (l.Content != null)
                        {
                            HeaderTXT += l.Content.ToString();
                        }
                    }
                }

                bool bFound = HeaderTXT.ToUpper().Contains(txt.ToUpper());
                if (bFound || txt.Length == 0)
                {
                    tvi.Visibility = System.Windows.Visibility.Visible;

                    // go over all parents to make them visible
                    TreeViewItem tviParent = tvi;
                    while (tviParent.Parent is TreeViewItem)
                    {
                        tviParent = (TreeViewItem)tviParent.Parent;
                        tviParent.Visibility = System.Windows.Visibility.Visible;
                    }                    
                }
                else
                {
                    tvi.Visibility = System.Windows.Visibility.Collapsed;                    
                }

                // Goto sub items
                if (tvi.HasItems)
                {                    
                    FilterItemsByText(tvi.Items, txt);
                }
            }
            //Show the root item
                ((TreeViewItem)Tree.Items[0]).Visibility = System.Windows.Visibility.Visible;
            
        }       
                
        public void SetBtnImage(Button btn, string imageName)
        {
            Image image = new Image();
            image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + imageName));
            btn.Content = image;
        }

        public void ExpandTreeItem(ITreeViewItem treeItem)
        {
            TreeViewItem TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], treeItem);
            if (TVI != null)
            {
                TVI.IsExpanded = true;
                GingerCore.General.DoEvents();
            }            
        }


        public ITreeViewItem AddChildItemAndSelect(ITreeViewItem Parent, ITreeViewItem Child)
        {
            TreeViewItem TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], Parent);
            if (TVI != null)
            {                
                TVI.IsExpanded = true;             
                GingerCore.General.DoEvents();
            }
            
            TreeViewItem TVIChild = AddItem(Child, TVI);
            TVIChild.Focus();

            return (ITreeViewItem)TVIChild.Tag;
        }

        public TreeViewItem FindMatchingTreeItem(ITreeViewItem item)
        {
            TreeViewItem TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], item);
            return TVI;
        }

        public TreeViewItem FindMatchingTreeItemByObject(TreeViewItem root, Object searchItem)
        {
            Object currentItem = null;

            foreach (TreeViewItem TVI in root.Items)
            {
                ITreeViewItem o = (ITreeViewItem)TVI.Tag;
                if (o != null)
                {
                    currentItem = o.NodeObject();

                    if (currentItem.Equals(searchItem))
                    {
                        return TVI;
                    }
                }
                if (TVI.Items.Count > 0)
                {
                    TVI.IsExpanded = true;
                    TreeViewItem vv = FindMatchingTreeItemByObject(TVI, searchItem);
                    if (vv != null) return vv;
                }
            }
            return null;
        }

        public TreeViewItem FindMatchingTreeItemByType(TreeViewItem root, Type searchItem)
        {
            Object currentItem = null;

            foreach (TreeViewItem TVI in root.Items)
            {
                ITreeViewItem o = (ITreeViewItem)TVI.Tag;
                if (o != null)
                {
                    currentItem = o.GetType();

                    if (currentItem == searchItem)
                    {
                        return TVI;
                    }
                }
                if (TVI.Items.Count > 0)
                {
                    TVI.IsExpanded = true;
                    TreeViewItem vv = FindMatchingTreeItemByType(TVI, searchItem);
                    if (vv != null) return vv;
                }
            }
            return null;
        }

        public void SelectItem(ITreeViewItem item)
        {
            TreeViewItem TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], item);
            TVI.IsExpanded = true;
            GingerCore.General.DoEvents();
            TVI.Focus();
        }

        public ITreeViewItem GetChildItembyNameandSelect(string nodeName, ITreeViewItem Parent = null)
        {
            TreeViewItem TVI = (TreeViewItem)Tree.Items[0];
            if (Parent != null)
            {
                TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], Parent);
                TVI.IsExpanded = true;
                GingerCore.General.DoEvents();
            }

            TreeViewItem TVIChild = ExpandNodeByNameTVIRecursive(TVI, nodeName, true, false);
            TVIChild.Focus();

            return (ITreeViewItem)TVIChild.Tag;
        }

        public void SelectFirstVisibleChildItem(ItemCollection treeItems, List<Type> childTypes)
        {
            foreach (TreeViewItem item in treeItems)
                if (item.Visibility == Visibility.Visible)
                {
                    if (childTypes.Contains(item.Tag.GetType()))
                    {
                        item.Focus();
                        return;
                    }

                    if (item.HasItems)
                        SelectFirstVisibleChildItem(item.Items, childTypes);
                }
        }

        public void DeleteItemAndSelectParent(ITreeViewItem NodeItem)
        {
            TreeViewItem TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], NodeItem);
            TreeViewItem parent = (TreeViewItem)TVI.Parent;
            parent.Items.Remove(TVI);
            parent.Focus();
        }

        public void DeleteItemByObjectAndSelectParent(object itemObject, ITreeViewItem parentItem = null)
        {                                  
                TreeViewItem parentTVI;
                if (parentItem != null)
                    parentTVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], parentItem);
                else
                    parentTVI = (TreeViewItem)Tree.Items[0];
                if (parentTVI != null)
                {
                    TreeViewItem toDeleteTVI = SearchTVIByObjectRecursive(parentTVI, itemObject);
                    TreeViewItem parent = (TreeViewItem)toDeleteTVI.Parent;
                
                    parent.Items.Remove(toDeleteTVI);
                    parent.Focus();
                }         
        }

        public void SelectParentItem(ITreeViewItem NodeItem)
        {
            TreeViewItem TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], NodeItem);
            if (TVI != null)
                if (TVI.Equals((TreeViewItem)Tree.Items[0]) == false)
                {
                    TreeViewItem parent = (TreeViewItem)TVI.Parent;
                    parent.Focus();
                }
                else
                {
                    TVI.Focus();
                }
        }

        public void RefreshHeader(ITreeViewItem NodeItem)
        {
            TreeViewItem TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], NodeItem);
            if (TVI != null)
            {
                ITreeViewItem ITVI = (ITreeViewItem)TVI.Tag;
                TVI.Header = ITVI.Header();
                if (Tree.SelectedItem == TVI)
                    TVI.Header = SetSelectedTreeNodeHeaderStyle((StackPanel)TVI.Header);
            }
        }

        public void RefreshHeader(TreeViewItem TVI)
        {
            if (TVI != null)
            {
                ITreeViewItem ITVI = (ITreeViewItem)TVI.Tag;
                TVI.Header = ITVI.Header();
                if (Tree.SelectedItem == TVI)
                    TVI.Header = SetSelectedTreeNodeHeaderStyle((StackPanel)TVI.Header);
            }
        }

        private TreeViewItem SearchTVIRecursive(TreeViewItem node, ITreeViewItem SearchedNode)
        {
            ITreeViewItem o = null;
            // can be triggered by tree change of FileWathcher which is on another thread, so running on dispatcher            
                o = (ITreeViewItem)node.Tag;            
            if (o != null && o.Equals(SearchedNode))            
                return node;

            foreach (TreeViewItem TVI in node.Items)
            {
                TreeViewItem vv = SearchTVIRecursive(TVI, SearchedNode);
                if (vv != null)
                    return vv;
            }
            return null;
        }

        public TreeViewItem SearchTVIByObjectRecursive(TreeViewItem parentNode, object treeNodeObject)
        {
            TreeViewItem TVI = null;            
                TVI = SearchTVIByObjectRecursive2(parentNode, treeNodeObject);

            return TVI;
        }

        private TreeViewItem SearchTVIByObjectRecursive2(TreeViewItem parentNode, object treeNodeObject)
        {
            if (parentNode == null)
            {
                parentNode = (TreeViewItem)Tree.Items[0];
            }

            foreach (TreeViewItem child in parentNode.Items)
            {
                if (child.Header.ToString() != "DUMMY") //added for stability bc sometimes i was getting issues
                {
                    if (((ITreeViewItem)child.Tag).NodeObject().Equals(treeNodeObject))
                        return child;
                    else
                    {
                        foreach (TreeViewItem subChild in child.Items)
                        {
                            TreeViewItem foundItem = SearchTVIByObjectRecursive(subChild, treeNodeObject);
                            if (foundItem != null)
                                return foundItem;
                        }
                    }
                }
            }

            return null;
        }

            private TreeViewItem ExpandNodeByNameTVIRecursive(TreeViewItem StartNode, string NodeName, bool Refresh, bool ExpandChildren)
        {
            foreach (TreeViewItem TVI in StartNode.Items)
            {
                StackPanel head = null;
                string o = null;
                if (TVI.Header.ToString() != "DUMMY") //added for stability bc sometimes i was getting issues
                {
                    head = (StackPanel)TVI.Header;
                    foreach (var child in head.Children)
                    {
                        if (child is Label)
                        {
                            o = (string)((Label)child).Content.ToString();
                            break;
                        }
                    }
                }

                // If searched Node is found by name, refresh and expand
                if (o != null && o == NodeName)
                {
                    if (Refresh)
                    {
                        TVI.IsSelected = true;
                        RefreshTreeViewItemChildrens(TVI);
                    }
                    if (ExpandChildren)
                    {
                        TVI.IsExpanded = true;
                        TVI.ExpandSubtree();
                    }
                    else
                    {
                        TVI.IsExpanded = true;
                    }
                    return TVI;
                }

                if (TVI.Items.Count > 0)
                {
                    TreeViewItem vv = ExpandNodeByNameTVIRecursive(TVI, NodeName, Refresh, ExpandChildren);
                    if (vv != null) return vv;
                }
            }
            return null;
        }

        // DragDrop handlers

        void IDragDrop.StartDrag(DragInfo Info)
        {
            TreeViewItem item = TreeViewItemFromUIElement((UIElement)Info.OriginalSource);

            if (item != null)
            {
                Info.DragSource = this;
                Info.Data = ((ITreeViewItem)item.Tag).NodeObject();

                if (Info.Data == null)
                {
                    return; // no drag info = no drag                
                }
                // Each item which can be dragged should override ToString - so there will be nice text dragging header
                Info.Header = ((ITreeViewItem)item.Tag).NodeObject().ToString();
            }
        }

        TreeViewItem TreeViewItemFromUIElement(UIElement element)
        {
            UIElement retVal = element;

            while ((retVal != null) && !(retVal is TreeViewItem)) retVal = VisualTreeHelper.GetParent(retVal) as UIElement;

            return retVal as TreeViewItem;
        }

        void IDragDrop.DragOver(DragInfo Info)
        {

        }

        void IDragDrop.DragEnter(DragInfo Info)
        {
            Info.DragTarget = this;
        }

        void IDragDrop.Drop(DragInfo Info)
        {
            EventHandler handler = ItemDropped;
            if (handler != null)
            {
                handler(Info, new EventArgs());
            }

            // TODO: if in same grid then do move, 
        }
    }
}