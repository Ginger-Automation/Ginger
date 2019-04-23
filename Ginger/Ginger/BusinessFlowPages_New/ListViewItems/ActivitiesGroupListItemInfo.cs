using Amdocs.Ginger.Common;
using Amdocs.Ginger.UserControls;
using Ginger.UserControlsLib.UCListView;
using GingerCore.Activities;
using System.Collections.Generic;

namespace Ginger.BusinessFlowPages.ListViewItems
{
    public class ActivitiesGroupListItemInfo : IListViewItemInfo
    {
        ActivitiesGroup mActivitiesGroup;
        Context mContext;

        public ActivitiesGroupListItemInfo(Context context)
        {
            mContext = context;
        }

        public void SetItem(object item)
        {
            if (item is ActivitiesGroup)
            {
                mActivitiesGroup = (ActivitiesGroup)item;
            }
            else if (item is ucButton)
            {
                mActivitiesGroup = (ActivitiesGroup)(((ucButton)item).Tag);
            }
        }

        public string GetItemNameField()
        {
            return nameof(ActivitiesGroup.Name);
        }

        public string GetItemDescriptionField()
        {
            return nameof(ActivitiesGroup.Description);
        }

        public string GetItemGroupField()
        {
            return null;
        }

        public string GetItemTagsField()
        {
            return nameof(ActivitiesGroup.Tags);
        }

        public string GetItemExecutionStatusField()
        {
            return nameof(ActivitiesGroup.RunStatus);
        }

        public string GetItemActiveField()
        {
            return null;
        }

        public string GetItemIconField()
        {
            return null;//TODO: return ActivitiesGroup image type
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
