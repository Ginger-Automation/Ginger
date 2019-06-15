using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.ALM;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using GingerCore.Activities;
using GingerCore.GeneralLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            //if (!string.IsNullOrEmpty(mActivity.ActivitiesGroupID))
            //{
            //    return new ListItemUniqueIdentifier() { Color = mActivity.ActivitiesGroupColor, Tooltip = mActivity.ActivitiesGroupID };
            //}
            //else 
            if (mActivity.AddDynamicly)
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

        public List<ListItemGroupOperation> GetGroupOperationsList()
        {
            List<ListItemGroupOperation> groupOperationsList = new List<ListItemGroupOperation>();

            ListItemGroupOperation rename = new ListItemGroupOperation();
            rename.Header = "Rename";
            rename.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit;
            rename.ToolTip = "Rename group";
            rename.OperationHandler = RenameGroupHandler;
            groupOperationsList.Add(rename);

            ListItemGroupOperation moveUp = new ListItemGroupOperation();
            moveUp.Header = "Move Up";
            moveUp.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveUp;
            moveUp.ToolTip = "Move all group up";
            moveUp.OperationHandler = MoveGroupUpHandler;
            groupOperationsList.Add(moveUp);

            ListItemGroupOperation moveDown = new ListItemGroupOperation();
            moveDown.Header = "Move Down";
            moveDown.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveDown;
            moveDown.ToolTip = "Move all group down";
            moveDown.OperationHandler = MoveGroupDownHandler;
            groupOperationsList.Add(moveDown);

            ListItemGroupOperation delete = new ListItemGroupOperation();
            delete.Header = "Delete";
            delete.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
            delete.ToolTip = "Delete all group " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            delete.OperationHandler = DeleteGroupHandler;
            groupOperationsList.Add(delete);

            ListItemGroupOperation addToSR = new ListItemGroupOperation();
            addToSR.Header = "Add to Shared Repository";
            addToSR.ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem;
            addToSR.ToolTip = "Add group and it " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " to Shared Repository";
            addToSR.OperationHandler = AddGroupToSRHandler;
            groupOperationsList.Add(addToSR);

            ListItemGroupOperation export = new ListItemGroupOperation();
            export.Header = "Export";
            export.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Share;
            export.ToolTip = "Export group and it " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " to ALM";
            export.OperationHandler = ExportGroupHandler;
            groupOperationsList.Add(export);

            return groupOperationsList;
        }


        private void RenameGroupHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == ((MenuItem)sender).Tag.ToString()).FirstOrDefault();

            string newName = activitiesGroup.Name;
            if (InputBoxWindow.GetInputWithValidation("Rename Group", "New Group Name:", ref newName))
            {
                if (!string.IsNullOrEmpty(newName))
                {
                    if (mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name.Trim() == newName.Trim()).FirstOrDefault() == null)
                    {
                        activitiesGroup.ChangeName(newName);
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Group with same name already exist, please set unique name.");
                    }
                }
            }
        }

        private void MoveGroupUpHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == ((MenuItem)sender).Tag.ToString()).FirstOrDefault();
            mContext.BusinessFlow.MoveActivitiesGroupUp(activitiesGroup);
        }

        private void MoveGroupDownHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == ((MenuItem)sender).Tag.ToString()).FirstOrDefault();
            mContext.BusinessFlow.MoveActivitiesGroupDown(activitiesGroup);
        }

        private void DeleteGroupHandler(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.SureWantToDeleteGroup) == eUserMsgSelection.Yes)
            {
                ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == ((MenuItem)sender).Tag.ToString()).FirstOrDefault();
                mContext.BusinessFlow.DeleteActivitiesGroup(activitiesGroup);
            }
        }

        private void AddGroupToSRHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == ((MenuItem)sender).Tag.ToString()).FirstOrDefault();
            List<RepositoryItemBase> list = new List<RepositoryItemBase>();
            list.Add(activitiesGroup);
            (new Repository.SharedRepositoryOperations()).AddItemsToRepository(mContext, list);
        }

        private void ExportGroupHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == ((MenuItem)sender).Tag.ToString()).FirstOrDefault();
            ObservableList<ActivitiesGroup> list = new ObservableList<ActivitiesGroup>();
            list.Add(activitiesGroup);
            ALMIntegration.Instance.ExportBfActivitiesGroupsToALM(mContext.BusinessFlow, list);
        }
    }
}
