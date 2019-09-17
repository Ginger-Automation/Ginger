#region License
/*
Copyright © 2014-2019 European Support Limited

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

        public General.eRIPageViewMode PageViewMode { get; set; }

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

        public bool AllowExpandItems { get; set; } = true;

        public bool ExpandItemOnLoad { get; set; } = false;

        public ActivitiesListViewHelper(Context context, General.eRIPageViewMode pageViewMode)
        {
            mContext = context;
            PageViewMode = pageViewMode;
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
            if (PageViewMode == General.eRIPageViewMode.Automation)
            {
                return nameof(Activity.Status);
            }
            else
            {
                return null;
            }
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

            ListItemOperation addNew = new ListItemOperation();
            addNew.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            addNew.AutomationID = "addNew";
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
            deleteAll.AutomationID = "deleteAll";
            deleteAll.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            deleteAll.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
            deleteAll.Header = "Delete All " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            deleteAll.ToolTip = "Delete All " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            deleteAll.OperationHandler = DeleteAllHandler;
            extraOperationsList.Add(deleteAll);

            ListItemOperation activeUnactiveAllActivities = new ListItemOperation();
            activeUnactiveAllActivities.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            activeUnactiveAllActivities.AutomationID = "activeUnactiveAllActivities";
            activeUnactiveAllActivities.ImageType = Amdocs.Ginger.Common.Enums.eImageType.CheckBox;
            activeUnactiveAllActivities.Header = "Activate/De-Activate All " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            activeUnactiveAllActivities.ToolTip = "Activate/De-Activate all " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            activeUnactiveAllActivities.OperationHandler = ActiveUnactiveAllActivitiesHandler;
            extraOperationsList.Add(activeUnactiveAllActivities);

            ListItemOperation activitiesVarsDep = new ListItemOperation();
            activitiesVarsDep.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            activitiesVarsDep.AutomationID = "activitiesVarsDep";
            activitiesVarsDep.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MapSigns;
            activitiesVarsDep.Header = string.Format("{0}-{1} Dependencies", GingerDicser.GetTermResValue(eTermResKey.Activities), GingerDicser.GetTermResValue(eTermResKey.Variables));
            activitiesVarsDep.ToolTip = string.Format("Set {0}-{1} Dependencies", GingerDicser.GetTermResValue(eTermResKey.Activities), GingerDicser.GetTermResValue(eTermResKey.Variables));
            activitiesVarsDep.OperationHandler = ActivitiesVarsHandler;
            extraOperationsList.Add(activitiesVarsDep);

            ListItemOperation copyAllList = new ListItemOperation();
            copyAllList.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.View, General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            copyAllList.AutomationID = "copyAllList";
            copyAllList.Group = "Clipboard";
            copyAllList.GroupImageType = Amdocs.Ginger.Common.Enums.eImageType.Clipboard;
            copyAllList.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Copy;
            copyAllList.Header = "Copy All List Items";
            copyAllList.OperationHandler = CopyAllListHandler;
            extraOperationsList.Add(copyAllList);

            ListItemOperation cutAllList = new ListItemOperation();
            cutAllList.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            cutAllList.AutomationID = "cutAllList";
            cutAllList.Group = "Clipboard";
            cutAllList.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Cut;
            cutAllList.Header = "Cut All List Items";
            cutAllList.OperationHandler = CutAllListHandler;
            extraOperationsList.Add(cutAllList);

            ListItemOperation copySelected = new ListItemOperation();
            copySelected.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.View, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            copySelected.AutomationID = "copySelected";
            copySelected.Group = "Clipboard";
            copySelected.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Copy;
            copySelected.Header = "Copy Selected Items";
            copySelected.OperationHandler = CopySelectedHandler;
            extraOperationsList.Add(copySelected);

            ListItemOperation cutSelected = new ListItemOperation();
            cutSelected.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            cutSelected.AutomationID = "cutSelected";
            cutSelected.Group = "Clipboard";
            cutSelected.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Cut;
            cutSelected.Header = "Cut Selected Items";
            cutSelected.OperationHandler = CutSelectedHandler;
            extraOperationsList.Add(cutSelected);

            ListItemOperation pasteInList = new ListItemOperation();
            pasteInList.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            pasteInList.AutomationID = "pasteInList";
            pasteInList.Group = "Clipboard";
            pasteInList.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Paste;
            pasteInList.Header = "Paste";
            pasteInList.OperationHandler = PasteInListHandler;
            extraOperationsList.Add(pasteInList);

            return extraOperationsList;
        }

        public List<ListItemNotification> GetItemNotificationsList(object item)
        {
            SetItem(item);
            List<ListItemNotification> notificationsList = new List<ListItemNotification>();

            ListItemNotification activitiesVarsDepInd = new ListItemNotification();            
            activitiesVarsDepInd.AutomationID = "activitiesVarsDepInd";
            activitiesVarsDepInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MapSigns;
            activitiesVarsDepInd.ToolTip = string.Format("{0} {1}-{2} dependency is enabled", GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), GingerDicser.GetTermResValue(eTermResKey.Activities), GingerDicser.GetTermResValue(eTermResKey.Variables));
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

            ListItemOperation moveUp = new ListItemOperation();
            moveUp.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            moveUp.AutomationID = "moveUp";
            moveUp.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveUp;
            moveUp.ToolTip = "Move Up";
            moveUp.OperationHandler = MoveUpHandler;
            operationsList.Add(moveUp);

            ListItemOperation moveDown = new ListItemOperation();
            moveDown.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            moveDown.AutomationID = "moveDown";
            moveDown.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveDown;
            moveDown.ToolTip = "Move Down";
            moveDown.OperationHandler = MoveDownHandler;
            operationsList.Add(moveDown);

            ListItemOperation delete = new ListItemOperation();
            delete.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            delete.AutomationID = "delete";
            delete.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
            delete.ToolTip = "Delete";
            delete.OperationHandler = DeleteHandler;
            operationsList.Add(delete);

            ListItemOperation active = new ListItemOperation();
            active.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            active.AutomationID = "active";
            active.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active;
            active.ImageBindingObject = mActivity;
            active.ImageBindingFieldName = nameof(Activity.Active);
            active.ImageBindingConverter = new ActiveImageTypeConverter();
            active.ToolTip = "Active";
            active.IsEnabeled = mActivity.IsNotGherkinOptimizedActivity;
            active.OperationHandler = ActiveHandler;
            operationsList.Add(active);

            return operationsList;
        }

        public List<ListItemOperation> GetItemExtraOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> extraOperationsList = new List<ListItemOperation>();

            ListItemOperation mandatory = new ListItemOperation();
            mandatory.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
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
            reset.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation };
            reset.AutomationID = "reset";
            reset.Group = "Reset Operations";
            reset.GroupImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset;
            reset.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset;
            reset.Header = string.Format("Reset {0} execution details", GingerDicser.GetTermResValue(eTermResKey.Activity));
            reset.ToolTip = string.Format("Reset {0} execution details", GingerDicser.GetTermResValue(eTermResKey.Activity));
            reset.OperationHandler = ResetHandler;
            extraOperationsList.Add(reset);

            ListItemOperation resetRest = new ListItemOperation();
            resetRest.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation };
            resetRest.AutomationID = "resetRest";
            resetRest.Group = "Reset Operations";
            resetRest.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset;
            resetRest.Header = string.Format("Reset execution details from this {0}", GingerDicser.GetTermResValue(eTermResKey.Activity));
            resetRest.ToolTip = string.Format("Reset execution details from this {0}", GingerDicser.GetTermResValue(eTermResKey.Activity));
            resetRest.OperationHandler = ResetResetHandler;
            extraOperationsList.Add(resetRest);

            ListItemOperation copy = new ListItemOperation();
            copy.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.View, General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            copy.AutomationID = "copy";
            copy.Group = "Clipboard";
            copy.GroupImageType = Amdocs.Ginger.Common.Enums.eImageType.Clipboard;
            copy.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Copy;
            copy.Header = "Copy";
            copy.OperationHandler = CopyHandler;
            extraOperationsList.Add(copy);

            ListItemOperation cut = new ListItemOperation();
            cut.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            cut.AutomationID = "cut";
            cut.Group = "Clipboard";
            cut.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Cut;
            cut.Header = "Cut";
            cut.OperationHandler = CutHandler;
            extraOperationsList.Add(cut);

            ListItemOperation pasterAfterCurrent = new ListItemOperation();
            pasterAfterCurrent.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            pasterAfterCurrent.AutomationID = "pasterAfterCurrent";
            pasterAfterCurrent.Group = "Clipboard";
            pasterAfterCurrent.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Paste;
            pasterAfterCurrent.Header = "Paste";
            pasterAfterCurrent.OperationHandler = PasteAfterCurrentHandler;
            extraOperationsList.Add(pasterAfterCurrent);

            ListItemOperation moveToOtherGroup = new ListItemOperation();
            moveToOtherGroup.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            moveToOtherGroup.AutomationID = "moveToOtherGroup";
            moveToOtherGroup.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveUpDown;
            moveToOtherGroup.Header = "Move to Other Group";
            moveToOtherGroup.ToolTip = "Move to Other Group";
            moveToOtherGroup.OperationHandler = MoveToOtherGroupHandler;
            extraOperationsList.Add(moveToOtherGroup);

            ListItemOperation addToSR = new ListItemOperation();
            addToSR.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
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

            ListItemOperation run = new ListItemOperation();
            run.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation };
            run.AutomationID = "run";
            run.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Run;
            run.ToolTip = "Run " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            run.OperationHandler = RunHandler;
            executionOperationsList.Add(run);

            ListItemOperation continueRun = new ListItemOperation();
            continueRun.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation };
            continueRun.AutomationID = "continueRun";
            continueRun.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Continue;
            continueRun.ToolTip = "Continue Run from " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            continueRun.OperationHandler = ContinueRunHandler;
            executionOperationsList.Add(continueRun);

            ListItemOperation runAction = new ListItemOperation();
            runAction.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation };
            runAction.AutomationID = "runAction";
            runAction.ImageType = Amdocs.Ginger.Common.Enums.eImageType.RunSingle;
            runAction.ToolTip = "Run Current Action";
            runAction.OperationHandler = RunActionHandler;
            executionOperationsList.Add(runAction);

            return executionOperationsList;
        }

        public List<ListItemNotification> GetItemGroupNotificationsList(string GroupName)
        {
            ActivitiesGroup group = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == GroupName).FirstOrDefault();
            if (group != null)
            {
                List<ListItemNotification> notificationsList = new List<ListItemNotification>();

                ListItemNotification sharedRepoInd = new ListItemNotification();
                sharedRepoInd.AutomationID = "sharedRepoInd";
                sharedRepoInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem;
                sharedRepoInd.ToolTip = string.Format("{0} source is from Shared Repository", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup));
                sharedRepoInd.ImageForeground = Brushes.Orange;
                sharedRepoInd.BindingObject = group;
                sharedRepoInd.BindingFieldName = nameof(ActivitiesGroup.IsSharedRepositoryInstance);
                sharedRepoInd.BindingConverter = new BoolVisibilityConverter();
                notificationsList.Add(sharedRepoInd);

                return notificationsList;
            }
            else
            {
                return null;
            }
        }

        public List<ListItemGroupOperation> GetItemGroupOperationsList()
        {
            List<ListItemGroupOperation> groupOperationsList = new List<ListItemGroupOperation>();

            ListItemGroupOperation addNewActivity = new ListItemGroupOperation();
            addNewActivity.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            addNewActivity.AutomationID = "addNewGroupActivity";
            addNewActivity.Header = "Add New " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            addNewActivity.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Add;
            addNewActivity.ToolTip = "Add New " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            addNewActivity.OperationHandler = AddNewActivityToGroupHandler;
            groupOperationsList.Add(addNewActivity);

            ListItemGroupOperation rename = new ListItemGroupOperation();
            rename.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            rename.AutomationID = "renameGroup";
            rename.Header = "Rename Group";
            rename.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit;
            rename.ToolTip = "Rename group";
            rename.OperationHandler = RenameGroupHandler;
            groupOperationsList.Add(rename);

            ListItemGroupOperation moveUp = new ListItemGroupOperation();
            moveUp.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            moveUp.AutomationID = "moveGroupUp";
            moveUp.Header = "Move Group Up";
            moveUp.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveUp;
            moveUp.ToolTip = "Move all group up";
            moveUp.OperationHandler = MoveGroupUpHandler;
            groupOperationsList.Add(moveUp);

            ListItemGroupOperation moveDown = new ListItemGroupOperation();
            moveDown.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            moveDown.AutomationID = "moveGroupDown";
            moveDown.Header = "Move Group Down";
            moveDown.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveDown;
            moveDown.ToolTip = "Move all group down";
            moveDown.OperationHandler = MoveGroupDownHandler;
            groupOperationsList.Add(moveDown);

            ListItemGroupOperation delete = new ListItemGroupOperation();
            delete.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            delete.AutomationID = "deleteGroup";
            delete.Header = "Delete Group";
            delete.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
            delete.ToolTip = "Delete all group " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            delete.OperationHandler = DeleteGroupHandler;
            groupOperationsList.Add(delete);

            ListItemGroupOperation copyGroup = new ListItemGroupOperation();
            copyGroup.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.View, General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            copyGroup.AutomationID = "copyGroup";
            copyGroup.Group = "Clipboard";
            copyGroup.GroupImageType = Amdocs.Ginger.Common.Enums.eImageType.Clipboard;
            copyGroup.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Copy;
            copyGroup.Header = "Copy Group Items";
            copyGroup.OperationHandler = CopyGroupHandler;
            groupOperationsList.Add(copyGroup);

            ListItemGroupOperation cutGroup = new ListItemGroupOperation();
            cutGroup.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            cutGroup.AutomationID = "cutGroup";
            cutGroup.Group = "Clipboard";
            cutGroup.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Cut;
            cutGroup.Header = "Cut Group Items";
            cutGroup.OperationHandler = CutGroupHandler;
            groupOperationsList.Add(cutGroup);

            ListItemGroupOperation pasteInGroup = new ListItemGroupOperation();
            pasteInGroup.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            pasteInGroup.AutomationID = "pasterAfterCurrent";
            pasteInGroup.Group = "Clipboard";
            pasteInGroup.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Paste;
            pasteInGroup.Header = "Paste";
            pasteInGroup.OperationHandler = PasteInGroupHandler;
            groupOperationsList.Add(pasteInGroup);

            ListItemGroupOperation addToSR = new ListItemGroupOperation();
            addToSR.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            addToSR.AutomationID = "addGroupToSR";
            addToSR.Header = "Add Group to Shared Repository";
            addToSR.ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem;
            addToSR.ToolTip = "Add group and it " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " to Shared Repository";
            addToSR.OperationHandler = AddGroupToSRHandler;
            groupOperationsList.Add(addToSR);

            ListItemGroupOperation export = new ListItemGroupOperation();
            export.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
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


        private void MoveToOtherGroupHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            ActivitiesGroup targetGroup = (new ActivitiesGroupSelectionPage(mContext.BusinessFlow)).ShowAsWindow();
            if (targetGroup != null)
            {
                try
                {
                    mContext.BusinessFlow.MoveActivityBetweenGroups(mActivity, targetGroup);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Error occured while dragging Activity to other group", ex);
                }
                ListView.UpdateGrouping();
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
            int index = mContext.BusinessFlow.Activities.IndexOf(mActivity);
            if (index > 0 && mContext.BusinessFlow.Activities[index - 1].ActivitiesGroupID == mActivity.ActivitiesGroupID)
            {
                ExpandItemOnLoad = true;
                mContext.BusinessFlow.MoveActivityInGroup(mActivity, index - 1);
            }
        }

        private void MoveDownHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            int index = mContext.BusinessFlow.Activities.IndexOf(mActivity);
            if (index < (mContext.BusinessFlow.Activities.Count - 1) && mContext.BusinessFlow.Activities[index + 1].ActivitiesGroupID == mActivity.ActivitiesGroupID)
            {
                ExpandItemOnLoad = true;
                mContext.BusinessFlow.MoveActivityInGroup(mActivity, index+1);
            }
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
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "This is an automatic group created from Gherkin file and can not be modified");
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
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == ((MenuItem)sender).Tag.ToString()).FirstOrDefault();
            if (Reporter.ToUser(eUserMsgKey.SureWantToDeleteGroup, activitiesGroup.Name) == eUserMsgSelection.Yes)
            {                
                mContext.BusinessFlow.DeleteActivitiesGroup(activitiesGroup);
            }
        }

        private void AddGroupToSRHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == ((MenuItem)sender).Tag.ToString()).FirstOrDefault();

            List<RepositoryItemBase> list = new List<RepositoryItemBase>();
            list.Add(activitiesGroup);
            foreach (ActivityIdentifiers activityIdnt in activitiesGroup.ActivitiesIdentifiers)
            {
                list.Add(activityIdnt.IdentifiedActivity);
            }

            (new Repository.SharedRepositoryOperations()).AddItemsToRepository(mContext, list);
        }

        private void ExportGroupHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == ((MenuItem)sender).Tag.ToString()).FirstOrDefault();
            ObservableList<ActivitiesGroup> list = new ObservableList<ActivitiesGroup>();
            list.Add(activitiesGroup);
            ALMIntegration.Instance.ExportBfActivitiesGroupsToALM(mContext.BusinessFlow, list);
        }

        private void CopyAllListHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            foreach (Activity activity in mContext.BusinessFlow.Activities)
            {
                list.Add(activity);
            }
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutAllListHandler(object sender, RoutedEventArgs e)
        {            
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            foreach (Activity activity in mContext.BusinessFlow.Activities)
            {
                list.Add(activity);
            }
            ClipboardOperationsHandler.SetCutItems(ListView, list);
        }

        private void CopySelectedHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            foreach (Activity act in ListView.List.SelectedItems)
            {
                list.Add(act);
            }
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutSelectedHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            foreach (Activity act in ListView.List.SelectedItems)
            {
                list.Add(act);
            }
            ClipboardOperationsHandler.SetCutItems(ListView, list);
        }

        private void CopyHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            list.Add(mActivity);
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            list.Add(mActivity);
            ClipboardOperationsHandler.SetCutItems(ListView, list);
        }

        private void PasteAfterCurrentHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);

            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == mActivity.ActivitiesGroupID).FirstOrDefault();
            int insertIndex = mContext.BusinessFlow.Activities.IndexOf(mActivity) + 1;
            DoActivitiesPaste(activitiesGroup, insertIndex);
        }

        private void CopyGroupHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == ((MenuItem)sender).Tag.ToString()).FirstOrDefault();
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            foreach (ActivityIdentifiers activityIdnt in activitiesGroup.ActivitiesIdentifiers)
            {
                list.Add(activityIdnt.IdentifiedActivity);
            }
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutGroupHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == ((MenuItem)sender).Tag.ToString()).FirstOrDefault();
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            foreach (ActivityIdentifiers activityIdnt in activitiesGroup.ActivitiesIdentifiers)
            {
                list.Add(activityIdnt.IdentifiedActivity);
            }
            ClipboardOperationsHandler.SetCutItems(ListView, list);
        }

        private void PasteInGroupHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == ((MenuItem)sender).Tag.ToString()).FirstOrDefault();
            int insertIndex = mContext.BusinessFlow.Activities.IndexOf(activitiesGroup.ActivitiesIdentifiers[activitiesGroup.ActivitiesIdentifiers.Count - 1].IdentifiedActivity) + 1;
            DoActivitiesPaste(activitiesGroup, insertIndex);
        }

        private void PasteInListHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = (new ActivitiesGroupSelectionPage(mContext.BusinessFlow)).ShowAsWindow();
            if (activitiesGroup != null)
            {
                int insertIndex = 0;
                if (activitiesGroup.ActivitiesIdentifiers.Count > 0)
                {
                    insertIndex = mContext.BusinessFlow.Activities.IndexOf(activitiesGroup.ActivitiesIdentifiers[activitiesGroup.ActivitiesIdentifiers.Count - 1].IdentifiedActivity) + 1;
                }
                else
                {
                    insertIndex = mContext.BusinessFlow.Activities.Count;//last
                }

                DoActivitiesPaste(activitiesGroup, insertIndex);
            }
        }

        private void DoActivitiesPaste(ActivitiesGroup activitiesGroup, int insertIndex)
        {
            try
            {
                List<RepositoryItemBase> CopiedorCutActivities = ClipboardOperationsHandler.CopiedorCutItems.Where(x => x is Activity).ToList();
                if (CopiedorCutActivities.Count > 0)
                {
                    Reporter.ToStatus(eStatusMsgKey.PasteProcess, null, string.Format("Performing paste operation for {0} items...", ClipboardOperationsHandler.CopiedorCutItems.Count));

                    foreach (RepositoryItemBase item in CopiedorCutActivities)
                    {
                        if (item is Activity)
                        {
                            if (ClipboardOperationsHandler.CutSourceList == null)//Copy
                            {
                                Activity copiedItem = (Activity)item.CreateCopy();
                                //set unique name
                                GingerCoreNET.GeneralLib.General.SetUniqueNameToRepoItem(GetActivitiesList(), copiedItem, "_Copy");
                                //Set T.app
                                if (mContext.BusinessFlow.TargetApplications.Where(x=>x.Name == copiedItem.TargetApplication).FirstOrDefault() == null 
                                                && mContext.BusinessFlow.TargetApplications.Count > 0)
                                {
                                    copiedItem.TargetApplication = mContext.BusinessFlow.TargetApplications[0].Name;
                                }
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
                                    insertIndex--;
                                }
                                else
                                {
                                    //clear from source  
                                    ClipboardOperationsHandler.CutSourceList.Remove(item);
                                    //set unique name
                                    GingerCoreNET.GeneralLib.General.SetUniqueNameToRepoItem(GetActivitiesList(), item);
                                }
                                //Set T.app
                                if (mContext.BusinessFlow.TargetApplications.Where(x => x.Name == ((Activity)item).TargetApplication).FirstOrDefault() == null
                                                            && mContext.BusinessFlow.TargetApplications.Count > 0)
                                {
                                    ((Activity)item).TargetApplication = mContext.BusinessFlow.TargetApplications[0].Name;
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
                else
                {
                    Reporter.ToStatus(eStatusMsgKey.PasteProcess, null, string.Format("No {0} found to paste", GingerDicser.GetTermResValue(eTermResKey.Activities)));
                }
            }
            finally
            {
                Reporter.HideStatusMessage();
            }
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
