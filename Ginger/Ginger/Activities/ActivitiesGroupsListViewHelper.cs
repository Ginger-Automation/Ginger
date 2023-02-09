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
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.Activities;
using Ginger.Repository;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using GingerCore.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Ginger.BusinessFlowPages.ListHelpers
{
    public class ActivitiesGroupsListViewHelper : IListViewHelper
    {
        ActivitiesGroup mActivitiesGroup;
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

        public bool AllowExpandItems { get; set; } = true;

        public bool ExpandItemOnLoad { get; set; } = false;

        public ActivitiesGroupsListViewHelper(Context context, General.eRIPageViewMode pageViewMode)
        {
            mContext = context;
            PageViewMode = pageViewMode;
        }

        public void SetItem(object item)
        {
            if (item is ActivitiesGroup)
            {
                mActivitiesGroup = (ActivitiesGroup)item;
            }
        }

        public string GetItemNameField()
        {
            return nameof(ActivitiesGroup.Name);
        }

        public string GetItemMandatoryField()
        {
            return null;
        }

        public string GetItemDescriptionField()
        {
            return nameof(ActivitiesGroup.Description);
        }

        public string GetItemErrorField()
        {
            return null;
        }

        public string GetItemNameExtentionField()
        {
            return null;
        }

        public string GetItemTagsField()
        {
            return nameof(ActivitiesGroup.Tags);
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
            return null;
        }

        public string GetItemIconField()
        {
            return nameof(RepositoryItemBase.ItemImageType);
        }

        public string GetItemIconTooltipField()
        {
            return null;
        }

        public List<ListItemOperation> GetListOperations()
        {
            List<ListItemOperation> operationsList = new List<ListItemOperation>();

            ListItemOperation addToFlow = new ListItemOperation();
            addToFlow.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.AddFromShardRepository };
            addToFlow.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveLeft;
            addToFlow.ToolTip = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, "Add to");
            addToFlow.OperationHandler = AddFromRepository;
            operationsList.Add(addToFlow);

            ListItemOperation editItem = new ListItemOperation();
            editItem.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.AddFromShardRepository };
            editItem.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit;
            editItem.ToolTip = "Edit";
            editItem.OperationHandler = EditActivityGroup;
            operationsList.Add(editItem);

            return operationsList;
        }

        private void AddFromRepository(object sender, RoutedEventArgs e)
        {
            if (mListView.List.SelectedItems != null && mListView.List.SelectedItems.Count > 0)
            {
                if (mContext.BusinessFlow == null)
                {
                    return;
                }
                List<ActivitiesGroup> list = new List<ActivitiesGroup>();
                foreach (ActivitiesGroup selectedItem in mListView.List.SelectedItems)
                {
                    list.Add(selectedItem);
                }
                ActionsFactory.AddActivitiesGroupsFromSRHandler(list, mContext.BusinessFlow);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        private void EditActivityGroup(object sender, RoutedEventArgs e)
        {
            if (mListView.List.SelectedItems != null && mListView.List.SelectedItems.Count > 0)
            {
                ActivitiesGroup activityGroup = (ActivitiesGroup)mListView.List.SelectedItem;
                BusinessFlow currentBF = null;
                if (mContext != null)
                {
                    currentBF = mContext.BusinessFlow;
                }

                ActivitiesGroupPage mActivitiesGroupPage = new ActivitiesGroupPage(activityGroup, currentBF, ActivitiesGroupPage.eEditMode.SharedRepository);
                mActivitiesGroupPage.ShowAsWindow();
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectItem);
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
            return notificationsList;
        }

        public List<ListItemOperation> GetItemOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> operationsList = new List<ListItemOperation>();

            ListItemOperation ViewLinkedInstances = new ListItemOperation();
            ViewLinkedInstances.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.AddFromShardRepository };
            ViewLinkedInstances.AutomationID = "ViewLinkedInstances";
            ViewLinkedInstances.ImageType = Amdocs.Ginger.Common.Enums.eImageType.InstanceLink;
            ViewLinkedInstances.ToolTip = "View Item Usage";
            ViewLinkedInstances.OperationHandler = ViewRepositoryItemUsage;
            operationsList.Add(ViewLinkedInstances);

            return operationsList;
        }
        private void ViewRepositoryItemUsage(object sender, RoutedEventArgs e)
        {
            List<object> SelectedItemsList = mListView.List.SelectedItems.Cast<object>().ToList();

            if (SelectedItemsList.Count > 0)
            {
                RepositoryItemUsagePage usagePage = new RepositoryItemUsagePage((RepositoryItemBase)mListView.List.SelectedItem);
                usagePage.extraDetails = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
                usagePage.ShowAsWindow();
            }
            else
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected); 
            
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

        public void CopySelected()
        {
            throw new System.NotImplementedException();
        }

        public void CutSelected()
        {
            throw new System.NotImplementedException();
        }

        public void Paste()
        {
            throw new System.NotImplementedException();
        }

        public void DeleteSelected()
        {
            throw new System.NotImplementedException();
        }

    }
}
