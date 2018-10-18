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

using Ginger.Actions;
using Ginger.Activities;
using Ginger.BusinessFlowFolder;
using Ginger.Repository;
using Ginger.SolutionWindows.TreeViewItems;
using GingerWPF.UserControlsLib.UCTreeView;
using Ginger.Variables;
using GingerCore;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;
using System.Windows.Media;
using Amdocs.Ginger;
using Ginger.Actions.ActionConversion;
using Ginger.AnalyzerLib;
using Ginger.Functionalities;
using System.Windows.Input;
using Ginger.GherkinLib;
using System.Linq;
using Ginger.UserControlsLib.TextEditor;
using Ginger.Reports;
using GingerCore.Actions;
using Ginger.BusinessFlowWindows;
using System.IO;
using System.Diagnostics;
using Ginger.ALM;
using Amdocs.Ginger.Common;
using Ginger.Run;
using GingerCore.Environments;
using Amdocs.Ginger.UserControls;
using Amdocs.Ginger.Common.Enums;
using GingerCoreNET.RunLib;
using Ginger.Agents;
using Ginger.Extensions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Threading.Tasks;

namespace Ginger
{
    public enum eAutomatePageViewStyles
    {
        Execution, 
        Design,
        Details
    }

    /// <summary>
    /// Interaction logic for AutomatePage.xaml
    /// </summary>
    public partial class AutomatePage : Page //NOSONAR 
    {                 
        BusinessFlowPage mCurrentBusPage;
        VariablesPage mVariablesPage;
        ActivitiesGroupsPage mActivitiesGroupsPage;
        ActivitiesPage mActivitiesPage;
        ActivitiesMiniViewPage mActivitiesMiniPage;        
        VariablesPage mActivityVariablesPage;
        public ActionsPage mActionsPage;
        RepositoryPage mReposiotryPage;
                
        GridLength mlastRepositoryColWidth = new GridLength(300);

        GridLength mlastBFVariablesRowHeight = new GridLength(200, GridUnitType.Star);
        GridLength mlastBFActivitiesGroupsRowHeight = new GridLength(200, GridUnitType.Star);
        GridLength mlastBFActivitiesRowHeight = new GridLength(300, GridUnitType.Star);
        GridLength mlastActivityVariablesRowHeight = new GridLength(200, GridUnitType.Star);
        GridLength mlastActivitiyActionsRowHeight = new GridLength(300, GridUnitType.Star);
        readonly GridLength mMinRowsExpanderSize = new GridLength(35);
        readonly GridLength mMinColsExpanderSize = new GridLength(35);

        public void GoToBusFlowsListHandler(RoutedEventHandler clickHandler)
        {           
            xToBusinessFlowsListBtn.Visibility = Visibility.Visible;
            xToBusinessFlowsListBtn.LargeImageSource = ImageMakerControl.GetImageSource(eImageType.GoBack, width: 32);
            xToBusinessFlowsListBtn.SmallImageSource = ImageMakerControl.GetImageSource(eImageType.GoBack, width: 16);
            xToBusinessFlowsListBtn.Label = GingerDicser.GetTermResValue(eTermResKey.BusinessFlows, "Back to ", " List");
            xToBusinessFlowsListBtn.ToolTip = GingerDicser.GetTermResValue(eTermResKey.BusinessFlows, "Back to ", " List");
            xToBusinessFlowsListBtn.Click += clickHandler;
        }

        public AutomatePage()
        {
            InitializeComponent();

            if (App.BusinessFlow == null) //Not supposed to happen because now Automate is done from BF itself           
            {
                App.SetDefaultBusinessFlow();
            }

            //Ribbon
            btnRunActivity.Label = "Run " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            ContinuteRunActiviy.Header = "Continue Run from Current " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            btnResetStatus.LargeImageSource = ImageMakerControl.GetImageSource(eImageType.Reset, width: 32);
            btnResetStatus.SmallImageSource = ImageMakerControl.GetImageSource(eImageType.Reset, width: 16);
            btnResetFlow.ImageSource = ImageMakerControl.GetImageSource(eImageType.Reset, width: 14);
            btnResetFromCurrentActivity.ImageSource = ImageMakerControl.GetImageSource(eImageType.Reset, width: 14);
            btnResetFromCurrentAction.ImageSource = ImageMakerControl.GetImageSource(eImageType.Reset, width: 14);
            cboSpeed.Text = "0";
            App.ObjFieldBinding(SimulationMode, CheckBox.IsCheckedProperty, App.AutomateTabGingerRunner, Ginger.Run.GingerRunner.Fields.RunInSimulationMode);
            AppAgentsMappingExpander2Frame.Content = new ApplicationAgentsMapPage(App.AutomateTabGingerRunner);
            BindEnvsCombo();

            SetExpanders();
            //Bind between Menu expanders and actual grid expanders
            App.ObjFieldBinding(BFVariablesExpander, Expander.IsExpandedProperty, BFVariablesExpander2, "IsExpanded");
            App.ObjFieldBinding(BFActivitiesGroupsExpander, Expander.IsExpandedProperty, BFActivitiesGroupsExpander2, "IsExpanded");
            App.ObjFieldBinding(BFActivitiesExpander, Expander.IsExpandedProperty, BFActivitiesExpander2, "IsExpanded");
            App.ObjFieldBinding(ActivityVariablesExpander, Expander.IsExpandedProperty, ActivityVariablesExpander2, "IsExpanded");
            App.ObjFieldBinding(ActivityActionsExpander, Expander.IsExpandedProperty, ActivityActionsExpander2, "IsExpanded");
            BFVariablesExpander.IsExpanded = false;
            BFActivitiesGroupsExpander.IsExpanded = false;
            ActivityVariablesExpander.IsExpanded = false;
            mlastBFVariablesRowHeight = new GridLength(200, GridUnitType.Star);
            mlastActivityVariablesRowHeight = new GridLength(200, GridUnitType.Star);
            SetFramesContent();            

            App.PropertyChanged += AppPropertychanged;
            App.UserProfile.PropertyChanged += UserProfilePropertyChanged;
            App.AutomateTabGingerRunner.GingerRunnerEvent += GingerRunner_GingerRunnerEvent;

            App.AutomateBusinessFlowEvent -= App_AutomateBusinessFlowEvent;
            App.AutomateBusinessFlowEvent += App_AutomateBusinessFlowEvent;

            SetGridsView(eAutomatePageViewStyles.Design.ToString());
            SetGherkinOptions();                     
        }

        private void App_AutomateBusinessFlowEvent(AutomateEventArgs args)
        {
            switch (args.EventType)
            {
                case AutomateEventArgs.eEventType.SetupRunnerForExecution:
                    SetAutomateTabRunnerForExecution();
                    break;
                case AutomateEventArgs.eEventType.RunCurrentAction:
                    RunCurrentAutomatePageAction(false);
                    break;
                case AutomateEventArgs.eEventType.RunCurrentActivity:
                    RunActivity();
                    break;
                case AutomateEventArgs.eEventType.ContinueActionRun:
                    ContinueRunFRomAutomateTab(Run.GingerRunner.eContinueFrom.SpecificAction);
                    break;
                case AutomateEventArgs.eEventType.ContinueActivityRun:
                    ContinueRunFRomAutomateTab(Run.GingerRunner.eContinueFrom.SpecificActivity);
                    break;
                case AutomateEventArgs.eEventType.StopRun:
                    StopAutomateRun();
                    break;
                case AutomateEventArgs.eEventType.GenerateLastExecutedItemReport:
                    GenerateLastExecutedItemReport();
                    break;
                default:
                    //Avoid other operations
                    break;

            }
        }

        private void GingerRunner_GingerRunnerEvent(GingerCoreNET.RunLib.GingerRunnerEventArgs EventArgs)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (EventArgs.EventType == GingerRunnerEventArgs.eEventType.DoEventsRequired)
                {
                    GingerCore.General.DoEvents();
                }
            });
        }

        private void SetFramesContent()
        {
            mCurrentBusPage = new BusinessFlowPage(App.BusinessFlow, true);
            CurrentBusFrame.Content = mCurrentBusPage;
            CurrentBusExpander.IsExpanded = false;

            mVariablesPage = new VariablesPage(eVariablesLevel.BusinessFlow);
            mVariablesPage.grdVariables.ShowTitle = System.Windows.Visibility.Collapsed;
            BFVariablesFrame.Content = mVariablesPage;
            mActivitiesGroupsPage = new ActivitiesGroupsPage();
            mActivitiesGroupsPage.grdActivitiesGroups.ShowTitle = System.Windows.Visibility.Collapsed;
            BFActivitiesGroupsFrame.Content = mActivitiesGroupsPage;

            mActivitiesPage = new ActivitiesPage();
            mActivitiesPage.grdActivities.ShowTitle = System.Windows.Visibility.Collapsed;
            BFActivitiesFrame.Content = mActivitiesPage;

            mActivityVariablesPage = new VariablesPage(eVariablesLevel.Activity);
            mActivityVariablesPage.grdVariables.ShowTitle = System.Windows.Visibility.Collapsed;
            ActivityVariablesFrame.Content = mActivityVariablesPage;

            mActionsPage = new ActionsPage();
            mActionsPage.grdActions.ShowTitle = System.Windows.Visibility.Collapsed;
            ActivityActionsFrame.Content = mActionsPage;

            mReposiotryPage = new RepositoryPage();
            RepositoryFrame.Content = mReposiotryPage;
        }

        private void SetExpanders()
        {
            // We do UI changes using the dispatcher since it might trgigger from STA like IB
            App.MainWindow.Dispatcher.Invoke(() =>
                    {
                        //set daynamic expanders titles
                        if (App.BusinessFlow != null)
                        {
                            App.BusinessFlow.PropertyChanged -= BusinessFlow_PropertyChanged;
                            App.BusinessFlow.PropertyChanged += BusinessFlow_PropertyChanged;

                            CurrentBusExpanderLable.Content = "'" + App.BusinessFlow.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);

                            mCurrentBusPage = new BusinessFlowPage(App.BusinessFlow, true);
                            CurrentBusFrame.Content = mCurrentBusPage;

                            UpdateCurrentActiityExpander();
                        }
                        else
                        {
                            CurrentBusExpanderLable.Content = "No " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                        }

                    });
        }

        private void UpdateCurrentActiityExpander()
        {
            if (Dispatcher.CheckAccess())
            {
                if (App.BusinessFlow.CurrentActivity != null)
                {
                    ActivityVariablesExpanderLabel.Content = "'" + App.BusinessFlow.CurrentActivity.ActivityName + "' - " + GingerDicser.GetTermResValue(eTermResKey.Activity, suffixString: " ") + GingerDicser.GetTermResValue(eTermResKey.Variables);
                    ActivityActionsExpanderLabel.Content = "'" + App.BusinessFlow.CurrentActivity.ActivityName + "' - " + GingerDicser.GetTermResValue(eTermResKey.Activity, suffixString: " ") + "Actions";
                }
                else
                {
                    ActivityVariablesExpanderLabel.Content = GingerDicser.GetTermResValue(eTermResKey.Activity) + " - " + GingerDicser.GetTermResValue(eTermResKey.Activity, suffixString: " ") + GingerDicser.GetTermResValue(eTermResKey.Variables);
                    ActivityActionsExpanderLabel.Content = GingerDicser.GetTermResValue(eTermResKey.Activity) + " - " + GingerDicser.GetTermResValue(eTermResKey.Activity, suffixString: " ") + "Actions";
                }
            }
        }

        private void SetGherkinOptions()
        {
            if (App.BusinessFlow != null && App.BusinessFlow.Source == BusinessFlow.eSource.Gherkin)
            {
                Gherkin.Visibility = Visibility.Visible;
                GenerateScenario.Visibility = Visibility.Visible;
                CleanScenario.Visibility = Visibility.Visible;
                OpenFeatureFile.Visibility = Visibility.Visible;
            }
            else
            {
                Gherkin.Visibility = Visibility.Collapsed;
                GenerateScenario.Visibility = Visibility.Collapsed;
                CleanScenario.Visibility = Visibility.Collapsed;
                OpenFeatureFile.Visibility = Visibility.Collapsed;
            }
        }

        public void SetGridsView(string viewName)
        {
            if (mActionsPage != null)
            {
                mActionsPage.grdActions.ChangeGridView(viewName);
            }
            if (mActivitiesPage != null)
            {
                mActivitiesPage.grdActivities.ChangeGridView(viewName);
            }
        }

        private void UserProfilePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Handle Solution change
            //TODO: cleanup close current biz flow etc...
            if (e.PropertyName == nameof(UserProfile.Solution))
            {
                
                lstEnvs.ItemsSource = null;

                if (App.UserProfile.Solution == null)
                {
                    return;
                }

                if (mReposiotryPage != null)
                {
                    mReposiotryPage.RefreshCurrentRepo();
                }
                App.UpdateApplicationsAgentsMapping();
                BindEnvsCombo();
            }
        }

        private void BindEnvsCombo()
        {
            lstEnvs.ItemsSource = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().AsCollectionViewOrderBy(nameof(ProjEnvironment.Name));
            lstEnvs.DisplayMemberPath = nameof(ProjEnvironment.Name);
            lstEnvs.SelectedValuePath = nameof(ProjEnvironment.Guid);

            if (App.UserProfile.Solution != null)
            {
                //select last used environment
                if (lstEnvs.Items != null && lstEnvs.Items.Count > 0)
                {
                    if (lstEnvs.Items.Count > 1 && App.UserProfile.RecentEnvironment != null && App.UserProfile.RecentEnvironment != Guid.Empty)
                    {
                        foreach (object env in lstEnvs.Items)
                        {
                            if (((ProjEnvironment)env).Guid == App.UserProfile.RecentEnvironment)
                            {
                                lstEnvs.SelectedIndex = lstEnvs.Items.IndexOf(env);
                                return;
                            }
                        }
                    }

                    //defualt selection
                    lstEnvs.SelectedIndex = 0;
                }
            }

            //move to top after bind
            if (lstEnvs.Items.Count == 0)
            {
                CreateDefaultEnvironment();
                lstEnvs.SelectedItem = lstEnvs.Items[0];
            }
        }

        public static void CreateDefaultEnvironment()
        {
            ProjEnvironment newEnv = new ProjEnvironment() { Name = "Default" };
            WorkSpace.Instance.SolutionRepository.AddRepositoryItem(newEnv);

            // Add all solution target app
            foreach (ApplicationPlatform AP in App.UserProfile.Solution.ApplicationPlatforms)
            {
                EnvApplication EA = new EnvApplication();
                EA.Name = AP.AppName;
                EA.CoreProductName = AP.Core;
                EA.CoreVersion = AP.CoreVersion;
                EA.Active = true;
                newEnv.Applications.Add(EA);
            }
        }

        private void CurrentBusExpander_Expanded(object sender, RoutedEventArgs e)
        {
            CurrentBusRow.Height = new GridLength(215, GridUnitType.Star);
        }

        private void CurrentBusExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            CurrentBusRow.Height = new GridLength(35);
        } 

        private void RepositoryExpander_ExpandedCollapsed(object sender, RoutedEventArgs e)
        {
            if (RepositoryExpander.IsExpanded)
            {
                RepositoryExpanderLabel.Visibility = System.Windows.Visibility.Collapsed;
                RepositoryGridColumn.Width = mlastRepositoryColWidth;
            }
            else
            {
                mlastRepositoryColWidth = RepositoryGridColumn.Width;
                RepositoryExpanderLabel.Visibility = System.Windows.Visibility.Visible;
                RepositoryGridColumn.Width = mMinColsExpanderSize;
            }
        }

        private void RepositoryExpander_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (RepositoryExpander.IsExpanded == false && e.NewSize.Width > mMinColsExpanderSize.Value)
            {
                RepositoryGridColumn.Width = mMinColsExpanderSize;
            }
        }

        private void RepositoryFrame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (RepositoryExpander.IsExpanded && e.NewSize.Width <= 50)
            {
                RepositoryExpander.IsExpanded = false;                
                mlastRepositoryColWidth = new GridLength(300);
                RepositoryGridColumn.Width = mMinColsExpanderSize;
            }
        }
      
        private void BFActivitiesFrame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height < 70)
            {
                if (mActivitiesMiniPage == null)
                {
                    mActivitiesMiniPage = new ActivitiesMiniViewPage();
                }
                BFActivitiesFrame.Content = mActivitiesMiniPage;
            }
            else
            {
                BFActivitiesFrame.Content = mActivitiesPage;
            }
        }        

        private void Expanders_Changed(object sender, RoutedEventArgs e)
        {
            Expanders_Changed();
        }

        private void Expanders_Changed()
        {
            if (BFVariablesExpander == null || BFActivitiesExpander == null || ActivityVariablesExpander == null || ActivityActionsExpander == null)
            {
                return;
            }

            int rowIndex = 3;   //were dynamic content starts       
            if (BFVariablesExpander != null)
            {
                if (BFVariablesExpander.IsExpanded)
                {
                    if (BFVariablesFrame != null && BFVariablesFrame.ActualHeight != 0)
                    {
                        mlastBFVariablesRowHeight = new GridLength(BFVariablesFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }
                    BFVariablesExpander.SetValue(Grid.RowProperty, ++rowIndex);
                    PageMainGrid.RowDefinitions[rowIndex].MinHeight = mMinRowsExpanderSize.Value;
                    PageMainGrid.RowDefinitions[rowIndex].Height = mlastBFVariablesRowHeight;
                    BFVariablesExpander.Visibility = System.Windows.Visibility.Visible;
                    BFVariablesExpander2.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    if (BFVariablesFrame != null && BFVariablesFrame.ActualHeight != 0)
                    {
                        mlastBFVariablesRowHeight = new GridLength(BFVariablesFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }                        
                    BFVariablesExpander.Visibility = System.Windows.Visibility.Collapsed;
                    BFVariablesExpander2.Visibility = System.Windows.Visibility.Visible;
                }
            }

            if (BFActivitiesGroupsExpander != null)
            {
                if (BFActivitiesGroupsExpander.IsExpanded)
                {
                    if (BFActivitiesGroupsFrame != null && BFActivitiesGroupsFrame.ActualHeight != 0)
                    {
                        mlastBFActivitiesGroupsRowHeight = new GridLength(BFActivitiesGroupsFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }
                    BFActivitiesGroupsExpander.SetValue(Grid.RowProperty, ++rowIndex);
                    PageMainGrid.RowDefinitions[rowIndex].MinHeight = mMinRowsExpanderSize.Value;
                    PageMainGrid.RowDefinitions[rowIndex].Height = mlastBFActivitiesGroupsRowHeight;
                    BFActivitiesGroupsExpander.Visibility = System.Windows.Visibility.Visible;
                    BFActivitiesGroupsExpander2.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    if (BFActivitiesGroupsFrame != null && BFActivitiesGroupsFrame.ActualHeight != 0)
                    {
                        mlastBFActivitiesGroupsRowHeight = new GridLength(BFActivitiesGroupsFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }
                    BFActivitiesGroupsExpander.Visibility = System.Windows.Visibility.Collapsed;
                    BFActivitiesGroupsExpander2.Visibility = System.Windows.Visibility.Visible;
                }
            }

            if (BFActivitiesExpander != null)
            {
                if (BFActivitiesExpander.IsExpanded)
                {
                    if (BFActivitiesFrame != null && BFActivitiesFrame.ActualHeight != 0)
                    {
                        mlastBFActivitiesRowHeight = new GridLength(BFActivitiesFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }
                    BFActivitiesExpander.SetValue(Grid.RowProperty, ++rowIndex);
                    PageMainGrid.RowDefinitions[rowIndex].MinHeight = mMinRowsExpanderSize.Value;
                    PageMainGrid.RowDefinitions[rowIndex].Height = mlastBFActivitiesRowHeight;
                    BFActivitiesExpander.Visibility = System.Windows.Visibility.Visible;
                    BFActivitiesExpander2.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    if (BFActivitiesFrame != null && BFActivitiesFrame.ActualHeight != 0)
                    {
                        mlastBFActivitiesRowHeight = new GridLength(BFActivitiesFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }
                    BFActivitiesExpander.Visibility = System.Windows.Visibility.Collapsed;
                    BFActivitiesExpander2.Visibility = System.Windows.Visibility.Visible;
                }
            }

            if (ActivityVariablesExpander != null)
            {
                if (ActivityVariablesExpander.IsExpanded)
                {
                    if (ActivityVariablesFrame != null && ActivityVariablesFrame.ActualHeight != 0)
                    {
                        mlastActivityVariablesRowHeight = new GridLength(ActivityVariablesFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }
                    ActivityVariablesExpander.SetValue(Grid.RowProperty, ++rowIndex);
                    PageMainGrid.RowDefinitions[rowIndex].MinHeight = mMinRowsExpanderSize.Value;
                    PageMainGrid.RowDefinitions[rowIndex].Height = mlastActivityVariablesRowHeight;
                    ActivityVariablesExpander.Visibility = System.Windows.Visibility.Visible;
                    ActivityVariablesExpander2.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    if (ActivityVariablesFrame != null && ActivityVariablesFrame.ActualHeight != 0)
                    {
                        mlastActivityVariablesRowHeight = new GridLength(ActivityVariablesFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }
                    ActivityVariablesExpander.Visibility = System.Windows.Visibility.Collapsed;
                    ActivityVariablesExpander2.Visibility = System.Windows.Visibility.Visible;
                }
            }

            if (ActivityActionsExpander != null)
            {
                if (ActivityActionsExpander.IsExpanded)
                {
                    if (ActivityActionsFrame != null && ActivityActionsFrame.ActualHeight != 0)
                    {
                        mlastActivitiyActionsRowHeight = new GridLength(ActivityActionsFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }
                    ActivityActionsExpander.SetValue(Grid.RowProperty, ++rowIndex);
                    PageMainGrid.RowDefinitions[rowIndex].MinHeight = mMinRowsExpanderSize.Value;
                    PageMainGrid.RowDefinitions[rowIndex].Height = mlastActivitiyActionsRowHeight;
                    ActivityActionsExpander.Visibility = System.Windows.Visibility.Visible;
                    ActivityActionsExpander2.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    if (ActivityActionsFrame != null && ActivityActionsFrame.ActualHeight != 0)
                    {
                        mlastActivitiyActionsRowHeight = new GridLength(ActivityActionsFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }
                    ActivityActionsExpander.Visibility = System.Windows.Visibility.Collapsed;
                    ActivityActionsExpander2.Visibility = System.Windows.Visibility.Visible;
                }
            }

            //hide all unneeded rows
            for (int indx = 8; indx > rowIndex; indx--)
            {
                PageMainGrid.RowDefinitions[indx].MinHeight = 0;
                PageMainGrid.RowDefinitions[indx].Height = new GridLength(0);
            }

            //Disable all unneeded splitters
            List<GridSplitter> gridSplitters = new List<GridSplitter>();
            gridSplitters.Add(Row1Splitter);
            gridSplitters.Add(Row2Splitter);
            gridSplitters.Add(Row3Splitter);
            gridSplitters.Add(Row4Splitter);
            gridSplitters.Add(Row5Splitter);
            for (int indx = 1; indx <= gridSplitters.Count; indx++)
            {
                if (gridSplitters[indx - 1] != null)
                {
                    if (indx < (rowIndex-1))
                    {
                        gridSplitters[indx - 1].IsEnabled = true;
                    }                        
                    else
                    {
                        gridSplitters[indx - 1].IsEnabled = false;
                    }                        
                }
            }

            //arange expanders menu look 
            if (rowIndex == 8)
            {
                PageMainGrid.RowDefinitions[3].Height = new GridLength(0);
            }
            else
            {
                PageMainGrid.RowDefinitions[3].Height = new GridLength(35);
            }

            //make sure at least one expander open
            if (rowIndex == 1)
            {
                BFActivitiesExpander.IsExpanded = true;
            }
        }

        private void BusinessFlow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (e.PropertyName == nameof(BusinessFlow.CurrentActivity))
                {
                    UpdateCurrentActiityExpander();
                }
                else if(e.PropertyName == BusinessFlow.Fields.Name)
                {
                    CurrentBusExpanderLable.Content = "'" + App.BusinessFlow.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                }
            });
        }

        private void AppPropertychanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(App.BusinessFlow))
            {
                SetExpanders();
                SetGherkinOptions();
            }
        }

        private void SaveBizFlowButton_Click(object sender, RoutedEventArgs e)
        {
            //warn in case dynamic shared reposiotry Activities are included and going to be deleted
            if (App.BusinessFlow.Activities.Where(x => x.AddDynamicly == true).FirstOrDefault() != null)
            {
                if (Reporter.ToUser(eUserMsgKeys.WarnOnDynamicActivities) == MessageBoxResult.No)
                {
                    return;
                }
            }

            Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, App.BusinessFlow.Name,
                                      GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(App.BusinessFlow);
            Reporter.CloseGingerHelper();
        }

        private void UndoBizFlowChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.BusinessFlow != null && Reporter.ToUser(eUserMsgKeys.AskIfSureWantToUndoChange) == MessageBoxResult.Yes)
            {
                Reporter.ToGingerHelper(eGingerHelperMsgKey.UndoChanges, null, App.BusinessFlow.Name);
                App.BusinessFlow.RestoreFromBackup();
                App.BusinessFlow.SaveBackup();
                Reporter.CloseGingerHelper();
            }
        }

        private void AutomateAnalyzerButton_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("AutomateAnalyzerButton_Click");

            AnalyzerPage AP = new AnalyzerPage();
            AP.Init(App.UserProfile.Solution, App.BusinessFlow);
            AP.ShowAsWindow();

            AutoLogProxy.UserOperationEnd();
        }

        private void btnActionConversion_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("btnConversionMechanism_Click");
            ActionConverterPage gtb = new ActionConverterPage(App.BusinessFlow);
            // combine in the abover constructor
            gtb.Init(App.UserProfile.Solution, App.BusinessFlow);
            gtb.ShowAsWindow();
            AutoLogProxy.UserOperationEnd();
        }

        private void btnResetFlow_Click(object sender, RoutedEventArgs e)
        {
            ResetStatusRunFRomAutomateTab(Run.GingerRunner.eResetStatus.All);
        }

        private void btnResetFromCurrentActivity_Click(object sender, RoutedEventArgs e)
        {
            ResetStatusRunFRomAutomateTab(Run.GingerRunner.eResetStatus.FromSpecificActivityOnwards);
        }

        private void btnResetFromCurrentAction_Click(object sender, RoutedEventArgs e)
        {
            ResetStatusRunFRomAutomateTab(Run.GingerRunner.eResetStatus.FromSpecificActionOnwards);
        }

        private async Task ResetStatusRunFRomAutomateTab(Run.GingerRunner.eResetStatus resetFrom)
        {
            try
            {
                AutoLogProxy.UserOperationStart("ResetStatusFrom" + resetFrom.ToString() + "_Click", App.UserProfile.Solution.Name, App.GetProjEnvironmentName());
                App.AutomateTabGingerRunner.ExecutionLogger.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.Reset;
                switch (resetFrom)
                {
                    case Run.GingerRunner.eResetStatus.All:
                        App.AutomateTabGingerRunner.ResetStatus(Run.GingerRunner.eContinueLevel.StandalonBusinessFlow, Run.GingerRunner.eResetStatus.All, App.BusinessFlow);
                        break;
                    case Run.GingerRunner.eResetStatus.FromSpecificActivityOnwards:
                        App.AutomateTabGingerRunner.ResetStatus(Run.GingerRunner.eContinueLevel.StandalonBusinessFlow, Run.GingerRunner.eResetStatus.FromSpecificActivityOnwards, App.BusinessFlow, App.BusinessFlow.CurrentActivity);
                        break;
                    case Run.GingerRunner.eResetStatus.FromSpecificActionOnwards:
                        App.AutomateTabGingerRunner.ResetStatus(Run.GingerRunner.eContinueLevel.StandalonBusinessFlow, Run.GingerRunner.eResetStatus.FromSpecificActionOnwards, App.BusinessFlow, App.BusinessFlow.CurrentActivity, (Act)App.BusinessFlow.CurrentActivity.Acts.CurrentItem);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                AutoLogProxy.UserOperationEnd();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error occured on Reset Status Run from Automate Tab", ex);
                throw ex;                
            }
        }

        private void FindAndReplaceAutomatePage_Click(object sender, RoutedEventArgs e)
        {
            AutomateFindAndReplace();
        }

        FindAndReplacePage mfindAndReplacePageAutomate = null;
        private void AutomateFindAndReplace()
        {
            if (mfindAndReplacePageAutomate == null)
            {
                mfindAndReplacePageAutomate = new FindAndReplacePage(FindAndReplacePage.eContext.AutomatePage);
            }
            mfindAndReplacePageAutomate.ShowAsWindow();
        }

        private void GenerateScenarioButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.BusinessFlow != null)
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                ScenariosGenerator SG = new ScenariosGenerator();
                SG.CreateScenarios(App.BusinessFlow);
                int cnt = App.BusinessFlow.ActivitiesGroups.Count;               
                int optCount = App.BusinessFlow.ActivitiesGroups.Where(z => z.Name.StartsWith("Optimized Activities")).Count();
                if (optCount > 0)
                {
                    cnt = cnt - optCount;
                }
                Reporter.ToUser(eUserMsgKeys.GherkinScenariosGenerated, cnt);
                Mouse.OverrideCursor = null;
            }
        }

        private void CleanScenarioButton_Click(object sender, RoutedEventArgs e)
        {
            ScenariosGenerator SG = new ScenariosGenerator();
            SG.ClearOptimizedScenariosVariables(App.BusinessFlow);
            SG.ClearGeneretedActivites(App.BusinessFlow);
        }

        private void OpenFeatureFileButton_Click(object sender, RoutedEventArgs e)
        {
            DocumentEditorPage documentEditorPage = new DocumentEditorPage(App.AutomateTabGingerRunner.CurrentBusinessFlow.ExternalID.Replace("~", App.UserProfile.Solution.Folder), true);
            documentEditorPage.Title = "Gherkin Page";
            documentEditorPage.Height = 700;
            documentEditorPage.Width = 1000;
            documentEditorPage.ShowAsWindow();
        }

        public void RunActionButton_Click(object sender, RoutedEventArgs e)
        {
            RunCurrentAutomatePageAction();
        }

        private void btnRunActivity_Click(object sender, RoutedEventArgs e)
        {
            ActionsPage ActPage = (ActionsPage)ActivityActionsFrame.Content;
            try
            {
                btnGridViewExecution_Click(sender, e);

                DisableGridSelectedItemChangeOnClick(ActPage.grdActions);
                AutoLogProxy.UserOperationStart("btnRunActivity_Click", App.UserProfile.Solution.Name, App.GetProjEnvironmentName());

                SetAutomateTabRunnerForExecution();

                App.AutomateTabGingerRunner.ExecutionLogger.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ActivityRun;
                RunActivity();
                AutoLogProxy.UserOperationEnd();
            }
            finally
            {
                EnabelGridSelectedItemChangeOnClick(ActPage.grdActions);
                if (App.AutomateTabGingerRunner.CurrentBusinessFlow.CurrentActivity.CurrentAgent != null)
                {
                    App.AutomateTabGingerRunner.CurrentBusinessFlow.CurrentActivity.CurrentAgent.IsFailedToStart = false;
                }                    
            }
        }

        // Run Preparations before execution of Action/activity/Flow/start agent
        public void SetAutomateTabRunnerForExecution()
        {
            App.AutomateTabGingerRunner.ProjEnvironment = App.AutomateTabEnvironment;
            App.AutomateTabGingerRunner.SolutionFolder = App.UserProfile.Solution.Folder;
            App.AutomateTabGingerRunner.DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerCore.DataSource.DataSourceBase>();
            App.AutomateTabGingerRunner.SolutionAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();            
            App.AutomateTabGingerRunner.SolutionApplications = App.UserProfile.Solution.ApplicationPlatforms;

            SetGingerRunnerSpeed();
        }

        public async Task RunActivity()
        {
            await App.AutomateTabGingerRunner.RunActivityAsync(App.BusinessFlow.CurrentActivity, false).ConfigureAwait(false);

            //When running Runactivity as standalone from GUI, SetActionSkipStatus is not called. Handling it here for now.
            foreach (Act act in App.BusinessFlow.CurrentActivity.Acts)
            {
                if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending)
                {
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                }
            }
        }

        private void SetGingerRunnerSpeed()
        {
            App.AutomateTabGingerRunner.SetSpeed(int.Parse(cboSpeed.Text));
        }

        private void DisableGridSelectedItemChangeOnClick(ucGrid UCGrid)
        {
            UCGrid.DisableUserItemSelectionChange = true;
        }

        private void EnabelGridSelectedItemChangeOnClick(ucGrid UCGrid)
        {
            UCGrid.DisableUserItemSelectionChange = false;
        }


        private void btnRunFlow_Click(object sender, RoutedEventArgs e)
        {
            RunAutomateTabFlow(true);
        }

        private void btnRunFlowNoAnaylze_Click(object sender, RoutedEventArgs e)
        {
            RunAutomateTabFlow();
        }

        private void btnRunFlowAndGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            RunAutomateTabFlow(true, true);
        }
        
        private async Task RunAutomateTabFlow(bool Analyz = false, bool ReportNeeded = false)
        {
            if (Analyz)
            {
                //Run Analyzer check if not including any High or Critical issues before execution
                Reporter.ToGingerHelper(eGingerHelperMsgKey.AnalyzerIsAnalyzing, null, App.BusinessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                try
                {
                    AnalyzerPage analyzerPage = new AnalyzerPage();
                    await Task.Run(() =>
                    {
                        analyzerPage.Init(App.UserProfile.Solution, App.BusinessFlow);
                        analyzerPage.AnalyzeWithoutUI();
                    });
                    while (analyzerPage.IsAnalyzeDone == false)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                    Reporter.CloseGingerHelper();
                    if (analyzerPage.TotalHighAndCriticalIssues > 0)
                    {
                        Reporter.ToUser(eUserMsgKeys.AnalyzerFoundIssues);
                        analyzerPage.ShowAsWindow();
                        return;
                    }
                }
                finally
                {
                    Reporter.CloseGingerHelper();
                }
            }
            try
            {
                if (ReportNeeded)
                {
                    RunAutomateTabFlowConf("Run Automate Tab Flow And Generate Report");
                }
                else
                {
                    RunAutomateTabFlowConf("Run Automate Tab Flow");
                }
                //execute
                await App.AutomateTabGingerRunner.RunBusinessFlowAsync(App.BusinessFlow, true, false).ConfigureAwait(false);
                this.Dispatcher.Invoke(() =>
                {
                    AutoLogProxy.UserOperationEnd();
                    if (ReportNeeded)
                    {
                        btnLastExecutionHTMLReport_click(this, null);
                    }
                    else
                    {
                        ExecutionSummaryPage w = new ExecutionSummaryPage(App.BusinessFlow);
                        w.ShowAsWindow();
                    }
                });

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //enable grids
                EnableDisableAutomateTabGrids(true);
                App.AutomateTabGingerRunner.ResetFailedToStartFlagForAgents();
            }
        }

        private void RunAutomateTabFlowConf(string runType)
        {
            btnGridViewExecution_Click(null, null);//shift to execution view

            AutoLogProxy.UserOperationStart(runType, App.UserProfile.Solution.Name, App.GetProjEnvironmentName());

            //disable grids  
            EnableDisableAutomateTabGrids(false);

            //execute preperations
            SetAutomateTabRunnerForExecution();
            App.AutomateTabGingerRunner.ResetRunnerExecutionDetails(true);
            App.AutomateTabGingerRunner.ExecutionLogger.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.BussinessFlowRun;
        }

        private void EnableDisableAutomateTabGrids(bool enableGrids)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    ActivitiesPage AP = null;
                    if (BFActivitiesFrame.Content.GetType() == typeof(ActivitiesPage)) // it can be the Activities MiniView
                    {
                        AP = (ActivitiesPage)BFActivitiesFrame.Content;
                    }

                    ActionsPage ActPage = (ActionsPage)ActivityActionsFrame.Content;
                    if (ActPage != null)
                    {
                        if (enableGrids)
                        {
                            EnabelGridSelectedItemChangeOnClick(ActPage.grdActions);
                        }
                        else
                        {
                            DisableGridSelectedItemChangeOnClick(ActPage.grdActions);
                        }
                    }

                    if (AP != null)
                    {
                        if (enableGrids)
                        {
                            EnabelGridSelectedItemChangeOnClick(AP.grdActivities);
                        }
                        else
                        {
                            DisableGridSelectedItemChangeOnClick(AP.grdActivities);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to disable Automate Tab grids for execution", ex);
            }
        }


        public async Task RunCurrentAutomatePageAction(bool checkIfActionAllowedToRun = true)
        {
            AutoLogProxy.UserOperationStart("RunActionButton_Click", App.UserProfile.Solution.Name, App.GetProjEnvironmentName());

            //TODO: Check if grid we are in execution view, no need to try and change of already in correct view
            btnGridViewExecution_Click(null, null);

            if (App.BusinessFlow.CurrentActivity.Acts.Count() == 0)
            {
                Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, "No Action to Run.");
                return;
            }

            SetAutomateTabRunnerForExecution();

            // If no action selected move to the first.
            if (App.BusinessFlow.CurrentActivity.Acts.CurrentItem == null && App.BusinessFlow.CurrentActivity.Acts.Count() > 0)
            {
                App.BusinessFlow.CurrentActivity.Acts.CurrentItem = App.BusinessFlow.CurrentActivity.Acts[0];
            }

            //No need of agent for actions like DB and read for excel. For other need agent   
            if (!(typeof(ActWithoutDriver).IsAssignableFrom(App.AutomateTabGingerRunner.CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem.GetType())) || App.BusinessFlow.CurrentActivity.Acts.CurrentItem.GetType() == typeof(ActAgentManipulation))
            {
                App.AutomateTabGingerRunner.SetCurrentActivityAgent();
            }

            App.AutomateTabGingerRunner.ExecutionLogger.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ActionRun;
            var result = await App.AutomateTabGingerRunner.RunActionAsync((Act)App.BusinessFlow.CurrentActivity.Acts.CurrentItem, checkIfActionAllowedToRun, true).ConfigureAwait(false);

            if (App.AutomateTabGingerRunner.CurrentBusinessFlow.CurrentActivity.CurrentAgent != null)
            {
                App.AutomateTabGingerRunner.CurrentBusinessFlow.CurrentActivity.CurrentAgent.IsFailedToStart = false;
            }

            AutoLogProxy.UserOperationEnd();
        }

        private void StopRunButton_Click(object sender, RoutedEventArgs e)
        {
            StopAutomateRun();
        }

        public void StopAutomateRun()
        {
            try
            {
                App.AutomateTabGingerRunner.StopRun();
                AutoLogProxy.UserOperationStart("StopRunButton_Click");
            }
            finally
            {
                EnableDisableAutomateTabGrids(true);
                AutoLogProxy.UserOperationEnd();
            }
        }

        private void btnContinute_Click(object sender, RoutedEventArgs e)
        {
            ContinueRunFRomAutomateTab(Run.GingerRunner.eContinueFrom.LastStoppedAction);
        }

        private void ContinuteRunActiviytButton_Click(object sender, RoutedEventArgs e)
        {
            ContinueRunFRomAutomateTab(Run.GingerRunner.eContinueFrom.SpecificActivity);
        }

        private async Task ContinueRunFRomAutomateTab(Run.GingerRunner.eContinueFrom continueFrom)
        {
            try
            {
                btnGridViewExecution_Click(null, null);
                EnableDisableAutomateTabGrids(false);

                AutoLogProxy.UserOperationStart("ContinuteRunFrom" + continueFrom.ToString() + "_Click", App.UserProfile.Solution.Name, App.GetProjEnvironmentName());
                App.AutomateTabGingerRunner.ExecutionLogger.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ContinueRun;
                switch (continueFrom)
                {
                    case Run.GingerRunner.eContinueFrom.LastStoppedAction:
                        await App.AutomateTabGingerRunner.ContinueRunAsync(Run.GingerRunner.eContinueLevel.StandalonBusinessFlow, Run.GingerRunner.eContinueFrom.LastStoppedAction);
                        break;
                    case Run.GingerRunner.eContinueFrom.SpecificAction:
                        await App.AutomateTabGingerRunner.ContinueRunAsync(Run.GingerRunner.eContinueLevel.StandalonBusinessFlow, Run.GingerRunner.eContinueFrom.SpecificAction, App.BusinessFlow, App.BusinessFlow.CurrentActivity, (Act)App.BusinessFlow.CurrentActivity.Acts.CurrentItem);
                        break;
                    case Run.GingerRunner.eContinueFrom.SpecificActivity:
                        await App.AutomateTabGingerRunner.ContinueRunAsync(Run.GingerRunner.eContinueLevel.StandalonBusinessFlow, Run.GingerRunner.eContinueFrom.SpecificActivity, App.BusinessFlow, App.BusinessFlow.CurrentActivity);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                AutoLogProxy.UserOperationEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                EnableDisableAutomateTabGrids(true);
            }
        }

        private void ContinuteRunButton_Click(object sender, RoutedEventArgs e)
        {
            ContinueRunFRomAutomateTab(Run.GingerRunner.eContinueFrom.SpecificAction);
        }

        private void StartAgent_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("StartAgent_Click");

            string agentsNames = App.AutomateTabGingerRunner.GetAgentsNameToRun();
            Reporter.ToGingerHelper(eGingerHelperMsgKey.StartAgents, null, agentsNames);

            App.AutomateTabGingerRunner.StopAgents();
            SetAutomateTabRunnerForExecution();
            App.AutomateTabGingerRunner.StartAgents();

            Reporter.CloseGingerHelper();
            AutoLogProxy.UserOperationEnd();
        }

        private void btnGridViewAll_Click(object sender, RoutedEventArgs e)
        {          
           SetGridsView(Ginger.UserControls.GridViewDef.DefaultViewName);
        }

        private void btnGridViewDesign_Click(object sender, RoutedEventArgs e)
        {
            SetGridsView(eAutomatePageViewStyles.Design.ToString());
        }

        private void btnGridViewExecution_Click(object sender, RoutedEventArgs e)
        {
           SetGridsView(eAutomatePageViewStyles.Execution.ToString());
        }

        private void SummeryReportButton_Click(object sender, RoutedEventArgs e)
        {
            ExecutionSummaryPage w = new ExecutionSummaryPage(App.BusinessFlow);
            w.ShowAsWindow();
        }

        public void btnLastExecutionHTMLReport_click(object sender, RoutedEventArgs e)
        {
            GenerateLastExecutedItemReport();
        }

        private void GenerateLastExecutedItemReport()
        {
            ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration = App.UserProfile.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            if (!_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {
                Reporter.ToUser(eUserMsgKeys.ExecutionsResultsProdIsNotOn);
                return;
            }
            HTMLReportsConfiguration currentConf = App.UserProfile.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            //get logger files
            string exec_folder = Ginger.Run.ExecutionLogger.GetLoggerDirectory(_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder + "\\" + Ginger.Run.ExecutionLogger.defaultAutomationTabLogName);
            //create the report
            string reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(exec_folder), true, null, null, false, currentConf.HTMLReportConfigurationMaximalFolderSize);

            if (reportsResultFolder == string.Empty)
            {
                Reporter.ToUser(eUserMsgKeys.AutomationTabExecResultsNotExists);                
            }
            else
            {
                foreach (string txt_file in System.IO.Directory.GetFiles(reportsResultFolder))
                {
                    string fileName = Path.GetFileName(txt_file);
                    if (fileName.Contains(".html"))
                    {
                        Process.Start(reportsResultFolder);
                        Process.Start(reportsResultFolder + "\\" + fileName);
                    }
                }
            }
        }

        private void btnOfflineExecutionHTMLReport_click(object sender, RoutedEventArgs e)
        {
            ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration = App.UserProfile.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            if (!_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {
                Reporter.ToUser(eUserMsgKeys.ExecutionsResultsProdIsNotOn);
                return;
            }
            HTMLReportsConfiguration currentConf = App.UserProfile.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            //create the execution logger files            
            string exec_folder = Ginger.Run.ExecutionLogger.GetLoggerDirectory(_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder + "\\" + Ginger.Run.ExecutionLogger.defaultAutomationTabOfflineLogName);

            if (Directory.Exists(exec_folder))
            {
                GingerCore.General.ClearDirectoryContent(exec_folder);
            }                
            else
            {
                Directory.CreateDirectory(exec_folder);
            }
            
            if (App.AutomateTabGingerRunner.ExecutionLogger.OfflineBusinessFlowExecutionLog(App.BusinessFlow, exec_folder))
            {
                //create the HTML report
                try
                {
                    string reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(exec_folder), true, null, null, false, currentConf.HTMLReportConfigurationMaximalFolderSize);
                    if (reportsResultFolder == string.Empty)
                    {
                        Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Failed to generate the report for the '" + App.BusinessFlow.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ", please execute it fully first.");
                        return;
                    }
                    else
                    {
                        foreach (string txt_file in System.IO.Directory.GetFiles(reportsResultFolder))
                        {
                            string fileName = Path.GetFileName(txt_file);
                            if (fileName.Contains(".html"))
                            {
                                Process.Start(reportsResultFolder);
                                Process.Start(reportsResultFolder + "\\" + fileName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eAppReporterLogLevel.WARN, "Failed to generate offline full business flow report", ex);
                    Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Failed to generate the report for the '" + App.BusinessFlow.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ", please execute it fully first.");
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Failed to generate the report for the '" + App.BusinessFlow.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ", please execute it fully first.");
            }
        }

        private void ExportBizFlowButton_Click(object sender, RoutedEventArgs e)
        {
            if (ALMIntegration.Instance.ExportBusinessFlowToALM(App.BusinessFlow))
            {
                if (Reporter.ToUser(eUserMsgKeys.AskIfToSaveBFAfterExport, App.BusinessFlow.Name) == MessageBoxResult.Yes)
                {
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, App.BusinessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(App.BusinessFlow);
                    Reporter.CloseGingerHelper();
                }
            }
        }

        private void RefreshFromALM_Click(object sender, RoutedEventArgs e)
        {
            if (App.BusinessFlow != null && App.BusinessFlow.ActivitiesGroups != null && App.BusinessFlow.ActivitiesGroups.Count > 0)
            {
                ALMIntegration.Instance.RefreshAllGroupsFromALM(App.BusinessFlow);
            }
        }

        private void ExportExecutionResultsToALM_Click(object sender, RoutedEventArgs e)
        {
            ObservableList<BusinessFlow> bfs = new ObservableList<BusinessFlow>();
            bfs.Add(App.BusinessFlow);
            ExportResultsToALMConfigPage.Instance.Init(bfs, new GingerCore.ValueExpression(App.AutomateTabEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerCore.DataSource.DataSourceBase>(), false, "", false, App.UserProfile.Solution.Variables));
            ExportResultsToALMConfigPage.Instance.ShowAsWindow();
        }

        private void lstEnvs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstEnvs != null && lstEnvs.SelectedItem != null)
            {
                App.AutomateTabEnvironment = (ProjEnvironment)lstEnvs.SelectedItem;
            }
                
        }

        private void xRibbon_Loaded(object sender, RoutedEventArgs e)
        {
            Grid child = VisualTreeHelper.GetChild((DependencyObject)sender, 0) as Grid;
            if (child != null)
            {
                child.RowDefinitions[0].Height = new GridLength(0);
            }

            RibbonRow.Height = new GridLength(120);
        }
    }
}
