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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Activities;
using Ginger.BusinessFlowWindows;
using Ginger.Repository;
using Ginger.Variables;
using GingerCore;
using GingerCore.Activities;
using GingerCore.Platforms;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.BusinessFlowFolder
{
    /// <summary>
    /// Interaction logic for BusinessFlowPage.xaml
    /// </summary>
    public partial class BusinessFlowPage : Page
    {
        BusinessFlow mBusinessFlow;
        RepositoryPage mReposiotryPage;
        ActivitiesGroupsPage mActivitiesGroupsPage;
        VariablesPage mVariablesPage;
        ActivitiesPage mActivitiesPage;    
        
        GridLength mlastRepositoryColWidth = new GridLength(150);
        GridLength mMinColsExpanderSize = new GridLength(35);
        GenericWindow _pageGenericWin = null;
        public bool OKButtonClicked = false;
        private bool saveWasDone = false;

        public General.RepositoryItemPageViewMode mEditMode { get; set; }

        public BusinessFlowPage(BusinessFlow BizFlow, bool showMiniView=false, General.RepositoryItemPageViewMode editMode = General.RepositoryItemPageViewMode.SharedReposiotry)
        {
            InitializeComponent();

            mBusinessFlow = BizFlow;
            RunDescritpion.Init(BizFlow, BusinessFlow.Fields.RunDescription);
            mEditMode = editMode;
            LoadBizFlowData();
            App.PropertyChanged += AppPropertychanged;

            if (mBusinessFlow.TargetApplications == null)
            {
                mBusinessFlow.TargetApplications = new ObservableList<TargetBase>();
            }
            PlatformListBox.ItemsSource = mBusinessFlow.TargetApplications;
            PlatformListBox.DisplayMemberPath = nameof(TargetApplication.AppName);
            TagsViewer.Init(mBusinessFlow.Tags);

            TrackBusinessFlowAutomationPrecentage();
            mBusinessFlow.AttachActivitiesGroupsAndActivities();

            if (!showMiniView)
            {
                mActivitiesPage = new ActivitiesPage(mBusinessFlow, mEditMode);
                if(mEditMode!= General.RepositoryItemPageViewMode.View)
                {
                    mActivitiesPage.grdActivities.ChangeGridView(eAutomatePageViewStyles.Design.ToString());
                    mBusinessFlow.SaveBackup();
                }
                mActivitiesPage.grdActivities.ShowTitle = System.Windows.Visibility.Collapsed;
                BfActivitiesFrame.Content = mActivitiesPage;

                mActivitiesGroupsPage = new ActivitiesGroupsPage(mBusinessFlow, mEditMode);
                mActivitiesGroupsPage.grdActivitiesGroups.ShowTitle = System.Windows.Visibility.Collapsed;
                BfActivitiesGroupsFrame.Content = mActivitiesGroupsPage;
                if (mBusinessFlow.ActivitiesGroups.Count == 0) ActivitiesGroupsExpander.IsExpanded = false;

                mVariablesPage = new VariablesPage(GingerCore.Variables.eVariablesLevel.BusinessFlow, mBusinessFlow, mEditMode);
                mVariablesPage.grdVariables.ShowTitle = System.Windows.Visibility.Collapsed;
                BfVariablesFrame.Content = mVariablesPage;
                if (mBusinessFlow.Variables.Count == 0) VariablesExpander.IsExpanded = false;
            }
            else
            {
                //hide Variables / Activities groups/ Activities / Repository
                Row3.MinHeight = 0;
                Row3.Height = new GridLength(0);
                Row4.MinHeight = 0;
                Row4.Height = new GridLength(0);
                Row5.MinHeight = 0;
                Row5.Height = new GridLength(0);
            }
            if(mEditMode == General.RepositoryItemPageViewMode.View)
            {
                txtName.IsEnabled = false;
                txtDescription.IsEnabled = false;
                RunDescritpion.IsEnabled = false;
                TagsViewer.IsEnabled = false;
                xBusinessflowinfo.IsEnabled = false;
                xTargetApplication.IsEnabled = false;
                xAutomateBtn.Visibility = Visibility.Collapsed;
            }

            if (!App.UserProfile.UserTypeHelper.IsSupportAutomate)
            {
                xAutomateBtn.Visibility = Visibility.Collapsed;
            }

            SetExpandersLabels();
        }
        
        private void TrackBusinessFlowAutomationPrecentage()
        {
            mBusinessFlow.Activities.CollectionChanged += mBusinessFlowActivities_CollectionChanged;
            foreach (Activity activity in mBusinessFlow.Activities)
            {
                try { activity.PropertyChanged -= mBusinessFlowActivity_PropertyChanged; }catch(Exception ex){ Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }  //to make sure wont be added twice              
                activity.PropertyChanged += mBusinessFlowActivity_PropertyChanged;
            }
        }
        private void mBusinessFlowActivities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            mBusinessFlow.OnPropertyChanged(BusinessFlow.Fields.AutomationPrecentage);
            
            //Perf imrprovements
            if (WorkSpace.Instance.BetaFeatures.BFPageActivitiesHookOnlyNewActivities)
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (object o in e.NewItems)
                    {
                        Activity activity = (Activity)o;
                        activity.PropertyChanged += mBusinessFlowActivity_PropertyChanged;
                    }
                }
            }
            else
            {
                foreach (Activity activity in mBusinessFlow.Activities)
                {
                    try { activity.PropertyChanged -= mBusinessFlowActivity_PropertyChanged; } catch (Exception ex) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }  //to make sure wont be added twice              
                    activity.PropertyChanged += mBusinessFlowActivity_PropertyChanged;
                }
            }
        }
        private void mBusinessFlowActivity_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Activity.Fields.AutomationStatus)
            {
                mBusinessFlow.OnPropertyChanged(BusinessFlow.Fields.AutomationPrecentage);
                Activity changedActivity= (Activity)sender;
                if (string.IsNullOrEmpty(changedActivity.ActivitiesGroupID) == false)
                {
                    foreach(ActivitiesGroup actGroup in mBusinessFlow.ActivitiesGroups)
                        if (actGroup.Name == changedActivity.ActivitiesGroupID)
                        {
                            actGroup.OnPropertyChanged(ActivitiesGroup.Fields.AutomationPrecentage);
                            break;
                        }                    
                }
            }
        }
       
        private void LoadBizFlowData()
        {
            App.ObjFieldBinding(txtName, TextBox.TextProperty, mBusinessFlow, BusinessFlow.Fields.Name);
            App.ObjFieldBinding(txtDescription, TextBox.TextProperty, mBusinessFlow, BusinessFlow.Fields.Description);
            App.FillComboFromEnumVal(StatusComboBox, mBusinessFlow.Status);
            App.ObjFieldBinding(StatusComboBox, ComboBox.TextProperty, mBusinessFlow, BusinessFlow.Fields.Status);                     
            App.ObjFieldBinding(CreatedByTextBox, TextBox.TextProperty, mBusinessFlow.RepositoryItemHeader,  nameof(RepositoryItemHeader.CreatedBy));  
            App.ObjFieldBinding(AutoPrecentageTextBox, TextBox.TextProperty, mBusinessFlow, BusinessFlow.Fields.AutomationPrecentage, System.Windows.Data.BindingMode.OneWay);            
            
            // Per source we can show specific source page info
            if (mBusinessFlow.Source == BusinessFlow.eSource.Gherkin)
            {
                SourceGherkinPage SGP = new SourceGherkinPage(mBusinessFlow);
                SourceFrame.Content = SGP;
            }
        }

        private void AppPropertychanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BusinessFlow")
            {
                LoadBizFlowData();
            }            
        }     

        private void AddPlatformButton_Click(object sender, RoutedEventArgs e)
        {
            EditBusinessFlowAppsPage EBFP = new EditBusinessFlowAppsPage(mBusinessFlow);
            EBFP.ShowAsWindow();

            //make sure all Activities mapped to Application after change
            foreach (Activity activity in mBusinessFlow.Activities)
                if (mBusinessFlow.TargetApplications.Where(x => x.Name == activity.TargetApplication).FirstOrDefault() == null)
                    activity.TargetApplication = mBusinessFlow.TargetApplications[0].Name;
        }

        private void ActivitiesGroupsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Row4.Height = new GridLength(100, GridUnitType.Star);
        }

        private void ActivitiesGroupsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Row4.Height = new GridLength(35);
        }

        private void ActivitiesExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Row5.Height = new GridLength(300, GridUnitType.Star);
        }

        private void ActivitiesExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Row5.Height = new GridLength(0);
            if (ActivitiesGroupsExpander.IsExpanded == false)
                ActivitiesExpander.IsExpanded = true;
        }        

        private void VariablesExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Row3.Height = new GridLength(100, GridUnitType.Star);
        }

        private void VariablesExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            Row3.Height = new GridLength(35);
        }
        public bool ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset = false)
        {
            string title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
            RoutedEventHandler closeHandler = CloseWinClicked;
            string closeContent = "Undo & Close";
            ObservableList<Button> winButtons = new ObservableList<Button>();
            switch (mEditMode)
            {
                case General.RepositoryItemPageViewMode.Automation:
                    Button okBtn = new Button();
                    okBtn.Content = "Ok";
                    okBtn.Click += new RoutedEventHandler(okBtn_Click);
                    Button undoBtn = new Button();
                    undoBtn.Content = "Undo & Close";
                    undoBtn.Click += new RoutedEventHandler(undoAndCloseBtn_Click);
                    winButtons.Add(undoBtn);
                    winButtons.Add(okBtn);
                    break;
                case General.RepositoryItemPageViewMode.Standalone:
                    title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                    Button saveBtn = new Button();
                    saveBtn.Content = "Save";
                    saveBtn.Click += new RoutedEventHandler(saveBtn_Click);
                    Button undoBtnSr = new Button();
                    undoBtnSr.Content = "Undo & Close";
                    undoBtnSr.Click += new RoutedEventHandler(undoAndCloseBtn_Click);
                    winButtons.Add(undoBtnSr);
                    winButtons.Add(saveBtn);
                    break;
                case General.RepositoryItemPageViewMode.View:
                    title = "View " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                    Button okBtnView = new Button();
                    okBtnView.Content = "Ok";
                    okBtnView.Click += new RoutedEventHandler(okBtn_Click);
                    winButtons.Add(okBtnView);
                    closeHandler = new RoutedEventHandler(okBtn_Click);
                    closeContent = okBtnView.Content.ToString();
                    break;
            }
            
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, title, this, winButtons, false, closeContent, closeHandler, startupLocationWithOffset: startupLocationWithOffset);
            return saveWasDone;
        }

        private void CloseWinClicked(object sender, EventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKeys.AskIfToUndoChanges) == MessageBoxResult.Yes)
            {
                UndoChanges();
                _pageGenericWin.Close();
            }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SharedRepositoryOperations.CheckIfSureDoingChange(mBusinessFlow, "change") == true)
            {
                saveWasDone = true;                
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mBusinessFlow);
                _pageGenericWin.Close();
            }
        }

        private void undoAndCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            UndoChanges();
            _pageGenericWin.Close();
        }

        private void UndoChanges()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            mBusinessFlow.RestoreFromBackup(true);
            Mouse.OverrideCursor = null;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            OKButtonClicked = true;
            _pageGenericWin.Close();
        }

        private void xAutomateBtn_Click(object sender, RoutedEventArgs e)
        {
           App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.Automate, mBusinessFlow);
        }

        private void SetExpandersLabels()
        {
            UpdateVariabelsExpanderLabel();
            mBusinessFlow.Variables.CollectionChanged += Variables_CollectionChanged;
            UpdateActivitiesGroupsExpanderLabel();
            mBusinessFlow.ActivitiesGroups.CollectionChanged += ActivitiesGroups_CollectionChanged;
            UpdateActivitiesExpanderLabel();
            mBusinessFlow.Activities.CollectionChanged += Activities_CollectionChanged;
        }

        private void Activities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateActivitiesExpanderLabel();
        }

        private void ActivitiesGroups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateActivitiesGroupsExpanderLabel();
        }

        private void Variables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateVariabelsExpanderLabel();
        }

        private void UpdateVariabelsExpanderLabel()
        {
            this.Dispatcher.Invoke(() =>
            {
                VariablesExpanderLabel.Content = string.Format("{0} ({1})", GingerDicser.GetTermResValue(eTermResKey.Variables), mBusinessFlow.Variables.Count);
            });
        }
        private void UpdateActivitiesGroupsExpanderLabel()
        {
            this.Dispatcher.Invoke(() =>
            {
                ActivitiesGroupsExpanderLabel.Content = string.Format("{0} ({1})", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups), mBusinessFlow.ActivitiesGroups.Count);
            });
        }
        private void UpdateActivitiesExpanderLabel()
        {
            this.Dispatcher.Invoke(() =>
            {
                ActivitiesExpanderLabel.Content = string.Format("{0} ({1})", GingerDicser.GetTermResValue(eTermResKey.Activities), mBusinessFlow.Activities.Count);
            });
        }
    }
}
