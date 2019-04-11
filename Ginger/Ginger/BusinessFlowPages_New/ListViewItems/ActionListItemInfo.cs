using Ginger.UserControlsLib.UCListView;
using GingerCore.Actions;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Ginger.BusinessFlowPages_New.ListViewItems
{
    public class ActionListItemInfo : IListViewItemInfo
    {
        Act mAction;

        public void SetItem(object item)
        {
            mAction = (Act)item;
        }

        public string GetItemNameField()
        {
            return nameof(Act.Description);
        }

        public string GetItemDescriptionField()
        {
            return nameof(Act.ActionType);
        }

        public string GetItemExecutionStatusField()
        {
            return nameof(Act.Status);
        }

        public string GetItemIconField()
        {
            return null;
        }

        public List<ListItemNotification> GetNotificationsList()
        {
            List<ListItemNotification> notificationsList = new List<ListItemNotification>();

            ListItemNotification flowControlInd = new ListItemNotification();
            flowControlInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MapSigns;
            flowControlInd.ToolTip = "Action contains Flow Control conditions";
            flowControlInd.ImageSize = 14;
            flowControlInd.BindingObject = mAction;
            flowControlInd.BindingFieldName = nameof(Act.FlowControlsInfo);
            flowControlInd.BindingConverter = new StringVisibilityConverter();
            notificationsList.Add(flowControlInd);

            ListItemNotification outputValuesInd = new ListItemNotification();
            outputValuesInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Output;
            outputValuesInd.ToolTip = "Action contains Output Values";
            outputValuesInd.BindingObject = mAction;
            outputValuesInd.BindingFieldName = nameof(Act.ReturnValuesInfo);
            outputValuesInd.BindingConverter = new StringVisibilityConverter();
            notificationsList.Add(outputValuesInd);

            ListItemNotification waitInd = new ListItemNotification();
            waitInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Clock;
            waitInd.ToolTip = "Action contains Wait time before execution starts";
            //waitInd.ImageSize = 14;
            waitInd.BindingObject = mAction;
            waitInd.BindingFieldName = nameof(Act.WaitVE);
            waitInd.BindingConverter = new WaitVisibilityConverter();
            notificationsList.Add(waitInd);

            ListItemNotification retryInd = new ListItemNotification();
            retryInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Retry;
            retryInd.ToolTip = "Action configured to Rerun in case of failure";
            //retryInd.ImageSize = 14;
            retryInd.BindingObject = mAction;
            retryInd.BindingFieldName = nameof(Act.EnableRetryMechanism);
            retryInd.BindingConverter = new BoolVisibilityConverter();
            notificationsList.Add(retryInd);

            ListItemNotification screenshotInd = new ListItemNotification();
            screenshotInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Image;
            screenshotInd.ToolTip = "Action configured to take Screenshot";
            //retryInd.ImageSize = 14;
            screenshotInd.BindingObject = mAction;
            screenshotInd.BindingFieldName = nameof(Act.TakeScreenShot);
            screenshotInd.BindingConverter = new BoolVisibilityConverter();
            notificationsList.Add(screenshotInd);

            ListItemNotification sharedRepoInd = new ListItemNotification();
            sharedRepoInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem;
            sharedRepoInd.ToolTip = "Action source is from Shared Repository";
            sharedRepoInd.ImageForeground = Brushes.Orange;
            sharedRepoInd.BindingObject = mAction;
            sharedRepoInd.BindingFieldName = nameof(Act.IsSharedRepositoryInstance);
            sharedRepoInd.BindingConverter = new BoolVisibilityConverter();
            notificationsList.Add(sharedRepoInd);

            return notificationsList;
        }

    }

    public class WaitVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()) || value.ToString() == "0")
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
