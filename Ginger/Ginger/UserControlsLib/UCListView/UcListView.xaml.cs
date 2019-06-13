using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.UserControls;
using GingerCore.GeneralLib;
using GingerWPF.DragDropLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.UserControlsLib.UCListView
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UcListView : UserControl, IDragDrop
    {
        IObservableList mObjList;

        public delegate void UcListViewEventHandler(UcListViewEventArgs EventArgs);
        public event UcListViewEventHandler UcListViewEvent;
        private void OnUcListViewEvent(UcListViewEventArgs.eEventType eventType, Object eventObject = null)
        {
            UcListViewEventHandler handler = UcListViewEvent;
            if (handler != null)
            {
                handler(new UcListViewEventArgs(eventType, eventObject));
            }
        }

        // DragDrop event handler
        public event EventHandler ItemDropped;
        public delegate void ItemDroppedEventHandler(DragInfo DragInfo);

        public event EventHandler PreviewDragItem;
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

        IListViewItemInfo mListItemInfo = null;

        public UcListView()
        {
            InitializeComponent();

            //Hook Drag Drop handler
            mIsDragDropCompatible = true;
            DragDrop2.HookEventHandlers(this);
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

        public IObservableList DataSourceList
        {
            set
            {
                try
                {
                    if (mObjList != null)
                    {
                        mObjList.PropertyChanged -= ObjListPropertyChanged;
                    }

                    mObjList = value;
                    //mCollectionView = CollectionViewSource.GetDefaultView(mObjList);

                    //if (mCollectionView != null)
                    //{
                    //    try
                    //    {
                    //        CollectFilterData();
                    //        mCollectionView.Filter = FilterGridRows;
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        grdMain.CommitEdit();
                    //        grdMain.CancelEdit();
                    //        mCollectionView.Filter = FilterGridRows;
                    //        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                    //    }
                    //}
                    this.Dispatcher.Invoke(() =>
                    {
                        xListView.ItemsSource = mObjList;

                        // Make the first row selected
                        if (value != null && value.Count > 0)
                        {
                            xListView.SelectedIndex = 0;
                            xListView.SelectedItem = value[0];
                            // Make sure that in case we have only one item it will be the current - otherwise gives err when one record
                            mObjList.CurrentItem = value[0];
                        }
                    });
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to set ucListView.DataSourceList", ex);
                }

                if (mObjList != null)
                {
                    mObjList.PropertyChanged += ObjListPropertyChanged;
                    BindingOperations.EnableCollectionSynchronization(mObjList, mObjList);//added to allow collection changes from other threads
                    mObjList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(CollectionChangedMethod);
                    UpdateTitleListCount();
                }
            }

            get
            {
                return mObjList;
            }
        }

        private void ObjListPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            GingerCore.General.DoEvents();
            if (e.PropertyName == nameof(IObservableList.CurrentItem))
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (mObjList.CurrentItem != xListView.SelectedItem)
                    {
                        xListView.SelectedItem = mObjList.CurrentItem;
                        int index = xListView.Items.IndexOf(mObjList.CurrentItem);
                        xListView.SelectedIndex = index;
                    }
                });
            }
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

        public Visibility AddBtnVisiblity
        {
            get
            {
                return xAddBtn.Visibility;
            }
            set
            {
                xAddBtn.Visibility = value;
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

        public Visibility DeleteAllBtnVisiblity
        {
            get
            {
                return xDeleteAllBtn.Visibility;
            }
            set
            {
                xDeleteAllBtn.Visibility = value;
            }
        }

        public Visibility MoveBtnsVisiblity
        {
            get
            {
                return xMoveBtns.Visibility;
            }
            set
            {
                xMoveBtns.Visibility = value;
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

        public RoutedEventHandler AddItemHandler
        {
            set
            {
                xAddBtn.Click += value;
            }
        }

        private void xDeleteAllBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mObjList.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.NoItemToDelete);
                return;
            }

            if ((Reporter.ToUser(eUserMsgKey.SureWantToDeleteAll)) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                mObjList.SaveUndoData();
                mObjList.ClearAll();
            }
        }

        private void xMoveUpBtn_Click(object sender, RoutedEventArgs e)
        {
            int currentIndx = CurrentItemIndex;
            if (currentIndx >= 1)
            {
                mObjList.Move(currentIndx, currentIndx - 1);
                ScrollToViewCurrentItem();
            }
        }

        private void xMoveDownBtn_Click(object sender, RoutedEventArgs e)
        {
            int currentIndx = CurrentItemIndex;
            if (currentIndx >= 0)
            {
                mObjList.Move(currentIndx, currentIndx + 1);
                ScrollToViewCurrentItem();
            }
        }

        public void ScrollToViewCurrentItem()
        {
            if (mObjList.CurrentItem != null)
            {
                xListView.ScrollIntoView(mObjList.CurrentItem);
            }
        }

        private void xListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (SkipItemSelection)//avoid user item selection in run time 
            //{
            //    SkipItemSelection = false;
            //    return;
            //}

            if (mObjList == null) return;

            if (mObjList.CurrentItem == xListView.SelectedItem) return;

            if (mObjList != null)
            {
                mObjList.CurrentItem = xListView.SelectedItem;
                ScrollToViewCurrentItem();
            }

            //e.Handled = true;
        }

        private void XExpandCollapseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (xExpandCollapseBtn.ButtonImageType == eImageType.ExpandAll)
            {
                OnUcListViewEvent(UcListViewEventArgs.eEventType.ExpandAllItems);
                xExpandCollapseBtn.ButtonImageType = eImageType.CollapseAll;
            }
            else
            {
                OnUcListViewEvent(UcListViewEventArgs.eEventType.CollapseAllItems);
                xExpandCollapseBtn.ButtonImageType = eImageType.ExpandAll;
            }
        }

        public void SetDefaultListDataTemplate(IListViewItemInfo listItemInfo)
        {
            mListItemInfo = listItemInfo;
            DataTemplate dataTemp = new DataTemplate();
            FrameworkElementFactory listItemFac = new FrameworkElementFactory(typeof(UcListViewItem));
            listItemFac.SetBinding(UcListViewItem.ItemProperty, new Binding());
            listItemFac.SetValue(UcListViewItem.ItemInfoProperty, listItemInfo);
            dataTemp.VisualTree = listItemFac;
            this.Dispatcher.Invoke(() =>
            {
                xListView.ItemTemplate = dataTemp;
            });
        }

        public void AddListOperations(List<ListItemOperation> operations)
        {
            if (operations != null && operations.Count > 0)
            {
                foreach (ListItemOperation operation in operations)
                {
                    ucButton operationBtn = new ucButton();
                    operationBtn.ButtonType = Amdocs.Ginger.Core.eButtonType.CircleImageButton;
                    operationBtn.ButtonImageType = operation.ImageType;
                    operationBtn.ToolTip = operation.ToolTip;
                    operationBtn.Margin = new Thickness(-5, 0, -5, 0);
                    operationBtn.ButtonImageHeight = 18;
                    operationBtn.ButtonImageWidth = 18;
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

                    xListCommonOperationsPnl.Children.Add(operationBtn);
                }
            }
        }

        public void StartDrag(DragInfo Info)
        {

        }

        void IDragDrop.DragOver(DragInfo Info)
        {

        }

        void IDragDrop.Drop(DragInfo Info)
        {
            // first check if we did drag and drop in the same grid then it is a move - reorder
            if (Info.DragSource == this)
            {
                if (!(xMoveUpBtn.Visibility == System.Windows.Visibility.Visible)) return;  // Do nothing if reorder up/down arrow are not allowed
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

            EventHandler handler = PreviewDragItem;
            if (handler != null)
            {
                handler(Info, new EventArgs());
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
            //List<ListItemGroupOperation> groupOperations = mListItemInfo.GetGroupOperationsList();
            //if (groupOperations == null || groupOperations.Count == 0)
            //{

            //}
        }

        public void UpdateGrouping()
        {
            DoGrouping();
        }

        private void DoGrouping()
        {
            CollectionView groupView = (CollectionView)CollectionViewSource.GetDefaultView(xListView.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription(mGroupByProperty);
            groupView.GroupDescriptions.Clear();
            groupView.GroupDescriptions.Add(groupDescription);
        }

        private void SetGroupOperations(Menu menu)
        {
            List<ListItemGroupOperation> groupOperations = mListItemInfo.GetGroupOperationsList();
            if (groupOperations != null && groupOperations.Count > 0)
            {
                foreach (ListItemGroupOperation operation in groupOperations)
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

                    menuitem.Tag = menu.Tag;

                    menu.Items.Add(menuitem);
                }
            }

        }

        private void XGroupOperationsMenu_Loaded(object sender, RoutedEventArgs e)
        {
            //SetGroupOperations((Menu)sender);
        }
    }

    public class UcListViewEventArgs
        {
            public enum eEventType
            {
                ExpandAllItems,
                CollapseAllItems,
                UpdateIndex,
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
