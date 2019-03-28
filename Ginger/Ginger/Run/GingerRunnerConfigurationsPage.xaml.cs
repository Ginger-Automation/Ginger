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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Platforms;
using GingerCore.Environments;
using amdocs.ginger.GingerCoreNET;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace Ginger.Run
{
    public partial class GingerRunnerConfigurationsPage : Page
    {
        public enum ePageContext { AutomateTab,RunTab}
        ePageContext mPageContext;
        GingerRunner mGingerRunner;

        GenericWindow genWin = null;

        public GingerRunnerConfigurationsPage(GingerRunner GR, ePageContext pageContext)
        {
            InitializeComponent();

            mGingerRunner = GR;
            mPageContext = pageContext;

            SetGridView();
            GR.UpdateApplicationAgents();

            ObservableList<ApplicationAgent> ApplicationAgents = new ObservableList<ApplicationAgent>();

            foreach (ApplicationAgent Apag in GR.ApplicationAgents)
            {
                if (GR.SolutionApplications.Where(x => x.AppName == Apag.AppName && x.Platform == ePlatformType.NA).FirstOrDefault() == null)
                {
                    ApplicationAgents.Add(Apag);
                }
            }

            grdApplicationsAgentsMapping.DataSourceList = ApplicationAgents;
            grdApplicationsAgentsMapping.btnEdit.AddHandler(Button.ClickEvent, new RoutedEventHandler(EditMapping));
            grdApplicationsAgentsMapping.RowDoubleClick += grdApplicationsAgentsMapping_RowDoubleClick;

            ExecutionTags.Init(mGingerRunner.FilterExecutionTags);
            if (mPageContext == ePageContext.RunTab)
            {
                GingerCore.General.FillComboFromEnumObj(RunOptionComboBox, mGingerRunner.RunOption);
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RunOptionComboBox, ComboBox.SelectedValueProperty, mGingerRunner, GingerRunner.Fields.RunOption);

                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(GingerNameTextBox, TextBox.TextProperty, mGingerRunner, GingerRunner.Fields.Name);

                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(useSpecificEnvChkbox, CheckBox.IsCheckedProperty, mGingerRunner, GingerRunner.Fields.UseSpecificEnvironment);
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ExecutionTagsChkbox, CheckBox.IsCheckedProperty, mGingerRunner, GingerRunner.Fields.FilterExecutionByTags);
                //GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(specificEnvComboBox, ComboBox.DisplayMemberPathProperty, mGingerRunner, GingerRunner.Fields.SpecificEnvironmentName);
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SimulationMode, CheckBox.IsCheckedProperty, mGingerRunner, GingerRunner.Fields.RunInSimulationMode);
            }
            else
            {
                GingerDetailsPanel.Visibility = System.Windows.Visibility.Collapsed;
                this.Title = "Agents Configurations";
            }
        }

        private void SetGridView()
        {           
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = ApplicationAgent.Fields.AppName, Header = "Target Application Name", WidthWeight = 50, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = ApplicationAgent.Fields.AgentName, Header = "Agent Name", WidthWeight = 50, BindingMode = BindingMode.OneWay, ReadOnly = true });

            grdApplicationsAgentsMapping.SetAllColumnsDefaultView(view);
            grdApplicationsAgentsMapping.InitViewItems();
        }

        public void ShowAsWindow()
        {
            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, Ginger.eWindowShowStyle.Dialog, this.Title, this);
        }

        private void RunOptionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void EditMapping()
        {
            if (grdApplicationsAgentsMapping.CurrentItem != null)
            {
                ApplicationAgentSelectionPage w = new ApplicationAgentSelectionPage(mGingerRunner, (ApplicationAgent)grdApplicationsAgentsMapping.CurrentItem);
                w.ShowAsWindow();
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectItem);
            }
        }

        private void EditMapping(object sender, RoutedEventArgs e)
        {
            EditMapping();
        }

        private void grdApplicationsAgentsMapping_RowDoubleClick(object sender, EventArgs e)
        {
            EditMapping();
        }

        private void useSpecificEnvChkbox_Checked(object sender, RoutedEventArgs e)
        {
            if (useSpecificEnvChkbox.IsChecked == true)
            {
                useSpecificEnvChkbox.Content = "Use Specific Environment:";
                specificEnvSelectionPnl.Visibility = System.Windows.Visibility.Visible;
                SetEnvironments();                
            }
            else
            {
                useSpecificEnvChkbox.Content = "Use Specific Environment";
                specificEnvSelectionPnl.Visibility = System.Windows.Visibility.Collapsed;
                specificEnvComboBox.SelectedValue = null;
            }
        }

        private void SetEnvironments()
        {
            specificEnvComboBox.ItemsSource = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>();
            specificEnvComboBox.DisplayMemberPath = nameof(ProjEnvironment.Name);
            specificEnvComboBox.SelectedValuePath = nameof(ProjEnvironment.Guid);

            ProjEnvironment selectedEnv = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Where(x => x.Name == mGingerRunner.SpecificEnvironmentName).FirstOrDefault();
            if (selectedEnv != null)
            {
                specificEnvComboBox.SelectedItem = selectedEnv;
            }
        }

        private void ExecutionTagsChkbox_Checked(object sender, RoutedEventArgs e)
        {
            if (ExecutionTagsChkbox.IsChecked == true)
            {
                ExecutionTags.Visibility = Visibility.Visible;
            }
            
        }

        private void ExecutionTagsChkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            ExecutionTags.Visibility = Visibility.Collapsed;
        }

        private void SpecificEnvComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (specificEnvComboBox.SelectedItem != null)
            {
                mGingerRunner.ProjEnvironment = (ProjEnvironment)specificEnvComboBox.SelectedItem;
                mGingerRunner.SpecificEnvironmentName = ((ProjEnvironment)specificEnvComboBox.SelectedItem).Name;
            }
            else
            {
                mGingerRunner.ProjEnvironment = null;
                mGingerRunner.SpecificEnvironmentName = string.Empty;
            }
        }
    }
}
