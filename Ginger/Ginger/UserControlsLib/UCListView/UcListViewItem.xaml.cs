using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.UserControls;
using GingerCore.GeneralLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Ginger.UserControlsLib.UCListView
{
    /// <summary>
    /// Interaction logic for ListViewItem.xaml
    /// </summary>
    public partial class UcListViewItem : UserControl
    {
        //public static readonly DependencyProperty ParentListProperty = DependencyProperty.Register(nameof(ParentList), typeof(UcListView), typeof(UcListViewItem), new PropertyMetadata(null, new PropertyChangedCallback(OnParentListPropertyChanged)));
        //public UcListView ParentList
        //{
        //    get
        //    {
        //        return (UcListView)GetValue(ParentListProperty);
        //    }
        //    set
        //    {
        //        SetValue(ParentListProperty, value);                                    
        //    }
        //}
        //private static void OnParentListPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var control = d as UcListViewItem;
        //    if (control != null && e.NewValue != null)
        //    {
        //        control.ParentList = ((UcListView)e.NewValue);
        //    }
        //}
        public UcListView ParentList { get; set; }

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(object), typeof(UcListViewItem), new PropertyMetadata(null, new PropertyChangedCallback(OnItemPropertyChanged)));
        public object Item
        {
            get
            {
                return (object)GetValue(ItemProperty);
            }
            set
            {
                SetValue(ItemProperty, value);
            }
        }
        private static void OnItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as UcListViewItem;
            if (control != null && e.NewValue != null)
            {
                control.Item = ((object)e.NewValue);
            }
        }

        public static readonly DependencyProperty ItemInfoProperty = DependencyProperty.Register(nameof(ItemInfo), typeof(IListViewItemInfo), typeof(UcListViewItem), new PropertyMetadata(null, new PropertyChangedCallback(OnItemInfoPropertyChanged)));
        public IListViewItemInfo ItemInfo
        {
            get
            {
                return (IListViewItemInfo)GetValue(ItemInfoProperty);
            }
            set
            {
                SetValue(ItemInfoProperty, value);          
                //ItemInfo.SetItem(Item);
                ConfigItem();
            }
        }
        private static void OnItemInfoPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = d as UcListViewItem;
            if (c != null && e.NewValue != null)
            {
                c.ItemInfo = ((IListViewItemInfo)e.NewValue);
            }
        }

        //object mItem;
        //public object Item { get { return mItem; } }

        public string ItemNameField { get; set; }
        public string ItemDescriptionField { get; set; }
        public string ItemIconField { get; set; }
        public string ItemExecutionStatusField { get; set; }


        public UcListViewItem()
        {
            InitializeComponent();

            SetInitView();
        }

        private void SetInitView()
        {
            //collapse
            xExtraDetailsRow.Height = new GridLength(0);
        }

        private void ParentList_UcListViewEvent(UcListViewEventArgs EventArgs)
        {
            switch (EventArgs.EventType)
            {
                case UcListViewEventArgs.eEventType.ExpandAllItems:
                    ExpandItem();
                    break;

                case UcListViewEventArgs.eEventType.CollapseAllItems:
                    CollapseItem();
                    break;

                case UcListViewEventArgs.eEventType.UpdateIndex:
                    SetItemIndex();
                    break;
            }
        }

        //public void ConfigItem(object item, string itemNameField, string itemDescriptionField, string itemIconField, string itemExecutionStatusField="", List<ListItemNotification> notifications = null)
        //{
        //    mItem = item;

        //    ItemNameField = itemNameField;
        //    ItemDescriptionField = itemDescriptionField;
        //    ItemIconField = itemIconField;
        //    ItemExecutionStatusField = itemExecutionStatusField;
        public void ConfigItem()
        {
            ItemNameField = ItemInfo.GetItemNameField();
            ItemDescriptionField = ItemInfo.GetItemDescriptionField();
            ItemIconField = ItemInfo.GetItemIconField();
            ItemExecutionStatusField = ItemInfo.GetItemExecutionStatusField();
            SetItemNotifications();
            SetItemOperations();

            SetItemBindings();
        }

        private void SetItemNotifications()
        {
            List<ListItemNotification> notifications = ItemInfo.GetNotificationsList(Item);
            if (notifications != null)
            {
                foreach (ListItemNotification notification in notifications)
                {
                    ImageMakerControl itemInd = new ImageMakerControl();
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

                    xItemNotificationsPnl.Children.Add(itemInd);
                }
            }
        }

        private void SetItemOperations()
        {
            List<ListItemOperation> operations = ItemInfo.GetOperationsList(Item);
            if (operations != null && operations.Count > 0)
            {
                xItemOperationsPnl.Visibility = Visibility.Visible;
                foreach (ListItemOperation operation in operations)
                {
                    ucButton operationBtn = new ucButton();
                    operationBtn.ButtonType = Amdocs.Ginger.Core.eButtonType.ImageButton;
                    operationBtn.ButtonImageType = operation.ImageType;
                    operationBtn.ToolTip = operation.ToolTip;
                    operationBtn.Margin = new Thickness(-5, 0, -5, 0);
                    operationBtn.ButtonImageHeight = 16;
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
                    operationBtn.Tag = Item;

                    xItemOperationsPnl.Children.Add(operationBtn);
                }
            }
            else
            {
                xItemOperationsPnl.Visibility = Visibility.Collapsed;
            }
        }

        private void SetItemBindings()
        {
            BindingHandler.ObjFieldBinding(xItemNameTxtBlock, TextBlock.TextProperty, Item, ItemNameField);

            if (string.IsNullOrEmpty(ItemDescriptionField))
            {
                xItemDescriptionTxtBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                BindingHandler.ObjFieldBinding(xItemDescriptionTxtBlock, TextBlock.TextProperty, Item, ItemDescriptionField, BindingMode.OneWay);
            }

            if (!string.IsNullOrEmpty(ItemIconField))
            {
                BindingHandler.ObjFieldBinding(xItemIcon, ImageMakerControl.ImageTypeProperty, Item, ItemIconField);
            }

            if (string.IsNullOrEmpty(ItemExecutionStatusField))
            {
                xItemIcon.Visibility = Visibility.Collapsed;
            }
            else
            {
                BindingHandler.ObjFieldBinding(xItemStatusImage, UcItemExecutionStatus.StatusProperty, Item, ItemExecutionStatusField);
            }
        }        
        

        private void xDetailViewBtn_Click(object sender, RoutedEventArgs e)
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
            xExtraDetailsRow.Height = new GridLength(25);
            xDetailViewBtn.ButtonImageType = Amdocs.Ginger.Common.Enums.eImageType.Collapse;
            xDetailViewBtn.ToolTip = "Collapse";
        }

        public void CollapseItem()
        {
            xExtraDetailsRow.Height = new GridLength(0);
            xDetailViewBtn.ButtonImageType = Amdocs.Ginger.Common.Enums.eImageType.Expand;
            xDetailViewBtn.ToolTip = "Expand";
        }

        private void xRunnerItemContinue_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UcListViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (ParentList == null)
            {
                var parent = GingerCore.General.TryFindParent<UcListView>(this);
                if (parent != null)
                {
                    ParentList = (UcListView)parent;
                    ParentList.UcListViewEvent -= ParentList_UcListViewEvent;
                    ParentList.UcListViewEvent += ParentList_UcListViewEvent;
                }
            }
            SetItemIndex();
        }

        private void SetItemIndex()
        {
            if (ParentList != null)
            {
                //xItemIndexTxt.Text = (ParentList.List.ItemContainerGenerator.IndexFromContainer(this) + 1).ToString();
                xItemIndexTxt.Text = (ParentList.List.Items.IndexOf(Item) + 1).ToString();
            }
        }
    }
}
