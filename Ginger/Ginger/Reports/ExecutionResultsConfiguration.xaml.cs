#region License
/*
Copyright © 2014-2021 European Support Limited

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

namespace Ginger.Reports
{
    /// <summary>
    /// Interaction logic for ExecutionResultsConfiguration.xaml
    /// </summary>
    public partial class ExecutionResultsConfiguration : Page
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
            _selectedExecutionLoggerConfiguration.StartDirtyTracking();
            SetControls();
            isControlsSet = true;
        }

        private void SetControls()
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(FolderTextBox, TextBox.TextProperty, _selectedExecutionLoggerConfiguration, nameof(ExecutionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xFolderMaximumSizeTextBox, TextBox.TextProperty, _selectedExecutionLoggerConfiguration, nameof(ExecutionLoggerConfiguration.ExecutionLoggerConfigurationMaximalFolderSize), bindingConvertor: new GingerCore.GeneralLib.LongStringConverter());

            xPublishLogToCentralDBRadioButton.Init(typeof(ExecutionLoggerConfiguration.ePublishToCentralDB),
                xPublishLogToCentralDBRadioBtnPanel, _selectedExecutionLoggerConfiguration,
                nameof(ExecutionLoggerConfiguration.PublishLogToCentralDB), PublishLogToCentralDBRadioButton_CheckedHandler);
            //, new RoutedEventHandler(PublishLogToCentralDBRadioButton_Click)
            if (_selectedExecutionLoggerConfiguration.PublishLogToCentralDB == ExecutionLoggerConfiguration.ePublishToCentralDB.No)
            {
                SetExecutionLoggerRadioButtonToOff();
            }

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

                if (xCentralExecutionLoggerGrid != null && _selectedExecutionLoggerConfiguration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                {
                    xCentralExecutionLoggerGrid.Visibility = Visibility.Visible;
                }
            }
        }

        private void executionResultOffRadioBtnsPnl_Checked(object sender, RoutedEventArgs e)
        {
            _selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled = false;
            _selectedExecutionLoggerConfiguration.OnPropertyChanged(nameof(ExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled));
            _selectedExecutionLoggerConfiguration.PublishLogToCentralDB = ExecutionLoggerConfiguration.ePublishToCentralDB.No;
            _selectedExecutionLoggerConfiguration.OnPropertyChanged(nameof(ExecutionLoggerConfiguration.PublishLogToCentralDB));
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

            WorkSpace.Instance.Solution.SaveSolution(true, SolutionGeneral.Solution.eSolutionItemToSave.LoggerConfiguration);

            // validate the paths of inserted folders
            Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetReportDirectory(WorkSpace.Instance.Solution.LoggerConfigurations.ExecutionLoggerConfigurationHTMLReportsFolder);
        }
        private void TextFileRadioBtnsPnl_Checked(object sender, RoutedEventArgs e)
        {
            if (_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {
                xCentralExecutionLoggerGrid.Visibility = Visibility.Collapsed;
                xFolderMaximumSizeLabel.Visibility = Visibility.Visible;
                xFolderMaximumSizeTextBox.Visibility = Visibility.Visible;
                _selectedExecutionLoggerConfiguration.SelectedDataRepositoryMethod = ExecutionLoggerConfiguration.DataRepositoryMethod.TextFile;

                _selectedExecutionLoggerConfiguration.OnPropertyChanged(nameof(ExecutionLoggerConfiguration.SelectedDataRepositoryMethod));
                _selectedExecutionLoggerConfiguration.PublishLogToCentralDB = ExecutionLoggerConfiguration.ePublishToCentralDB.No;
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
                xCentralExecutionLoggerGrid.Visibility = Visibility.Visible;
                xFolderMaximumSizeLabel.Visibility = Visibility.Collapsed;
                xFolderMaximumSizeTextBox.Visibility = Visibility.Collapsed;
                _selectedExecutionLoggerConfiguration.SelectedDataRepositoryMethod = ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB;
                _selectedExecutionLoggerConfiguration.OnPropertyChanged(nameof(ExecutionLoggerConfiguration.SelectedDataRepositoryMethod));
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
                xEndPointURLLabel.Visibility = Visibility.Visible;
                xEndPointURLTextBox.Visibility = Visibility.Visible;
                xDeleteLocalData.Visibility = Visibility.Visible;
                xDeleteLocalDataOnPublishPanel.Visibility = Visibility.Visible;
                xPublishingPhase.Visibility = Visibility.Visible;
                xPublishingPhasePanel.Visibility = Visibility.Visible;
            }
            else
            {
                xEndPointURLLabel.Visibility = Visibility.Collapsed;
                xEndPointURLTextBox.Visibility = Visibility.Collapsed;
                xDeleteLocalData.Visibility = Visibility.Collapsed;
                xDeleteLocalDataOnPublishPanel.Visibility = Visibility.Collapsed;
                xPublishingPhase.Visibility = Visibility.Collapsed;
                xPublishingPhasePanel.Visibility = Visibility.Collapsed;
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
                            if (control.Name == "No")
                            {
                                control.IsChecked = true;
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
