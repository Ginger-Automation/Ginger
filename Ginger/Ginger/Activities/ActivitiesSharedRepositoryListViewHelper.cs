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
    public class ActivitiesSharedRepositoryListViewHelper : IListViewHelper
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

        public bool AllowExpandItems { get; set; } = true;

        public bool ExpandItemOnLoad { get; set; } = false;

        public ActivitiesSharedRepositoryListViewHelper(Context context, General.eRIPageViewMode pageViewMode)
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
            return null;
        }

        public string GetItemTagsField()
        {
            return nameof(Activity.Tags);
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
            //return new ListItemUniqueIdentifier() { Color = mActivity.GroupColor, Tooltip = mActivity.Name };
            return null;
        }

        public string GetItemIconField()
        {
            return nameof(RepositoryItemBase.ItemImageType); // g
        }

        public string GetItemIconTooltipField()
        {
            return nameof(Activity.ActivityType);
        }

        public List<ListItemOperation> GetListOperations()
        {
            List<ListItemOperation> operationsList = new List<ListItemOperation>();

            ListItemOperation addToFlow = new ListItemOperation();
            addToFlow.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            addToFlow.ImageType = Amdocs.Ginger.Common.Enums.eImageType.AngleArrowLeft;
            addToFlow.ToolTip = "Add to Flow";
            addToFlow.OperationHandler = AddFromRepository;
            operationsList.Add(addToFlow);

            ListItemOperation editItem = new ListItemOperation();
            editItem.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            editItem.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit;
            editItem.ToolTip = "Edit Item";
            editItem.OperationHandler = EditActivity;
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
                List<Activity> list = new List<Activity>();
                foreach (Activity selectedItem in mListView.List.SelectedItems)
                {
                    list.Add(selectedItem);
                }
                ActionsFactory.AddActivitiesFromSRHandler(list, mContext.BusinessFlow);
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
                GingerWPF.BusinessFlowsLib.ActivityPage w = new GingerWPF.BusinessFlowsLib.ActivityPage(a, new Context() { Activity = a }, General.eRIPageViewMode.SharedReposiotry);
                w.ShowAsWindow();
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
            ViewLinkedInstances.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.Automation, General.eRIPageViewMode.SharedReposiotry, General.eRIPageViewMode.Child, General.eRIPageViewMode.ChildWithSave, General.eRIPageViewMode.Standalone };
            ViewLinkedInstances.AutomationID = "ViewLinkedInstances";
            ViewLinkedInstances.ImageType = Amdocs.Ginger.Common.Enums.eImageType.InstanceLink;
            ViewLinkedInstances.ToolTip = "View Linked Instances";
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
                usagePage.ShowAsWindow();
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }          
        }

        public List<ListItemOperation> GetItemExtraOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> extraOperationsList = new List<ListItemOperation>();

            //ListItemOperation mandatory = new ListItemOperation();
            //mandatory.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active;
            //mandatory.ImageBindingObject = mActivity;
            //mandatory.ImageBindingFieldName = nameof(Activity.Mandatory);
            //mandatory.ImageBindingConverter = new ActiveImageTypeConverter();
            //mandatory.ToolTip = "Mandatory";
            ////active.ImageSize = 15;
            //mandatory.OperationHandler = MandatoryHandler;
            //extraOperationsList.Add(mandatory);

            return extraOperationsList;
        }

        public List<ListItemOperation> GetItemExecutionOperationsList(object item)
        {
            SetItem(item);
            List<ListItemOperation> executionOperationsList = new List<ListItemOperation>();

            //ListItemOperation run = new ListItemOperation();
            //run.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Run;
            //run.ToolTip = "Run " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            //run.OperationHandler = RunHandler;
            //executionOperationsList.Add(run);

            //ListItemOperation continueRun = new ListItemOperation();
            //continueRun.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Continue;
            //continueRun.ToolTip = "Continue Run " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            //continueRun.OperationHandler = ContinueRunHandler;
            //executionOperationsList.Add(continueRun);

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

        //private void EditHandler(object sender, RoutedEventArgs e)
        //{
        //    SetItem(sender);
        //    mAction.SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper();
        //    mAction.Context = mContext;
        //    ActionEditPage actedit = new ActionEditPage(mAction, General.RepositoryItemPageViewMode.Automation);//TODO: check if need diifrent mode
        //    //actedit.ap = this;
        //    actedit.ShowAsWindow();
        //}

        //private void ActiveHandler(object sender, RoutedEventArgs e)
        //{
        //    SetItem(sender);
        //    mAction.Active = !mAction.Active;
        //}


    }
}
