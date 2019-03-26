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
using Ginger.Run.RunSetActions;
using Ginger.UserControls;
using GingerCore;
using GingerCoreNET.RunLib;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;
using System.Collections.Generic;


namespace Ginger.Run
{
    /// <summary>
    /// Interaction logic for RunsetOperationsPage.xaml
    /// </summary>
    public partial class RunsetOperationsPage : Page
    {
        RunSetConfig mRunSetConfig;

        public RunsetOperationsPage(RunSetConfig runSetConfig)
        {
            InitializeComponent();
            mRunSetConfig = runSetConfig;
            LoadActionsGrid();
        }
        private void LoadActionsGrid()
        {
            SetGridView();
            RunSetActionsGrid.AddSeparator();
            RunSetActionsGrid.AddToolbarTool("@AddHTMLReport_16x16.png", "Add Produce HTML Report Action", AddHTMLReport);
            RunSetActionsGrid.AddToolbarTool("@AddMail_16x16.png", "Add Send HTML Report Email Action", AddSendHTMLReportEmailAction);
            RunSetActionsGrid.AddToolbarTool("@AddMail2_16x16.png", "Add Send Free Text Email Action", AddSendFreeEmailAction);
            RunSetActionsGrid.AddToolbarTool("@AddScript_16x16.png", "Add Generate TestNGReport", AddGenerateTestNGReportAction);           
            RunSetActionsGrid.AddToolbarTool("@AddRunSetALMAction_16x16.png", "Add Publish to ALM Action", AddPublishtoALMAction);
            RunSetActionsGrid.AddToolbarTool("@AddScript2_16x16.png", "Add Script Action", AddScriptAction);
            RunSetActionsGrid.AddToolbarTool("@AddScript_16x16.png", "Add Generate TestNGReport", AddGenerateTestNGReportAction);
            RunSetActionsGrid.AddToolbarTool("@AddDefectsToALM_16x16.png", "Automated ALM Defect’s Opening", AddAutomatedALMDefectsOperation);
            RunSetActionsGrid.AddToolbarTool("@AddSMS_16x16.png", "Add Send SMS Action", AddSendSMS);
            RunSetActionsGrid.AddSeparator();
            RunSetActionsGrid.AddToolbarTool("@Run_16x16.png", "Run Selected", RunSelected);
            RunSetActionsGrid.AddToolbarTool("@RunAll_16x16.png", "Run All", RunAll);
            RunSetActionsGrid.AddSeparator();
            SetContentAndEventsListeners();
        }

        private void SetContentAndEventsListeners()
        {
            RunSetActionsGrid.DataSourceList = mRunSetConfig.RunSetActions;
            RunSetActionsGrid.RowChangedEvent += RunSetActionsGrid_RowChangedEvent;

            RunSetActionEditFrame.Content = null;
            if (RunSetActionsGrid.CurrentItem != null)
            {
                RunSetActionEditPage RSAEP = new RunSetActionEditPage((RunSetActionBase)RunSetActionsGrid.CurrentItem);
                RunSetActionEditFrame.Content = RSAEP;
            }           
        }

        public void Init(RunSetConfig runSetConfig)
        {
            mRunSetConfig = runSetConfig;
            SetContentAndEventsListeners();
        }

        private void RunSetActionsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            RunSetActionEditFrame.Content = null;
            if (RunSetActionsGrid.CurrentItem != null)
            {
                RunSetActionEditPage RSAEP = new RunSetActionEditPage((RunSetActionBase)RunSetActionsGrid.CurrentItem);
                RunSetActionEditFrame.Content = RSAEP;
            }
        }

        private void SetGridView()
        {
            RunSetActionsGrid.ShowHeader = Visibility.Collapsed;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;

            viewCols.Add(new GridColView() { Field = RunSetActionBase.Fields.Active, WidthWeight = 50, StyleType = GridColView.eGridColStyleType.CheckBox });
            viewCols.Add(new GridColView() { Field = RunSetActionBase.Fields.Name, WidthWeight = 150 });
            viewCols.Add(new GridColView() { Field = RunSetActionBase.Fields.Type, Header = "Type", WidthWeight = 150, BindingMode = System.Windows.Data.BindingMode.OneWay, ReadOnly=true });
            List<GingerCore.General.ComboEnumItem> runAtOptionList = GingerCore.General.GetEnumValuesForCombo(typeof(RunSetActionBase.eRunAt));
            viewCols.Add(new GridColView() { Field = RunSetActionBase.Fields.RunAt, Header = "Run At", WidthWeight = 100, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = runAtOptionList });
            List<GingerCore.General.ComboEnumItem> conditionOptionList = GingerCore.General.GetEnumValuesForCombo(typeof(RunSetActionBase.eRunSetActionCondition));
            viewCols.Add(new GridColView() { Field = RunSetActionBase.Fields.Condition, WidthWeight = 100, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = conditionOptionList });
            viewCols.Add(new GridColView() { Field = RunSetActionBase.Fields.Status, WidthWeight = 80, BindingMode = System.Windows.Data.BindingMode.OneWay, ReadOnly = true });
            viewCols.Add(new GridColView() { Field = RunSetActionBase.Fields.Errors, WidthWeight = 80, BindingMode = System.Windows.Data.BindingMode.OneWay, ReadOnly = true });
            viewCols.Add(new GridColView() { Field = RunSetActionBase.Fields.ElapsedSecs, Header = "Execution Duration (Seconds)", WidthWeight = 50, BindingMode = System.Windows.Data.BindingMode.OneWay, ReadOnly = true });
            
            RunSetActionsGrid.SetAllColumnsDefaultView(view);
            RunSetActionsGrid.InitViewItems();
        }

        private void AddScriptAction(object sender, RoutedEventArgs e)
        {
            RunSetActionScript RSAS = new RunSetActionScript();
            RSAS.Name = "Execute Script";
            RSAS.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(RSAS);
            RunSetActionsGrid.Grid.SelectedItem = RSAS;
        }
        private void RunSelected(object sender, RoutedEventArgs e)
        {
            ((RunSetActionBase)RunSetActionsGrid.CurrentItem).ExecuteWithRunPageBFES();
        }
        private void RunAll(object sender, RoutedEventArgs e)
        {
            foreach (RunSetActionBase a in mRunSetConfig.RunSetActions)
            {
                mRunSetConfig.RunSetActions.CurrentItem = a;
                a.ExecuteWithRunPageBFES();
            }
        }

        private void AddSendSMS(object sender, RoutedEventArgs e)
        {
            RunSetActionSendSMS RSASS = new RunSetActionSendSMS();
            RSASS.Name = "Send SMS";
            RSASS.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(RSASS);
            RunSetActionsGrid.Grid.SelectedItem = RSASS;
        }

        private void AddSaveResults(object sender, RoutedEventArgs e)
        {
            RunSetActionSaveResults RSASR = new RunSetActionSaveResults();
            RSASR.Name = "Save Results";
            RSASR.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(RSASR);
            RunSetActionsGrid.Grid.SelectedItem = RSASR;
        }
        private void AddPublishtoALMAction(object sender, RoutedEventArgs e)
        {
            RunSetActionPublishToQC RSAPTAC = new RunSetActionPublishToQC();
            RSAPTAC.Name = "Publish to ALM";
            RSAPTAC.RunAt = RunSetActionBase.eRunAt.DuringExecution;
            mRunSetConfig.RunSetActions.Add(RSAPTAC);
            RunSetActionsGrid.Grid.SelectedItem = RSAPTAC;
        }

        private void AddSendHTMLReportEmailAction(object sender, RoutedEventArgs e)
        {
            RunSetActionHTMLReportSendEmail RSASR = new RunSetActionHTMLReportSendEmail();
            RSASR.Name = "Send HTML Report Email";
            RSASR.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(RSASR);
            RunSetActionsGrid.Grid.SelectedItem = RSASR;
        }

        private void AddSendFreeEmailAction(object sender, RoutedEventArgs e)
        {
            RunSetActionSendFreeEmail RSAFTE = new RunSetActionSendFreeEmail();
            RSAFTE.Name = "Send Free Text Email";
            mRunSetConfig.RunSetActions.Add(RSAFTE);
            RunSetActionsGrid.Grid.SelectedItem = RSAFTE;
        }

        private void AddSendEmailAction(object sender, RoutedEventArgs e)
        {
            RunSetActionSendEmail RSASR = new RunSetActionSendEmail();
            RSASR.Name = "Send Email";
            RSASR.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(RSASR);
            RunSetActionsGrid.Grid.SelectedItem = RSASR;
        }
        private void AddGenerateTestNGReportAction(object sender, RoutedEventArgs e)
        {
            RunSetActionGenerateTestNGReport TNGRPT = new RunSetActionGenerateTestNGReport();
            TNGRPT.Name = "Generate TestNG Report";
            TNGRPT.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(TNGRPT);
            RunSetActionsGrid.Grid.SelectedItem = TNGRPT;
        }
        private void AddHTMLReport(object sender, RoutedEventArgs e)
        {
            if (! WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().ExecutionLoggerConfigurationIsEnabled)
            {
                Reporter.ToUser(eUserMsgKey.ExecutionsResultsProdIsNotOn);
                return;
            }

            RunSetActionHTMLReport RSAHR = new RunSetActionHTMLReport();
            RSAHR.Name = "Produce HTML Report";
            RSAHR.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(RSAHR);
            RunSetActionsGrid.Grid.SelectedItem = RSAHR;
        }

        private void AddAutomatedALMDefectsOperation(object sender, RoutedEventArgs e)
        {
            if (! WorkSpace.Instance.Solution.UseRest && WorkSpace.Instance.Solution.AlmType != GingerCoreNET.ALMLib.ALMIntegration.eALMType.Jira)
            {
                Reporter.ToUser(eUserMsgKey.ALMDefectsUserInOtaAPI);
                return;
            }
            ObservableList<ALMDefectProfile> ALMDefectProfiles = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ALMDefectProfile>();
            if ((ALMDefectProfiles == null) || (ALMDefectProfiles.Count < 1))
            {
                Reporter.ToUser(eUserMsgKey.NoDefectProfileCreated);
                return;
            }

            RunSetActionAutomatedALMDefects RSAAAD = new RunSetActionAutomatedALMDefects();
            RSAAAD.Name = "Automated ALM Defect’s Opening";
            RSAAAD.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(RSAAAD);
            RunSetActionsGrid.Grid.SelectedItem = RSAAAD;
        }
    }
}
