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
using Amdocs.Ginger.CoreNET.Run.RunSetActions;
using Amdocs.Ginger.Repository;
using Ginger.Run.RunSetActions;
using Ginger.UserControls;
using GingerCore.ALM;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
           
            Binding b = new Binding();
            b.Source = WorkSpace.Instance.UserProfile;
            b.Path = new PropertyPath(nameof(UserProfile.ShowEnterpriseFeatures));
            b.Mode = BindingMode.OneWay;
            b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            b.NotifyOnValidationError = true;
            b.Converter = new GingerCore.GeneralLib.BoolVisibilityConverter();

            RunSetActionsGrid.AddSeparator(b);
            RunSetActionsGrid.AddToolbarTool("@AddRunSetALMAction_16x16.png", "Add Publish Execution Results to ALM Operation", AddPublishtoALMAction, binding: b);
            RunSetActionsGrid.AddToolbarTool("@AddDefectsToALM_16x16.png", "Add Open ALM Defects Operation", AddAutomatedALMDefectsOperation, binding: b);
            
            RunSetActionsGrid.AddSeparator();
            RunSetActionsGrid.AddToolbarTool("@AddScript2_16x16.png", "Add Run Script Operation", AddScriptAction);
            RunSetActionsGrid.AddSeparator();
            RunSetActionsGrid.AddToolbarTool(Amdocs.Ginger.Common.Enums.eImageType.SignOut, "Add Send Execution JSON Data To External Source Operation", AddSendExecutionDataToExternalSourceAction);

            RunSetActionsGrid.AddSeparator();
            RunSetActionsGrid.AddToolbarTool("@Run_16x16.png", "Run Selected", RunSelected);
            RunSetActionsGrid.AddSeparator();
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

        RunSetActionEditPage runSetActionEditPage;
        private void RunSetActionsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            RunSetActionEditFrame.Content = null;
            if (RunSetActionsGrid.CurrentItem != null)
            {
                RunSetActionEditPage RSAEP = null;
                if (((Amdocs.Ginger.Repository.RepositoryItemBase)RunSetActionsGrid.CurrentItem).ItemName == "Open ALM Defects")
                {
                    if (runSetActionEditPage == null)
                    {
                        runSetActionEditPage = new RunSetActionEditPage((RunSetActionBase)RunSetActionsGrid.CurrentItem);
                        RSAEP = runSetActionEditPage;
                    }
                    else
                    {
                        RSAEP = runSetActionEditPage;
                    }
                }
                else
                {
                    RSAEP = new RunSetActionEditPage((RunSetActionBase)RunSetActionsGrid.CurrentItem);
                }
                RunSetActionEditFrame.Content = RSAEP;
            }
        }

        private void SetGridView()
        {
            RunSetActionsGrid.ShowHeader = Visibility.Collapsed;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;

            viewCols.Add(new GridColView() { Field = nameof(RunSetActionBase.Active), WidthWeight = 50, StyleType = GridColView.eGridColStyleType.CheckBox });
            viewCols.Add(new GridColView() { Field = nameof(RunSetActionBase.Name), WidthWeight = 150 });
            viewCols.Add(new GridColView() { Field = nameof(RunSetActionBase.Type), Header = "Type", WidthWeight = 150, BindingMode = System.Windows.Data.BindingMode.OneWay, ReadOnly=true });
            List<ComboEnumItem> runAtOptionList = GingerCore.General.GetEnumValuesForCombo(typeof(RunSetActionBase.eRunAt));
            viewCols.Add(new GridColView() { Field = nameof(RunSetActionBase.RunAt), Header = "Run At", WidthWeight = 100, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = runAtOptionList });
            List<ComboEnumItem> conditionOptionList = GingerCore.General.GetEnumValuesForCombo(typeof(RunSetActionBase.eRunSetActionCondition));
            viewCols.Add(new GridColView() { Field = nameof(RunSetActionBase.Condition), WidthWeight = 100, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = conditionOptionList });
            viewCols.Add(new GridColView() { Field = nameof(RunSetActionBase.Status), WidthWeight = 80, BindingMode = System.Windows.Data.BindingMode.OneWay, ReadOnly = true });
            viewCols.Add(new GridColView() { Field = nameof(RunSetActionBase.Errors), WidthWeight = 80, BindingMode = System.Windows.Data.BindingMode.OneWay, ReadOnly = true });
            viewCols.Add(new GridColView() { Field = nameof(RunSetActionBase.ElapsedSecs), Header = "Execution Duration (Seconds)", WidthWeight = 50, BindingMode = System.Windows.Data.BindingMode.OneWay, ReadOnly = true });
            
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

            RunSetActionScriptOperations runSetActionScript = new RunSetActionScriptOperations(RSAS);
            RSAS.RunSetActionScriptOperations = runSetActionScript;

            RunSetActionBaseOperations runSetActionBaseOperations = new RunSetActionBaseOperations(RSAS);
            RSAS.runSetActionBaseOperations = runSetActionBaseOperations;
        }
        private void RunSelected(object sender, RoutedEventArgs e)
        {
            if ((WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == Reports.ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB || WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == Reports.ExecutionLoggerConfiguration.DataRepositoryMethod.TextFile)
                && ((RunSetActionBase)RunSetActionsGrid.CurrentItem).GetType() == typeof(RunSetActionHTMLReportSendEmail) 
                && WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunSetExecutionStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending)
            {
                Reporter.ToUser(eUserMsgKey.RunSetNotExecuted);
                return;
            }
                ((RunSetActionBase)RunSetActionsGrid.CurrentItem).runSetActionBaseOperations.ExecuteWithRunPageBFES();
        }
        private void RunAll(object sender, RoutedEventArgs e)
        {
            foreach (RunSetActionBase a in mRunSetConfig.RunSetActions)
            {
                mRunSetConfig.RunSetActions.CurrentItem = a;
                a.runSetActionBaseOperations.ExecuteWithRunPageBFES();
            }
        }

        private void AddSendSMS(object sender, RoutedEventArgs e)
        {
            RunSetActionSendSMS RSASS = new RunSetActionSendSMS();
            RSASS.Name = RSASS.Type;
            RSASS.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(RSASS);
            RunSetActionsGrid.Grid.SelectedItem = RSASS;

            RunSetActionSendSMSOperations runSetActionSendSMS = new RunSetActionSendSMSOperations(RSASS);
            RSASS.RunSetActionSendSMSOperations = runSetActionSendSMS;

            RunSetActionBaseOperations runSetActionBaseOperations = new RunSetActionBaseOperations(RSASS);
            RSASS.runSetActionBaseOperations = runSetActionBaseOperations;
        }

        private void AddSaveResults(object sender, RoutedEventArgs e)
        {
            RunSetActionSaveResults RSASR = new RunSetActionSaveResults();
            RSASR.Name = RSASR.Type;
            RSASR.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(RSASR);
            RunSetActionsGrid.Grid.SelectedItem = RSASR;

            RunSetActionSaveResultsOperations runSetActionSaveResults = new RunSetActionSaveResultsOperations(RSASR);
            RSASR.RunSetActionSaveResultsOperations = runSetActionSaveResults;

            RunSetActionBaseOperations runSetActionBaseOperations = new RunSetActionBaseOperations(RSASR);
            RSASR.runSetActionBaseOperations = runSetActionBaseOperations;
        }
        private void AddPublishtoALMAction(object sender, RoutedEventArgs e)
        {
            RunSetActionPublishToQC RSAPTAC = new RunSetActionPublishToQC();
            RSAPTAC.Name = RSAPTAC.Type;
            RSAPTAC.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(RSAPTAC);
            RunSetActionsGrid.Grid.SelectedItem = RSAPTAC;

            RunSetActionPublishToQCOperations runSetActionPublishToQC = new RunSetActionPublishToQCOperations(RSAPTAC);
            RSAPTAC.RunSetActionPublishToQCOperations = runSetActionPublishToQC;

            RunSetActionBaseOperations runSetActionBaseOperations = new RunSetActionBaseOperations(RSAPTAC);
            RSAPTAC.runSetActionBaseOperations = runSetActionBaseOperations;
        }

        private void AddSendHTMLReportEmailAction(object sender, RoutedEventArgs e)
        {
            RunSetActionHTMLReportSendEmail RSASR = new RunSetActionHTMLReportSendEmail();
            RSASR.Name = RSASR.Type;
            RSASR.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(RSASR);
            RunSetActionsGrid.Grid.SelectedItem = RSASR;

            RunSetActionHTMLReportSendEmailOperations runSetActionHTMLReportSendEmail = new RunSetActionHTMLReportSendEmailOperations(RSASR);
            RSASR.RunSetActionHTMLReportSendEmailOperations = runSetActionHTMLReportSendEmail;

            RunSetActionBaseOperations runSetActionBaseOperations = new RunSetActionBaseOperations(RSASR);
            RSASR.runSetActionBaseOperations = runSetActionBaseOperations;
        }

        private void AddSendFreeEmailAction(object sender, RoutedEventArgs e)
        {
            RunSetActionSendFreeEmail RSAFTE = new RunSetActionSendFreeEmail();
            RSAFTE.Name = RSAFTE.Type;
            mRunSetConfig.RunSetActions.Add(RSAFTE);
            RunSetActionsGrid.Grid.SelectedItem = RSAFTE;

            RunSetActionSendFreeEmailOperations runSetActionSendFreeEmail = new RunSetActionSendFreeEmailOperations(RSAFTE);
            RSAFTE.RunSetActionSendFreeEmailOperations = runSetActionSendFreeEmail;

            RunSetActionBaseOperations runSetActionBaseOperations = new RunSetActionBaseOperations(RSAFTE);
            RSAFTE.runSetActionBaseOperations = runSetActionBaseOperations;
        }

        private void AddSendEmailAction(object sender, RoutedEventArgs e)
        {
            RunSetActionSendEmail RSASR = new RunSetActionSendEmail();
            RSASR.Name = RSASR.Type;
            RSASR.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(RSASR);
            RunSetActionsGrid.Grid.SelectedItem = RSASR;

            RunSetActionSendEmailOperations runSetActionSendEmail = new RunSetActionSendEmailOperations(RSASR);
            RSASR.RunSetActionSendEmailOperations = runSetActionSendEmail;

            RunSetActionBaseOperations runSetActionBaseOperations = new RunSetActionBaseOperations(RSASR);
            RSASR.runSetActionBaseOperations = runSetActionBaseOperations;
        }
        private void AddGenerateTestNGReportAction(object sender, RoutedEventArgs e)
        {
            RunSetActionGenerateTestNGReport TNGRPT = new RunSetActionGenerateTestNGReport();
            TNGRPT.Name = TNGRPT.Type;
            TNGRPT.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(TNGRPT);
            RunSetActionsGrid.Grid.SelectedItem = TNGRPT;

            RunSetActionGenerateTestNGReportOperations runSetActionGenerateTestNGReport = new RunSetActionGenerateTestNGReportOperations(TNGRPT);
            TNGRPT.RunSetActionGenerateTestNGReportOperations = runSetActionGenerateTestNGReport;

            RunSetActionBaseOperations runSetActionBaseOperations = new RunSetActionBaseOperations(TNGRPT);
            TNGRPT.runSetActionBaseOperations = runSetActionBaseOperations;
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

            RunSetActionHTMLReportOperations runSetActionHTMLReport = new RunSetActionHTMLReportOperations(RSAHR);
            RSAHR.RunSetActionHTMLReportOperations = runSetActionHTMLReport;

            RunSetActionBaseOperations runSetActionBaseOperations = new RunSetActionBaseOperations(RSAHR);
            RSAHR.runSetActionBaseOperations = runSetActionBaseOperations;
        }

        private void AddAutomatedALMDefectsOperation(object sender, RoutedEventArgs e)
        {
            ObservableList<ALMDefectProfile> ALMDefectProfiles = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ALMDefectProfile>();
            if ((ALMDefectProfiles == null) || (ALMDefectProfiles.Count < 1))
            {
                Reporter.ToUser(eUserMsgKey.NoDefectProfileCreated);
                return;
            }

            RunSetActionAutomatedALMDefects RSAAAD = new RunSetActionAutomatedALMDefects();
            RSAAAD.Name = RSAAAD.Type;
            RSAAAD.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            RSAAAD.DefectsOpeningModeForAll = true;
            mRunSetConfig.RunSetActions.Add(RSAAAD);
            RunSetActionsGrid.Grid.SelectedItem = RSAAAD;

            RunSetActionAutomatedALMDefectsOperations runSetActionAutomatedALMDefects = new RunSetActionAutomatedALMDefectsOperations(RSAAAD);
            RSAAAD.RunSetActionAutomatedALMDefectsOperations = runSetActionAutomatedALMDefects;

            RunSetActionBaseOperations runSetActionBaseOperations = new RunSetActionBaseOperations(RSAAAD);
            RSAAAD.runSetActionBaseOperations = runSetActionBaseOperations;
        }

        private void AddJSONSummary(object sender, RoutedEventArgs e)
        {
            RunSetActionJSONSummary runSetActionJSONSummary = new RunSetActionJSONSummary();
            runSetActionJSONSummary.Name = runSetActionJSONSummary.Type;
            runSetActionJSONSummary.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(runSetActionJSONSummary);
            RunSetActionsGrid.Grid.SelectedItem = runSetActionJSONSummary;

            RunSetActionJSONSummaryOperations runSetActionJSONSummaryOperations = new RunSetActionJSONSummaryOperations(runSetActionJSONSummary);
            runSetActionJSONSummary.RunSetActionJSONSummaryOperations = runSetActionJSONSummaryOperations;

            RunSetActionBaseOperations runSetActionBaseOperations = new RunSetActionBaseOperations(runSetActionJSONSummary);
            runSetActionJSONSummary.runSetActionBaseOperations = runSetActionBaseOperations;
        }
        private void AddSendExecutionDataToExternalSourceAction(object sender, RoutedEventArgs e)
        {
            RunSetActionSendDataToExternalSource runSetActionSendDataToExternalSource = new RunSetActionSendDataToExternalSource();
            runSetActionSendDataToExternalSource.Name = runSetActionSendDataToExternalSource.Type;
            runSetActionSendDataToExternalSource.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            mRunSetConfig.RunSetActions.Add(runSetActionSendDataToExternalSource);
            RunSetActionsGrid.Grid.SelectedItem = runSetActionSendDataToExternalSource;

            RunSetActionSendDataToExternalSourceOperations runSetActionSendDataToExternalSourceOperations = new RunSetActionSendDataToExternalSourceOperations(runSetActionSendDataToExternalSource);
            runSetActionSendDataToExternalSource.RunSetActionSendDataToExternalSourceOperations = runSetActionSendDataToExternalSourceOperations;

            RunSetActionBaseOperations runSetActionBaseOperations = new RunSetActionBaseOperations(runSetActionSendDataToExternalSource);
            runSetActionSendDataToExternalSource.runSetActionBaseOperations = runSetActionBaseOperations;
        }

    }
}
