#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Ginger.Repository;
using Ginger.Repository.AddItemToRepositoryWizard;
using Ginger.SolutionGeneral;
using Ginger.UserControlsLib;
using Ginger.UserControlsLib.UCListView;
using Ginger.Variables;
using GingerCore;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using GingerCore.Variables;
using GingerWPF.WizardLib;
using Microsoft.VisualStudio.Services.Common;
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


        public string TitleName
        {
            get
            {
                return (VariablesParent is not null and EnvApplication) ? GingerDicser.GetTermResValue(eTermResKey.Parameter) : GingerDicser.GetTermResValue(eTermResKey.Variables);
            }
        }
        public delegate void VariabelListItemEventHandler(ListItemEventArgs EventArgs);
        public event VariabelListItemEventHandler VariabelListItemEvent;
        private void OnActionListItemEvent(ListItemEventArgs.eEventType eventType, Object eventObject = null)
        {
            VariabelListItemEventHandler handler = VariabelListItemEvent;
            if (handler != null)
            {
                handler(new ListItemEventArgs(eventType, eventObject));
            }
        }

        public bool AllowExpandItems { get; set; } = true;

        public bool ExpandItemOnLoad { get; set; } = false;

        public bool ShowIndex
        {
            get
            {
                return false;

            }
        }

        public VariablesListViewHelper(ObservableList<VariableBase> variables, RepositoryItemBase variablesParent, eVariablesLevel variablesLevel, Context context, General.eRIPageViewMode pageViewMode)
        {
            Variables = variables;
            VariablesParent = variablesParent;
            VariablesLevel = variablesLevel;
            mContext = context;
            PageViewMode = pageViewMode;
        }

        public void UpdatePageViewMode(Ginger.General.eRIPageViewMode pageViewMode)
        {
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

        public string GetItemMandatoryField()
        {
            return nameof(VariableBase.MandatoryInput);
        }

        public string GetItemDescriptionField()
        {
            return nameof(VariableBase.Description);
        }

        public string GetItemErrorField()
        {
            return null;
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
            List<ListItemOperation> operationsList = [];

            if (VariablesParent.GOpsFlag)
            {
                return operationsList;
            }

            ListItemOperation addNew = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "addNew",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Add,
                ToolTip = "Add New " + TitleName,
                OperationHandler = AddNewHandler
            };
            operationsList.Add(addNew);

            ListItemOperation deleteSelected = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "deleteSelected",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete,
                ToolTip = "Delete Selected " + TitleName + " (Del)",
                OperationHandler = DeleteSelectedHandler
            };
            operationsList.Add(deleteSelected);

            return operationsList;
        }

        public List<ListItemOperation> GetListExtraOperations()
        {
            List<ListItemOperation> extraOperationsList = [];

            if (VariablesParent.GOpsFlag)
            {
                return extraOperationsList;
            }

            ListItemOperation resetAll = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation],
                AutomationID = "resetAll",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset,
                Header = "Reset All " + TitleName,
                ToolTip = "Reset All " + TitleName,
                OperationHandler = ResetAllHandler
            };
            extraOperationsList.Add(resetAll);

            ListItemOperation copyAllList = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.View, General.eRIPageViewMode.ViewAndExecute, General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "copyAllList",
                Group = "Clipboard",
                GroupImageType = Amdocs.Ginger.Common.Enums.eImageType.Clipboard,
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Copy,
                Header = "Copy All List Items",
                OperationHandler = CopyAllListHandler
            };
            extraOperationsList.Add(copyAllList);

            ListItemOperation cutAllList = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "cutAllList",
                Group = "Clipboard",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Cut,
                Header = "Cut All List Items",
                OperationHandler = CutAllListHandler
            };
            extraOperationsList.Add(cutAllList);

            ListItemOperation copySelected = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.View, General.eRIPageViewMode.ViewAndExecute, General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "copySelected",
                Group = "Clipboard",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Copy,
                Header = "Copy Selected Items (Ctrl+C)",
                OperationHandler = CopySelectedHandler
            };
            extraOperationsList.Add(copySelected);

            ListItemOperation cutSelected = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "cutSelected",
                Group = "Clipboard",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Cut,
                Header = "Cut Selected Items (Ctrl+X)",
                OperationHandler = CutSelectedHandler
            };
            extraOperationsList.Add(cutSelected);

            ListItemOperation pasteInList = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "pasteInList",
                Group = "Clipboard",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Paste,
                Header = "Paste (Ctrl+V)",
                OperationHandler = PasteInListHandler
            };
            extraOperationsList.Add(pasteInList);

            ListItemOperation deleteAll = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "deleteAll",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete,
                Header = "Delete All " + TitleName,
                OperationHandler = DeleteAllHandler
            };
            extraOperationsList.Add(deleteAll);

            if (VariablesParent is not EnvApplication)
            {
                ListItemOperation addSelectedToSR = new ListItemOperation
                {
                    SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                    AutomationID = "addSelectedToSR",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem,
                    Header = "Add Selected to Shared Repository",
                    OperationHandler = AddSelectedToSRHandler
                };
                extraOperationsList.Add(addSelectedToSR);
            }

            if (VariablesParent is EnvApplication)
            {
                ListItemOperation addSelectedToSR = new ListItemOperation
                {
                    SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                    AutomationID = "shareSelectedToOtherEnv",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.Share,
                    Header = "Add Selected Parameter to Other Environment",
                    OperationHandler = AddSelectedToOtherEnv
                };
                extraOperationsList.Add(addSelectedToSR);
            }

            //if(VariablesParent.GetType() == typeof(BusinessFlow))
            //{
            ListItemOperation inputvariablesRules = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.Child],
                AutomationID = "inputvrules",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Rules,
                Header = "Input " + TitleName + " Rules",
                ToolTip = "Input " + TitleName + " Rules",
                OperationHandler = InputVariablesRuleHandler
            };
            extraOperationsList.Add(inputvariablesRules);
            //}

            return extraOperationsList;
        }

        private void AddSelectedToOtherEnv(object sender, RoutedEventArgs e)
        {
            var SelectedVariables = ListView.List.SelectedItems.Cast<VariableBase>().ToList();

            ObservableList<ProjEnvironment> ProjEnvironments = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>();

            foreach (var varToAdd in SelectedVariables)
            {
                ProjEnvironments.ForEach((projEnv) =>
                {

                    projEnv.Applications.Where((envApp) =>
                    {
                        return envApp.Name.Equals(((EnvApplication)VariablesParent).Name) && !envApp.Variables.Any((var) => var.Name.Equals(varToAdd.Name));
                    })
                    .ForEach((filteredApp) =>
                    {

                        filteredApp.Variables.Add((VariableBase)varToAdd.CreateCopy());
                    });
                });
            }

        }

        public List<ListItemNotification> GetItemNotificationsList(object item)
        {
            SetItem(item);
            List<ListItemNotification> notificationsList = [];

            if (PageViewMode != General.eRIPageViewMode.Add)
            {
                ListItemNotification inputInd = new ListItemNotification
                {
                    AutomationID = "inputInd",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.Input,
                    ToolTip = "Input " + GingerDicser.GetTermResValue(eTermResKey.Variable),
                    BindingObject = mVariable,
                    BindingFieldName = nameof(VariableBase.SetAsInputValue),
                    BindingConverter = new BoolVisibilityConverter()
                };
                notificationsList.Add(inputInd);

                ListItemNotification outputInd = new ListItemNotification
                {
                    AutomationID = "outputInd",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.Output,
                    ToolTip = "Output " + GingerDicser.GetTermResValue(eTermResKey.Variable),
                    BindingObject = mVariable,
                    BindingFieldName = nameof(VariableBase.SetAsOutputValue),
                    BindingConverter = new BoolVisibilityConverter()
                };
                notificationsList.Add(outputInd);

                ListItemNotification linkedInd = new ListItemNotification
                {
                    AutomationID = "linkedInd",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.Link,
                    ToolTip = string.Format("{0} is linked to other {0}", GingerDicser.GetTermResValue(eTermResKey.Variable)),
                    BindingObject = mVariable,
                    BindingFieldName = nameof(VariableBase.LinkedVariableName),
                    BindingConverter = new StringVisibilityConverter()
                };
                notificationsList.Add(linkedInd);

                ListItemNotification publishInd = new ListItemNotification
                {
                    AutomationID = "publishInd",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.Share,
                    ToolTip = string.Format("{0} is marked to be Published to third party applications", GingerDicser.GetTermResValue(eTermResKey.Variable)),
                    BindingObject = mVariable,
                    BindingFieldName = nameof(RepositoryItemBase.Publish),
                    BindingConverter = new BoolVisibilityConverter()
                };
                notificationsList.Add(publishInd);

                ListItemNotification sharedRepoInd = new ListItemNotification
                {
                    AutomationID = "sharedRepoInd",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem,
                    ToolTip = string.Format("{0} source is from Shared Repository", GingerDicser.GetTermResValue(eTermResKey.Variable)),
                    ImageForeground = Brushes.Orange,
                    BindingObject = mVariable,
                    BindingFieldName = nameof(VariableBase.IsSharedRepositoryInstance),
                    BindingConverter = new BoolVisibilityConverter()
                };
                notificationsList.Add(sharedRepoInd);
            }

            return notificationsList;
        }

        public List<ListItemOperation> GetItemOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> operationsList = [];

            ListItemOperation edit = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "edit",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit,
                ToolTip = "Edit " + TitleName,
                OperationHandler = EditHandler
            };
            operationsList.Add(edit);

            ListItemOperation moveUp = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "moveUp",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveUp,
                ToolTip = "Move Up",
                OperationHandler = MoveUpHandler
            };
            operationsList.Add(moveUp);

            ListItemOperation moveDown = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "moveDown",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveDown,
                ToolTip = "Move Down",
                OperationHandler = MoveDownHandler
            };
            operationsList.Add(moveDown);

            ListItemOperation delete = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "delete",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete,
                ToolTip = "Delete",
                OperationHandler = DeleteHandler
            };
            operationsList.Add(delete);

            ListItemOperation itemUsage = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.AddFromShardRepository],
                AutomationID = "itemUsage",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.InstanceLink,
                ToolTip = string.Format("View Repository {0} Usage", GingerDicser.GetTermResValue(eTermResKey.Variable)),
                OperationHandler = ViewRepositoryItemUsageHandler
            };
            operationsList.Add(itemUsage);

            //removing for gingerops
            if (item is VariableBase vb && vb.GOpsFlag)
            {
                operationsList.Remove(edit);
                operationsList.Remove(delete);
            }

            return operationsList;
        }

        public List<ListItemOperation> GetItemExtraOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> extraOperationsList = [];

            if (item is VariableBase vb && vb.GOpsFlag)
            {
                return extraOperationsList;
            }

            if (mVariable.SupportResetValue)
            {
                ListItemOperation reset = new ListItemOperation
                {
                    SupportedViews = [General.eRIPageViewMode.Automation],
                    AutomationID = "reset",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset,
                    Header = "Reset",
                    ToolTip = "Reset",
                    OperationHandler = ResetHandler
                };
                extraOperationsList.Add(reset);
            }

            if (mVariable.SupportAutoValue)
            {
                ListItemOperation autoValue = new ListItemOperation
                {
                    SupportedViews = [General.eRIPageViewMode.Automation],
                    AutomationID = "autoValue",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.Action,
                    Header = "Generate Auto Value",
                    ToolTip = "Generate Auto Value",
                    OperationHandler = AutoValueHandler
                };
                extraOperationsList.Add(autoValue);
            }

            if (VariablesParent is not EnvApplication)
            {

                ListItemOperation input = new ListItemOperation
                {
                    SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                    AutomationID = "input",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active,
                    ImageBindingObject = mVariable,
                    ImageBindingFieldName = nameof(VariableBase.SetAsInputValue),
                    ImageBindingConverter = new ActiveImageTypeConverter(),
                    Header = "Set as Input",
                    ToolTip = "Set as Input",
                    OperationHandler = InputHandler
                };
                extraOperationsList.Add(input);

                ListItemOperation output = new ListItemOperation
                {
                    SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                    AutomationID = "output",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active,
                    ImageBindingObject = mVariable,
                    ImageBindingFieldName = nameof(VariableBase.SetAsOutputValue),
                    ImageBindingConverter = new ActiveImageTypeConverter(),
                    Header = "Set as Output",
                    ToolTip = "Set as Output",
                    OperationHandler = OutputHandler
                };
                extraOperationsList.Add(output);

                ListItemOperation publish = new ListItemOperation
                {
                    SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                    AutomationID = "publish",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active,
                    ImageBindingObject = mVariable,
                    ImageBindingFieldName = nameof(RepositoryItemBase.Publish),
                    ImageBindingConverter = new ActiveImageTypeConverter(),
                    Header = "Publish",
                    ToolTip = "Publish to third party applications",
                    OperationHandler = PublishHandler
                };
                extraOperationsList.Add(publish);

                ListItemOperation addToSR = new ListItemOperation
                {
                    SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                    AutomationID = "addToSR",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem,
                    Header = "Add to Shared Repository",
                    ToolTip = "Add to Shared Repository",
                    OperationHandler = AddToSRHandler
                };
                extraOperationsList.Add(addToSR);
            }

            ListItemOperation copy = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.View, General.eRIPageViewMode.ViewAndExecute, General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "copy",
                Group = "Clipboard",
                GroupImageType = Amdocs.Ginger.Common.Enums.eImageType.Clipboard,
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Copy,
                Header = "Copy (Ctrl+C)",
                OperationHandler = CopyHandler
            };
            extraOperationsList.Add(copy);

            ListItemOperation cut = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "cut",
                Group = "Clipboard",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Cut,
                Header = "Cut (Ctrl+X)",
                OperationHandler = CutHandler
            };
            extraOperationsList.Add(cut);

            ListItemOperation pasterAfterCurrent = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "pasterAfterCurrent",
                Group = "Clipboard",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Paste,
                Header = "Paste (Ctrl+V)",
                OperationHandler = PasteAfterCurrentHandler
            };
            extraOperationsList.Add(pasterAfterCurrent);

            if (VariablesParent is EnvApplication)
            {
                ListItemOperation addSelectedToSR = new ListItemOperation
                {
                    SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                    AutomationID = "shareSelectedToOtherEnv",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.Share,
                    Header = "Add Selected Parameter to Other Environment",
                    OperationHandler = AddSelectedToOtherEnv
                };
                extraOperationsList.Add(addSelectedToSR);
            }

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

        private void InputVariablesRuleHandler(object sender, RoutedEventArgs e)
        {
            InputVariablesRules inputVariableRule = new InputVariablesRules(mContext.BusinessFlow);
            inputVariableRule.ShowAsWindow();
        }

        private void AddNewHandler(object sender, RoutedEventArgs e)
        {
            AddVariablePage addVarPage = new AddVariablePage(VariablesLevel, VariablesParent, mContext);
            addVarPage.xLibraryTabListView.ListSelectionMode = SelectionMode.Single;
            addVarPage.xLibraryTabListView.SelectTitleVisibility = Visibility.Visible;
            addVarPage.xLibraryTabListView.SelectTitleFontWeight = FontWeight.FromOpenTypeWeight(400);
            if (VariablesLevel.Equals(eVariablesLevel.EnvApplication))
            {
                addVarPage.xLibraryTabListView.SelectTitleContent = "Select Parameter Type";
            }
            else
            {
                addVarPage.xLibraryTabListView.SelectTitleContent = "Select Variable Type";
            }
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

            if (Reporter.ToUser(eUserMsgKey.SureWantToDeleteSelectedItems, TitleName, ((VariableBase)ListView.List.SelectedItems[0]).Name) == eUserMsgSelection.Yes)
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
            switch (VariablesLevel)
            {
                case eVariablesLevel.Solution:
                    ((Solution)VariablesParent).SolutionOperations.SaveSolution(true, Solution.eSolutionItemToSave.GlobalVariabels);
                    break;
            }
        }

        private void ResetAllHandler(object sender, RoutedEventArgs e)
        {
            foreach (VariableBase var in Variables)
            {
                var.ResetValue();
            }
        }

        private void EditHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            OnActionListItemEvent(ListItemEventArgs.eEventType.ShowEditPage, mVariable);
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
            string errorMsg = string.Empty;
            mVariable.GenerateAutoValue(ref errorMsg);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                Reporter.ToUser(eUserMsgKey.VariablesAssignError, errorMsg);
            }
        }

        private void InputHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mVariable.SetAsInputValue = !mVariable.SetAsInputValue;
        }

        private void PublishHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mVariable.Publish = !mVariable.Publish;
        }

        private void OutputHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mVariable.SetAsOutputValue = !mVariable.SetAsOutputValue;
        }

        private void AddToSRHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(mContext, mVariable));

        }

        private void AddSelectedToSRHandler(object sender, RoutedEventArgs e)
        {
            List<RepositoryItemBase> list = [];
            List<object> SelectedItemsList = ListView.List.SelectedItems.Cast<object>().ToList();
            foreach (VariableBase var in SelectedItemsList)
            {
                list.Add(var);
            }

            WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(mContext, list));

        }

        private void CopyAllListHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = [.. Variables];
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutAllListHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = [.. Variables];
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
            ObservableList<RepositoryItemBase> list = [mVariable];
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            ObservableList<RepositoryItemBase> list = [mVariable];
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

        public void CopySelected()
        {
            CopySelectedHandler(null, null);
        }

        public void CutSelected()
        {
            if (PageViewMode is General.eRIPageViewMode.Automation or General.eRIPageViewMode.SharedReposiotry or
                General.eRIPageViewMode.Child or General.eRIPageViewMode.ChildWithSave or
                   General.eRIPageViewMode.Standalone)
            {
                CutSelectedHandler(null, null);
            }
        }

        public void Paste()
        {
            if (PageViewMode is General.eRIPageViewMode.Automation or General.eRIPageViewMode.SharedReposiotry or
                General.eRIPageViewMode.Child or General.eRIPageViewMode.ChildWithSave or
                   General.eRIPageViewMode.Standalone)
            {
                PasteInListHandler(null, null);
            }
        }

        public void DeleteSelected()
        {
            if (PageViewMode is General.eRIPageViewMode.Automation or General.eRIPageViewMode.SharedReposiotry or
                General.eRIPageViewMode.Child or General.eRIPageViewMode.ChildWithSave or
                   General.eRIPageViewMode.Standalone)
            {
                DeleteSelectedHandler(null, null);
            }
        }
    }

    public class ListItemEventArgs
    {
        public enum eEventType
        {
            ShowEditPage,
        }

        public eEventType EventType;
        public Object EventObject;

        public ListItemEventArgs(eEventType eventType, object eventObject = null)
        {
            this.EventType = eventType;
            this.EventObject = eventObject;
        }
    }
}
