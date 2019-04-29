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
using Ginger.Actions;
using Ginger.BusinessFlowPages.ListViewItems;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using GingerCore.Actions;
using GingerWPF.DragDropLib;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.BusinessFlowsLib
{
    /// <summary>
    /// Interaction logic for ActivityActionsPage.xaml
    /// </summary>
    public partial class ActionsListViewPage : Page
    {
        Activity mActivity;
        Context mContext;

        ActionListItemInfo mActionListItemInfo;
        UcListView mActionsListView;
        ActionEditPage mActionEditPage;

        public UcListView ListView
        {
            get { return mActionsListView; }
        }

        public ActionsListViewPage(Activity Activity, Context context)
        {
            InitializeComponent();

            mActivity = Activity;
            mContext = context;

            SetListView();
            ShowHideEditPage(null);
        }

        private void ShowHideEditPage(Act actionToEdit)
        {
            if (actionToEdit != null)
            {
                xBackToListPnl.Visibility = Visibility.Visible;
                mActionEditPage = new ActionEditPage(actionToEdit, Ginger.General.RepositoryItemPageViewMode.Automation);//need to pass Context?
                xMainFrame.Content = mActionEditPage;
            }
            else
            {
                xBackToListPnl.Visibility = Visibility.Collapsed;
                mActionEditPage = null;
                xMainFrame.Content = mActionsListView;
            }
        }

        private void SetListView() 
        {
            mActionsListView = new UcListView();
            mActionsListView.Title = "Actions";
            mActionsListView.ListImageType = Amdocs.Ginger.Common.Enums.eImageType.Action;

            mActionListItemInfo = new ActionListItemInfo(mContext);
            mActionListItemInfo.ActionListItemEvent += MActionListItemInfo_ActionListItemEvent;
            mActionsListView.SetDefaultListDataTemplate(mActionListItemInfo);

            mActionsListView.AddBtnVisiblity = Visibility.Collapsed;

            mActionsListView.DataSourceList = mActivity.Acts;

            mActionsListView.PreviewDragItem += listActions_PreviewDragItem;
            mActionsListView.ItemDropped += listActions_ItemDropped;

            mActionsListView.List.MouseDoubleClick += ActionsListView_MouseDoubleClick;
        }

        private void ActionsListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (mActionsListView.CurrentItem != null)
            {
                ShowHideEditPage((Act)mActionsListView.CurrentItem);
            }
        }

        private void MActionListItemInfo_ActionListItemEvent(ActionListItemEventArgs EventArgs)
        {
           switch(EventArgs.EventType)
            {
                case ActionListItemEventArgs.eEventType.ShowActionEditPage:
                    ShowHideEditPage((Act)EventArgs.EventObject);
                    break;
            }
        }

        public void UpdateActivity(Activity activity)
        {
            if (mActivity != activity)
            {
                mActivity = activity;
                if (mActivity != null)
                {
                    mActionsListView.DataSourceList = mActivity.Acts;
                }
                else
                {
                    mActionsListView.DataSourceList = null;
                }
                ShowHideEditPage(null);
            }
        }

        // Drag Drop handlers
        private void listActions_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DragInfo.DataIsAssignableToType(typeof(Act)))
            {
                // OK to drop                         
                DragDrop2.DragInfo.DragIcon = GingerWPF.DragDropLib.DragInfo.eDragIcon.Copy;
            }
        }

        private void listActions_ItemDropped(object sender, EventArgs e)
        {
            Act a = (Act)((DragInfo)sender).Data;
            Act instance = (Act)a.CreateInstance(true);
            mActivity.Acts.Add(instance);

            int selectedActIndex = -1;
            if (mActivity.Acts.CurrentItem != null)
            {
                selectedActIndex = mActivity.Acts.IndexOf((Act)mActivity.Acts.CurrentItem);
            }
            if (selectedActIndex >= 0)
            {
                mActivity.Acts.Move(mActivity.Acts.Count - 1, selectedActIndex + 1);
            }
        }

        private void xGoToActionsList_Click(object sender, RoutedEventArgs e)
        {
            ShowHideEditPage(null);
        }
    }
}
