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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.ApiModelsFolder;
using Ginger.BusinessFlowPages.ListHelpers;
using Ginger.BusinessFlowPages_New.AddActionMenu;
using Ginger.Repository;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Drivers.Common;
using GingerCore.GeneralLib;
using GingerCore.Platforms.PlatformsInfo;
using GingerWPF.DragDropLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
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
        Ginger.General.eRIPageViewMode mPageViewMode;

        ActionsListViewHelper mActionsListHelper;
        UcListView mActionsListView;
        ActionEditPage mActionEditPage;

        public UcListView ListView
        {
            get { return mActionsListView; }
        }

        public ActionsListViewPage(Activity Activity, Context context, Ginger.General.eRIPageViewMode pageViewMode)
        {
            InitializeComponent();

            mActivity = Activity;
            mContext = context;
            mPageViewMode = pageViewMode;

            SetListView();
            SetSharedRepositoryMark();
            ShowHideEditPage(null);
        }

        private void ShowHideEditPage(Act actionToEdit)
        {
            if (actionToEdit != null)
            {
                xBackToListPnl.Visibility = Visibility.Visible;
                BindingHandler.ObjFieldBinding(xSelectedItemTitleText, Label.ContentProperty, actionToEdit, nameof(Act.Description));
                BindingHandler.ObjFieldBinding(xSelectedItemTitleText, Label.ToolTipProperty, actionToEdit, nameof(Act.Description));
                mActionEditPage = new ActionEditPage(actionToEdit, Ginger.General.eRIPageViewMode.Automation);//need to pass Context?
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

            mActionsListHelper = new ActionsListViewHelper(mContext, mPageViewMode);
            mActionsListHelper.ActionListItemEvent += MActionListItemInfo_ActionListItemEvent;
            mActionsListView.SetDefaultListDataTemplate(mActionsListHelper);

            mActionsListView.ListSelectionMode = SelectionMode.Extended;
            mActionsListView.DataSourceList = mActivity.Acts;

            mActionsListView.PreviewDragItem += listActions_PreviewDragItem;
            mActionsListView.ItemDropped += listActions_ItemDropped;

            mActionsListView.List.MouseDoubleClick += ActionsListView_MouseDoubleClick;
        }

        private void ActionsListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (mActionsListView.CurrentItem != null)
            {
                (mActionsListView.CurrentItem as Act).Context = mContext;
                ShowHideEditPage((Act)mActionsListView.CurrentItem);
            }
        }

        private void MActionListItemInfo_ActionListItemEvent(ActionListItemEventArgs EventArgs)
        {
            switch (EventArgs.EventType)
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
                    SetSharedRepositoryMark();
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
            if (DragDrop2.DragInfo.DataIsAssignableToType(typeof(Act))
                || DragDrop2.DragInfo.DataIsAssignableToType(typeof(ApplicationPOMModel))
                    || DragDrop2.DragInfo.DataIsAssignableToType(typeof(ElementInfo))
                        || DragDrop2.DragInfo.DataIsAssignableToType(typeof(RepositoryFolder<ApplicationAPIModel>))
                            || DragDrop2.DragInfo.DataIsAssignableToType(typeof(ApplicationAPIModel)))
            {
                // OK to drop                         
                DragDrop2.DragInfo.DragIcon = GingerWPF.DragDropLib.DragInfo.eDragIcon.Copy;
            }
        }

        private void listActions_ItemDropped(object sender, EventArgs e)
        {
            object droppedItem = ((DragInfo)sender).Data as object;
            if (droppedItem != null)
            {
                ActionsFactory.AddActionsHandler(droppedItem, mContext);
            }
        }

        private void xGoToActionsList_Click(object sender, RoutedEventArgs e)
        {
            ShowHideEditPage(null);
        }

        private void SetSharedRepositoryMark()
        {
            ObservableList<Act> sharedActions = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>();
            SharedRepositoryOperations.MarkSharedRepositoryItems((IEnumerable<object>)mActivity.Acts, (IEnumerable<object>)sharedActions);
        }
    }
}
