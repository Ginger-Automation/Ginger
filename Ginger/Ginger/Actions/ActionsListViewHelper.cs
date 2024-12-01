#region License
/*
Copyright © 2014-2024 European Support Limited

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

        public bool ShowIndex
        {
            get
            {
                if (PageViewMode is General.eRIPageViewMode.Add or General.eRIPageViewMode.AddFromModel or General.eRIPageViewMode.AddFromShardRepository)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

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
            else if (item is ucButton)
            {
                if (((ucButton)item).Tag is Act)
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
            else if (item is ApplicationPOMModel)
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
            return PageViewMode switch
            {
                General.eRIPageViewMode.Automation or General.eRIPageViewMode.ViewAndExecute => nameof(Act.ElapsedSecs),
                //Add from POM
                General.eRIPageViewMode.AddFromModel => nameof(ElementInfo.ElementTypeEnum),
                _ => null,
            };
        }

        public string GetItemTagsField()
        {
            return nameof(Act.Tags);
        }

        public string GetItemDescriptionField()
        {
            return PageViewMode switch
            {
                General.eRIPageViewMode.Automation or General.eRIPageViewMode.AddFromShardRepository or General.eRIPageViewMode.AddFromModel => nameof(Act.ActionType),
                //Actions Library
                General.eRIPageViewMode.Add => nameof(Act.ActionUserDescription),
                _ => nameof(Act.ActionType),
            };
        }

        public string GetItemErrorField()
        {
            return nameof(Act.Error);
        }

        public string GetItemExecutionStatusField()
        {
            if (PageViewMode is General.eRIPageViewMode.Automation or General.eRIPageViewMode.ViewAndExecute)
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

            if ((PageViewMode == General.eRIPageViewMode.Automation || PageViewMode == General.eRIPageViewMode.View || PageViewMode == General.eRIPageViewMode.ViewAndExecute)
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
            List<ListItemOperation> operationsList = [];

            ListItemOperation deleteSelected = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "deleteSelected",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete,
                ToolTip = "Delete Selected Actions (Del)",
                OperationHandler = DeleteSelectedHandler
            };
            operationsList.Add(deleteSelected);

            ListItemOperation addSelected = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Add],
                AutomationID = "addSelected",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveLeft,
                ToolTip = "Add Selected Actions",
                OperationHandler = AddActionListView
            };
            operationsList.Add(addSelected);

            ListItemOperation addToFlow = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.AddFromShardRepository],
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveLeft,
                ToolTip = "Add to Flow",
                OperationHandler = AddFromRepository
            };
            operationsList.Add(addToFlow);

            ListItemOperation editItem = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.AddFromShardRepository],
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit,
                ToolTip = "Edit Item",
                OperationHandler = EditAct
            };
            operationsList.Add(editItem);

            return operationsList;
        }

        private void AddActionListView(object sender, RoutedEventArgs e)
        {
            List<RepositoryItemBase> list = [];
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
            List<ListItemOperation> extraOperationsList = [];

            ListItemOperation actionVarsDep = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "actionVarsDep",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.MapSigns,
                Header = "Actions-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies",
                ToolTip = "Set Actions-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies",
                OperationHandler = ActionsVarsHandler
            };
            extraOperationsList.Add(actionVarsDep);

            ListItemOperation activeUnactiveAllActions = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "activeUnactiveAllActions",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.CheckBox,
                Header = "Activate/Deactivate all Actions",
                ToolTip = "Activate/Deactivate all Actions",
                OperationHandler = ActiveUnactiveAllActionsHandler
            };
            extraOperationsList.Add(activeUnactiveAllActions);

            ListItemOperation takeUntakeSS = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "takeUntakeSS",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Image,
                Header = "Take/Untake Screen Shots",
                ToolTip = "Set Take/Untake Screen Shots to all Actions",
                OperationHandler = TakeUntakeSSHandler
            };
            extraOperationsList.Add(takeUntakeSS);

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
                Header = "Delete All Actions",
                OperationHandler = DeleteAllHandler
            };
            extraOperationsList.Add(deleteAll);

            ListItemOperation addSelectedToSR = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "addSelectedToSR",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem,
                Header = "Add Selected to Shared Repository",
                OperationHandler = AddSelectedToSRHandler
            };
            extraOperationsList.Add(addSelectedToSR);

            return extraOperationsList;
        }

        public List<ListItemNotification> GetItemNotificationsList(object item)
        {
            if (PageViewMode != General.eRIPageViewMode.Add)
            {
                SetItem(item);
                List<ListItemNotification> notificationsList = [];

                ListItemNotification simulationInd = new ListItemNotification
                {
                    AutomationID = "simulationInd",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.Simulate,
                    ToolTip = "Action support Simulation mode",
                    ImageSize = 14,
                    BindingObject = mAction,
                    BindingFieldName = nameof(Act.SupportSimulation),
                    BindingConverter = new BoolVisibilityConverter()
                };
                notificationsList.Add(simulationInd);

                ListItemNotification flowControlInd = new ListItemNotification
                {
                    AutomationID = "flowControlInd",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.MapSigns,
                    ToolTip = "Action contains Flow Control conditions",
                    ImageSize = 14,
                    BindingObject = mAction,
                    BindingFieldName = nameof(Act.FlowControlsInfo),
                    BindingConverter = new StringVisibilityConverter()
                };
                notificationsList.Add(flowControlInd);

                ListItemNotification actionsVarsDepInd = new ListItemNotification
                {
                    AutomationID = "actionsVarsDepInd",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.MapSigns,
                    ToolTip = string.Format("{0} Actions-{1} dependency is enabled", GingerDicser.GetTermResValue(eTermResKey.Activity), GingerDicser.GetTermResValue(eTermResKey.Variables)),
                    ImageSize = 14,
                    BindingObject = mContext.Activity,
                    BindingFieldName = nameof(Activity.EnableActionsVariablesDependenciesControl),
                    BindingConverter = new BoolVisibilityConverter()
                };
                notificationsList.Add(actionsVarsDepInd);

                ListItemNotification outputValuesInd = new ListItemNotification
                {
                    AutomationID = "outputValuesInd",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.Output,
                    ToolTip = "Action contains Output Values",
                    BindingObject = mAction,
                    BindingFieldName = nameof(Act.ReturnValuesCount),
                    BindingConverter = new OutPutValuesCountConverter()
                };
                notificationsList.Add(outputValuesInd);

                ListItemNotification waitInd = new ListItemNotification
                {
                    AutomationID = "waitInd",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.Clock,
                    ToolTip = "Action contains Wait time before execution starts",
                    BindingObject = mAction,
                    BindingFieldName = nameof(Act.WaitVE),
                    BindingConverter = new WaitVisibilityConverter()
                };
                notificationsList.Add(waitInd);

                ListItemNotification retryInd = new ListItemNotification
                {
                    AutomationID = "retryInd",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.Retry,
                    ToolTip = "Action configured to Rerun in case of failure",
                    BindingObject = mAction,
                    BindingFieldName = nameof(Act.EnableRetryMechanism),
                    BindingConverter = new BoolVisibilityConverter()
                };
                notificationsList.Add(retryInd);

                ListItemNotification screenshotInd = new ListItemNotification
                {
                    AutomationID = "screenshotInd",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.Image,
                    ToolTip = "Action configured to take Screenshot",
                    BindingObject = mAction,
                    BindingFieldName = nameof(Act.TakeScreenShot),
                    BindingConverter = new BoolVisibilityConverter()
                };
                notificationsList.Add(screenshotInd);

                if (PageViewMode != General.eRIPageViewMode.AddFromShardRepository)
                {
                    ListItemNotification sharedRepoInd = new ListItemNotification
                    {
                        AutomationID = "sharedRepoInd",
                        ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem,
                        ToolTip = "Action source is from Shared Repository",
                        ImageForeground = Brushes.Orange,
                        BindingObject = mAction,
                        BindingFieldName = nameof(Act.IsSharedRepositoryInstance),
                        BindingConverter = new BoolVisibilityConverter()
                    };
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
            List<ListItemOperation> operationsList = [];

            ListItemOperation edit = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "edit",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit,
                ToolTip = "Edit Action",
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

            ListItemOperation active = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "active",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active,
                ImageBindingObject = mAction,
                ImageBindingFieldName = nameof(Act.Active),
                ImageBindingConverter = new ActiveImageTypeConverter(),
                ToolTip = "Active",
                OperationHandler = ActiveHandler
            };
            operationsList.Add(active);

            ListItemOperation viewLinkedInstances = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.AddFromShardRepository],
                AutomationID = "ViewLinkedInstances",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.InstanceLink,
                ToolTip = "View Item Usage",
                OperationHandler = ViewRepositoryItemUsage
            };
            operationsList.Add(viewLinkedInstances);

            return operationsList;
        }

        public List<ListItemOperation> GetItemExtraOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> extraOperationsList = [];

            ListItemOperation breakPoint = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.ViewAndExecute],
                AutomationID = "breakPoint",
                Header = "Break Point",
                ToolTip = "Stop execution on that Action",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active,
                ImageBindingObject = mAction,
                ImageBindingFieldName = nameof(Act.BreakPoint),
                ImageBindingConverter = new ActiveImageTypeConverter(),
                OperationHandler = BreakPointHandler
            };
            extraOperationsList.Add(breakPoint);

            ListItemOperation reset = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.ViewAndExecute],
                AutomationID = "reset",
                Group = "Reset Operations",
                GroupImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset,
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset,
                Header = "Reset Action execution details",
                ToolTip = "Reset Action execution details",
                OperationHandler = ResetHandler
            };
            extraOperationsList.Add(reset);

            ListItemOperation resetRest = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.ViewAndExecute],
                AutomationID = "resetRest",
                Group = "Reset Operations",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset,
                Header = "Reset execution details from this Action",
                ToolTip = "Reset execution details from this Action",
                OperationHandler = ResetResetHandler
            };
            extraOperationsList.Add(resetRest);

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

            return extraOperationsList;
        }

        public List<ListItemOperation> GetItemExecutionOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> executionOperationsList = [];

            ListItemOperation run = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.ViewAndExecute],
                AutomationID = "run",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Run,
                ToolTip = "Run Action",
                OperationHandler = RunHandler
            };
            executionOperationsList.Add(run);

            ListItemOperation continueRun = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.ViewAndExecute],
                AutomationID = "continueRun",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Continue,
                ToolTip = "Continue Run from Action",
                OperationHandler = ContinueRunHandler
            };
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
            if (index < mContext.Activity.Acts.Count - 1)
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
            App.OnAutomateBusinessFlowEvent(BusinessFlowWindows.AutomateEventArgs.eEventType.RunCurrentActionAndMoveOn, new Tuple<Activity, Act, bool>(mContext.Activity, mAction, false));
        }

        private void ContinueRunHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            App.OnAutomateBusinessFlowEvent(BusinessFlowWindows.AutomateEventArgs.eEventType.ContinueActionRun, new Tuple<Activity, Act>(mContext.Activity, mAction));
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
                mAction.Platform = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(x => x.AppName == mContext.Activity.TargetApplication).Platform;
            }
            WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(mContext, mAction));
        }


        private void AddSelectedToSRHandler(object sender, RoutedEventArgs e)
        {
            List<RepositoryItemBase> list = [];
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
                    mAction.Platform = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(x => x.AppName == mContext.Activity.TargetApplication).Platform;
                }
            }
            WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(mContext, list));
        }

        private void CopyAllListHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = [];
            foreach (Act act in mContext.Activity.Acts)
            {
                list.Add(act);
            }
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutAllListHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = [];
            foreach (Act act in mContext.Activity.Acts)
            {
                list.Add(act);
            }
            ClipboardOperationsHandler.SetCutItems(ListView, list);
        }

        private void CopySelectedHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = [];
            foreach (Act act in ListView.List.SelectedItems)
            {
                list.Add(act);
            }
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutSelectedHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = [];
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
            ObservableList<RepositoryItemBase> list = [];
            list.Add(mAction);
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            ObservableList<RepositoryItemBase> list = [];
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
