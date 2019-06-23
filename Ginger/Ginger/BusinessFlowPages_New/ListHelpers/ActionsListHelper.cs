using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.UserControlsLib.UCListView;
using Ginger.Variables;
using GingerCore;
using GingerCore.Actions;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Ginger.BusinessFlowPages.ListHelpers
{
    public class ActionsListHelper : IListViewHelper
    {
        Act mAction;
        Context mContext;
        General.eRIPageViewMode mPageViewMode;

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

        public ActionsListHelper(Context context, General.eRIPageViewMode pageViewMode)
        {
            mContext = context;
            mPageViewMode = pageViewMode;
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
            return nameof(Act.ActionType);
        }

        public string GetItemExecutionStatusField()
        {
            return nameof(Act.Status);
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

            if (mPageViewMode != General.eRIPageViewMode.View)
            {
                ListItemOperation deleteAll = new ListItemOperation();
                deleteAll.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
                deleteAll.ToolTip = "Delete All Actions";
                deleteAll.OperationHandler = DeleteAllHandler;
                operationsList.Add(deleteAll);
            }

            return operationsList;
        }

        public List<ListItemOperation> GetListExtraOperations()
        {
            List<ListItemOperation> extraOperationsList = new List<ListItemOperation>();

            if (mPageViewMode != General.eRIPageViewMode.View)
            {
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
            }

            return extraOperationsList;
        }

        public List<ListItemNotification> GetItemNotificationsList(object item)
        {
            SetItem(item);
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

        public List<ListItemOperation> GetItemOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> operationsList = new List<ListItemOperation>();

            if (mPageViewMode != General.eRIPageViewMode.View)
            {
                ListItemOperation edit = new ListItemOperation();
                edit.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit;
                edit.ToolTip = "Edit Action";
                edit.OperationHandler = EditHandler;
                operationsList.Add(edit);

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
                active.ImageBindingObject = mAction;
                active.ImageBindingFieldName = nameof(Act.Active);
                active.ImageBindingConverter = new ActiveImageTypeConverter();
                active.ToolTip = "Active";
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
                ListItemOperation breakPoint = new ListItemOperation();
                breakPoint.Header = "Break Point";
                breakPoint.ToolTip = "Stop execution on that Action";
                breakPoint.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active;
                breakPoint.ImageBindingObject = mAction;
                breakPoint.ImageBindingFieldName = nameof(Act.BreakPoint);
                breakPoint.ImageBindingConverter = new ActiveImageTypeConverter();
                breakPoint.OperationHandler = BreakPointHandler;
                extraOperationsList.Add(breakPoint);

                ListItemOperation reset = new ListItemOperation();
                reset.Group = "Reset Operations";
                reset.GroupImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset;
                reset.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset;
                reset.Header = "Reset Action execution details";
                reset.ToolTip = "Reset Action execution details";
                reset.OperationHandler = ResetHandler;
                extraOperationsList.Add(reset);

                ListItemOperation resetRest = new ListItemOperation();
                resetRest.Group = "Reset Operations";
                resetRest.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset;
                resetRest.Header = "Reset execution details from this Action";
                resetRest.ToolTip = "Reset execution details from this Action";
                reset.OperationHandler = ResetResetHandler;
                extraOperationsList.Add(resetRest);
            }

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

            if (mPageViewMode != General.eRIPageViewMode.View)
            {
                ListItemOperation run = new ListItemOperation();
                run.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Run;
                run.ToolTip = "Run Action";
                run.OperationHandler = RunHandler;
                executionOperationsList.Add(run);

                ListItemOperation continueRun = new ListItemOperation();
                continueRun.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Continue;
                continueRun.ToolTip = "Continue Run from Action";
                continueRun.OperationHandler = ContinueRunHandler;
                executionOperationsList.Add(continueRun);
            }

            return executionOperationsList;
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
        private void EditHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mAction.SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper();
            mAction.Context = mContext;
            //ActionEditPage actedit = new ActionEditPage(mAction, General.RepositoryItemPageViewMode.Automation);//TODO: check if need diifrent mode
            //actedit.ap = this;
            //actedit.ShowAsWindow();
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
            for (int indx = mContext.Activity.Acts.IndexOf(mAction); indx <= mContext.Activity.Acts.Count; indx++)
            {
                ((Act)mContext.Activity.Acts[indx]).Reset();
            }

            //reset next Activities
            for (int indx = mContext.BusinessFlow.Activities.IndexOf(mContext.Activity) + 1; indx <= mContext.BusinessFlow.Activities.Count; indx++)
            {
                mContext.BusinessFlow.Activities[indx].Reset();
            }
        }

        private void RunHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            App.OnAutomateBusinessFlowEvent(BusinessFlowWindows.AutomateEventArgs.eEventType.RunCurrentAction, null);
        }

        private void ContinueRunHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            App.OnAutomateBusinessFlowEvent(BusinessFlowWindows.AutomateEventArgs.eEventType.ContinueActionRun, null);
        }

        private void AddToSRHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            List<RepositoryItemBase> list = new List<RepositoryItemBase>();
            list.Add(mAction);
            (new Repository.SharedRepositoryOperations()).AddItemsToRepository(mContext, list);
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
