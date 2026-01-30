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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.Activities;
using Ginger.ALM;
using Ginger.BusinessFlowWindows;
using Ginger.Repository;
using Ginger.Repository.AddItemToRepositoryWizard;
using Ginger.Repository.ItemToRepositoryWizard;
using Ginger.Run;
using Ginger.UserControlsLib;
using Ginger.UserControlsLib.UCListView;
using Ginger.Variables;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.GeneralLib;
using GingerCore.Variables;
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
                    mListView = value;
                }
            }
        }

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

        public string GetItemMandatoryField()
        {
            return null;
        }

        public string GetItemDescriptionField()
        {
            return nameof(Activity.Description);
        }

        public string GetItemErrorField()
        {
            return null;
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
            //needed only on pom page
            if (PageViewMode == General.eRIPageViewMode.AddFromModel)
            {
                if (mActivity.IsSharedRepositoryInstance)
                {
                    return new ListItemUniqueIdentifier() { Color = "DarkOrange", Tooltip = "This is a Shared Activity" };
                }
                if (mActivity.IsAutoLearned)
                {
                    return new ListItemUniqueIdentifier() { Color = "Purple", Tooltip = "This is a Auto Learned Activity" };
                }
            }
            if (mActivity.Type == eSharedItemType.Link)
            {
                return new ListItemUniqueIdentifier() { Color = "Orange", Tooltip = "Linked Shared Activity" };
            }
            else if (mActivity.AddDynamicly)
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
            return nameof(Activity.TargetApplicationPlatformImage);
        }

        public string GetItemIconTooltipField()
        {
            return nameof(Activity.TargetApplicationPlatformName);
        }

        public List<ListItemOperation> GetListOperations(bool AddOperationIcon = true)
        {
            List<ListItemOperation> operationsList = [];

            if (AddOperationIcon)
            {
                ListItemOperation addNew = new ListItemOperation
                {
                    SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                    AutomationID = "addNew",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.Add,
                    ToolTip = "Add New " + GingerDicser.GetTermResValue(eTermResKey.Activity),
                    OperationHandler = AddNewHandler
                };
                operationsList.Add(addNew);

                ListItemOperation addToFlow = new ListItemOperation
                {
                    SupportedViews = [General.eRIPageViewMode.AddFromShardRepository, General.eRIPageViewMode.AddFromModel],
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveLeft,
                    ToolTip = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, "Add to"),
                    OperationHandler = AddFromRepository
                };
                operationsList.Add(addToFlow);
            }

            

            ListItemOperation editItem = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.AddFromShardRepository, General.eRIPageViewMode.AddFromModel],
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit,
                ToolTip = "Edit Item",
                OperationHandler = EditActivity
            };
            operationsList.Add(editItem);

            return operationsList;
        }

        public List<ListItemOperation> GetListExtraOperations()
        {
            List<ListItemOperation> extraOperationsList = [];

            ListItemOperation deleteAll = new ListItemOperation
            {
                AutomationID = "deleteAll",
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete,
                Header = "Delete All " + GingerDicser.GetTermResValue(eTermResKey.Activities),
                ToolTip = "Delete All " + GingerDicser.GetTermResValue(eTermResKey.Activities),
                OperationHandler = DeleteAllHandler
            };
            extraOperationsList.Add(deleteAll);

            ListItemOperation activeUnactiveAllActivities = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "activeUnactiveAllActivities",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.CheckBox,
                Header = "Activate/De-Activate All " + GingerDicser.GetTermResValue(eTermResKey.Activities),
                ToolTip = "Activate/De-Activate all " + GingerDicser.GetTermResValue(eTermResKey.Activities),
                OperationHandler = ActiveUnactiveAllActivitiesHandler
            };
            extraOperationsList.Add(activeUnactiveAllActivities);

            ListItemOperation activitiesVarsDep = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "activitiesVarsDep",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.MapSigns,
                Header = string.Format("{0}-{1} Dependencies", GingerDicser.GetTermResValue(eTermResKey.Activities), GingerDicser.GetTermResValue(eTermResKey.Variables)),
                ToolTip = string.Format("Set {0}-{1} Dependencies", GingerDicser.GetTermResValue(eTermResKey.Activities), GingerDicser.GetTermResValue(eTermResKey.Variables)),
                OperationHandler = ActivitiesVarsHandler
            };
            extraOperationsList.Add(activitiesVarsDep);

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
                SupportedViews = [General.eRIPageViewMode.View, General.eRIPageViewMode.ViewAndExecute, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "copySelected",
                Group = "Clipboard",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Copy,
                Header = "Copy Selected Items (Ctrl+C)",
                OperationHandler = CopySelectedHandler
            };
            extraOperationsList.Add(copySelected);

            ListItemOperation cutSelected = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
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


            return extraOperationsList;
        }

        public List<ListItemNotification> GetItemNotificationsList(object item)
        {
            SetItem(item);
            List<ListItemNotification> notificationsList = [];

            if (PageViewMode != General.eRIPageViewMode.AddFromShardRepository)
            {
                ListItemNotification activitiesVarsDepInd = new ListItemNotification
                {
                    AutomationID = "activitiesVarsDepInd",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.MapSigns,
                    ToolTip = string.Format("{0} {1}-{2} dependency is enabled", GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), GingerDicser.GetTermResValue(eTermResKey.Activities), GingerDicser.GetTermResValue(eTermResKey.Variables)),
                    ImageSize = 14,
                    BindingObject = mContext.BusinessFlow,
                    BindingFieldName = nameof(BusinessFlow.EnableActivitiesVariablesDependenciesControl),
                    BindingConverter = new BoolVisibilityConverter()
                };
                notificationsList.Add(activitiesVarsDepInd);
            }

            ListItemNotification mandatoryInd = new ListItemNotification
            {
                AutomationID = "mandatoryInd",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Mandatory,
                ToolTip = string.Format("{0} is Mandatory", GingerDicser.GetTermResValue(eTermResKey.Activity)),
                ImageSize = 14,
                BindingObject = mActivity,
                BindingFieldName = nameof(Activity.Mandatory),
                BindingConverter = new BoolVisibilityConverter()
            };
            notificationsList.Add(mandatoryInd);

            ListItemNotification publishInd = new ListItemNotification
            {
                AutomationID = "publishInd",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Share,
                ToolTip = string.Format("{0} is marked to be Published to third party applications", GingerDicser.GetTermResValue(eTermResKey.Activity)),
                ImageSize = 14,
                BindingObject = mActivity,
                BindingFieldName = nameof(RepositoryItemBase.Publish),
                BindingConverter = new BoolVisibilityConverter()
            };
            notificationsList.Add(publishInd);

            if (PageViewMode is not General.eRIPageViewMode.AddFromShardRepository and not General.eRIPageViewMode.AddFromModel)
            {
                ListItemNotification sharedRepoInd = new ListItemNotification
                {
                    AutomationID = "sharedRepoInd",
                    ImageType = mActivity.IsLinkedItem ? Amdocs.Ginger.Common.Enums.eImageType.InstanceLink : Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem,
                    ToolTip = mActivity.IsLinkedItem ? string.Format("{0} source is linked to {0} from Shared Repository", GingerDicser.GetTermResValue(eTermResKey.Activity)) : string.Format("{0} source is instance from Shared Repository", GingerDicser.GetTermResValue(eTermResKey.Activity)),
                    ImageSize = 13.5,
                    ImageForeground = Brushes.Orange,
                    BindingObject = mActivity,
                    BindingFieldName = nameof(Activity.IsSharedRepositoryInstance),
                    BindingConverter = new BoolVisibilityConverter(),

                    ImageTypeBindingFieldName = nameof(Activity.Type),
                    ImageTypeBindingConverter = new ActivityTypeConverter(),

                    TooltipBindingFieldName = nameof(Activity.Type),
                    TooltipBindingConverter = new TooltipConverter()
                };

                notificationsList.Add(sharedRepoInd);
            }
            if (mActivity.AIGenerated)
            {
                ListItemNotification aIGeneratedInd = new ListItemNotification
                {
                    AutomationID = "aIGeneratedInd",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.AIActivity,
                    ToolTip = string.Format("{0} is AI Generated", GingerDicser.GetTermResValue(eTermResKey.Activity)),
                    ImageSize = 16,
                    BindingObject = mActivity,
                    BindingFieldName = nameof(RepositoryItemBase.AIGenerated),
                    BindingConverter = new BoolVisibilityConverter()
                };
                notificationsList.Add(aIGeneratedInd);
            }
            return notificationsList;
        }

        public List<ListItemOperation> GetItemOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> operationsList = [];

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
                ImageBindingObject = mActivity,
                ImageBindingFieldName = nameof(Activity.Active),
                ImageBindingConverter = new ActiveImageTypeConverter(),
                ToolTip = "Active",
                IsEnabeled = mActivity.IsNotGherkinOptimizedActivity,
                OperationHandler = ActiveHandler
            };
            operationsList.Add(active);


            ListItemOperation ViewLinkedInstances = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.AddFromShardRepository],
                AutomationID = "ViewLinkedInstances",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.InstanceLink,
                ToolTip = "View Item Usage",
                OperationHandler = ViewRepositoryItemUsage
            };
            operationsList.Add(ViewLinkedInstances);

            return operationsList;
        }

        public List<ListItemOperation> GetItemExtraOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> extraOperationsList = [];

            ListItemOperation mandatory = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "mandatory",
                Header = "Mandatory",
                ToolTip = string.Format("If {0} fails so stop execution", GingerDicser.GetTermResValue(eTermResKey.Activity)),
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active,
                ImageBindingObject = mActivity,
                ImageBindingFieldName = nameof(Activity.Mandatory),
                ImageBindingConverter = new ActiveImageTypeConverter(),
                OperationHandler = MandatoryHandler
            };
            extraOperationsList.Add(mandatory);

            ListItemOperation reset = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation],
                AutomationID = "reset",
                Group = "Reset Operations",
                GroupImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset,
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset,
                Header = string.Format("Reset {0} execution details", GingerDicser.GetTermResValue(eTermResKey.Activity)),
                ToolTip = string.Format("Reset {0} execution details", GingerDicser.GetTermResValue(eTermResKey.Activity)),
                OperationHandler = ResetHandler
            };
            extraOperationsList.Add(reset);

            ListItemOperation resetRest = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation],
                AutomationID = "resetRest",
                Group = "Reset Operations",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Reset,
                Header = string.Format("Reset execution details from this {0}", GingerDicser.GetTermResValue(eTermResKey.Activity)),
                ToolTip = string.Format("Reset execution details from this {0}", GingerDicser.GetTermResValue(eTermResKey.Activity)),
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

            ListItemOperation moveToOtherGroup = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "moveToOtherGroup",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveUpDown,
                Header = "Move to Other Group",
                ToolTip = "Move to Other Group",
                OperationHandler = MoveToOtherGroupHandler
            };
            extraOperationsList.Add(moveToOtherGroup);

            ListItemOperation publish = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "publish",
                Header = "Publish",
                ToolTip = "Publish to third party applications",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active,
                ImageBindingObject = mActivity,
                ImageBindingFieldName = nameof(RepositoryItemBase.Publish),
                ImageBindingConverter = new ActiveImageTypeConverter(),
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

            if (mActivity.IsSharedRepositoryInstance && mActivity.Type == eSharedItemType.Link)
            {
                ListItemOperation convertToSR = new ListItemOperation
                {
                    SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                    AutomationID = "ConvertToSR",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem,
                    Header = "Convert to Regular Instance",
                    ToolTip = "Convert to Regular Shared Repository",
                    OperationHandler = ConvertToRegularSRHandler
                };
                extraOperationsList.Add(convertToSR);
            }
            if (mActivity.IsSharedRepositoryInstance && mActivity.Type == eSharedItemType.Regular)
            {
                ListItemOperation convertToLSR = new ListItemOperation
                {
                    SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                    AutomationID = "ConvertToLink",
                    ImageType = Amdocs.Ginger.Common.Enums.eImageType.InstanceLink,
                    Header = "Convert to Link Instance",
                    ToolTip = "Convert to Link Shared Repository",
                    OperationHandler = ConvertToLinkedSRHandler
                };
                extraOperationsList.Add(convertToLSR);
            }

            return extraOperationsList;
        }

        public List<ListItemOperation> GetItemExecutionOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> executionOperationsList = [];

            ListItemOperation run = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation],
                AutomationID = "run",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Run,
                ToolTip = "Run " + GingerDicser.GetTermResValue(eTermResKey.Activity),
                OperationHandler = RunHandler
            };
            executionOperationsList.Add(run);

            ListItemOperation continueRun = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation],
                AutomationID = "continueRun",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Continue,
                ToolTip = "Continue Run from " + GingerDicser.GetTermResValue(eTermResKey.Activity),
                OperationHandler = ContinueRunHandler
            };
            executionOperationsList.Add(continueRun);

            ListItemOperation runAction = new ListItemOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation],
                AutomationID = "runAction",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.RunSingle,
                ToolTip = "Run Current Action",
                OperationHandler = RunActionHandler
            };
            executionOperationsList.Add(runAction);

            return executionOperationsList;
        }

        public List<ListItemNotification> GetItemGroupNotificationsList(string GroupName)
        {
            if (PageViewMode == General.eRIPageViewMode.Automation)
            {
                ActivitiesGroup group = mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == GroupName);
                if (group != null)
                {
                    List<ListItemNotification> notificationsList = [];

                    ListItemNotification publishInd = new ListItemNotification
                    {
                        AutomationID = "publishInd",
                        ImageType = Amdocs.Ginger.Common.Enums.eImageType.Share,
                        ToolTip = string.Format("{0} is marked to be Published to third party applications", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)),
                        //publishInd.ImageForeground = Brushes.Orange;
                        BindingObject = group,
                        BindingFieldName = nameof(RepositoryItemBase.Publish),
                        BindingConverter = new BoolVisibilityConverter()
                    };
                    notificationsList.Add(publishInd);

                    ListItemNotification sharedRepoInd = new ListItemNotification
                    {
                        AutomationID = "sharedRepoInd",
                        ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem,
                        ToolTip = string.Format("{0} source is from Shared Repository", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)),
                        ImageForeground = Brushes.Orange,
                        BindingObject = group,
                        BindingFieldName = nameof(ActivitiesGroup.IsSharedRepositoryInstance),
                        BindingConverter = new BoolVisibilityConverter()
                    };
                    notificationsList.Add(sharedRepoInd);

                    return notificationsList;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public List<ListItemGroupOperation> GetItemGroupOperationsList()
        {
            List<ListItemGroupOperation> groupOperationsList = [];

            ListItemGroupOperation addNewActivity = new ListItemGroupOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "addNewGroupActivity",
                Header = string.Concat("Add New ", GingerDicser.GetTermResValue(eTermResKey.Activity)),
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Add,
                ToolTip = string.Concat("Add New ", GingerDicser.GetTermResValue(eTermResKey.Activity)),
                OperationHandler = AddNewActivityToGroupHandler
            };
            groupOperationsList.Add(addNewActivity);

            ListItemGroupOperation rename = new ListItemGroupOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "renameGroup",
                Header = string.Concat("Rename ", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)),
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit,
                ToolTip = string.Concat("Rename " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)),
                OperationHandler = RenameGroupHandler
            };
            groupOperationsList.Add(rename);

            ListItemGroupOperation moveUp = new ListItemGroupOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "moveGroupUp",
                Header = string.Concat("Move ", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), " Up"),
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveUp,
                ToolTip = string.Concat("Move all ", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), " Up"),
                OperationHandler = MoveGroupUpHandler
            };
            groupOperationsList.Add(moveUp);

            ListItemGroupOperation moveDown = new ListItemGroupOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "moveGroupDown",
                Header = string.Concat("Move ", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), " Down"),
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveDown,
                ToolTip = string.Concat("Move all ", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), " down"),
                OperationHandler = MoveGroupDownHandler
            };
            groupOperationsList.Add(moveDown);

            ListItemGroupOperation delete = new ListItemGroupOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "deleteGroup",
                Header = string.Concat("Delete ", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)),
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete,
                ToolTip = string.Concat("Delete " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), " and its ", GingerDicser.GetTermResValue(eTermResKey.Activities)),
                OperationHandler = DeleteGroupHandler
            };
            groupOperationsList.Add(delete);

            ListItemGroupOperation disable = new ListItemGroupOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "disableGroup",
                Header = string.Concat("Disable ", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)),
                ImageSize = 14,
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.InActive,
                ToolTip = string.Concat("Disable ", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), " and its ", GingerDicser.GetTermResValue(eTermResKey.Activities)),
                OperationHandler = DisableGroupHandler
            };
            groupOperationsList.Add(disable);

            ListItemGroupOperation activate = new ListItemGroupOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "activateGroup",
                Header = string.Concat("Activate ", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)),
                ImageSize = 14,
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active,
                ToolTip = string.Concat("Activate ", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), " and its ", GingerDicser.GetTermResValue(eTermResKey.Activities)),
                OperationHandler = ActivateGroupHandler
            };
            groupOperationsList.Add(activate);

            ListItemGroupOperation copyGroup = new ListItemGroupOperation
            {
                SupportedViews = [General.eRIPageViewMode.View, General.eRIPageViewMode.ViewAndExecute, General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "copyGroup",
                Group = "Clipboard",
                GroupImageType = Amdocs.Ginger.Common.Enums.eImageType.Clipboard,
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Copy,
                Header = string.Concat("Copy ", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), " Items"),
                OperationHandler = CopyGroupHandler
            };
            groupOperationsList.Add(copyGroup);

            ListItemGroupOperation cutGroup = new ListItemGroupOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "cutGroup",
                Group = "Clipboard",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Cut,
                Header = string.Concat("Cut ", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), " Items"),
                OperationHandler = CutGroupHandler
            };
            groupOperationsList.Add(cutGroup);

            ListItemGroupOperation pasteInGroup = new ListItemGroupOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "pasterAfterCurrent",
                Group = "Clipboard",
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Paste,
                Header = "Paste",
                OperationHandler = PasteInGroupHandler
            };
            groupOperationsList.Add(pasteInGroup);

            ListItemGroupOperation export = new ListItemGroupOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "exportGroup",
                GroupImageType = Amdocs.Ginger.Common.Enums.eImageType.ALM,
                Group = "ALM Operations",
                Header = string.Concat("Export ", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)),
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Share,
                ToolTip = string.Concat("Export ", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), " and its ", GingerDicser.GetTermResValue(eTermResKey.Activities), " to ALM"),
                OperationHandler = ExportGroupHandler,
                Visible = false
            };
            groupOperationsList.Add(export);

            ListItemGroupOperation publishGroup = new ListItemGroupOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "publishGroup",
                Header = string.Concat("Publish/Unpublish ", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)),
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Share,
                ToolTip = string.Format("Set if {0} is marked to be Published to third party applications", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)),
                OperationHandler = SetPublishGroupHandler
            };
            groupOperationsList.Add(publishGroup);

            ListItemGroupOperation addToSR = new ListItemGroupOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "addGroupToSR",
                Header = string.Concat("Add ", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), " to Shared Repository"),
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.SharedRepositoryItem,
                ToolTip = string.Concat("Add ", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), " and its ", GingerDicser.GetTermResValue(eTermResKey.Activities), " to Shared Repository"),
                OperationHandler = AddGroupToSRHandler
            };
            groupOperationsList.Add(addToSR);

            ListItemGroupOperation details = new ListItemGroupOperation
            {
                SupportedViews = [General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone],
                AutomationID = "detailsGroup",
                Header = string.Concat(GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), " Details"),
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Config,
                ToolTip = string.Concat(GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), " Details"),
                OperationHandler = DetailsGroupHandler
            };
            groupOperationsList.Add(details);


            return groupOperationsList;
        }

        private void AddNewHandler(object sender, RoutedEventArgs e)
        {
            WizardWindow.ShowWizard(new AddActivityWizard(mContext));
        }

        private void AddFromRepository(object sender, RoutedEventArgs e)
        {
            if (mListView.List.SelectedItems != null && mListView.List.SelectedItems.Count > 0)
            {
                if (mContext.BusinessFlow == null)
                {
                    return;
                }
                List<Activity> list = [];
                bool isPomActivity = false;
                foreach (Activity selectedItem in mListView.List.SelectedItems)
                {
                    list.Add(selectedItem);
                    isPomActivity = selectedItem.IsAutoLearned;
                }
                ActionsFactory.AddActivitiesFromSRHandler(list, mContext.BusinessFlow, null, -1, isPomActivity);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        private void EditActivity(object sender, RoutedEventArgs e)
        {
            if (mListView.List.SelectedItems != null && mListView.List.SelectedItems.Count > 0)
            {
                Activity a = (Activity)mListView.CurrentItem;
                Context context = new Context()
                {
                    Activity = a,
                    Runner = new GingerExecutionEngine(new GingerRunner())
                };
                GingerWPF.BusinessFlowsLib.ActivityPage w = new GingerWPF.BusinessFlowsLib.ActivityPage(a, context, General.eRIPageViewMode.SharedReposiotry);
                w.ShowAsWindow();
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectItem);
            }
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

            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.RunCurrentActionAndMoveOn, new Tuple<Activity, Act, bool>(mActivity, (Act)mActivity.Acts.CurrentItem, false));
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

        private void MandatoryHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mActivity.Mandatory = !mActivity.Mandatory;
        }

        private void PublishHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mActivity.Publish = !mActivity.Publish;
        }

        private void AddToSRHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(mContext, mActivity));
        }
        private void ConvertToRegularSRHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(mContext, mActivity, true, UploadItemSelection.eActivityInstanceType.RegularInstance));
        }
        private void ConvertToLinkedSRHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(mContext, mActivity, true, UploadItemSelection.eActivityInstanceType.LinkInstance));
        }

        private void ResetHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            mActivity.Reset();
        }

        private void ResetResetHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            for (int indx = mContext.BusinessFlow.Activities.IndexOf(mActivity); indx < mContext.BusinessFlow.Activities.Count; indx++)
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
                    Reporter.ToLog(eLogLevel.DEBUG, "Error occurred while dragging Activity to other group", ex);
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
                mContext.BusinessFlow.MoveActivityInGroup(mActivity, index + 1);
            }
        }

        private void AddNewActivityToGroupHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == ((MenuItem)sender).Tag.ToString());

            WizardWindow.ShowWizard(new AddActivityWizard(mContext, activitiesGroup));
        }

        private void RenameGroupHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == ((MenuItem)sender).Tag.ToString());

            if (activitiesGroup.Name.Contains("Optimized Activities"))
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "This is an automatic group created from Gherkin file and can not be modified");
                return;
            }
            string newName = activitiesGroup.Name;


            if (InputBoxWindow.GetInputWithValidation($"Rename {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)} ", $"New {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)} Name:", ref newName))
            {
                if (!string.IsNullOrEmpty(newName))
                {
                    if (mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name.Trim() == newName.Trim()) == null)
                    {
                        activitiesGroup.ChangeName(newName);
                        OnActivityListItemEvent(ActivityListItemEventArgs.eEventType.UpdateGrouping);
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, $"{GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)} with same name already exist, please set unique name.");
                    }
                }
            }
        }

        private void MoveGroupUpHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == ((MenuItem)sender).Tag.ToString());
            mContext.BusinessFlow.MoveActivitiesGroupUp(activitiesGroup);
            OnActivityListItemEvent(ActivityListItemEventArgs.eEventType.UpdateGrouping);
        }

        private void MoveGroupDownHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == ((MenuItem)sender).Tag.ToString());
            mContext.BusinessFlow.MoveActivitiesGroupDown(activitiesGroup);
            OnActivityListItemEvent(ActivityListItemEventArgs.eEventType.UpdateGrouping);
        }

        private void DeleteGroupHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == ((MenuItem)sender).Tag.ToString());
            if (Reporter.ToUser(eUserMsgKey.SureWantToDeleteGroup, activitiesGroup.Name) == eUserMsgSelection.Yes)
            {
                mContext.BusinessFlow.DeleteActivitiesGroup(activitiesGroup);
            }
        }

        private void DisableGroupHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == ((MenuItem)sender).Tag.ToString());
            foreach (ActivityIdentifiers activityIdt in activitiesGroup.ActivitiesIdentifiers)
            {
                activityIdt.IdentifiedActivity.Active = false;
            }
        }

        private void ActivateGroupHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == ((MenuItem)sender).Tag.ToString());
            foreach (ActivityIdentifiers activityIdt in activitiesGroup.ActivitiesIdentifiers)
            {
                activityIdt.IdentifiedActivity.Active = true;
            }
        }

        private void AddGroupToSRHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == ((MenuItem)sender).Tag.ToString());

            List<RepositoryItemBase> list = [activitiesGroup];
            foreach (ActivityIdentifiers activityIdnt in activitiesGroup.ActivitiesIdentifiers)
            {
                list.Add(activityIdnt.IdentifiedActivity);
            }

            WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(mContext, list));
        }

        private void DetailsGroupHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == ((MenuItem)sender).Tag.ToString());
            BusinessFlow currentBF = null;
            if (mContext != null)
            {
                currentBF = mContext.BusinessFlow;
            }
            ActivitiesGroupPage mActivitiesGroupPage = new ActivitiesGroupPage(activitiesGroup, currentBF, ActivitiesGroupPage.eEditMode.ExecutionFlow, mContext);
            mActivitiesGroupPage.ShowAsWindow();
        }

        private void SetPublishGroupHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == ((MenuItem)sender).Tag.ToString());
            activitiesGroup.Publish = !activitiesGroup.Publish;
            foreach (ActivityIdentifiers activityIdnt in activitiesGroup.ActivitiesIdentifiers)
            {
                activityIdnt.IdentifiedActivity.Publish = activitiesGroup.Publish;
            }
        }

        private void ExportGroupHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == ((MenuItem)sender).Tag.ToString());
            ObservableList<ActivitiesGroup> list = [activitiesGroup];
            ALMIntegration.Instance.ExportBfActivitiesGroupsToALM(mContext.BusinessFlow, list);
        }

        private void CopyAllListHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = [.. mContext.BusinessFlow.Activities];
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutAllListHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = [.. mContext.BusinessFlow.Activities];
            ClipboardOperationsHandler.SetCutItems(ListView, list);
        }

        private void CopySelectedHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = [];
            foreach (Activity var in ListView.List.SelectedItems)
            {
                list.Add(var);
            }
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutSelectedHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<RepositoryItemBase> list = [];
            foreach (Activity var in ListView.List.SelectedItems)
            {
                list.Add(var);
            }
            ClipboardOperationsHandler.SetCutItems(ListView, list);
        }

        private void CopyHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            ObservableList<RepositoryItemBase> list = [mActivity];
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutHandler(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            ObservableList<RepositoryItemBase> list = [mActivity];
            ClipboardOperationsHandler.SetCutItems(ListView, list);
        }

        private void PasteAfterCurrentHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = null;
            if (sender != null)
            {
                SetItem(sender);
                activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == mActivity.ActivitiesGroupID);
            }
            else if (ListView.List.SelectedItems.Count > 0)
            {
                activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == ((Activity)(ListView.List.SelectedItems[0])).ActivitiesGroupID);
            }
            if (activitiesGroup != null)
            {
                int insertIndex = mContext.BusinessFlow.Activities.IndexOf(mActivity) + 1;
                DoActivitiesPaste(activitiesGroup, insertIndex);
            }
        }

        private void CopyGroupHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == ((MenuItem)sender).Tag.ToString());
            ObservableList<RepositoryItemBase> list = [];
            foreach (ActivityIdentifiers activityIdnt in activitiesGroup.ActivitiesIdentifiers)
            {
                list.Add(activityIdnt.IdentifiedActivity);
            }
            ClipboardOperationsHandler.SetCopyItems(list);
        }

        private void CutGroupHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == ((MenuItem)sender).Tag.ToString());
            ObservableList<RepositoryItemBase> list = [];
            foreach (ActivityIdentifiers activityIdnt in activitiesGroup.ActivitiesIdentifiers)
            {
                list.Add(activityIdnt.IdentifiedActivity);
            }
            ClipboardOperationsHandler.SetCutItems(ListView, list);
        }

        private void PasteInGroupHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == ((MenuItem)sender).Tag.ToString());
            int insertIndex = mContext.BusinessFlow.Activities.IndexOf(activitiesGroup.ActivitiesIdentifiers[^1].IdentifiedActivity) + 1;
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
                    insertIndex = mContext.BusinessFlow.Activities.IndexOf(activitiesGroup.ActivitiesIdentifiers[^1].IdentifiedActivity) + 1;
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
                                if (mContext.BusinessFlow.TargetApplications.FirstOrDefault(x => x.Name == copiedItem.TargetApplication) == null
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
                                if (mContext.BusinessFlow.TargetApplications.FirstOrDefault(x => x.Name == ((Activity)item).TargetApplication) == null
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
            ActivitiesGroup activitiesGroup = mContext.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == ((MenuItem)sender).Tag.ToString());
            ObservableList<RepositoryItemBase> list = [];
            foreach (ActivityIdentifiers groupActivityIdent in activitiesGroup.ActivitiesIdentifiers)
            {
                list.Add(groupActivityIdent.IdentifiedActivity);
            }

            return list;
        }

        private ObservableList<RepositoryItemBase> GetActivitiesList()
        {
            ObservableList<RepositoryItemBase> list = [.. mContext.BusinessFlow.Activities];
            return list;
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
                if (ListView.List.SelectedItems.Count > 0)
                {
                    PasteAfterCurrentHandler(null, null);
                }
                else
                {
                    PasteInListHandler(null, null);
                }
            }
        }

        public void DeleteSelected()
        {
            if (PageViewMode is General.eRIPageViewMode.Automation or General.eRIPageViewMode.SharedReposiotry or
                 General.eRIPageViewMode.Child or General.eRIPageViewMode.ChildWithSave or
                    General.eRIPageViewMode.Standalone)
            {
                if (ListView.List.SelectedItems.Count == 0)
                {
                    Reporter.ToUser(eUserMsgKey.SelectItemToDelete);
                    return;
                }

                if (Reporter.ToUser(eUserMsgKey.SureWantToDeleteSelectedItems, GingerDicser.GetTermResValue(eTermResKey.Activities), ((Activity)ListView.List.SelectedItems[0]).ActivityName) == eUserMsgSelection.Yes)
                {
                    List<object> SelectedItemsList = ListView.List.SelectedItems.Cast<object>().ToList();
                    foreach (Activity activity in SelectedItemsList)
                    {
                        mContext.BusinessFlow.DeleteActivity(activity);
                    }
                }
            }
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
