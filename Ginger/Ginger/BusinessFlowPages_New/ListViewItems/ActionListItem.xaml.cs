using Ginger.UserControlsLib.UCListView;
using GingerCore.Actions;
using GingerCore.GeneralLib;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.BusinessFlowPages_New.ItemsListControls
{
    /// <summary>
    /// Interaction logic for ucActionListItem.xaml
    /// </summary>
    public partial class ActionListItem : UserControl
    {

        public static readonly DependencyProperty ActionProperty = DependencyProperty.Register(nameof(ActionProperty), typeof(Act), typeof(ActionListItem), new PropertyMetadata(null, new PropertyChangedCallback(OnActionPropertyChanged)));
        public Act Action
        {
            get
            {
                return (Act)GetValue(ActionProperty);
            }
            set
            {
                SetValue(ActionProperty, value);                
                //xActionListItem.ConfigItem(item: value, itemNameField: nameof(Act.Description), itemDescriptionField: nameof(Act.ActionType), itemIconField: null, itemExecutionStatusField: nameof(Act.Status), notifications: GetActionNotifications());
            }
        }
        private static void OnActionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ActionListItem;
            if (control != null && e.NewValue != null)
            {
                control.Action = ((Act)e.NewValue);
            }          
        }

        public ActionListItem()
        {
            InitializeComponent();
        }

        private List<ListItemNotification> GetActionNotifications()
        {
            List<ListItemNotification> notificationsList = new List<ListItemNotification>();

            ListItemNotification flowControlInd = new ListItemNotification();
            flowControlInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MapSigns;
            flowControlInd.ToolTip = "Action contains Flow Control conditions";
            flowControlInd.ImageSize = 14;
            flowControlInd.BindingObject = Action;
            flowControlInd.BindingFieldName = nameof(Act.FlowControlsInfo);
            flowControlInd.BindingConverter = new StringVisibilityConverter();         
            notificationsList.Add(flowControlInd);

            ListItemNotification outputValuesInd = new ListItemNotification();
            outputValuesInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Output;
            outputValuesInd.ToolTip = "Action contains Output Values";
            outputValuesInd.BindingObject = Action;
            outputValuesInd.BindingFieldName = nameof(Act.ReturnValuesInfo);
            outputValuesInd.BindingConverter = new StringVisibilityConverter();
            notificationsList.Add(outputValuesInd);

            return notificationsList;
        }
    }



}
