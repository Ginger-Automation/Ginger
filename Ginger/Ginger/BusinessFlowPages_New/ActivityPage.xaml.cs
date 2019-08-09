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
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowWindows;
using Ginger.Repository;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using GingerCore.Actions;
using GingerCore.GeneralLib;
using GingerCore.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GingerWPF.BusinessFlowsLib
{
    /// <summary>
    /// Interaction logic for ActivityPage.xaml
    /// </summary>
    public partial class ActivityPage : Page
    {
        Activity mActivity;
        Context mContext;
        Ginger.General.eRIPageViewMode mPageViewMode;

        ActionsListViewPage mActionsPage;
        VariabelsListViewPage mVariabelsPage;
        ActivityConfigurationsPage mConfigurationsPage;

        GenericWindow mGenericWin = null;

        public UcListView ActionListView
        {
            get { return mActionsPage.ListView; }
        }

        // We keep a static page so even if we move between activities the Run controls and info stay the same
        public ActivityPage(Activity activity, Context context, Ginger.General.eRIPageViewMode pageViewMode)
        {
            InitializeComponent();

            mActivity = activity;
            mContext = context;
            mPageViewMode = pageViewMode;

            SetUIView();
            BindControlsToActivity();
        }

        private void SetUIView()
        {
            if (mPageViewMode != Ginger.General.eRIPageViewMode.Automation)
            {
                xOperationsPnl.Visibility = Visibility.Collapsed;
            }

            if (mPageViewMode == Ginger.General.eRIPageViewMode.SharedReposiotry)
            {
                xUploadToShareRepoMenuItem.Visibility = Visibility.Collapsed;
                xSharedRepoInstanceUC.Visibility = Visibility.Collapsed;
            }

            if (mPageViewMode == Ginger.General.eRIPageViewMode.View)
            {
                xSharedRepoInstanceUC.IsEnabled = false;
            }

            xRunBtn.ButtonText = GingerDicser.GetTermResValue(eTermResKey.Activity, "Run");
        }

        private void BindControlsToActivity()
        {
            if (mPageViewMode != Ginger.General.eRIPageViewMode.View)
            {
                mActivity.SaveBackup();
            }

            //General Info Section Bindings
            BindingHandler.ObjFieldBinding(xNameTextBlock, TextBlock.TextProperty, mActivity, nameof(Activity.ActivityName));
            BindingHandler.ObjFieldBinding(xNameTextBlock, TextBlock.ToolTipProperty, mActivity, nameof(Activity.ActivityName));
            mActivity.PropertyChanged -= mActivity_PropertyChanged;
            mActivity.PropertyChanged += mActivity_PropertyChanged;
            UpdateDescription();
            xSharedRepoInstanceUC.Init(mActivity, mContext.BusinessFlow);

            //Actions Tab Bindings    
            mActivity.Acts.CollectionChanged -= Acts_CollectionChanged;
            mActivity.Acts.CollectionChanged += Acts_CollectionChanged;
            UpdateActionsTabHeader();
            if (mActionsPage != null && xActionsTab.IsSelected)
            {
                mActionsPage.UpdateActivity(mActivity);
            }

            //Variables Tab Bindings   
            mActivity.Variables.CollectionChanged -= Variables_CollectionChanged;
            mActivity.Variables.CollectionChanged += Variables_CollectionChanged;
            UpdateVariabelsTabHeader();
            if (mVariabelsPage != null && xVariablesTab.IsSelected)
            {
                mVariabelsPage.UpdateParent(mActivity);
            }

            //Configurations Tab Bindings
            if (mConfigurationsPage != null && xConfigurationsTab.IsSelected)
            {
                mConfigurationsPage.UpdateActivity(mActivity);
            }
        }

        public void UpdateActivity(Activity activity)
        {
            if (mActivity != activity)
            {
                ClearActivityBindings();
                mActivity = activity;
                if (mActivity != null)
                {
                    BindControlsToActivity();
                }
            }
        }

        TabItem mLastSelectedTab = null;
        private void XItemsTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xItemsTabs.SelectedItem != mLastSelectedTab)
            {
                if (xActionsTab.IsSelected == true)
                {
                    if (mActionsPage == null)
                    {
                        mActionsPage = new ActionsListViewPage(mActivity, mContext, mPageViewMode);
                        if (mActionsPage.ListView != null)
                        {
                            mActionsPage.ListView.ListTitleVisibility = Visibility.Collapsed;
                        }
                        mActionsPage.ShiftToActionEditEvent += MActionsPage_ShiftToActionEditEvent;
                        mActionsPage.ShiftToActionsListEvent += MActionsPage_ShiftToActionsListEvent;
                        xActionsTabFrame.SetContent(mActionsPage);
                    }
                    else
                    {
                        mActionsPage.UpdateActivity(mActivity);
                    }
                }
                else if (xVariablesTab.IsSelected == true)
                {
                    if (mVariabelsPage == null)
                    {
                        mVariabelsPage = new VariabelsListViewPage(mActivity, mContext, mPageViewMode);
                        if (mVariabelsPage.ListView != null)
                        {
                            mVariabelsPage.ListView.ListTitleVisibility = Visibility.Collapsed;
                        }
                        xVariabelsTabFrame.SetContent(mVariabelsPage);
                    }
                    else
                    {
                        mVariabelsPage.UpdateParent(mActivity);
                    }
                }
                else if (xConfigurationsTab.IsSelected == true)
                {
                    if (mConfigurationsPage == null)
                    {
                        mConfigurationsPage = new ActivityConfigurationsPage(mActivity, mContext, mPageViewMode);
                        xConfigurationsFrame.SetContent(mConfigurationsPage);
                    }
                    else
                    {
                        mConfigurationsPage.UpdateActivity(mActivity);
                    }
                }

                mLastSelectedTab = (TabItem)xItemsTabs.SelectedItem;
            }
        }

        private void MActionsPage_ShiftToActionsListEvent(object sender, RoutedEventArgs e)
        {
            if (mPageViewMode == Ginger.General.eRIPageViewMode.Automation)
            {
                xRunSelectedActionBtn.Visibility = Visibility.Visible;
            }
        }

        private void MActionsPage_ShiftToActionEditEvent(object sender, RoutedEventArgs e)
        {
            if (mPageViewMode == Ginger.General.eRIPageViewMode.Automation)
            {
                xRunSelectedActionBtn.Visibility = Visibility.Collapsed;
            }
        }


        private void ClearActivityBindings()
        {
            mActivity.PropertyChanged -= mActivity_PropertyChanged;
            mActivity.Acts.CollectionChanged -= Acts_CollectionChanged;
            mActivity.Variables.CollectionChanged -= Variables_CollectionChanged;

            if (mActivity != null && mPageViewMode != Ginger.General.eRIPageViewMode.View)
            {
                mActivity.ClearBackup();
            }
        }

        //public void ClearBindings()
        //{
        //    //clean Tabs
        //    xActionsTabFrame.Content = null;
        //    xActionsTabFrame.NavigationService.RemoveBackEntry();
        //    xVariabelsTabFrame.Content = null;
        //    xVariabelsTabFrame.NavigationService.RemoveBackEntry();
        //    xConfigurationsFrame.Content = null;
        //    xConfigurationsFrame.NavigationService.RemoveBackEntry();

        //    if (mActivity != null)
        //    {
        //        ClearActivityBindings();
        //    }

        //    this.ClearControlsBindings();
        //    BindingOperations.ClearAllBindings(xNameTextBlock);

        //    if (mActionsPage != null)
        //    {
        //        mActionsPage.ShiftToActionEditEvent -= MActionsPage_ShiftToActionEditEvent;
        //        mActionsPage.ShiftToActionsListEvent -= MActionsPage_ShiftToActionsListEvent;
        //        mActionsPage.ClearBindings();
        //        mActionsPage.KeepAlive = false;
        //        mActionsPage = null;
        //    }

        //    if (mVariabelsPage != null)
        //    {
        //        mVariabelsPage.ClearBindings();
        //        mVariabelsPage.KeepAlive = false;
        //        mVariabelsPage = null;
        //    }

        //    if (mConfigurationsPage != null)
        //    {
        //        mConfigurationsPage.ClearBindings();
        //        mConfigurationsPage.KeepAlive = false;
        //        mConfigurationsPage = null;
        //    }
        //}
       
        private void mActivity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateDescription();
        }

        private void UpdateDescription()
        {
            this.Dispatcher.Invoke(() =>
            {
                xDescriptionTextBlock.Text = string.Empty;
                TextBlockHelper xDescTextBlockHelper = new TextBlockHelper(xDescriptionTextBlock);
                SolidColorBrush foregroundColor = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$Color_DarkBlue")).ToString());
                    if (mActivity != null)
                    {
                        //Application info
                        if (!string.IsNullOrEmpty(mActivity.Description))
                        {
                            if (mActivity.Description.Length > 100)
                            {
                                xDescTextBlockHelper.AddText("Description: " + mActivity.Description.Substring(0,99) + "...");
                            }
                            else
                            {
                                xDescTextBlockHelper.AddText("Description: " + mActivity.Description);
                            }
                            xDescTextBlockHelper.AddText(" " + Ginger.General.GetTagsListAsString(mActivity.Tags));
                            xDescTextBlockHelper.AddLineBreak();
                        }
                        //if (!string.IsNullOrEmpty(mActivity.RunDescription))
                        //{
                        //    xDescTextBlockHelper.AddText("Run Description: " + mActivity.RunDescription);
                        //    xDescTextBlockHelper.AddLineBreak();
                        //}
                        if (!string.IsNullOrEmpty(mActivity.ActivitiesGroupID))
                        {
                            xDescTextBlockHelper.AddText("Parent Group: " + mActivity.ActivitiesGroupID);
                            xDescTextBlockHelper.AddLineBreak();
                        }
                        xDescTextBlockHelper.AddText("Target: " + mActivity.TargetApplication);
                    }
            });
        
        }

        private void xUploadToShareRepoMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            List<RepositoryItemBase> list = new List<RepositoryItemBase>();
            list.Add(mActivity);
            (new Ginger.Repository.SharedRepositoryOperations()).AddItemsToRepository(mContext, list);
        }

        private void xRunBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {           
            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.RunCurrentActivity, mActivity);
        }

        private void xContinueRunBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {            
            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.ContinueActivityRun, mActivity);
        }

        private void Acts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateActionsTabHeader();
        }

        private void Variables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateVariabelsTabHeader();
        }

        private void UpdateVariabelsTabHeader()
        {
            this.Dispatcher.Invoke(() =>
            {
                xVariabelsTabHeaderText.Text = string.Format("{0} ({1})", GingerDicser.GetTermResValue(eTermResKey.Variables), mActivity.Variables.Count);
            });
        }
        private void UpdateActionsTabHeader()
        {
            this.Dispatcher.Invoke(() =>
            {
                xActionsTabHeaderText.Text = string.Format("Actions ({0})", mActivity.Acts.Count);
            });
        }

        private void xRunActionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mActionsPage.ListView.CurrentItem != null)
            {
                App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.RunCurrentAction, new Tuple<Activity, Act>(mActivity, (Act)mActionsPage.ListView.CurrentItem));
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        private void xResetBtn_Click(object sender, RoutedEventArgs e)
        {
            mActivity.Reset();
        }

        private void xResetRestMenuItem_Click(object sender, RoutedEventArgs e)
        {
            for (int indx = mContext.BusinessFlow.Activities.IndexOf(mActivity); indx < mContext.BusinessFlow.Activities.Count; indx++)
            {
                mContext.BusinessFlow.Activities[indx].Reset();
            }
        }

        bool mSaveWasDone = false;
        public bool ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset = false)
        {
            string title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.Activity);

            ObservableList<Button> winButtons = new ObservableList<Button>();

            RoutedEventHandler CloseHandler = CloseWinClicked;
            string closeContent = "Undo & Close";

            Button okBtn = new Button();
            okBtn.Content = "Ok";            
            okBtn.Click += new RoutedEventHandler(OkBtn_Click);

            Button undoBtn = new Button();
            undoBtn.Content = "Undo & Close";
            undoBtn.Click += new RoutedEventHandler(UndoBtn_Click);

            Button saveBtn = new Button();
            saveBtn.Content = "Save";

            switch (mPageViewMode)
            {
                case Ginger.General.eRIPageViewMode.SharedReposiotry:
                    mActivity.SaveBackup();
                    title = "Edit Shared Repository " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                    saveBtn.Click += new RoutedEventHandler(SharedRepoSaveBtn_Click);
                    winButtons.Add(saveBtn);
                    winButtons.Add(undoBtn);
                    break;

                case Ginger.General.eRIPageViewMode.ChildWithSave:
                    mActivity.SaveBackup();
                    title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                    saveBtn.Click += new RoutedEventHandler(ParentItemSaveButton_Click);
                    winButtons.Add(saveBtn);
                    winButtons.Add(undoBtn);
                    break;

                case Ginger.General.eRIPageViewMode.View:
                    title = "View " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                    winButtons.Add(okBtn);
                    CloseHandler = new RoutedEventHandler(OkBtn_Click);
                    closeContent = okBtn.Content.ToString();                    
                    break;
            }

            this.Height = 800;
            this.Width = 1000;

            GingerCore.General.LoadGenericWindow(ref mGenericWin, App.MainWindow, windowStyle, title, this, winButtons, false, closeBtnText: closeContent, closeEventHandler: CloseHandler, startupLocationWithOffset: startupLocationWithOffset);

            return mSaveWasDone;
        }

        private void CloseWinClicked(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.AskIfToUndoChanges) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                UndoChangesAndClose();
            }
        }

        private void UndoChangesAndClose()
        {
            Ginger.General.UndoChangesInRepositoryItem(mActivity, true);

            mGenericWin.Close();
        }

        private void ParentItemSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (mContext.BusinessFlow != null && Reporter.ToUser(eUserMsgKey.SaveItemParentWarning) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mContext.BusinessFlow);
                mSaveWasDone = true;
            }
            mGenericWin.Close();
        }

        private void UndoBtn_Click(object sender, RoutedEventArgs e)
        {
            UndoChangesAndClose();
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            //OKButtonClicked = true;
            mGenericWin.Close();
        }

        private void SharedRepoSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mPageViewMode == Ginger.General.eRIPageViewMode.SharedReposiotry)
            {
                if (SharedRepositoryOperations.CheckIfSureDoingChange(mActivity, "change") == true)
                {
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mActivity);
                    mSaveWasDone = true;
                    mGenericWin.Close();
                }
            }
        }

        private void xUndoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Ginger.General.UndoChangesInRepositoryItem(mActivity, true))
            {
                mActivity.SaveBackup();
            }
        }

        private void RunBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((ucButton)sender).ButtonImageForground = (SolidColorBrush)FindResource("$SelectionColor_LightBlue");
        }

        private void RunBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((ucButton)sender).ButtonImageForground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
        }
    }
}
