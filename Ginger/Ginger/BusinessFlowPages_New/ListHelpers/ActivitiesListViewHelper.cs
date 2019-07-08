using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.ALM;
using Ginger.BusinessFlowWindows;
using Ginger.UserControlsLib;
using Ginger.UserControlsLib.UCListView;
using Ginger.Variables;
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
    public class ActivitiesListViewHelper : IListViewHelper
    {
        Activity mActivity;
        Context mContext;
        General.eRIPageViewMode mPageViewMode;

        public UcListView ListView { get; set; }

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
        public ActivitiesListViewHelper(Context context, General.eRIPageViewMode pageViewMode)
        {
            mContext = context;
            mPageViewMode = pageViewMode;
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

        public string GetItemNameExtentionField()
        {
            return nameof(Activity.TargetApplication);
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

            if (mActivity.AddDynamicly)
            {//Brushes.MediumPurple
                return new ListItemUniqueIdentifier() { Color = "MediumPurple", Tooltip = "Added Dynamically from Shared Repository" };
            }
            else if (!mActivity.IsNotGherkinOptimizedActivity)
            {
                return new ListItemUniqueIdentifier() { Color = "Goldenrod", Tooltip = "This is a Gherkin Optimized " + GingerDicser.GetTermResValue(eTermResKey.Activity) };
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

            if (mPageViewMode != General.eRIPageViewMode.View)
            {
                ListItemOperation addNew = new ListItemOperation();
                addNew.AutomationID = "addNew";
                addNew.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Add;
                addNew.ToolTip = "Add New " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                addNew.OperationHandler = AddNewHandler;
                operationsList.Add(addNew);
            }

            return operationsList;
        }

        public List<ListItemOperation> GetListExtraOperations()
        {
            List<ListItemOperation> extraOperationsList = new List<ListItemOperation>();

            if (mPageViewMode != General.eRIPageViewMode.View)
            {
                ListItemOperation deleteAll = new ListItemOperation();
                deleteAll.AutomationID = "deleteAll";
                deleteAll.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
                deleteAll.Header = "Delete All " + GingerDicser.GetTermResValue(eTermResKey.Activities);
                deleteAll.ToolTip = "Delete All " + GingerDicser.GetTermResValue(eTermResKey.Activities);
                deleteAll.OperationHandler = DeleteAllHandler;
                extraOperationsList.Add(deleteAll);

                ListItemOperation activeUnactiveAllActivities = new ListItemOperation();
                activeUnactiveAllActivities.AutomationID = "activeUnactiveAllActivities";
                activeUnactiveAllActivities.ImageType = Amdocs.Ginger.Common.Enums.eImageType.CheckBox;
                activeUnactiveAllActivities.Header = "Activate/Un-Activate all " + GingerDicser.GetTermResValue(eTermResKey.Activities);
                activeUnactiveAllActivities.ToolTip = "Activate/Un-Activate all " + GingerDicser.GetTermResValue(eTermResKey.Activities);
                activeUnactiveAllActivities.OperationHandler = ActiveUnactiveAllActivitiesHandler;
                extraOperationsList.Add(activeUnactiveAllActivities);

                ListItemOperation activitiesVarsDep = new ListItemOperation();
                activitiesVarsDep.AutomationID = "activitiesVarsDep";
                activitiesVarsDep.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MapSigns;
                activitiesVarsDep.Header = string.Format("{0}-{1} Dependencies", GingerDicser.GetTermResValue(eTermResKey.Activities), GingerDicser.GetTermResValue(eTermResKey.Variables));
                activitiesVarsDep.ToolTip = string.Format("Set {0}-{1} Dependencies", GingerDicser.GetTermResValue(eTermResKey.Activities), GingerDicser.GetTermResValue(eTermResKey.Variables));
                activitiesVarsDep.OperationHandler = ActivitiesVarsHandler;
                extraOperationsList.Add(activitiesVarsDep);
            }

            return extraOperationsList;
        }

        public List<ListItemNotification> GetItemNotificationsList(object item)
        {
            SetItem(item);
            List<ListItemNotification> notificationsList = new List<ListItemNotification>();

            ListItemNotification activitiesVarsDepInd = new ListItemNotification();
            activitiesVarsDepInd.AutomationID = "activitiesVarsDepInd";
            activitiesVarsDepInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MapSigns;
            activitiesVarsDepInd.ToolTip = string.Format("{0} {1}-{2} dependency is enabeled", GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), GingerDicser.GetTermResValue(eTermResKey.Activities), GingerDicser.GetTermResValue(eTermResKey.Variables));
            activitiesVarsDepInd.ImageSize = 14;
            activitiesVarsDepInd.BindingObject = mContext.BusinessFlow;
            activitiesVarsDepInd.BindingFieldName = nameof(BusinessFlow.EnableActivitiesVariablesDependenciesControl);
            activitiesVarsDepInd.BindingConverter = new BoolVisibilityConverter();
            notificationsList.Add(activitiesVarsDepInd);

            ListItemNotification mandatoryInd = new ListItemNotification();
            mandatoryInd.AutomationID = "mandatoryInd";
            mandatoryInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Mandatory;
            mandatoryInd.ToolTip = string.Format("{0} is Mandatory", GingerDicser.GetTermResValue(eTermResKey.Activity));
            mandatoryInd.ImageSize = 14;
            mandatoryInd.BindingObject = mActivity;
            mandatoryInd.BindingFieldName = nameof(Activity.Mandatory);
            mandatoryInd.BindingConverter = new BoolVisibilityConverter();
            notificationsList.Add(mandatoryInd);

            ListItemNotification sharedRepoInd = new ListItemNotification();
            sharedRepoInd.AutomationID = "sharedRepoInd";
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

            if (mPageViewMode != General.eRIPageViewMode.View)
            {
                ListItemOperation moveUp = new ListItemOperation();
                moveUp.AutomationID = "moveUp";
                moveUp.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveUp;
                moveUp.ToolTip = "Move Up";
                moveUp.OperationHandler = MoveUpHandler;
                operationsList.Add(moveUp);

                ListItemOperation moveDown = new ListItemOperation();
                moveDown.AutomationID = "moveDown";
                moveDown.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveDown;
                moveDown.ToolTip = "Move Down";
                moveDown.OperationHandler = MoveDownHandler;
                operationsList.Add(moveDown);

                ListItemOperation delete = new ListItemOperation();
                delete.AutomationID = "delete";
                delete.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
                delete.ToolTip = "Delete";
                delete.OperationHandler = DeleteHandler;
                operationsList.Add(delete);

                ListItemOperation active = new ListItemOperation();
                active.AutomationID = "active";
                active.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active;
                active.ImageBindingObject = mActivity;
                active.ImageBindingFieldName = nameof(Activity.Active);
                active.ImageBindingConverter = new ActiveImageTypeConverter();
                active.ToolTip = "Active";
                active.IsEnabeled = mActivity.IsNotGherkinOptimizedActivity;
                active.OperationHandler = ActiveHandler;
                operationsList.Add(active);               
            }

            return operationsList;
        }

        public List<ListItemOperation> GetItemExtraOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> extraOperationsList = new List<ListItemOperation>();

            if (mPageViewMode != General.eRIPageViewMode.View)
            {
                ListItemOperation mandatory = new ListItemOperation();
                mandatory.AutomationID = "mandatory";
                mandatory.Header = "Mandatory";
                mandatory.ToolTip = string.Format("If {0} fails so stop execution", GingerDicser.GetTermResValue(eTermResKey.Activity));
                mandatory.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active;
                mandatory.ImageBindingObject = mActivity;
                mandatory.ImageBindingFieldName = nameof(Activity.Mandatory);
                mandatory.ImageBindingConverter = new ActiveImageTypeConverter();
                mandatory.OperationHandler = MandatoryHandler;
                extraOperationsList.Add(mandatory);

                ListItemOperation reset = new ListItemOperation();
                reset.AutomationID = "reset";
                reset.Group = "Reset Operations";
                reset.GroupImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset;
                reset.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset;
                reset.Header = string.Format("Reset {0} execution details", GingerDicser.GetTermResValue(eTermResKey.Activity));
                reset.ToolTip = string.Format("Reset {0} execution details", GingerDicser.GetTermResValue(eTermResKey.Activity));
                reset.OperationHandler = ResetHandler;
                extraOperationsList.Add(reset);

                ListItemOperation resetRest = new ListItemOperation();
                resetRest.AutomationID = "resetRest";
                resetRest.Group = "Reset Operations";
                resetRest.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset;
                resetRest.Header = string.Format("Reset execution details from this {0}", GingerDicser.GetTermResValue(eTermResKey.Activity));
                resetRest.ToolTip = string.Format("Reset execution details from this {0}", GingerDicser.GetTermResValue(eTermResKey.Activity));
                resetRest.OperationHandler = ResetResetHandler;
                extraOperationsList.Add(resetRest);

                ListItemOperation copy = new ListItemOperation();
                copy.AutomationID = "copy";
                copy.Group = "Clipboard";
                copy.GroupImageType = Amdocs.Ginger.Common.Enums.eImageType.Clipboard;
                copy.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Copy;
                copy.Header = "Copy";
                copy.OperationHandler = CopyHandler;
                extraOperationsList.Add(copy);

                ListItemOperation cut = new ListItemOperation();
                cut.AutomationID = "cut";
                cut.Group = "Clipboard";
                cut.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Cut;
                cut.Header = "Cut";
                cut.OperationHandler = CutHandler;
                extraOperationsList.Add(cut);

                ListItemOperation pasterAfterCurrent = new ListItemOperation();
                pasterAfterCurrent.AutomationID = "pasterAfterCurrent";
                pasterAfterCurrent.Group = "Clipboard";
                pasterAfterCurrent.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Paste;
                pasterAfterCurrent.Header = "Paste";
                pasterAfterCurrent.OperationHandler = PasteAfterCurrentHandler;
                extraOperationsList.Add(pasterAfterCurrent);
            }

            ListItemOperation addToSR = new ListItemOperation();
            addToSR.AutomationID = "addToSR";
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

            if (mPageViewMode != General.eRIPageViewMode.View)
            {
                ListItemOperation runAction = new ListItemOperation();
                runAction.AutomationID = "runAction";
                runAction.ImageType = Amdocs.Ginger.Common.Enums.eImageType.RunSingle;
                runAction.ToolTip = "Run Current Action";
                runAction.OperationHandler = RunActionHandler;
                executionOperationsList.Add(runAction);

                ListItemOperation run = new ListItemOperation();
                run.AutomationID = "run";
                run.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Run;
                run.ToolTip = "Run " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                run.OperationHandler = RunHandler;
                executionOperationsList.Add(run);

                ListItemOperation continueRun = new ListItemOperation();
                continueRun.AutomationID = "continueRun";
                continueRun.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Continue;
                continueRun.ToolTip = "Continue Run from " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                continueRun.OperationHandler = ContinueRunHandler;
                executionOperationsList.Add(continueRun);
            }

            return executionOperationsList;
        }

        public List<ListItemGroupOperation> GetItemGroupOperationsList()
        {
            List<ListItemGroupOperation> groupOperationsList = new List<ListItemGroupOperation>();

            if (mPageViewMode != General.eRIPageViewMode.View)
            {
                ListItemGroupOperation addNewActivity = new ListItemGroupOperation();
                addNewActivity.AutomationID = "addNewGroupActivity";
                addNewActivity.Header = "Add New " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                addNewActivity.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Add;
                addNewActivity.ToolTip = "Add New " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                addNewActivity.OperationHandler = AddNewActivityToGroupHandler;
                groupOperationsList.Add(addNewActivity);

                ListItemGroupOperation rename = new ListItemGroupOperation();
                rename.AutomationID = "renameGroup";
                rename.Header = "Rename Group";
                rename.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit;
                rename.ToolTip = "Rename group";
                rename.OperationHandler = RenameGroupHandler;
                groupOperationsList.Add(rename);

                ListItemGroupOperation moveUp = new ListItemGroupOperation();
                moveUp.AutomationID = "moveGroupUp";
                moveUp.Header = "Move Group Up";
                moveUp.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveUp;
                moveUp.ToolTip = "Move all group up";
                moveUp.OperationHandler = MoveGroupUpHandler;
                groupOperationsList.Add(moveUp);

                ListItemGroupOperation moveDown = new ListItemGroupOperation();
                moveDown.AutomationID = "moveGroupDown";
                moveDown.Header = "Move Group Down";
                moveDown.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveDown;
                moveDown.ToolTip = "Move all group down";
                moveDown.OperationHandler = MoveGroupDownHandler;
                groupOperationsList.Add(moveDown);

                ListItemGroupOperation delete = new ListItemGroupOperation();
                delete.AutomationID = "deleteGroup";
                delete.Header = "Delete Group";
                delete.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
                delete.ToolTip = "Delete all group " + GingerDicser.GetTermResValue(eTermResKey.Activities);
                delete.OperationHandler = DeleteGroupHandler;
                groupOperationsList.Add(delete);

                ListItemGroupOperation copyGroup = new ListItemGroupOperation();
                copyGroup.AutomationID = "copyGroup";
                copyGroup.Group = "Clipboard";
                copyGroup.GroupImageType = Amdocs.Ginger.Common.Enums.eImageType.Clipboard;
                copyGroup.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Copy;
                copyGroup.Header = "Copy Group Items";
                copyGroup.OperationHandler = CopyGroupHandler;
                groupOperationsList.Add(copyGroup);

                ListItemGroupOperation cutGroup = new ListItemGroupOperation();
                cutGroup.AutomationID = "cutGroup";
                cutGroup.Group = "Clipboard";
                cutGroup.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Cut;
                cutGroup.Header = "Cut Group Items";
                cutGroup.OperationHandler = CutGroupHandler;
                groupOperationsList.Add(cutGroup);

                ListItemGroupOperation pasteInGroup = new ListItemGroupOperation();
                pasteInGroup.AutomationID = "pasterAfterCurrent";
                pasteInGroup.Group = "Clipboard";
                pasteInGroup.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Paste;
                pasteInGroup.Header = "Paste";
                pasteInGroup.OperationHandler = PasteInGroupHandler;
                groupOperationsList.Add(pasteInGroup);
            }

            ListItemGroupOperation addToSR = new ListItemGroupOperation();
            addToSR.AutomationID = "addGroupToSR";
            addToSR.Header = "Add Group to Shared Repository";
            addToSR.ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem;
            addToSR.ToolTip = "Add group and it " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " to Shared Repository";
            addToSR.OperationHandler = AddGroupToSRHandler;
            groupOperationsList.Add(addToSR);

            ListItemGroupOperation export = new ListItemGroupOperation();
            export.AutomationID = "exportGroup";
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

        private void RunActionHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
           
            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.RunCurrentAction, new Tuple<Activity, Act>(mActivity, (Act)mActivity.Acts.CurrentItem));
        }

        private void RunHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
          
            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.RunCurrentActivity, mActivity);
        }

        private void ContinueRunHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);           
            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.ContinueActivityRun, mActivity);
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

        private void ResetResetHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);            
            for(int indx=mContext.BusinessFlow.Activities.IndexOf(mActivity); indx < mContext.BusinessFlow.Activities.Count;indx++)
            {
                mContext.BusinessFlow.Activities[indx].Reset();
            }
        }

        private void ActivitiesVarsHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            VariablesDependenciesPage activitiesVarsDepPage = new VariablesDependenciesPage(mContext.BusinessFlow);
            activitiesVarsDepPage.ShowAsWindow();
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

            if (activitiesGroup.Name.Contains("Optimized Activities"))
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "This is an automaic group created from Gherkin file and can not be modified");
                return;
            }
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

        private void CopyHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            ListView.List.SelectedItem = mActivity;
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            list.Add(mActivity);
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            ListView.List.SelectedItem = mActivity;
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            list.Add(mActivity);
            ClipboardOperationsHandler.SetCutItems(ListView, list);
        }

        private void PasteAfterCurrentHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);

            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == mActivity.ActivitiesGroupID).FirstOrDefault();
            int insertIndex = mContext.BusinessFlow.Activities.IndexOf(mActivity) + 1;
            foreach (RepositoryItemBase item in ClipboardOperationsHandler.CopiedorCutItems)
            {
                if (item is Activity)
                {
                    if (ClipboardOperationsHandler.CutSourceList == null)//Copy
                    {
                        Activity copiedItem = (Activity)item.CreateCopy();
                        //set unique name
                        GingerCoreNET.GeneralLib.General.SetUniqueNameToRepoItem(GetActivitiesList(), copiedItem, "_Copy");
                        mContext.BusinessFlow.AddActivity(copiedItem, activitiesGroup, insertIndex, false);
                        //Trigger event for changing sub classes fields
                        ListView.OnPasteItemEvent(PasteItemEventArgs.ePasteType.PasteCopiedItem, copiedItem);
                        insertIndex++;
                    }
                    else // cut
                    {                                              
                        if (mContext.BusinessFlow.Activities.Contains(item))
                        {
                            //delete from list and group
                            mContext.BusinessFlow.DeleteActivity((Activity)item);                           
                        }
                        else
                        {
                            //clear from source  
                            ClipboardOperationsHandler.CutSourceList.Remove(item);
                            //set unique name
                            GingerCoreNET.GeneralLib.General.SetUniqueNameToRepoItem(GetActivitiesList(), item);
                        }
                        //paste on target                      
                        mContext.BusinessFlow.AddActivity((Activity)item, activitiesGroup, insertIndex, false);
                        //Trigger event for changing sub classes fields
                        ListView.OnPasteItemEvent(PasteItemEventArgs.ePasteType.PasteCutedItem, item);
                    }
                }            
            }

            if (ClipboardOperationsHandler.CutSourceList != null)
            {
                //clear so will be past only once
                ClipboardOperationsHandler.CutSourceList = null;
                ClipboardOperationsHandler.CopiedorCutItems.Clear();
            }

            mContext.BusinessFlow.AttachActivitiesGroupsAndActivities();
            OnActivityListItemEvent(ActivityListItemEventArgs.eEventType.UpdateGrouping);
        }

        private ObservableList<RepositoryItemBase> GetSelectedGroupActivitiesList(object sender)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == ((MenuItem)sender).Tag.ToString()).FirstOrDefault();
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            foreach (ActivityIdentifiers groupActivityIdent in activitiesGroup.ActivitiesIdentifiers)
            {
                list.Add(groupActivityIdent.IdentifiedActivity);
            }

            return list;
        }

        private void CopyGroupHandler(object sender, RoutedEventArgs e)
        {
            ClipboardOperationsHandler.SetCopyItems(GetSelectedGroupActivitiesList(sender));
        }

        private void CutGroupHandler(object sender, RoutedEventArgs e)
        {
            ClipboardOperationsHandler.SetCutItems(ListView, GetSelectedGroupActivitiesList(sender));
        }

        private void PasteInGroupHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            //if ()
            //ClipboardOperationsHandler.PasteItems(ListView);
        }

        private ObservableList<RepositoryItemBase> GetActivitiesList()
        {
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            foreach (RepositoryItemBase activity in mContext.BusinessFlow.Activities)
            {
                list.Add(activity);
            }
            return list;
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
