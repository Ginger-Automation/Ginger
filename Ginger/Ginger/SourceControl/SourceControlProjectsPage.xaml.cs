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
using Amdocs.Ginger.UserControls;
using Ginger.UserControls;
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.SourceControl
{
    /// <summary>
    /// Interaction logic for TeamGetLatestPage.xaml
    /// </summary>
    public partial class SourceControlProjectsPage : Page
    {
        ObservableList<SolutionInfo> SourceControlSolutions = [];

        SolutionInfo solutionInfo = null;

        ImageMakerControl loaderElement = new();
        TextBlock progressText = new();
        ProgressBar progressBar = new();
        ProgressNotifier progressNotifier = new();
        GenericWindow genWin = null;
        Button downloadProjBtn = null;
        public SourceControlBase mSourceControl = null;
        static bool IsImportSolution = true;
        SourceControlBase.eSourceControlType SourceControlType;
        public SourceControlProjectsPage(bool IsCalledFromImportPage = false)
        {
            InitializeComponent();
            SolutionsGrid.SetTitleLightStyle = true;

            ConnectionDetailsExpender.IsExpanded = true;


            SourceControlLocalFolderLable.Visibility = Visibility.Collapsed;
            SourceControlLocalFolderTextBox.Visibility = Visibility.Collapsed;
            BrowseButton.Visibility = Visibility.Collapsed;
            SolutionsGrid.Visibility = Visibility.Collapsed;

            IsImportSolution = IsCalledFromImportPage;

            progressNotifier.LabelHandler += HandleProgressUpdated;
            progressNotifier.StatusUpdateHandler += HandleProgressBarUpdated;

            SourceControlClassComboBox.ComboBox.SelectionChanged += SourceControlClassComboBox_SelectionChanged;
            SourceControlInit();
        }
        private void SourceControlInit()
        {
            var recentDownloadedSolutionGuid = WorkSpace.Instance.UserProfile.RecentDownloadedSolutionGuid;

            if (recentDownloadedSolutionGuid != Guid.Empty)
            {
                SourceControlInit(recentDownloadedSolutionGuid);
                BindComponent();
            }
            else
            {
                mSourceControl = new GITSourceControl();
                BindComponent();
            }

        }
        private void BindComponent()
        {
            SourceControlType = mSourceControl.GetSourceControlType;
            GingerCore.General.FillComboFromEnumType(SourceControlClassComboBox.ComboBox, typeof(SourceControlBase.eSourceControlType));
            if (mSourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.GIT)
            {
                SourceControlClassComboBox.ComboBox.SelectedIndex = 1;
            }
            else
            {
                SourceControlClassComboBox.ComboBox.SelectedIndex = 2;
            }

            if (IsImportSolution)
            {
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlLocalFolderTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.SourceControlLocalFolderForGlobalSolution));
                mSourceControl.SourceControlLocalFolderForGlobalSolution = @"C:\GingerSourceControl\GlobalCrossSolutions";
            }
            else
            {
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlLocalFolderTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.LocalFolder));
            }

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlURLTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.URL));


            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xBranchesCombo, ComboBox.TextProperty, mSourceControl, nameof(SourceControlBase.Branch));
            xBranchesCombo.ItemsSource = new ObservableList<string>();
            ObservableList<string> itemsource = (ObservableList<string>)xBranchesCombo.ItemsSource;
            itemsource.ClearAll();
            itemsource.Add(mSourceControl.Branch);
            xBranchesCombo.SelectedIndex = 0;


            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlUserTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.Username));


            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlPassTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.Password));
            SourceControlPassTextBox.Password = mSourceControl.Password;


            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ConfigureProxyCheckBox, CheckBox.IsCheckedProperty, mSourceControl, nameof(SourceControlBase.IsProxyConfigured));


            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ProxyAddressTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.ProxyAddress));


            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ProxyPortTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.ProxyPort));


            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtConnectionTimeout, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.Timeout));

            SolutionsGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));

        }
        void setPassword()
        {
            mSourceControl.Password = SourceControlPassTextBox.Password;
        }
        private void SourceControlClassComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SourceControlClassComboBox.ComboBox.SelectedValue==null||(SourceControlBase.eSourceControlType)SourceControlClassComboBox.ComboBox.SelectedValue == SourceControlBase.eSourceControlType.None)
            {
                if (mSourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.GIT)
                {
                    SourceControlClassComboBox.ComboBox.SelectedIndex = 1;
                }
                else
                {
                    SourceControlClassComboBox.ComboBox.SelectedIndex = 2;
                }
                return;
            }
            var selectedSourceControlType = (SourceControlBase.eSourceControlType)SourceControlClassComboBox.ComboBox.SelectedValue;
            if (selectedSourceControlType == SourceControlBase.eSourceControlType.None || selectedSourceControlType == SourceControlType)
            {
                return;
            }
            setVisibility(selectedSourceControlType);
            if (selectedSourceControlType == SourceControlBase.eSourceControlType.SVN)
            {
                mSourceControl = new SVNSourceControl();
            }
            else
            {
                mSourceControl = new GITSourceControl();
            }
            BindComponent();

        }
        public void SourceControlInit(Guid solutionGuid)
        {
            var GingerSolutionSourceControl = WorkSpace.Instance.UserProfile.GetSolutionSourceControlInfo(solutionGuid);
            if (GingerSolutionSourceControl.SourceControlInfo.Type == SourceControlBase.eSourceControlType.GIT)
            {
                mSourceControl = new GITSourceControl();
            }
            else if (GingerSolutionSourceControl.SourceControlInfo.Type == SourceControlBase.eSourceControlType.SVN)
            {
                mSourceControl = new SVNSourceControl();
            }
            else if (GingerSolutionSourceControl.SourceControlInfo.Type == SourceControlBase.eSourceControlType.None)
            {
                mSourceControl = new GITSourceControl();
                GingerSolutionSourceControl.SourceControlInfo.Type = SourceControlBase.eSourceControlType.GIT;
            }

            setSourceControlFromUserProfile(solutionGuid);
        }

        void setSourceControlFromUserProfile(Guid solutionGuid)
        {

            if (mSourceControl != null)
            {
                WorkSpace.Instance.UserProfile.UserProfileOperations.LoadPasswords(WorkSpace.Instance.UserProfile);
                WorkSpace.Instance.UserProfile.GetSourceControlPropertyFromUserProfile(mSourceControl, solutionGuid);
                mSourceControl.IsImportSolution = IsImportSolution;
            }
        }


        private void setVisibility(SourceControlBase.eSourceControlType sourceControlType)
        {

            if (sourceControlType == SourceControlBase.eSourceControlType.GIT)
            {
                xTimeoutPanel.Visibility = Visibility.Hidden;
                xFetchBranchesButton.Visibility = Visibility.Visible;
                xSelectBranchLabel.Visibility = Visibility.Visible;
                xBranchesCombo.Visibility = Visibility.Visible;
            }
            if (sourceControlType == SourceControlBase.eSourceControlType.SVN)
            {
                xTimeoutPanel.Visibility = Visibility.Visible;
                xFetchBranchesButton.Visibility = Visibility.Hidden;
                xSelectBranchLabel.Visibility = Visibility.Hidden;
                xBranchesCombo.Visibility = Visibility.Hidden;
            }

        }




        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
            SourceControlIntegration.BusyInProcessWhileDownloading = false;
            GetProjetList();
        }

        private async Task GetProjetList()
        {
            try
            {
                SetLocalFolderPath();
                if (!IsImportSolution)
                {
                    downloadProjBtn.IsEnabled = false;
                }
                if (SolutionsGrid.DataSourceList != null)
                {
                    SolutionsGrid.DataSourceList.Clear();
                }
                loaderElement.Visibility = Visibility.Visible;
                if (SourceControlIntegration.BusyInProcessWhileDownloading)
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Please wait for current process to end.");
                    return;
                }
                SourceControlIntegration.BusyInProcessWhileDownloading = true;

                SourceControlIntegration.CreateConfigFile(mSourceControl);
                if (SourceControlUI.TestConnection(mSourceControl, true) == false)
                {
                    SourceControlIntegration.BusyInProcessWhileDownloading = false;
                    Mouse.OverrideCursor = null;
                    return;
                }

                await Task.Run(() => SourceControlSolutions = SourceControlIntegration.GetProjectsList(mSourceControl)
                );

                SolutionsGrid.DataSourceList = SourceControlSolutions;
                SolutionsGrid.grdMain.SelectedCellsChanged += GrdMain_SelectedCellsChanged;
                SolutionsGrid.grdMain.SelectedItem = SolutionsGrid.DataSourceList[0];
                SetGridView();
                Mouse.OverrideCursor = null;

                ConnectionConfigurationsExpender.IsExpanded = false;
                ConnectionDetailsExpender.IsExpanded = false;

                //Changed to save password only after successful Connect and search repositories
                WorkSpace.Instance.UserProfile.UserProfileOperations.SaveUserProfile();
            }
            catch (Exception e)
            {
                Mouse.OverrideCursor = null;
                Reporter.ToUser(eUserMsgKey.FailedToGetProjectsListFromSVN, e.Message);
            }
            finally
            {
                loaderElement.Visibility = Visibility.Collapsed;
                SourceControlIntegration.BusyInProcessWhileDownloading = false;

                if (SolutionsGrid.DataSourceList != null)
                {
                    if (SolutionsGrid.DataSourceList.Count > 0)
                    {
                        if (!IsImportSolution)
                        {
                            DownloadButtonRow.Height = new GridLength(0);
                            downloadProjBtn.IsEnabled = true;
                        }
                        else
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                DownloadButtonRow.Height = new GridLength(40);
                                DownloadButton.Visibility = Visibility.Visible;
                            });
                        }
                    }
                }

            }
        }

        private void GrdMain_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            solutionInfo = (SolutionInfo)SolutionsGrid.grdMain.SelectedItem;
        }

        private void SetGridView()
        {
            //Set the Data Grid columns
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(SolutionInfo.SourceControlLocation), Header = "Source Control Solution Name", WidthWeight = 30, ReadOnly = true },
                new GridColView() { Field = nameof(SolutionInfo.LocalFolder), Header = "Solution Local Folder Path", WidthWeight = 50, ReadOnly = true },
                new GridColView() { Field = nameof(SolutionInfo.ExistInLocaly), Header = "Exist Locally", StyleType = GridColView.eGridColStyleType.Text, WidthWeight = 20, MaxWidth = 800, ReadOnly = true },
            ]
            };

            SolutionsGrid.SetAllColumnsDefaultView(view);
            SolutionsGrid.InitViewItems();
        }

        private async void GetProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SourceControlIntegration.BusyInProcessWhileDownloading)
                {
                    StopDownload();
                }
                else
                {
                    await DownloadSolution().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error Occurred :", ex);
            }
        }

        private void OpenSolution(string Path, string ProjectURI)
        {
            string SoFileName = CheckForSolutionFileName(Path);
            if (System.IO.File.Exists(SoFileName))
            {
                WorkSpace.Instance.OpenSolution(System.IO.Path.GetDirectoryName(SoFileName));
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.SolutionFileNotFound, SoFileName);
            }
        }

        private string CheckForSolutionFileName(string path)
        {
            if (System.IO.File.Exists(path + @"\Ginger.Solution.xml"))
            {
                return path + @"\Ginger.Solution.xml";
            }

            string[] childrens = System.IO.Directory.GetDirectories(path);
            foreach (string child in childrens)
            {
                string SolutionPath = CheckForSolutionFileName(child);
                if (!string.IsNullOrEmpty(SolutionPath))
                {
                    return SolutionPath;
                }
            }
            return string.Empty;
        }

        public string ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            downloadProjBtn = new Button
            {
                Content = "Download Selected Solution"
            };
            downloadProjBtn.Click += new RoutedEventHandler(GetProject_Click);

            progressBar.Name = "progressBar";
            progressBar.Height = 16;
            progressBar.VerticalAlignment = VerticalAlignment.Top;
            progressBar.HorizontalAlignment = HorizontalAlignment.Stretch; // Set to Stretch to use available space
            progressBar.Background = System.Windows.Media.Brushes.LightYellow;
            progressBar.Foreground = System.Windows.Media.Brushes.ForestGreen;
            progressBar.Visibility = Visibility.Collapsed;
            progressBar.Margin = new Thickness(5, 5, 5, 0);

            progressText.Name = "progressText";
            progressText.HorizontalAlignment = HorizontalAlignment.Center;
            progressText.Margin = new Thickness(0, 1, 0, 0); // Adjusted margin to add space between progressBar and progressText
            progressText.VerticalAlignment = VerticalAlignment.Top; // Changed to Top to align below progressBar
            progressText.Visibility = Visibility.Collapsed;
            progressText.FontSize = 12;

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, "Download Source Control Solution", this, [downloadProjBtn], true, "Close", null, false, null, progressBar, progressText);

            if (solutionInfo != null)
            {
                if (solutionInfo.ExistInLocaly)
                {
                    return solutionInfo.LocalFolder;
                }
            }
            return "";
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Local Folder",
                RootFolder = Environment.SpecialFolder.MyComputer,
                ShowNewFolderButton = true
            };
            SourceControlIntegration.BusyInProcessWhileDownloading = false;
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                SourceControlLocalFolderTextBox.Text = dlg.SelectedPath;
                SetLocalFolderPath();
                GetProjetList();
            }
        }

        private void SetLocalFolderPath()
        {
            if (Directory.Exists(SourceControlLocalFolderTextBox.Text))
            {
                mSourceControl.LocalFolder = SourceControlLocalFolderTextBox.Text;
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, "Local folder path does not exists." + SourceControlLocalFolderTextBox.Text);
            }
        }

        private void ConnectionDetailsExpended(object sender, RoutedEventArgs e)
        {
            ExpenderDetailsRow.Height = new GridLength(230);
        }

        private void ConnectionDetailsCollapsed(object sender, RoutedEventArgs e)
        {
            ExpenderDetailsRow.Height = new GridLength(50);
        }


        private async void TestConnectionAndSearchRepositories_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(SourceControlLocalFolderTextBox.Text))
                {
                    SourceControlLocalFolderTextBox.Text = @"C:\GingerSourceControl\Solutions\";
                }

                if (!mSourceControl.IsRepositoryPublic() && (string.IsNullOrEmpty(mSourceControl.Username) || string.IsNullOrEmpty(mSourceControl.Password)) || string.IsNullOrEmpty(mSourceControl.URL))
                {
                    Reporter.ToUser(eUserMsgKey.SourceControlConnMissingConnInputs);
                    return;
                }
                xConnectButton.Visibility = Visibility.Visible;
                loaderElement.Visibility = Visibility.Visible;

                xConnectButton.IsEnabled = false;
                SourceControlLocalFolderLable.Visibility = Visibility.Visible;
                SourceControlLocalFolderTextBox.Visibility = Visibility.Visible;
                BrowseButton.Visibility = Visibility.Visible;
                SolutionsGrid.Visibility = Visibility.Visible;
                SourceControlIntegration.BusyInProcessWhileDownloading = false;

                await GetProjetList().ConfigureAwait(false);

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error Occurred :", ex);
            }
            finally
            {
                this.Dispatcher.Invoke(() =>
               {
                   xConnectButton.IsEnabled = true;
               }
                );
            }
        }

        private void SourceControlURLTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SourceControlIntegration.Init(mSourceControl);
        }

        private void SourceControlUserTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SourceControlIntegration.Init(mSourceControl);
        }

        private void ConnectionConfigurationsExpended(object sender, RoutedEventArgs e)
        {
            ExpenderConfigurationRow.Height = new GridLength(200);
        }

        private void ConnectionConfigurationsCollapsed(object sender, RoutedEventArgs e)
        {
            ExpenderConfigurationRow.Height = new GridLength(50);
        }

        private void ConfigureProxyCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            ProxyAddressTextBox.IsEnabled = true;
            ProxyPortTextBox.IsEnabled = true;
        }

        private void ConfigureProxyCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            ProxyAddressTextBox.IsEnabled = false;
            ProxyPortTextBox.IsEnabled = false;
        }
        private async void FetchBranches_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                loaderElement.Visibility = Visibility.Visible;
                setPassword();
                mSourceControl.IsPublicRepo = false;
                ObservableList<string> itemsource = (ObservableList<string>)xBranchesCombo.ItemsSource;
                itemsource.ClearAll();
                await Task.Run(() =>
                {
                    try
                    {
                        var branches = SourceControlIntegration.GetBranches(mSourceControl);
                        if (branches == null || !branches.Any())
                        {
                            throw new Exception("No branches found.");
                        }
                        Dispatcher.Invoke(() =>
                        {
                            foreach (string branch in branches)
                            {
                                itemsource.Add(branch);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, ex.Message);
                    }
                });
                if (xBranchesCombo.Items.Count > 0)
                {
                    xBranchesCombo.SelectedIndex = 0;
                    if (String.IsNullOrEmpty(mSourceControl.Username) || String.IsNullOrEmpty(mSourceControl.Password))
                    {
                        mSourceControl.IsPublicRepo = true;
                    }
                }
                loaderElement.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message);
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            await DownloadSolution().ConfigureAwait(false);
        }

        private async Task DownloadSolution()
        {
            try
            {
                loaderElement.Visibility = Visibility.Visible;
                progressText.Visibility = Visibility.Visible;
                progressBar.Visibility = Visibility.Visible;
                downloadProjBtn.Content = "Cancel Downloading";
                if (SourceControlIntegration.BusyInProcessWhileDownloading)
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Please wait for current process to end.");
                    return;
                }
                SourceControlIntegration.BusyInProcessWhileDownloading = true;
                if (string.IsNullOrEmpty(mSourceControl.LocalFolder))
                {
                    Reporter.ToUser(eUserMsgKey.SourceControlConnMissingLocalFolderInput);
                }

                solutionInfo = (SolutionInfo)SolutionsGrid.grdMain.SelectedItem;
                if (solutionInfo == null)
                {
                    Reporter.ToUser(eUserMsgKey.AskToSelectSolution);
                    return;
                }

                string ProjectURI = string.Empty;
                if (mSourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.SVN)
                {
                    ProjectURI = mSourceControl.URL.StartsWith("SVN", StringComparison.CurrentCultureIgnoreCase) ?
                    solutionInfo.SourceControlLocation : mSourceControl.URL + solutionInfo.SourceControlLocation;
                }
                else
                {
                    ProjectURI = mSourceControl.URL;
                }
                _cancellationTokenSource = new CancellationTokenSource();
                bool getProjectResult = await Task.Run(() =>
                {
                    try
                    {
                        return SourceControlIntegration.GetProject(mSourceControl, solutionInfo.LocalFolder, ProjectURI, progressNotifier, _cancellationTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Error occurred while getting the project:", ex);
                        return false;
                    }
                });

                SourceControlIntegration.BusyInProcessWhileDownloading = false;

                if (getProjectResult)
                {
                    solutionInfo.ExistInLocaly = true;
                }
                Mouse.OverrideCursor = null;

                if (!IsImportSolution && getProjectResult && (Reporter.ToUser(eUserMsgKey.DownloadedSolutionFromSourceControl, solutionInfo.LocalFolder) == Amdocs.Ginger.Common.eUserMsgSelection.Yes))
                {
                    OpenSolution(solutionInfo.LocalFolder, ProjectURI);

                    if (WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.SourceControl != null)
                    {
                        WorkSpace.Instance.UserProfile.RecentDownloadedSolutionGuid = WorkSpace.Instance.Solution.Guid;
                        WorkSpace.Instance.UserProfile.SetSourceControlPropertyOnUserProfile(mSourceControl, WorkSpace.Instance.Solution.Guid);
                        if (WorkSpace.Instance.Solution.SourceControl.Username != mSourceControl.Username || WorkSpace.Instance.Solution.SourceControl.Password != mSourceControl.Password)
                        {
                            WorkSpace.Instance.Solution.SourceControl.Username = mSourceControl.Username;
                            WorkSpace.Instance.Solution.SourceControl.Password = mSourceControl.Password;
                            WorkSpace.Instance.Solution.SourceControl.Disconnect();
                        }
                    }
                    genWin.Close();
                }
            }
            finally
            {
                SourceControlIntegration.BusyInProcessWhileDownloading = false;
                downloadProjBtn.Content = "Download Selected Solution";
                progressText.Text = "";
                progressBar.Maximum = 100;
                progressBar.Value = 0;
                loaderElement.Visibility = Visibility.Collapsed;
                progressText.Visibility = Visibility.Collapsed;
                progressBar.Visibility = Visibility.Collapsed;
                _cancellationTokenSource.Dispose();
            }
        }
        /// <summary>
        /// Stops the ongoing download process by canceling the associated cancellation token.
        /// </summary>
        private void StopDownload()
        {
            _cancellationTokenSource?.Cancel();
        }
        private CancellationTokenSource _cancellationTokenSource;
        /// <summary>
        /// Handles the progress update event by updating the progress text on the UI thread.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="message">The progress message.</param>
        private void HandleProgressUpdated(object sender, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            try
            {
                Dispatcher.Invoke(() => progressText.Text = message);
            }
            catch (TaskCanceledException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, e.Message);
            }
        }

        /// <summary>
        /// Handles the progress bar update event by updating the progress bar value on the UI thread.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="progress">A tuple containing the completed steps and total steps.</param>
        private void HandleProgressBarUpdated(object sender, (string ProgressType, int CompletedSteps, int TotalSteps) progress)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    progressBar.Maximum = progress.TotalSteps;
                    progressBar.Value = progress.CompletedSteps;
                });
            }
            catch (TaskCanceledException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, e.Message);
            }
        }
    }
}
