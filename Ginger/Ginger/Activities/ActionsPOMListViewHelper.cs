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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.Activities;
using Ginger.Repository;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Ginger.BusinessFlowPages.ListHelpers
{
    public class ActionsPOMListViewHelper : IListViewHelper
    {
        Act mAct;
        public Context mContext;
        private Agent mAgent;
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

        public ActionsPOMListViewHelper(Context context, General.eRIPageViewMode pageViewMode)
        {
            mContext = context;
            PageViewMode = pageViewMode;
        }

        public void SetItem(object item)
        {
            if (item is ApplicationPOMModel)
            {
                mAct = (Act)item;
            }
            else if (item is ucButton)
            {
                mAct = (Act)(((ucButton)item).Tag);
            }
        }

        public string GetItemNameField()
        {
            return nameof(ElementInfo.ElementName);
        }

        public string GetItemMandatoryField()
        {
            return null;
        }

        public string GetItemDescriptionField()
        {
            return nameof(ElementInfo.ElementTypeEnum);
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
            return null; 
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
            //return new ListItemUniqueIdentifier() { Color = mAct.GroupColor, Tooltip = mAct.Name };
            return null;
        }

        public string GetItemIconField()
        {
            return nameof(ElementInfo.ElementTypeImage);
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
            addToFlow.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Add;
            addToFlow.ToolTip = "Add to Actions";
            addToFlow.OperationHandler = AddFromPOMNavPage;
            operationsList.Add(addToFlow);

            return operationsList;
        }

        private void AddFromPOMNavPage(object sender, RoutedEventArgs e)
        {
            if (mListView.List.SelectedItems != null && mListView.List.SelectedItems.Count > 0)
            {
                foreach (ElementInfo selectedItem in mListView.List.SelectedItems)
                {
                    ActionsFactory.AddActionsHandler(selectedItem, mContext);
                }
            }
            else
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
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
            ViewLinkedInstances.AutomationID = "HighlightElement";
            ViewLinkedInstances.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Pointer;
            ViewLinkedInstances.ToolTip = "Highlight Element";
            ViewLinkedInstances.OperationHandler = HighlightElementClicked;
            operationsList.Add(ViewLinkedInstances);

            return operationsList;
        }
        private void HighlightElementClicked(object sender, RoutedEventArgs e)
        {
            if (!ValidateDriverAvalability())
            {
                return;
            }

            if (mSelectedElement != null)
            {
                mWinExplorer.HighLightElement(mSelectedElement, true);
            }
        }

        private bool ValidateDriverAvalability()
        {
            if (mWinExplorer == null)
            {
                Reporter.ToUser(eUserMsgKey.POMAgentIsNotRunning);
                return false;
            }

            if (IsDriverBusy())
            {
                Reporter.ToUser(eUserMsgKey.POMDriverIsBusy);
                return false;
            }

            return true;
        }
        private bool IsDriverBusy()
        {
            if (mAgent != null && mAgent.Driver.IsDriverBusy)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        ElementInfo mSelectedElement
        {
            get
            {
                if (mListView.List.SelectedItem != null)
                {
                    return (ElementInfo)mListView.List.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }

        IWindowExplorer mWinExplorer
        {
            get
            {
                if (mAgent != null && mAgent.Status == Agent.eStatus.Running)
                {
                    return mAgent.Driver as IWindowExplorer;
                }
                else
                {
                    if (mAgent != null)
                    {
                        mAgent.Close();
                    }
                    return null;
                }
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
