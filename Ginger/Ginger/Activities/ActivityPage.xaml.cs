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
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowWindows;
using Ginger.Repository;
using Ginger.Repository.AddItemToRepositoryWizard;
using Ginger.SolutionWindows;
using Ginger.UserControlsLib;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using GingerCore.Actions;
using GingerCore.GeneralLib;
using GingerCore.Helpers;
using GingerWPF.WizardLib;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace GingerWPF.BusinessFlowsLib
{
    /// <summary>
    /// Interaction logic for ActivityPage.xaml
    /// </summary>
    public partial class ActivityPage : GingerUIPage
    {
        Activity mActivity;
        public Activity Activity { get { return mActivity; } }

        Context mContext;
        Ginger.General.eRIPageViewMode mPageViewMode;

        ActionsListViewPage mActionsPage;
        VariabelsListViewPage mVariabelsPage;
        ActivityDetailsPage mConfigurationsPage;

        GenericWindow mGenericWin = null;

        public UcListView ActionListView
        {
            get { return mActionsPage.ListView; }
        }

        // We keep a static page so even if we move between activities the Run controls and info stay the same
        public ActivityPage(Activity activity, Context context, Ginger.General.eRIPageViewMode pageViewMode, bool highlightActivityName = false)
        {
            InitializeComponent();

            mActivity = activity;
            mContext = context;
            mPageViewMode = pageViewMode;

            SetUIView();
            BindControlsToActivity();

            if (highlightActivityName)
            {
                xNameTextBlock.Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
            }
        }

        private void UpdateActivityViewMode(Ginger.General.eRIPageViewMode pageViewMod)
        {
            mPageViewMode = pageViewMod;
            SetUIView();

        }

        private void SetIconImageType()
        {
            Dispatcher.Invoke(() =>
            {
                // Set the image glyph based on Activity
                xIconImage.ImageType = Activity.TargetApplicationPlatformImage;

                // Default tooltip value (fallback)
                string platformText = Activity.TargetApplicationPlatformName ?? string.Empty;

                try
                {
                    // Try to get the platform enum for the Activity target app and get its description attribute value
                    var ws = WorkSpace.Instance;
                    if (ws?.Solution != null && !string.IsNullOrEmpty(mActivity?.TargetApplication))
                    {
                        var platformEnum = ws.Solution.GetApplicationPlatformForTargetApp(mActivity.TargetApplication);
                        // Use the General helper to return the EnumValueDescription attribute when present
                        platformText = Amdocs.Ginger.Common.GeneralLib.General.GetEnumValueDescription(platformEnum.GetType(), platformEnum);
                    }
                }
                catch
                {
                    // keep fallback Activity.TargetApplicationPlatformName if anything goes wrong
                }

                // Set tooltip on the ImageMakerControl - ImageToolTip updates the inner image/font's ToolTip
                xIconImage.ImageToolTip = platformText;
                // Also set the UserControl ToolTip for safety/fallback
                xIconImage.ToolTip = platformText;
            });
        }

        public void SetUIElementsBehaverBasedOnRunnerStatus(bool IsRunning)
        {
            Dispatcher.Invoke(() =>
            {
                xUndoBtn.IsEnabled = IsRunning;
                xExtraOperationsMenu.IsEnabled = IsRunning;
                xContinueRunBtn.IsEnabled = IsRunning;
                xRunBtn.IsEnabled = IsRunning;
                xRunSelectedActionBtn.IsEnabled = IsRunning;
                xResetBtn.IsEnabled = IsRunning;
            });

            mActionsPage?.SetUIElementsBehaverBasedOnRunnerStatus(IsRunning);
        }

        private void SetUIView()
        {
            if (mPageViewMode == Ginger.General.eRIPageViewMode.Automation)
            {
                xOperationsPnl.Visibility = Visibility.Visible;
                xUndoBtn.Visibility = Visibility.Visible;
                xEditButton.Visibility = Visibility.Collapsed;
                if (mActivity.EnableEdit)
                {
                    xSaveButton.Visibility = Visibility.Visible;
                }
                else
                {
                    xSaveButton.Visibility = Visibility.Collapsed;
                }
                xUploadToShareRepoMenuItem.Visibility = Visibility.Visible;

            }
            else if (mPageViewMode == Ginger.General.eRIPageViewMode.ViewAndExecute)
            {
                xOperationsPnl.Visibility = Visibility.Visible;
                xUndoBtn.Visibility = Visibility.Collapsed;
                xEditButton.Visibility = Visibility.Visible;    //???            
                if (!mActivity.EnableEdit)
                {
                    xSaveButton.Visibility = Visibility.Collapsed;
                }
                xUploadToShareRepoMenuItem.Visibility = Visibility.Collapsed;
            }
            else if (mPageViewMode == Ginger.General.eRIPageViewMode.View)
            {
                xOperationsPnl.Visibility = Visibility.Collapsed;
                xUndoBtn.Visibility = Visibility.Collapsed;
                xEditButton.Visibility = Visibility.Collapsed;
                xSaveButton.Visibility = Visibility.Collapsed;
                xUploadToShareRepoMenuItem.Visibility = Visibility.Collapsed;
            }

            else if (mPageViewMode == Ginger.General.eRIPageViewMode.SharedReposiotry)
            {
                xOperationsPnl.Visibility = Visibility.Collapsed;
                xUndoBtn.Visibility = Visibility.Collapsed;
                xEditButton.Visibility = Visibility.Collapsed;
                xSaveButton.Visibility = Visibility.Collapsed;
                xUploadToShareRepoMenuItem.Visibility = Visibility.Collapsed;
                //xSharedRepoInstanceUC.Visibility = Visibility.Collapsed;
            }
            mActionsPage?.UpdatePageViewMode(mPageViewMode);
            mVariabelsPage?.UpdatePageViewMode(mPageViewMode);
            mConfigurationsPage?.UpdatePageViewMode(mPageViewMode, mActivity);

            //if (mPageViewMode == Ginger.General.eRIPageViewMode.View)
            //{
            //    xSharedRepoInstanceUC.IsEnabled = false;
            //}

            xRunBtn.ButtonText = GingerDicser.GetTermResValue(eTermResKey.Activity, "Run");
        }
        string allProperties = string.Empty;
        private void BindControlsToActivity()
        {
            if (mPageViewMode != Ginger.General.eRIPageViewMode.View && mPageViewMode != Ginger.General.eRIPageViewMode.ViewAndExecute && mActivity.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.NoChange)
            {
                mActivity.SaveBackup();
            }

            //General Info Section Bindings
            BindingHandler.ObjFieldBinding(xNameTextBlock, TextBlock.TextProperty, mActivity, nameof(Activity.ActivityName));
            BindingHandler.ObjFieldBinding(xNameTextBlock, TextBlock.ToolTipProperty, mActivity, nameof(Activity.ActivityName));
            PropertyChangedEventManager.RemoveHandler(source: mActivity, handler: mActivity_PropertyChanged, propertyName: allProperties);
            PropertyChangedEventManager.AddHandler(source: mActivity, handler: mActivity_PropertyChanged, propertyName: allProperties);



            SetIconImageType();
            UpdateDescription();
            //xSharedRepoInstanceUC.Init(mActivity, mContext.BusinessFlow);

            //Actions Tab Bindings    
            CollectionChangedEventManager.RemoveHandler(source: mActivity.Acts, handler: Acts_CollectionChanged);
            CollectionChangedEventManager.AddHandler(source: mActivity.Acts, handler: Acts_CollectionChanged);




            UpdateActionsTabHeader();
            if (mActionsPage != null && xActionsTab.IsSelected)
            {
                mActionsPage.UpdateActivity(mActivity);
            }

            //Variables Tab Bindings   
            CollectionChangedEventManager.RemoveHandler(source: mActivity.Variables, handler: Variables_CollectionChanged);
            CollectionChangedEventManager.AddHandler(source: mActivity.Variables, handler: Variables_CollectionChanged);





            UpdateVariabelsTabHeader();
            if (mVariabelsPage != null && xVariablesTab.IsSelected)
            {
                mVariabelsPage.UpdateParent(mActivity);
            }

            //Configurations Tab Bindings
            if (mConfigurationsPage != null && xDetailsTab.IsSelected)
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
                    if (mActivity.Type == eSharedItemType.Link)//Check if this is callled when opening activity from runset?
                    {
                        UpdateActivityViewMode(Ginger.General.eRIPageViewMode.ViewAndExecute);
                    }
                    else
                    {
                        UpdateActivityViewMode(Ginger.General.eRIPageViewMode.Automation);
                    }

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
                        WeakEventManager<ActionsListViewPage, RoutedEventArgs>.RemoveHandler(source: mActionsPage, eventName: nameof(ActionsListViewPage.ShiftToActionEditEvent), handler: MActionsPage_ShiftToActionEditEvent);
                        WeakEventManager<ActionsListViewPage, RoutedEventArgs>.AddHandler(source: mActionsPage, eventName: nameof(ActionsListViewPage.ShiftToActionsListEvent), handler: MActionsPage_ShiftToActionsListEvent);






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
                else if (xDetailsTab.IsSelected == true)
                {
                    if (mConfigurationsPage == null)
                    {
                        mConfigurationsPage = new ActivityDetailsPage(mActivity, mContext, mPageViewMode);
                        xConfigurationsFrame.SetContent(mConfigurationsPage);
                        TargetApplicationsPage.OnActivityUpdate += UpdateTargetApplication;
                    }
                    else
                    {
                        mConfigurationsPage.UpdateActivity(mActivity);
                    }
                }

                mLastSelectedTab = (TabItem)xItemsTabs.SelectedItem;
            }
        }

        private void UpdateTargetApplication()
        {

            if (mConfigurationsPage != null)
            {
                mConfigurationsPage.UpdateTargetApplication();
            }
        }

        private void MActionsPage_ShiftToActionsListEvent(object sender, RoutedEventArgs e)
        {
            if (mPageViewMode is Ginger.General.eRIPageViewMode.Automation or Ginger.General.eRIPageViewMode.ViewAndExecute)
            {
                xRunSelectedActionBtn.Visibility = Visibility.Visible;
            }
        }

        private void MActionsPage_ShiftToActionEditEvent(object sender, RoutedEventArgs e)
        {
            if (mPageViewMode is Ginger.General.eRIPageViewMode.Automation or Ginger.General.eRIPageViewMode.ViewAndExecute)
            {
                xRunSelectedActionBtn.Visibility = Visibility.Collapsed;
            }
        }


        private void ClearActivityBindings()
        {
            PropertyChangedEventManager.RemoveHandler(source: mActivity, handler: mActivity_PropertyChanged, propertyName: allProperties);
            CollectionChangedEventManager.RemoveHandler(source: mActivity.Acts, handler: Acts_CollectionChanged);
            CollectionChangedEventManager.RemoveHandler(source: mActivity.Variables, handler: Variables_CollectionChanged);

            if (mActivity != null && mPageViewMode != Ginger.General.eRIPageViewMode.View && mPageViewMode != Ginger.General.eRIPageViewMode.ViewAndExecute && mActivity.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.NoChange)
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
            SetIconImageType();
            UpdateDescription();
        }

        private void UpdateDescription()
        {
            this.Dispatcher.Invoke(() =>
            {
                xDescriptionTextBlock.Text = string.Empty;
                TextBlockHelper xDescTextBlockHelper = new TextBlockHelper(xDescriptionTextBlock);
                SolidColorBrush foregroundColor = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$PrimaryColor_Black")).ToString());
                if (mActivity != null)
                {
                    //Application info
                    //if (!string.IsNullOrEmpty(mActivity.Description))
                    //{
                    //    if (mActivity.Description.Length > 100)
                    //    {
                    //        xDescTextBlockHelper.AddText("Description: " + mActivity.Description.Substring(0,99) + "...");
                    //    }
                    //    else
                    //    {
                    //        xDescTextBlockHelper.AddText("Description: " + mActivity.Description);
                    //    }
                    //    xDescTextBlockHelper.AddText(" " + Ginger.General.GetTagsListAsString(mActivity.Tags));
                    //    xDescTextBlockHelper.AddLineBreak();
                    //}
                    //if (!string.IsNullOrEmpty(mActivity.RunDescription))
                    //{
                    //    xDescTextBlockHelper.AddText("Run Description: " + mActivity.RunDescription);
                    //    xDescTextBlockHelper.AddLineBreak();
                    //}
                    //if (!string.IsNullOrEmpty(mActivity.ActivitiesGroupID))
                    //{
                    //    xDescTextBlockHelper.AddText("Parent Group: " + mActivity.ActivitiesGroupID);
                    //    xDescTextBlockHelper.AddLineBreak();
                    //}
                    xDescTextBlockHelper.AddText("Target: " + mActivity.TargetApplication);
                }
            });

        }

        private void xUploadToShareRepoMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(mContext, mActivity));
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
                App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.RunCurrentActionAndMoveOn, new Tuple<Activity, Act, bool>(mActivity, (Act)mActionsPage.ListView.CurrentItem, false));
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

            ObservableList<Button> winButtons = [];

            RoutedEventHandler CloseHandler = CloseWinClicked;
            string closeContent = "Undo & Close";

            Button okBtn = new Button
            {
                Content = "Ok"
            };
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: okBtn, eventName: nameof(ButtonBase.Click), handler: OkBtn_Click);

            Button undoBtn = new Button
            {
                Content = "Undo & Close"
            };
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: undoBtn, eventName: nameof(ButtonBase.Click), handler: UndoBtn_Click);

            Button saveBtn = new Button
            {
                Content = "Save"
            };

            switch (mPageViewMode)
            {
                case Ginger.General.eRIPageViewMode.SharedReposiotry:
                    mActivity.SaveBackup();
                    title = "Edit Shared Repository " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                    WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: saveBtn, eventName: nameof(ButtonBase.Click), handler: SharedRepoSaveBtn_Click);
                    winButtons.Add(saveBtn);
                    winButtons.Add(undoBtn);
                    break;

                case Ginger.General.eRIPageViewMode.ChildWithSave:
                    mActivity.SaveBackup();
                    title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                    WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: saveBtn, eventName: nameof(ButtonBase.Click), handler: ParentItemSaveButton_Click);


                    winButtons.Add(saveBtn);
                    winButtons.Add(undoBtn);
                    break;

                case Ginger.General.eRIPageViewMode.View:
                case Ginger.General.eRIPageViewMode.ViewAndExecute:
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
                    //await SharedRepositoryOperations.UpdateLinkedInstances(mActivity); //this method is already being called from Activity.PostSaveHandler
                    mSaveWasDone = true;
                    mGenericWin.Close();
                }
            }
        }

        private void xUndoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Ginger.General.UndoChangesInRepositoryItem(mActivity, true))
            {
                mActionsPage.xGoToActionsList.DoClick();
                mActivity.SaveBackup();
            }
        }

        private void xEditBtn_Click(object sender, RoutedEventArgs e)
        {
            mActivity.EnableEdit = true;
            if (Reporter.ToUser(eUserMsgKey.WarnOnEditLinkSharedActivities) == Amdocs.Ginger.Common.eUserMsgSelection.No)
            {
                return;
            }
            //create back up
            mActivity.SaveBackup();
            mActivity.StartTimer();
            UpdateActivityViewMode(Ginger.General.eRIPageViewMode.Automation);
        }

        private void RunBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((ucButton)sender).ButtonImageForground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
        }

        private void RunBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((ucButton)sender).ButtonImageForground = (SolidColorBrush)FindResource("$HighlightColor_LightBlue");
        }

        private async void xSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            await SharedRepositoryOperations.SaveLinkedActivity(mActivity, mContext.BusinessFlow.Guid.ToString());

            UpdateActivityViewMode(Ginger.General.eRIPageViewMode.ViewAndExecute);
        }

        protected override void IsVisibleChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (mPageViewMode == Ginger.General.eRIPageViewMode.SharedReposiotry && mActivity != null && !String.IsNullOrEmpty(mActivity.ContainingFolder))
            {
                CurrentItemToSave = mActivity;
                base.IsVisibleChangedHandler(sender, e);
            }
        }
    }
}
