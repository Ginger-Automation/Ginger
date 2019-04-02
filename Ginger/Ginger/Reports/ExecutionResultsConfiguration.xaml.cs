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

        private static ExecutionResultsConfiguration mInstance;

        public static ExecutionResultsConfiguration Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new ExecutionResultsConfiguration();

                return mInstance;
            }
        }

        private ExecutionResultsConfiguration()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            _selectedExecutionLoggerConfiguration =  WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            _selectedExecutionLoggerConfiguration.StartDirtyTracking();
            SetControls();
        }

        private void SetControls()
        {
            if (_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {
                ExecutionResultFolderPnl.IsEnabled = true;
            }
            else
            {
                ExecutionResultFolderPnl.IsEnabled = false;
            }

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

            FolderTextBox.Text = _selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder == null ? string.Empty : _selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder;
            SizeTextBox.Text = _selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationMaximalFolderSize.ToString();
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string s = General.OpenSelectFolderDialog("Save Results to Folder");
            if (s != null)
            {
                FolderTextBox.Text = s;
            }
        }

        private void FolderTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder = FolderTextBox.Text.ToString();
            _selectedExecutionLoggerConfiguration.OnPropertyChanged(nameof(ExecutionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder));
        }

        private void executionResultOnRadioBtnsPnl_Checked(object sender, RoutedEventArgs e)
        {
            _selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled = true;
            _selectedExecutionLoggerConfiguration.OnPropertyChanged(nameof(ExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled));
            if (ExecutionResultFolderPnl != null)
            {
                ExecutionResultFolderPnl.IsEnabled = true;
            }
        }

        private void executionResultOffRadioBtnsPnl_Checked(object sender, RoutedEventArgs e)
        {
            _selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled = false;
            _selectedExecutionLoggerConfiguration.OnPropertyChanged(nameof(ExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled));
            if (ExecutionResultFolderPnl != null)
            {
                ExecutionResultFolderPnl.IsEnabled = false;
            }
        }

        private void SizeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                _selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationMaximalFolderSize = (long)Convert.ToInt32(SizeTextBox.Text.ToString());
            }
            catch
            {
                _selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationMaximalFolderSize = 0;
            }
            _selectedExecutionLoggerConfiguration.OnPropertyChanged(nameof(ExecutionLoggerConfiguration.ExecutionLoggerConfigurationMaximalFolderSize));
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
                if (Convert.ToInt16(SizeTextBox.Text.ToString()) < 50)
                {
                    Reporter.ToUser(eUserMsgKey.FolderSizeTooSmall);
                    return;
                }
            }
            catch
            {
                return;
            }

             WorkSpace.Instance.Solution.SaveSolution(true, SolutionGeneral.Solution.eSolutionItemToSave.LoggerConfiguration);

            // validate the paths of inserted folders
            Ginger.Run.ExecutionLogger.GetLoggerDirectory( WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().ExecutionLoggerConfigurationExecResultsFolder);
            Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetReportDirectory( WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().ExecutionLoggerConfigurationHTMLReportsFolder);

            //App.AutomateTabGingerRunner.ExecutionLogger.Configuration =  WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
        }
    }
}
