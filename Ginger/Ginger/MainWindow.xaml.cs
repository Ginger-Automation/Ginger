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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Amdocs.Ginger.IO;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.ALM;
using Ginger.AnalyzerLib;
using Ginger.BusinessFlowWindows;
using Ginger.ConfigurationsLib;
using Ginger.ConflictResolve;
using Ginger.Drivers.DriversWindows;
using Ginger.Functionalities;
using Ginger.GeneralLib;
using Ginger.GeneralWindows;
using Ginger.Help;
using Ginger.MenusLib;
using Ginger.SolutionGeneral;
using Ginger.SolutionWindows;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.SourceControl;
using Ginger.User;
using GingerCore;
using GingerCore.ALM;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.UpgradeLib;
using GingerCoreNET.SourceControl;
using GingerWPF;
using GingerWPF.UserControlsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;

namespace Ginger
{
    public partial class MainWindow : Window
    {
        public enum eSolutionTabType { None, BusinessFlows, Run, Configurations, Resources };
        public eSolutionTabType SelectedSolutionTab;

        private readonly DevTimeTrackerIdle devTimeTrackerIdle; //for Pause/resume the development time

        private bool mAskUserIfToClose = true;

        ObservableList<HelpLayoutArgs> mHelpLayoutList = [];
        private bool mRestartApplication = false;
        private bool mLaunchInAdminMode = false;
        public MainWindow()
        {
            InitializeComponent();

            devTimeTrackerIdle = new();
            devTimeTrackerIdle.AttachActivityHandlers(this);

            StateChanged -= MainWindow_StateChanged;
            StateChanged += MainWindow_StateChanged;

            mRestartApplication = false;
            lblAppVersion.Content = "Version " + Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ApplicationUIversion;
            xVersionAndNewsIcon.Visibility = Visibility.Collapsed;

            mHelpLayoutList.CollectionChanged += MHelpLayoutList_CollectionChanged;

            DriverWindowHandler.Init();

            GingerCore.General.DoEvents();

        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                devTimeTrackerIdle.PauseDevelopmentTimeTracker();
            }
            else
            {
                devTimeTrackerIdle.ResumeDevelopmentTimeTracker();
            }
        }

        private void DetachEventHandlers()
        {
            if (devTimeTrackerIdle != null)
            {
                devTimeTrackerIdle.DetachActivityHandlers(this);
            }
        }

        private void TelemetryEventHandler(object sender, Amdocs.Ginger.CoreNET.TelemetryLib.Telemetry.TelemetryEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                xVersionAndNewsIcon.ToolTip = Amdocs.Ginger.CoreNET.TelemetryLib.Telemetry.VersionAndNewsInfo + ", click for details";
                xVersionAndNewsIcon.Visibility = Visibility.Visible;
            });


        }

        private void XVersionAndNewsIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            xVersionAndNewsIcon.Visibility = Visibility.Collapsed;
            VersionAndNewsPage versionAndNewsPage = new VersionAndNewsPage();
            versionAndNewsPage.ShowAsWindow();
        }


        public void Init()
        {
            bool autoLoadSolDone = false;

            try
            {
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSolutionALMMenu, Expander.VisibilityProperty, WorkSpace.Instance.UserProfile, nameof(WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures), bindingConvertor: new GingerCore.GeneralLib.BoolVisibilityConverter());

                //App
                App.AutomateBusinessFlowEvent += App_AutomateBusinessFlowEvent;

                //User Profile
                WorkSpace.Instance.PropertyChanged += Workspace_PropertyChanged;
                WorkSpace.Instance.PropertyChanged += WorkSpacePropertyChanged;
                if (WorkSpace.Instance.UserProfile.GingerStatus == eGingerStatus.Active)
                {
                    Reporter.ToStatus(eStatusMsgKey.ExitMode);
                    WorkSpace.Instance.UserProfile.DoNotAskToRecoverSolutions = false;
                }
                else if (WorkSpace.Instance.UserProfile.GingerStatus == eGingerStatus.Closed)
                {
                    WorkSpace.Instance.UserProfile.DoNotAskToRecoverSolutions = true;
                }

                WorkSpace.Instance.UserProfile.GingerStatus = eGingerStatus.Active;
                WorkSpace.Instance.UserProfile.UserProfileOperations.SaveUserProfile();
                ((UserProfileOperations)WorkSpace.Instance.UserProfile.UserProfileOperations).RecentSolutionsAsObjects.CollectionChanged += RecentSolutionsObjects_CollectionChanged;

                //Main Menu                            
                xGingerIconImg.ToolTip = Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ApplicationName + Environment.NewLine + "Version " + Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ApplicationUIversion;
                SetSolutionDependedUIElements();
                UpdateUserDetails();
                if (((UserProfileOperations)WorkSpace.Instance.UserProfile.UserProfileOperations).RecentSolutionsAsObjects.Count > 0)
                {
                    xRecentSolutionsMenuItem.Visibility = Visibility.Visible;
                }
                xBusinessFlowsListItemText.Text = GingerDicser.GetTermResValue(eTermResKey.BusinessFlows).ToUpper();
                xRunListItemText.Text = GingerDicser.GetTermResValue(eTermResKey.RunSets).ToUpper();

                //Status Bar            
                xLogErrorsPnl.Visibility = Visibility.Collapsed;
                xProcessMsgPnl.Visibility = Visibility.Collapsed;
                WorkSpace.Instance.BetaFeatures.PropertyChanged += BetaFeatures_PropertyChanged;
                SetBetaFlagIconVisibility();
                lblVersion.Content = "Version " + Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ApplicationUIversion;

                //Solution                  
                if (WorkSpace.Instance.UserProfile.AutoLoadLastSolution && !WorkSpace.Instance.RunningInExecutionMode && !WorkSpace.Instance.RunningFromUnitTest)
                {
                    AutoLoadLastSolution();
                    autoLoadSolDone = true;
                }

                //Messages
                if (!WorkSpace.Instance.UserProfile.NewHelpLibraryMessgeShown)
                {
                    Reporter.ToStatus(eStatusMsgKey.GingerHelpLibrary);
                    WorkSpace.Instance.UserProfile.NewHelpLibraryMessgeShown = true;
                }

                if (General.IsAdmin())
                {
                    xAdminModeIcon.ImageType = eImageType.AdminUser;
                    xAdminModeIcon.ToolTip = "Ginger Running In Admin Mode";
                    xAdminModeIcon.ImageForeground = (SolidColorBrush)FindResource("$PassedStatusColor");
                }
                else
                {
                    xAdminModeIcon.Foreground = (SolidColorBrush)FindResource("$HighlightColor_Yellow");
                }
                Reporter.ReporterData.PropertyChanged += ReporterDataChanged;

                WorkSpace.Instance.UserProfile.PropertyChanged += AskLisaPropertyChanged;

                DoOptionsHandler.LoadRunSetConfigEvent += DoOptionsHandler_LoadRunSetConfigEvent;
                DoOptionsHandler.LoadSharedRepoEvent += DoOptionsHandler_LoadSharedRepoEvent;
                DoOptionsHandler.LoadSourceControlDownloadPage += DoOptionsHandler_LoadSourceControlDownloadPage;
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.ApplicationInitError, ex.Message);
                Reporter.ToLog(eLogLevel.ERROR, "Error in Init Main Window", ex);
            }
            finally
            {
                HideSplash();
                if (!autoLoadSolDone && WorkSpace.Instance.Solution == null)
                {
                    AddHelpLayoutToShow("MainWindow_AddSolutionHelp", xSolutionSelectionMainMenuItem, "Click here to create new Solution or to open / download an existing one");
                }
            }

        }

        private void DoOptionsHandler_LoadSourceControlDownloadPage(object? sender, EventArgs e)
        {
            xDownloadSolutionMenuItem_Click(null, null);
        }


        /// <summary>
        /// Handles the event to load a shared repository.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="activity">The activity to load.</param>
        private void DoOptionsHandler_LoadSharedRepoEvent(object? sender, GingerCore.Activity activity)
        {
            this.Dispatcher.Invoke(() =>
            {
                try
                {
                    if (xResourcesListItem.Tag == null)
                    {
                        xResourcesListItem.Tag = ResourcesMenu.MenusPage;
                    }
                    xSolutionTabsListView.SelectedItem = xResourcesListItem;
                    var menuPage = xResourcesListItem.Tag as TwoLevelMenuPage;
                    if (menuPage == null)
                    {
                        throw new InvalidOperationException("Menu page not initialized");
                    }
                    menuPage.SelectTopMenu(0);
                    menuPage.xSubNavigationListView.SelectedIndex = 1;

                    var treeView = ((SingleItemTreeViewExplorerPage)ResourcesMenu.MenusPage.mTwoLevelMenu.MenuList[0].SubItems[1].ItemPage)?.xTreeView?.Tree;



                    if (treeView != null)
                    {

                        ITreeViewItem s = new SharedActivityTreeItem(activity);

                        treeView.SelectItemByNameAndOpenFolder(activity);

                    }



                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to load shared repository", ex);
                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to load shared repository: ", ex.Message);
                }
            });
        }

        /// <summary>
        /// Handles the event to load a run set configuration.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The run set configuration to load.</param>
        private void DoOptionsHandler_LoadRunSetConfigEvent(object? sender, Run.RunSetConfig e)
        {
            this.Dispatcher.Invoke(() =>
            {
                try
                {
                    if (xSolutionTabsListView == null || xRunListItem == null)
                    {
                        throw new InvalidOperationException("UI elements not initialized");
                    }
                    xSolutionTabsListView.SelectedItem = xRunListItem;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to load run set configuration", ex);
                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to load run set: " + ex.Message);
                }
            });
        }

        private void AskLisaPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                EnableChatBot();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    EnableChatBot();
                });
            }

        }

        private void EnableChatBot()
        {
            if (WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.AskLisaConfiguration.EnableChat == Configurations.AskLisaConfiguration.eEnableChatBot.Yes && WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures)
            {
                xChatPanel.Visibility = Visibility.Visible;
                xChatbotWindow.IsVisibleChanged += XChatbotWindow_IsVisibleChanged;
            }
            else
            {
                xChatPanel.Visibility = Visibility.Collapsed;
                xChatbotWindow.IsVisibleChanged -= XChatbotWindow_IsVisibleChanged;
            }
        }

        private void XChatbotWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (xChatbotWindow.Visibility == Visibility.Collapsed && xChatbotIcon.Visibility == Visibility.Collapsed)
            {
                xChatbotIcon.Visibility = Visibility.Visible;
            }
        }

        private void ReporterDataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ReporterData.ErrorCounter))
            {
                this.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                 delegate ()
                 {
                     if (Reporter.ReporterData.ErrorCounter == 0)
                     {
                         xLogErrorsPnl.Visibility = Visibility.Collapsed;

                     }
                     else
                     {
                         xLogErrorsPnl.Visibility = Visibility.Visible;
                         xLogErrorsLbl.Content = "[" + Reporter.ReporterData.ErrorCounter + "]";
                         xLogErrorsPnl.ToolTip = Reporter.ReporterData.ErrorCounter + " Errors were logged to Ginger log, click to view log file";
                     }
                 }
              ));
            }
        }


        private void BetaFeatures_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WorkSpace.Instance.BetaFeatures.IsUsingBetaFeatures))
            {
                SetBetaFlagIconVisibility();
            }
        }

        private void SetBetaFlagIconVisibility()
        {
            if (WorkSpace.Instance.BetaFeatures.IsUsingBetaFeatures)
            {
                xBetaFeaturesIcon.Visibility = Visibility.Visible;
            }
            else
            {
                xBetaFeaturesIcon.Visibility = Visibility.Collapsed;
            }
        }

        private void Workspace_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WorkSpace.LoadingSolution))
            {
                if (WorkSpace.Instance.LoadingSolution)
                {
                    xNoLoadedSolutionImg.Visibility = Visibility.Collapsed;
                    xMainWindowFrame.ClearAndSetContent(new LoadingPage("Loading Solution..."));
                    xMainWindowFrame.Visibility = Visibility.Visible;
                    xModifiedItemsCounter.Visibility = Visibility.Collapsed;
                    GingerCore.General.DoEvents();
                }
                else if (WorkSpace.Instance.Solution == null)
                {
                    xMainWindowFrame.Visibility = Visibility.Collapsed;
                    xNoLoadedSolutionImg.Visibility = Visibility.Visible;
                }
            }

            if (e.PropertyName == nameof(WorkSpace.ReencryptingVariables))
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (WorkSpace.Instance.ReencryptingVariables)
                    {
                        xNoLoadedSolutionImg.Visibility = Visibility.Collapsed;
                        xMainWindowFrame.ClearAndSetContent(new LoadingPage("Re-Encrypting Password Variables..."));
                        xMainWindowFrame.Visibility = Visibility.Visible;
                        GingerCore.General.DoEvents();
                    }
                    else
                    {
                        xMainWindowFrame.Visibility = Visibility.Collapsed;
                        xNoLoadedSolutionImg.Visibility = Visibility.Visible;
                        GingerCore.General.DoEvents();
                        xSolutionTabsListView.SelectedItem = null;
                        SelectBusinessFlowsMenu();
                    }
                });
            }
            if (e.PropertyName == nameof(WorkSpace.SolutionLoaded))
            {
                if (WorkSpace.Instance.SolutionLoaded)
                {
                    WorkSpace.Instance.SolutionRepository.ModifiedFiles.CollectionChanged += ModifiedFilesChanged;
                    EnableChatBot();
                    WorkSpace.Instance.Solution.AskLisaConfiguration.PropertyChanged += AskLisaPropertyChanged;

                }
            }

            if (e.PropertyName == nameof(WorkSpace.CurrentSelectedItem))
            {
                if (WorkSpace.Instance.CurrentSelectedItem != null)
                {
                    BindingHandler.ObjFieldBinding(xModifiedLabel, ImageMakerControl.ImageTypeProperty, WorkSpace.Instance.CurrentSelectedItem, nameof(RepositoryItemBase.DirtyStatusImage), BindingMode.OneWay);
                }
                else
                {
                    xModifiedLabel.ImageType = eImageType.Empty;
                }
            }
        }


        private void SetRecentSolutionsAsMenuItems()
        {
            //delete all shown Recent Solutions menu items
            for (int i = 0; i < xSolutionSelectionMainMenuItem.Items.Count; i++)
            {
                if (((MenuItem)xSolutionSelectionMainMenuItem.Items[i]).Tag is Solution)
                {
                    xSolutionSelectionMainMenuItem.Items.RemoveAt(i);
                    i--;
                }
            }

            if (xRecentSolutionsMenuItem.Tag != null)
            {
                //Insert
                int insertIndex = xSolutionSelectionMainMenuItem.Items.IndexOf(xRecentSolutionsMenuItem) + 1;
                if (((UserProfileOperations)WorkSpace.Instance.UserProfile.UserProfileOperations).RecentSolutionsAsObjects.Count > 0)
                {
                    xRecentSolutionsMenuItem.Visibility = Visibility.Visible;

                    foreach (Solution sol in ((UserProfileOperations)WorkSpace.Instance.UserProfile.UserProfileOperations).RecentSolutionsAsObjects)
                    {
                        AddSubMenuItem(xSolutionSelectionMainMenuItem, sol.Name, sol, RecentSolutionSelection_Click, insertIndex++, sol.Folder, eImageType.Solution);
                    }
                }
                else
                {
                    xRecentSolutionsMenuItem.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void RecentSolutionsObjects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (xRecentSolutionsMenuItem.Tag != null) //means it is expanded
            {
                SetRecentSolutionsAsMenuItems();
            }
        }

        private void xRecentSolutionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (xRecentSolutionsMenuItem.Tag == null)
            {
                xRecentSolutionsMenuItem.Tag = true;//expanded
            }
            else
            {
                xRecentSolutionsMenuItem.Tag = null;
            }

            SetRecentSolutionsAsMenuItems();
        }


        // New method to set staus bar text and icon
        internal void ShowStatus(eStatusMsgType messageType, string statusText)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(statusText))
                {
                    xProcessMsgPnl.Visibility = Visibility.Visible;
                    xProcessMsgTxtBlock.Text = statusText;
                    xProcessMsgTxtBlock.ToolTip = statusText;

                    switch (messageType)
                    {
                        case eStatusMsgType.PROCESS:
                            xProcessMsgIcon.ImageType = eImageType.Processing;
                            break;

                        case eStatusMsgType.INFO:
                            xProcessMsgIcon.ImageType = eImageType.Info;
                            break;
                    }

                    GingerCore.General.DoEvents();
                }
                else
                {
                    xProcessMsgPnl.Visibility = Visibility.Collapsed;
                }
            });

        }


        internal void AutoLoadLastSolution()
        {
            try
            {
                if (((UserProfileOperations)WorkSpace.Instance.UserProfile.UserProfileOperations).RecentSolutionsAsObjects.Count > 0)
                {
                    WorkSpace.Instance.OpenSolution(((UserProfileOperations)WorkSpace.Instance.UserProfile.UserProfileOperations).RecentSolutionsAsObjects[0].Folder);
                    xSolutionTabsListView.SelectedItem = null;
                    SelectBusinessFlowsMenu();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.SolutionLoadError, ex);
            }
        }

        public void WorkSpacePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Handle Solution change
            //TODO: cleanup close current biz flow etc...
            if (e.PropertyName == nameof(WorkSpace.Solution))
            {
                SetSolutionDependedUIElements();
                if (WorkSpace.Instance.Solution == null)
                {
                    xSolutionTabsListView.SelectedItem = null;
                    xSolutionNameTextBlock.Text = "Please Load Solution";
                }
                else
                {
                    xNoLoadedSolutionImg.Visibility = Visibility.Collapsed;

                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSolutionNameTextBlock, TextBlock.TextProperty, WorkSpace.Instance.Solution, nameof(Solution.Name), System.Windows.Data.BindingMode.OneWay);
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSolutionNameTextBlock, TextBlock.ToolTipProperty, WorkSpace.Instance.Solution, nameof(Solution.Folder), System.Windows.Data.BindingMode.OneWay);
                    xSolutionTabsListView.SelectedItem = null;
                    SelectBusinessFlowsMenu();
                }
            }
        }

        private void SelectBusinessFlowsMenu()
        {
            xSolutionTabsListView.SelectedItem = xBusinessFlowsListItem;
            ((TwoLevelMenuPage)xBusinessFlowsListItem.Tag).SelectFirstTopMenu();
        }

        public void CloseWithoutAsking()
        {
            mAskUserIfToClose = false;
            this.Close();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            eUserMsgSelection userSelection;
            if (mRestartApplication)
            {
                if (mLaunchInAdminMode)
                {
                    userSelection = Reporter.ToUser(eUserMsgKey.AskIfSureWantToRestartInAdminMode);
                }
                else if (WorkSpace.Instance.SolutionRepository != null && WorkSpace.Instance.SolutionRepository.ModifiedFiles.Any())
                {
                    userSelection = Reporter.ToUser(eUserMsgKey.AskIfSureWantToRestart);
                }
                else
                {
                    userSelection = Reporter.ToUser(eUserMsgKey.AskIfSureWantToRestartWithoutNote);
                }
            }
            else if (!mAskUserIfToClose)
            {
                userSelection = eUserMsgSelection.Yes;
            }
            else
            {
                if (WorkSpace.Instance.SolutionRepository != null && WorkSpace.Instance.SolutionRepository.ModifiedFiles.Any())
                {
                    userSelection = Reporter.ToUser(eUserMsgKey.AskIfSureWantToClose);
                }
                else
                {
                    userSelection = Reporter.ToUser(eUserMsgKey.AskIfSureWantToCloseWithoutNote);
                }
            }


            if (userSelection == eUserMsgSelection.Yes)
            {
                DetachEventHandlers();

                AppCleanUp();
            }
            else
            {
                mRestartApplication = false;
                mLaunchInAdminMode = false;
                e.Cancel = true;
            }
        }

        private void AppCleanUp()
        {
            ClosingWindow CW = new ClosingWindow();
            CW.Show();
            GingerCore.General.DoEvents();

            if (WorkSpace.Instance != null && WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.LoggerConfigurations != null)
            {
                while (WorkSpace.Instance.Solution.LoggerConfigurations.IsPublishToCentralDBRunning)
                {
                    Thread.Sleep(500);
                    GingerCore.General.DoEvents();
                }
            }

            WorkSpace.Instance.Close();

            GingerCore.General.CleanDirectory(GingerCore.Actions.Act.ScreenshotTempFolder, true);


            CW.Close();
        }



        private void xSolutionTopNavigationListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xMainWindowFrame == null)
            {
                return;
            }

            SelectedSolutionTab = eSolutionTabType.None;
            if (xMainWindowFrame.Content is not LoadingPage)
            {
                xMainWindowFrame.Visibility = Visibility.Collapsed;
            }
            ListViewItem selectedTopListItem = (ListViewItem)xSolutionTabsListView.SelectedItem;

            if (selectedTopListItem != null)
            {
                if (selectedTopListItem == xBusinessFlowsListItem)
                {
                    if (xBusinessFlowsListItem.Tag == null)
                    {
                        xBusinessFlowsListItem.Tag = BusinessFlowsMenu.MenusPage;
                        SelectBusinessFlowsMenu();
                    }

                    SelectedSolutionTab = eSolutionTabType.BusinessFlows;
                }
                else if (selectedTopListItem == xRunListItem)
                {
                    xRunListItem.Tag ??= RunMenu.MenusPage;
                    SelectedSolutionTab = eSolutionTabType.Run;
                    RunMenu.MenusPage.SelectFirstTopMenu();
                }
                else if (selectedTopListItem == xConfigurationsListItem)
                {
                    xConfigurationsListItem.Tag ??= ConfigurationsMenu.MenusPage;
                    SelectedSolutionTab = eSolutionTabType.Configurations;
                }
                else
                {
                    xResourcesListItem.Tag ??= ResourcesMenu.MenusPage;
                    SelectedSolutionTab = eSolutionTabType.Resources;
                }

                xMainWindowFrame.ClearAndSetContent(selectedTopListItem.Tag);
                xMainWindowFrame.Visibility = Visibility.Visible;
            }
        }


        private void xOpenSolutionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string solutionFolder = General.OpenSelectFolderDialog("Select Ginger Solution Folder");
            if (solutionFolder != null)
            {
                string solutionFileName = System.IO.Path.Combine(solutionFolder, @"Ginger.Solution.xml");
                if (System.IO.File.Exists(PathHelper.GetLongPath(solutionFileName)))
                {
                    WorkSpace.Instance.OpenSolution(Path.GetDirectoryName(PathHelper.GetLongPath(solutionFolder)));
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.SolutionFileNotFound, solutionFileName);
                }
            }
        }

        private void xCreateNewSolutionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Solution s1 = new Solution();
            SolutionOperations solutionOperations = new SolutionOperations(s1);
            s1.SolutionOperations = solutionOperations;
            AddSolutionPage addSol = new AddSolutionPage(s1);
            addSol.ShowAsWindow();
            if (addSol.IsUploadSolutionToSourceControl)
            {
                UploadSolutionMenuItem_Click(sender, e);
            }
        }

        public void SetSolutionDependedUIElements()
        {
            if (WorkSpace.Instance.Solution != null)
            {
                xLoadedSolutionMenusPnl.Visibility = Visibility.Visible;
                if (WorkSpace.Instance.UserProfile.UserTypeHelper.IsSupportAutomate)
                {
                    xRunListItem.Visibility = Visibility.Visible;
                }
                else
                {
                    xRunListItem.Visibility = Visibility.Collapsed;
                }
                if (WorkSpace.Instance.Solution.SourceControl != null)
                {
                    xSolutionSourceControlSetMenuItem.Visibility = Visibility.Visible;
                    xSolutionSourceControlInitMenuItem.Visibility = Visibility.Collapsed;
                }
                else
                {
                    xSolutionSourceControlInitMenuItem.Visibility = Visibility.Visible;
                    xSolutionSourceControlSetMenuItem.Visibility = Visibility.Collapsed;
                }

                if (!WorkSpace.Instance.RunningInExecutionMode)
                {
                    xChatbotIcon.Visibility = Visibility.Visible;
                }
                else
                {
                    xChatbotIcon.Visibility = Visibility.Collapsed;
                }
                xChatbotWindow.Visibility = Visibility.Collapsed;
            }
            else
            {
                xLoadedSolutionMenusPnl.Visibility = Visibility.Collapsed;
            }
            UpdateSourceControlIndicators();
        }

        private void UpdateSourceControlIndicators()
        {
            if (WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.SourceControl != null)
            {
                Task.Run(() =>
                {
                    List<string> conflictPaths = SourceControlIntegration.GetConflictPaths(WorkSpace.Instance.Solution.SourceControl);
                    if (conflictPaths.Any())
                    {
                        ShowConflictIndicators();
                    }
                    else
                    {
                        HideConflictIndicators();
                    }
                });
            }
            else
            {
                HideConflictIndicators();
            }
        }

        private void ShowConflictIndicators()
        {
            Dispatcher.Invoke(() =>
            {
                xSourceControlOperationsWarn.ImageType = eImageType.MediumWarn;
                xResolveConflictsWarn.ImageType = eImageType.MediumWarn;
            });
        }

        private void HideConflictIndicators()
        {
            Dispatcher.Invoke(() =>
            {
                xSourceControlOperationsWarn.ImageType = eImageType.Empty;
                xResolveConflictsWarn.ImageType = eImageType.Empty;
            });
        }

        private async void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Alt + CTRL + Shift + G = show beta features
            if (e.Key == Key.G && Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyDown(Key.LeftAlt))
            {
                BetaFeaturesPage p = new BetaFeaturesPage();
                p.ShowAsWindow();
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftAlt) && Keyboard.IsKeyDown(Key.LeftShift) && e.Key == Key.T)
            {
                Ginger.Telemetry.TelemetryConfigPage telemetryConfigPage = new();
                telemetryConfigPage.ShowAsWindow();
            }
            else if (e.Key == Key.F && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                SolutionFindAndReplace();
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S && WorkSpace.Instance.SolutionLoaded)
            {
                e.Handled = true;
                await SaveCurrentItemAsync();
                return;
            }
            else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.S && WorkSpace.Instance.SolutionLoaded)
            {
                e.Handled = true;
                ModifiedRepositoryFilesPage MRFPage = new ModifiedRepositoryFilesPage();
                MRFPage.ShowAsWindow();
                return;
            }
        }

        private void ALMConfigButton_Click(object sender, RoutedEventArgs e)
        {
            ALMConnectionPage almConnPage = new ALMConnectionPage(eALMConnectType.SettingsPage);
            almConnPage.ShowAsWindow();
        }

        private void ALMFieldsConfiguration_Click(object sender, RoutedEventArgs e)
        {
            GingerCoreNET.ALMLib.ALMConfig AlmConfig = ALMCore.GetDefaultAlmConfig();
            ALMIntegration.Instance.OpenALMItemsFieldsPage(eALMConfigType.MainMenu, ALMIntegration.Instance.GetALMType(), WorkSpace.Instance.Solution.ExternalItemsFields);
        }

        private void ALMDefectsProfiles_Click(object sender, RoutedEventArgs e)
        {
            //open defect profile page for each almtype
            ALMDefectsProfilesPage defectsProfilesPage = new ALMDefectsProfilesPage();
            defectsProfilesPage.ShowAsWindow();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            App.MainWindow.Close();
        }
        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            mRestartApplication = true;
            App.MainWindow.Close();
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutPage AP = new AboutPage();
            AP.ShowAsWindow();
        }

        private void ViewSolutionFiles_Click(object sender, RoutedEventArgs e)
        {
            //show solution folder files
            if (WorkSpace.Instance.Solution != null)
            {
                ProcessStartInfo pi = new ProcessStartInfo(WorkSpace.Instance.Solution.Folder)
                {
                    UseShellExecute = true
                };

                Process proc = new Process
                {
                    StartInfo = pi
                };

                proc.Start();
            }
        }

        private void btnSourceControlConnectionDetails_Click(object sender, RoutedEventArgs e)
        {
            SourceControlConnDetailsPage p = new SourceControlConnDetailsPage();
            p.ShowAsWindow(eWindowShowStyle.Dialog);
        }

        private void xDownloadSolutionMenuItem_Click(object sender, RoutedEventArgs e)
        {

            SourceControlProjectsPage p = new SourceControl.SourceControlProjectsPage();
            p.ShowAsWindow();
        }

        private void btnSourceControlCheckIn_Click(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.LoseChangesWarn) == Amdocs.Ginger.Common.eUserMsgSelection.No)
            {
                return;
            }

            App.CheckIn(WorkSpace.Instance.Solution.Folder);
        }
        private void btnSourceControlCreateBranch_Click(object sender, RoutedEventArgs e)
        {
            App.CreateNewBranch();
        }
        ProgressNotifier progressNotifier = null;
        /// <summary>
        /// Handles the event to get the latest changes from the source control.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void btnSourceControlGetLatest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Reporter.ToUser(eUserMsgKey.LoseChangesWarn) == Amdocs.Ginger.Common.eUserMsgSelection.No)
                {
                    return;
                }

                InitializeProgressNotifier();

                Reporter.ToStatus(eStatusMsgKey.GetLatestFromSourceControl);

                if (string.IsNullOrEmpty(WorkSpace.Instance.Solution.Folder))
                {
                    Reporter.ToUser(eUserMsgKey.SourceControlUpdateFailed, "Invalid Path provided. Please check the solution folder path.");
                }
                else
                {
                    SourceControlUI.GetLatest(WorkSpace.Instance.Solution.Folder, WorkSpace.Instance.Solution.SourceControl, progressNotifier);
                }

                App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.UpdateAppAgentsMapping, null);

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get the latest from source control. Please check the logs for more details.", ex);
            }
            finally
            {
                Reporter.HideStatusMessage();
                CleanupProgressNotifier();
            }
        }

        /// <summary>
        /// Initializes the progress notifier for source control operations.
        /// </summary>
        private void InitializeProgressNotifier()
        {
            progressNotifier = new ProgressNotifier();
            progressNotifier.StatusUpdateHandler += ProgressNotifier_ProgressUpdated;
        }

        /// <summary>
        /// Cleans up the progress notifier after source control operations.
        /// </summary>
        private void CleanupProgressNotifier()
        {
            if (progressNotifier != null)
            {
                progressNotifier.StatusUpdateHandler -= ProgressNotifier_ProgressUpdated;
            }
        }

        /// <summary>
        /// Updates the progress notifier with the current progress of the operation.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">A tuple containing the completed steps and total steps.</param>
        private void ProgressNotifier_ProgressUpdated(object? sender, (string ProgressType, int CompletedSteps, int TotalSteps) e)
        {
            try
            {
                if (e.CompletedSteps > 0 && e.TotalSteps > 0 && e.CompletedSteps <= e.TotalSteps)
                {
                    double progress = Math.Round(((double)e.CompletedSteps / e.TotalSteps) * 100, 2);
                    if (Math.Abs(progress) < 0.01)
                    {
                        return;
                    }
                    string gitProgress = $"{e.ProgressType}{progress}% complete ";
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        xProcessMsgTxtBlock.Text = gitProgress;
                    }));
                }
                else
                {
                    return;
                }
            }
            catch (Exception t)
            {
                Reporter.ToLog(eLogLevel.ERROR, t.Message);
            }
        }
        private void btnGlobalSolutionImport_Click(object sender, RoutedEventArgs e)
        {
            GingerWPF.WizardLib.WizardWindow.ShowWizard(new Ginger.GlobalSolutionLib.ImportItemWizardLib.ImportItemWizard());
        }

        private void AnalyzerButton_Click(object sender, RoutedEventArgs e)
        {
            AnalyzerPage AP = new AnalyzerPage();
            AP.Init(WorkSpace.Instance.Solution);
            AP.ShowAsWindow();
        }

        private void ResolveSourceControlConflicts(eResolveConflictsSide side)
        {
            Reporter.ToStatus(eStatusMsgKey.ResolveSourceControlConflicts);
            SourceControlIntegration.ResolveConflicts(WorkSpace.Instance.Solution.SourceControl, WorkSpace.Instance.Solution.Folder, side);
            Reporter.HideStatusMessage();
        }

        private void xResolveConflictsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            List<string> conflictPaths = SourceControlIntegration.GetConflictPaths(WorkSpace.Instance.Solution.SourceControl);
            ResolveConflictWindow resolveConflictWindow = new(conflictPaths);
            resolveConflictWindow.ShowAsWindow();
            if (resolveConflictWindow.IsResolved)
            {
                HideConflictIndicators();
            }
        }

        private void xHelpOptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (xHelpOptionsMenuItem.Tag == null)
            {
                xHelpOptionsMenuItem.Tag = "Expanded";//expanded
            }
            else
            {
                xHelpOptionsMenuItem.Tag = null;
            }

            SetHelpOptionsMenuItems();
        }

        private void btnUpgrade_Click(object sender, RoutedEventArgs e)
        {
            if (WorkSpace.Instance.Solution != null)
            {
                Solution sol = WorkSpace.Instance.Solution;
                SolutionUpgrade.ClearPreviousScans();
                ConcurrentBag<string> lowerVersionFiles = SolutionUpgrade.GetSolutionFilesCreatedWithRequiredGingerVersion(SolutionUpgrade.GetSolutionFilesWithVersion(Solution.SolutionFiles(sol.Folder)), eGingerVersionComparisonResult.LowerVersion);
                if (lowerVersionFiles != null && lowerVersionFiles.Count > 0)
                {
                    UpgradePage solutionUpgradePage = new UpgradePage(SolutionUpgradePageViewMode.UpgradeSolution, sol.Folder, sol.Name, lowerVersionFiles.ToList());
                    solutionUpgradePage.ShowAsWindow();
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Upgrade is not needed, all solution items were created with latest version.");
                }
            }
        }

        private void btnRecover_Click(object sender, RoutedEventArgs e)
        {
            if (WorkSpace.Instance.Solution != null)
            {
                WorkSpace.Instance.AppSolutionRecover.SolutionRecoverStart(true);
            }
        }

        private void btnViewLog_Click(object sender, RoutedEventArgs e)
        {
            ShowGingerLog();
        }

        LogDetailsPage mLogDetailsPage = null;
        private void btnViewLogDetails_Click(object sender, RoutedEventArgs e)
        {
            if (mLogDetailsPage == null)
            {
                mLogDetailsPage = new LogDetailsPage(LogDetailsPage.eLogShowLevel.ERROR);
            }
            else
            {
                mLogDetailsPage.Refresh();
            }
            mLogDetailsPage.ShowAsWindow();
        }

        private void ShowGingerLog()
        {
            if (System.IO.File.Exists(Amdocs.Ginger.CoreNET.log4netLib.GingerLog.GingerLogFile))
            {
                Process.Start(new ProcessStartInfo() { FileName = Amdocs.Ginger.CoreNET.log4netLib.GingerLog.GingerLogFile, UseShellExecute = true });
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Ginger log file was not found in the Path:'" + Amdocs.Ginger.CoreNET.log4netLib.GingerLog.GingerLogFile + "'");
            }
        }

        private void btnSourceControlRepositoryDetails_Click(object sender, RoutedEventArgs e)
        {

            SourceControlItemInfoDetails SCIInfoDetails = SourceControlIntegration.GetRepositoryInfo(WorkSpace.Instance.Solution.SourceControl);
            if (SCIInfoDetails != null)
            {
                SourceControlItemInfoPage SCIIP = new SourceControlItemInfoPage(SCIInfoDetails);
                SCIIP.ShowAsWindow();
            }
        }

        private void btnViewLogLocation_Click(object sender, RoutedEventArgs e)
        {

            string folder = System.IO.Path.GetDirectoryName(Amdocs.Ginger.CoreNET.log4netLib.GingerLog.GingerLogFile);
            if (System.IO.Directory.Exists(folder))
            {
                Process.Start(new ProcessStartInfo() { FileName = folder, UseShellExecute = true });
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Ginger log file folder was not found in the path: '" + folder + "'");
            }
        }

        DebugConsoleWindow mDebugConsoleWin = null;
        private void btnLaunchConsole_Click(object sender, RoutedEventArgs e)
        {
            Reporter.ReportAllAlsoToConsole = true;

            if (mDebugConsoleWin == null)
            {
                mDebugConsoleWin = new DebugConsoleWindow();
            }
            else
            {
                mDebugConsoleWin.ClearConsole();
            }

            mDebugConsoleWin.ShowAsWindow();
        }

        private void xBetaFeaturesIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BetaFeaturesPage p = new BetaFeaturesPage();
            p.ShowAsWindow();
        }

        private void xLogErrors_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LogDetailsPage logDetailsPage = new LogDetailsPage(LogDetailsPage.eLogShowLevel.ERROR);
            logDetailsPage.ShowAsWindow();

            xLogErrorsPnl.Visibility = Visibility.Collapsed;
            Reporter.ReporterData.ResetErrorCounter();
        }

        private void xFindAndReplaceSolutionButton_Click(object sender, RoutedEventArgs e)
        {
            SolutionFindAndReplace();
        }


        private void SolutionFindAndReplace()
        {
            FindAndReplacePage mfindAndReplacePageSolution = new FindAndReplacePage(FindAndReplacePage.eContext.SolutionPage);            
            mfindAndReplacePageSolution.ShowAsWindow();
        }

        private void App_AutomateBusinessFlowEvent(AutomateEventArgs args)
        {
            if (args.EventType == AutomateEventArgs.eEventType.Automate)
            {
                //TODO: load Business Flows tab
                xSolutionTabsListView.SelectedItem = xBusinessFlowsListItem;
                ((TwoLevelMenuPage)xBusinessFlowsListItem.Tag).SelectTopMenu(1);//selecting Automate menu option
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (mRestartApplication)
            {
                if (mLaunchInAdminMode)
                {
                    Process.Start(new ProcessStartInfo() { FileName = System.AppDomain.CurrentDomain.FriendlyName, UseShellExecute = true, Verb = "runas" });
                }
                else
                {
                    Process.Start(new ProcessStartInfo() { FileName = System.AppDomain.CurrentDomain.FriendlyName, UseShellExecute = true });
                }
            }

            Application.Current.Shutdown();
        }

        private void xSolutionEditBtn_Click(object sender, RoutedEventArgs e)
        {
            string newName = WorkSpace.Instance.Solution.Name;
            if (GingerCore.GeneralLib.InputBoxWindow.GetInputWithValidation("Solution Rename", "New Solution Name:", ref newName))
            {
                WorkSpace.Instance.Solution.Name = newName;
            }
        }

        private void xLoadSupportSiteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = @"http://ilrnaginger01/", UseShellExecute = true });
        }

        private void xLoadForumSiteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = @"http://ilrnaginger01:81/", UseShellExecute = true });
        }

        private void xGingerGithubMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = @"https://github.com/Ginger-Automation", UseShellExecute = true });
        }

        private void xOpenTicketMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = @"http://ilrnaginger01/Ticket/Create", UseShellExecute = true });
        }

        private void xSupportTeamMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = @"mailto:AmdocsTestingGingerDVCISupport@int.amdocs.com", UseShellExecute = true });
        }

        private void xCoreTeamMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = @"mailto:GingerCoreTeam@int.amdocs.com", UseShellExecute = true });
        }

        private void RecentSolutionSelection_Click(object sender, RoutedEventArgs e)
        {
            Solution selectedSol = (Solution)((MenuItem)sender).Tag;

            if (selectedSol != null && Directory.Exists(selectedSol.Folder))
            {
                WorkSpace.Instance.OpenSolution(selectedSol.Folder);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.SolutionLoadError, "Selected Solution was not found");
            }

            e.Handled = true;
        }

        UserSettingsPage mUserSettingsPage;
        private void xUserSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (mUserSettingsPage == null)
            {
                mUserSettingsPage = new UserSettingsPage();
            }

            mUserSettingsPage.ShowAsWindow();
        }

        UserProfilePage mUserProfilePage;
        private void xUserProfileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (mUserProfilePage == null)
            {
                mUserProfilePage = new UserProfilePage();
            }

            mUserProfilePage.ShowAsWindow();
            UpdateUserDetails();
        }

        private void UpdateUserDetails()
        {
            if (string.IsNullOrEmpty(WorkSpace.Instance.UserProfile.ProfileImage))
            {
                xProfileImageImgBrush.ImageSource = ImageMakerControl.GetImageSource(Amdocs.Ginger.Common.Enums.eImageType.User, foreground: (System.Windows.Media.SolidColorBrush)FindResource("$BackgroundColor_White"), width: 50);
            }
            else
            {
                xProfileImageImgBrush.ImageSource = Ginger.General.GetImageStream(Ginger.General.Base64StringToImage(WorkSpace.Instance.UserProfile.ProfileImage));
            }

            if (String.IsNullOrEmpty(WorkSpace.Instance.UserProfile.UserFirstName))
            {
                xUserNameLbl.Content = WorkSpace.Instance.UserProfile.UserName;
            }
            else
            {
                xUserNameLbl.Content = WorkSpace.Instance.UserProfile.UserFirstName;
            }
            string displayName;
            string OriginalUserName = Convert.ToString(xUserNameLbl.Content);
            if (OriginalUserName.Length > 10)
            {
                displayName = OriginalUserName[..7] + "...";
            }
            else
            {
                displayName = OriginalUserName;
            }
            xUserNameLbl.Content = displayName;
            xUserNameLbl.ToolTip = OriginalUserName;
        }

        private void xLogOptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (xLogOptionsMenuItem.Tag == null)
            {
                xLogOptionsMenuItem.Tag = "Expanded";//expanded
            }
            else
            {
                xLogOptionsMenuItem.Tag = null;
            }

            SetLogOptionsMenuItems();
        }

        private void SetLogOptionsMenuItems()
        {
            //delete all shown Log options sub menu items
            for (int i = 0; i < xExtraOperationsMainMenuItem.Items.Count; i++)
            {
                if ((string)((MenuItem)xExtraOperationsMainMenuItem.Items[i]).Tag == "Log")
                {
                    xExtraOperationsMainMenuItem.Items.RemoveAt(i);
                    i--;
                }
            }

            if (xLogOptionsMenuItem.Tag != null)
            {
                //Insert
                int insertIndex = xExtraOperationsMainMenuItem.Items.IndexOf(xLogOptionsMenuItem) + 1;

                AddSubMenuItem(xExtraOperationsMainMenuItem, "View Current Log Details", "Log", btnViewLogDetails_Click, insertIndex++, iconType: eImageType.View);
                AddSubMenuItem(xExtraOperationsMainMenuItem, "Open Full Log File", "Log", btnViewLog_Click, insertIndex++, iconType: eImageType.File);
                AddSubMenuItem(xExtraOperationsMainMenuItem, "Open Log File Folder", "Log", btnViewLogLocation_Click, insertIndex++, iconType: eImageType.OpenFolder);
                AddSubMenuItem(xExtraOperationsMainMenuItem, "Open Ginger Console Window", "Log", btnLaunchConsole_Click, insertIndex, iconType: eImageType.Window);
            }
        }

        private void AddSubMenuItem(MenuItem parentMenuItem, string itemHeader, object itemTag, RoutedEventHandler clickEventHandler, int insertIndex, string toolTip = "", eImageType iconType = eImageType.Null)
        {
            MenuItem subMenuItem = new MenuItem
            {
                Style = (Style)TryFindResource("$MenuItemStyle_ButtonSubMenuItem"),
                Header = itemHeader,
                Tag = itemTag
            };
            subMenuItem.Click += clickEventHandler;
            if (!string.IsNullOrEmpty(toolTip))
            {
                subMenuItem.ToolTip = toolTip;
            }
            if (iconType != eImageType.Null)
            {
                //< usercontrols:ImageMakerControl SetAsFontImageWithSize = "16" ImageType = "Edit" />
                ImageMakerControl imageMaker = new ImageMakerControl
                {
                    SetAsFontImageWithSize = 16,
                    ImageType = iconType
                };
                subMenuItem.Icon = imageMaker;
                imageMaker.Margin = new Thickness(5, 0, -5, 0);
            }
            parentMenuItem.Items.Insert(insertIndex, subMenuItem);
        }

        private void xSupportOptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (xSupportOptionsMenuItem.Tag == null)
            {
                xSupportOptionsMenuItem.Tag = "Expanded";//expanded
            }
            else
            {
                xSupportOptionsMenuItem.Tag = null;
            }

            SetSupportOptionsMenuItems();
        }

        private void SetHelpOptionsMenuItems()
        {
            //delete all Help options Sub menu items
            for (int i = 0; i < xExtraOperationsMainMenuItem.Items.Count; i++)
            {
                if ((string)((MenuItem)xExtraOperationsMainMenuItem.Items[i]).Tag == "Help")
                {
                    xExtraOperationsMainMenuItem.Items.RemoveAt(i);
                    i--;
                }
            }

            if (xHelpOptionsMenuItem.Tag != null)
            {
                //Insert
                int insertIndex = xExtraOperationsMainMenuItem.Items.IndexOf(xHelpOptionsMenuItem) + 1;

                AddSubMenuItem(xExtraOperationsMainMenuItem, "Ginger Offline Help Library", "Help", xLoadOfflineHelpMenuItem_Click, insertIndex++, iconType: eImageType.File);
                AddSubMenuItem(xExtraOperationsMainMenuItem, "Ginger Online Help Library", "Help", xLoadOnlineHelpMenuItem_Click, insertIndex, iconType: eImageType.Globe);
            }
        }

        private void SetSupportOptionsMenuItems()
        {
            //delete all Support options Sub menu items
            for (int i = 0; i < xExtraOperationsMainMenuItem.Items.Count; i++)
            {
                if ((string)((MenuItem)xExtraOperationsMainMenuItem.Items[i]).Tag == "Support")
                {
                    xExtraOperationsMainMenuItem.Items.RemoveAt(i);
                    i--;
                }
            }

            if (xSupportOptionsMenuItem.Tag != null)
            {
                //Insert
                int insertIndex = xExtraOperationsMainMenuItem.Items.IndexOf(xSupportOptionsMenuItem) + 1;

                AddSubMenuItem(xExtraOperationsMainMenuItem, "Ginger Public Site", "Support", xLoadPublicSiteMenuItem_Click, insertIndex++, iconType: eImageType.Globe);
                AddSubMenuItem(xExtraOperationsMainMenuItem, "Ginger Public Support", "Support", xLoadPublicSupportSiteMenuItem_Click, insertIndex++, iconType: eImageType.Support);
                AddSubMenuItem(xExtraOperationsMainMenuItem, "Ginger Internal Support Site", "Support", xLoadSupportSiteMenuItem_Click, insertIndex++, iconType: eImageType.Website);
                AddSubMenuItem(xExtraOperationsMainMenuItem, "Ginger Internal Q&A Fourm Site", "Support", xLoadForumSiteMenuItem_Click, insertIndex++, iconType: eImageType.Forum);
                AddSubMenuItem(xExtraOperationsMainMenuItem, "Raise Internal Ticket", "Support", xOpenTicketMenuItem_Click, insertIndex, iconType: eImageType.Ticket);
            }
        }

        private void xContactOptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (xContactOptionsMenuItem.Tag == null)
            {
                xContactOptionsMenuItem.Tag = "Expanded";//expanded
            }
            else
            {
                xContactOptionsMenuItem.Tag = null;
            }

            SetContactOptionsMenuItems();
        }

        private void SetContactOptionsMenuItems()
        {
            //delete all Support options Sub menu items
            for (int i = 0; i < xExtraOperationsMainMenuItem.Items.Count; i++)
            {
                if ((string)((MenuItem)xExtraOperationsMainMenuItem.Items[i]).Tag == "Contact")
                {
                    xExtraOperationsMainMenuItem.Items.RemoveAt(i);
                    i--;
                }
            }

            if (xContactOptionsMenuItem.Tag != null)
            {
                //Insert
                int insertIndex = xExtraOperationsMainMenuItem.Items.IndexOf(xContactOptionsMenuItem) + 1;

                AddSubMenuItem(xExtraOperationsMainMenuItem, "Contact Ginger Core Team", "Contact", xCoreTeamMenuItem_Click, insertIndex, "GingerCoreTeam@int.amdocs.com", iconType: eImageType.Email);
            }
        }

        SolutionPage mSolutionPage;
        private void EditSolutionDetailsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (mSolutionPage == null)
            {
                mSolutionPage = new SolutionPage();
            }

            mSolutionPage.ShowAsWindow();
        }

        private void xCheckForUpgradeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = @"https://ginger.amdocs.com/#downloads", UseShellExecute = true });
        }

        private void xLoadPublicSiteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = @"https://ginger.amdocs.com/", UseShellExecute = true });
        }

        private void xLoadPublicSupportSiteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = @"https://github.com/Ginger-Automation/Ginger/issues", UseShellExecute = true });
        }

        private void xLoadOfflineHelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GingerHelpProvider.ShowOfflineHelpLibrary(silent: false);
        }

        private void xLoadOnlineHelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GingerHelpProvider.ShowOnlineHelpLibrary();
        }

        internal void LoadingInfo(string text)
        {
            ShowStatus(eStatusMsgType.PROCESS, text);
            GingerCore.General.DoEvents();
        }

        void HideSplash()
        {
            // Hide the splash after one second
            this.Dispatcher.Invoke(() =>
            {
                if (xSplashGrid.Visibility == Visibility.Collapsed)
                {
                    return;
                }
                Thread.Sleep(1000);
                xSplashGrid.Visibility = Visibility.Collapsed;
            });
        }

        public void AddHelpLayoutToShow(string helpLayoutKey, FrameworkElement focusedControl, string helpText)
        {
            if (WorkSpace.Instance.RunningInExecutionMode)
            {
                return;//not showing help in automatic run mode
            }
            HelpLayoutArgs helpLayoutArgs = new HelpLayoutArgs(helpLayoutKey, focusedControl, helpText);
            if (!WorkSpace.Instance.UserProfile.ShownHelpLayoutsKeys.Contains(helpLayoutArgs.HelpLayoutKey))
            {
                mHelpLayoutList.Add(helpLayoutArgs);
            }
        }

        private void MHelpLayoutList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (mHelpLayoutList.Count > 0) // check if help were loaded and need to  be shown
            {
                ShowHelpLayout();
            }
        }

        private void ShowHelpLayout()
        {
            if (mHelpLayoutList.Count == 0)
            {
                return;
            }

            HelpLayoutArgs helpArgs = mHelpLayoutList[0];

            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    //--general canvas setup                
                    xHelpLayoutCanvas.Width = xMainWindowPnl.ActualWidth;
                    xHelpLayoutCanvas.Height = xMainWindowPnl.ActualHeight;

                    //---get control to focus details
                    FrameworkElement controlToFocus = helpArgs.FocusedControl;
                    double controlToFocusWidth = controlToFocus.ActualWidth;
                    double controlToFocusHeight = controlToFocus.ActualHeight;
                    Point controlToFocusLocation = controlToFocus.TransformToAncestor(App.MainWindow).Transform(new Point(0, 0));

                    //---set background rectangles
                    double gapSize = 0.25;
                    xHelpLayoutRectangleLeft.Width = controlToFocusLocation.X + gapSize;
                    xHelpLayoutRectangleLeft.Height = xMainWindowPnl.ActualHeight;

                    xHelpLayoutRectangleRight.SetValue(Canvas.LeftProperty, controlToFocusLocation.X + controlToFocusWidth - gapSize);
                    var calcWidth = xMainWindowPnl.ActualWidth - (controlToFocusLocation.X + controlToFocusWidth);
                    xHelpLayoutRectangleRight.Width = (calcWidth > 0) ? calcWidth : xMainWindowPnl.ActualWidth;
                    xHelpLayoutRectangleRight.Height = xMainWindowPnl.ActualHeight;

                    xHelpLayoutRectangleTop.SetValue(Canvas.LeftProperty, controlToFocusLocation.X);
                    xHelpLayoutRectangleTop.Width = controlToFocusWidth;
                    xHelpLayoutRectangleTop.Height = controlToFocusLocation.Y;

                    xHelpLayoutRectangleBottom.SetValue(Canvas.LeftProperty, controlToFocusLocation.X);
                    xHelpLayoutRectangleBottom.SetValue(Canvas.TopProperty, controlToFocusLocation.Y + controlToFocusHeight);
                    xHelpLayoutRectangleBottom.Width = controlToFocusWidth;
                    xHelpLayoutRectangleBottom.Height = xMainWindowPnl.ActualHeight - (controlToFocusLocation.Y + controlToFocusHeight);

                    //-- set text and it location 
                    xHelpLayoutTextBlock.Text = helpArgs.HelpText;
                    double textNeededWidth = 450;
                    double textNeededHeight = 250;
                    double arrowNeededLength = 100;
                    double arrowDistanceFromTarget = 10;
                    Point helpTextLocation = new Point();
                    Point arrowSourceLocation = new Point();
                    Point arrowTargetLocation = new Point();
                    //focused item top left corner
                    if (controlToFocusLocation.X >= textNeededWidth && controlToFocusLocation.Y >= textNeededHeight)
                    {
                        helpTextLocation.X = controlToFocusLocation.X - textNeededWidth - arrowNeededLength;
                        helpTextLocation.Y = controlToFocusLocation.Y - arrowNeededLength;
                        arrowSourceLocation = new Point(helpTextLocation.X + textNeededWidth, helpTextLocation.Y + 50);
                        arrowTargetLocation = new Point(controlToFocusLocation.X, controlToFocusLocation.Y - arrowDistanceFromTarget);
                    }
                    //focused item bottom left corner
                    else if (controlToFocusLocation.X >= textNeededWidth && (xMainWindowPnl.ActualHeight - (controlToFocusLocation.Y + controlToFocusHeight)) >= textNeededHeight)
                    {
                        helpTextLocation.X = controlToFocusLocation.X - textNeededWidth;
                        helpTextLocation.Y = controlToFocusLocation.Y + controlToFocusHeight + arrowNeededLength;
                        arrowSourceLocation = new Point(helpTextLocation.X + textNeededWidth / 2, helpTextLocation.Y);
                        arrowTargetLocation = new Point(controlToFocusLocation.X, controlToFocusLocation.Y + controlToFocusHeight + arrowDistanceFromTarget);
                    }
                    //focused item top right corner
                    else if ((xMainWindowPnl.ActualWidth - (controlToFocusLocation.X + controlToFocusWidth)) >= textNeededWidth && controlToFocusLocation.Y >= textNeededHeight)
                    {
                        helpTextLocation.X = controlToFocusLocation.X + controlToFocusWidth + textNeededWidth;
                        helpTextLocation.Y = controlToFocusLocation.Y - arrowNeededLength;
                        arrowSourceLocation = new Point(helpTextLocation.X - 20, helpTextLocation.Y + 20);
                        arrowTargetLocation = new Point(controlToFocusLocation.X + controlToFocusWidth, controlToFocusLocation.Y - arrowDistanceFromTarget);
                    }
                    //focused item bottom right corner
                    else if ((xMainWindowPnl.ActualWidth - (controlToFocusLocation.X + controlToFocusWidth)) >= textNeededWidth && (xMainWindowPnl.ActualHeight - (controlToFocusLocation.Y + controlToFocusHeight)) >= textNeededHeight)
                    {
                        helpTextLocation.X = controlToFocusLocation.X + controlToFocusWidth + textNeededWidth;
                        helpTextLocation.Y = controlToFocusLocation.Y + controlToFocusHeight + arrowNeededLength;
                        arrowSourceLocation = new Point(helpTextLocation.X, helpTextLocation.Y);
                        arrowTargetLocation = new Point(controlToFocusLocation.X + controlToFocusWidth, controlToFocusLocation.Y + controlToFocusHeight + arrowDistanceFromTarget);
                    }
                    //middle of screen
                    else
                    {
                        helpTextLocation.X = xMainWindowPnl.ActualWidth / 2;
                        helpTextLocation.Y = xMainWindowPnl.ActualHeight / 2;
                    }
                    xHelpLayoutTextBlock.SetValue(Canvas.LeftProperty, helpTextLocation.X);
                    xHelpLayoutTextBlock.SetValue(Canvas.TopProperty, helpTextLocation.Y);

                    //-- draw Arrow
                    while (xHelpLayoutCanvas.Children.Count > 5)//removing previous dynamic arrows
                    {
                        xHelpLayoutCanvas.Children.RemoveAt(xHelpLayoutCanvas.Children.Count - 1);
                    }
                    xHelpLayoutCanvas.Children.Add(GeneralWindows.HelpLayout.DrawArrow.DrawLinkArrow(arrowSourceLocation, arrowTargetLocation));

                    xHelpLayoutCanvas.Visibility = Visibility.Visible;
                });

                if (!WorkSpace.Instance.UserProfile.ShownHelpLayoutsKeys.Contains(helpArgs.HelpLayoutKey))
                {
                    WorkSpace.Instance.UserProfile.ShownHelpLayoutsKeys.Add(helpArgs.HelpLayoutKey);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to show user Help Layout, help key ='{0}'", helpArgs.HelpLayoutKey), ex);
                HideHelpLayout();
            }
        }

        private void HideHelpLayout()
        {
            this.Dispatcher.Invoke(() =>
            {
                xHelpLayoutCanvas.Visibility = Visibility.Collapsed;
                mHelpLayoutList.RemoveAt(0);//remove help just shown               
            });
        }

        private void xHelpLayoutCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HideHelpLayout();
        }

        private void xMainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (xHelpLayoutCanvas.Visibility == Visibility.Visible)
            {
                ShowHelpLayout();//need to show again in correct size
            }
        }

        private void UploadSolutionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SourceControlUploadSolutionPage uploadPage = new SourceControl.SourceControlUploadSolutionPage();
            uploadPage.ShowAsWindow(eWindowShowStyle.Dialog);
            SetSolutionDependedUIElements();
        }
        private void ModifiedFilesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (WorkSpace.Instance.SolutionRepository.ModifiedFiles.Count == 0)
                {
                    xModifiedItemsCounter.Visibility = Visibility.Collapsed;
                }
                else
                {
                    xModifiedItemsCounter.Visibility = Visibility.Visible;
                    xModifiedItemsCounterText.Text = WorkSpace.Instance.SolutionRepository.ModifiedFiles.Count.ToString();
                }
            });
        }
        private void xSaveCurrentBtn_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            ucButton saveBtn = sender as ucButton;
            if (WorkSpace.Instance.CurrentSelectedItem != null)
            {
                saveBtn.ToolTip = String.Format("Save {0}", WorkSpace.Instance.CurrentSelectedItem.ItemName);
            }
            else
            {
                saveBtn.ToolTip = "No Item Selected";
            }
        }

        private async void xSaveCurrentBtn_Click(object sender, RoutedEventArgs e)
        {
            await SaveCurrentItemAsync();
        }
        public async Task SaveCurrentItemAsync()
        {
            if (WorkSpace.Instance.CurrentSelectedItem == null || WorkSpace.Instance.CurrentSelectedItem.DirtyStatus != eDirtyStatus.Modified)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Nothing found to Save.");
                return;
            }
            await Task.Run(() => SaveCurrentItem());
        }

        private void SaveCurrentItem()
        {
            try
            {
                if (Reporter.ToUser(eUserMsgKey.SaveBusinessFlowChanges, WorkSpace.Instance.CurrentSelectedItem.ItemName) == eUserMsgSelection.Yes)
                {
                    SaveHandler.Save(WorkSpace.Instance.CurrentSelectedItem);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
        }

        private void xSaveAllBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            xSaveAllBtn.ButtonImageType = eImageType.SaveAllGradient;
        }

        private void xSaveAllBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            xSaveAllBtn.ButtonImageType = eImageType.SaveAll;
        }

        private void xSaveAllBtn_Click(object sender, RoutedEventArgs e)
        {
            if (WorkSpace.Instance.SolutionRepository.ModifiedFiles.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Nothing found to Save.");
                return;
            }
            ModifiedRepositoryFilesPage MRFPage = new ModifiedRepositoryFilesPage();
            MRFPage.ShowAsWindow();
        }

        private void xGlobalSolutionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (xGlobalSolutionMenuItem.Tag == null)
            {
                xGlobalSolutionMenuItem.Tag = "Expanded";
            }
            else
            {
                xGlobalSolutionMenuItem.Tag = null;
            }

            SetGlobalSolutionMenuItems();
        }

        private void SetGlobalSolutionMenuItems()
        {
            //delete all Global Solution Sub menu items
            for (int i = 0; i < xExtraSolutionOperationsMainMenuItem.Items.Count; i++)
            {
                if ((string)((MenuItem)xExtraSolutionOperationsMainMenuItem.Items[i]).Tag == "Global Solution")
                {
                    xExtraSolutionOperationsMainMenuItem.Items.RemoveAt(i);
                    i--;
                }
            }

            if (xGlobalSolutionMenuItem.Tag != null)
            {
                //Insert
                int insertIndex = xExtraSolutionOperationsMainMenuItem.Items.IndexOf(xGlobalSolutionMenuItem) + 1;

                AddSubMenuItem(xExtraSolutionOperationsMainMenuItem, "Import Global Cross Solution Items", "Global Solution", btnGlobalSolutionImport_Click, insertIndex, iconType: eImageType.GetLatest);
            }
        }
        private void xSolutionALMMenu_Click(object sender, RoutedEventArgs e)
        {
            if (xSolutionALMMenu.Tag == null)
            {
                xSolutionALMMenu.Tag = "Expanded";
            }
            else
            {
                xSolutionALMMenu.Tag = null;
            }

            SetALMMenuItems();
        }
        private void SetALMMenuItems()
        {
            //delete all Global Solution Sub menu items
            for (int i = 0; i < xExtraSolutionOperationsMainMenuItem.Items.Count; i++)
            {
                if ((string)((MenuItem)xExtraSolutionOperationsMainMenuItem.Items[i]).Tag == "ALM")
                {
                    xExtraSolutionOperationsMainMenuItem.Items.RemoveAt(i);
                    i--;
                }
            }

            if (xSolutionALMMenu.Tag != null)
            {
                //Insert
                int insertIndex = xExtraSolutionOperationsMainMenuItem.Items.IndexOf(xSolutionALMMenu) + 1;

                AddSubMenuItem(xExtraSolutionOperationsMainMenuItem, "ALM Connection Settings", "ALM", ALMConfigButton_Click, insertIndex++, iconType: eImageType.Parameter);
                AddSubMenuItem(xExtraSolutionOperationsMainMenuItem, "ALM Items Fields Configuration", "ALM", ALMFieldsConfiguration_Click, insertIndex++, iconType: eImageType.ListGroup);
                AddSubMenuItem(xExtraSolutionOperationsMainMenuItem, "ALM Defect's Profiles", "ALM", ALMDefectsProfiles_Click, insertIndex, iconType: eImageType.Bug);
            }
        }

        private void xSaveCurrentBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            xSaveCurrentBtn.ButtonImageType = eImageType.SaveGradient;
        }
        private void xSaveCurrentBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            xSaveCurrentBtn.ButtonImageType = eImageType.SaveLightGrey;
        }

        private void xAdminModeIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!General.IsAdmin())
            {
                mLaunchInAdminMode = true;
                mRestartApplication = true;
                App.MainWindow.Close();
            }
        }

        private void ChatbotIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (xChatbotWindow.Visibility == Visibility.Collapsed)
            {
                xChatbotWindow.Visibility = Visibility.Visible;
                xChatbotIcon.Visibility = Visibility.Collapsed;
            }

        }

        private void xChatbotIcon_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (xChatbotWindow.Visibility == Visibility.Collapsed)
            {
                xChatbotWindow.Visibility = Visibility.Visible;
                xChatbotIcon.Visibility = Visibility.Collapsed;
            }
        }

        private void xChatbotIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (xChatbotWindow.Visibility == Visibility.Collapsed)
            {
                xChatbotWindow.Visibility = Visibility.Visible;
                xChatbotIcon.Visibility = Visibility.Collapsed;
            }
        }

        private void xChatbotIcon_Click(object sender, RoutedEventArgs e)
        {
            if (xChatbotWindow.Visibility == Visibility.Collapsed)
            {
                xChatbotWindow.Visibility = Visibility.Visible;
                xChatbotIcon.Visibility = Visibility.Collapsed;
            }
        }
    }
}