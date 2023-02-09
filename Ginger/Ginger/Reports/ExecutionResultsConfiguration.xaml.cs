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
using Ginger.UserControls;
using amdocs.ginger.GingerCoreNET;
using System.Windows.Input;
using System.Text.RegularExpressions;
using Ginger.ValidationRules;
using Ginger.UserControlsLib;

namespace Ginger.Reports
{
    /// <summary>
    /// Interaction logic for ExecutionResultsConfiguration.xaml
    /// </summary>
    public partial class ExecutionResultsConfiguration : GingerUIPage
    {
        GenericWindow _pageGenericWin = null;
        ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration = new ExecutionLoggerConfiguration();
        bool isControlsSet = false;
        //private static ExecutionResultsConfiguration mInstance;

        //public static ExecutionResultsConfiguration Instance
        //{
        //    get
        //    {
        //        if (mInstance == null)
        //            mInstance = new ExecutionResultsConfiguration();

        //        return mInstance;
        //    }
        //}

        public ExecutionResultsConfiguration()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            _selectedExecutionLoggerConfiguration = WorkSpace.Instance.Solution.LoggerConfigurations;
            //Notify User about Change in sealights configurations settings
            if (!string.IsNullOrEmpty(WorkSpace.Instance.Solution.LoggerConfigurations.SealightsURL))
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Sealights Configurations section moved to Configurations -> External Integrations -> Sealights Configurations. \n Please save the configuration details.");
            }
            CurrentItemToSave = WorkSpace.Instance.Solution;
            SetControls();
            _selectedExecutionLoggerConfiguration.StartDirtyTracking();
            isControlsSet = true;
        }

        private void SetControls()
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(FolderTextBox, TextBox.TextProperty, _selectedExecutionLoggerConfiguration, nameof(ExecutionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xFolderMaximumSizeTextBox, TextBox.TextProperty, _selectedExecutionLoggerConfiguration, nameof(ExecutionLoggerConfiguration.ExecutionLoggerConfigurationMaximalFolderSize), bindingConvertor: new GingerCore.GeneralLib.LongStringConverter());


            xPublishingPhaseRadioButton.Init(typeof(ExecutionLoggerConfiguration.eDataPublishingPhase),
                xPublishingPhasePanel, _selectedExecutionLoggerConfiguration,
                nameof(ExecutionLoggerConfiguration.DataPublishingPhase));

            xDeleteLocalDataRadioButton.Init(typeof(ExecutionLoggerConfiguration.eDeleteLocalDataOnPublish),
                xDeleteLocalDataOnPublishPanel, _selectedExecutionLoggerConfiguration,
                nameof(ExecutionLoggerConfiguration.DeleteLocalDataOnPublish));

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xEndPointURLTextBox, TextBox.TextProperty, _selectedExecutionLoggerConfiguration,
                nameof(ExecutionLoggerConfiguration.CentralLoggerEndPointUrl));

            if (_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {
                executionResultOnRadioBtnsPnl.IsChecked = true;
                executionResultOffRadioBtnsPnl.IsChecked = false;
            }
            else
            {
                executionResultOnRadioBtnsPnl.IsChecked = false;
                executionResultOffRadioBtnsPnl.IsChecked = true;
            }
            if (_selectedExecutionLoggerConfiguration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.TextFile)
            {
                textFileRadioBtnsPnl.IsChecked = true;
            }
            else
            {
                liteDbRadioBtnsPnl.IsChecked = true;
            }

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xCentralExecutionLoggerExpander, Expander.VisibilityProperty, WorkSpace.Instance.UserProfile, nameof(WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures), bindingConvertor: new GingerCore.GeneralLib.BoolVisibilityConverter(), BindingMode: System.Windows.Data.BindingMode.OneWay);
            
            if (_selectedExecutionLoggerConfiguration.PublishLogToCentralDB == ExecutionLoggerConfiguration.ePublishToCentralDB.No)
            {
                SetExecutionLoggerRadioButtonToOff();
            }
            xPublishLogToCentralDBRadioButton.Init(typeof(ExecutionLoggerConfiguration.ePublishToCentralDB),
                xPublishLogToCentralDBRadioBtnPanel, _selectedExecutionLoggerConfiguration,
                nameof(ExecutionLoggerConfiguration.PublishLogToCentralDB), PublishLogToCentralDBRadioButton_CheckedHandler);

        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string s = General.OpenSelectFolderDialog("Save Results to Folder");
            if (s != null)
            {
                FolderTextBox.Text = s;
            }
        }

        private void executionResultOnRadioBtnsPnl_Checked(object sender, RoutedEventArgs e)
        {
            _selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled = true;
            _selectedExecutionLoggerConfiguration.OnPropertyChanged(nameof(ExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled));
            _selectedExecutionLoggerConfiguration.PublishLogToCentralDB = ExecutionLoggerConfiguration.ePublishToCentralDB.No;
            _selectedExecutionLoggerConfiguration.OnPropertyChanged(nameof(ExecutionLoggerConfiguration.PublishLogToCentralDB));
            SetExecutionLoggerRadioButtonToOff();
            if (xLoggerSettingsGrid != null)
            {
                xLoggerSettingsGrid.Visibility = Visibility.Visible;

                if (xCentralExecutionLoggerGrid != null &&
                    _selectedExecutionLoggerConfiguration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB &&
                    _selectedExecutionLoggerConfiguration.PublishLogToCentralDB == ExecutionLoggerConfiguration.ePublishToCentralDB.Yes)
                {
                    xCentralExecutionLoggerGrid.Visibility = Visibility.Visible;
                }
            }
        }

        private void executionResultOffRadioBtnsPnl_Checked(object sender, RoutedEventArgs e)
        {
            _selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled = false;
            _selectedExecutionLoggerConfiguration.PublishLogToCentralDB = ExecutionLoggerConfiguration.ePublishToCentralDB.No;
            SetExecutionLoggerRadioButtonToOff();
            if (xLoggerSettingsGrid != null)
            {
                xLoggerSettingsGrid.Visibility = Visibility.Collapsed;
                if (xCentralExecutionLoggerGrid != null)
                {
                    xCentralExecutionLoggerGrid.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void xSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTextBox.Text.Length > 100)
            {
                Reporter.ToUser(eUserMsgKey.FolderNamesAreTooLong);
                return;
            }

            try
            {
                if (Convert.ToInt16(xFolderMaximumSizeTextBox.Text.ToString()) < 50)
                {
                    Reporter.ToUser(eUserMsgKey.FolderSizeTooSmall);
                    return;
                }
                if (WorkSpace.Instance.Solution.LoggerConfigurations.PublishLogToCentralDB == ExecutionLoggerConfiguration.ePublishToCentralDB.Yes)
                {
                    if (string.IsNullOrEmpty(xEndPointURLTextBox.Text))
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Please provide endpoint URI");
                        return;
                    }
                }
            }
            catch
            {
                return;
            }

            WorkSpace.Instance.Solution.SolutionOperations.SaveSolution(true, SolutionGeneral.Solution.eSolutionItemToSave.LoggerConfiguration);

            // validate the paths of inserted folders
            Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetReportDirectory(WorkSpace.Instance.Solution.LoggerConfigurations.ExecutionLoggerConfigurationHTMLReportsFolder);
        }
        private void TextFileRadioBtnsPnl_Checked(object sender, RoutedEventArgs e)
        {
            if (_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {
                _selectedExecutionLoggerConfiguration.PublishLogToCentralDB = ExecutionLoggerConfiguration.ePublishToCentralDB.No;
                SetExecutionLoggerRadioButtonToOff();
                _selectedExecutionLoggerConfiguration.OnPropertyChanged(nameof(ExecutionLoggerConfiguration.SelectedDataRepositoryMethod));
                xCentralExecutionLoggerExpander.Visibility = Visibility.Collapsed;
                xFolderMaximumSizeRow.Height = new GridLength(30);
                _selectedExecutionLoggerConfiguration.SelectedDataRepositoryMethod = ExecutionLoggerConfiguration.DataRepositoryMethod.TextFile;
                if (isControlsSet)
                {
                    Reporter.ToUser(eUserMsgKey.ChangesRequireRestart);
                }
            }
        }
        private void LiteDbRadioBtnsPnl_Checked(object sender, RoutedEventArgs e)
        {
            if (_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {
                if (WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures)
                {
                    xCentralExecutionLoggerExpander.Visibility = Visibility.Visible;
                }
                xFolderMaximumSizeRow.Height = new GridLength(0);
                _selectedExecutionLoggerConfiguration.SelectedDataRepositoryMethod = ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB;
                if (isControlsSet)
                {
                    Reporter.ToUser(eUserMsgKey.ChangesRequireRestart);
                }
            }

        }

        private void PublishLogToCentralDBRadioButton_CheckedHandler(object sender, RoutedEventArgs e)
        {
            string value = ((RadioButton)sender).Tag?.ToString();

            ExecutionLoggerConfiguration.ePublishToCentralDB publishToCentralDB;

            Enum.TryParse(value, out publishToCentralDB);

            if (publishToCentralDB == ExecutionLoggerConfiguration.ePublishToCentralDB.Yes)
            {
                xCentralExecutionLoggerGrid.Visibility = Visibility.Visible;
            }
            else
            {
                xCentralExecutionLoggerGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void SetExecutionLoggerRadioButtonToOff()
        {
            if (_selectedExecutionLoggerConfiguration.PublishLogToCentralDB == ExecutionLoggerConfiguration.ePublishToCentralDB.No)
            {
                if (xPublishLogToCentralDBRadioBtnPanel != null && xPublishLogToCentralDBRadioBtnPanel.Children != null && xPublishLogToCentralDBRadioBtnPanel.Children.Count != 0)
                {
                    for (int i = 0; i < xPublishLogToCentralDBRadioBtnPanel.Children.Count; i++)
                    {
                        try
                        {
                            var control = xPublishLogToCentralDBRadioBtnPanel.Children[i] as RadioButton;
                            if (control == null)
                            {
                                continue;
                            }
                            if (control.Name == "No")
                            {
                                control.IsChecked = true;
                                break;
                            }
                            else
                            {
                                control.IsChecked = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                }
            }
        }
    }
}
