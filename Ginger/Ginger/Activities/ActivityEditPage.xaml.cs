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
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GingerCore;
using Ginger.Variables;
using GingerCore.Variables;
using GingerCore.Platforms;
using Ginger.Actions;
using Ginger.Repository;
using Ginger.Activities;
using Amdocs.Ginger;
using amdocs.ginger.GingerCoreNET;
using System.Linq;
using Amdocs.Ginger.Common.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace Ginger.BusinessFlowWindows
{
    /// <summary>
    /// Interaction logic for ActivityEditPage.xaml
    /// </summary>
    public partial class ActivityEditPage : Page
    {
        private Activity mActivity;
        GenericWindow _pageGenericWin = null;
        public bool OKButtonClicked = false;
        private bool saveWasDone = false;
        private BusinessFlow mActivityParentBusinessFlow = null;
        Context mContext;

        public General.RepositoryItemPageViewMode editMode { get; set; }

        public ActivityEditPage(Activity activity, General.RepositoryItemPageViewMode mode = General.RepositoryItemPageViewMode.Automation, BusinessFlow activityParentBusinessFlow = null, Context context=null)
        {
            InitializeComponent();

            this.Title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.Activity);

            mActivity = activity;

            if (context != null)
            {
                mContext = context;
            }
            else
            {
                mContext = new Context();
                mContext.BusinessFlow = activityParentBusinessFlow;
                mContext.Activity = mActivity;
            }

            if (editMode != General.RepositoryItemPageViewMode.View)
                mActivity.SaveBackup();
            editMode = mode;

            RunDescritpion.Init(mContext, activity, Activity.Fields.RunDescription);
            
            mActivityParentBusinessFlow = activityParentBusinessFlow;            

            List<string> automationStatusList = GingerCore.General.GetEnumValues(typeof(eActivityAutomationStatus));
            AutomationStatusCombo.ItemsSource = automationStatusList;
            RunOptionCombo.BindControl(activity, Activity.Fields.ActionRunOption);            
            HandlerTypeCombo.ItemsSource = GingerCore.General.GetEnumValues(typeof(eHandlerType));

            GingerCore.General.FillComboFromEnumObj(cmbErrorHandlerMapping, mActivity.ErrorHandlerMappingType);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtActivityName, TextBox.TextProperty, mActivity, Activity.Fields.ActivityName);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtActivityDescription, TextBox.TextProperty, mActivity, Activity.Fields.Description);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtExpected, TextBox.TextProperty, mActivity, Activity.Fields.Expected);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtScreen, TextBox.TextProperty, mActivity, Activity.Fields.Screen);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtGroup, TextBox.TextProperty, mActivity, Activity.Fields.ActivitiesGroupID);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(AutomationStatusCombo, ComboBox.TextProperty, mActivity, Activity.Fields.AutomationStatus);            
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(MandatoryActivityCB, CheckBox.IsCheckedProperty, mActivity, Activity.Fields.Mandatory);

            if (activity.GetType() == typeof(ErrorHandler))
            {
                HandlerTypeStack.Visibility = Visibility.Visible;
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(HandlerTypeCombo, ComboBox.TextProperty, mActivity, ErrorHandler.Fields.HandlerType);                
            }
            else
            {
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(cmbErrorHandlerMapping, ComboBox.SelectedValueProperty, mActivity, Activity.Fields.ErrorHandlerMappingType);
                HandlerMappingStack.Visibility = Visibility.Visible;
                Row1.Height = new GridLength(Row1.Height.Value - 38);
            }

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(TargetApplicationComboBox, ComboBox.SelectedValueProperty, mActivity, Activity.Fields.TargetApplication);

            FillTargetAppsComboBox();

            TagsViewer.Init(mActivity.Tags);

            VariablesPage varbsPage;
            ActionsPage actionsPage;
            if (editMode == General.RepositoryItemPageViewMode.View)
            {
                varbsPage = new VariablesPage(eVariablesLevel.Activity, mActivity, General.RepositoryItemPageViewMode.View);
                actionsPage = new ActionsPage(mActivity, mActivityParentBusinessFlow, General.RepositoryItemPageViewMode.View, mContext);
                SetViewMode();
            }
            else
            {
                varbsPage = new VariablesPage(eVariablesLevel.Activity, mActivity, General.RepositoryItemPageViewMode.Child);
                actionsPage = new ActionsPage(mActivity, mActivityParentBusinessFlow, General.RepositoryItemPageViewMode.Child, mContext);
            }

            varbsPage.grdVariables.ShowTitle = System.Windows.Visibility.Collapsed;
            if (varbsPage.grdVariables.Grid.Items.Count == 0)
                VariablesExpander.IsExpanded = false;
           
            actionsPage.grdActions.ShowTitle = System.Windows.Visibility.Collapsed;          
            VariablesFrame.Content = varbsPage;
            ActionsFrame.Content = actionsPage;

            if (editMode == General.RepositoryItemPageViewMode.Automation)
                SharedRepoInstanceUC.Init(mActivity, mActivityParentBusinessFlow);
            else
            {
                SharedRepoInstanceUC.Visibility = Visibility.Collapsed;
                SharedRepoInstanceUC_Col.Width = new GridLength(0);
            }
            if (!mActivity.IsNotGherkinOptimizedActivity)
                txtActivityName.IsEnabled = false;

            SetExpandersLabels();

            txtActivityName.Focus();        
        }

        private void FillTargetAppsComboBox()
        {
            if (mActivityParentBusinessFlow != null)
            {
                TargetApplicationComboBox.ItemsSource = mActivityParentBusinessFlow.TargetApplications;
            }
            else
            {
                TargetApplicationComboBox.ItemsSource = WorkSpace.Instance.Solution.GetSolutionTargetApplications();
            }
            TargetApplicationComboBox.SelectedValuePath = nameof(TargetApplication.AppName);
            TargetApplicationComboBox.DisplayMemberPath = nameof(TargetApplication.AppName);
        }

        public bool ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset=false)
        {
            string title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            RoutedEventHandler closeHandler = CloseWinClicked;
            string closeContent= "Undo & Close";
            ObservableList<Button> winButtons = new ObservableList<Button>();
            Button okBtn = new Button();
            okBtn.Content = "Ok";
            okBtn.Click += new RoutedEventHandler(okBtn_Click);
            Button undoBtn = new Button();
            undoBtn.Content = "Undo & Close";
            undoBtn.Click += new RoutedEventHandler(undoBtn_Click);
            Button saveBtn = new Button();
            saveBtn.Content = "Save";
            switch (editMode)
            {
                case General.RepositoryItemPageViewMode.Automation:                    
                    winButtons.Add(okBtn);                    
                    winButtons.Add(undoBtn);
                    break;

                case General.RepositoryItemPageViewMode.SharedReposiotry:
                    title = "Edit Shared Repository " + GingerDicser.GetTermResValue(eTermResKey.Activity);                    
                    saveBtn.Click += new RoutedEventHandler(SharedRepoSaveBtn_Click);
                    winButtons.Add(saveBtn);
                    winButtons.Add(undoBtn);
                    break;

                case General.RepositoryItemPageViewMode.ChildWithSave:
                    title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                    saveBtn.Click += new RoutedEventHandler(ParentItemSaveButton_Click);
                    winButtons.Add(saveBtn);
                    winButtons.Add(undoBtn);
                    break;

                case General.RepositoryItemPageViewMode.View:
                    title = "View " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                    winButtons.Add(okBtn);
                    closeHandler = new RoutedEventHandler(okBtn_Click);
                    closeContent = okBtn.Content.ToString();
                    break;
            }

            this.Height = 800;
            this.Width = 1000;

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, title, this, winButtons, false, closeBtnText: closeContent, closeEventHandler: closeHandler, startupLocationWithOffset: startupLocationWithOffset);
            return saveWasDone;
        }

        private void CloseWinClicked(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.AskIfToUndoChanges) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                UndoChangesAndClose();
            }
        }

        private void SetViewMode()
        {
            xActivityDetails.IsEnabled = xActivityInfo.IsEnabled = TagsViewer.IsEnabled = xActivityVariables.IsEnabled = false;
        }

        private void ParentItemSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (mActivityParentBusinessFlow != null && Reporter.ToUser(eUserMsgKey.SaveItemParentWarning) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {                
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mActivityParentBusinessFlow);
                saveWasDone = true;
            }
            _pageGenericWin.Close();
        }

        private void UndoChangesAndClose()
        {
            Mouse.OverrideCursor = Cursors.Wait;            
            mActivity.RestoreFromBackup(true);
            Mouse.OverrideCursor = null;

            _pageGenericWin.Close();
        }

        private void undoBtn_Click(object sender, RoutedEventArgs e)
        {
            UndoChangesAndClose();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            OKButtonClicked = true;
            _pageGenericWin.Close();
        }

        private void CheckIfUserWantToSave()
        {
            if (editMode == General.RepositoryItemPageViewMode.SharedReposiotry)
            {
                if (SharedRepositoryOperations.CheckIfSureDoingChange(mActivity, "change") == true)
                {
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mActivity);
                    saveWasDone = true;
                    _pageGenericWin.Close();
                }
            }
        }

        private void SharedRepoSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckIfUserWantToSave();
        }

        private void VariablesExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Row4.Height = new GridLength(150, GridUnitType.Star);
        }

        private void VariablesExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Row4.Height = new GridLength(35);
        }

        private void ActionsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Row5.Height = new GridLength(150, GridUnitType.Star);
        }

        private void ActionsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Row5.Height = new GridLength(0);
            if (VariablesExpander.IsExpanded == false)
                ActionsExpander.IsExpanded = true;
        }

        private void cmbErrorHandlerMapping_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbErrorHandlerMapping.SelectedValue.ToString() == eHandlerMappingType.SpecificErrorHandlers.ToString())
            {
                btnSpecificErrorHandler.Visibility = Visibility.Visible;

            }
            else
                btnSpecificErrorHandler.Visibility = Visibility.Collapsed;
        }

        private void btnSpecificErrorHandler_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("cmbErrorHandlerMapping_SelectionChanged");
            ErrorHandlerMappingPage errorHandlerMappingPage = new ErrorHandlerMappingPage(mActivity, mActivityParentBusinessFlow);
            errorHandlerMappingPage.ShowAsWindow();
            AutoLogProxy.UserOperationEnd();
        }

        private void SetExpandersLabels()
        {
            UpdateVariabelsExpanderLabel();
            mActivity.Variables.CollectionChanged += Variables_CollectionChanged;
            UpdateActionsExpanderLabel();
            mActivity.Acts.CollectionChanged += Acts_CollectionChanged;
        }

        private void Acts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateActionsExpanderLabel();
        }

        private void Variables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateVariabelsExpanderLabel();
        }

        private void UpdateVariabelsExpanderLabel()
        {
            this.Dispatcher.Invoke(() =>
            {
                VariablesExpanderLabel.Content = string.Format("{0} ({1})", GingerDicser.GetTermResValue(eTermResKey.Variables), mActivity.Variables.Count);
            });
        }
        private void UpdateActionsExpanderLabel()
        {
            this.Dispatcher.Invoke(() =>
            {
                ActionsExpanderLabel.Content = string.Format("Actions ({0})", mActivity.Acts.Count);
            });
        }
    }
}
