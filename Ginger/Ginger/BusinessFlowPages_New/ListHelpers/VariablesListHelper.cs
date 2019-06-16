using Amdocs.Ginger.Common;
using Amdocs.Ginger.UserControls;
using Ginger.UserControlsLib.UCListView;
using GingerCore.Variables;
using System;
using System.Collections.Generic;

namespace Ginger.BusinessFlowPages.ListHelpers
{
    public class VariablesListHelper : IListViewHelper
    {
        VariableBase mVariable;
        Context mContext;

        public delegate void VariabelListItemEventHandler(VariabelListItemEventArgs EventArgs);
        public event VariabelListItemEventHandler VariabelListItemEvent;
        private void OnActionListItemEvent(VariabelListItemEventArgs.eEventType eventType, Object eventObject = null)
        {
            VariabelListItemEventHandler handler = VariabelListItemEvent;
            if (handler != null)
            {
                handler(new VariabelListItemEventArgs(eventType, eventObject));
            }
        }

        public VariablesListHelper(Context context)
        {
            mContext = context;
        }

        public void SetItem(object item)
        {
            if (item is VariableBase)
            {
                mVariable = (VariableBase)item;
            }
            else if (item is ucButton)
            {
                mVariable = (VariableBase)(((ucButton)item).Tag);
            }
        }

        public string GetItemNameField()
        {
            return nameof(VariableBase.Name);
        }

        public string GetItemDescriptionField()
        {
            return nameof(VariableBase.Description);
        }

        public string GetItemGroupField()
        {
            return null;
        }

        public string GetItemTagsField()
        {
            return nameof(VariableBase.Tags);
        }

        public string GetItemExecutionStatusField()
        {
            return null;
        }

        public string GetItemActiveField()
        {
            return null;
        }

        public ListItemUniqueIdentifier GetItemUniqueIdentifier(object item)
        {
            SetItem(item);
            //return new ListItemUniqueIdentifier() { Color = mActivitiesGroup.GroupColor, Tooltip = mActivitiesGroup.Name };
            return null;
        }

        public string GetItemIconField()
        {
            return nameof(VariableBase.Image);
        }

        public string GetItemIconTooltipField()
        {
            return nameof(VariableBase.VariableType);
        }

        public List<ListItemOperation> GetListOperations()
        {
            List<ListItemOperation> operationsList = new List<ListItemOperation>();

            //ListItemOperation addNew = new ListItemOperation();
            //addNew.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Add;
            //addNew.ToolTip = "Add New " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            //addNew.OperationHandler = AddNewHandler;
            //operationsList.Add(addNew);

            //ListItemOperation deleteAll = new ListItemOperation();
            //deleteAll.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
            //deleteAll.ToolTip = "Delete All " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            //deleteAll.OperationHandler = DeleteAllHandler;
            //operationsList.Add(deleteAll);

            return operationsList;
        }

        public List<ListItemOperation> GetListExtraOperations()
        {
            List<ListItemOperation> extraOperationsList = new List<ListItemOperation>();

            //ListItemOperation activeUnactiveAllActivities = new ListItemOperation();
            //activeUnactiveAllActivities.ImageType = Amdocs.Ginger.Common.Enums.eImageType.CheckBox;
            //activeUnactiveAllActivities.Header = "Activate/Un-Activate all " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            //activeUnactiveAllActivities.ToolTip = "Activate/Un-Activate all " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            //activeUnactiveAllActivities.OperationHandler = ActiveUnactiveAllActivitiesHandler;
            //extraOperationsList.Add(activeUnactiveAllActivities);

            return extraOperationsList;
        }

        public List<ListItemNotification> GetItemNotificationsList(object item)
        {
            SetItem(item);
            List<ListItemNotification> notificationsList = new List<ListItemNotification>();
            return notificationsList;
        }

        public List<ListItemOperation> GetItemOperationsList(object item)
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

        public List<ListItemOperation> GetItemExtraOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> extraOperationsList = new List<ListItemOperation>();

            //ListItemOperation mandatory = new ListItemOperation();
            //mandatory.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active;
            //mandatory.ImageBindingObject = mActivity;
            //mandatory.ImageBindingFieldName = nameof(Activity.Mandatory);
            //mandatory.ImageBindingConverter = new ActiveImageTypeConverter();
            //mandatory.ToolTip = "Mandatory";
            ////active.ImageSize = 15;
            //mandatory.OperationHandler = MandatoryHandler;
            //extraOperationsList.Add(mandatory);

            return extraOperationsList;
        }

        public List<ListItemOperation> GetItemExecutionOperationsList(object item)
        {
            return null;
        }

        public List<ListItemGroupOperation> GetItemGroupOperationsList()
        {
            return null;
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

    public class VariabelListItemEventArgs
    {
        public enum eEventType
        {
            ShowVariabelEditPage,
        }

        public eEventType EventType;
        public Object EventObject;

        public VariabelListItemEventArgs(eEventType eventType, object eventObject = null)
        {
            this.EventType = eventType;
            this.EventObject = eventObject;
        }
    }
}
