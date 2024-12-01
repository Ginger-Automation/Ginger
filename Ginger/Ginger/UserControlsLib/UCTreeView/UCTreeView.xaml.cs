#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Repository;
using GingerWPF.DragDropLib;
using GingerWPF.TreeViewItemsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        public event EventHandler ItemAdded;
        public delegate void ItemDroppedEventHandler(DragInfo DI);
        public bool TreeItemDoubleClicked = false;
        public bool TreeChildFolderOnly { get; set; }

        public Tuple<string, string> TreeNodesFilterByField { get; set; }

        eFilteroperationType mFilterType = eFilteroperationType.Equals;
        public eFilteroperationType FilterType
        {
            get { return mFilterType; }
            set { mFilterType = value; }
        }

        public enum eFilteroperationType
        {
            Equals,
            Contains
        }


        public TreeViewItem LastSelectedTVI
        {
            get; set;

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
                {
                    return (ITreeViewItem)((TreeViewItem)Tree.SelectedItem).Tag;
                }
                else
                {
                    return null;
                }
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


            //Hook Drag Drop handler
            //TODO: add flag to decide if Drag and drop is needed          

        }

        bool mEnableRightClick = true;
        public bool EnableRightClick
        {
            get
            {
                return mEnableRightClick;
            }
            set
            {
                if (value)
                {
                    Tree.PreviewMouseRightButtonDown += Tree_PreviewMouseRightButtonDown;
                }
                else
                {
                    Tree.PreviewMouseRightButtonDown -= Tree_PreviewMouseRightButtonDown;
                }
                mEnableRightClick = value;
            }
        }

        bool mEnableDragDrop = true;
        public bool EnableDragDrop
        {
            get
            {
                return mEnableDragDrop;
            }
            set
            {
                if (value)
                {
                    DragDrop2.HookEventHandlers(this);
                }
                else
                {
                    DragDrop2.UnHookEventHandlers(this);
                }
                mEnableDragDrop = value;
            }
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeItemDoubleClicked = true;
            if (ItemDoubleClick != null)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    if (e.Source is TreeViewItem && (e.Source as TreeViewItem).IsSelected)
                    {
                        ItemDoubleClick(Tree.SelectedItem, e);
                    }
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                    e.Handled = true;
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
                if (LastSelectedTVI != null)
                {
                    LastSelectedTVI.Header = SetUnSelectedTreeNodeHeaderStyle((StackPanel)LastSelectedTVI.Header);
                } ((TreeViewItem)Tree.SelectedItem).Header = SetSelectedTreeNodeHeaderStyle((StackPanel)((TreeViewItem)Tree.SelectedItem).Header);

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

                LastSelectedTVI = (TreeViewItem)Tree.SelectedItem;
            }
        }

        // TODO: remove temp code after cleanup 
        public TreeViewItem AddItem(ITreeViewItem item, TreeViewItem Parent = null)
        {
            TreeViewItem TVI = new()
            {
                Tag = item,
                Header = item.Header()
            };
            if (item is NewTreeViewItemBase newTreeViewItem)
            {
                newTreeViewItem.TreeViewItem = TVI;
                Binding visibilityBinding = new()
                {
                    Mode = BindingMode.TwoWay,
                    Source = item,
                    Path = new PropertyPath(nameof(NewTreeViewItemBase.Visibility)),
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                };
                TVI.SetBinding(TreeViewItem.VisibilityProperty, visibilityBinding);
            }
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
                WeakEventManager<TreeViewItem, RoutedEventArgs>.AddHandler(source: TVI, eventName: nameof(TreeViewItem.Expanded), handler: TVI_Expanded);
                WeakEventManager<TreeViewItem, RoutedEventArgs>.AddHandler(source: TVI, eventName: nameof(TreeViewItem.Collapsed), handler: TVI_Collapsed);

                TreeViewItem TVDummy = new TreeViewItem() { Header = "DUMMY" };
                TVI.Items.Add(TVDummy);
            }


            ItemAdded?.Invoke(item, null);

            return TVI;
        }


        private void TVI_Expanded(object? sender, RoutedEventArgs e)
        {
            TreeViewItem treeViewItem = (TreeViewItem)e.Source;
            Mouse.OverrideCursor = Cursors.Wait;
            _ = LoadChildItems(treeViewItem);
            Mouse.OverrideCursor = null;
        }

        private async Task LoadChildItems(TreeViewItem treeViewItem)
        {
            bool hadDummyNode = TryRemoveDummyNode(treeViewItem);
            if (hadDummyNode)
            {
                SetRepositoryFolderIsExpanded(treeViewItem, isExpanded: true);
                await SetTreeNodeItemChilds(treeViewItem);
                GingerCore.General.DoEvents();
                // remove the handler as expand data is cached now on tree
                WeakEventManager<TreeViewItem, RoutedEventArgs>.RemoveHandler(treeViewItem, nameof(TreeViewItem.Expanded), TVI_Expanded);
                WeakEventManager<TreeViewItem, RoutedEventArgs>.AddHandler(treeViewItem, nameof(TreeViewItem.Expanded), TVI_ExtraExpanded);
            }
        }

        private void TVI_Collapsed(object? sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)e.Source;
            SetRepositoryFolderIsExpanded(tvi, false);
        }

        private void TVI_ExtraExpanded(object? sender, RoutedEventArgs e)
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

        private AutoResetEvent? mSetTreeNodeItemChildsEvent = null;

        private readonly Dictionary<TreeViewItem, Task> tviChildNodesLoadTaskMap = [];
        public enum ChildrenLoadState
        {
            Started,
            Completed
        }

        public delegate void ChildrenLoadHandler(ChildrenLoadState state);

        public event ChildrenLoadHandler ChildrenLoadEvent;

        private Task SetTreeNodeItemChilds(TreeViewItem TVI)
        {
            // TODO: remove temp code after cleanup 
            Task setChildItemsTask = Task.CompletedTask;

            if (TVI.Tag is ITreeViewItem ITVI)
            {
                List<ITreeViewItem>? Childs = null;

                var tviChildren = ITVI.Childrens();
                if (tviChildren != null)
                {
                    Childs = new(tviChildren);
                }

                TVI.Items.Clear();
                if (Childs != null)
                {
                    setChildItemsTask = Task.Run(() =>
                    {
                        ChildrenLoadEvent?.Invoke(ChildrenLoadState.Started);
                        try
                        {
                            mSetTreeNodeItemChildsEvent = new AutoResetEvent(false);
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
                                        Dispatcher.Invoke(() => AddItem(item, TVI));
                                    }
                                }
                                else
                                {
                                    Dispatcher.Invoke(() => AddItem(item, TVI));
                                }
                                Thread.Sleep(5);
                            }
                            mSetTreeNodeItemChildsEvent.Set();
                            if (tviChildNodesLoadTaskMap.ContainsKey(TVI))
                            {
                                tviChildNodesLoadTaskMap[TVI] = setChildItemsTask;
                            }
                            else
                            {
                                tviChildNodesLoadTaskMap.Add(TVI, setChildItemsTask);
                            }
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                        }
                        finally
                        {
                            ChildrenLoadEvent?.Invoke(ChildrenLoadState.Completed);
                        }
                    });
                }
            }

            return setChildItemsTask;
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
                PropertyInfo pInfo = null;
                if (filterByObject != null)
                {
                    pInfo = filterByObject.GetType().GetProperty(hierarchyElement);
                }

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
            string filterbyValue = Convert.ToString(TreeNodesFilterByField.Item2);
            if (Convert.ToString(filterByObject) == filterbyValue)
            {
                return true;
            }

            if (FilterType == eFilteroperationType.Contains)
            {
                if (Convert.ToString(filterByObject).Contains(filterbyValue))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryRemoveDummyNode(TreeViewItem node)
        {
            if (node.Items.Count > 0)
            {
                string? header = ((TreeViewItem)node.Items[0]).Header.ToString();
                if (header != null && header.IndexOf("DUMMY") >= 0)
                {
                    node.Items.Clear();
                    return true;
                }
            }
            return false;
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

        public void RefreshTreeNodeChildrens(ITreeViewItem NodeItem)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    TreeViewItem TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], NodeItem);
                    if (TVI != null)
                    {
                        TVI.Items.Clear();
                        TVI.IsExpanded = true;
                        _ = SetTreeNodeItemChilds(TVI);
                    }
                });
            }

            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
        }

        public void RefreshSelectedTreeNodeParent()
        {
            TreeViewItem TVI = null;
            if (((TreeViewItem)Tree.SelectedItem).Parent is TreeView)
            {
                TVI = (TreeViewItem)Tree.SelectedItem;
            }
            else
            {
                TVI = (TreeViewItem)((TreeViewItem)Tree.SelectedItem).Parent;
            }

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
            {
                return (ITreeViewItem)((TreeViewItem)Tree.Items[0]).Tag;
            }
            else
            {
                return null;
            }
        }

        public List<ITreeViewItem> GetTreeNodeChilds(ITreeViewItem treeNode)
        {
            List<ITreeViewItem> childs = [];
            TreeViewItem TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], treeNode);
            if (TVI != null)
            {
                GetNodeChilds(TVI, childs, false);
            }
            return childs;
        }

        public List<ITreeViewItem> GetTreeNodeChildsIncludingSubChilds(ITreeViewItem treeNode)
        {
            List<ITreeViewItem> childs = [];
            TreeViewItem TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], treeNode);
            if (TVI != null)
            {
                GetNodeChilds(TVI, childs, true);
            }
            return childs;
        }

        private void GetNodeChilds(TreeViewItem TVI, List<ITreeViewItem> childsList, bool isRecursive)
        {
            if (TVI == null)
            {
                return;
            }

            if (TVI.Tag is ITreeViewItem tviTreeViewItem)
            {
                GetNodeChilds(tviTreeViewItem, childsList, isRecursive);
                return;
            }

            if (TVI.Items.Count == 0 || (TVI.Items.Count == 1 && ((TreeViewItem)TVI.Items[0]).Tag == null))
            {
                SetTreeNodeItemChilds(TVI);
            }

            foreach (TreeViewItem childTVI in TVI.Items)
            {
                if (childTVI.Tag != null)
                {
                    childsList.Add((ITreeViewItem)childTVI.Tag);
                    if (isRecursive)
                    {
                        GetNodeChilds(childTVI, childsList, true);
                    }
                }
            }
        }

        private void GetNodeChilds(ITreeViewItem TVI, List<ITreeViewItem> childsList, bool isRecursive)
        {
            if (TVI == null)
            {
                return;
            }

            List<ITreeViewItem> children = TVI.Childrens();
            if (children == null)
            {
                return;
            }
            children = new(children);

            foreach (ITreeViewItem childTVI in children)
            {
                childsList.Add(childTVI);
                if (isRecursive)
                {
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
                {
                    if (ctrl.GetType() == typeof(Label))
                    {
                        if (ctrl != null)
                        {
                            return ((Label)ctrl).Content.ToString();
                        }
                        else
                        {
                            return "";
                        }
                    }
                }
            }

            return string.Empty;
        }

        public object GetSelectedTreeNodeObject()
        {
            TreeViewItem TVI = (TreeViewItem)Tree.SelectedItem;
            if (TVI != null && TVI.Tag != null)
            {
                return ((ITreeViewItem)TVI.Tag).NodeObject();
            }

            return null;
        }

        private StackPanel SetSelectedTreeNodeHeaderStyle(StackPanel header)
        {
            if (header != null)
            {
                foreach (object ctrl in header.Children)
                {
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
            }

            return header;
        }

        private StackPanel SetUnSelectedTreeNodeHeaderStyle(StackPanel header)
        {
            if (header != null)
            {
                foreach (object ctrl in header.Children)
                {
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
            }

            return header;
        }

        /// <summary>
        /// Return if the current tree items support new filter by text functionality using <see cref="FilterItemsByTextNew(IEnumerable{ITreeViewItem}, string)"/> method.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if <see cref="FilterItemsByTextNew(IEnumerable{ITreeViewItem}, string)"/> is supported, <see langword="false"/> otherwise.</returns>
        public bool SupportNewFilterMethod()
        {
            if (Tree.Items.Count <= 0 || ((TreeViewItem)Tree.Items[0]).Items == null)
            {
                return false;
            }

            foreach (TreeViewItem tvi in ((TreeViewItem)Tree.Items[0]).Items)
            {
                if (tvi.Tag is null or not ITreeViewItem)
                {
                    return false;
                }

                ITreeViewItem itvi = (ITreeViewItem)tvi.Tag;
                List<ITreeViewItem>? oldChildren = itvi.Childrens();
                List<ITreeViewItem>? newChildren = itvi.Childrens();
                if (oldChildren == null || newChildren == null)
                {
                    continue;
                }

                if (oldChildren.Count <= 0 || newChildren.Count <= 0)
                {
                    continue;
                }

                if (oldChildren[0] != newChildren[0])
                {
                    return false;
                }
            }

            return true;
        }

        public void FilterItemsByTextNew(string text)
        {
            long startTime = DateTime.UtcNow.Ticks;

            List<ITreeViewItem> items = [];
            foreach (TreeViewItem tvi in ((TreeViewItem)Tree.Items[0]).Items)
            {
                if (tvi.Tag is ITreeViewItem item)
                {
                    items.Add(item);
                }
            }

            FilterItemsByTextNew(items, text);

            Reporter.ToLog(eLogLevel.DEBUG, $"FilterItemsByTextNew took {TimeSpan.FromTicks(DateTime.UtcNow.Ticks - startTime).TotalMilliseconds}ms");
        }

        private bool FilterItemsByTextNew(IEnumerable<ITreeViewItem> items, string text)
        {
            if (items == null)
            {
                return false;
            }

            bool wasFound = false;
            foreach (ITreeViewItem item in items)
            {
                NewTreeViewItemBase itemBase = (NewTreeViewItemBase)item;

                itemBase.Visibility = Visibility.Collapsed;

                string header = GetItemHeaderText(item);
                if (!string.IsNullOrEmpty(header) && header.Contains(text, StringComparison.OrdinalIgnoreCase))
                {
                    itemBase.Visibility = Visibility.Visible;
                    wasFound = true;
                }

                bool foundMatchInChildren = FilterItemsByTextNew(item.Childrens(), text);
                if (foundMatchInChildren)
                {
                    wasFound = true;
                    itemBase.Visibility = Visibility.Visible;
                }

                bool canExpand = item.IsExpandable() && itemBase.TreeViewItem != null;
                if (foundMatchInChildren && canExpand)
                {
                    itemBase.TreeViewItem!.IsExpanded = true;
                }
            }

            return wasFound;
        }

        private string GetItemHeaderText(ITreeViewItem item)
        {
            StackPanel SP = item.Header();

            //Combine text of all label child's of the header Stack panel
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
            return HeaderTXT;
        }

        /// <summary>
        /// Recursive method to go over all tree nodes
        /// </summary>
        /// <param name="itemCollection"></param>
        /// <param name="txt"></param>
        public void FilterItemsByText(ItemCollection itemCollection, string txt, CancellationToken cancellationToken = new CancellationToken())
        {
            Task.Run(() =>
            {
                try
                {
                    // Need to expand to get all lazy loading
                    foreach (TreeViewItem tvi in itemCollection)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            tvi.IsExpanded = true;
                        });

                        if (tviChildNodesLoadTaskMap.TryGetValue(tvi, out Task? loadChildrenTask))
                            loadChildrenTask.Wait();
                    }

                    // Filter not working for new TVI            
                    foreach (TreeViewItem tvi in itemCollection)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                List<TreeViewItem> pathNodes = [];
                                if (LastSelectedTVI != null)
                                {
                                    pathNodes = getSelecetdItemPathNodes(LastSelectedTVI);
                                }
                                CollapseUnselectedTreeNodes(TreeItemsCollection, pathNodes);


                                return;
                            }
                        });

                        //ITreeViewItem ITVI = (ITreeViewItem)tvi.Tag;

                        Dispatcher.Invoke(() =>
                        {
                            // Find the label in the header, this is label child of the Header Stack Panel
                            StackPanel SP = (StackPanel)tvi.Header;

                            //Combine text of all label child's of the header Stack panel
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
                                FilterItemsByText(tvi.Items, txt, cancellationToken);
                            }
                        });
                    }

                    Dispatcher.Invoke(() =>
                    {
                        //Show the root item
                        ((TreeViewItem)Tree.Items[0]).Visibility = System.Windows.Visibility.Visible;
                    });
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                }
            }, cancellationToken);

        }
        public static List<TreeViewItem> getSelecetdItemPathNodes(TreeViewItem SelectedItem)
        {
            List<TreeViewItem> pathNodes = [];
            object ParentItem = getParentItem(SelectedItem);
            while (ParentItem?.GetType() == typeof(TreeViewItem))
            {
                pathNodes.Add((TreeViewItem)ParentItem);
                ParentItem = getParentItem(ParentItem);
            }
            return pathNodes;
        }
        public static object getParentItem(object tvi)
        {
            return ((TreeViewItem)tvi).Parent;
        }
        public static void CollapseUnselectedTreeNodes(ItemCollection itemCollection, List<TreeViewItem> pathNodes)
        {
            foreach (TreeViewItem tvItem in itemCollection)
            {
                if (tvItem.HasItems)
                {
                    CollapseUnselectedTreeNodes(tvItem.Items, pathNodes);
                    foreach (TreeViewItem item in tvItem.Items)
                    {
                        if (!(pathNodes != null && pathNodes.Contains(item)))
                        {
                            item.IsExpanded = false;
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }
        public void SetBtnImage(Button btn, string imageName)
        {
            Image image = new Image
            {
                Source = new BitmapImage(new Uri(@"/Images/" + imageName, UriKind.RelativeOrAbsolute))
            };
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

        public ITreeViewItem? FindMatchingTreeItem(Predicate<object> matchPredicate)
        {
            return FindMatchingTreeItemPrivate(matchPredicate, (TreeViewItem)Tree.Items[0]);
        }

        private ITreeViewItem? FindMatchingTreeItemPrivate(Predicate<object> matchPredicate, TreeViewItem root)
        {
            foreach (TreeViewItem currentTreeItem in root.Items)
            {
                ITreeViewItem tvi = (ITreeViewItem)currentTreeItem.Tag;
                if (tvi != null)
                {
                    if (matchPredicate(tvi.NodeObject()))
                    {
                        return tvi;
                    }
                }
                if (currentTreeItem.Items.Count > 0)
                {
                    currentTreeItem.IsExpanded = true;
                    ITreeViewItem? matchedSubTreeItem = FindMatchingTreeItemPrivate(matchPredicate, currentTreeItem);
                    if (matchedSubTreeItem != null)
                    {
                        return matchedSubTreeItem;
                    }
                }
            }
            return null;
        }

        public Task IterateTreeViewItemsAsync(Func<ITreeViewItem, bool> iterationConsumer, bool inReverseOrder)
        {
            return IterateTreeViewItemsAsync(treeViewItem =>
            {
                bool continueIteration = true;
                object treeViewItemTag = treeViewItem.Tag;
                if (treeViewItemTag is not null and ITreeViewItem iTreeViewItem)
                {
                    continueIteration = iterationConsumer.Invoke(iTreeViewItem);
                }
                return continueIteration;
            }, inReverseOrder);
        }

        public Task IterateTreeViewItemsAsync(Func<TreeViewItem, bool> iterationConsumer, bool inReverseOrder = false)
        {
            TreeViewItem firstTreeItem = (TreeViewItem)Tree.Items[0];
            return IterateTreeViewItemsPrivateAsync(iterationConsumer, inReverseOrder, root: firstTreeItem);
        }

        private async Task<bool> IterateTreeViewItemsPrivateAsync(Func<TreeViewItem, bool> iterationConsumer, bool inReverseOrder, TreeViewItem root)
        {
            try
            {
                bool continueIteration = true;

                int index = 0;
                Predicate<int> boundCheck = index => index < root.Items.Count;
                int indexIncrementation = 1;

                await LoadChildItems(root);

                if (inReverseOrder)
                {
                    index = root.Items.Count - 1;
                    boundCheck = index => index >= 0;
                    indexIncrementation = -1;
                }

                while (boundCheck.Invoke(index))
                {
                    TreeViewItem currentTreeItem = (TreeViewItem)root.Items[index];
                    ITreeViewItem tvi = (ITreeViewItem)currentTreeItem.Tag;

                    if (tvi != null)
                    {
                        continueIteration = iterationConsumer.Invoke(currentTreeItem);
                        if (!continueIteration)
                        {
                            break;
                        }
                    }

                    if (currentTreeItem.Items.Count > 0)
                    {
                        continueIteration = await IterateTreeViewItemsPrivateAsync(iterationConsumer, inReverseOrder, currentTreeItem);
                        if (!continueIteration)
                        {
                            break;
                        }
                    }

                    index += indexIncrementation;
                }

                return continueIteration;
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while iterating UCTreeView items.", e);
                return false;
            }
        }

        public void FocusItem(TreeViewItem treeViewItem)
        {
            DependencyObject? parent = treeViewItem.Parent;
            while (parent is not null and TreeViewItem parentTreeViewItem)
            {
                parentTreeViewItem.IsExpanded = true;
                parent = parentTreeViewItem.Parent;
            }
            treeViewItem.IsSelected = true;
        }

        public void SelectItem(ITreeViewItem item)
        {
            TreeViewItem TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], item);
            TVI.IsExpanded = true;
            GingerCore.General.DoEvents();
            TVI.Focus();
        }

        public void GetChildItembyNameandSelect(string nodeName, ITreeViewItem Parent = null)
        {
            Task.Run(() =>
            {
                if (mSetTreeNodeItemChildsEvent != null)
                {
                    mSetTreeNodeItemChildsEvent.WaitOne();
                }
                Dispatcher.Invoke(() =>
                {
                    TreeViewItem TVI = (TreeViewItem)Tree.Items[0];
                    if (Parent != null)
                    {
                        TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], Parent);
                        TVI.IsExpanded = true;
                        GingerCore.General.DoEvents();
                    }

                    TreeViewItem TVIChild = ExpandNodeByNameTVIRecursive(TVI, nodeName, true, false);
                    if (TVIChild != null)
                    {
                        TVIChild.Focus();
                    }
                });
            });
        }

        public void SelectFirstVisibleChildItem(ItemCollection treeItems, List<Type> childTypes)
        {
            foreach (TreeViewItem item in treeItems)
            {
                if (item.Visibility == Visibility.Visible)
                {
                    if (item.Tag != null && childTypes.Contains(item.Tag.GetType()))
                    {
                        item.Focus();
                        return;
                    }

                    if (item.HasItems)
                    {
                        SelectFirstVisibleChildItem(item.Items, childTypes);
                    }
                }
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
            {
                parentTVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], parentItem);
            }
            else
            {
                parentTVI = (TreeViewItem)Tree.Items[0];
            }

            if (parentTVI != null)
            {
                TreeViewItem toDeleteTVI = SearchTVIByObjectRecursive(parentTVI, itemObject);
                if (toDeleteTVI != null)
                {
                    TreeViewItem parent = (TreeViewItem)toDeleteTVI.Parent;
                    parent.Items.Remove(toDeleteTVI);
                    parent.Focus();
                }
            }
        }

        public void SelectParentItem(ITreeViewItem NodeItem)
        {
            TreeViewItem TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], NodeItem);
            if (TVI != null)
            {
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
        }

        public void RefreshHeader(ITreeViewItem NodeItem)
        {
            TreeViewItem TVI = SearchTVIRecursive((TreeViewItem)Tree.Items[0], NodeItem);
            if (TVI != null)
            {
                ITreeViewItem ITVI = (ITreeViewItem)TVI.Tag;
                TVI.Header = ITVI.Header();
                if (Tree.SelectedItem == TVI)
                {
                    TVI.Header = SetSelectedTreeNodeHeaderStyle((StackPanel)TVI.Header);
                }
            }
        }

        public void RefreshHeader(TreeViewItem TVI)
        {
            if (TVI != null)
            {
                ITreeViewItem ITVI = (ITreeViewItem)TVI.Tag;
                TVI.Header = ITVI.Header();
                if (Tree.SelectedItem == TVI)
                {
                    TVI.Header = SetSelectedTreeNodeHeaderStyle((StackPanel)TVI.Header);
                }
            }
        }

        private TreeViewItem SearchTVIRecursive(TreeViewItem node, ITreeViewItem SearchedNode)
        {
            ITreeViewItem o = null;
            // can be triggered by tree change of FileWathcher which is on another thread, so running on dispatcher            
            o = (ITreeViewItem)node.Tag;
            if (o != null && o.Equals(SearchedNode))
            {
                return node;
            }

            foreach (TreeViewItem TVI in node.Items)
            {
                TreeViewItem vv = SearchTVIRecursive(TVI, SearchedNode);
                if (vv != null)
                {
                    return vv;
                }
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
                    {
                        return child;
                    }
                    else
                    {
                        foreach (TreeViewItem subChild in child.Items)
                        {
                            TreeViewItem foundItem = SearchTVIByObjectRecursive(subChild, treeNodeObject);
                            if (foundItem != null)
                            {
                                return foundItem;
                            }
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
                            o = ((Label)child).Content.ToString();
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

                if (TVI != null && TVI.Items.Count > 0)
                {
                    _ = LoadChildItems(TVI);
                    TreeViewItem vv = ExpandNodeByNameTVIRecursive(TVI, NodeName, Refresh, ExpandChildren);
                    if (vv != null)
                    {
                        return vv;
                    }
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

            while (retVal is not null and not TreeViewItem)
            {
                retVal = VisualTreeHelper.GetParent(retVal) as UIElement;
            }

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
        public enum eUcTreeValidationRules
        {
            NoItemSelected,
        }

        public List<eUcTreeValidationRules> ValidationRules = [];

        public bool HasValidationError()
        {
            bool validationRes = false;
            foreach (eUcTreeValidationRules rule in ValidationRules)
            {
                if (rule == eUcTreeValidationRules.NoItemSelected)
                {
                    if (Tree.SelectedItem == null)
                    {
                        validationRes = true;
                    }
                }
            }

            //set border color based on validation
            if (validationRes == true)
            {
                Tree.BorderThickness = new Thickness(1);
                Tree.BorderBrush = System.Windows.Media.Brushes.Red;
            }
            else
            {
                Tree.BorderThickness = new Thickness(0);
                Tree.BorderBrush = FindResource("$PrimaryColor_Black") as Brush;
            }

            return validationRes;
        }


    }
}
