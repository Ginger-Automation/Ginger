#region License
/*
Copyright © 2014-2021 European Support Limited

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
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
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
    public class ActionsLibraryListViewHelper : IListViewHelper
    {
        Act mAction;
        Context mContext;
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

        public ActionsLibraryListViewHelper(Context context, General.eRIPageViewMode pageViewMode)
        {
            mContext = context;
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
                mAction = (Act)(((ucButton)item).Tag);
            }
            else if (item is MenuItem)
            {
                mAction = (Act)(((MenuItem)item).Tag);
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
            return nameof(Act.ElapsedSecs);
        }

        public string GetItemTagsField()
        {
            return nameof(Act.Tags);
        }

        public string GetItemDescriptionField()
        {
            return nameof(Act.ActionUserDescription);
        }

        public string GetItemErrorField()
        {
            return nameof(Act.Error);
        }

        public string GetItemExecutionStatusField()
        {
            return null;
        }

        public string GetItemActiveField()
        {
            return nameof(Act.Active);
        }

        public ListItemUniqueIdentifier GetItemUniqueIdentifier(object item)
        {
            SetItem(item);
            if (mAction.BreakPoint)
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

            if (PageViewMode != General.eRIPageViewMode.View)
            {
                ListItemOperation addSelected = new ListItemOperation();
                addSelected.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
                addSelected.AutomationID = "addSelected";
                addSelected.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Add;
                addSelected.ToolTip = "Add Selected Actions";
                addSelected.OperationHandler = AddActionListView;
                operationsList.Add(addSelected);
            }

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

        public List<ListItemOperation> GetListExtraOperations()
        {
            List<ListItemOperation> extraOperationsList = new List<ListItemOperation>();
            
            return extraOperationsList;
        }

        public List<ListItemNotification> GetItemNotificationsList(object item)
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

            ListItemNotification sharedRepoInd = new ListItemNotification();
            sharedRepoInd.AutomationID = "sharedRepoInd";
            sharedRepoInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem;
            sharedRepoInd.ToolTip = "Action source is from Shared Repository";
            sharedRepoInd.ImageForeground = Brushes.Orange;
            sharedRepoInd.BindingObject = mAction;
            sharedRepoInd.BindingFieldName = nameof(Act.IsSharedRepositoryInstance);
            sharedRepoInd.BindingConverter = new BoolVisibilityConverter();
            notificationsList.Add(sharedRepoInd);

            return notificationsList;
        }

        public List<ListItemOperation> GetItemOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> operationsList = new List<ListItemOperation>();
            
            return operationsList;
        }

        public List<ListItemOperation> GetItemExtraOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> extraOperationsList = new List<ListItemOperation>();

            return extraOperationsList;
        }

        public List<ListItemOperation> GetItemExecutionOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> executionOperationsList = new List<ListItemOperation>();
            
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



}
