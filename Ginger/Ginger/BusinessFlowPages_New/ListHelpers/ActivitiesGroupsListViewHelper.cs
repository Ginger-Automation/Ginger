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
using Amdocs.Ginger.UserControls;
using Ginger.UserControlsLib.UCListView;
using GingerCore.Activities;
using System.Collections.Generic;

namespace Ginger.BusinessFlowPages.ListHelpers
{
    public class ActivitiesGroupsListViewHelper : IListViewHelper
    {
        ActivitiesGroup mActivitiesGroup;
        Context mContext;

        public General.eRIPageViewMode PageViewMode { get; set; }

        public UcListView ListView { get; set; }

        public bool AllowExpandItems { get; set; } = true;

        public bool ExpandItemOnLoad { get; set; } = false;

        public ActivitiesGroupsListViewHelper(Context context)
        {
            mContext = context;
        }

        public void SetItem(object item)
        {
            if (item is ActivitiesGroup)
            {
                mActivitiesGroup = (ActivitiesGroup)item;
            }
            else if (item is ucButton)
            {
                mActivitiesGroup = (ActivitiesGroup)(((ucButton)item).Tag);
            }
        }

        public string GetItemNameField()
        {
            return nameof(ActivitiesGroup.Name);
        }

        public string GetItemDescriptionField()
        {
            return nameof(ActivitiesGroup.Description);
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
            return nameof(ActivitiesGroup.RunStatus);
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
            return null;//TODO: return ActivitiesGroup image type
        }

        public string GetItemIconTooltipField()
        {
            return null;
        }

        public List<ListItemOperation> GetListOperations()
        {
            List<ListItemOperation> operationsList = new List<ListItemOperation>();

            //ListItemOperation addNew = new ListItemOperation();
            //addNew.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Add;
            //addNew.ToolTip = "Add New " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            //addNew.OperationHandler = AddNewHandler;
            //operationsList.Add(addNew);

            //ListItemOperation deleteAll = new ListItemOperation();
            //deleteAll.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Delete;
            //deleteAll.ToolTip = "Delete All " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            //deleteAll.OperationHandler = DeleteAllHandler;
            //operationsList.Add(deleteAll);

            return operationsList;
        }

        public List<ListItemOperation> GetListExtraOperations()
        {
            List<ListItemOperation> extraOperationsList = new List<ListItemOperation>();

            //ListItemOperation activeUnactiveAllActivities = new ListItemOperation();
            //activeUnactiveAllActivities.ImageType = Amdocs.Ginger.Common.Enums.eImageType.CheckBox;
            //activeUnactiveAllActivities.Header = "Activate/Un-Activate all " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            //activeUnactiveAllActivities.ToolTip = "Activate/Un-Activate all " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            //activeUnactiveAllActivities.OperationHandler = ActiveUnactiveAllActivitiesHandler;
            //extraOperationsList.Add(activeUnactiveAllActivities);

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

            //ListItemOperation edit = new ListItemOperation();
            //edit.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Edit;
            //edit.ToolTip = "Edit Action";
            //edit.OperationHandler = EditHandler;
            //operationsList.Add(edit);


            //ListItemOperation active = new ListItemOperation();
            //active.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Active;
            //active.ImageBindingObject = mAction;
            //active.ImageBindingFieldName = nameof(Act.Active);
            //active.ImageBindingConverter = new ActiveImageTypeConverter();
            //active.ToolTip = "Activate/Un-Activate Action";
            ////active.ImageSize = 15;
            //active.OperationHandler = ActiveHandler;
            //operationsList.Add(active);

            return operationsList;
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
