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

extern alias UIAComWrapperNetstandard;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using GingerCore.GeneralLib;
using GingerWPF.DragDropLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;

namespace Ginger.UserControlsLib.UCListView
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UcListView : UserControl, IDragDrop, IClipboardOperations
    {
        IObservableList mObjList;
        ICollectionView filteredView;

        private string mSearchString;
        public ObservableList<Guid> Tags = null;

        CollectionView mGroupView;

        public delegate void UcListViewEventHandler(UcListViewEventArgs EventArgs);
        public event UcListViewEventHandler UcListViewEvent;
        public void OnUcListViewEvent(UcListViewEventArgs.eEventType eventType, Object eventObject = null)
        {
            UcListViewEventHandler handler = UcListViewEvent;
            if (handler != null)
            {
                handler(new UcListViewEventArgs(eventType, eventObject));
            }
        }

        NotifyCollectionChangedEventHandler CollectionChangedHandler;

        public delegate void ItemMouseDoubleClickEventHandler(object? sender, MouseButtonEventArgs e);
        public event ItemMouseDoubleClickEventHandler ItemMouseDoubleClick;

        // DragDrop event handler
        public event EventHandler ItemDropped;
        public delegate void ItemDroppedEventHandler(DragInfo DragInfo);

        public event EventHandler SameFrameItemDropped;
        public delegate void SameFrameItemDroppedEventHandler(DragInfo DragInfo);

        public event EventHandler PreviewDragItem;
        public event PasteItemEventHandler PasteItemEvent;

        public delegate void PreviewDragItemEventHandler(DragInfo DragInfo);

        private bool mIsDragDropCompatible;
        public bool IsDragDropCompatible
        {
            get { return mIsDragDropCompatible; }
            set
            {
                if (mIsDragDropCompatible == value)
                    return;
                else if (value == false)
                {
                    DragDrop2.UnHookEventHandlers(this);
                    mIsDragDropCompatible = value;
                    return;
                }
                else if (value == true)
                {
                    DragDrop2.HookEventHandlers(this);
                    mIsDragDropCompatible = value;
                    return;
                }

            }
        }

        IListViewHelper mListViewHelper = null;

        public UcListView()
        {
            InitializeComponent();

            //Hook Drag Drop handler
            mIsDragDropCompatible = true;
            DragDrop2.HookEventHandlers(this);

            if (Tags == null)
            {
                Tags = new ObservableList<Guid>();
            }

            xTagsFilter.Init(Tags);
            xTagsFilter.TagsStackPanlChanged += TagsFilter_TagsStackPanlChanged;
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ItemMouseDoubleClick?.Invoke(sender, e);
        }

        private void TagsFilter_TagsStackPanlChanged(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                CollectFilterData();
                filteredView.Refresh();
            });
        }

        public ListView List
        {
            get
            {
                return xListView;
            }
            set
            {
                xListView = value;
            }
        }

        public SelectionMode ListSelectionMode
        {
            get
            {
                return xListView.SelectionMode;
            }
            set
            {
                xListView.SelectionMode = value;
            }
        }

        public IObservableList DataSourceList
        {
            set
            {
                try
                {
                    if (mObjList != null)
                    {
                        mObjList.PropertyChanged -= ObjListPropertyChanged;
                        mObjList.CollectionChanged -= CollectionChangedHandler;
                        OnUcListViewEvent(UcListViewEventArgs.eEventType.ClearBindings);//so all list items will release their binding
                    }

                    mObjList = value;

                    filteredView = new CollectionViewSource() { Source = mObjList }.View;

                    if (filteredView != null)
                    {
                        CollectFilterData();
                        //if(filteredView.Filter == null)
                        filteredView.Filter = LVItemFilter;
                    }

                    xListView.ItemsSource = filteredView;

                    this.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        xSearchTextBox.Text = "";
                        
                        // Make the first row selected
                        if (value != null && value.Count > 0)
                        {
                            xListView.SelectedIndex = 0;
                            xListView.SelectedItem = value[0];
                            // Make sure that in case we have only one item it will be the current - otherwise gives err when one record
                            if (mObjList.SyncCurrentItemWithViewSelectedItem && mObjList.Count > 0)
                            {
                                mObjList.CurrentItem = value[0];
                            }
                        }

                        //show items as collapsed
                        mListViewHelper.ExpandItemOnLoad = false;
                        xExpandCollapseBtn.ButtonImageType = eImageType.ExpandAll;
                    }));
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to set ucListView.DataSourceList", ex);
                }

                if (mObjList != null)
                {
                    mObjList.PropertyChanged += ObjListPropertyChanged;
                    BindingOperations.EnableCollectionSynchronization(mObjList, mObjList);//added to allow collection changes from other threads
                    CollectionChangedHandler = new NotifyCollectionChangedEventHandler(CollectionChangedMethod);
                    mObjList.CollectionChanged += CollectionChangedHandler;
                    UpdateTitleListCount();
                }
            }
            get
            {
                return mObjList;
            }
        }

        List<Guid> mFilterSelectedTags = null;
        private void CollectFilterData()
        {
            //collect search values           
            this.Dispatcher.Invoke(() =>
            {
                mObjList.FilterStringData = xSearchTextBox.Text;
                mFilterSelectedTags = xTagsFilter.GetSelectedTagsList();
            });
        }

        bool LVItemFilter(object item)
        {
            if (string.IsNullOrWhiteSpace(mObjList.FilterStringData) && (mFilterSelectedTags == null || mFilterSelectedTags.Count == 0))
                return true;

            //Filter by search text            
            if (!string.IsNullOrEmpty(mObjList.FilterStringData))
            {
                return ((item as RepositoryItemBase).ItemName.IndexOf(mObjList.FilterStringData, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            //Filter by Tags            
            if (mFilterSelectedTags != null && mFilterSelectedTags.Count > 0)
            {
                return TagsFilter(item, mFilterSelectedTags);
            }

            return false;
        }

        private bool TagsFilter(object obj, List<Guid> selectedTagsGUID)
        {
            if (obj is ISearchFilter)
            {
                return ((ISearchFilter)obj).FilterBy(eFilterBy.Tags, selectedTagsGUID);
            }
            return false;
        }

        private void ObjListPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            GingerCore.General.DoEvents();
            if (e.PropertyName == nameof(IObservableList.CurrentItem))
            {
                if (mObjList.SyncViewSelectedItemWithCurrentItem)
                {
                    SetListSelectedItemAsSourceCurrentItem();
                }
            }
            if (e.PropertyName == nameof(IObservableList.FilterStringData))
            {
                this.Dispatcher.Invoke(() => xSearchTextBox.Text = mObjList.FilterStringData);
            }
        }

        private void SetListSelectedItemAsSourceCurrentItem()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (mObjList.CurrentItem != xListView.SelectedItem)
                {
                    xListView.SelectedItem = mObjList.CurrentItem;
                    int index = xListView.Items.IndexOf(mObjList.CurrentItem);
                    xListView.SelectedIndex = index;
                    ScrollToViewCurrentItem();
                }
            });
        }
        
        private void CollectionChangedMethod(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                //different kind of changes that may have occurred in collection
                if (e.Action == NotifyCollectionChangedAction.Add ||
                    e.Action == NotifyCollectionChangedAction.Replace ||
                    e.Action == NotifyCollectionChangedAction.Remove ||
                    e.Action == NotifyCollectionChangedAction.Move)
                {
                    OnUcListViewEvent(UcListViewEventArgs.eEventType.UpdateIndex);
                }
                UpdateTitleListCount();
            });
        }

        private void UpdateTitleListCount()
        {
            this.Dispatcher.Invoke(() =>
            {
                xListCountTitleLbl.Content = string.Format("({0})", mObjList.Count);
            });
        }

        public object CurrentItem
        {
            get
            {
                object o = null;
                this.Dispatcher.Invoke(() =>
                {
                    o = xListView.SelectedItem;
                });
                return o;
            }
        }

        public int CurrentItemIndex
        {
            get
            {
                if (xListView.Items != null && xListView.Items.Count > 0)
                {
                    return xListView.Items.IndexOf(xListView.SelectedItem);
                }
                else
                {
                    return -1;
                }
            }
        }

        public Visibility ExpandCollapseBtnVisiblity
        {
            get
            {
                return xExpandCollapseBtn.Visibility;
            }
            set
            {
                xExpandCollapseBtn.Visibility = value;
            }
        }

        public void ResetExpandCollapseBtn()
        {
            xExpandCollapseBtn.ButtonImageType = eImageType.ExpandAll;
        }

        public Visibility ListOperationsBarPnlVisiblity
        {
            get
            {
                return xAllListOperationsBarPnl.Visibility;
            }
            set
            {
                xAllListOperationsBarPnl.Visibility = value;
            }
        }


        public Visibility ListTitleVisibility
        {
            get
            {
                return xListTitlePnl.Visibility;
            }
            set
            {
                xListTitlePnl.Visibility = value;
            }
        }
        public string Title
        {
            get
            {
                if (xListTitleLbl.Content != null)
                {
                    return xListTitleLbl.Content.ToString(); ;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                xListTitleLbl.Content = value;
            }
        }

        public eImageType ListImageType
        {
            get
            {
                return xListTitleImage.ImageType;
            }
            set
            {
                xListTitleImage.ImageType = value;
            }
        }

        public void ScrollToViewCurrentItem()
        {
            if (mObjList.CurrentItem != null)
            {
                this.Dispatcher.Invoke(() =>
                {
                    xListView.ScrollIntoView(mObjList.CurrentItem);
                });
            }
        }

        private void xListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TODO:Create a event for Activity list selection changed and reload linked activity from SR
            if (mObjList != null && mObjList.SyncCurrentItemWithViewSelectedItem)
            {
                SetSourceCurrentItemAsListSelectedItem();
            }

            //e.Handled = true;
        }

        private void SetSourceCurrentItemAsListSelectedItem()
        {
            if (mObjList == null) return;

            if (mObjList.CurrentItem == xListView.SelectedItem) return;

            if (mObjList != null)
            {
                mObjList.CurrentItem = xListView.SelectedItem;
                ScrollToViewCurrentItem();
            }
        }

        private void XExpandCollapseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (xExpandCollapseBtn.ButtonImageType == eImageType.ExpandAll)
            {
                OnUcListViewEvent(UcListViewEventArgs.eEventType.ExpandAllItems);
                xExpandCollapseBtn.ButtonImageType = eImageType.CollapseAll;
                mListViewHelper.ExpandItemOnLoad = true;
            }
            else
            {
                OnUcListViewEvent(UcListViewEventArgs.eEventType.CollapseAllItems);
                xExpandCollapseBtn.ButtonImageType = eImageType.ExpandAll;
                mListViewHelper.ExpandItemOnLoad = false;
            }
        }

        public void SetDefaultListDataTemplate(IListViewHelper listViewHelper)
        {
            mListViewHelper = listViewHelper;
            mListViewHelper.ListView = this;
            this.Dispatcher.Invoke(() =>
            {
                DataTemplate dataTemp = new DataTemplate();
                FrameworkElementFactory listItemFac = new FrameworkElementFactory(typeof(UcListViewItem));
                listItemFac.SetBinding(UcListViewItem.ItemProperty, new Binding());
                listItemFac.SetValue(UcListViewItem.ListHelperProperty, listViewHelper);
                dataTemp.VisualTree = listItemFac;
                xListView.ItemTemplate = dataTemp;

                SetListOperations();
                SetListExtraOperations();
            });
        }

        public void SetListOperations()
        {
            List<ListItemOperation> listOperations = mListViewHelper.GetListOperations();
            if (listOperations != null && listOperations.Count > 0)
            {
                xListOperationsPnl.Children.Clear();
                xListOperationsPnl.Visibility = Visibility.Visible;

                foreach (ListItemOperation operation in listOperations.Where(x => x.SupportedViews.Contains(mListViewHelper.PageViewMode)).ToList())
                {
                    ucButton operationBtn = new ucButton();
                    operationBtn.ButtonType = Amdocs.Ginger.Core.eButtonType.CircleImageButton;
                    operationBtn.ButtonImageType = operation.ImageType;
                    operationBtn.ToolTip = operation.ToolTip;
                    operationBtn.Margin = new Thickness(-2, 0, -2, 0);
                    operationBtn.ButtonImageHeight = 14;
                    operationBtn.ButtonImageWidth = 14;
                    operationBtn.ButtonFontImageSize = operation.ImageSize;

                    if (operation.ImageForeground == null)
                    {
                        //operationBtn.ButtonImageForground = (SolidColorBrush)FindResource("$BackgroundColor_DarkBlue");
                    }
                    else
                    {
                        operationBtn.ButtonImageForground = operation.ImageForeground;
                    }

                    if (operation.ImageBindingObject != null)
                    {
                        if (operation.ImageBindingConverter == null)
                        {
                            BindingHandler.ObjFieldBinding(operationBtn, ucButton.ButtonImageTypeProperty, operation.ImageBindingObject, operation.ImageBindingFieldName, BindingMode.OneWay);
                        }
                        else
                        {
                            BindingHandler.ObjFieldBinding(operationBtn, ucButton.ButtonImageTypeProperty, operation.ImageBindingObject, operation.ImageBindingFieldName, bindingConvertor: operation.ImageBindingConverter, BindingMode.OneWay);
                        }
                    }

                    operationBtn.Click += operation.OperationHandler;
                    operationBtn.Tag = xListView.ItemsSource;

                    xListOperationsPnl.Children.Add(operationBtn);
                }
            }

            if (xListOperationsPnl.Children.Count == 0)
            {
                xListOperationsPnl.Visibility = Visibility.Collapsed;
            }
        }

        private void SetListExtraOperations()
        {
            List<ListItemOperation> extraOperations = mListViewHelper.GetListExtraOperations();
            if (extraOperations != null && extraOperations.Count > 0)
            {
                ((MenuItem)(xListExtraOperationsMenu.Items[0])).Items.Clear();
                xListExtraOperationsMenu.Visibility = Visibility.Visible;
                foreach (ListItemOperation operation in extraOperations.Where(x => x.SupportedViews.Contains(mListViewHelper.PageViewMode)).ToList())
                {
                    MenuItem menuitem = new MenuItem();
                    menuitem.Style = (Style)FindResource("$MenuItemStyle");
                    ImageMakerControl iconImage = new ImageMakerControl();
                    iconImage.ImageType = operation.ImageType;
                    iconImage.SetAsFontImageWithSize = operation.ImageSize;
                    iconImage.HorizontalAlignment = HorizontalAlignment.Left;
                    menuitem.Icon = iconImage;
                    menuitem.Header = operation.Header;
                    menuitem.ToolTip = operation.ToolTip;

                    if (operation.ImageForeground == null)
                    {
                        //iconImage.ImageForeground = (SolidColorBrush)FindResource("$BackgroundColor_DarkBlue");
                    }
                    else
                    {
                        iconImage.ImageForeground = operation.ImageForeground;
                    }

                    if (operation.ImageBindingObject != null)
                    {
                        if (operation.ImageBindingConverter == null)
                        {
                            BindingHandler.ObjFieldBinding(iconImage, ImageMaker.ContentProperty, operation.ImageBindingObject, operation.ImageBindingFieldName, BindingMode.OneWay);
                        }
                        else
                        {
                            BindingHandler.ObjFieldBinding(iconImage, ImageMaker.ContentProperty, operation.ImageBindingObject, operation.ImageBindingFieldName, bindingConvertor: operation.ImageBindingConverter, BindingMode.OneWay);
                        }
                    }

                    menuitem.Click += operation.OperationHandler;

                    menuitem.Tag = xListView.ItemsSource;

                    if (string.IsNullOrEmpty(operation.Group))
                    {
                        ((MenuItem)(xListExtraOperationsMenu.Items[0])).Items.Add(menuitem);
                    }
                    else
                    {
                        //need to add to Group
                        bool addedToGroup = false;
                        foreach (MenuItem item in ((MenuItem)(xListExtraOperationsMenu.Items[0])).Items)
                        {
                            if (item.Header.ToString() == operation.Group)
                            {
                                //adding to existing group
                                item.Items.Add(menuitem);
                                addedToGroup = true;
                                break;
                            }
                        }
                        if (!addedToGroup)
                        {
                            //creating the group and adding
                            MenuItem groupMenuitem = new MenuItem();
                            groupMenuitem.Style = (Style)FindResource("$MenuItemStyle");
                            ImageMakerControl groupIconImage = new ImageMakerControl();
                            groupIconImage.ImageType = operation.GroupImageType;
                            groupIconImage.SetAsFontImageWithSize = operation.ImageSize;
                            groupIconImage.HorizontalAlignment = HorizontalAlignment.Left;
                            groupMenuitem.Icon = groupIconImage;
                            groupMenuitem.Header = operation.Group;
                            groupMenuitem.ToolTip = operation.Group;
                            ((MenuItem)(xListExtraOperationsMenu.Items[0])).Items.Add(groupMenuitem);
                            groupMenuitem.Items.Add(menuitem);
                        }
                    }
                }
            }

            if (((MenuItem)(xListExtraOperationsMenu.Items[0])).Items.Count == 0)
            {
                xListExtraOperationsMenu.Visibility = Visibility.Collapsed;
            }
        }

        //public void ClearBindings()
        //{
        //    if (xTagsFilter != null)
        //    {
        //        xTagsFilter.TagsStackPanlChanged -= TagsFilter_TagsStackPanlChanged;
        //        xTagsFilter.ClearBinding();
        //        xTagsFilter.ClearControlsBindings();
        //        xTagsFilter = null;
        //    }

        //    if (mObjList != null)
        //    {
        //        mObjList.PropertyChanged -= ObjListPropertyChanged;
        //        BindingOperations.DisableCollectionSynchronization(mObjList);
        //        mObjList.CollectionChanged -= CollectionChangedHandler;
        //    }

        //    foreach (ucButton operation in xListOperationsPnl.Children)
        //    {
        //        BindingOperations.ClearAllBindings(operation);
        //    }
        //    foreach (MenuItem extraOperation in xListExtraOperationsMenu.Items)
        //    {
        //        BindingOperations.ClearAllBindings(extraOperation);
        //    }
        //    OnUcListViewEvent(UcListViewEventArgs.eEventType.ClearBindings);
        //    this.ClearControlsBindings();
        //}

        void IDragDrop.StartDrag(DragInfo Info)
        {
            // Get the item under the mouse, or nothing, avoid selecting scroll bars. or empty areas etc..
            Info.DragSource = this;

            var rowItem = ItemsControl.ContainerFromElement(this.xListView, (DependencyObject)Info.OriginalSource);
            if (rowItem != null)
            {
                if (rowItem is ListViewItem)
                {
                    int selectedItemsCount = this.GetSelectedItems().Count;

                    if (selectedItemsCount > 1)
                    {
                        Info.Data = this.GetSelectedItems();
                        int identityTextLength = (rowItem as ListViewItem).Content.ToString().ToCharArray().Length;
                        if (identityTextLength > 16)
                        {
                            identityTextLength = 16;
                        }
                        Info.Header = (rowItem as ListViewItem).Content.ToString().Substring(0, identityTextLength) + ".. + " + (selectedItemsCount - 1);
                    }
                    else
                    {
                        Info.Data = (rowItem as ListViewItem).Content;
                        Info.Header = Info.Data.ToString();
                    }
                }
                else if (rowItem is GroupItem)
                {
                    Info.Data = ((GroupItem)ItemsControl.ContainerFromElement(this.xListView, (DependencyObject)Info.OriginalSource)).Content;
                    Info.Header = Info.Data.GetType().GetProperty("Name").GetValue(Info.Data).ToString();
                }
            }
        }

        void IDragDrop.DragOver(DragInfo Info)
        {

        }

        void IDragDrop.Drop(DragInfo Info)
        {
            // first check if we did drag and drop on the same ListView then it is a move - reorder
            if (Info.DragSource == this)
            {
                EventHandler mHandler = SameFrameItemDropped;
                if (mHandler != null)
                {
                    mHandler(Info, new EventArgs());
                }
                else
                {
                    RepositoryItemBase draggedItem = Info.Data as RepositoryItemBase;

                    if (draggedItem != null)
                    {
                        RepositoryItemBase draggedOnItem = DragDrop2.GetRepositoryItemHit(this) as RepositoryItemBase;
                        if (draggedOnItem != null)
                        {
                            DragDrop2.ShuffleControlsItems(draggedItem, draggedOnItem, this);
                        }
                    }
                }
                //if (!(xMoveUpBtn.Visibility == System.Windows.Visibility.Visible)) return;  // Do nothing if reorder up/down arrow are not allowed
                return;
            }

            // OK this is a dropped from external
            EventHandler handler = ItemDropped;
            if (handler != null)
            {
                handler(Info, new EventArgs());
            }
            // TODO: if in same grid then do move, 
        }

        void IDragDrop.DragEnter(DragInfo Info)
        {
            Info.DragTarget = this;

            if (Info.DragSource == Info.DragTarget)
            {
                DragDrop2.DrgInfo.DragIcon = DragInfo.eDragIcon.Move;
            }
            else
            {
                EventHandler handler = PreviewDragItem;
                if (handler != null)
                {
                    handler(Info, new EventArgs());
                }
            }
        }

        private void XDeleteGroupBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        string mGroupByProperty = string.Empty;
        public void AddGrouping(string groupByProperty)
        {
            mGroupByProperty = groupByProperty;
            DoGrouping();
        }

        public void UpdateGrouping()
        {
            mGroupView.Refresh();
            ResetExpandCollapseBtn();
            //if (xExpandCollapseBtn.ButtonImageType == eImageType.CollapseAll)
            //{
            //    OnUcListViewEvent(UcListViewEventArgs.eEventType.ExpandAllItems);
            //}
        }

        private void DoGrouping()
        {
            mGroupView = (CollectionView)CollectionViewSource.GetDefaultView(xListView.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription(mGroupByProperty);
            mGroupView.GroupDescriptions.Clear();
            mGroupView.GroupDescriptions.Add(groupDescription);
        }

        private void SetGroupNotifications(DockPanel panel)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (panel.Tag != null)
                {
                    List<ListItemNotification> notifications = mListViewHelper.GetItemGroupNotificationsList(panel.Tag.ToString());
                    if (notifications != null && notifications.Count > 0)
                    {
                        panel.Visibility = Visibility.Visible;
                        foreach (ListItemNotification notification in notifications)
                        {
                            ImageMakerControl itemInd = new ImageMakerControl();
                            itemInd.SetValue(AutomationProperties.AutomationIdProperty, notification.AutomationID);
                            itemInd.ImageType = notification.ImageType;
                            itemInd.ToolTip = notification.ToolTip;
                            itemInd.Margin = new Thickness(3, 0, 3, 0);
                            itemInd.Height = 16;
                            itemInd.Width = 16;
                            itemInd.SetAsFontImageWithSize = notification.ImageSize;

                            if (notification.ImageForeground == null)
                            {
                                itemInd.ImageForeground = System.Windows.Media.Brushes.LightPink;
                            }
                            else
                            {
                                itemInd.ImageForeground = notification.ImageForeground;
                            }

                            if (notification.BindingConverter == null)
                            {
                                BindingHandler.ObjFieldBinding(itemInd, ImageMakerControl.VisibilityProperty, notification.BindingObject, notification.BindingFieldName, BindingMode.OneWay);
                            }
                            else
                            {
                                BindingHandler.ObjFieldBinding(itemInd, ImageMakerControl.VisibilityProperty, notification.BindingObject, notification.BindingFieldName, bindingConvertor: notification.BindingConverter, BindingMode.OneWay);
                            }

                            panel.Children.Add(itemInd);
                        }
                    }
                    else
                    {
                        panel.Visibility = Visibility.Collapsed;
                    }
                }
            });
        }

        private void SetGroupOperations(Menu menu)
        {
            List<ListItemGroupOperation> groupOperations = mListViewHelper.GetItemGroupOperationsList();
            if (groupOperations != null && groupOperations.Count > 0)
            {
                foreach (ListItemGroupOperation operation in groupOperations.Where(x => x.SupportedViews.Contains(mListViewHelper.PageViewMode)).ToList())
                {
                    MenuItem menuitem = new MenuItem();
                    menuitem.Style = (Style)FindResource("$MenuItemStyle");
                    ImageMakerControl iconImage = new ImageMakerControl();
                    iconImage.ImageType = operation.ImageType;
                    iconImage.SetAsFontImageWithSize = operation.ImageSize;
                    iconImage.HorizontalAlignment = HorizontalAlignment.Left;
                    menuitem.Icon = iconImage;
                    menuitem.Header = operation.Header;
                    menuitem.ToolTip = operation.ToolTip;
                    menuitem.Click += operation.OperationHandler;

                    if (operation.Visible == false)
                    {
                        GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(menuitem, MenuItem.VisibilityProperty, WorkSpace.Instance.UserProfile, nameof(WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures), bindingConvertor: new GingerCore.GeneralLib.BoolVisibilityConverter(), BindingMode: System.Windows.Data.BindingMode.OneWay);
                    }

                    menuitem.Tag = menu.Tag;

                    if (string.IsNullOrEmpty(operation.Group))
                    {
                        ((MenuItem)menu.Items[0]).Items.Add(menuitem);
                    }
                    else
                    {
                        //need to add to Group
                        bool addedToGroup = false;
                        foreach (MenuItem item in (((MenuItem)menu.Items[0]).Items))
                        {
                            if (item.Header.ToString() == operation.Group)
                            {
                                //adding to existing group
                                item.Items.Add(menuitem);
                                addedToGroup = true;
                                break;
                            }
                        }
                        if (!addedToGroup)
                        {
                            //creating the group and adding
                            MenuItem groupMenuitem = new MenuItem();
                            groupMenuitem.Style = (Style)FindResource("$MenuItemStyle");
                            ImageMakerControl groupIconImage = new ImageMakerControl();
                            groupIconImage.ImageType = operation.GroupImageType;
                            groupIconImage.SetAsFontImageWithSize = operation.ImageSize;
                            groupIconImage.HorizontalAlignment = HorizontalAlignment.Left;
                            groupMenuitem.Icon = groupIconImage;
                            groupMenuitem.Header = operation.Group;
                            groupMenuitem.ToolTip = operation.Group;
                            ((MenuItem)menu.Items[0]).Items.Add(groupMenuitem);
                            groupMenuitem.Items.Add(menuitem);

                            if (operation.Visible == false)
                            {
                                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(groupMenuitem, MenuItem.VisibilityProperty, WorkSpace.Instance.UserProfile, nameof(WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures), bindingConvertor: new GingerCore.GeneralLib.BoolVisibilityConverter(), BindingMode: System.Windows.Data.BindingMode.OneWay);
                            }
                        }
                    }
                }
            }

        }

        private void XGroupOperationsMenu_Loaded(object sender, RoutedEventArgs e)
        {
            if (((MenuItem)((Menu)sender).Items[0]).Items.Count == 0)
            {
                SetGroupOperations((Menu)sender);
            }
        }

        private void XGroupNotificationsPnl_Loaded(object sender, RoutedEventArgs e)
        {
            if (((DockPanel)sender).Children.Count == 0)
            {
                SetGroupNotifications((DockPanel)sender);
            }
        }

        private async void xSearchTextBox_TextChangedAsync(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(xSearchTextBox.Text))
            {
                xSearchClearBtn.Visibility = Visibility.Collapsed;
                xSearchBtn.Visibility = Visibility.Visible;
            }
            else
            {
                xSearchClearBtn.Visibility = Visibility.Visible;
                xSearchBtn.Visibility = Visibility.Collapsed;

                // this inner method checks if user is still typing
                async Task<bool> UserKeepsTyping()
                {
                    string txt = xSearchTextBox.Text;
                    await Task.Delay(1000);
                    return txt != xSearchTextBox.Text;
                }
                if (await UserKeepsTyping() || xSearchTextBox.Text == mSearchString) return;
            }

            mSearchString = xSearchTextBox.Text;
            CollectFilterData();
            filteredView.Refresh();
        }

        private void xSearchClearBtn_Click(object sender, RoutedEventArgs e)
        {
            xSearchTextBox.Text = "";
            mSearchString = null;
        }

        private void xSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(xSearchTextBox.Text))
            {
                mSearchString = xSearchTextBox.Text;
                CollectFilterData();
                filteredView.Refresh();
            }
        }

        public ObservableList<RepositoryItemBase> GetSelectedItems()
        {
            ObservableList<RepositoryItemBase> selectedItemsList = new ObservableList<RepositoryItemBase>();
            foreach (object selectedItem in xListView.SelectedItems)
            {
                if (selectedItem is RepositoryItemBase)
                {
                    selectedItemsList.Add((RepositoryItemBase)selectedItem);
                }
            }
            return selectedItemsList;
        }

        public IObservableList GetSourceItemsAsIList()
        {
            return DataSourceList;
        }

        public ObservableList<RepositoryItemBase> GetSourceItemsAsList()
        {
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            foreach (RepositoryItemBase item in mObjList)
            {
                list.Add(item);
            }
            return list;
        }

        public void SetSelectedIndex(int index)
        {
            xListView.SelectedIndex = index;
        }

        public void OnPasteItemEvent(PasteItemEventArgs.ePasteType pasteType, RepositoryItemBase item)
        {
            PasteItemEvent?.Invoke(new PasteItemEventArgs(pasteType, item));
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (Keyboard.IsKeyDown(Key.C))
                {
                    //Do Copy
                    mListViewHelper.CopySelected();
                }
                else if(Keyboard.IsKeyDown(Key.X))
                {
                    //Do Cut
                    mListViewHelper.CutSelected();
                }
                else if (Keyboard.IsKeyDown(Key.V))
                {
                    //Do Paste
                    mListViewHelper.Paste();
                }
            }
            else if(Keyboard.IsKeyDown(Key.Delete))
            {
                //delete selected
                mListViewHelper.DeleteSelected();
            }
        }
    }

    public class UcListViewEventArgs
    {
        public enum eEventType
        {
            ExpandAllItems,
            ExpandItem,
            CollapseAllItems,
            UpdateIndex,
            ClearBindings,
        }

        public eEventType EventType;
        public Object EventObject;

        public UcListViewEventArgs(eEventType eventType, object eventObject = null)
        {
            this.EventType = eventType;
            this.EventObject = eventObject;
        }
    }
}
