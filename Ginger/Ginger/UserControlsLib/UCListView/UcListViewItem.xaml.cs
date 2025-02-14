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

extern alias UIAComWrapperNetstandard;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Ginger.UserControlsLib.UCListView
{
    /// <summary>
    /// Interaction logic for ListViewItem.xaml
    /// </summary>
    public partial class UcListViewItem : UserControl, INotifyPropertyChanged
    {
        //static int ListViewItemsNum = 0;
        //static int LiveListViewItemsCounter = 0;
        //~UcListViewItem()
        //{
        //    LiveListViewItemsCounter--;
        //}

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public UcListView ParentList { get; set; }

        public bool IsSelected
        {
            get
            {
                if (ParentList != null && ParentList.CurrentItem == Item)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(object), typeof(UcListViewItem), new PropertyMetadata(null, new PropertyChangedCallback(OnItemPropertyChanged)));
        public object Item
        {
            get
            {
                return GetValue(ItemProperty);
            }
            set
            {
                SetValue(ItemProperty, value);
            }
        }
        private static void OnItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UcListViewItem control && e.NewValue != null)
            {
                control.Item = ((object)e.NewValue);
            }
        }

        public static readonly DependencyProperty ListHelperProperty = DependencyProperty.Register(nameof(ListHelper), typeof(IListViewHelper), typeof(UcListViewItem), new PropertyMetadata(null, new PropertyChangedCallback(OnItemInfoPropertyChanged)));
        public IListViewHelper ListHelper
        {
            get
            {
                return (IListViewHelper)GetValue(ListHelperProperty);
            }
            set
            {
                SetValue(ListHelperProperty, value);
                SetInitViewWithHelper();
                SetItemMainView();
            }
        }
        private static void OnItemInfoPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UcListViewItem c && e.NewValue != null)
            {
                c.ListHelper = ((IListViewHelper)e.NewValue);
            }
        }

        string mItemNameField;
        string mItemMandatoryField;
        string mItemDescriptionField;
        string mItemErrorField;
        string mItemNameExtentionField;
        string mItemTagsField;
        string mItemIconField;
        string mItemIconTooltipField;
        string mItemExecutionStatusField;
        string mItemActiveField;

        bool mMainViewWasSet;
        bool mSubViewWasSet;

        public UcListViewItem()
        {
            InitializeComponent();

            //ListViewItemsNum++;
            //LiveListViewItemsCounter++;

            SetInitView();
        }

        private void SetInitView()
        {
            CollapseItem();
        }

        private void SetInitViewWithHelper()
        {
            if (ListHelper != null)
            {
                if (ListHelper.AllowExpandItems == false)
                {
                    CollapseItem();
                    xExpandCollapseBtn.Visibility = Visibility.Collapsed;
                }
                else if (ListHelper.ExpandItemOnLoad)
                {
                    ExpandItem();
                }

                if (!ListHelper.ShowIndex)
                {
                    xIndexCol.Width = new GridLength(0);
                    xExpandCollapseCol.Width = new GridLength(20);
                }
            }
        }

        private void SetItemMainView()
        {
            if (!mMainViewWasSet)
            {
                mItemIconField = ListHelper.GetItemIconField();
                mItemIconTooltipField = ListHelper.GetItemIconTooltipField();
                mItemNameField = ListHelper.GetItemNameField();
                mItemMandatoryField = ListHelper.GetItemMandatoryField();
                mItemNameExtentionField = ListHelper.GetItemNameExtentionField();
                mItemExecutionStatusField = ListHelper.GetItemExecutionStatusField();
                mItemActiveField = ListHelper.GetItemActiveField();

                this.Dispatcher.Invoke(() =>
                {
                    if (Item is RepositoryItemBase repositoryItemBase)
                    {
                        string allProperties = string.Empty;
                        PropertyChangedEventManager.RemoveHandler(source: repositoryItemBase, handler: Item_PropertyChanged, propertyName: allProperties);
                        PropertyChangedEventManager.AddHandler(source: repositoryItemBase, handler: Item_PropertyChanged, propertyName: allProperties);
                    }

                    if (!string.IsNullOrEmpty(mItemIconField))
                    {
                        BindingHandler.ObjFieldBinding(xItemIcon, ImageMakerControl.ImageTypeProperty, Item, mItemIconField, BindingMode: BindingMode.OneWay);
                    }
                    if (!string.IsNullOrEmpty(mItemIconTooltipField))
                    {
                        BindingHandler.ObjFieldBinding(xItemIcon, ImageMakerControl.ImageToolTipProperty, Item, mItemIconTooltipField, BindingMode: BindingMode.OneWay);
                    }

                    SetItemFullName();

                    SetItemUniqueIdentifier();

                    SetItemNotifications();

                    if (string.IsNullOrEmpty(mItemExecutionStatusField))
                    {
                        xItemStatusImage.Visibility = Visibility.Collapsed;
                        xItemStatusClm.Width = new GridLength(0);
                    }
                    else
                    {
                        BindingHandler.ObjFieldBinding(xItemStatusImage, UcItemExecutionStatus.StatusProperty, Item, mItemExecutionStatusField);
                        xItemStatusClm.Width = new GridLength(25);
                    }

                    if (!string.IsNullOrEmpty(mItemActiveField))
                    {
                        System.Windows.Data.Binding b = new System.Windows.Data.Binding
                        {
                            Source = Item,
                            Path = new PropertyPath(mItemActiveField),
                            Mode = BindingMode.OneWay,
                            Converter = new ActiveBackgroundColorConverter(),
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        };
                        xMainBorder.SetBinding(Border.BackgroundProperty, b);
                    }
                });
            }

            mMainViewWasSet = true;
        }


        private void SetItemSubView()
        {
            if (!mSubViewWasSet)
            {
                mItemDescriptionField = ListHelper.GetItemDescriptionField();
                mItemTagsField = ListHelper.GetItemTagsField();
                mItemErrorField = ListHelper.GetItemErrorField();

                this.Dispatcher.Invoke(() =>
                {
                    SetItemDescription();

                    SetItemOperations();

                    SetItemExtraOperations();

                    SetItemExecutionOperations();
                });
            }

            mSubViewWasSet = true;
        }

        public void ClearBindings()
        {
            Dispatcher.Invoke(() =>
            {
                WeakEventManager<UcListView, UcListViewEventArgs>.RemoveHandler(source: ParentList, eventName: nameof(UcListView.UcListViewEvent), handler: ParentList_UcListViewEvent);
                WeakEventManager<Selector, SelectionChangedEventArgs>.RemoveHandler(source: ParentList.List, eventName: nameof(Selector.SelectionChanged), handler: ParentList_SelectionChanged);
                PropertyChangedEventManager.RemoveHandler(source: (RepositoryItemBase)Item, handler: Item_PropertyChanged, propertyName: string.Empty);

                BindingOperations.ClearAllBindings(xItemIcon);
                foreach (ImageMakerControl notification in xItemNotificationsPnl.Children)
                {
                    BindingOperations.ClearAllBindings(notification);
                }
                BindingOperations.ClearAllBindings(xItemStatusImage);
                BindingOperations.ClearAllBindings(xListItemGrid);
                foreach (ucButton operation in xItemOperationsPnl.Children)
                {
                    BindingOperations.ClearAllBindings(operation);
                }
                foreach (MenuItem extraOperation in xItemExtraOperationsMenu.Items)
                {
                    BindingOperations.ClearAllBindings(extraOperation);
                }
                foreach (ucButton executionOperation in xItemExecutionOperationsPnl.Children)
                {
                    BindingOperations.ClearAllBindings(executionOperation);
                }

                this.ClearControlsBindings();
            });
        }

        private void ParentList_UcListViewEvent(object? sender, UcListViewEventArgs EventArgs)
        {
            switch (EventArgs.EventType)
            {
                case UcListViewEventArgs.eEventType.ExpandAllItems:
                    ExpandItem();
                    break;

                case UcListViewEventArgs.eEventType.ExpandItem:
                    if (Item == EventArgs.EventObject)
                    {
                        ExpandItem();
                    }
                    break;

                case UcListViewEventArgs.eEventType.CollapseAllItems:
                    CollapseItem();
                    break;

                case UcListViewEventArgs.eEventType.UpdateIndex:
                    SetItemIndex();
                    break;

                case UcListViewEventArgs.eEventType.ClearBindings:
                    ClearBindings();
                    break;
            }
        }

        private void SetItemUniqueIdentifier()
        {
            this.Dispatcher.Invoke(() =>
            {
                ListItemUniqueIdentifier identifier = ListHelper.GetItemUniqueIdentifier(Item);
                if (identifier != null)
                {
                    xIdentifierCol.Width = new GridLength(8);
                    if (!String.IsNullOrEmpty(identifier.Color))
                    {
                        BrushConverter conv = new BrushConverter();
                        xIdentifierBorder.Background = conv.ConvertFromString(identifier.Color) as SolidColorBrush;
                    }
                    xIdentifierBorder.ToolTip = identifier.Tooltip;
                    xIdentifierBorder.Visibility = Visibility.Visible;
                }
                else
                {
                    xIdentifierCol.Width = new GridLength(0);
                    xIdentifierBorder.Background = System.Windows.Media.Brushes.Transparent;
                    xIdentifierBorder.ToolTip = string.Empty;
                    xIdentifierBorder.Visibility = Visibility.Collapsed;
                }
            });
        }

        private void SetItemNotifications()
        {
            this.Dispatcher.Invoke(() =>
            {
                List<ListItemNotification> notifications = ListHelper.GetItemNotificationsList(Item);
                if (notifications != null)
                {
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

                        if (notification.ImageTypeBindingConverter != null)
                        {
                            BindingHandler.ObjFieldBinding(itemInd, ImageMakerControl.ImageTypeProperty, notification.BindingObject, notification.ImageTypeBindingFieldName, bindingConvertor: notification.ImageTypeBindingConverter, BindingMode.OneWay);
                        }

                        if (notification.TooltipBindingConverter != null)
                        {
                            BindingHandler.ObjFieldBinding(itemInd, ImageMakerControl.ToolTipProperty, notification.BindingObject, notification.TooltipBindingFieldName, bindingConvertor: notification.TooltipBindingConverter, BindingMode.OneWay);
                        }

                        xItemNotificationsPnl.Children.Add(itemInd);
                        xItemNotificationsClm.Width = new GridLength(xItemNotificationsClm.Width.Value + itemInd.Width + 10);
                    }
                }
            });
        }

        private void SetItemOperations()
        {
            this.Dispatcher.Invoke(() =>
            {
                List<ListItemOperation> operations = ListHelper.GetItemOperationsList(Item);
                if (operations != null && operations.Count > 0)
                {
                    xItemOperationsPnl.Visibility = Visibility.Visible;
                    foreach (ListItemOperation operation in operations.Where(x => x.SupportedViews.Contains(ListHelper.PageViewMode)).ToList())
                    {
                        ucButton operationBtn = new ucButton();
                        operationBtn.SetValue(AutomationProperties.AutomationIdProperty, operation.AutomationID);
                        operationBtn.ButtonType = Amdocs.Ginger.Core.eButtonType.ImageButton;
                        operationBtn.ButtonImageType = operation.ImageType;
                        operationBtn.ToolTip = operation.ToolTip;
                        operationBtn.Margin = new Thickness(-5, 0, -5, 0);
                        operationBtn.ButtonFontImageSize = operation.ImageSize;
                        operationBtn.IsEnabled = operation.IsEnabeled;

                        if (operation.ImageForeground != null)
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
                        operationBtn.Tag = Item;

                        xItemOperationsPnl.Children.Add(operationBtn);
                    }
                }

                if (xItemOperationsPnl.Children.Count == 0)
                {
                    xItemOperationsPnl.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ShowItemMainOperationsPnl(true);
                }
            });
        }

        private void SetItemExtraOperations()
        {
            this.Dispatcher.Invoke(() =>
            {
                List<ListItemOperation> extraOperations = ListHelper.GetItemExtraOperationsList(Item);
                if (extraOperations != null && extraOperations.Count > 0)
                {
                    xItemExtraOperationsMenu.Visibility = Visibility.Visible;
                    foreach (ListItemOperation operation in extraOperations.Where(x => x.SupportedViews.Contains(ListHelper.PageViewMode)).ToList())
                    {
                        MenuItem menuitem = new MenuItem();
                        menuitem.SetValue(AutomationProperties.AutomationIdProperty, operation.AutomationID);
                        menuitem.Style = (Style)FindResource("$MenuItemStyle");
                        ImageMakerControl iconImage = new ImageMakerControl
                        {
                            ImageType = operation.ImageType,
                            SetAsFontImageWithSize = operation.ImageSize,
                            HorizontalAlignment = HorizontalAlignment.Left
                        };
                        menuitem.Icon = iconImage;
                        menuitem.Header = operation.Header;
                        menuitem.ToolTip = operation.ToolTip;

                        if (operation.ImageForeground != null)
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

                        menuitem.Tag = Item;

                        if (string.IsNullOrEmpty(operation.Group))
                        {
                            ((MenuItem)(xItemExtraOperationsMenu.Items[0])).Items.Add(menuitem);
                        }
                        else
                        {
                            //need to add to Group
                            bool addedToGroup = false;
                            foreach (MenuItem item in ((MenuItem)(xItemExtraOperationsMenu.Items[0])).Items)
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
                                MenuItem groupMenuitem = new MenuItem
                                {
                                    Style = (Style)FindResource("$MenuItemStyle")
                                };
                                ImageMakerControl groupIconImage = new ImageMakerControl
                                {
                                    ImageType = operation.GroupImageType,
                                    SetAsFontImageWithSize = operation.ImageSize,
                                    HorizontalAlignment = HorizontalAlignment.Left
                                };
                                groupMenuitem.Icon = groupIconImage;
                                groupMenuitem.Header = operation.Group;
                                groupMenuitem.ToolTip = operation.Group;
                                ((MenuItem)(xItemExtraOperationsMenu.Items[0])).Items.Add(groupMenuitem);
                                groupMenuitem.Items.Add(menuitem);
                            }
                        }
                    }
                }

                if (((MenuItem)(xItemExtraOperationsMenu.Items[0])).Items.Count == 0)
                {
                    xItemExtraOperationsMenu.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ShowItemMainOperationsPnl(true);
                }
            });
        }

        private void SetItemExecutionOperations()
        {
            this.Dispatcher.Invoke(() =>
            {
                List<ListItemOperation> executionOperations = ListHelper.GetItemExecutionOperationsList(Item);
                if (executionOperations != null && executionOperations.Count > 0)
                {
                    xOperationsSplitter.Visibility = Visibility.Visible;
                    xItemExecutionOperationsPnl.Visibility = Visibility.Visible;
                    foreach (ListItemOperation operation in executionOperations.Where(x => x.SupportedViews.Contains(ListHelper.PageViewMode)).ToList())
                    {
                        ucButton operationBtn = new ucButton();
                        operationBtn.SetValue(AutomationProperties.AutomationIdProperty, operation.AutomationID);
                        operationBtn.ButtonType = Amdocs.Ginger.Core.eButtonType.ImageButton;
                        operationBtn.ButtonImageType = operation.ImageType;
                        operationBtn.ToolTip = operation.ToolTip;
                        operationBtn.Margin = new Thickness(-5, 0, -5, 0);
                        operationBtn.ButtonImageHeight = 15;
                        operationBtn.ButtonImageWidth = 15;
                        operationBtn.ButtonFontImageSize = operation.ImageSize;
                        operationBtn.ButtonStyle = (Style)FindResource("$ImageButtonStyle_Execution");

                        if (operation.ImageForeground == null)
                        {
                            //operationBtn.ButtonImageForground = (SolidColorBrush)FindResource("$BackgroundColor_Black");
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
                        operationBtn.Tag = Item;

                        xItemExecutionOperationsPnl.Children.Add(operationBtn);
                    }
                }

                if (xItemExecutionOperationsPnl.Children.Count == 0)
                {
                    xOperationsSplitter.Visibility = Visibility.Collapsed;
                    xItemExecutionOperationsPnl.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ShowItemMainOperationsPnl(true);
                }
            });
        }


        private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == mItemNameField || e.PropertyName == mItemNameExtentionField || e.PropertyName == mItemMandatoryField)
            {
                SetItemFullName();
            }
            else if (e.PropertyName == mItemDescriptionField || e.PropertyName == mItemTagsField || e.PropertyName == mItemErrorField)
            {
                SetItemDescription();
            }
            SetItemUniqueIdentifier();
        }

        private void xExpandCollapseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (xExtraDetailsRow.ActualHeight == 0)
            {
                //expand
                ExpandItem();
            }
            else
            {
                //collapse
                CollapseItem();
            }
        }

        public void ExpandItem()
        {
            if (ListHelper.AllowExpandItems == false)
            {
                return;
            }

            this.Dispatcher.Invoke(() =>
                {
                    xExtraDetailsRow.Height = new GridLength(25);
                    xExpandCollapseBtn.ButtonImageType = Amdocs.Ginger.Common.Enums.eImageType.Collapse;
                    xExpandCollapseBtn.ToolTip = "Collapse";
                    SetItemSubView();
                });
        }

        public void CollapseItem()
        {
            this.Dispatcher.Invoke(() =>
            {
                xExtraDetailsRow.Height = new GridLength(0);
                xExpandCollapseBtn.ButtonImageType = Amdocs.Ginger.Common.Enums.eImageType.Expand;
                xExpandCollapseBtn.ToolTip = "Expand";
            });
        }

        private void UcListViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (ParentList == null)
                {
                    var parent = GingerCore.General.TryFindParent<UcListView>(this);
                    if (parent != null)
                    {
                        ParentList = parent;
                        WeakEventManager<UcListView, UcListViewEventArgs>.RemoveHandler(ParentList, nameof(UcListView.UcListViewEvent), ParentList_UcListViewEvent);
                        WeakEventManager<UcListView, UcListViewEventArgs>.AddHandler(ParentList, nameof(UcListView.UcListViewEvent), ParentList_UcListViewEvent);
                        WeakEventManager<Selector, SelectionChangedEventArgs>.RemoveHandler(ParentList.List, nameof(Selector.SelectionChanged), ParentList_SelectionChanged);
                        WeakEventManager<Selector, SelectionChangedEventArgs>.AddHandler(ParentList.List, nameof(Selector.SelectionChanged), ParentList_SelectionChanged);
                    }
                    OnPropertyChanged(nameof(IsSelected));
                }
                SetItemIndex();
            });
        }

        private void ParentList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(IsSelected));
        }

        private void SetItemIndex()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (ParentList != null)
                {
                    //xItemIndexTxt.Text = (ParentList.List.ItemContainerGenerator.IndexFromContainer(this) + 1).ToString();
                    xItemIndexTxt.Text = (ParentList.List.Items.IndexOf(Item) + 1).ToString();
                }
            });
        }

        private void SetItemFullName()
        {
            this.Dispatcher.Invoke(() =>
            {
                try
                {
                    AlignGridandTextWidth();
                    xDetailsGrid.SizeChanged -= XDetailsGrid_SizeChanged;
                    xDetailsGrid.SizeChanged += XDetailsGrid_SizeChanged;
                    xItemNameTxtBlock.Text = string.Empty;
                    xItemExtraInfoTxtBlock.Text = string.Empty;
                    xItemExtraInfoTxtBlock.Visibility = Visibility.Collapsed;
                    string fullname = string.Empty;
                    if (!string.IsNullOrEmpty(mItemNameField))
                    {
                        Object name = Item.GetType().GetProperty(mItemNameField).GetValue(Item);
                        if (name != null)
                        {
                            xItemNameTxtBlock.Inlines.Add(new System.Windows.Documents.Run
                            {
                                FontSize = 15,
                                Text = name.ToString()
                            });
                            fullname += name;
                        }
                    }
                    if (!string.IsNullOrEmpty(mItemMandatoryField))
                    {
                        bool isMandatory = (bool)Item.GetType().GetProperty(mItemMandatoryField).GetValue(Item);
                        if (isMandatory)
                        {
                            xItemExtraInfoTxtBlock.Visibility = Visibility.Visible;
                            xItemExtraInfoTxtBlock.Inlines.Add(new System.Windows.Documents.Run
                            {
                                FontSize = 18,
                                Text = "*",
                                FontWeight = FontWeights.Bold,
                                Foreground = Brushes.Red
                            });
                            fullname += "*";
                        }
                    }
                    if (!string.IsNullOrEmpty(mItemNameExtentionField))
                    {
                        Object extension = Item.GetType().GetProperty(mItemNameExtentionField).GetValue(Item);
                        if (extension != null)
                        {
                            xItemExtraInfoTxtBlock.Visibility = Visibility.Visible;
                            xItemExtraInfoTxtBlock.Inlines.Add(new System.Windows.Documents.Run
                            {
                                FontSize = 11,
                                Text = string.Format(" [{0}]", extension.ToString())
                            });

                            fullname += string.Format(" [{0}]", extension.ToString());
                        }
                    }

                    xItemNameTxtBlock.ToolTip = fullname;
                    xItemExtraInfoTxtBlock.ToolTip = fullname;
                }
                catch (Exception ex)
                {
                    xItemDescriptionTxtBlock.Text = "Failed to set Name!;";
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to set ListViewItem Name", ex);
                }
            });
        }

        private void AlignGridandTextWidth()
        {
            if (xItemNameColumn.ActualWidth > 200)
            {
                xItemNameTxtBlock.MaxWidth = xItemNameColumn.ActualWidth - 100;
                xItemDescriptionTxtBlock.MaxWidth = xItemNameTxtBlock.MaxWidth;
            }
        }

        private void XDetailsGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AlignGridandTextWidth();
        }

        private void SetItemDescription()
        {
            this.Dispatcher.Invoke(() =>
            {

                try
                {
                    string fullDesc = string.Empty;
                    bool errorWasSet = false;
                    xItemDescriptionTxtBlock.Foreground = FindResource("$BackgroundColor_Black") as Brush;
                    if (!string.IsNullOrEmpty(mItemErrorField))
                    {
                        Object error = Item.GetType().GetProperty(mItemErrorField).GetValue(Item);
                        if (error != null)
                        {
                            fullDesc += error.ToString();
                            xItemDescriptionTxtBlock.Foreground = FindResource("$FailedStatusColor") as Brush;
                            xItemDescriptionTxtBlock.Text = "Error: " + fullDesc;
                            xItemDescriptionTxtBlock.ToolTip = "Error: " + fullDesc;
                            errorWasSet = true;
                        }
                    }

                    if (!errorWasSet)
                    {
                        if (!string.IsNullOrEmpty(mItemDescriptionField))
                        {
                            Object desc = Item.GetType().GetProperty(mItemDescriptionField).GetValue(Item);
                            if (desc != null)
                            {
                                fullDesc += desc.ToString() + " ";
                            }
                        }

                        if (!string.IsNullOrEmpty(mItemTagsField))
                        {
                            Object tags = Item.GetType().GetField(mItemTagsField).GetValue(Item);
                            fullDesc += General.GetTagsListAsString((ObservableList<Guid>)tags) + " ";
                        }


                        xItemDescriptionTxtBlock.Text = fullDesc;
                        if (!string.IsNullOrEmpty(fullDesc))
                        {
                            xItemDescriptionTxtBlock.ToolTip = fullDesc;
                        }
                    }
                }
                catch (Exception ex)
                {
                    xItemDescriptionTxtBlock.Text = "Failed to set description!;";
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to set ListViewItem Description", ex);
                }
            });
        }

        private void XListItemGrid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ExpandItem();
        }

        private void XListItemGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (xItemOperationsPnl.Children.Count > 0 || ((MenuItem)(xItemExtraOperationsMenu.Items[0])).Items.Count > 0 || xItemExecutionOperationsPnl.Children.Count > 0)
            {
                ShowItemMainOperationsPnl(true);
            }
        }


        private void XListItemGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ShowItemMainOperationsPnl(false);
        }

        private void ShowItemMainOperationsPnl(bool toShow)
        {
            if (toShow)
            {
                xItemOperationsMainPnl.Visibility = Visibility.Visible;
                xItemOperationsClm.Width = new GridLength(175);
            }
            else
            {
                xItemOperationsMainPnl.Visibility = Visibility.Collapsed;
                xItemOperationsClm.Width = new GridLength(0);
            }
        }
    }

    public class ActiveBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is bool && ((bool)value) == false)
            {
                return System.Windows.Media.Brushes.LightGray;
            }
            else
            {
                return System.Windows.Media.Brushes.Transparent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
