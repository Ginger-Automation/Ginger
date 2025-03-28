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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger;
using Ginger.Actions;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowPages.ListHelpers;
using Ginger.BusinessFlowWindows;
using Ginger.Repository;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using GingerCore.Actions;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.DragDropLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

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
        Act mActionBeenEdit;

        ActionsListViewHelper mActionsListHelper;
        UcListView mActionsListView;
        ActionEditPage mActionEditPage;

        public event RoutedEventHandler ShiftToActionEditEvent;
        public event RoutedEventHandler ShiftToActionsListEvent;

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
            ShowHideEditPage(null);
        }

        public void UpdatePageViewMode(Ginger.General.eRIPageViewMode pageViewMode)
        {
            mPageViewMode = pageViewMode;
            SetListView();
            ShowHideEditPage(null);
        }

        private void ShowHideEditPage(Act actionToEdit)
        {
            if (actionToEdit != null)
            {
                xBackToListGrid.Visibility = Visibility.Visible;
                mActionBeenEdit = actionToEdit;
                mActionBeenEdit.Context = mContext;
                BindingHandler.ObjFieldBinding(xSelectedItemTitleText, TextBlock.TextProperty, mActionBeenEdit, nameof(Act.Description));
                BindingHandler.ObjFieldBinding(xSelectedItemTitleText, TextBlock.ToolTipProperty, mActionBeenEdit, nameof(Act.Description));
                if (mPageViewMode == Ginger.General.eRIPageViewMode.View)
                {
                    xEditOperationsPnl.Visibility = Visibility.Collapsed;
                    xRunOperationsPnl.Visibility = Visibility.Collapsed;
                }
                else if (mPageViewMode == Ginger.General.eRIPageViewMode.ViewAndExecute)
                {
                    xEditOperationsPnl.Visibility = Visibility.Collapsed;
                    xRunOperationsPnl.Visibility = Visibility.Visible;
                }
                else
                {
                    xEditOperationsPnl.Visibility = Visibility.Visible;
                    xRunOperationsPnl.Visibility = Visibility.Visible;
                    mActionBeenEdit.SaveBackup();
                    BindingHandler.ObjFieldBinding(xActiveBtn, ucButton.ButtonImageTypeProperty, mActionBeenEdit, nameof(Act.Active), bindingConvertor: new ActiveImageTypeConverter(), BindingMode.OneWay);
                    BindingHandler.ObjFieldBinding(xBreakPointMenuItemIcon, ImageMaker.ContentProperty, mActionBeenEdit, nameof(Act.BreakPoint), bindingConvertor: new ActiveImageTypeConverter(), BindingMode.OneWay);
                }

                if (mActionEditPage == null)
                {
                    mActionEditPage = new ActionEditPage(mActionBeenEdit, mPageViewMode);
                }
                else
                {
                    mActionEditPage.Init(mActionBeenEdit, mPageViewMode);
                }

                xMainFrame.ClearAndSetContent(mActionEditPage);
                if (ShiftToActionEditEvent != null)
                {
                    ShiftToActionEditEvent.Invoke(this, null);
                }
            }
            else
            {
                xBackToListGrid.Visibility = Visibility.Collapsed;
                mActionBeenEdit = null;
                if (mActionEditPage != null)
                {
                    //mActionEditPage.ClearPageBindings();
                    mActionEditPage.Clear();
                    //GC.Collect();
                }
                xMainFrame.ClearAndSetContent(mActionsListView);
                mActionsListView.ScrollToViewCurrentItem();

                if (ShiftToActionsListEvent != null)
                {
                    ShiftToActionsListEvent.Invoke(this, null);
                }
            }
        }

        //private void ClearListViewBindings()
        //{
        //    if (mActionsListHelper != null)
        //    {
        //        mActionsListHelper.ActionListItemEvent -= MActionListItemInfo_ActionListItemEvent;
        //        mActionsListHelper = null;
        //    }

        //    if (mActionsListView != null)
        //    {
        //        mActionsListView.PreviewDragItem -= listActions_PreviewDragItem;
        //        mActionsListView.ItemDropped -= listActions_ItemDropped;
        //        mActionsListView.List.MouseDoubleClick -= ActionsListView_MouseDoubleClick;
        //        mActionsListView.ClearBindings();
        //        mActionsListView.DataSourceList = null;
        //        mActionsListView = null;
        //    }
        //}

        //public void ClearBindings()
        //{
        //    xMainFrame.Content = null;
        //    xMainFrame.NavigationService.RemoveBackEntry();

        //    ClearListViewBindings();

        //    BindingOperations.ClearAllBindings(xSelectedItemTitleText);
        //    BindingOperations.ClearAllBindings(xActiveBtn);
        //    BindingOperations.ClearAllBindings(xBreakPointMenuItemIcon);
        //    this.ClearControlsBindings();

        //}

        private void SetListView()
        {
            if (mActionsListView == null)
            {
                mActionsListView = new UcListView
                {
                    Title = "Actions",
                    ListImageType = Amdocs.Ginger.Common.Enums.eImageType.Action
                };

                mActionsListHelper = new ActionsListViewHelper(mContext, mPageViewMode);
                mActionsListHelper.ActionListItemEvent += MActionListItemInfo_ActionListItemEvent;

                mActionsListView.SetDefaultListDataTemplate(mActionsListHelper);

                mActionsListView.ListSelectionMode = SelectionMode.Extended;

                mActionsListView.PreviewDragItem += listActions_PreviewDragItem;
                mActionsListView.ItemDropped += listActions_ItemDropped;
                WeakEventManager<Control, MouseButtonEventArgs>.AddHandler(source: mActionsListView.List, eventName: nameof(Control.MouseDoubleClick), handler: ActionsListView_MouseDoubleClick);

                // Enable Virtualization for Actions ListView to improve the loading time/performance
                mActionsListView.List.SetValue(ScrollViewer.CanContentScrollProperty, true);

            }
            else
            {
                mActionsListHelper.UpdatePageViewMode(mPageViewMode);
                mActionsListView.SetDefaultListDataTemplate(mActionsListHelper);
                if (mActionsListHelper.Context != null && mActivity != mActionsListHelper.Context.Activity)
                {
                    UpdateActivity(mActionsListHelper.Context.Activity);
                }
            }

            if (mPageViewMode is Ginger.General.eRIPageViewMode.View or Ginger.General.eRIPageViewMode.ViewAndExecute)
            {
                mActionsListView.IsDragDropCompatible = false;
            }
            else
            {
                mActionsListView.IsDragDropCompatible = true;
            }

            if (mActivity != null)
            {
                //update actions platform
                Task.Run(() =>
                {
                    ePlatformType platform = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(x => x.AppName == mContext.Activity.TargetApplication).Platform;
                    foreach (Act act in mActivity.Acts)
                    {
                        if (act is ActWithoutDriver)
                        {
                            act.Platform = ePlatformType.NA;
                        }
                        else
                        {
                            act.Platform = platform;
                        }
                    }
                });
                mActionsListView.DataSourceList = mActivity.Acts;
                SetSharedRepositoryMark();
            }
            else
            {
                mActionsListView.DataSourceList = null;
            }
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
                SetListView();
            }
            ShowHideEditPage(null);
        }

        // Drag Drop handlers
        private void listActions_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DrgInfo.DataIsAssignableToType(typeof(Act), true)
               || DragDrop2.DrgInfo.DataIsAssignableToType(typeof(ApplicationPOMModel), true)
                   || DragDrop2.DrgInfo.DataIsAssignableToType(typeof(ElementInfo), true)
                       || DragDrop2.DrgInfo.DataIsAssignableToType(typeof(RepositoryFolder<ApplicationAPIModel>), true)
                           || DragDrop2.DrgInfo.DataIsAssignableToType(typeof(ApplicationAPIModel), true))
            {
                // OK to drop
                if (DragDrop2.DrgInfo.Data is ObservableList<RepositoryItemBase>)
                {
                    DragDrop2.SetDragIcon(true, true);
                }
                else
                {
                    DragDrop2.SetDragIcon(true);
                }
            }
            else
            {
                // Do Not Drop
                DragDrop2.SetDragIcon(false);
            }
        }

        private void listActions_ItemDropped(object sender, EventArgs e)
        {
            if (((DragInfo)sender).Data is object droppedItem)
            {
                int mouseIndex = -1;

                if (DragDrop2.GetRepositoryItemHit(ListView) is Act actDroppedOn)
                {
                    mouseIndex = ListView.DataSourceList.IndexOf(actDroppedOn);
                }
                DroppedItemHandler(droppedItem, mouseIndex);
            }
        }

        private void DroppedItemHandler(object droppedItem, int mouseIndex)
        {
            int lastAddedIndex = -1;

            lastAddedIndex = ActionsFactory.AddActionsHandler(droppedItem, mContext, mouseIndex);

            if (lastAddedIndex > mouseIndex)
            {
                ListView.xListView.SelectedItems.Clear();
                for (int itemIndex = mouseIndex; itemIndex < lastAddedIndex; itemIndex++)
                {
                    if (ListView.DataSourceList[itemIndex] is RepositoryItemBase repoBaseItem)
                    {
                        ListView.xListView.SelectedItems.Add(repoBaseItem);
                    }
                }
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

        private void XUndoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Ginger.General.UndoChangesInRepositoryItem(mActionBeenEdit, isLocalBackup: true, clearBackup: false))
            {
                //mActionBeenEdit.SaveBackup();
                int selectedTabIndx = mActionEditPage.SelectedTabIndx;
                ShowHideEditPage(mActionBeenEdit);
                mActionEditPage.SelectedTabIndx = selectedTabIndx;
            }
        }

        private void xPreviousActionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mActionsListView.List.Items.CurrentPosition >= 1)
            {
                int currentSelectedTabIndx = mActionEditPage.SelectedTabIndx;
                mActionsListView.List.Items.MoveCurrentToPrevious();
                ShowHideEditPage((Act)mActionsListView.List.Items.CurrentItem);
                mActionEditPage.SelectedTabIndx = currentSelectedTabIndx;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "No Action to move to.");
            }
        }

        private void xNextActionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mActionsListView.List.Items.CurrentPosition < mActionsListView.List.Items.Count - 1)
            {
                int currentSelectedTabIndx = mActionEditPage.SelectedTabIndx;
                mActionsListView.List.Items.MoveCurrentToNext();
                ShowHideEditPage((Act)mActionsListView.List.Items.CurrentItem);
                mActionEditPage.SelectedTabIndx = currentSelectedTabIndx;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "No Action to move to.");
            }
        }

        private void xRunActionBtn_Click(object sender, RoutedEventArgs e)
        {
            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.RunCurrentAction, new Tuple<Activity, Act, bool>(mActivity, mActionBeenEdit, false));
        }

        public void SetUIElementsBehaverBasedOnRunnerStatus(bool IsRunning)
        {
            Dispatcher.Invoke(() =>
            {
                xUndoBtn.IsEnabled = IsRunning;
                xRunActionBtn.IsEnabled = IsRunning;
            });
        }

        private void xDeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.SureWantToDelete, mActionBeenEdit.Description) == eUserMsgSelection.Yes)
            {
                mActivity.Acts.Remove(mActionBeenEdit);
                if (mActionsListView.List.Items.CurrentItem != null)
                {
                    ShowHideEditPage((Act)mActionsListView.List.Items.CurrentItem);
                }
                else
                {
                    ShowHideEditPage(null);
                }
            }
        }

        private void xActiveBtn_Click(object sender, RoutedEventArgs e)
        {
            mActionBeenEdit.Active = !mActionBeenEdit.Active;
        }

        private void xBreakPointMenuItem_Click(object sender, RoutedEventArgs e)
        {
            mActionBeenEdit.BreakPoint = !mActionBeenEdit.BreakPoint;
        }

        private void xResetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            mActionBeenEdit.Reset();
        }

        private void RunBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((ucButton)sender).ButtonImageForground = (SolidColorBrush)FindResource("$HighlightColor_LightBlue");
        }

        private void RunBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((ucButton)sender).ButtonImageForground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
        }

        private void xExpandActionBtn_Click(object sender, RoutedEventArgs e)
        {
            mActionEditPage.ShowAsWindow(windowStyle: eWindowShowStyle.OnlyDialog);
            mActionEditPage.Width = xMainFrame.ActualWidth;
            mActionEditPage.HorizontalAlignment = HorizontalAlignment.Stretch;
            xMainFrame.Refresh();
        }

        private void xMainFrame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (mActionEditPage != null)
            {
                mActionEditPage.Width = xMainFrame.ActualWidth;
                mActionEditPage.HorizontalAlignment = HorizontalAlignment.Stretch;
                //commenting below line for #25193 bug-fix, this line is preventing mActionsListView from showing up when user clicks on
                //edit button for a linked-instance activity
                //xMainFrame.Refresh();
            }
        }
    }
}
