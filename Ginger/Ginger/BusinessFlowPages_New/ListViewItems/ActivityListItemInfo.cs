using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.BusinessFlowPages.ListViewItems
{
    public class ActivityListItemInfo : IListViewItemInfo
    {
        Activity mActivity;
        Context mContext;

        public ActivityListItemInfo(Context context)
        {
            mContext = context;
        }

        public void SetItem(object item)
        {
            if (item is Activity)
            {
                mActivity = (Activity)item;
            }
            else if (item is ucButton)
            {
                mActivity = (Activity)(((ucButton)item).Tag);
            }
        }

        public string GetItemNameField()
        {
            return nameof(Activity.ActivityName);
        }

        public string GetItemDescriptionField()
        {
            return nameof(Activity.Description);
        }

        public string GetItemGroupField()
        {
            return nameof(Activity.ActivitiesGroupID);
        }

        public string GetItemTagsField()
        {
            return nameof(Activity.Tags);
        }

        public string GetItemExecutionStatusField()
        {
            return nameof(Activity.Status);
        }

        public string GetItemActiveField()
        {
            return nameof(Activity.Active);
        }

        public ListItemUniqueIdentifier GetItemUniqueIdentifier(object item)
        {
            SetItem(item);
            if (!string.IsNullOrEmpty(mActivity.ActivitiesGroupID))
            {
                return new ListItemUniqueIdentifier() { Color = mActivity.ActivitiesGroupColor, Tooltip = mActivity.ActivitiesGroupID };
            }
            else if (mActivity.AddDynamicly)
            {
                return new ListItemUniqueIdentifier() { Color = "Plum", Tooltip = "Added Dynamically from Shared Repository" };
            }
            else
            {
                return null;
            }
        }

        public string GetItemIconField()
        {
            return nameof(RepositoryItemBase.ItemImageType);
        }

        public List<ListItemNotification> GetNotificationsList(object item)
        {
            SetItem(item);
            List<ListItemNotification> notificationsList = new List<ListItemNotification>();
            return notificationsList;
        }

        public List<ListItemOperation> GetOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> operationsList = new List<ListItemOperation>();

            //ListItemOperation edit = new ListItemOperation();
            //edit.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit;
            //edit.ToolTip = "Edit Action";
            //edit.OperationHandler = EditHandler;
            //operationsList.Add(edit);


            //ListItemOperation active = new ListItemOperation();
            //active.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active;
            //active.ImageBindingObject = mAction;
            //active.ImageBindingFieldName = nameof(Act.Active);
            //active.ImageBindingConverter = new ActiveImageTypeConverter();
            //active.ToolTip = "Activate/Un-Activate Action";
            ////active.ImageSize = 15;
            //active.OperationHandler = ActiveHandler;
            //operationsList.Add(active);

            return operationsList;
        }

        //private void EditHandler(object sender, RoutedEventArgs e)
        //{
        //    SetItem(sender);
        //    mAction.SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper();
        //    mAction.Context = mContext;
        //    ActionEditPage actedit = new ActionEditPage(mAction, General.RepositoryItemPageViewMode.Automation);//TODO: check if need diifrent mode
        //    //actedit.ap = this;
        //    actedit.ShowAsWindow();
        //}

        //private void ActiveHandler(object sender, RoutedEventArgs e)
        //{
        //    SetItem(sender);
        //    mAction.Active = !mAction.Active;
        //}
    }
}
