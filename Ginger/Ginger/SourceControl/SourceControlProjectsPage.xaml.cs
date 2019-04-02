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
using Ginger.UserControls;
using GingerCore;
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using amdocs.ginger.GingerCoreNET;
namespace Ginger.SourceControl
{
    /// <summary>
    /// Interaction logic for TeamGetLatestPage.xaml
    /// </summary>
    public partial class SourceControlProjectsPage : Page
    {
        ObservableList<SolutionInfo> SourceControlSolutions = new ObservableList<SolutionInfo>();

        GenericWindow genWin = null;
        Button downloadProjBtn = null;

        public static SourceControlBase mSourceControl;

        public SourceControlProjectsPage()
        {
            InitializeComponent();
            SolutionsGrid.SetTitleLightStyle = true;

            ConnectionDetailsExpender.IsExpanded = true;

            SourceControlInit();

            SourceControlLocalFolderLable.Visibility = Visibility.Collapsed;
            SourceControlLocalFolderTextBox.Visibility = Visibility.Collapsed;
            BrowseButton.Visibility = Visibility.Collapsed;
            SolutionsGrid.Visibility = Visibility.Collapsed;

            Init();
        }

        private void Init()
        {
            //ConnecitonDetailsPage Binding
            if ( WorkSpace.Instance.UserProfile.SourceControlURL == null)
            {

                 WorkSpace.Instance.UserProfile.SourceControlURL = "";
            }

            SourceControlClassComboBox.Init( WorkSpace.Instance.UserProfile, nameof(UserProfile.SourceControlType), typeof(SourceControlBase.eSourceControlType), false, SourceControlClassComboBox_SelectionChanged);
            SourceControlClassComboBox.ComboBox.Items.RemoveAt(0);//removing the NONE option from user selection

            //ProjectPage Binding.
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlLocalFolderTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.SourceControlLocalFolder));
            if (String.IsNullOrEmpty( WorkSpace.Instance.UserProfile.SourceControlLocalFolder))
            {
                // Default local solutions folder
                mSourceControl.SourceControlLocalFolder = @"C:\GingerSourceControl\Solutions\";
            }

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ConfigureProxyCheckBox, CheckBox.IsCheckedProperty, mSourceControl, nameof(SourceControlBase.SourceControlConfigureProxy));

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ProxyAddressTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.SourceControlProxyAddress));
            if (String.IsNullOrEmpty( WorkSpace.Instance.UserProfile.SolutionSourceControlProxyAddress))
                mSourceControl.SourceControlProxyAddress = @"http://genproxy.amdocs.com";

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ProxyPortTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.SourceControlProxyPort));
            if (String.IsNullOrEmpty( WorkSpace.Instance.UserProfile.SolutionSourceControlProxyPort))
                mSourceControl.SourceControlProxyPort = @"8080";

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtConnectionTimeout, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.SourceControlTimeout));

            SolutionsGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));
        }
        private void UpdateTimeoutVisibility()
        {
            if ( WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.GIT)
            {
                xTimeoutPanel.Visibility = Visibility.Hidden;
            }
            if ( WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN)
            {
                xTimeoutPanel.Visibility = Visibility.Visible;
            }
        }
        
        private void Bind()
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlURLTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.SourceControlURL));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlUserTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.SourceControlUser));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlPassTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.SourceControlPass));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtConnectionTimeout, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.SourceControlTimeout));
            UpdateTimeoutVisibility();
            SourceControlPassTextBox.Password = mSourceControl.SourceControlPass;
            SourceControlPassTextBox.PasswordChanged += SourceControlPassTextBox_PasswordChanged;
        }

        private void SourceControlClassComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SourceControlInit();
            Bind();
        }

        private void SourceControlPassTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            mSourceControl.SourceControlPass = ((PasswordBox)sender).Password;
             WorkSpace.Instance.UserProfile.SaveUserProfile();//todo: check if needed
            SourceControlIntegration.Init(mSourceControl);
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
                downloadProjBtn.IsEnabled = false;
                if (SolutionsGrid.DataSourceList != null)
                {
                    SolutionsGrid.DataSourceList.Clear();
                }
                xProcessingIcon.Visibility = Visibility.Visible;
                if (SourceControlIntegration.BusyInProcessWhileDownloading)
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Please wait for current process to end.");
                    return;
                }
                SourceControlIntegration.BusyInProcessWhileDownloading = true;

                SourceControlIntegration.CreateConfigFile(mSourceControl);
                if (SourceControlIntegration.TestConnection(mSourceControl, SourceControlConnDetailsPage.eSourceControlContext.DownloadProjectPage, true) == false)
                {
                    SourceControlIntegration.BusyInProcessWhileDownloading = false;
                    Mouse.OverrideCursor = null;
                    return;
                }

                await Task.Run(() => SourceControlSolutions = SourceControlIntegration.GetProjectsList(mSourceControl)
                );

                SolutionsGrid.DataSourceList = SourceControlSolutions;
                SetGridView();
                Mouse.OverrideCursor = null;

                ConnectionConfigurationsExpender.IsExpanded = false;
                ConnectionDetailsExpender.IsExpanded = false;

            }
            catch (Exception e)
            {
                Mouse.OverrideCursor = null;
                Reporter.ToUser(eUserMsgKey.FailedToGetProjectsListFromSVN, e.Message);
            }
            finally
            {
                xProcessingIcon.Visibility = Visibility.Collapsed;
                SourceControlIntegration.BusyInProcessWhileDownloading = false;

                if (SolutionsGrid.DataSourceList != null)
                {
                    if (SolutionsGrid.DataSourceList.Count > 0)
                    {
                        downloadProjBtn.IsEnabled = true;
                    }
                }

            }
        }

        private void SetGridView()
        {
            //Set the Data Grid columns
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(SolutionInfo.SourceControlLocation), Header = "Source Control Solution Name", WidthWeight = 30, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(SolutionInfo.LocalFolder), Header = "Solution Local Folder Path", WidthWeight = 50, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(SolutionInfo.ExistInLocaly), Header = "Exist Locally", StyleType = GridColView.eGridColStyleType.Text, WidthWeight = 20, MaxWidth = 800, ReadOnly = true });

            SolutionsGrid.SetAllColumnsDefaultView(view);
            SolutionsGrid.InitViewItems();
        }

        private async void GetProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                xProcessingIcon.Visibility = Visibility.Visible;
                if (SourceControlIntegration.BusyInProcessWhileDownloading)
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Please wait for current process to end.");
                    return;
                }
                SourceControlIntegration.BusyInProcessWhileDownloading = true;

                if ( WorkSpace.Instance.UserProfile.SourceControlLocalFolder == string.Empty)
                {
                    Reporter.ToUser(eUserMsgKey.SourceControlConnMissingLocalFolderInput);
                }

                SolutionInfo sol = (SolutionInfo)SolutionsGrid.grdMain.SelectedItem;
                if (sol == null)
                {
                    Reporter.ToUser(eUserMsgKey.AskToSelectSolution);
                    return;
                }

                string ProjectURI = string.Empty;
                if ( WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN)
                {
                    ProjectURI =  WorkSpace.Instance.UserProfile.SourceControlURL.StartsWith("SVN", StringComparison.CurrentCultureIgnoreCase) ?
                    sol.SourceControlLocation :  WorkSpace.Instance.UserProfile.SourceControlURL + sol.SourceControlLocation;
                }
                else
                {
                    ProjectURI =  WorkSpace.Instance.UserProfile.SourceControlURL;
                }

                bool getProjectResult = await Task.Run(() => SourceControlIntegration.GetProject(mSourceControl, sol.LocalFolder, ProjectURI));
                SourceControlIntegration.BusyInProcessWhileDownloading = false;

                GetProjetList();
                Mouse.OverrideCursor = null;

                if (getProjectResult && (Reporter.ToUser(eUserMsgKey.DownloadedSolutionFromSourceControl, sol.LocalFolder) == Amdocs.Ginger.Common.eUserMsgSelection.Yes))
                {
                    OpenSolution(sol.LocalFolder, ProjectURI);

                    if ( WorkSpace.Instance.Solution != null &&  WorkSpace.Instance.Solution.SourceControl != null)
                    {
                        if ( WorkSpace.Instance.Solution.SourceControl.SourceControlUser !=  WorkSpace.Instance.UserProfile.SourceControlUser ||  WorkSpace.Instance.Solution.SourceControl.SourceControlPass !=  WorkSpace.Instance.UserProfile.SourceControlPass)
                        {
                             WorkSpace.Instance.Solution.SourceControl.SourceControlUser =  WorkSpace.Instance.UserProfile.SourceControlUser;
                             WorkSpace.Instance.Solution.SourceControl.SourceControlPass =  WorkSpace.Instance.UserProfile.SourceControlPass;
                             WorkSpace.Instance.Solution.SourceControl.Disconnect();
                        }
                    }
                    genWin.Close();
                }
            }
            finally
            {
                SourceControlIntegration.BusyInProcessWhileDownloading = false;
                xProcessingIcon.Visibility = Visibility.Collapsed;
            }
        }

        private void OpenSolution(string Path, string ProjectURI)
        {
            string SoFileName = CheckForSolutionFileName(Path);
            if (System.IO.File.Exists(SoFileName))
            {
                App.SetSolution(System.IO.Path.GetDirectoryName(SoFileName));
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.SolutionFileNotFound, SoFileName);
            }
        }

        private string CheckForSolutionFileName(string path)
        {
            if (System.IO.File.Exists(path + @"\Ginger.Solution.xml"))
                return path + @"\Ginger.Solution.xml";
            string[] childrens = System.IO.Directory.GetDirectories(path);
            foreach (string child in childrens)
            {
                string SolutionPath = CheckForSolutionFileName(child);
                if (!string.IsNullOrEmpty(SolutionPath))
                    return SolutionPath;
            }
            return string.Empty;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            downloadProjBtn = new Button();
            downloadProjBtn.Content = "Download Selected Solution";
            downloadProjBtn.Click += new RoutedEventHandler(GetProject_Click);

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, "Download Source Control Solution", this, new ObservableList<Button> { downloadProjBtn });

        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "Select Local Folder";
            dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dlg.ShowNewFolderButton = true;
            SourceControlIntegration.BusyInProcessWhileDownloading = false;
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                SourceControlLocalFolderTextBox.Text = dlg.SelectedPath;
                GetProjetList();
            }
        }

        private void ConnectionDetailsExpended(object sender, RoutedEventArgs e)
        {
            ExpenderDetailsRow.Height = new GridLength(200);
        }

        private void ConnectionDetailsCollapsed(object sender, RoutedEventArgs e)
        {
            ExpenderDetailsRow.Height = new GridLength(50);
        }

        public static void SourceControlInit()
        {
            if ( WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.GIT)
                mSourceControl = new GITSourceControl();
            else if ( WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN)
                mSourceControl = new SVNSourceControl();
            else if ( WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.None)
                mSourceControl = new SVNSourceControl();

            if (mSourceControl != null)
            {
                 WorkSpace.Instance.UserProfile.SourceControlType = mSourceControl.GetSourceControlType;
                mSourceControl.SourceControlURL =  WorkSpace.Instance.UserProfile.SourceControlURL;
                mSourceControl.SourceControlUser =  WorkSpace.Instance.UserProfile.SourceControlUser;
                mSourceControl.SourceControlPass =  WorkSpace.Instance.UserProfile.SourceControlPass;
                mSourceControl.SourceControlLocalFolder =  WorkSpace.Instance.UserProfile.SourceControlLocalFolder;

                mSourceControl.SourceControlConfigureProxy =  WorkSpace.Instance.UserProfile.SolutionSourceControlConfigureProxy;
                mSourceControl.SourceControlProxyAddress =  WorkSpace.Instance.UserProfile.SolutionSourceControlProxyAddress;
                mSourceControl.SourceControlProxyPort =  WorkSpace.Instance.UserProfile.SolutionSourceControlProxyPort;

                // If the UserProfile has been deleted or been created for the first time
                if ( WorkSpace.Instance.UserProfile.SolutionSourceControlTimeout == 0)
                {
                     WorkSpace.Instance.UserProfile.SolutionSourceControlTimeout = 80;
                }
                mSourceControl.SourceControlTimeout =  WorkSpace.Instance.UserProfile.SolutionSourceControlTimeout;

                mSourceControl.PropertyChanged += SourceControl_PropertyChanged;
            }
        }

        private static void SourceControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
             WorkSpace.Instance.UserProfile.SourceControlType = mSourceControl.GetSourceControlType;
             WorkSpace.Instance.UserProfile.SourceControlURL = mSourceControl.SourceControlURL;
             WorkSpace.Instance.UserProfile.SourceControlUser = mSourceControl.SourceControlUser;
             WorkSpace.Instance.UserProfile.SourceControlPass = mSourceControl.SourceControlPass;
             WorkSpace.Instance.UserProfile.SourceControlLocalFolder = mSourceControl.SourceControlLocalFolder;

             WorkSpace.Instance.UserProfile.SolutionSourceControlConfigureProxy = mSourceControl.SourceControlConfigureProxy;
             WorkSpace.Instance.UserProfile.SolutionSourceControlProxyAddress = mSourceControl.SourceControlProxyAddress;
             WorkSpace.Instance.UserProfile.SolutionSourceControlProxyPort = mSourceControl.SourceControlProxyPort;
             WorkSpace.Instance.UserProfile.SolutionSourceControlTimeout = mSourceControl.SourceControlTimeout;
        }


        private async void TestConnectionAndSearchRepositories_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(mSourceControl.SourceControlUser) || string.IsNullOrEmpty(mSourceControl.SourceControlPass) || string.IsNullOrEmpty(mSourceControl.SourceControlURL))
                {
                    Reporter.ToUser(eUserMsgKey.SourceControlConnMissingConnInputs);
                    return;
                }
                xConnectButton.Visibility = Visibility.Visible;
                xProcessingIcon.Visibility = Visibility.Visible;

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
                Reporter.ToLog(eLogLevel.ERROR, "Error Occured :", ex);
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

  
    }
}
