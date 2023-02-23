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
using Ginger.Agents;
using GingerCore.Environments;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Run
{
    public partial class GingerRunnerConfigurationsPage : Page
    {
        public enum ePageViewMode { AutomatePage, RunsetPage }
        ePageViewMode mPageViewMode;
        GingerExecutionEngine mRunner;
        Context mContext;
        GenericWindow genWin = null;

        public GingerRunnerConfigurationsPage(GingerExecutionEngine runner, ePageViewMode pageViewMode, Context context)
        {
            InitializeComponent();

            mRunner = runner;
            mPageViewMode = pageViewMode;
            mContext = context;

            mRunner.GingerRunner.PauseDirtyTracking();

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xNameTextBox, TextBox.TextProperty, mRunner.GingerRunner, nameof(GingerRunner.Name));
            xNameTextBox.AddValidationRule(new RunnerNameValidationRule());
            xShowIDUC.Init(mRunner.GingerRunner);

            mRunner.UpdateApplicationAgents();
            if (mPageViewMode == ePageViewMode.AutomatePage)
            {
                xAppAgentsMappingFrame.Content = new ApplicationAgentsMapPage(mRunner, mContext);
            }
            else
            {
                xAppAgentsMappingFrame.Content = new ApplicationAgentsMapPage(mRunner, mContext, false);
            }

            List<int> waitOptions = new List<int>() { 0, 1, 2, 3, 4, 5 };
            xAutoWaitComboBox.ItemsSource = waitOptions;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAutoWaitComboBox, ComboBox.SelectedValueProperty, mRunner.GingerRunner, nameof(GingerRunner.AutoWait));

            GingerCore.General.FillComboFromEnumObj(xRunOptionComboBox, mRunner.GingerRunner.RunOption);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xRunOptionComboBox, ComboBox.SelectedValueProperty, mRunner.GingerRunner, nameof(GingerRunner.RunOption));

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSimulationMode, CheckBox.IsCheckedProperty, mRunner.GingerRunner, nameof(GingerRunner.RunInSimulationMode));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xVisualTestingMode, CheckBox.IsCheckedProperty, mRunner.GingerRunner, nameof(GingerRunner.RunInVisualTestingMode));

            SetEnvironments();
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xUseSpecificEnvChkbox, CheckBox.IsCheckedProperty, mRunner.GingerRunner, nameof(GingerRunner.UseSpecificEnvironment));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSpecificEnvComboBox, ComboBox.SelectedItemProperty, mRunner.GingerRunner, nameof(GingerRunner.ProjEnvironment));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSpecificEnvComboBox, ComboBox.SelectedValueProperty, mRunner.GingerRunner, nameof(GingerRunner.SpecificEnvironmentName));


            xExecutionTags.Init(mRunner.GingerRunner.FilterExecutionTags);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xExecutionTagsChkbox, CheckBox.IsCheckedProperty, mRunner.GingerRunner, nameof(GingerRunner.FilterExecutionByTags));

            if (mPageViewMode == ePageViewMode.AutomatePage)
            {
                xNamePnl.Visibility = Visibility.Collapsed;
                xRunOptionPnl.Visibility = Visibility.Collapsed;
                xUseSpecificEnvChkbox.IsEnabled = false;
            }
            mRunner.GingerRunner.ResumeDirtyTracking();
        }

        public void ShowAsWindow()
        {
            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, Ginger.eWindowShowStyle.Dialog, this.Title, this);
        }

        private void useSpecificEnvChkbox_Checked(object sender, RoutedEventArgs e)
        {
            if (xUseSpecificEnvChkbox.IsChecked == true)
            {
                xUseSpecificEnvChkbox.Content = "Use Specific Environment:";
                xSpecificEnvComboBox.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                xUseSpecificEnvChkbox.Content = "Use Specific Environment";
                xSpecificEnvComboBox.Visibility = System.Windows.Visibility.Collapsed;
                xSpecificEnvComboBox.SelectedValue = null;
            }
        }

        private void SetEnvironments()
        {
            xSpecificEnvComboBox.ItemsSource = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>();
            xSpecificEnvComboBox.DisplayMemberPath = nameof(ProjEnvironment.Name);
            xSpecificEnvComboBox.SelectedValuePath = nameof(ProjEnvironment.Name);

            //ProjEnvironment selectedEnv = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Where(x => x.Name == mRunner.SpecificEnvironmentName).FirstOrDefault();
            //if (selectedEnv != null)
            //{
            //    xSpecificEnvComboBox.SelectedItem = selectedEnv;
            //}
        }

        //private void SpecificEnvComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (xSpecificEnvComboBox.SelectedItem != null)
        //    {
        //        mRunner.UseSpecificEnvironment = true;
        //        mRunner.ProjEnvironment = (ProjEnvironment)xSpecificEnvComboBox.SelectedItem;
        //        mRunner.SpecificEnvironmentName = ((ProjEnvironment)xSpecificEnvComboBox.SelectedItem).Name;
        //    }
        //    else
        //    {
        //        mRunner.UseSpecificEnvironment = false;
        //        mRunner.ProjEnvironment = null;
        //        mRunner.SpecificEnvironmentName = string.Empty;
        //    }
        //}

        private void ExecutionTagsChkbox_Checked(object sender, RoutedEventArgs e)
        {
            if (xExecutionTagsChkbox.IsChecked == true)
            {
                xExecutionTagsChkbox.Content = "Filter Execution by Tags:";
                xExecutionTags.Visibility = Visibility.Visible;
            }
        }

        private void ExecutionTagsChkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            xExecutionTagsChkbox.Content = "Filter Execution by Tags";
            xExecutionTags.Visibility = Visibility.Collapsed;
        }
    }
}
