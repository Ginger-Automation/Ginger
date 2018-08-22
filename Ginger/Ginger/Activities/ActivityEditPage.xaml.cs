#region License
/*
Copyright © 2014-2018 European Support Limited

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

        public General.RepositoryItemPageViewMode editMode { get; set; }

        public ActivityEditPage(Activity activity, General.RepositoryItemPageViewMode mode = General.RepositoryItemPageViewMode.Automation, BusinessFlow activityParentBusinessFlow = null)
        {
            InitializeComponent();

            this.Title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.Activity);

            mActivity = activity;
            if (editMode != General.RepositoryItemPageViewMode.View)
                mActivity.SaveBackup();
            editMode = mode;

            RunDescritpion.Init(activity, Activity.Fields.RunDescription);

            if (activityParentBusinessFlow != null)
                mActivityParentBusinessFlow = activityParentBusinessFlow;
            else
                mActivityParentBusinessFlow = App.BusinessFlow;

            List<string> automationStatusList = GingerCore.General.GetEnumValues(typeof(Activity.eActivityAutomationStatus));
            AutomationStatusCombo.ItemsSource = automationStatusList;
            RunOptionCombo.BindControl(activity, Activity.Fields.ActionRunOption);            
            HandlerTypeCombo.ItemsSource = GingerCore.General.GetEnumValues(typeof(ErrorHandler.eHandlerType));

            App.FillComboFromEnumVal(cmbErrorHandlerMapping, mActivity.ErrorHandlerMappingType);
            App.ObjFieldBinding(txtActivityName, TextBox.TextProperty, mActivity, Activity.Fields.ActivityName);
            App.ObjFieldBinding(txtActivityDescription, TextBox.TextProperty, mActivity, Activity.Fields.Description);
            App.ObjFieldBinding(txtExpected, TextBox.TextProperty, mActivity, Activity.Fields.Expected);
            App.ObjFieldBinding(txtScreen, TextBox.TextProperty, mActivity, Activity.Fields.Screen);
            App.ObjFieldBinding(txtGroup, TextBox.TextProperty, mActivity, Activity.Fields.ActivitiesGroupID);
            App.ObjFieldBinding(AutomationStatusCombo, ComboBox.TextProperty, mActivity, Activity.Fields.AutomationStatus);            
            App.ObjFieldBinding(MandatoryActivityCB, CheckBox.IsCheckedProperty, mActivity, Activity.Fields.Mandatory);

            if (activity.GetType() == typeof(ErrorHandler))
            {
                HandlerTypeStack.Visibility = Visibility.Visible;
                App.ObjFieldBinding(HandlerTypeCombo, ComboBox.TextProperty, mActivity, ErrorHandler.Fields.HandlerType);                
            }
            else
            {
                App.ObjFieldBinding(cmbErrorHandlerMapping, ComboBox.SelectedValueProperty, mActivity, Activity.Fields.ErrorHandlerMappingType);
                HandlerMappingStack.Visibility = Visibility.Visible;
                Row1.Height = new GridLength(Row1.Height.Value - 38);
            }

            App.ObjFieldBinding(TargetApplicationComboBox, ComboBox.SelectedValueProperty, mActivity, Activity.Fields.TargetApplication);

            FillTargetAppsComboBox();

            TagsViewer.Init(mActivity.Tags);

            VariablesPage varbsPage;
            ActionsPage actionsPage;
            if (editMode == General.RepositoryItemPageViewMode.View)
            {
                varbsPage = new VariablesPage(eVariablesLevel.Activity, mActivity, General.RepositoryItemPageViewMode.View);
                actionsPage = new ActionsPage(mActivity, General.RepositoryItemPageViewMode.View);
                SetViewMode();
            }
            else
            {
                varbsPage = new VariablesPage(eVariablesLevel.Activity, mActivity);
                actionsPage = new ActionsPage(mActivity);
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

            txtActivityName.Focus();        
        }

        private void FillTargetAppsComboBox()
        {
            TargetApplicationComboBox.ItemsSource = mActivityParentBusinessFlow.TargetApplications;
            TargetApplicationComboBox.SelectedValuePath = TargetApplication.Fields.AppName;
            TargetApplicationComboBox.DisplayMemberPath = TargetApplication.Fields.AppName;
        }

        public bool ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset=false)
        {
            string title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            ObservableList<Button> winButtons = new ObservableList<Button>();
            switch (editMode)
            {
                case General.RepositoryItemPageViewMode.Automation:
                    Button okBtn = new Button();
                    okBtn.Content = "Ok";
                    okBtn.Click += new RoutedEventHandler(okBtn_Click);
                    Button undoBtn = new Button();
                    undoBtn.Content = "Undo & Close";
                    undoBtn.Click += new RoutedEventHandler(undoBtn_Click);
                    winButtons.Add(undoBtn);
                    winButtons.Add(okBtn);
                    break;

                case General.RepositoryItemPageViewMode.SharedReposiotry:
                    title = "Edit Shared Repository " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                    Button saveBtn = new Button();
                    saveBtn.Content = "Save";
                    saveBtn.Click += new RoutedEventHandler(saveBtn_Click);
                    Button undoBtnSr = new Button();
                    undoBtnSr.Content = "Undo & Close";
                    undoBtnSr.Click += new RoutedEventHandler(undoBtn_Click);
                    winButtons.Add(undoBtnSr);
                    winButtons.Add(saveBtn);
                    break;

                case General.RepositoryItemPageViewMode.Child:
                    title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                    Button saveButton = new Button();
                    saveButton.Content = "Save";
                    saveButton.Click += new RoutedEventHandler(saveButton_Click);
                    Button undoButton = new Button();
                    undoButton.Content = "Undo & Close";
                    undoButton.Click += new RoutedEventHandler(undoBtn_Click);
                    winButtons.Add(undoButton);
                    winButtons.Add(saveButton);
                    break;
                case General.RepositoryItemPageViewMode.View:
                    title = "View " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                    Button okBtnView = new Button();
                    okBtnView.Content = "Ok";
                    okBtnView.Click += new RoutedEventHandler(okBtn_Click);
                    winButtons.Add(okBtnView);                   
                    break;

            }
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, title, this, winButtons, false, string.Empty, CloseWinClicked, startupLocationWithOffset: startupLocationWithOffset);
            return saveWasDone;
        }

        private void SetViewMode()
        {
            xActivityDetails.IsEnabled = xActivityInfo.IsEnabled = TagsViewer.IsEnabled = xActivityVariables.IsEnabled = false;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (mActivityParentBusinessFlow != null && Reporter.ToUser(eUserMsgKeys.SaveItemParentWarning) == MessageBoxResult.Yes)
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

        private void CloseWinClicked(object sender, EventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKeys.ToSaveChanges) == MessageBoxResult.No)
            {
                UndoChangesAndClose();
            }
            else
            {
                if (editMode == General.RepositoryItemPageViewMode.SharedReposiotry)
                    CheckIfUserWantToSave();
                else
                    _pageGenericWin.Close();
            }
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
                mActivity.Save();
                saveWasDone = true;
                _pageGenericWin.Close();
            }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
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
            if (cmbErrorHandlerMapping.SelectedValue.ToString() == Activity.eHandlerMappingType.SpecificErrorHandlers.ToString())
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
    }
}
