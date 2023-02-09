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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using GingerCore.Actions;
using System.Collections.Generic;
using System.Windows;

namespace Ginger.BusinessFlowPages.ListHelpers
{
    public class ElementInfoListViewHelper : IListViewHelper
    {
        ElementInfo mElementInfo;
        
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

        Context mContext;
        public Context Context
        {
            get
            {
                return mContext;
            }
        }

        public bool AllowExpandItems { get; set; } = true;

        public bool ExpandItemOnLoad { get; set; } = false;

        public ElementInfoListViewHelper(Context context, General.eRIPageViewMode pageViewMode)
        {
            mContext = context;
            PageViewMode = pageViewMode;
        }

        public void SetItem(object item)
        {
            if (item is ElementInfo)
            {
                mElementInfo = (ElementInfo)item;
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
            return nameof(ElementInfo.Description);
        }

        public string GetItemErrorField()
        {
            return null;
        }

        public string GetItemNameExtentionField()
        {
            return nameof(ElementInfo.ElementTypeEnum);
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
            return null;
        }

        public string GetItemIconField()
        {
            return nameof(ElementInfo.ElementTypeImage);
        }

        public string GetItemIconTooltipField()
        {
            return nameof(ElementInfo.ElementTypeEnum);
        }

        public List<ListItemOperation> GetListOperations()
        {
            List<ListItemOperation> operationsList = new List<ListItemOperation>();

            ListItemOperation addToFlow = new ListItemOperation();
            addToFlow.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.AddFromModel };
            addToFlow.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MoveLeft;
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
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
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
            ViewLinkedInstances.SupportedViews = new List<General.eRIPageViewMode>() { General.eRIPageViewMode.AddFromModel };
            ViewLinkedInstances.AutomationID = "HighlightElement";
            ViewLinkedInstances.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Visible;
            ViewLinkedInstances.ToolTip = "Highlight Element";
            ViewLinkedInstances.OperationHandler = HighlightElementClicked;
            operationsList.Add(ViewLinkedInstances);

            return operationsList;
        }

        private void HighlightElementClicked(object sender, RoutedEventArgs e)
        {
            SetItem(sender);
            App.OnAutomateBusinessFlowEvent(BusinessFlowWindows.AutomateEventArgs.eEventType.HighlightElement, mElementInfo);
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
