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

using Amdocs.Ginger.Common;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GingerCore;
using amdocs.ginger.GingerCoreNET;
using Ginger.UserControlsLib;

namespace Ginger.Reports
{
    /// <summary>
    /// Interaction logic for ExecutionResultsConfiguration.xaml
    /// </summary>
    public partial class HTMLReportsConfigurationPage : GingerUIPage
    {        
        HTMLReportsConfiguration mHTMLReportConfiguration = new HTMLReportsConfiguration();


        public HTMLReportsConfigurationPage()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            mHTMLReportConfiguration =  WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            mHTMLReportConfiguration.StartDirtyTracking();
            CurrentItemToSave = WorkSpace.Instance.Solution;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(LimitReportFolder, CheckBox.IsCheckedProperty, mHTMLReportConfiguration, nameof(mHTMLReportConfiguration.LimitReportFolderSize));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xCentralizedReportDataServiceURLTextBox, TextBox.TextProperty, mHTMLReportConfiguration, nameof(mHTMLReportConfiguration.CentralizedReportDataServiceURL));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xCentralizedHtmlReportServiceURLTextBox, TextBox.TextProperty, mHTMLReportConfiguration, nameof(mHTMLReportConfiguration.CentralizedHtmlReportServiceURL));


            if (LimitReportFolder.IsChecked == true)
            {
                FolderMaxSize.Visibility = Visibility.Visible;
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SizeTextBox, TextBox.TextProperty, mHTMLReportConfiguration, nameof(mHTMLReportConfiguration.HTMLReportConfigurationMaximalFolderSize));
            }
            else
                FolderMaxSize.Visibility = Visibility.Collapsed;

            SetControls();
        }

        private void SetControls()
        {
            DefaultTemplatePickerCbx_Binding();
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(HTMLReportFolderTextBox, TextBox.TextProperty, mHTMLReportConfiguration, nameof(mHTMLReportConfiguration.HTMLReportsFolder));
            if (mHTMLReportConfiguration.HTMLReportsAutomaticProdIsEnabled)
            {
                htmlReportAutoProdOnRadioBtn.IsChecked = true;
                htmlReportAutoProdOffRadioBtn.IsChecked = false;
            }
            else
            {
                htmlReportAutoProdOnRadioBtn.IsChecked = false;
                htmlReportAutoProdOffRadioBtn.IsChecked = true;
            }

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xCentralizedReportSettingsGrid, Expander.VisibilityProperty, WorkSpace.Instance.UserProfile, nameof(WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures), bindingConvertor: new GingerCore.GeneralLib.BoolVisibilityConverter());
        }
       

        

        private void SelectHTMLReportsFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string s = General.OpenSelectFolderDialog("Save HTML Reports to Folder");
            if (s != null)
            {
                HTMLReportFolderTextBox.Text = s;
            }
        }

        private void htmlReportAutoProdOnRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (htmlAutoProdReportSwitchPnl != null)
            {
                mHTMLReportConfiguration.HTMLReportsAutomaticProdIsEnabled = true;                
            }
        }

        private void htmlReportAutoProdOffRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (htmlAutoProdReportSwitchPnl != null)
            {
                mHTMLReportConfiguration.HTMLReportsAutomaticProdIsEnabled = false;                
            }
        }

       

        private void SizeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                mHTMLReportConfiguration.HTMLReportConfigurationMaximalFolderSize = (long)Convert.ToInt32(SizeTextBox.Text.ToString());
            }
            catch
            {
                mHTMLReportConfiguration.HTMLReportConfigurationMaximalFolderSize = 0;
            }         
        }


        HTMLReportConfiguration mDefualtConfig;
        public void DefaultTemplatePickerCbx_Binding()
        {
            DefaultTemplatePickerCbx.ItemsSource = null;
            ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
            if ( WorkSpace.Instance.Solution != null && HTMLReportConfigurations.Count > 0)
            {
                DefaultTemplatePickerCbx.ItemsSource = HTMLReportConfigurations;
                DefaultTemplatePickerCbx.DisplayMemberPath = HTMLReportConfiguration.Fields.Name;
                DefaultTemplatePickerCbx.SelectedValuePath = HTMLReportConfiguration.Fields.ID;
                SelectDefualtTemplate();
            }            
        }

        private void SelectDefualtTemplate()
        {
            if (mDefualtConfig != null)
                mDefualtConfig.PropertyChanged -= MDefualtConfig_PropertyChanged;

            ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
            mDefualtConfig = HTMLReportConfigurations.Where(x => (x.IsDefault == true)).FirstOrDefault();
            if (mDefualtConfig != null)
            {
                DefaultTemplatePickerCbx.SelectionChanged -= DefaultTemplatePickerCbx_SelectionChanged;
                DefaultTemplatePickerCbx.SelectedItem = mDefualtConfig;
                DefaultTemplatePickerCbx.SelectionChanged += DefaultTemplatePickerCbx_SelectionChanged;
                mDefualtConfig.PropertyChanged += MDefualtConfig_PropertyChanged;
            }
        }

        private void MDefualtConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HTMLReportConfiguration.IsDefault))
                SelectDefualtTemplate();
        }

        private void LimitReportFolder_Checked(object sender, RoutedEventArgs e)
        {
            FolderMaxSize.Visibility = Visibility.Visible;
        }

        private void LimitReportFolder_Unchecked(object sender, RoutedEventArgs e)
        {
            FolderMaxSize.Visibility = Visibility.Collapsed;
        }

        private void xSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (HTMLReportFolderTextBox.Text.Length > 100)
            {
                Reporter.ToUser(eUserMsgKey.FolderNamesAreTooLong);
                return;
            }
            if (Convert.ToInt16(SizeTextBox.Text.ToString()) < 50)
            {
                Reporter.ToUser(eUserMsgKey.FolderSizeTooSmall);
                return;
            }

            //App.AutomateTabGingerRunner.ExecutionLogger.Configuration = WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            WorkSpace.Instance.Solution.SolutionOperations.SaveSolution(true, SolutionGeneral.Solution.eSolutionItemToSave.ReportConfiguration);
        }

        private void DefaultTemplatePickerCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DefaultTemplatePickerCbx.SelectedItem != null)
            {
                Ginger.Reports.GingerExecutionReport.ExtensionMethods.SetTemplateAsDefault((HTMLReportConfiguration)DefaultTemplatePickerCbx.SelectedItem);
                SelectDefualtTemplate();
            }
        }
    }
}