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
using Ginger.Repository;
using Ginger.SolutionGeneral;
using Ginger.UserControlsLib;
using Ginger.UserControlsLib.UCListView;
using Ginger.Variables;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ginger.BusinessFlowPages.ListHelpers
{
    public class VariablesListViewHelper : IListViewHelper
    {
        public eVariablesLevel VariablesLevel;
        public RepositoryItemBase VariablesParent;
        public ObservableList<VariableBase> Variables;

        VariableBase mVariable;
        Context mContext;

        public General.eRIPageViewMode PageViewMode { get; set; }

        public UcListView ListView { get; set; }

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

        public bool AllowExpandItems { get; set; } = true;

        public bool ExpandItemOnLoad { get; set; } = false;

        public VariablesListViewHelper(ObservableList<VariableBase> variables, RepositoryItemBase variablesParent, eVariablesLevel variablesLevel, Context context, General.eRIPageViewMode pageViewMode)
        {
            Variables = variables;
            VariablesParent = variablesParent;
            VariablesLevel = variablesLevel;
            mContext = context;
            PageViewMode = pageViewMode;
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
            return nameof(VariableBase.VariableUIType);
        }

        public List<ListItemOperation> GetListOperations()
        {
            List<ListItemOperation> operationsList = new List<ListItemOperation>();

            ListItemOperation addNew = new ListItemOperation();
            addNew.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            addNew.AutomationID = "addNew";
            addNew.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Add;
            addNew.ToolTip = "Add New " + GingerDicser.GetTermResValue(eTermResKey.Variable);
            addNew.OperationHandler = AddNewHandler;
            operationsList.Add(addNew);

            ListItemOperation deleteSelected = new ListItemOperation();
            deleteSelected.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            deleteSelected.AutomationID = "deleteSelected";
            deleteSelected.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
            deleteSelected.ToolTip = "Delete Selected " + GingerDicser.GetTermResValue(eTermResKey.Variables);
            deleteSelected.OperationHandler = DeleteSelectedHandler;
            operationsList.Add(deleteSelected);

            ListItemOperation save = new ListItemOperation();
            save.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Standalone };
            save.AutomationID = "save";
            save.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Save;
            save.ToolTip = "Save All Changes";
            save.OperationHandler = SaveAllHandler;
            operationsList.Add(save);

            return operationsList;
        }

        public List<ListItemOperation> GetListExtraOperations()
        {
            List<ListItemOperation> extraOperationsList = new List<ListItemOperation>();

            ListItemOperation resetAll = new ListItemOperation();
            resetAll.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation};
            resetAll.AutomationID = "resetAll";
            resetAll.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset;
            resetAll.Header = "Reset All " + GingerDicser.GetTermResValue(eTermResKey.Variables);
            resetAll.ToolTip = "Reset All " + GingerDicser.GetTermResValue(eTermResKey.Variables);
            resetAll.OperationHandler = ResetAllHandler;
            extraOperationsList.Add(resetAll);

            ListItemOperation copyAllList = new ListItemOperation();
            copyAllList.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.View, General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
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
            copySelected.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.View, General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            copySelected.AutomationID = "copySelected";
            copySelected.Group = "Clipboard";
            copySelected.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Copy;
            copySelected.Header = "Copy Selected Items";
            copySelected.OperationHandler = CopySelectedHandler;
            extraOperationsList.Add(copySelected);

            ListItemOperation cutSelected = new ListItemOperation();
            cutSelected.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
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

            ListItemOperation deleteAll = new ListItemOperation();
            deleteAll.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            deleteAll.AutomationID = "deleteAll";
            deleteAll.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
            deleteAll.Header = "Delete All " + GingerDicser.GetTermResValue(eTermResKey.Variables);
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
            SetItem(item);
            List<ListItemNotification> notificationsList = new List<ListItemNotification>();

            if (PageViewMode != General.eRIPageViewMode.Add)
            {
                ListItemNotification inputInd = new ListItemNotification();
                inputInd.AutomationID = "inputInd";
                inputInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Input;
                inputInd.ToolTip = "Input " + GingerDicser.GetTermResValue(eTermResKey.Variable);
                inputInd.BindingObject = mVariable;
                inputInd.BindingFieldName = nameof(VariableBase.SetAsInputValue);
                inputInd.BindingConverter = new BoolVisibilityConverter();
                notificationsList.Add(inputInd);

                ListItemNotification outputInd = new ListItemNotification();
                outputInd.AutomationID = "outputInd";
                outputInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Output;
                outputInd.ToolTip = "Output " + GingerDicser.GetTermResValue(eTermResKey.Variable);
                outputInd.BindingObject = mVariable;
                outputInd.BindingFieldName = nameof(VariableBase.SetAsOutputValue);
                outputInd.BindingConverter = new BoolVisibilityConverter();
                notificationsList.Add(outputInd);

                ListItemNotification linkedInd = new ListItemNotification();
                linkedInd.AutomationID = "linkedInd";
                linkedInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Link;
                linkedInd.ToolTip = string.Format("{0} is linked to other {0}", GingerDicser.GetTermResValue(eTermResKey.Variable));
                linkedInd.BindingObject = mVariable;
                linkedInd.BindingFieldName = nameof(VariableBase.LinkedVariableName);
                linkedInd.BindingConverter = new StringVisibilityConverter();
                notificationsList.Add(linkedInd);

                ListItemNotification sharedRepoInd = new ListItemNotification();
                sharedRepoInd.AutomationID = "sharedRepoInd";
                sharedRepoInd.ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem;
                sharedRepoInd.ToolTip = string.Format("{0} source is from Shared Repository", GingerDicser.GetTermResValue(eTermResKey.Variable));
                sharedRepoInd.ImageForeground = Brushes.Orange;
                sharedRepoInd.BindingObject = mVariable;
                sharedRepoInd.BindingFieldName = nameof(VariableBase.IsSharedRepositoryInstance);
                sharedRepoInd.BindingConverter = new BoolVisibilityConverter();
                notificationsList.Add(sharedRepoInd);
            }

            return notificationsList;
        }

        public List<ListItemOperation> GetItemOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> operationsList = new List<ListItemOperation>();

            ListItemOperation edit = new ListItemOperation();
            edit.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            edit.AutomationID = "edit";
            edit.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit;
            edit.ToolTip = "Edit " + GingerDicser.GetTermResValue(eTermResKey.Variable);
            edit.OperationHandler = EditHandler;
            operationsList.Add(edit);

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

            ListItemOperation itemUsage = new ListItemOperation();
            itemUsage.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.AddFromShardRepository };
            itemUsage.AutomationID = "itemUsage";
            itemUsage.ImageType = Amdocs.Ginger.Common.Enums.eImageType.InstanceLink;
            itemUsage.ToolTip = string.Format("View Repository {0} Usage", GingerDicser.GetTermResValue(eTermResKey.Variable));
            itemUsage.OperationHandler = ViewRepositoryItemUsageHandler;
            operationsList.Add(itemUsage);            

            return operationsList;
        }

        public List<ListItemOperation> GetItemExtraOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> extraOperationsList = new List<ListItemOperation>();

            if (mVariable.SupportResetValue)
            {
                ListItemOperation reset = new ListItemOperation();
                reset.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation};
                reset.AutomationID = "reset";
                reset.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset;
                reset.Header = "Reset";
                reset.ToolTip = "Reset";
                reset.OperationHandler = ResetHandler;
                extraOperationsList.Add(reset);
            }

            if (mVariable.SupportAutoValue)
            {
                ListItemOperation autoValue = new ListItemOperation();
                autoValue.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation };
                autoValue.AutomationID = "autoValue";
                autoValue.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Action;
                autoValue.Header = "Generate Auto Value";
                autoValue.ToolTip = "Generate Auto Value";
                autoValue.OperationHandler = AutoValueHandler;
                extraOperationsList.Add(autoValue);
            }

            ListItemOperation input = new ListItemOperation();
            input.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            input.AutomationID = "input";
            input.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active;
            input.ImageBindingObject = mVariable;
            input.ImageBindingFieldName = nameof(VariableBase.SetAsInputValue);
            input.ImageBindingConverter = new ActiveImageTypeConverter();
            input.Header = "Set as Input";
            input.ToolTip = "Set as Input";
            input.OperationHandler = InputHandler;
            extraOperationsList.Add(input);

            ListItemOperation output = new ListItemOperation();
            output.SupportedViews = new List<General.eRIPageViewMode>() {General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            output.AutomationID = "output";
            output.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active;
            output.ImageBindingObject = mVariable;
            output.ImageBindingFieldName = nameof(VariableBase.SetAsOutputValue);
            output.ImageBindingConverter = new ActiveImageTypeConverter();
            output.Header = "Set as Output";
            output.ToolTip = "Set as Output";
            output.OperationHandler = OutputHandler;
            extraOperationsList.Add(output);

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
            return null;
        }

        public List<ListItemNotification> GetItemGroupNotificationsList(string GroupName)
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

        private void DeleteSelectedHandler(object sender, RoutedEventArgs e)
        {
            if (ListView.List.SelectedItems.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.SelectItemToDelete);
                return;
            }

            if (Reporter.ToUser(eUserMsgKey.SureWantToDeleteSelectedItems) == eUserMsgSelection.Yes)
            {
                List<object> SelectedItemsList = ListView.List.SelectedItems.Cast<object>().ToList();
                foreach (VariableBase var in SelectedItemsList)
                {
                    Variables.Remove(var);
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
                ExpandItemOnLoad = true;
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
                ExpandItemOnLoad = true;
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

        private void AddSelectedToSRHandler(object sender, RoutedEventArgs e)
        {
            List<RepositoryItemBase> list = new List<RepositoryItemBase>();
            List<object> SelectedItemsList = ListView.List.SelectedItems.Cast<object>().ToList();
            foreach (VariableBase var in SelectedItemsList)
            {
                list.Add(var);
            }
            (new Repository.SharedRepositoryOperations()).AddItemsToRepository(mContext, list);
        }

        private void CopyAllListHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            foreach (VariableBase var in Variables)
            {
                list.Add(var);
            }
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutAllListHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            foreach (VariableBase var in Variables)
            {
                list.Add(var);
            }
            ClipboardOperationsHandler.SetCutItems(ListView, list);
        }

        private void CopySelectedHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            foreach (VariableBase var in ListView.List.SelectedItems)
            {
                list.Add(var);
            }
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutSelectedHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            foreach (VariableBase var in ListView.List.SelectedItems)
            {
                list.Add(var);
            }
            ClipboardOperationsHandler.SetCutItems(ListView, list);
        }


        private void PasteInListHandler(object sender, RoutedEventArgs e)
        {
            ClipboardOperationsHandler.PasteItems(ListView);
        }

        private void CopyHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            list.Add(mVariable);
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            list.Add(mVariable);
            ClipboardOperationsHandler.SetCutItems(ListView, list);
        }

        private void PasteAfterCurrentHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            ClipboardOperationsHandler.PasteItems(ListView, currentIndex: Variables.IndexOf(mVariable));
        }

        private void ViewRepositoryItemUsageHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            RepositoryItemUsagePage usagePage = new RepositoryItemUsagePage(mVariable);
            usagePage.ShowAsWindow();
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
