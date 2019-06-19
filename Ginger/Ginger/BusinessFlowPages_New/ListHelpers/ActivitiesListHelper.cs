using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.ALM;
using Ginger.BusinessFlowWindows;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.GeneralLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ginger.BusinessFlowPages.ListHelpers
{
    public class ActivitiesListHelper : IListViewHelper
    {
        Activity mActivity;
        Context mContext;

        public delegate void ActivityListItemEventHandler(ActivityListItemEventArgs EventArgs);
        public event ActivityListItemEventHandler ActivityListItemEvent;
        private void OnActivityListItemEvent(ActivityListItemEventArgs.eEventType eventType, Object eventObject = null)
        {
            ActivityListItemEventHandler handler = ActivityListItemEvent;
            if (handler != null)
            {
                handler(new ActivityListItemEventArgs(eventType, eventObject));
            }
        }

        public ActivitiesListHelper(Context context)
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
            else if (item is MenuItem)
            {
                mActivity = (Activity)(((MenuItem)item).Tag);
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

        public string GetItemIconTooltipField()
        {
            return nameof(Activity.ActivityType);
        }

        public List<ListItemOperation> GetListOperations()
        {
            List<ListItemOperation> operationsList = new List<ListItemOperation>();

            ListItemOperation addNew = new ListItemOperation();
            addNew.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Add;
            addNew.ToolTip = "Add New " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            addNew.OperationHandler = AddNewHandler;
            operationsList.Add(addNew);

            return operationsList;
        }

        public List<ListItemOperation> GetListExtraOperations()
        {
            List<ListItemOperation> extraOperationsList = new List<ListItemOperation>();

            ListItemOperation deleteAll = new ListItemOperation();
            deleteAll.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
            deleteAll.Header = "Delete All " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            deleteAll.ToolTip = "Delete All " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            deleteAll.OperationHandler = DeleteAllHandler;
            extraOperationsList.Add(deleteAll);

            ListItemOperation activeUnactiveAllActivities = new ListItemOperation();
            activeUnactiveAllActivities.ImageType = Amdocs.Ginger.Common.Enums.eImageType.CheckBox;
            activeUnactiveAllActivities.Header = "Activate/Un-Activate all " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            activeUnactiveAllActivities.ToolTip = "Activate/Un-Activate all " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            activeUnactiveAllActivities.OperationHandler = ActiveUnactiveAllActivitiesHandler;
            extraOperationsList.Add(activeUnactiveAllActivities);

            return extraOperationsList;
        }

        public List<ListItemNotification> GetItemNotificationsList(object item)
        {
            SetItem(item);
            List<ListItemNotification> notificationsList = new List<ListItemNotification>();

            ListItemNotification varsDepInd = new ListItemNotification();
            varsDepInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MapSigns;
            varsDepInd.ToolTip = string.Format("{0} Actions-{1} dependency is enabeled", GingerDicser.GetTermResValue(eTermResKey.Activity), GingerDicser.GetTermResValue(eTermResKey.Variables)) ;
            varsDepInd.ImageSize = 14;
            varsDepInd.BindingObject = mActivity;
            varsDepInd.BindingFieldName = nameof(Activity.EnableActionsVariablesDependenciesControl);
            varsDepInd.BindingConverter = new BoolVisibilityConverter();
            notificationsList.Add(varsDepInd);

            ListItemNotification mandatoryInd = new ListItemNotification();
            mandatoryInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Mandatory;
            mandatoryInd.ToolTip = string.Format("{0} is Mandatory", GingerDicser.GetTermResValue(eTermResKey.Activity));
            mandatoryInd.ImageSize = 14;
            mandatoryInd.BindingObject = mActivity;
            mandatoryInd.BindingFieldName = nameof(Activity.Mandatory);
            mandatoryInd.BindingConverter = new BoolVisibilityConverter();
            notificationsList.Add(mandatoryInd);

            ListItemNotification sharedRepoInd = new ListItemNotification();
            sharedRepoInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem;
            sharedRepoInd.ToolTip = string.Format("{0} source is from Shared Repository", GingerDicser.GetTermResValue(eTermResKey.Activity));
            sharedRepoInd.ImageForeground = Brushes.Orange;
            sharedRepoInd.BindingObject = mActivity;
            sharedRepoInd.BindingFieldName = nameof(Activity.IsSharedRepositoryInstance);
            sharedRepoInd.BindingConverter = new BoolVisibilityConverter();
            notificationsList.Add(sharedRepoInd);

            return notificationsList;
        }

        public List<ListItemOperation> GetItemOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> operationsList = new List<ListItemOperation>();

            ListItemOperation moveUp = new ListItemOperation();
            moveUp.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveUp;
            moveUp.ToolTip = "Move Up";
            moveUp.OperationHandler = MoveUpHandler;
            operationsList.Add(moveUp);

            ListItemOperation moveDown = new ListItemOperation();
            moveDown.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveDown;
            moveDown.ToolTip = "Move Down";
            moveDown.OperationHandler = MoveDownHandler;
            operationsList.Add(moveDown);

            ListItemOperation delete = new ListItemOperation();
            delete.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
            delete.ToolTip = "Delete";
            delete.OperationHandler = DeleteHandler;
            operationsList.Add(delete);

            ListItemOperation active = new ListItemOperation();
            active.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active;
            active.ImageBindingObject = mActivity;
            active.ImageBindingFieldName = nameof(Activity.Active);
            active.ImageBindingConverter = new ActiveImageTypeConverter();
            active.ToolTip = "Active";
            //active.ImageSize = 15;
            active.OperationHandler = ActiveHandler;
            operationsList.Add(active);

            return operationsList;
        }

        public List<ListItemOperation> GetItemExtraOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> extraOperationsList = new List<ListItemOperation>();

            ListItemOperation mandatory = new ListItemOperation();
            mandatory.Header = "Mandatory";
            mandatory.ToolTip = string.Format("If {0} fails so stop execution", GingerDicser.GetTermResValue(eTermResKey.Activity));
            mandatory.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active;
            mandatory.ImageBindingObject = mActivity;
            mandatory.ImageBindingFieldName = nameof(Activity.Mandatory);
            mandatory.ImageBindingConverter = new ActiveImageTypeConverter();
            mandatory.OperationHandler = MandatoryHandler;
            extraOperationsList.Add(mandatory);

            ListItemOperation reset = new ListItemOperation();
            reset.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset;
            reset.Header = "Reset";
            reset.ToolTip = "Reset execution details";
            reset.OperationHandler = ResetHandler;
            extraOperationsList.Add(reset);

            ListItemOperation actionVarsDep = new ListItemOperation();
            actionVarsDep.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MapSigns;
            actionVarsDep.Header = "Actions-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies";
            actionVarsDep.ToolTip = "Set Actions-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies";
            actionVarsDep.OperationHandler = ActionsVarsHandler;
            extraOperationsList.Add(actionVarsDep);

            ListItemOperation activeUnactiveAllActions = new ListItemOperation();
            activeUnactiveAllActions.ImageType = Amdocs.Ginger.Common.Enums.eImageType.CheckBox;
            activeUnactiveAllActions.Header = "Activate/Un-Activate all Actions";
            activeUnactiveAllActions.ToolTip = "Activate/Un-Activate all Actions";
            activeUnactiveAllActions.OperationHandler = ActiveUnactiveAllActionsHandler;
            extraOperationsList.Add(activeUnactiveAllActions);

            ListItemOperation takeUntakeSS = new ListItemOperation();
            takeUntakeSS.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Image;
            takeUntakeSS.Header = "Take/Un-Take Screen Shots";
            takeUntakeSS.ToolTip = "Set Take/Un-Take Screen Shots to all Actions";
            takeUntakeSS.OperationHandler = TakeUntakeSSHandler;
            extraOperationsList.Add(takeUntakeSS);            

            ListItemOperation addToSR = new ListItemOperation();
            addToSR.ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem;
            addToSR.Header = "Add to Shared Repository";
            addToSR.ToolTip = "Add to Shared Repository";
            addToSR.OperationHandler = AddToSRHandler;
            extraOperationsList.Add(addToSR);

            return extraOperationsList;
        }

        public List<ListItemOperation> GetItemExecutionOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> executionOperationsList = new List<ListItemOperation>();

            ListItemOperation run = new ListItemOperation();
            run.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Run;
            run.ToolTip = "Run " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            run.OperationHandler = RunHandler;
            executionOperationsList.Add(run);

            ListItemOperation continueRun = new ListItemOperation();
            continueRun.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Continue;
            continueRun.ToolTip = "Continue Run " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            continueRun.OperationHandler = ContinueRunHandler;
            executionOperationsList.Add(continueRun);

            return executionOperationsList;
        }

        public List<ListItemGroupOperation> GetItemGroupOperationsList()
        {
            List<ListItemGroupOperation> groupOperationsList = new List<ListItemGroupOperation>();

            ListItemGroupOperation addNewActivity = new ListItemGroupOperation();
            addNewActivity.Header = "Add New " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            addNewActivity.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Add;
            addNewActivity.ToolTip = "Add New " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            addNewActivity.OperationHandler = AddNewActivityToGroupHandler;
            groupOperationsList.Add(addNewActivity);

            ListItemGroupOperation rename = new ListItemGroupOperation();
            rename.Header = "Rename Group";
            rename.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit;
            rename.ToolTip = "Rename group";
            rename.OperationHandler = RenameGroupHandler;
            groupOperationsList.Add(rename);

            ListItemGroupOperation moveUp = new ListItemGroupOperation();
            moveUp.Header = "Move Group Up";
            moveUp.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveUp;
            moveUp.ToolTip = "Move all group up";
            moveUp.OperationHandler = MoveGroupUpHandler;
            groupOperationsList.Add(moveUp);

            ListItemGroupOperation moveDown = new ListItemGroupOperation();
            moveDown.Header = "Move Group Down";
            moveDown.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveDown;
            moveDown.ToolTip = "Move all group down";
            moveDown.OperationHandler = MoveGroupDownHandler;
            groupOperationsList.Add(moveDown);

            ListItemGroupOperation delete = new ListItemGroupOperation();
            delete.Header = "Delete Group";
            delete.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
            delete.ToolTip = "Delete all group " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            delete.OperationHandler = DeleteGroupHandler;
            groupOperationsList.Add(delete);

            ListItemGroupOperation addToSR = new ListItemGroupOperation();
            addToSR.Header = "Add Group to Shared Repository";
            addToSR.ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem;
            addToSR.ToolTip = "Add group and it " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " to Shared Repository";
            addToSR.OperationHandler = AddGroupToSRHandler;
            groupOperationsList.Add(addToSR);

            ListItemGroupOperation export = new ListItemGroupOperation();
            export.Header = "Export Group";
            export.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Share;
            export.ToolTip = "Export group and it " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " to ALM";
            export.OperationHandler = ExportGroupHandler;
            groupOperationsList.Add(export);

            return groupOperationsList;
        }

        private void AddNewHandler(object sender, RoutedEventArgs e)
        {
            WizardWindow.ShowWizard(new AddActivityWizard(mContext));
            //OnActivityListItemEvent(ActivityListItemEventArgs.eEventType.UpdateGrouping);
        }

        private void DeleteAllHandler(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.SureWantToDeleteAll) == eUserMsgSelection.Yes)
            {
                mContext.BusinessFlow.Activities.Clear();
                mContext.BusinessFlow.ActivitiesGroups.Clear();
            }
        }

        private void RunHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mContext.BusinessFlow.CurrentActivity = mActivity;
            mContext.Runner.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = Ginger.Reports.ExecutionLoggerConfiguration.AutomationTabContext.ActivityRun;
            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.RunCurrentActivity, null);
        }

        private void ContinueRunHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mContext.BusinessFlow.CurrentActivity = mActivity;
            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.ContinueActivityRun, null);
        }

        private void ActiveHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mActivity.Active = !mActivity.Active;
        }

        private void MandatoryHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mActivity.Mandatory = !mActivity.Mandatory;
        }

        private void AddToSRHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            List<RepositoryItemBase> list = new List<RepositoryItemBase>();
            list.Add(mActivity);
            (new Repository.SharedRepositoryOperations()).AddItemsToRepository(mContext, list);
        }

        private void ResetHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mActivity.Reset();
        }

        private void ActionsVarsHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            Ginger.Variables.VariablesDependenciesPage actionsDepPage = new Ginger.Variables.VariablesDependenciesPage(mActivity);
            actionsDepPage.ShowAsWindow();
        }

        private void ActiveUnactiveAllActivitiesHandler(object sender, RoutedEventArgs e)
        {
            if (mContext.BusinessFlow.Activities.Count > 0)
            {
                bool activeValue = !mContext.BusinessFlow.Activities[0].Active;
                foreach (Activity a in mContext.BusinessFlow.Activities)
                {
                    a.Active = activeValue;
                }
            }
        }

        private void ActiveUnactiveAllActionsHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            if (mActivity.Acts.Count > 0)
            {
                bool activeValue = !mActivity.Acts[0].Active;
                foreach (Act a in mActivity.Acts)
                {
                    a.Active = activeValue;
                }
            }
        }

        private void TakeUntakeSSHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            if (mActivity.Acts.Count > 0)
            {
                bool takeValue = !((Act)mActivity.Acts[0]).TakeScreenShot;//decide if to take or not
                foreach (Act a in mActivity.Acts)
                {
                    a.TakeScreenShot = takeValue;
                }
            }
        }

        private void DeleteHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            if (Reporter.ToUser(eUserMsgKey.SureWantToDelete, mActivity.ActivityName) == eUserMsgSelection.Yes)
            {
                mContext.BusinessFlow.DeleteActivity(mActivity);
            }
        }

        private void MoveUpHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mContext.BusinessFlow.MoveActivityUp(mActivity);
        }

        private void MoveDownHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mContext.BusinessFlow.MoveActivityDown(mActivity);
        }

        private void AddNewActivityToGroupHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == ((MenuItem)sender).Tag.ToString()).FirstOrDefault();

            WizardWindow.ShowWizard(new AddActivityWizard(mContext, activitiesGroup));
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
                        OnActivityListItemEvent(ActivityListItemEventArgs.eEventType.UpdateGrouping);
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
            OnActivityListItemEvent(ActivityListItemEventArgs.eEventType.UpdateGrouping);
        }

        private void MoveGroupDownHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == ((MenuItem)sender).Tag.ToString()).FirstOrDefault();
            mContext.BusinessFlow.MoveActivitiesGroupDown(activitiesGroup);
            OnActivityListItemEvent(ActivityListItemEventArgs.eEventType.UpdateGrouping);
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

    public class ActivityListItemEventArgs
    {
        public enum eEventType
        {
            UpdateGrouping,
        }

        public eEventType EventType;
        public Object EventObject;

        public ActivityListItemEventArgs(eEventType eventType, object eventObject = null)
        {
            this.EventType = eventType;
            this.EventObject = eventObject;
        }
    }
}
