#region License
/*
Copyright © 2014-2023 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.Actions;
using Ginger.Repository;
using Ginger.Repository.AddItemToRepositoryWizard;
using Ginger.UserControlsLib;
using Ginger.UserControlsLib.UCListView;
using Ginger.Variables;
using GingerCore;
using GingerCore.Actions;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Ginger.BusinessFlowPages.ListHelpers
{
    public class ActionsListViewHelper : IListViewHelper
    {
        Act mAction;
        
        public General.eRIPageViewMode PageViewMode { get; set; }

        UcListView mListView = null;
        public UcListView ListView
        {
            get
            {
                return mListView;
            }
            set
            {
                if (mListView != value)
                {
                    //if (mListView != null)
                    //{
                    //    mListView.UcListViewEvent -= ListView_UcListViewEvent;
                    //}
                    mListView = value;
                    //if (mListView != null)
                    //{
                    //    mListView.UcListViewEvent += ListView_UcListViewEvent;
                    //}
                }
            }
        }

        Context mContext;
        public Context Context
        {
            get
            {
                return mContext;
            }
        }

        public delegate void ActionListItemEventHandler(ActionListItemEventArgs EventArgs);
        public event ActionListItemEventHandler ActionListItemEvent;
        private void OnActionListItemEvent(ActionListItemEventArgs.eEventType eventType, Object eventObject = null)
        {
            ActionListItemEventHandler handler = ActionListItemEvent;
            if (handler != null)
            {
                handler(new ActionListItemEventArgs(eventType, eventObject));
            }
        }

        public bool AllowExpandItems { get; set; } = true;

        public bool ExpandItemOnLoad { get; set; } = false;

        public ActionsListViewHelper(Context context, General.eRIPageViewMode pageViewMode)
        {
            mContext = context;
            PageViewMode = pageViewMode;
        }

        public void UpdatePageViewMode(Ginger.General.eRIPageViewMode pageViewMode)
        {
            PageViewMode = pageViewMode;
            
        }

        public void SetItem(object item)
        {
            if (item is Act)
            {
                mAction = (Act)item;
            }
            else if(item is ucButton)
            {
                if(((ucButton)item).Tag is Act)
                {
                    mAction = (Act)(((ucButton)item).Tag);
                }                
            }
            else if (item is MenuItem)
            {
                if (((MenuItem)item).Tag is Act)
                {
                    mAction = ((Act)((MenuItem)item).Tag);
                }
            }
            else if(item is ApplicationPOMModel)
            {
                mAction = (Act)item;
            }
        }

        public string GetItemNameField()
        {
            return nameof(Act.Description);
        }

        public string GetItemMandatoryField()
        {
            return null;
        }

        public string GetItemNameExtentionField()
        {
            switch (PageViewMode)
            {
                case General.eRIPageViewMode.Automation:
                case General.eRIPageViewMode.ViewAndExecute:
                    return nameof(Act.ElapsedSecs); 

                case General.eRIPageViewMode.AddFromModel: //Add from POM
                    return nameof(ElementInfo.ElementTypeEnum);

                default:
                    return null;
            }
        }

        public string GetItemTagsField()
        {
            return nameof(Act.Tags);
        }

        public string GetItemDescriptionField()
        {
            switch(PageViewMode)
            {
                case General.eRIPageViewMode.Automation:
                case General.eRIPageViewMode.AddFromShardRepository:
                case General.eRIPageViewMode.AddFromModel: //Add from POM
                    return nameof(Act.ActionType);

                case General.eRIPageViewMode.Add: //Actions Library
                    return nameof(Act.ActionUserDescription);

                default:
                    return nameof(Act.ActionType);
            }
        }

        public string GetItemErrorField()
        {
            return nameof(Act.Error);
        }

        public string GetItemExecutionStatusField()
        {
            if (PageViewMode == General.eRIPageViewMode.Automation || PageViewMode == General.eRIPageViewMode.ViewAndExecute)
            {
                return nameof(Act.Status);
            }    
            else
            {
                return null;
            }
        }

        public string GetItemActiveField()
        {
            return nameof(Act.Active);
        }

        public ListItemUniqueIdentifier GetItemUniqueIdentifier(object item)
        {
            SetItem(item);

            if ((PageViewMode == General.eRIPageViewMode.Automation|| PageViewMode == General.eRIPageViewMode.View || PageViewMode == General.eRIPageViewMode.ViewAndExecute) 
                && mAction.BreakPoint)
            {
                return new ListItemUniqueIdentifier() { Color = "Red", Tooltip = "Break Point was set for this Action" };
            }
            else
            {
                return null;
            }
        }

        public string GetItemIconField()
        {
            return nameof(Act.Image);
        }

        public string GetItemIconTooltipField()
        {
            return nameof(Act.ActionType);
        }

        public List<ListItemOperation> GetListOperations()
        {
            List<ListItemOperation> operationsList = new List<ListItemOperation>();

            ListItemOperation deleteSelected = new ListItemOperation();
            deleteSelected.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            deleteSelected.AutomationID = "deleteSelected";
            deleteSelected.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
            deleteSelected.ToolTip = "Delete Selected Actions (Del)";
            deleteSelected.OperationHandler = DeleteSelectedHandler;
            operationsList.Add(deleteSelected);

            ListItemOperation addSelected = new ListItemOperation();
            addSelected.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Add };
            addSelected.AutomationID = "addSelected";
            addSelected.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveLeft;
            addSelected.ToolTip = "Add Selected Actions";
            addSelected.OperationHandler = AddActionListView;
            operationsList.Add(addSelected);

            ListItemOperation addToFlow = new ListItemOperation();
            addToFlow.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.AddFromShardRepository };
            addToFlow.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveLeft;
            addToFlow.ToolTip = "Add to Flow";
            addToFlow.OperationHandler = AddFromRepository;
            operationsList.Add(addToFlow);

            ListItemOperation editItem = new ListItemOperation();
            editItem.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.AddFromShardRepository };
            editItem.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit;
            editItem.ToolTip = "Edit Item";
            editItem.OperationHandler = EditAct;
            operationsList.Add(editItem);

            return operationsList;
        }

        private void AddActionListView(object sender, RoutedEventArgs e)
        {
            List<RepositoryItemBase> list = new List<RepositoryItemBase>();
            List<object> SelectedItemsList = mListView.List.SelectedItems.Cast<object>().ToList();
            foreach (Act act in SelectedItemsList)
            {
                list.Add(act);
                ActionsFactory.AddActionsHandler(act, mContext);
            }

        }

        private void AddFromRepository(object sender, RoutedEventArgs e)
        {
            if (mListView.List.SelectedItems != null && mListView.List.SelectedItems.Count > 0)
            {
                if (mContext.BusinessFlow == null)
                {
                    return;
                }
                List<Act> list = new List<Act>();
                foreach (Act selectedItem in mListView.List.SelectedItems)
                {
                    ActionsFactory.AddActionsHandler(selectedItem, mContext);
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        private void EditAct(object sender, RoutedEventArgs e)
        {
            if (mListView.List.SelectedItems != null && mListView.List.SelectedItems.Count > 0)
            {
                Act a = (Act)mListView.CurrentItem;
                ActionEditPage actedit = new ActionEditPage(a, General.eRIPageViewMode.SharedReposiotry, new GingerCore.BusinessFlow(), new GingerCore.Activity());
                actedit.ShowAsWindow(eWindowShowStyle.Dialog);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectItem);
            }
        }

    
        public List<ListItemOperation> GetListExtraOperations()
        {
            List<ListItemOperation> extraOperationsList = new List<ListItemOperation>();

            ListItemOperation actionVarsDep = new ListItemOperation();
            actionVarsDep.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            actionVarsDep.AutomationID = "actionVarsDep";
            actionVarsDep.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MapSigns;
            actionVarsDep.Header = "Actions-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies";
            actionVarsDep.ToolTip = "Set Actions-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies";
            actionVarsDep.OperationHandler = ActionsVarsHandler;
            extraOperationsList.Add(actionVarsDep);

            ListItemOperation activeUnactiveAllActions = new ListItemOperation();
            activeUnactiveAllActions.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            activeUnactiveAllActions.AutomationID = "activeUnactiveAllActions";
            activeUnactiveAllActions.ImageType = Amdocs.Ginger.Common.Enums.eImageType.CheckBox;
            activeUnactiveAllActions.Header = "Activate/Deactivate all Actions";
            activeUnactiveAllActions.ToolTip = "Activate/Deactivate all Actions";
            activeUnactiveAllActions.OperationHandler = ActiveUnactiveAllActionsHandler;
            extraOperationsList.Add(activeUnactiveAllActions);

            ListItemOperation takeUntakeSS = new ListItemOperation();
            takeUntakeSS.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            takeUntakeSS.AutomationID = "takeUntakeSS";
            takeUntakeSS.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Image;
            takeUntakeSS.Header = "Take/Untake Screen Shots";
            takeUntakeSS.ToolTip = "Set Take/Untake Screen Shots to all Actions";
            takeUntakeSS.OperationHandler = TakeUntakeSSHandler;
            extraOperationsList.Add(takeUntakeSS);

            ListItemOperation copyAllList = new ListItemOperation();
            copyAllList.SupportedViews= new List<General.eRIPageViewMode>() { General.eRIPageViewMode.View, General.eRIPageViewMode.ViewAndExecute, General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            copyAllList.AutomationID = "copyAllList";
            copyAllList.Group = "Clipboard";
            copyAllList.GroupImageType = Amdocs.Ginger.Common.Enums.eImageType.Clipboard;
            copyAllList.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Copy;
            copyAllList.Header = "Copy All List Items";
            copyAllList.OperationHandler = CopyAllListHandler;
            extraOperationsList.Add(copyAllList);

            ListItemOperation cutAllList = new ListItemOperation();
            cutAllList.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            cutAllList.AutomationID = "cutAllList";
            cutAllList.Group = "Clipboard";
            cutAllList.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Cut;
            cutAllList.Header = "Cut All List Items";
            cutAllList.OperationHandler = CutAllListHandler;
            extraOperationsList.Add(cutAllList);

            ListItemOperation copySelected = new ListItemOperation();
            copySelected.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.View, General.eRIPageViewMode.ViewAndExecute, General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            copySelected.AutomationID = "copySelected";
            copySelected.Group = "Clipboard";
            copySelected.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Copy;
            copySelected.Header = "Copy Selected Items (Ctrl+C)";
            copySelected.OperationHandler = CopySelectedHandler;
            extraOperationsList.Add(copySelected);

            ListItemOperation cutSelected = new ListItemOperation();
            cutSelected.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            cutSelected.AutomationID = "cutSelected";
            cutSelected.Group = "Clipboard";
            cutSelected.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Cut;
            cutSelected.Header = "Cut Selected Items (Ctrl+X)";
            cutSelected.OperationHandler = CutSelectedHandler;
            extraOperationsList.Add(cutSelected);

            ListItemOperation pasteInList = new ListItemOperation();
            pasteInList.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            pasteInList.AutomationID = "pasteInList";
            pasteInList.Group = "Clipboard";
            pasteInList.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Paste;
            pasteInList.Header = "Paste (Ctrl+V)";
            pasteInList.OperationHandler = PasteInListHandler;
            extraOperationsList.Add(pasteInList);

            ListItemOperation deleteAll = new ListItemOperation();
            deleteAll.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            deleteAll.AutomationID = "deleteAll";
            deleteAll.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
            deleteAll.Header = "Delete All Actions";
            deleteAll.OperationHandler = DeleteAllHandler;
            extraOperationsList.Add(deleteAll);

            ListItemOperation addSelectedToSR = new ListItemOperation();
            addSelectedToSR.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            addSelectedToSR.AutomationID = "addSelectedToSR";
            addSelectedToSR.ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem;
            addSelectedToSR.Header = "Add Selected to Shared Repository";
            addSelectedToSR.OperationHandler = AddSelectedToSRHandler;
            extraOperationsList.Add(addSelectedToSR);

            return extraOperationsList;
        }

        public List<ListItemNotification> GetItemNotificationsList(object item)
        {
            if (PageViewMode != General.eRIPageViewMode.Add)
            {
                SetItem(item);
                List<ListItemNotification> notificationsList = new List<ListItemNotification>();

                ListItemNotification simulationInd = new ListItemNotification();
                simulationInd.AutomationID = "simulationInd";
                simulationInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Simulate;
                simulationInd.ToolTip = "Action support Simulation mode";
                simulationInd.ImageSize = 14;
                simulationInd.BindingObject = mAction;
                simulationInd.BindingFieldName = nameof(Act.SupportSimulation);
                simulationInd.BindingConverter = new BoolVisibilityConverter();
                notificationsList.Add(simulationInd);

                ListItemNotification flowControlInd = new ListItemNotification();
                flowControlInd.AutomationID = "flowControlInd";
                flowControlInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MapSigns;
                flowControlInd.ToolTip = "Action contains Flow Control conditions";
                flowControlInd.ImageSize = 14;
                flowControlInd.BindingObject = mAction;
                flowControlInd.BindingFieldName = nameof(Act.FlowControlsInfo);
                flowControlInd.BindingConverter = new StringVisibilityConverter();
                notificationsList.Add(flowControlInd);

                ListItemNotification actionsVarsDepInd = new ListItemNotification();
                actionsVarsDepInd.AutomationID = "actionsVarsDepInd";
                actionsVarsDepInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MapSigns;
                actionsVarsDepInd.ToolTip = string.Format("{0} Actions-{1} dependency is enabled", GingerDicser.GetTermResValue(eTermResKey.Activity), GingerDicser.GetTermResValue(eTermResKey.Variables));
                actionsVarsDepInd.ImageSize = 14;
                actionsVarsDepInd.BindingObject = mContext.Activity;
                actionsVarsDepInd.BindingFieldName = nameof(Activity.EnableActionsVariablesDependenciesControl);
                actionsVarsDepInd.BindingConverter = new BoolVisibilityConverter();
                notificationsList.Add(actionsVarsDepInd);

                ListItemNotification outputValuesInd = new ListItemNotification();
                outputValuesInd.AutomationID = "outputValuesInd";
                outputValuesInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Output;
                outputValuesInd.ToolTip = "Action contains Output Values";
                outputValuesInd.BindingObject = mAction;
                outputValuesInd.BindingFieldName = nameof(Act.ReturnValuesCount);
                outputValuesInd.BindingConverter = new OutPutValuesCountConverter();
                notificationsList.Add(outputValuesInd);

                ListItemNotification waitInd = new ListItemNotification();
                waitInd.AutomationID = "waitInd";
                waitInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Clock;
                waitInd.ToolTip = "Action contains Wait time before execution starts";
                waitInd.BindingObject = mAction;
                waitInd.BindingFieldName = nameof(Act.WaitVE);
                waitInd.BindingConverter = new WaitVisibilityConverter();
                notificationsList.Add(waitInd);

                ListItemNotification retryInd = new ListItemNotification();
                retryInd.AutomationID = "retryInd";
                retryInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Retry;
                retryInd.ToolTip = "Action configured to Rerun in case of failure";
                retryInd.BindingObject = mAction;
                retryInd.BindingFieldName = nameof(Act.EnableRetryMechanism);
                retryInd.BindingConverter = new BoolVisibilityConverter();
                notificationsList.Add(retryInd);

                ListItemNotification screenshotInd = new ListItemNotification();
                screenshotInd.AutomationID = "screenshotInd";
                screenshotInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Image;
                screenshotInd.ToolTip = "Action configured to take Screenshot";
                screenshotInd.BindingObject = mAction;
                screenshotInd.BindingFieldName = nameof(Act.TakeScreenShot);
                screenshotInd.BindingConverter = new BoolVisibilityConverter();
                notificationsList.Add(screenshotInd);

                if (PageViewMode != General.eRIPageViewMode.AddFromShardRepository)
                {
                    ListItemNotification sharedRepoInd = new ListItemNotification();
                    sharedRepoInd.AutomationID = "sharedRepoInd";
                    sharedRepoInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem;
                    sharedRepoInd.ToolTip = "Action source is from Shared Repository";
                    sharedRepoInd.ImageForeground = Brushes.Orange;
                    sharedRepoInd.BindingObject = mAction;
                    sharedRepoInd.BindingFieldName = nameof(Act.IsSharedRepositoryInstance);
                    sharedRepoInd.BindingConverter = new BoolVisibilityConverter();
                    notificationsList.Add(sharedRepoInd);
                }

                return notificationsList;
            }
            else
            {
                return null;
            }
        }

        public List<ListItemOperation> GetItemOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> operationsList = new List<ListItemOperation>();

            ListItemOperation edit = new ListItemOperation();
            edit.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            edit.AutomationID = "edit";
            edit.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit;
            edit.ToolTip = "Edit Action";
            edit.OperationHandler = EditHandler;
            operationsList.Add(edit);

            ListItemOperation moveUp = new ListItemOperation();
            moveUp.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            moveUp.AutomationID = "moveUp";
            moveUp.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveUp;
            moveUp.ToolTip = "Move Up";
            moveUp.OperationHandler = MoveUpHandler;
            operationsList.Add(moveUp);

            ListItemOperation moveDown = new ListItemOperation();
            moveDown.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            moveDown.AutomationID = "moveDown";
            moveDown.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveDown;
            moveDown.ToolTip = "Move Down";
            moveDown.OperationHandler = MoveDownHandler;
            operationsList.Add(moveDown);

            ListItemOperation delete = new ListItemOperation();
            delete.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            delete.AutomationID = "delete";
            delete.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
            delete.ToolTip = "Delete";
            delete.OperationHandler = DeleteHandler;
            operationsList.Add(delete);

            ListItemOperation active = new ListItemOperation();
            active.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            active.AutomationID = "active";
            active.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active;
            active.ImageBindingObject = mAction;
            active.ImageBindingFieldName = nameof(Act.Active);
            active.ImageBindingConverter = new ActiveImageTypeConverter();
            active.ToolTip = "Active";
            active.OperationHandler = ActiveHandler;
            operationsList.Add(active);

            ListItemOperation viewLinkedInstances = new ListItemOperation();
            viewLinkedInstances.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.AddFromShardRepository };
            viewLinkedInstances.AutomationID = "ViewLinkedInstances";
            viewLinkedInstances.ImageType = Amdocs.Ginger.Common.Enums.eImageType.InstanceLink;
            viewLinkedInstances.ToolTip = "View Item Usage";
            viewLinkedInstances.OperationHandler = ViewRepositoryItemUsage;
            operationsList.Add(viewLinkedInstances);

            return operationsList;
        }

        public List<ListItemOperation> GetItemExtraOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> extraOperationsList = new List<ListItemOperation>();

            ListItemOperation breakPoint = new ListItemOperation();
            breakPoint.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.ViewAndExecute };
            breakPoint.AutomationID = "breakPoint";
            breakPoint.Header = "Break Point";
            breakPoint.ToolTip = "Stop execution on that Action";
            breakPoint.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active;
            breakPoint.ImageBindingObject = mAction;
            breakPoint.ImageBindingFieldName = nameof(Act.BreakPoint);
            breakPoint.ImageBindingConverter = new ActiveImageTypeConverter();
            breakPoint.OperationHandler = BreakPointHandler;
            extraOperationsList.Add(breakPoint);

            ListItemOperation reset = new ListItemOperation();
            reset.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.ViewAndExecute };
            reset.AutomationID = "reset";
            reset.Group = "Reset Operations";
            reset.GroupImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset;
            reset.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset;
            reset.Header = "Reset Action execution details";
            reset.ToolTip = "Reset Action execution details";
            reset.OperationHandler = ResetHandler;
            extraOperationsList.Add(reset);

            ListItemOperation resetRest = new ListItemOperation();
            resetRest.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.ViewAndExecute };
            resetRest.AutomationID = "resetRest";
            resetRest.Group = "Reset Operations";
            resetRest.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset;
            resetRest.Header = "Reset execution details from this Action";
            resetRest.ToolTip = "Reset execution details from this Action";
            resetRest.OperationHandler = ResetResetHandler;
            extraOperationsList.Add(resetRest);

            ListItemOperation copy = new ListItemOperation();
            copy.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.View, General.eRIPageViewMode.ViewAndExecute, General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            copy.AutomationID = "copy";
            copy.Group = "Clipboard";
            copy.GroupImageType = Amdocs.Ginger.Common.Enums.eImageType.Clipboard;
            copy.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Copy;
            copy.Header = "Copy (Ctrl+C)";
            copy.OperationHandler = CopyHandler;
            extraOperationsList.Add(copy);

            ListItemOperation cut = new ListItemOperation();
            cut.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            cut.AutomationID = "cut";
            cut.Group = "Clipboard";
            cut.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Cut;
            cut.Header = "Cut (Ctrl+X)";
            cut.OperationHandler = CutHandler;
            extraOperationsList.Add(cut);

            ListItemOperation pasterAfterCurrent = new ListItemOperation();
            pasterAfterCurrent.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            pasterAfterCurrent.AutomationID = "pasterAfterCurrent";
            pasterAfterCurrent.Group = "Clipboard";
            pasterAfterCurrent.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Paste;
            pasterAfterCurrent.Header = "Paste (Ctrl+V)";
            pasterAfterCurrent.OperationHandler = PasteAfterCurrentHandler;
            extraOperationsList.Add(pasterAfterCurrent);

            ListItemOperation addToSR = new ListItemOperation();
            addToSR.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone};
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
            run.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.ViewAndExecute};
            run.AutomationID = "run";
            run.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Run;
            run.ToolTip = "Run Action";
            run.OperationHandler = RunHandler;
            executionOperationsList.Add(run);

            ListItemOperation continueRun = new ListItemOperation();
            continueRun.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.ViewAndExecute};
            continueRun.AutomationID = "continueRun";
            continueRun.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Continue;
            continueRun.ToolTip = "Continue Run from Action";
            continueRun.OperationHandler = ContinueRunHandler;
            executionOperationsList.Add(continueRun);

            return executionOperationsList;
        }

        public List<ListItemNotification> GetItemGroupNotificationsList(string GroupName)
        {
            return null;
        }

        public List<ListItemGroupOperation> GetItemGroupOperationsList()
        {
            return null;
        }

        private void DeleteAllHandler(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.SureWantToDeleteAll) == eUserMsgSelection.Yes)
            {                
                mContext.Activity.Acts.Clear();
            }
        }

        private void DeleteSelectedHandler(object sender, RoutedEventArgs e)
        {
            if (ListView.List.SelectedItems.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.SelectItemToDelete);
                return;
            }

            if (Reporter.ToUser(eUserMsgKey.SureWantToDeleteSelectedItems, "Actions", ((Act)ListView.List.SelectedItems[0]).Description) == eUserMsgSelection.Yes)
            {
                List<object> SelectedItemsList = ListView.List.SelectedItems.Cast<object>().ToList();
                foreach (Act act in SelectedItemsList)
                {
                    mContext.Activity.Acts.Remove(act);
                }
            }
        }

        private void EditHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mAction.SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper();
            mAction.Context = mContext;
            OnActionListItemEvent(ActionListItemEventArgs.eEventType.ShowActionEditPage, mAction);
        }

        private void ActiveHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mAction.Active = !mAction.Active;
        }

        private void ViewRepositoryItemUsage(object sender, RoutedEventArgs e)
        {
            List<object> SelectedItemsList = mListView.List.SelectedItems.Cast<object>().ToList();

            if (SelectedItemsList.Count > 0)
            {
                RepositoryItemUsagePage usagePage = new RepositoryItemUsagePage((RepositoryItemBase)mListView.List.SelectedItem);
                usagePage.ShowAsWindow();
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }


        private void ActionsVarsHandler(object sender, RoutedEventArgs e)
        {            
            VariablesDependenciesPage actionsDepPage = new VariablesDependenciesPage(mContext.Activity);
            actionsDepPage.ShowAsWindow();
        }

        private void ActiveUnactiveAllActionsHandler(object sender, RoutedEventArgs e)
        {            
            if (mContext.Activity.Acts.Count > 0)
            {
                bool activeValue = !mContext.Activity.Acts[0].Active;
                foreach (Act a in mContext.Activity.Acts)
                {
                    a.Active = activeValue;
                }
            }
        }

        private void TakeUntakeSSHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            if (mContext.Activity.Acts.Count > 0)
            {
                bool takeValue = !((Act)mContext.Activity.Acts[0]).TakeScreenShot;//decide if to take or not
                foreach (Act a in mContext.Activity.Acts)
                {
                    a.TakeScreenShot = takeValue;
                }
            }
        }

        private void DeleteHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            if (Reporter.ToUser(eUserMsgKey.SureWantToDelete, mAction.Description) == eUserMsgSelection.Yes)
            {
                mContext.Activity.Acts.Remove(mAction);
            }
        }

        private void MoveUpHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            int index = mContext.Activity.Acts.IndexOf(mAction);
            if (index > 0)
            {
                //move
                ExpandItemOnLoad = true;
                mContext.Activity.Acts.Move(index, index - 1);
            }
        }

        private void MoveDownHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);

            int index = mContext.Activity.Acts.IndexOf(mAction);
            if (index < mContext.Activity.Acts.Count-1)
            {
                //move
                ExpandItemOnLoad = true;
                mContext.Activity.Acts.Move(index, index + 1);
            }
        }

        private void BreakPointHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mAction.BreakPoint = !mAction.BreakPoint;
        }

        private void ResetHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mAction.Reset();
        }

        private void ResetResetHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);

            //reset current Activity
            mContext.Activity.Elapsed = null;
            mContext.Activity.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
            for (int indx = mContext.Activity.Acts.IndexOf(mAction); indx < mContext.Activity.Acts.Count; indx++)
            {
                ((Act)mContext.Activity.Acts[indx]).Reset();
            }

            //reset next Activities
            for (int indx = mContext.BusinessFlow.Activities.IndexOf(mContext.Activity) + 1; indx < mContext.BusinessFlow.Activities.Count; indx++)
            {
                mContext.BusinessFlow.Activities[indx].Reset();
            }
        }

        private void RunHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            App.OnAutomateBusinessFlowEvent(BusinessFlowWindows.AutomateEventArgs.eEventType.RunCurrentActionAndMoveOn, new Tuple<Activity, Act, bool>(mContext.Activity, (Act)mAction, false));
        }

        private void ContinueRunHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            App.OnAutomateBusinessFlowEvent(BusinessFlowWindows.AutomateEventArgs.eEventType.ContinueActionRun, new Tuple<Activity, Act>(mContext.Activity, (Act)mAction));
        }

        private void AddToSRHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            //get target application for the action
            if (mAction is ActWithoutDriver)
            {
                mAction.Platform = ePlatformType.NA;
            }
            else
            {
                mAction.Platform = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms where x.AppName == mContext.Activity.TargetApplication select x.Platform).FirstOrDefault();
            }
            WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(mContext, mAction));
        }


        private void AddSelectedToSRHandler(object sender, RoutedEventArgs e)
        {            
            List<RepositoryItemBase> list = new List<RepositoryItemBase>();
            List<object> SelectedItemsList = ListView.List.SelectedItems.Cast<object>().ToList();
            foreach (Act act in SelectedItemsList)
            {
                list.Add(act);
                //get target application for the action
                if (mAction is ActWithoutDriver)
                {
                    mAction.Platform = ePlatformType.NA;
                }
                else
                {
                    mAction.Platform = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms where x.AppName == mContext.Activity.TargetApplication select x.Platform).FirstOrDefault();
                }
            }
            WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(mContext, list));
        }

        private void CopyAllListHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            foreach (Act act in mContext.Activity.Acts)
            {
                list.Add(act);
            }
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutAllListHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            foreach (Act act in mContext.Activity.Acts)
            {
                list.Add(act);
            }
            ClipboardOperationsHandler.SetCutItems(ListView, list);
        }

        private void CopySelectedHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            foreach (Act act in ListView.List.SelectedItems)
            {
                list.Add(act);
            }
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutSelectedHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            foreach (Act act in ListView.List.SelectedItems)
            {
                list.Add(act);
            }
            ClipboardOperationsHandler.SetCutItems(ListView, list);
        }

        private void PasteInListHandler(object sender, RoutedEventArgs e)
        {
            ClipboardOperationsHandler.PasteItems(ListView, null, -1, mContext);
        }

        private void CopyHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            list.Add(mAction);
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            list.Add(mAction);
            ClipboardOperationsHandler.SetCutItems(ListView, list);
        }

        private void PasteAfterCurrentHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            ClipboardOperationsHandler.PasteItems(ListView, null, currentIndex: mContext.Activity.Acts.IndexOf(mAction), mContext);
        }

        public void CopySelected()
        {
            CopySelectedHandler(null, null);
        }

        public void CutSelected()
        {
            if (PageViewMode == General.eRIPageViewMode.Automation || PageViewMode == General.eRIPageViewMode.SharedReposiotry ||
                PageViewMode == General.eRIPageViewMode.Child || PageViewMode == General.eRIPageViewMode.ChildWithSave ||
                   PageViewMode == General.eRIPageViewMode.Standalone)
            {
                CutSelectedHandler(null, null);
            }
        }

        public void Paste()
        {
            if (PageViewMode == General.eRIPageViewMode.Automation || PageViewMode == General.eRIPageViewMode.SharedReposiotry ||
                PageViewMode == General.eRIPageViewMode.Child || PageViewMode == General.eRIPageViewMode.ChildWithSave ||
                   PageViewMode == General.eRIPageViewMode.Standalone)
            {
                PasteInListHandler(null, null);
            }
        }

        public void DeleteSelected()
        {
            if (PageViewMode == General.eRIPageViewMode.Automation || PageViewMode == General.eRIPageViewMode.SharedReposiotry ||
                PageViewMode == General.eRIPageViewMode.Child || PageViewMode == General.eRIPageViewMode.ChildWithSave ||
                   PageViewMode == General.eRIPageViewMode.Standalone)
            {
                DeleteSelectedHandler(null, null);
            }
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

    public class ActiveImageTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == false)
            {
                return Amdocs.Ginger.Common.Enums.eImageType.InActive;
            }
            else
            {
                return Amdocs.Ginger.Common.Enums.eImageType.Active;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ActionListItemEventArgs
    {
        public enum eEventType
        {
            ShowActionEditPage,
        }

        public eEventType EventType;
        public Object EventObject;

        public ActionListItemEventArgs(eEventType eventType, object eventObject = null)
        {
            this.EventType = eventType;
            this.EventObject = eventObject;
        }
    }
}
