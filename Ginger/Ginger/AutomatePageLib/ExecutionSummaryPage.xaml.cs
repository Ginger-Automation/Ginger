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
using De.TorstenMandelkow.MetroChart;
using Ginger.Run;
using GingerCore;
using GingerCore.DataSource;
using GingerCoreNET.GeneralLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.BusinessFlowWindows
{
    /// <summary>
    /// Interaction logic for ExecutionSummaryWindow.xaml
    /// </summary>
    public partial class ExecutionSummaryPage : Page
    {
        private Context mContext;

        public ExecutionSummaryPage(Context context)
        {
            InitializeComponent();

            this.Title = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Execution Summary";

            mContext = context;
            lblBizFlowName.Content = mContext.BusinessFlow.Name;

            ShowStatus();

            lblElapsed.Content = "Elapsed (Seconds): " + mContext.BusinessFlow.ElapsedSecs;
            ShowPie();
        }

        private void ShowPie()
        {
            int totalActivity = 0;
            int totalAction = 0;
            List<string> status;
            if (ActivityChart.Palette == null)
                ActivityChart.Palette = new ResourceDictionaryCollection();
            else
                ActivityChart.Palette.Clear();
            List<StatItems> activityStatList = new List<StatItems>();           
            List<StatItem> st = mContext.BusinessFlow.GetActivitiesStats();
            foreach (var v in st)
            {
                if (v.Description != "Running" &&  v.Description != "Pending" && v.Description != "Passed" && v.Description != "Failed" && v.Description != "Stopped" && !string.IsNullOrEmpty(v.Description))
                {
                    continue;
                }
                activityStatList.Add(new StatItems() { Description = v.Description, Count = (int)v.Count});
                ActivityChart.Palette.Add(GingerCore.General.SelectColor(v.Description));
                totalActivity += (int)v.Count;
            }         
           
            ViewModel activity = new ViewModel(activityStatList);
            ActivityChart.ChartTitle = GingerDicser.GetTermResValue(eTermResKey.Activities);
            ActivityChart.DataContext = activity;

            //Action
            if (ActionChart.Palette == null)
                ActionChart.Palette = new ResourceDictionaryCollection();
            else
                ActionChart.Palette.Clear();
            List<StatItems> actionStatList = new List<StatItems>();
            List<StatItem> act = mContext.BusinessFlow.GetActionsStat();           
            foreach (var v in act)
            {
                if (v.Description != "Running" && v.Description != "Pending" && v.Description != "Passed" && v.Description != "Failed" && v.Description != "Stopped" && v.Description != "FailIgnored" && !string.IsNullOrEmpty(v.Description))
                {
                    continue;
                }
                actionStatList.Add(new StatItems() { Description = v.Description, Count =(int)v.Count});
                ActionChart.Palette.Add(GingerCore.General.SelectColor(v.Description));
                totalAction += (int)v.Count;
            }
            ViewModel action = new ViewModel(actionStatList);
            ActionChart.DataContext = action;
            status = actionStatList.Select(b => b.Description).Concat(actionStatList.Select(c => c.Description)).Distinct().ToList();
            HideAllLegend();
            foreach (string s in status)
            {                
                SwitchLegend(s);
            }
            {
                stck.Children.Add(Ginger.General.makeImgFromControl(ActivityChart, totalActivity.ToString(),1));
                stck.Children.Add(Ginger.General.makeImgFromControl(ActionChart, totalAction.ToString(),2));
            }
            {                
                //App.RunsetActivityTextbox.Text = totalActivity.ToString();
                //App.RunsetActionTextbox.Text = totalAction.ToString();
            }
        }
        public void HideAllLegend()
        {
            Passed.Visibility = Visibility.Collapsed;
            Failed.Visibility = Visibility.Collapsed;
            Failed.Visibility = Visibility.Collapsed;
            Stopped.Visibility = Visibility.Collapsed;
            Pending.Visibility = Visibility.Collapsed;
            Running.Visibility = Visibility.Collapsed;
        }
        public void SwitchLegend(string status)
        {
            if (string.IsNullOrEmpty(status))
                status = "Pending";
            switch (status)
            {
                case "Passed":
                    Passed.Visibility = Visibility.Visible;
                    break;
                case "Failed":
                    Failed.Visibility = Visibility.Visible;
                    break;
                case "Fail":
                    Failed.Visibility = Visibility.Visible;
                    break;
                case "Stopped":
                    Stopped.Visibility = Visibility.Visible;
                    break;
                case "Pending":
                    Pending.Visibility = Visibility.Visible;
                    break;
                case "Running":
                    Running.Visibility = Visibility.Visible;
                    break;
                default:
                    Other.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void ShowStatus()
        {
            // Why we create new GR? !!!
            GingerRunner Gr = new GingerRunner();           
            foreach (Activity activity in mContext.BusinessFlow.Activities)
            {
                Gr.CalculateActivityFinalStatus(activity);
            }
            Gr.CalculateBusinessFlowFinalStatus(mContext.BusinessFlow);
            StatusLabel.Content = mContext.BusinessFlow.RunStatus;
            StatusLabel.Foreground = General.GetStatusBrush(mContext.BusinessFlow.RunStatus);            
        }

        internal void ShowAsWindow()
        {
            Button ReportButton = new Button();
            ReportButton.Content = "Generate Report";
            ReportButton.Click += ReportButton_Click;
            
            Button ExportBtn = new Button();
            ExportBtn.Content = "Export Execution Details";
            ExportBtn.Click += new RoutedEventHandler(ExportExecutionDetails);
            
            GenericWindow genWin = null;
            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, eWindowShowStyle.Dialog, this.Title, this, new ObservableList<Button> { ExportBtn, ReportButton });
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Reporter.ToStatus(eStatusMsgKey.CreatingReport);
            GingerCore.General.DoEvents();
            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.GenerateLastExecutedItemReport, null);
            Reporter.HideStatusMessage();
            Mouse.OverrideCursor = null;
        }

        private void ExportExecutionDetails(object sender, RoutedEventArgs e)
        {            
            ObservableList<BusinessFlow> bfs = new ObservableList<BusinessFlow>();
            bfs.Add(mContext.BusinessFlow);           
            if(!ExportResultsToALMConfigPage.Instance.IsProcessing)
            {
                ExportResultsToALMConfigPage.Instance.Init(bfs, new GingerCore.ValueExpression(mContext.Environment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false));
                ExportResultsToALMConfigPage.Instance.ShowAsWindow();
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.ExportedExecDetailsToALMIsInProcess);
            }
        }
    }
}
