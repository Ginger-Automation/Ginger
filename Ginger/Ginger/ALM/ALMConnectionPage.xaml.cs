#region License
/*
Copyright © 2014-2023 European Support Limited

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
using GingerCore.ALM;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;

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
        eALMConnectType almConectStyle;

        private void Bind()
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ConfigPackageTextBox, TextBox.TextProperty, CurrentAlmConfigurations, nameof(CurrentAlmConfigurations.ALMConfigPackageFolderPath));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ServerURLTextBox, TextBox.TextProperty, CurrentAlmConfigurations, nameof(CurrentAlmConfigurations.ALMServerURL));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RestAPICheckBox, CheckBox.IsCheckedProperty, CurrentAlmConfigurations, nameof(CurrentAlmConfigurations.UseRest));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(TokenCheckBox, CheckBox.IsCheckedProperty, CurrentAlmConfigurations, nameof(CurrentAlmConfigurations.UseToken));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(UserNameTextBox, TextBox.TextProperty, CurrentAlmUserConfigurations, nameof(CurrentAlmUserConfigurations.ALMUserName));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DomainComboBox, ComboBox.SelectedValueProperty, CurrentAlmConfigurations, nameof(CurrentAlmConfigurations.ALMDomain));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ProjectComboBox, ComboBox.SelectedValueProperty, CurrentAlmConfigurations, nameof(CurrentAlmConfigurations.ALMProjectKey));

            if (CurrentAlmConfigurations.AlmType == eALMType.Jira)
            {
                List<string> jiraTestingALMs = ALMIntegration.Instance.GetJiraTestingALMs();
                GingerCore.General.FillComboFromList(JiraTestingALMComboBox, jiraTestingALMs);
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(JiraTestingALMComboBox, ComboBox.TextProperty, CurrentAlmConfigurations, nameof(CurrentAlmConfigurations.JiraTestingALM));
            }
            PasswordTextBox.Password = CurrentAlmUserConfigurations.ALMPassword; //can't do regular binding with PasswordTextBox control for security reasons
        }
        public ALMConnectionPage(eALMConnectType almConnectStyle, bool isConnWin = false)
        {
            CurrentAlmConfigurations = ALMIntegration.Instance.GetDefaultAlmConfig();
            CurrentAlmConfigurations.PropertyChanged += CurrentAlmConfigurations_PropertyChanged;
            CurrentAlmUserConfigurations = ALMIntegration.Instance.GetCurrentAlmUserConfig(CurrentAlmConfigurations.AlmType);
            ALMIntegration.Instance.UpdateALMType(CurrentAlmConfigurations.AlmType);

            InitializeComponent();
            this.isConnWin = isConnWin;
            this.almConectStyle = almConnectStyle;

            Bind();

            if (!WorkSpace.Instance.BetaFeatures.Rally)
            {
                RallyRadioButton.Visibility = Visibility.Hidden;
                if (CurrentAlmConfigurations.AlmType == eALMType.RALLY)
                {
                    CurrentAlmConfigurations.AlmType = eALMType.QC;
                }
            }
            if (almConnectStyle != eALMConnectType.Silence)
            {
                if (GetProjectsDetails())
                {
                    ConnectProjectButton.Content = "Save Project Mapping";
                    ConnectProject();
                }
            }
            StyleRadioButtons();
            SetControls();
            ChangeALMType();
        }

        private void CurrentAlmConfigurations_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GingerCoreNET.ALMLib.ALMConfig.ALMUserName) ||
                 e.PropertyName == nameof(GingerCoreNET.ALMLib.ALMConfig.ALMPassword) ||
                 e.PropertyName == nameof(GingerCoreNET.ALMLib.ALMConfig.ALMServerURL))
            {
                if(ALMIntegration.Instance.AlmCore != null)
                {
                    ALMIntegration.Instance.AlmCore.IsConnectValidationDone = false;
                }
            }

        }

        private GingerCoreNET.ALMLib.ALMConfig CurrentAlmConfigurations { get; set; }
        private GingerCoreNET.ALMLib.ALMUserConfig CurrentAlmUserConfigurations { get; set; }
        
        private void SetControls()
        {
            if (!string.IsNullOrEmpty(ServerURLTextBox.Text) && !string.IsNullOrEmpty(UserNameTextBox.Text) && !(string.IsNullOrEmpty(PasswordTextBox.Password) && xPasswordPanel.Visibility == Visibility.Visible))
            {
                LoginServerButton.IsEnabled = true;
            }
            else
            {
                LoginServerButton.IsEnabled = false;
            }

            RQMLoadConfigPackageButton.IsEnabled = true;
            ConfigPackageTextBox.IsEnabled = true;
            if (CurrentAlmConfigurations.AlmType == eALMType.RQM)
            {
                ServerURLTextBox.IsEnabled = true;
                ServerURLTextBox.IsReadOnly = false;
            }
            else
            {
                ServerURLTextBox.IsEnabled = true;
                ServerURLTextBox.IsReadOnly = false;
            }
            UserNameTextBox.IsEnabled = true;
            PasswordTextBox.IsEnabled = true;
            RestAPICheckBox.IsEnabled = false;
            TokenCheckBox.IsEnabled = true;
            if (isConnWin)
            {
                LoginServerButton.Content = "Connect ALM Server";
            }
            else
            {
                LoginServerButton.Content = "Get Projects Details";
            }
            if (CurrentAlmConfigurations.AlmType == eALMType.RQM)
            {
                ALMDomainSelectionPanel.Visibility = Visibility.Collapsed;
            }
            else if (CurrentAlmConfigurations.AlmType == eALMType.Qtest)
            {
                ALMDomainSelectionPanel.Visibility = Visibility.Collapsed;
                RestAPICheckBox.IsEnabled = false;
            }
            else
            {
                ALMDomainSelectionPanel.Visibility = Visibility.Visible;
            }
            if (isServerDetailsCorrect)
            {
                RQMLoadConfigPackageButton.IsEnabled = false;
                ConfigPackageTextBox.IsEnabled = false;
                ServerURLTextBox.IsEnabled = false;
                UserNameTextBox.IsEnabled = false;
                PasswordTextBox.IsEnabled = false;
                RestAPICheckBox.IsEnabled = false;
                TokenCheckBox.IsEnabled = false;
                LoginServerButton.Content = "Change Server Details";
            }

            DomainComboBox.IsEnabled = true;
            ProjectComboBox.IsEnabled = true;
            JiraTestingALMComboBox.IsEnabled = true;
            if (isConnWin)
            {
                ConnectProjectButton.Content = "Connect";
            }
            else
            {
                ConnectProjectButton.Content = "Save Project Mapping";
            }
            if (isProjectMappingCorrect)
            {
                DomainComboBox.IsEnabled = false;
                ProjectComboBox.IsEnabled = false;
                JiraTestingALMComboBox.IsEnabled = false;
                ConnectProjectButton.Content = "Change Project Mapping";
            }
        }

        private void ChangeALMType()
        {
            if (CurrentAlmConfigurations.AlmType == eALMType.QC && !(bool)QCRadioButton.IsChecked)
            {
                QCRadioButton.IsChecked = true;
            }
            else if (CurrentAlmConfigurations.AlmType == eALMType.RQM && !(bool)RQMRadioButton.IsChecked)
            {
                RQMRadioButton.IsChecked = true;
            }
            else if (CurrentAlmConfigurations.AlmType == eALMType.Jira && !(bool)JiraRadioButton.IsChecked)
            {
                JiraRadioButton.IsChecked = true;
            }
            else if (CurrentAlmConfigurations.AlmType == eALMType.RALLY && (bool)RallyRadioButton.IsChecked)
            {
                RallyRadioButton.IsChecked = true;
            }
            else if (CurrentAlmConfigurations.AlmType == eALMType.Qtest && !(bool)qTestRadioButton.IsChecked)
            {
                qTestRadioButton.IsChecked = true;
            }
            else if (CurrentAlmConfigurations.AlmType == eALMType.Octane && !(bool)qTestRadioButton.IsChecked)
            {
                OctaneRadioButton.IsChecked = true;
            }
            else if (CurrentAlmConfigurations.AlmType == eALMType.ZephyrEnterprise && !(bool)ZephyrEntRadioButton.IsChecked)
            {
                ZephyrEntRadioButton.IsChecked = true;
            }

        }

        private bool GetProjectsDetails()
        {
            if (string.IsNullOrEmpty(CurrentAlmConfigurations.ALMServerURL))
            {
                isServerDetailsCorrect = false;
                return false;
            }
            Mouse.OverrideCursor = Cursors.Wait;
            //Removing ending "/" from the ServerURL for JIRA
            if (CurrentAlmConfigurations.AlmType == eALMType.Jira && CurrentAlmConfigurations.ALMServerURL.EndsWith("/"))
            {
                CurrentAlmConfigurations.ALMServerURL = CurrentAlmConfigurations.ALMServerURL.Substring(0, CurrentAlmConfigurations.ALMServerURL.LastIndexOf("/"));
            }
            bool almConn = false;
            ALMIntegration.Instance.UpdateALMType(CurrentAlmConfigurations.AlmType);

            if (LoginServerButton.Content.ToString() == "Get Projects Details" || LoginServerButton.Content.ToString() == "Connect ALM Server")
            {
                almConn = ALMIntegration.Instance.TestALMServerConn(almConectStyle);
                if (almConn)
                {
                    RefreshDomainList(almConectStyle);
                    RefreshProjectsList();
                }
                else
                {
                    DomainComboBox.Items.Clear();
                    ProjectComboBox.Items.Clear();
                    JiraTestingALMComboBox.Items.Clear();
                }
            }
     
            isServerDetailsCorrect = almConn;
            if (isServerDetailsCorrect)
            {
                isProjectMappingCorrect = true;
            }
            else
            {
                isProjectMappingCorrect = false;
            }
            SetControls();
            Mouse.OverrideCursor = null;
            return almConn;
        }

        private void GetProjectsDetails_Clicked(object sender, RoutedEventArgs e)
        {
            almConectStyle = eALMConnectType.Manual;
            GetProjectsDetails();
            isProjectMappingCorrect = false;
            SetControls();
        }

        private void RefreshDomainList(eALMConnectType userMsgStyle)
        {
            List<string> Domains = ALMIntegration.Instance.GetALMDomains(userMsgStyle);

            string currDomain = CurrentAlmConfigurations.ALMDomain;
            DomainComboBox.Items.Clear();
            foreach (string domain in Domains)
                DomainComboBox.Items.Add(domain);

            if (DomainComboBox.Items.Count > 0)
            {
                //sort
                DomainComboBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("", System.ComponentModel.ListSortDirection.Descending));

                if (string.IsNullOrEmpty(currDomain) == false)
                {
                    if (DomainComboBox.Items.Contains(currDomain))
                    {
                        CurrentAlmConfigurations.ALMDomain = currDomain;
                        DomainComboBox.SelectedIndex = DomainComboBox.Items.IndexOf(CurrentAlmConfigurations.ALMDomain);
                    }
                }
                if (DomainComboBox.SelectedIndex == -1)
                {
                    DomainComboBox.SelectedIndex = 0;
                }
            }
        }

        private void DomainComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null || ALMSettingsPannel == null || DomainComboBox.SelectedItem == null)
                return;

            RefreshProjectsList();
        }

        private void JiraTestingALMComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (JiraTestingALMComboBox != null && JiraTestingALMComboBox.SelectedItem != null)
            {
                CurrentAlmConfigurations.JiraTestingALM = (eTestingALMType)Enum.Parse(typeof(eTestingALMType), JiraTestingALMComboBox.SelectedItem.ToString());
            }
        }

        private void RefreshProjectsList()
        {
            if (ALMIntegration.Instance.TestALMServerConn(almConectStyle))
            {
                string currSelectedDomain = CurrentAlmConfigurations.ALMDomain;
                if (CurrentAlmConfigurations.AlmType != eALMType.Qtest)
                {
                    if (string.IsNullOrEmpty(currSelectedDomain))
                    {
                        if (string.IsNullOrEmpty(CurrentAlmConfigurations.ALMDomain))
                        {
                            return;
                        }
                        currSelectedDomain = CurrentAlmConfigurations.ALMDomain;
                        DomainComboBox.SelectedItem = currSelectedDomain;
                    }
                }
                Dictionary<string, string> lstProjects = ALMIntegration.Instance.GetALMDomainProjects(currSelectedDomain, almConectStyle);

                KeyValuePair<string, string> currSavedProj = new KeyValuePair<string, string>(CurrentAlmConfigurations.ALMProjectKey, CurrentAlmConfigurations.ALMProjectName);
                ProjectComboBox.Items.Clear();
                foreach (KeyValuePair<string, string> project in lstProjects)
                {
                    ProjectComboBox.Items.Add(new KeyValuePair<string, string>(project.Key, project.Value));
                }

                if (ProjectComboBox.Items.Count > 0)
                {
                    if (string.IsNullOrEmpty(currSavedProj.Key) == false)
                    {
                        if (ProjectComboBox.Items.Contains(currSavedProj))
                        {
                            ProjectComboBox.SelectedIndex = ProjectComboBox.Items.IndexOf(currSavedProj);
                            CurrentAlmConfigurations.ALMProjectName = currSavedProj.Value;
                            CurrentAlmConfigurations.ALMProjectKey = currSavedProj.Key;
                        }
                    }
                    if (ProjectComboBox.SelectedIndex == -1)
                    {
                        ProjectComboBox.SelectedIndex = 0;
                        CurrentAlmConfigurations.ALMProjectName = ProjectComboBox.Text;
                        CurrentAlmConfigurations.ALMProjectKey = ProjectComboBox.SelectedValue.ToString();
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
            if(ConnectProjectButton.Content.ToString() == "Save Project Mapping")
            {
                SaveConnectionDetails();
                return;
            }
            ConnectProject();
        }

        private void SaveConnectionDetails()
        {
            ALMIntegration.Instance.SetALMCoreConfigurations(CurrentAlmConfigurations.AlmType);
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            if (ALMIntegration.Instance.TestALMProjectConn(almConectStyle))
            {
                if ((almConectStyle == eALMConnectType.Manual) || (almConectStyle == eALMConnectType.SettingsPage))
                {
                    SaveALMConfigs();
                }
                isProjectMappingCorrect = true;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.ALMConnectFailureWithCurrSettings, "Failed to connect and save");
            }
            Mouse.OverrideCursor = null;
            SetControls();
        }

        public void ShowAsWindow()
        {
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, eWindowShowStyle.Dialog, this.Title, this, closeEventHandler: CloseWindow);
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            _pageGenericWin.Close();
        }

        private void StyleRadioButtons()
        {
            xDefualtImageQC.Visibility = Visibility.Collapsed;
            xDefualtImageRQM.Visibility = Visibility.Collapsed;
            xDefualtImageRally.Visibility = Visibility.Collapsed;
            xDefualtImageJIRA.Visibility = Visibility.Collapsed;
            xDefualtImageQTest.Visibility = Visibility.Collapsed;
            xDefualtImageOctane.Visibility = Visibility.Collapsed;
            xDefualtImageZephyrEnt.Visibility = Visibility.Collapsed;
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
            RQMLoadConfigPackageButton.Visibility = Visibility.Hidden;
            ConfigPackageLabel.Visibility = Visibility.Hidden;
            ConfigPackageTextBox.Visibility = Visibility.Hidden;
            JiraTestingALMSelectionPanel.Visibility = Visibility.Hidden;
            DownloadPackageLink.Visibility = Visibility.Collapsed;
            Grid.SetColumnSpan(ServerURLTextBox, 2);
            ExampleURLHint.Content = "Example: http://server:port ";
            ServerURLTextBox.Cursor = null;
            
            RestAPICheckBox.Visibility = Visibility.Collapsed;
            RestAPICheckBox.IsChecked = true;
            RestAPICheckBox.IsEnabled = false;

            TokenCheckBox.Visibility = Visibility.Collapsed;
            TokenCheckBox.IsEnabled = false;

            QCRadioButton.FontWeight = FontWeights.Regular;
            QCRadioButton.Foreground = Brushes.Black;
            RQMRadioButton.FontWeight = FontWeights.Regular;
            RQMRadioButton.Foreground = Brushes.Black;
            RallyRadioButton.FontWeight = FontWeights.Regular;
            RallyRadioButton.Foreground = Brushes.Black;
            JiraRadioButton.FontWeight = FontWeights.Regular;
            JiraRadioButton.Foreground = Brushes.Black;
            qTestRadioButton.FontWeight = FontWeights.Regular;
            qTestRadioButton.Foreground = Brushes.Black;
            OctaneRadioButton.FontWeight = FontWeights.Regular;
            OctaneRadioButton.Foreground = Brushes.Black;
            ZephyrEntRadioButton.FontWeight = FontWeights.Regular;
            ZephyrEntRadioButton.Foreground = Brushes.Black;
            xPasswordPanel.Visibility = Visibility.Visible;
            switch (CurrentAlmConfigurations.AlmType)
            {
                case GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.QC:
                    xDefualtImageQC.Visibility = Visibility.Visible;
                    QCRadioButton.FontWeight = FontWeights.ExtraBold;
                    QCRadioButton.Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                    RQMLoadConfigPackageButton.Visibility = Visibility.Collapsed;
                    ConfigPackageLabel.Visibility = Visibility.Collapsed;
                    ConfigPackageTextBox.Visibility = Visibility.Collapsed;
                    DownloadPackageLink.Visibility = Visibility.Collapsed;
                    PackageHint.Visibility = Visibility.Collapsed;
                    Grid.SetColumnSpan(ServerURLTextBox, 2);
                    ExampleURLHint.Content = "Example: http://server:8080/";
                    JiraTestingALMSelectionPanel.Visibility = Visibility.Hidden;
                    ServerURLTextBox.Cursor = null;                 
                    RestAPICheckBox.Visibility = Visibility.Visible;
                    RestAPICheckBox.IsEnabled = false;
                    break;

                case GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.RQM:
                    xDefualtImageRQM.Visibility = Visibility.Visible;
                    RQMRadioButton.FontWeight = FontWeights.ExtraBold;
                    RQMRadioButton.Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                    RQMLoadConfigPackageButton.Visibility = Visibility.Visible;
                    ConfigPackageLabel.Visibility = Visibility.Visible;
                    ConfigPackageTextBox.Visibility = Visibility.Visible;
                    DownloadPackageLink.Visibility = Visibility.Visible;
                    PackageHint.Visibility = Visibility.Visible;
                    Grid.SetColumnSpan(ServerURLTextBox, 1);
                    SetLoadPackageButtonContent();
                    ServerURLTextBox.IsReadOnly = true;
                    ServerURLTextBox.IsEnabled = false;
                    JiraTestingALMSelectionPanel.Visibility = Visibility.Hidden;
                    ServerURLTextBox.Cursor = Cursors.Arrow;
                    break;

                case GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.RALLY:
                    xDefualtImageRally.Visibility = Visibility.Visible;
                    RallyRadioButton.FontWeight = FontWeights.ExtraBold;
                    RallyRadioButton.Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                    RQMLoadConfigPackageButton.Visibility = Visibility.Collapsed;
                    ConfigPackageLabel.Visibility = Visibility.Collapsed;
                    ConfigPackageTextBox.Visibility = Visibility.Collapsed;
                    DownloadPackageLink.Visibility = Visibility.Collapsed;
                    PackageHint.Visibility = Visibility.Collapsed;
                    JiraTestingALMSelectionPanel.Visibility = Visibility.Hidden;
                    Grid.SetColumnSpan(ServerURLTextBox, 2);
                    ExampleURLHint.Content = "Example: http://server:8080/almbin";                    
                    ServerURLTextBox.Cursor = null;
                    break;

                case GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Jira:
                    xDefualtImageJIRA.Visibility = Visibility.Visible;
                    JiraRadioButton.FontWeight = FontWeights.ExtraBold;
                    JiraRadioButton.Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                    RQMLoadConfigPackageButton.Visibility = Visibility.Visible;
                    ConfigPackageLabel.Visibility = Visibility.Visible;
                    ConfigPackageTextBox.Visibility = Visibility.Visible;
                    DownloadPackageLink.Visibility = Visibility.Visible;
                    PackageHint.Visibility = Visibility.Visible;
                    JiraTestingALMSelectionPanel.Visibility = Visibility.Visible;
                    Grid.SetColumnSpan(ServerURLTextBox, 2);
                    SetLoadPackageButtonContent();                 
                    ServerURLTextBox.Cursor = null;
                    break;

                case GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Qtest:
                    xDefualtImageQTest.Visibility = Visibility.Visible;
                    qTestRadioButton.FontWeight = FontWeights.ExtraBold;
                    qTestRadioButton.Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                    RQMLoadConfigPackageButton.Visibility = Visibility.Visible;
                    ConfigPackageLabel.Visibility = Visibility.Visible;
                    ConfigPackageTextBox.Visibility = Visibility.Visible;
                    JiraTestingALMSelectionPanel.Visibility = Visibility.Hidden;
                    DownloadPackageLink.Visibility = Visibility.Visible;
                    PackageHint.Visibility = Visibility.Visible;
                    Grid.SetColumnSpan(ServerURLTextBox, 2);
                    SetLoadPackageButtonContent();
                    ExampleURLHint.Content = "Example: https://qtest-url.com/ ";                    
                    ServerURLTextBox.Cursor = null;
                    RestAPICheckBox.IsChecked = true;
                    RestAPICheckBox.IsEnabled = false;
                    TokenCheckBox.Visibility = Visibility.Visible;
                    TokenCheckBox.IsEnabled = true;
                    break;

                case GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Octane:
                    xDefualtImageOctane.Visibility = Visibility.Visible;
                    OctaneRadioButton.FontWeight = FontWeights.ExtraBold;
                    OctaneRadioButton.Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                    RQMLoadConfigPackageButton.Visibility = Visibility.Collapsed;
                    ConfigPackageLabel.Visibility = Visibility.Collapsed;
                    ConfigPackageTextBox.Visibility = Visibility.Collapsed;
                    JiraTestingALMSelectionPanel.Visibility = Visibility.Hidden;
                    DownloadPackageLink.Visibility = Visibility.Collapsed;
                    PackageHint.Visibility = Visibility.Collapsed;
                    Grid.SetColumnSpan(ServerURLTextBox, 2);
                    ExampleURLHint.Content = "Example: http://server:port ";
                    ServerURLTextBox.Cursor = null;
                    RestAPICheckBox.IsChecked = true;
                    RestAPICheckBox.IsEnabled = false;
                    xPasswordPanel.Visibility = Visibility.Collapsed;
                    break;

                case GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.ZephyrEnterprise:
                    xDefualtImageZephyrEnt.Visibility = Visibility.Visible;
                    ZephyrEntRadioButton.FontWeight = FontWeights.ExtraBold;
                    ZephyrEntRadioButton.Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                    RQMLoadConfigPackageButton.Visibility = Visibility.Collapsed;
                    ConfigPackageLabel.Visibility = Visibility.Collapsed;
                    ConfigPackageTextBox.Visibility = Visibility.Collapsed;
                    JiraTestingALMSelectionPanel.Visibility = Visibility.Hidden;
                    DownloadPackageLink.Visibility = Visibility.Collapsed;
                    PackageHint.Visibility = Visibility.Collapsed;
                    Grid.SetColumnSpan(ServerURLTextBox, 2);
                    ExampleURLHint.Content = "Example: http://server:port ";
                    ServerURLTextBox.Cursor = null;
                    RestAPICheckBox.IsChecked = true;
                    RestAPICheckBox.IsEnabled = false;
                    TokenCheckBox.Visibility = Visibility.Visible;
                    TokenCheckBox.IsEnabled = true;
                    break;

                default:
                    //Default not used
                    break;
            }
        }

        private void SaveALMConfigs()
        {
            WorkSpace.Instance.UserProfile.UserProfileOperations.SaveUserProfile();
            WorkSpace.Instance.Solution.SolutionOperations.SaveSolution(true, SolutionGeneral.Solution.eSolutionItemToSave.ALMSettings);
        }

        private void ALMRadioButton_Checked_Changed(object sender, RoutedEventArgs e)
        {
            string prevAlmType = string.Empty;
            if (sender == null || ALMSettingsPannel == null)
            {
                return;
            }
            if (CurrentAlmConfigurations != null)
            {
                prevAlmType = CurrentAlmConfigurations.AlmType.ToString();
                ALMIntegration.Instance.SetALMCoreConfigurations(CurrentAlmConfigurations.AlmType);
            }
            GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType almType = GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.QC;
            RadioButton rBtn = sender as RadioButton;
            if ((bool)rBtn.IsChecked)
            {
                switch (rBtn.Name)
                {
                    case "QCRadioButton":
                            almType = GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.QC;
                            RestAPICheckBox.IsEnabled = false;
                        break;
                    case "RQMRadioButton":
                            almType = GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.RQM;                           
                            SetLoadPackageButtonContent();
                        break;
                    case "RallyRadioButton":
                            almType = GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.RALLY;
                        break;
                    case "JiraRadioButton":
                            almType = GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Jira;
                        break;
                    case "qTestRadioButton":
                            almType = GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Qtest;
                        break;

                    case "OctaneRadioButton":
                        almType = GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Octane;
                        break;

                    case "ZephyrEntRadioButton":
                        almType = GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.ZephyrEnterprise;
                        break;

                    default:
                        //Not used
                        break;
                }
                //Clear bindings
                BindingOperations.ClearAllBindings(ConfigPackageTextBox);
                BindingOperations.ClearAllBindings(ServerURLTextBox);
                BindingOperations.ClearAllBindings(RestAPICheckBox);
                BindingOperations.ClearAllBindings(UserNameTextBox);
                BindingOperations.ClearAllBindings(TokenCheckBox);
                BindingOperations.ClearAllBindings(PasswordTextBox);
                BindingOperations.ClearAllBindings(DomainComboBox);
                BindingOperations.ClearAllBindings(ProjectComboBox);
                BindingOperations.ClearAllBindings(JiraTestingALMComboBox);
               
                ALMIntegration.Instance.SetDefaultAlmConfig(almType);
                ALMIntegration.Instance.UpdateALMType(almType);
                CurrentAlmConfigurations = ALMCore.GetCurrentAlmConfig(almType);
                CurrentAlmUserConfigurations = ALMIntegration.Instance.GetCurrentAlmUserConfig(almType);


                //Bind again as we changed the AlmConfig object
                Bind();

                //Select domain and project based on new AlmConfig
                LoginServerButton.Content = "Get Projects Details";
                if (!prevAlmType.Equals(CurrentAlmConfigurations.AlmType.ToString()))
                {
                    GetProjectsDetails();
                }
                StyleRadioButtons();
                SetControls();
            }
        }

        private void SetLoadPackageButtonContent()
        {
            if (string.IsNullOrEmpty(ConfigPackageTextBox.Text))
            {
                RQMLoadConfigPackageButton.Content = "Load";
            }
            else
            {
                RQMLoadConfigPackageButton.Content = "Replace";
            }

            if (!string.IsNullOrEmpty(ConfigPackageTextBox.Text))
            {
                PackageHint.Content = "Replace " + CurrentAlmConfigurations.AlmType + " Configuration Package";
            }
            else
            {
                PackageHint.Content = "Load " + CurrentAlmConfigurations.AlmType + " Configuration Package";
            }
        }

        private void ServerURLTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ServerURLTextBox.Text.ToLower().Contains("qcbin"))
            {
                //remove rest of URL
                ServerURLTextBox.Text = ServerURLTextBox.Text.Substring(0,ServerURLTextBox.Text.ToLower().IndexOf("qcbin") + 5);
            }

            SetControls();
        }

        private void UserNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetControls();
        }

        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            CurrentAlmUserConfigurations.ALMPassword = PasswordTextBox.Password;
            SetControls();
        }
        private void ProjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectComboBox != null && ProjectComboBox.SelectedItem != null)
            {
                KeyValuePair<string, string> newProject = (KeyValuePair<string, string>)ProjectComboBox.SelectedItem;
                CurrentAlmConfigurations.ALMProjectName = newProject.Value;
                CurrentAlmConfigurations.ALMProjectKey = newProject.Key;
            }
        }

        private void TestALMConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            ALMIntegration.Instance.SetALMCoreConfigurations(CurrentAlmConfigurations.AlmType);
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            bool connectionSucc = false;
            try { connectionSucc = ALMIntegration.Instance.TestALMProjectConn(almConectStyle); }
            catch (Exception) { }

            if (connectionSucc)
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Passed! ALM connection test passed successfully");
            else
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Failed! ALM connection test failed, Please check ALM connection details");
            Mouse.OverrideCursor = null;
        }

        private void LoadRQMConfigPackageButton_Click(object sender, RoutedEventArgs e)
        {
            if (ALMIntegration.Instance.LoadALMConfigurations())
                SetLoadPackageButtonContent();
        }

        private void HandleLinkClick(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo { FileName = @"http://ginger/Downloads/Other", UseShellExecute = true });

            e.Handled = true;
        }

        private void RestAPICheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ExampleURLHint.Content = "Example: http://server:8080/";
        }

        private void RestAPICheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ExampleURLHint.Content = "Example: http://server:8080/almbin";
        }
        private void TokenCheckBox_Checked(object sender, RoutedEventArgs e)
        {

            PasswordLabel.Content = "Token";
        }

        private void TokenCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordLabel.Content = "Password";
        }
        private void ShowToolTip(object sender, MouseEventArgs e)
        {
            if (CurrentAlmConfigurations.AlmType == GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Jira)
            {
                ToolTipService.SetToolTip(ServerURLTextBox, new ToolTip { Content = "Example: http://server", Style = FindResource("ToolTipStyle") as Style });
            }
            if (CurrentAlmConfigurations.AlmType == GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.QC || CurrentAlmConfigurations.AlmType == GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.RALLY)
            {
                ToolTipService.SetToolTip(ServerURLTextBox, new ToolTip { Content = "Example: http://server:8080/almbin", Style = FindResource("ToolTipStyle") as Style });
            }
            if (CurrentAlmConfigurations.AlmType == GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Qtest)
            {
                ToolTipService.SetToolTip(ServerURLTextBox, new ToolTip { Content = "Example: https://qtest-url.com/", Style = FindResource("ToolTipStyle") as Style });
            }
        }
    }
}
