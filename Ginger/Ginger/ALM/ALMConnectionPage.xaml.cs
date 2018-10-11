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
using GingerCore;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Ginger.ALM
{
    /// <summary>
    /// Interaction logic for ALMConnectionPage.xaml
    /// </summary>
    public partial class ALMConnectionPage : Page
    {
        //Show as connection or settings window
        bool isConnWin;
        bool isServerDetailsCorrect;
        bool isProjectMappingCorrect;
        GenericWindow _pageGenericWin;
        ALMIntegration.eALMConnectType almConectStyle;

        public ALMConnectionPage(ALMIntegration.eALMConnectType almConnectStyle, bool isConnWin = false)
        {
            InitializeComponent();
            this.isConnWin = isConnWin;
            this.almConectStyle = almConnectStyle;

            App.ObjFieldBinding(ServerURLTextBox, TextBox.TextProperty, ALMIntegration.Instance.AlmConfigurations, nameof(ALMIntegration.Instance.AlmConfigurations.ALMServerURL));
            App.ObjFieldBinding(RestAPICheckBox, CheckBox.IsCheckedProperty, ALMIntegration.Instance.AlmConfigurations, nameof(ALMIntegration.Instance.AlmConfigurations.UseRest));
            App.ObjFieldBinding(UserNameTextBox, TextBox.TextProperty, ALMIntegration.Instance.AlmConfigurations, nameof(ALMIntegration.Instance.AlmConfigurations.ALMUserName));
            App.ObjFieldBinding(DomainComboBox, ComboBox.SelectedValueProperty, ALMIntegration.Instance.AlmConfigurations, nameof(ALMIntegration.Instance.AlmConfigurations.ALMDomain));
            App.ObjFieldBinding(ProjectComboBox, ComboBox.SelectedValueProperty, ALMIntegration.Instance.AlmConfigurations, nameof(ALMIntegration.Instance.AlmConfigurations.ALMProjectName));
            PasswordTextBox.Password = ALMIntegration.Instance.ALMPassword(); //can't do regular binding with PasswordTextBox control for security reasons

            if (!WorkSpace.Instance.BetaFeatures.Rally)
            {
                RallyRadioButton.Visibility = Visibility.Hidden;
                if (App.UserProfile.Solution.AlmType == ALMIntegration.eALMType.RALLY)
                    App.UserProfile.Solution.AlmType = ALMIntegration.eALMType.QC;
            }

            if (!WorkSpace.Instance.BetaFeatures.RestAPI)
            {
                RestAPICheckBox.Visibility = Visibility.Hidden;
                App.UserProfile.Solution.UseRest = false;
                RestAPICheckBox.IsChecked = false;
                RestAPICheckBox.IsEnabled = false;
            }

            if (almConnectStyle != ALMIntegration.eALMConnectType.Silence)
            {
                if (GetProjectsDetails())
                    ConnectProject();
            }
            else RefreshALMSolutionSettings();

            SetControls();
            StyleRadioButtons();
        }

        private void SetControls()
        {
            bool ServerDetailsSelected = false;

            if (!string.IsNullOrEmpty(ServerURLTextBox.Text) && !string.IsNullOrEmpty(UserNameTextBox.Text) && !string.IsNullOrEmpty(PasswordTextBox.Password))
                ServerDetailsSelected = true;

            ALMSelectPanel.Visibility = Visibility.Visible;

            ALMServerDetailsPanel.Visibility = Visibility.Visible;
            if (ServerDetailsSelected)
                LoginServerButton.IsEnabled = true;
            else LoginServerButton.IsEnabled = false;


            if (isServerDetailsCorrect)
            {
                QCRadioButton.IsEnabled = false;
                RestAPICheckBox.IsEnabled = false;
                RQMRadioButton.IsEnabled = false;
                RallyRadioButton.IsEnabled = false;
                RQMLoadConfigPackageButton.IsEnabled = false;
                ServerURLTextBox.IsEnabled = false;
                UserNameTextBox.IsEnabled = false;
                PasswordTextBox.IsEnabled = false;
                LoginServerButton.Content = "Change Server Details";
                ALMProjectDetailsPanel.Visibility = Visibility.Visible;
                if (App.UserProfile.Solution.AlmType == ALMIntegration.eALMType.RQM)
                    ALMDomainSelectionPanel.Visibility = Visibility.Collapsed;
                else ALMDomainSelectionPanel.Visibility = Visibility.Visible;
            }
            else
            {
                QCRadioButton.IsEnabled = true;
                RestAPICheckBox.IsEnabled = true;
                RQMRadioButton.IsEnabled = true;
                RallyRadioButton.IsEnabled = true;
                RQMLoadConfigPackageButton.IsEnabled = true;
                if (App.UserProfile.Solution.AlmType == ALMIntegration.eALMType.RQM)
                    ServerURLTextBox.IsEnabled = false;
                else
                {
                    ServerURLTextBox.IsEnabled = true;
                    ServerURLTextBox.IsReadOnly = false;
                }
                UserNameTextBox.IsEnabled = true;
                PasswordTextBox.IsEnabled = true;
                if (isConnWin)
                    LoginServerButton.Content = "Connect ALM Server";
                else LoginServerButton.Content = "Get Projects Details";
                ALMProjectDetailsPanel.Visibility = Visibility.Collapsed;
            }

            if (isProjectMappingCorrect)
            {
                DomainComboBox.IsEnabled = false;
                ProjectComboBox.IsEnabled = false;
                ConnectProjectButton.Content = "Change Project Mapping";
            }
            else
            {
                DomainComboBox.IsEnabled = true;
                ProjectComboBox.IsEnabled = true;
                if (isConnWin)
                    ConnectProjectButton.Content = "Connect";
                else ConnectProjectButton.Content = "Save Project Mapping";
            }
        }

        public void RefreshALMSolutionSettings()
        {
            if (App.UserProfile.Solution.AlmType == ALMIntegration.eALMType.QC && QCRadioButton.IsChecked == false)
            {
                QCRadioButton.IsChecked = true;
                PasswordTextBox.Password = "";
                UserNameTextBox.Text = "";
                StyleRadioButtons();
            }
            else if (App.UserProfile.Solution.AlmType == ALMIntegration.eALMType.RQM && RQMRadioButton.IsChecked == false)
            {
                RQMRadioButton.IsChecked = true;
                PasswordTextBox.Password = "";
                UserNameTextBox.Text = "";
                StyleRadioButtons();
            }
            else if (App.UserProfile.Solution.AlmType == ALMIntegration.eALMType.RALLY && RallyRadioButton.IsChecked == false)
            {
                RallyRadioButton.IsChecked = true;
                PasswordTextBox.Password = "";
                UserNameTextBox.Text = "";
                StyleRadioButtons();
            }
        }

        private void ClearALMConfigs()
        {
            ServerURLTextBox.Text = null;
            UserNameTextBox.Text = null;
            PasswordTextBox.Password = null;
            App.UserProfile.ALMPassword = null;
            DomainComboBox.SelectedItem = null;
            DomainComboBox.SelectedValue = null;
            DomainComboBox.Items.Clear();
            ProjectComboBox.SelectedItem = null;
            ProjectComboBox.SelectedValue = null;
            ProjectComboBox.Items.Clear();
            RestAPICheckBox.IsChecked = false;

            ALMIntegration.Instance.SyncConfigurations();
        }

        private bool GetProjectsDetails()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            bool almConn = false;
            ALMIntegration.Instance.UpdateALMType(App.UserProfile.Solution.AlmType);

            if (LoginServerButton.Content.ToString() == "Get Projects Details" || LoginServerButton.Content.ToString() == "Connect ALM Server")
            {
                almConn = ALMIntegration.Instance.TestALMServerConn(almConectStyle);
                if (almConn)
                {
                    RefreshDomainList(almConectStyle);
                    RefreshProjectsList();
                }

            }

            isServerDetailsCorrect = almConn;
            if (!isServerDetailsCorrect)
                isProjectMappingCorrect = false;

            SetControls();
            Mouse.OverrideCursor = null;
            return almConn;
        }

        private void GetProjectsDetails_Clicked(object sender, RoutedEventArgs e)
        {
            almConectStyle = ALMIntegration.eALMConnectType.Manual;
            GetProjectsDetails();
        }

        private void RefreshDomainList(ALMIntegration.eALMConnectType userMsgStyle)
        {
            List<string> Domains = ALMIntegration.Instance.GetALMDomains(userMsgStyle);

            string currDomain = App.UserProfile.Solution.ALMDomain;
            DomainComboBox.Items.Clear();
            foreach (string domain in Domains)
                DomainComboBox.Items.Add(domain);

            if (DomainComboBox.Items.Count > 0)
            {
                if (string.IsNullOrEmpty(currDomain) == false)
                {
                    if (DomainComboBox.Items.Contains(currDomain))
                    {
                        App.UserProfile.Solution.ALMDomain = currDomain;
                        DomainComboBox.SelectedIndex = DomainComboBox.Items.IndexOf(App.UserProfile.Solution.ALMDomain);
                    }
                }
                if (DomainComboBox.SelectedIndex == -1)
                    DomainComboBox.SelectedIndex = 0;
            }
        }

        private void DomainComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null || ALMSettingsPannel == null || DomainComboBox.SelectedItem == null)
                return;

            RefreshProjectsList();
        }

        private void RefreshProjectsList()
        {
            if (ALMIntegration.Instance.TestALMServerConn(almConectStyle))
            {
                string currSelectedDomain = (string)DomainComboBox.SelectedItem;
                if (string.IsNullOrEmpty(currSelectedDomain))
                {
                    if (string.IsNullOrEmpty(App.UserProfile.Solution.ALMDomain))
                        return;

                    currSelectedDomain = App.UserProfile.Solution.ALMDomain;
                    DomainComboBox.SelectedItem = currSelectedDomain;
                }

                List<string> lstProjects = ALMIntegration.Instance.GetALMDomainProjects(currSelectedDomain, almConectStyle);

                string currSavedProj = App.UserProfile.Solution.ALMProject;
                ProjectComboBox.Items.Clear();
                foreach (string project in lstProjects)
                    ProjectComboBox.Items.Add(project);

                if (ProjectComboBox.Items.Count > 0)
                {
                    if (string.IsNullOrEmpty(currSavedProj) == false)
                    {
                        if (ProjectComboBox.Items.Contains(currSavedProj))
                        {
                            ProjectComboBox.SelectedIndex = ProjectComboBox.Items.IndexOf(currSavedProj);
                            App.UserProfile.Solution.ALMProject = currSavedProj;
                        }
                    }
                    if (ProjectComboBox.SelectedIndex == -1)
                    {
                        ProjectComboBox.SelectedIndex = 0;
                        App.UserProfile.Solution.ALMProject = ProjectComboBox.Text;
                    }

                }
            }
        }

        private void ConnectProject()
        {
            if (ConnectProjectButton.Content.ToString() == "Save Project Mapping" || ConnectProjectButton.Content.ToString() == "Connect")
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                if (ALMIntegration.Instance.TestALMProjectConn(almConectStyle))
                {
                    if ((almConectStyle == ALMIntegration.eALMConnectType.Manual) || (almConectStyle == ALMIntegration.eALMConnectType.SettingsPage))
                    {
                        SaveALMConfigs();
                    }
                    isProjectMappingCorrect = true;
                }
                Mouse.OverrideCursor = null;
            }
            else
            {
                ALMIntegration.Instance.DisconnectALMProjectStayLoggedIn();
                isProjectMappingCorrect = false;
            }
            SetControls();
        }

        private void ConnectProjectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectProject();
        }

        public void ShowAsWindow()
        {
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, eWindowShowStyle.Dialog, this.Title, this, closeEventHandler: CloseWindow);
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            ALMIntegration.Instance.SyncConfigurations();
        }

        private void StyleRadioButtons()
        {
            switch (App.UserProfile.Solution.AlmType)
            {
                case ALMIntegration.eALMType.QC:
                    QCRadioButton.IsChecked = true;
                    QCRadioButton.FontWeight = FontWeights.ExtraBold;
                    QCRadioButton.Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                    RQMLoadConfigPackageButton.Visibility = Visibility.Collapsed;
                    DownloadPackageLink.Visibility = Visibility.Collapsed;
                    Grid.SetColumnSpan(ServerURLTextBox, 2);
                    ExampleURLHint.Content = "Example: http://server:8080/almbin";
                    if (!isServerDetailsCorrect)
                    {
                        ServerURLTextBox.IsEnabled = true;
                        ServerURLTextBox.IsReadOnly = false;
                    }
                    else
                    {
                        ServerURLTextBox.IsEnabled = false;
                        ServerURLTextBox.IsReadOnly = true;
                    }
                    ServerURLTextBox.Cursor = null;
                    RQMRadioButton.FontWeight = FontWeights.Regular;
                    RQMRadioButton.Foreground = Brushes.Black;
                    RQMRadioButton.IsChecked = false;
                    RallyRadioButton.FontWeight = FontWeights.Regular;
                    RallyRadioButton.Foreground = Brushes.Black;
                    RallyRadioButton.IsChecked = false;
                    if (WorkSpace.Instance.BetaFeatures.RestAPI)
                    {
                        RestAPICheckBox.Visibility = Visibility.Visible;
                    }
                    break;
                case ALMIntegration.eALMType.RQM:
                    RQMRadioButton.IsChecked = true;
                    RQMRadioButton.FontWeight = FontWeights.ExtraBold;
                    RQMRadioButton.Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                    RQMLoadConfigPackageButton.Visibility = Visibility.Visible;
                    DownloadPackageLink.Visibility = Visibility.Visible;
                    Grid.SetColumnSpan(ServerURLTextBox, 1);
                    SetLoadPackageButtonContent();
                    ServerURLTextBox.IsReadOnly = true;
                    ServerURLTextBox.IsEnabled = false;
                    ServerURLTextBox.Cursor = Cursors.Arrow;
                    QCRadioButton.FontWeight = FontWeights.Regular;
                    QCRadioButton.Foreground = Brushes.Black;
                    QCRadioButton.IsChecked = false;
                    RallyRadioButton.FontWeight = FontWeights.Regular;
                    RallyRadioButton.Foreground = Brushes.Black;
                    RallyRadioButton.IsChecked = false;
                    RestAPICheckBox.Visibility = Visibility.Hidden;
                    break;
                case ALMIntegration.eALMType.RALLY:
                    RallyRadioButton.IsChecked = true;
                    RallyRadioButton.FontWeight = FontWeights.ExtraBold;
                    RallyRadioButton.Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                    RQMLoadConfigPackageButton.Visibility = Visibility.Collapsed;
                    DownloadPackageLink.Visibility = Visibility.Collapsed;
                    Grid.SetColumnSpan(ServerURLTextBox, 2);
                    ExampleURLHint.Content = "Example: http://server:8080/almbin";
                    if (!isServerDetailsCorrect)
                    {
                        ServerURLTextBox.IsEnabled = true;
                        ServerURLTextBox.IsReadOnly = false;
                    }
                    else
                    {
                        ServerURLTextBox.IsEnabled = false;
                        ServerURLTextBox.IsReadOnly = true;
                    }
                    ServerURLTextBox.Cursor = null;
                    QCRadioButton.FontWeight = FontWeights.Regular;
                    QCRadioButton.Foreground = Brushes.Black;
                    QCRadioButton.IsChecked = false;
                    RQMRadioButton.FontWeight = FontWeights.Regular;
                    RQMRadioButton.Foreground = Brushes.Black;
                    RQMRadioButton.IsChecked = false;
                    RestAPICheckBox.Visibility = Visibility.Hidden;
                    break;
            }
        }

        private void SaveALMConfigs()
        {
            ALMIntegration.Instance.SyncConfigurations();
            App.UserProfile.SaveUserProfile();
            App.UserProfile.Solution.SaveSolution(true, SolutionGeneral.Solution.eSolutionItemToSave.ALMSettings);
        }

        private void ALMRadioButton_Checked_Changed(object sender, RoutedEventArgs e)
        {
            if (sender == null || ALMSettingsPannel == null)
                return;

            RadioButton rBtn = sender as RadioButton;
            if ((bool)rBtn.IsChecked)
            {
                switch (rBtn.Name)
                {
                    case "QCRadioButton":
                        if (App.UserProfile.Solution.AlmType != ALMIntegration.eALMType.QC)
                        {
                            App.UserProfile.Solution.AlmType = ALMIntegration.eALMType.QC;
                            ClearALMConfigs();
                        }

                        break;
                    case "RQMRadioButton":
                        if (App.UserProfile.Solution.AlmType != ALMIntegration.eALMType.RQM)
                        {
                            App.UserProfile.Solution.AlmType = ALMIntegration.eALMType.RQM;
                            ClearALMConfigs();
                            ALMIntegration.Instance.UpdateALMType(ALMIntegration.eALMType.RQM);
                            ALMIntegration.Instance.SetALMCoreConfigurations(); //Because RQM need to update the server field from existing package
                            SetLoadPackageButtonContent();
                        }
                        break;
                    case "RallyRadioButton":
                        if (App.UserProfile.Solution.AlmType != ALMIntegration.eALMType.RALLY)
                        {
                            App.UserProfile.Solution.AlmType = ALMIntegration.eALMType.RALLY;
                            ClearALMConfigs();
                        }
                        break;
                }
                ALMIntegration.Instance.UpdateALMType(App.UserProfile.Solution.AlmType);
                StyleRadioButtons();
                SetControls();
            }
        }

        private void SetLoadPackageButtonContent()
        {
            if (!string.IsNullOrEmpty(ServerURLTextBox.Text))
            {
                RQMLoadConfigPackageButton.Content = "Replace";
                ExampleURLHint.Content = "and click Replace to change RQM Configuration Package";

            }
            else
            {
                RQMLoadConfigPackageButton.Content = "Load";
                ExampleURLHint.Content = "and Load RQM Configuration Package";
            }
        }

        private void ServerURLTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (ServerURLTextBox.Text.ToLower().Contains("qcbin"))
            {
                //remove rest of URL
                ServerURLTextBox.Text = ServerURLTextBox.Text.Substring(0,
                                           ServerURLTextBox.Text.ToLower().IndexOf("qcbin") + 5);
            }

            SetControls();
        }

        private void UserNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            App.UserProfile.ALMUserName = UserNameTextBox.Text;
            SetControls();
        }

        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ALMIntegration.Instance.SetALMPassword(PasswordTextBox.Password);
            SetControls();
        }

        private void ProjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectComboBox != null && ProjectComboBox.SelectedItem != null)
                ALMIntegration.Instance.SetALMProject(ProjectComboBox.SelectedItem.ToString());
        }

        private void TestALMConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            bool connectionSucc = false;
            try { connectionSucc = ALMIntegration.Instance.TestALMProjectConn(almConectStyle); }
            catch (Exception) { }

            if (connectionSucc)
                Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, "Passed! ALM connection test passed successfully");
            else
                Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, "Failed! ALM connection test failed, Please check ALM connection details");
            Mouse.OverrideCursor = null;
        }

        private void LoadRQMConfigPackageButton_Click(object sender, RoutedEventArgs e)
        {
            if (ALMIntegration.Instance.LoadALMConfigurations())
                SetLoadPackageButtonContent();
        }

        private void HandleLinkClick(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://ginger/Downloads/Other");
            e.Handled = true;
        }

        private void RestAPICheckBox_Checked(object sender, RoutedEventArgs e)
        {
            App.UserProfile.Solution.UseRest = true;
            ExampleURLHint.Content = "Example: http://server:8080/";
        }

        private void RestAPICheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            App.UserProfile.Solution.UseRest = false;
            ExampleURLHint.Content = "Example: http://server:8080/almbin";
        }
    }
}
