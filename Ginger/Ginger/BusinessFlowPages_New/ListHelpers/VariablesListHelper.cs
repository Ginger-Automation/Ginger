using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.SolutionGeneral;
using Ginger.UserControlsLib.UCListView;
using Ginger.Variables;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ginger.BusinessFlowPages.ListHelpers
{
    public class VariablesListHelper : IListViewHelper
    {
        public eVariablesLevel VariablesLevel;
        public RepositoryItemBase VariablesParent;
        public ObservableList<VariableBase> Variables;

        VariableBase mVariable;
        Context mContext;
        General.eRIPageViewMode mPageViewMode;

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

        public VariablesListHelper(ObservableList<VariableBase> variables, RepositoryItemBase variablesParent, eVariablesLevel variablesLevel, Context context, General.eRIPageViewMode pageViewMode)
        {
            Variables = variables;
            VariablesParent = variablesParent;
            VariablesLevel = variablesLevel;
            mContext = context;
            mPageViewMode = pageViewMode;
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
            else if (item is MenuItem)
            {
                mVariable = (VariableBase)(((MenuItem)item).Tag);
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

        public string GetItemNameExtentionField()
        {
            return nameof(VariableBase.Value);
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

            if (mPageViewMode != General.eRIPageViewMode.View && mPageViewMode != General.eRIPageViewMode.Add)
            {
                ListItemOperation addNew = new ListItemOperation();
                addNew.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Add;
                addNew.ToolTip = "Add New " + GingerDicser.GetTermResValue(eTermResKey.Variable);
                addNew.OperationHandler = AddNewHandler;
                operationsList.Add(addNew);

                ListItemOperation deleteAll = new ListItemOperation();
                deleteAll.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
                deleteAll.ToolTip = "Delete All " + GingerDicser.GetTermResValue(eTermResKey.Variables);
                deleteAll.OperationHandler = DeleteAllHandler;
                operationsList.Add(deleteAll);
            }

            if (mPageViewMode == General.eRIPageViewMode.Standalone)
            {
                ListItemOperation save = new ListItemOperation();
                save.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Save;
                save.ToolTip = "Save All Changes";
                save.OperationHandler = SaveAllHandler;
                operationsList.Add(save);
            }

            return operationsList;
        }

        public List<ListItemOperation> GetListExtraOperations()
        {
            List<ListItemOperation> extraOperationsList = new List<ListItemOperation>();

            if (mPageViewMode != General.eRIPageViewMode.View && mPageViewMode != General.eRIPageViewMode.Add)
            {
                ListItemOperation resetAll = new ListItemOperation();
                resetAll.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset;
                resetAll.Header = "Reset All " + GingerDicser.GetTermResValue(eTermResKey.Variables);
                resetAll.ToolTip = "Reset All " + GingerDicser.GetTermResValue(eTermResKey.Variables);
                resetAll.OperationHandler = ResetAllHandler;
                extraOperationsList.Add(resetAll);
            }

            return extraOperationsList;
        }

        public List<ListItemNotification> GetItemNotificationsList(object item)
        {
            SetItem(item);
            List<ListItemNotification> notificationsList = new List<ListItemNotification>();

            ListItemNotification inputInd = new ListItemNotification();
            inputInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Input;
            inputInd.ToolTip = "Input " + GingerDicser.GetTermResValue(eTermResKey.Variable);
            inputInd.BindingObject = mVariable;
            inputInd.BindingFieldName = nameof(VariableBase.SetAsInputValue);
            inputInd.BindingConverter = new BoolVisibilityConverter();
            notificationsList.Add(inputInd);

            ListItemNotification outputInd = new ListItemNotification();
            outputInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Output;
            outputInd.ToolTip = "Output " + GingerDicser.GetTermResValue(eTermResKey.Variable);
            outputInd.BindingObject = mVariable;
            outputInd.BindingFieldName = nameof(VariableBase.SetAsOutputValue);
            outputInd.BindingConverter = new BoolVisibilityConverter();
            notificationsList.Add(outputInd);

            ListItemNotification linkedInd = new ListItemNotification();
            linkedInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Link;
            linkedInd.ToolTip = string.Format("{0} is linked to other {0}", GingerDicser.GetTermResValue(eTermResKey.Variable));
            linkedInd.BindingObject = mVariable;
            linkedInd.BindingFieldName = nameof(VariableBase.LinkedVariableName);
            linkedInd.BindingConverter = new StringVisibilityConverter();
            notificationsList.Add(linkedInd);

            ListItemNotification sharedRepoInd = new ListItemNotification();
            sharedRepoInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem;            
            linkedInd.ToolTip = string.Format("{0} source is from Shared Repository", GingerDicser.GetTermResValue(eTermResKey.Variable));
            sharedRepoInd.ImageForeground = Brushes.Orange;
            sharedRepoInd.BindingObject = mVariable;
            sharedRepoInd.BindingFieldName = nameof(VariableBase.IsSharedRepositoryInstance);
            sharedRepoInd.BindingConverter = new BoolVisibilityConverter();
            notificationsList.Add(sharedRepoInd);

            return notificationsList;
        }

        public List<ListItemOperation> GetItemOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> operationsList = new List<ListItemOperation>();

            if (mPageViewMode != General.eRIPageViewMode.View && mPageViewMode != General.eRIPageViewMode.Add)
            {
                ListItemOperation edit = new ListItemOperation();
                edit.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit;
                edit.ToolTip = "Edit " + GingerDicser.GetTermResValue(eTermResKey.Variable);
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
            }

            return operationsList;
        }

        public List<ListItemOperation> GetItemExtraOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> extraOperationsList = new List<ListItemOperation>();

            if (mPageViewMode != General.eRIPageViewMode.View && mPageViewMode != General.eRIPageViewMode.Add)
            {
                ListItemOperation reset = new ListItemOperation();
                reset.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset;
                reset.Header = "Reset";
                reset.ToolTip = "Reset";
                reset.OperationHandler = ResetHandler;
                extraOperationsList.Add(reset);

                ListItemOperation autoValue = new ListItemOperation();
                autoValue.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Action;
                autoValue.Header = "Generate Auto Value";
                autoValue.ToolTip = "Generate Auto Value";
                autoValue.OperationHandler = AutoValueHandler;
                extraOperationsList.Add(autoValue);

                ListItemOperation input = new ListItemOperation();
                input.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active;
                input.ImageBindingObject = mVariable;
                input.ImageBindingFieldName = nameof(VariableBase.SetAsInputValue);
                input.ImageBindingConverter = new ActiveImageTypeConverter();
                input.Header = "Set as Input";
                input.ToolTip = "Set as Input";
                input.OperationHandler = InputHandler;
                extraOperationsList.Add(input);

                ListItemOperation output = new ListItemOperation();
                output.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active;
                output.ImageBindingObject = mVariable;
                output.ImageBindingFieldName = nameof(VariableBase.SetAsOutputValue);
                output.ImageBindingConverter = new ActiveImageTypeConverter();
                output.Header = "Set as Output";
                output.ToolTip = "Set as Output";
                output.OperationHandler = OutputHandler;
                extraOperationsList.Add(output);
            }

            if (mPageViewMode != General.eRIPageViewMode.Add)
            {
                ListItemOperation addToSR = new ListItemOperation();
                addToSR.ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem;
                addToSR.Header = "Add to Shared Repository";
                addToSR.ToolTip = "Add to Shared Repository";
                addToSR.OperationHandler = AddToSRHandler;
                extraOperationsList.Add(addToSR);
            }

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

        private void AddNewHandler(object sender, RoutedEventArgs e)
        {
            AddVariablePage addVarPage = new AddVariablePage(VariablesLevel, VariablesParent, mContext);
            addVarPage.ShowAsWindow();
        }

        private void DeleteAllHandler(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.SureWantToDeleteAll) == eUserMsgSelection.Yes)
            {
               if (Variables != null)
                {
                    Variables.Clear();
                }
            }
        }

        private void SaveAllHandler(object sender, RoutedEventArgs e)
        {
            switch(VariablesLevel)
            {
                case eVariablesLevel.Solution:
                    ((Solution)VariablesParent).SaveSolution(true, Solution.eSolutionItemToSave.GlobalVariabels);
                    break;
            }
        }

        private void ResetAllHandler(object sender, RoutedEventArgs e)
        {
            foreach(VariableBase var in Variables)
            {
                var.ResetValue();
            }
        }

        private void EditHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            OnActionListItemEvent(VariabelListItemEventArgs.eEventType.ShowVariabelEditPage, mVariable);
        }

        private void DeleteHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            if (Reporter.ToUser(eUserMsgKey.SureWantToDelete, mVariable.Name) == eUserMsgSelection.Yes)
            {
                Variables.Remove(mVariable);
            }
        }

        private void MoveUpHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            int index = Variables.IndexOf(mVariable);
            if (index > 0)
            {
                //move
                Variables.Move(index, index - 1);
            }
        }

        private void MoveDownHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);

            int index = Variables.IndexOf(mVariable);
            if (index < Variables.Count - 1)
            {
                //move
                Variables.Move(index, index + 1);
            }
        }

        private void ResetHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mVariable.ResetValue();
        }

        private void AutoValueHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mVariable.GenerateAutoValue();
        }

        private void InputHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mVariable.SetAsInputValue = !mVariable.SetAsInputValue;
        }

        private void OutputHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mVariable.SetAsOutputValue = !mVariable.SetAsOutputValue;
        }

        private void AddToSRHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            List<RepositoryItemBase> list = new List<RepositoryItemBase>();
            list.Add(mVariable);
            (new Repository.SharedRepositoryOperations()).AddItemsToRepository(mContext, list);
        }
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
