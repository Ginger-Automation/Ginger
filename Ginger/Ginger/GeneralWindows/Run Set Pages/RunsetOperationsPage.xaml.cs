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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Run.RunSetActions;
using Amdocs.Ginger.Repository;
using Ginger.Run.RunSetActions;
using Ginger.UserControls;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

            RunSetActionsGrid.AddToolbarTool("@AddHTMLReport_16x16.png", "Add Produce HTML Report Operation", AddHTMLReport);
            RunSetActionsGrid.AddToolbarTool("@AddMail_16x16.png", "Add Send HTML Report Email Operation", AddSendHTMLReportEmailAction);
            RunSetActionsGrid.AddToolbarTool("@AddFile_16x16.png", "Add Produce JSON Summary Report Operation", AddJSONSummary);
            RunSetActionsGrid.AddToolbarTool("@AddScript_16x16.png", "Add Produce TestNG Summary Report Operation", AddGenerateTestNGReportAction);
            RunSetActionsGrid.AddSeparator();
            RunSetActionsGrid.AddToolbarTool("@AddMail2_16x16.png", "Add Send Text Email Operation", AddSendFreeEmailAction);
            RunSetActionsGrid.AddToolbarTool("@AddSMS_16x16.png", "Add Send SMS Operation", AddSendSMS);
            RunSetActionsGrid.AddSeparator();
            RunSetActionsGrid.AddToolbarTool("@AddRunSetALMAction_16x16.png", "Add Publish Execution Results to ALM Operation", AddPublishtoALMAction);           
            RunSetActionsGrid.AddToolbarTool("@AddDefectsToALM_16x16.png", "Add Open ALM Defects Operation", AddAutomatedALMDefectsOperation);
            RunSetActionsGrid.AddSeparator();
            RunSetActionsGrid.AddToolbarTool("@AddScript2_16x16.png", "Add Run Script Operation", AddScriptAction);


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
            List<ComboEnumItem> runAtOptionList = GingerCore.General.GetEnumValuesForCombo(typeof(RunSetActionBase.eRunAt));
            viewCols.Add(new GridColView() { Field = RunSetActionBase.Fields.RunAt, Header = "Run At", WidthWeight = 100, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = runAtOptionList });
            List<ComboEnumItem> conditionOptionList = GingerCore.General.GetEnumValuesForCombo(typeof(RunSetActionBase.eRunSetActionCondition));
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
            RSAS.Name = RSAS.Type;
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
            RSASS.Name = RSASS.Type;
            RSASS.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(RSASS);
            RunSetActionsGrid.Grid.SelectedItem = RSASS;
        }

        private void AddSaveResults(object sender, RoutedEventArgs e)
        {
            RunSetActionSaveResults RSASR = new RunSetActionSaveResults();
            RSASR.Name = RSASR.Type;
            RSASR.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(RSASR);
            RunSetActionsGrid.Grid.SelectedItem = RSASR;
        }
        private void AddPublishtoALMAction(object sender, RoutedEventArgs e)
        {
            RunSetActionPublishToQC RSAPTAC = new RunSetActionPublishToQC();
            RSAPTAC.Name = RSAPTAC.Type;
            RSAPTAC.RunAt = RunSetActionBase.eRunAt.DuringExecution;
            mRunSetConfig.RunSetActions.Add(RSAPTAC);
            RunSetActionsGrid.Grid.SelectedItem = RSAPTAC;
        }

        private void AddSendHTMLReportEmailAction(object sender, RoutedEventArgs e)
        {
            RunSetActionHTMLReportSendEmail RSASR = new RunSetActionHTMLReportSendEmail();
            RSASR.Name = RSASR.Type;
            RSASR.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(RSASR);
            RunSetActionsGrid.Grid.SelectedItem = RSASR;
        }

        private void AddSendFreeEmailAction(object sender, RoutedEventArgs e)
        {
            RunSetActionSendFreeEmail RSAFTE = new RunSetActionSendFreeEmail();
            RSAFTE.Name = RSAFTE.Type;
            mRunSetConfig.RunSetActions.Add(RSAFTE);
            RunSetActionsGrid.Grid.SelectedItem = RSAFTE;
        }

        private void AddSendEmailAction(object sender, RoutedEventArgs e)
        {
            RunSetActionSendEmail RSASR = new RunSetActionSendEmail();
            RSASR.Name = RSASR.Type;
            RSASR.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(RSASR);
            RunSetActionsGrid.Grid.SelectedItem = RSASR;
        }
        private void AddGenerateTestNGReportAction(object sender, RoutedEventArgs e)
        {
            RunSetActionGenerateTestNGReport TNGRPT = new RunSetActionGenerateTestNGReport();
            TNGRPT.Name = TNGRPT.Type;
            TNGRPT.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(TNGRPT);
            RunSetActionsGrid.Grid.SelectedItem = TNGRPT;
        }
        private void AddHTMLReport(object sender, RoutedEventArgs e)
        {
            if (! WorkSpace.Instance.Solution.LoggerConfigurations.ExecutionLoggerConfigurationIsEnabled)
            {
                Reporter.ToUser(eUserMsgKey.ExecutionsResultsProdIsNotOn);
                return;
            }

            RunSetActionHTMLReport RSAHR = new RunSetActionHTMLReport();
            RSAHR.Name = RSAHR.Type;
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
            RSAAAD.Name = RSAAAD.Type;
            RSAAAD.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(RSAAAD);
            RunSetActionsGrid.Grid.SelectedItem = RSAAAD;
        }

        private void AddJSONSummary(object sender, RoutedEventArgs e)
        {
            RunSetActionJSONSummary runSetActionJSONSummary = new RunSetActionJSONSummary();
            runSetActionJSONSummary.Name = runSetActionJSONSummary.Type;
            runSetActionJSONSummary.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(runSetActionJSONSummary);
            RunSetActionsGrid.Grid.SelectedItem = runSetActionJSONSummary;
        }


    }
}
