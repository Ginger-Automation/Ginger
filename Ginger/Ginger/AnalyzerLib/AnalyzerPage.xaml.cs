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
using Ginger.Actions;
using Ginger.BusinessFlowWindows;
using Ginger.Run;
using Ginger.SolutionGeneral;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.DataSource;
using GingerCore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ginger.AnalyzerLib
{
    /// <summary>
    /// Interaction logic for AnalyzerPage.xaml
    /// </summary>
    public partial class AnalyzerPage : Page
    {
        private enum AnalyzedObject
        {
            Solution,
            BusinessFlow,
            RunSetConfig
        }

        AnalyzerUtils mAnalyzerUtils = new AnalyzerUtils();

        private AnalyzedObject mAnalyzedObject;

        GenericWindow _pageGenericWin = null;

        ObservableList<AnalyzerItemBase> mIssues = new ObservableList<AnalyzerItemBase>();
       
        private Solution mSolution;
        private BusinessFlow businessFlow;
        private RunSetConfig mRunSetConfig;
        // ObservableList<DataSourceBase> DSList;

        public bool BusyInProcess = false;

        private bool mAnalyzerCompleted;

        public bool IsAnalyzeDone
        {
            get { return mAnalyzerCompleted; }
        }

        public int TotalHighAndCriticalIssues
        {
            get { return (mIssues.Where(x => (x.Severity.ToString() == "High")).Count() + mIssues.Where(x => (x.Severity.ToString() == "Critical")).Count()); }
        }
       
        private bool mAnalyzeDoneOnce = false;
        private bool mAnalyzeWithUI = true;

        public AnalyzerPage()
        {
            InitializeComponent();

            SetAnalyzerItemsGridView();

            AnalyzerItemsGrid.DataSourceList = mIssues;

            mIssues.CollectionChanged += MIssues_CollectionChanged; 
        }
        
        private void MIssues_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
        {
            IssuesCounterLabel.Content = "Total Issues: ";
            IssuesCountLabel.Content = mIssues.Count();
            if ((mIssues.Where(x => (x.Severity.ToString() == "High")).Count()) > 0 || (mIssues.Where(x => (x.Severity.ToString() == "Critical")).Count()) > 0)
            {
                CriticalAndHighIssuesLabel.Content = "Total High & Critical Issues: ";
                CriticalAndHighIssuesLabelCounter.Content = (mIssues.Where(x => (x.Severity.ToString() == "High")).Count() + mIssues.Where(x => (x.Severity.ToString() == "Critical")).Count());
                CriticalAndHighIssuesLabelCounter.Foreground = new SolidColorBrush(Colors.Red);
                CriticalAndHighIssuesLabel.Visibility = Visibility.Visible;
            }
            if ((mIssues.Where(x => (x.CanAutoFix.ToString() == "Yes")).Count()) > 0)
            {
                CanAutoFixLable.Content = "Can be Auto Fixed: ";
                CanAutoFixLableCounter.Content = mIssues.Where(x => (x.CanAutoFix.ToString() == "Yes")).Count();
                CanAutoFixLable.Visibility = Visibility.Visible;
            }

        });

        }

        public void Init(Solution Solution)
        {
            mAnalyzedObject = AnalyzedObject.Solution;
            mSolution = Solution;
            AnalyzerItemsGrid.Title = "'" + mSolution.Name + "' Solution Issues";
        }

        public void Init(Solution Solution, BusinessFlow BusinessFlow,bool selfHealingAutoFixIssue=false)
        {
            mAnalyzedObject = AnalyzedObject.BusinessFlow;
            mSolution = Solution;
            businessFlow = BusinessFlow;
            AnalyzerItemsGrid.Title = "'" + BusinessFlow.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Issues";

            mAnalyzerUtils.SelfHealingAutoFixIssue = selfHealingAutoFixIssue;
        }

        internal void Init(Solution solution, Run.RunSetConfig RSC)
        {
            mRunSetConfig = RSC;
            mSolution = solution;
            mAnalyzedObject = AnalyzedObject.RunSetConfig;
            AnalyzerItemsGrid.Title = "'" + RSC.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " Issues";

            mAnalyzerUtils.SelfHealingAutoFixIssue = RSC.SelfHealingConfiguration.AutoFixAnalyzerIssue;

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (mAnalyzeDoneOnce == false)
            {
                await Analyze();
            }
           
        }

        private async void RerunButton_Click(object sender, RoutedEventArgs e)
        {
            if (BusyInProcess)
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Please wait for current process to end.");
                return;
            }
            mIssues.CollectionChanged -= MIssues_CollectionChanged;
            mIssues.CollectionChanged += MIssues_CollectionChanged;
            CriticalAndHighIssuesLabelCounter.Content = "0";
            CriticalAndHighIssuesLabelCounter.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#152B37");   //"#20334f";
            CanAutoFixLableCounter.Content = "0";
            await Analyze();
        }

        public async Task AnalyzeWithoutUI()
        {
            mAnalyzeWithUI = false;
            await Analyze();
        }

        private async Task Analyze()
        {
            // Each analyzer will set to true once completed, this is prep for multi run in threads for speed
            mIssues.Clear();
            BusyInProcess = true;
            mAnalyzerCompleted = false;
            mAnalyzeDoneOnce = true;
            
            try
            {
                if (mAnalyzeWithUI)
                {
                    SetStatus("Analyzing Started");
                }

                await Task.Factory.StartNew(() =>
                {
                    switch (mAnalyzedObject)
                    {
                        case AnalyzedObject.Solution:
                            SetStatus("Analyzing Solution...");
                            mAnalyzerUtils.RunSolutionAnalyzer(WorkSpace.Instance.Solution, mIssues);
                            break;

                        case AnalyzedObject.BusinessFlow:
                            SetStatus("Analyzing " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, suffixString: ":  ") + businessFlow.Name + "...");
                            mAnalyzerUtils.RunBusinessFlowAnalyzer(businessFlow, mIssues);
                            break;

                        case AnalyzedObject.RunSetConfig:
                            SetStatus("Analyzing " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + "...");
                            mAnalyzerUtils.RunRunSetConfigAnalyzer(mRunSetConfig, mIssues);
                            break;
                    }
                });
            }


            finally
            {
                if (mAnalyzerUtils.SelfHealingAutoFixIssue)
                {
                    for (var index = mIssues.Count - 1;index >=0 ; index--)
                    {
                        if (mIssues[index].CanAutoFix == AnalyzerItemBase.eCanFix.Yes && mIssues[index].FixItHandler != null)
                        {
                            mIssues[index].FixItHandler.Invoke(mIssues[index], null);
                            mIssues.RemoveAt(index);
                        }
                    }
                }
                

                BusyInProcess = false;
                mAnalyzerCompleted = true;
                SetAnalayzeProceesAsCompleted();
            }
            
        }

        private void SetAnalayzeProceesAsCompleted()
        {
            if (mAnalyzeWithUI)
            {
                SetStatus("Analyzer Completed");
                SetUIAfterAnalyzerCompleted();
            }

            mAnalyzerCompleted = true;
            mAnalyzeWithUI = true;
            BusyInProcess = false;
        }

        private void SetUIAfterAnalyzerCompleted()
        {
            try {
                Dispatcher.Invoke(() =>
                {
                    StatusLabel.Visibility = Visibility.Collapsed;
                });

                if (mIssues.Count > 0)
                {

                    //sort- placing Critical & High on top
                    Dispatcher.Invoke(() =>
                    {
                        ObservableList<AnalyzerItemBase> SortedList = new ObservableList<AnalyzerItemBase>();

                        foreach (AnalyzerItemBase issue in mIssues.OrderBy(nameof(AnalyzerItemBase.Severity)))
                            SortedList.Add(issue);                        
                        mIssues = SortedList;
                        AnalyzerItemsGrid.DataSourceList = mIssues;
                        AnalyzerItemsGrid.Grid.SelectedItem = mIssues[0];
                    });
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while sorting the Analyzer issues", ex);
            }
        }

        //private void RunRunSetConfigAnalyzer(RunSetConfig mRunSetConfig)
        //{
            
        //        List<AnalyzerItemBase> issues = RunSetConfigAnalyzer.Analyze(mRunSetConfig);
        //        AddIssues(issues);
        //        //TODO: check agents is not dup in different GR


        //        // Check all GRs BFS
        //        foreach (GingerRunner GR in mRunSetConfig.GingerRunners)
        //        {
        //            issues = AnalyzeGingerRunner.Analyze(GR,  WorkSpace.Instance.Solution.ApplicationPlatforms);
        //            AddIssues(issues);

        //            //Code to analyze Runner Unique Businessflow with Source BF
        //            List<Guid> checkedGuidList = new List<Guid>();
        //            foreach (BusinessFlow BF in GR.BusinessFlows)
        //            {
        //                if (!checkedGuidList.Contains(BF.Guid))//check if it already was analyzed
        //                {
        //                    checkedGuidList.Add(BF.Guid);
        //                    BusinessFlow actualBf = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Where(x => x.Guid == BF.Guid).FirstOrDefault();
        //                    if (actualBf != null)
        //                        RunBusinessFlowAnalyzer(actualBf, false);
        //                }
        //            }
        //            //Code to analyze Runner BF i.e. BFFlowControls
        //            foreach (BusinessFlow BF in GR.BusinessFlows)
        //            {
        //                List<AnalyzerItemBase> fcIssues = AnalyzeRunnerBusinessFlow.Analyze(GR, BF);
        //                AddIssues(fcIssues);
        //            }
        //        }

        //        SetAnalayzeProceesAsCompleted();
            
        //}

        //private List<string> RunBusinessFlowAnalyzer(BusinessFlow businessFlow, bool markCompletion = true)
        //{
        //    List<string> usedVariablesInBF = new List<string>();
        //    List<string> usedVariablesInActivity = new List<string>();

        //    DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
        //    SetStatus("Analyzing " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, suffixString: ":  ") + businessFlow.Name);
        //    List<AnalyzerItemBase> issues = AnalyzeBusinessFlow.Analyze(mSolution, businessFlow);
        //    AddIssues(issues);
           
        //    // Removing Parallel.Foreach as its freezing on main thread when running from CLI

        //        //Parallel.ForEach(businessFlow.Activities, new ParallelOptions { MaxDegreeOfParallelism = 5 }, activity =>
        //        // {
        //        foreach (Activity activity in businessFlow.Activities)
        //        {
        //            issues = AnalyzeActivity.Analyze(businessFlow, activity);
        //            AddIssues(issues);


        //            //Parallel.ForEach(activity.Acts, new ParallelOptions { MaxDegreeOfParallelism = 5 }, iaction =>
        //            //{
        //            foreach (IAct iaction in activity.Acts)
        //            {
        //                Act action = (Act)iaction;
        //                List<AnalyzerItemBase> actionissues = AnalyzeAction.Analyze(businessFlow, activity, action, DSList);
        //                AddIssues(actionissues);
        //                List<string> tempList = AnalyzeAction.GetUsedVariableFromAction(action);
        //                usedVariablesInActivity.AddRange(tempList);
        //            };

        //            List<string> activityVarList = AnalyzeActivity.GetUsedVariableFromActivity(activity);
        //            usedVariablesInActivity.AddRange(activityVarList);
        //            ReportUnusedVariables(activity, usedVariablesInActivity);
        //            usedVariablesInBF.AddRange(usedVariablesInActivity);
        //            usedVariablesInActivity.Clear();
        //        };
        //     ReportUnusedVariables(businessFlow, usedVariablesInBF);

        //     if (markCompletion)
        //     {
        //         SetAnalayzeProceesAsCompleted();
        //     }
         
        //    return usedVariablesInBF;

        //}

        //public void ReportUnusedVariables(object obj, List<string> usedVariables)
        //{
        //    List<AnalyzerItemBase> IssuesList = new List<AnalyzerItemBase>();
        //    Solution solution = null;
        //    BusinessFlow businessFlow = null;
        //    Activity activity = null;
        //    string variableSourceType = "";
        //    string variableSourceName = "";
        //    ObservableList<VariableBase> AvailableAllVariables = new ObservableList<VariableBase>();
        //    if (typeof(BusinessFlow).Equals(obj.GetType()))
        //    {
        //        businessFlow = (BusinessFlow)obj;
        //        if (businessFlow.Variables.Count > 0)
        //        {
        //            AvailableAllVariables = businessFlow.Variables;
        //            variableSourceType = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
        //            variableSourceName = businessFlow.Name;                    
        //        }
        //    }
        //    else if (typeof(Activity).Equals(obj.GetType()))
        //    {
        //        activity = (Activity)obj;
        //        if (activity.Variables.Count > 0)
        //        {
        //            AvailableAllVariables = activity.Variables;
        //            variableSourceType = GingerDicser.GetTermResValue(eTermResKey.Activity);
        //            variableSourceName = activity.ActivityName;                    
        //        }
        //    }
        //    else if(typeof(Solution).Equals(obj.GetType()))
        //    {
        //        solution = (Solution)obj;
        //        AvailableAllVariables = solution.Variables;
        //        variableSourceType = "Solution";
        //        variableSourceName = solution.Name;                                
        //    }

        //    foreach (VariableBase var in AvailableAllVariables)
        //    {
        //        if (usedVariables != null && (!usedVariables.Contains(var.Name)))
        //        {                    
        //            if (obj.GetType().Equals(typeof(BusinessFlow)))
        //            {
        //                AnalyzeBusinessFlow aa = new AnalyzeBusinessFlow();
        //                aa.Status = AnalyzerItemBase.eStatus.NeedFix;
        //                aa.ItemName = var.Name;
        //                aa.Description = var + " is Unused in " + variableSourceType + ": " + businessFlow.Name;
        //                aa.Details = variableSourceType;                        
        //                aa.mBusinessFlow = businessFlow;
        //                aa.ItemParent = variableSourceName;
        //                aa.CanAutoFix = AnalyzerItemBase.eCanFix.Yes;                   
        //                aa.IssueType = eType.Error;
        //                aa.FixItHandler = DeleteUnusedVariables;
        //                aa.Severity = eSeverity.Medium;
        //                IssuesList.Add(aa);
        //            }                        
        //            else if (obj.GetType().Equals(typeof(Solution)))
        //            {
        //                AnalyzeSolution aa = new AnalyzeSolution();
        //                aa.Status = AnalyzerItemBase.eStatus.NeedFix;
        //                aa.ItemName = var.Name;
        //                aa.Description = var + " is Unused in Solution";
        //                aa.Details = variableSourceType;                       
        //                aa.ItemParent = variableSourceName;
        //                aa.CanAutoFix = AnalyzerItemBase.eCanFix.Yes;                  
        //                aa.IssueType = eType.Error;
        //                aa.FixItHandler = DeleteUnusedVariables;
        //                aa.Severity = eSeverity.Medium;
        //                IssuesList.Add(aa);
        //            }                        
        //            else
        //            {
        //                AnalyzeActivity aa = new AnalyzeActivity();                        
        //                aa.Status = AnalyzerItemBase.eStatus.NeedFix;
        //                aa.ItemName = var.Name;
        //                aa.Description = var + " is Unused in " + variableSourceType + ": " + activity.ActivityName;
        //                aa.Details = variableSourceType;
        //                aa.mActivity = activity;
        //                //aa.mBusinessFlow = businessFlow;
        //                aa.ItemParent = variableSourceName;
        //                aa.CanAutoFix = AnalyzerItemBase.eCanFix.Yes;                  
        //                aa.IssueType = eType.Error;
        //                aa.FixItHandler = DeleteUnusedVariables;
        //                aa.Severity = eSeverity.Medium;
        //                IssuesList.Add(aa);
        //            }                    
        //        }
        //    }            
        //    AddIssues(IssuesList);
        //}
        //private static void DeleteUnusedVariables(object sender, EventArgs e)
        //{
        //    if (sender.GetType().Equals(typeof(AnalyzeActivity)))
        //    {
        //        Activity activity = ((AnalyzeActivity)sender).mActivity;
        //        foreach (VariableBase var in activity.Variables)
        //        {
        //            if (var.Name.Equals(((AnalyzeActivity)sender).ItemName))
        //            {
        //                activity.Variables.Remove(var);
        //                activity.RefreshVariablesNames();
        //                ((AnalyzeActivity)sender).Status = eStatus.Fixed;
        //                break;
        //            }
        //        }
        //    }
        //    else if(sender.GetType().Equals(typeof(AnalyzeBusinessFlow)))
        //    {
        //        BusinessFlow BFlow= ((AnalyzeBusinessFlow)sender).mBusinessFlow;                                                   
        //        foreach (VariableBase var in BFlow.Variables)
        //        {
        //            if (var.Name.Equals(((AnalyzeBusinessFlow)sender).ItemName))
        //            {
        //                BFlow.Variables.Remove(var);
        //                ((AnalyzeBusinessFlow)sender).Status = eStatus.Fixed;
        //                break;
        //            }
        //        }
        //    }
        //    else if (sender.GetType().Equals(typeof(AnalyzeSolution)))
        //    {                 
        //        foreach (VariableBase var in BusinessFlow.SolutionVariables)
        //        {
        //            if (var.Name.Equals(((AnalyzeSolution)sender).ItemName))
        //            {
        //                BusinessFlow.SolutionVariables.Remove(var);
        //                ((AnalyzeSolution)sender).Status = eStatus.Fixed;
        //                break;
        //            }
        //        }
        //    }            
        //}
    
        private void SetStatus(string txt)
        {
            // GingerCore.General.DoEvents();

            StatusLabel.Dispatcher.Invoke(
               System.Windows.Threading.DispatcherPriority.Normal,
                   new Action(
                       delegate ()
                       {
                           StatusLabel.Visibility = Visibility.Visible;
                           StatusLabel.Content = txt;
                       }
           ));

        }


        //private void RunSolutionAnalyzer()
        //{
        //    mIssues.Clear();

        //    //TODO: once this analyzer is taking long time due to many checks, run it using parallel
        //    //ObservableList<BusinessFlow> BFs = App.LocalRepository.GetAllBusinessFlows();
        //    ObservableList<BusinessFlow> BFs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();

        //    // Run it in another task so UI gets updates
        //    Task t = Task.Factory.StartNew(() =>
        //    {

        //        List<AnalyzerItemBase> issues = AnalyzeSolution.Analyze(mSolution);
        //        List<string> usedVariablesInSolution = new List<string>();
        //        AddIssues(issues);

        //        foreach (BusinessFlow BF in BFs)
        //        {
        //            List<string> tempList=RunBusinessFlowAnalyzer(BF, false);
        //            usedVariablesInSolution.AddRange(tempList);                    
        //        }                
        //        ReportUnusedVariables(mSolution, usedVariablesInSolution);

        //        SetAnalayzeProceesAsCompleted();
        //    });            
        //}


        //void AddIssues(List<AnalyzerItemBase> issues)
        //{

        //    StatusLabel.Dispatcher.Invoke(
        //        System.Windows.Threading.DispatcherPriority.Normal,
        //            new Action(
        //                delegate ()
        //                {
        //                    foreach (AnalyzerItemBase AIB in issues)
        //                    {
        //                        mIssues.Add(AIB);
        //                        IssuesCounterLabel.Content = "Total Issues: ";
        //                        IssuesCountLabel.Content = mIssues.Count();
        //                        if ((mIssues.Where(x => (x.Severity.ToString() == "High")).Count()) > 0 || (mIssues.Where(x => (x.Severity.ToString() == "Critical")).Count()) > 0)
        //                        {
        //                            CriticalAndHighIssuesLabel.Content = "Total High & Critical Issues: ";
        //                            CriticalAndHighIssuesLabelCounter.Content = (mIssues.Where(x => (x.Severity.ToString() == "High")).Count() + mIssues.Where(x => (x.Severity.ToString() == "Critical")).Count());
        //                            CriticalAndHighIssuesLabelCounter.Foreground = new SolidColorBrush(Colors.Red);
        //                            CriticalAndHighIssuesLabel.Visibility = Visibility.Visible;
        //                        }
        //                        if ((mIssues.Where(x => (x.CanAutoFix.ToString() == "Yes")).Count()) > 0)
        //                        {
        //                            CanAutoFixLable.Content = "Can be Auto Fixed: ";
        //                            CanAutoFixLableCounter.Content = mIssues.Where(x => (x.CanAutoFix.ToString() == "Yes")).Count();
        //                            CanAutoFixLable.Visibility = Visibility.Visible;
        //                        }
        //                    }
        //                }
        //    ));
        //}

        private void SetAnalyzerItemsGridView()
        {
            //# Default View

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            
            view.GridColsView.Add(new GridColView() { Field = nameof(AnalyzerItemBase.SeverityIcon), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 2.5, AllowSorting = true, MaxWidth = 20 });
            view.GridColsView.Add(new GridColView() { Field = nameof(AnalyzerItemBase.Selected), Header = "Selected", WidthWeight = 3, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.AnalyzerItems.Resources["FieldActive"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(AnalyzerItemBase.ItemClass), Header = "Item Type", WidthWeight = 3 ,AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(AnalyzerItemBase.ItemName), Header = "Item Name", WidthWeight = 10, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(AnalyzerItemBase.ItemParent), Header= "Item Parent", WidthWeight = 10, AllowSorting = true });                       
            view.GridColsView.Add(new GridColView() { Field = nameof(AnalyzerItemBase.Description), Header = "Issue", WidthWeight = 15, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(AnalyzerItemBase.Severity), Header = "Severity", WidthWeight = 3, MaxWidth = 50, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(AnalyzerItemBase.CanAutoFix), Header = "Auto Fixed?", WidthWeight = 3, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(AnalyzerItemBase.Status), Header = "Status", WidthWeight = 3, AllowSorting = true });

            AnalyzerItemsGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Select/UnSelect all issues which can be auto fixed", new RoutedEventHandler(MarkUnMark));

            AnalyzerItemsGrid.RowDoubleClick += AnalyzerItemsGrid_RowDoubleClick;
            AnalyzerItemsGrid.SetAllColumnsDefaultView(view);
            AnalyzerItemsGrid.InitViewItems();
            AnalyzerItemsGrid.SetTitleLightStyle = true;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            string title = string.Empty;
            switch (mAnalyzedObject)
            {
                case AnalyzedObject.BusinessFlow:
                    title = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Analyzer Results";
                    break;
                case AnalyzedObject.RunSetConfig:
                    title = GingerDicser.GetTermResValue(eTermResKey.RunSet) + " Analyzer Results";
                    break;
                case AnalyzedObject.Solution:
                    title = "Solution Analyzer Results";
                    break;
            }
            ObservableList<Button> winButtons = new ObservableList<Button>();

            Button SaveAllButton = new Button();
            SaveAllButton.Content = "Save Fixed Items";
            SaveAllButton.Margin = new Thickness(0, 0, 60, 0);
            //SaveAllButton.Click += new RoutedEventHandler(SaveAllButton_Click);
            SaveAllButton.Click += new RoutedEventHandler(SaveAllFixedItems_Click);
            winButtons.Add(SaveAllButton);

            Button FixSelectedButton = new Button();
            FixSelectedButton.Content = "Fix Selected";
            //FixSelectedButton.Click += new RoutedEventHandler(FixSelectedButton_Click);
            FixSelectedButton.Click += async (o, e) =>
            {
                if (mIssues.Where(x=> x.Selected==true).ToList().Count == 0)
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Please select issue to fix.");
                    return;
                }

                if (BusyInProcess)
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Please wait for current process to end.");
                    return;
                }
                BusyInProcess = true;
                SetStatus("Starting to Fix Selected Items...");
                StatusLabel.Visibility = Visibility.Visible;
                try
                { 
                    await Task.Run(() => FixSelectedItems());
                }
                finally
                {
                    BusyInProcess = false;
                }
                SetStatus("Finished to Fix Selected Items.");
                StatusLabel.Visibility = Visibility.Collapsed;               
            };
            winButtons.Add(FixSelectedButton);

            Button RerunButton = new Button();
            RerunButton.Content = "Re-Analyze";
            RerunButton.Margin = new Thickness(0, 0, 60, 0);
            RerunButton.Click += new RoutedEventHandler(RerunButton_Click);
            winButtons.Add(RerunButton);

            this.Height = 800;
            this.Width = 800;
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, title, this, winButtons);

            if (mAnalyzeDoneOnce)
                SetUIAfterAnalyzerCompleted();
        }

        private void MarkUnMark(object sender, RoutedEventArgs e)
        {
                List<AnalyzerItemBase> autoFixItemsList = mIssues.Where(x => x.CanAutoFix == AnalyzerItemBase.eCanFix.Yes).ToList();
                if (autoFixItemsList!= null && autoFixItemsList.Count>0)
                {
                    bool flagToset = (!autoFixItemsList[0].Selected);
                    foreach (AnalyzerItemBase item in autoFixItemsList)
                    {
                        item.Selected = flagToset;
                    }
                }
        }

        private async void SaveAllFixedItems_Click(object sender, RoutedEventArgs e)
        {
            if (BusyInProcess)
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Please wait for current process to end.");
                return;
            }
            // TODO: click/use the same code on solution which will save all changed items...
            // Meanwhile the below is good start 
            if (Reporter.ToUser(eUserMsgKey.SaveAllItemsParentWarning) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                BusyInProcess = true;
                SetStatus("Starting to Save Fixed Items...");
                StatusLabel.Visibility = Visibility.Visible;
                try
                { 
                    await Task.Run(() => SaveAllFixedItems());
                }
                finally
                {
                    BusyInProcess = false;
                }
                SetStatus("Finished to Save Selected Items.");
                StatusLabel.Visibility = Visibility.Collapsed;                
            }
        }

        private void SaveAllFixedItems()
        {
            Dictionary<BusinessFlow, List<AnalyzerItemBase>> itemsWhichWereSaved = new Dictionary<BusinessFlow, List<AnalyzerItemBase>>();            
            foreach (AnalyzerItemBase AI in mIssues)
            {
                if (AI.Status == AnalyzerItemBase.eStatus.Fixed)
                {
                    BusinessFlow bs = null;                    
                    if (AI.GetType() == typeof(AnalyzeBusinessFlow))
                    {
                        bs = ((AnalyzeBusinessFlow)AI).mBusinessFlow;
                    }
                    else if (AI.GetType() == typeof(AnalyzeActivity))
                    {
                        bs = ((AnalyzeActivity)AI).mBusinessFlow;
                    }
                    else if (AI.GetType() == typeof(AnalyzeAction))
                    {
                        bs = ((AnalyzeAction)AI).mBusinessFlow;
                    }                   
                    //TODO: add support for Run Set save
                    //using Dic so each BF will be saved only once  
                    if(bs!=null)
                    {
                        if (itemsWhichWereSaved.ContainsKey(bs) == false)
                            itemsWhichWereSaved.Add(bs, new List<AnalyzerItemBase>() { AI });
                        else
                            itemsWhichWereSaved[bs].Add(AI);
                    }
                }
            }            
            //do Bf's Save
            foreach (KeyValuePair<BusinessFlow, List<AnalyzerItemBase>> bfToSave in itemsWhichWereSaved)
            {
                if (bfToSave.Key != null)
                {
                    SetStatus("Saving " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, suffixString: ":")  + bfToSave.Key.Name);
                    
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(bfToSave.Key);
                    foreach (AnalyzerItemBase ai in bfToSave.Value)
                        ai.Status = AnalyzerItemBase.eStatus.FixedSaved;
                }
            }
            

        }

        private void FixSelectedItems()
        {            
            foreach (AnalyzerItemBase AI in mIssues)
            {
                if (AI.Selected && AI.Status == AnalyzerItemBase.eStatus.NeedFix)
                {
                    if (AI.FixItHandler != null)
                    {
                        //Reporter.ToGingerHelper(eStatusMsgKey.AnalyzerFixingIssues, null, AI.ItemName);
                        SetStatus("Fixing: " + AI.ItemName);                      
                        AI.FixItHandler.Invoke(AI, null);
                        //Reporter.CloseGingerHelper();                        
                    }
                    else
                    {
                        AI.Status = AnalyzerItemBase.eStatus.MissingFixHandler;
                    }
                }
            }
        }

        private void AnalyzerItemsGrid_RowDoubleClick(object sender, EventArgs e)
        {
            //show the item edit page 
            if (AnalyzerItemsGrid.CurrentItem is AnalyzeAction)
            {
                AnalyzeAction currentAnalyzeAction = (AnalyzeAction)AnalyzerItemsGrid.CurrentItem;
                Act actionIssue = currentAnalyzeAction.mAction;
                actionIssue.SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();
                ActionEditPage actedit = new ActionEditPage(actionIssue, General.eRIPageViewMode.ChildWithSave, currentAnalyzeAction.mBusinessFlow, currentAnalyzeAction.mActivity);
                //setting the BusinessFlow on the Action in Order to save 
                //actedit.mActParentBusinessFlow = ((AnalyzeAction)AnalyzerItemsGrid.CurrentItem).mBusinessFlow;
                //actedit.ap = null;
                actedit.ShowAsWindow(eWindowShowStyle.Dialog);
            }

            if (AnalyzerItemsGrid.CurrentItem is AnalyzeActivity)
            {
                AnalyzeActivity currentAnalyzeActivity = (AnalyzeActivity)AnalyzerItemsGrid.CurrentItem;
                Activity ActivityIssue = currentAnalyzeActivity.mActivity;
                //ActivityIssue.SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();
                GingerWPF.BusinessFlowsLib.ActivityPage ActivityEdit = new GingerWPF.BusinessFlowsLib.ActivityPage(ActivityIssue, new Context() { BusinessFlow = currentAnalyzeActivity.mBusinessFlow }, General.eRIPageViewMode.ChildWithSave);
                //setting the BusinessFlow on the Activity in Order to save
                //ActivityEdit.mBusinessFlow = ((AnalyzeActivity)AnalyzerItemsGrid.CurrentItem).mBusinessFlow;
                //ActivityEdit.ap = null;
                ActivityEdit.ShowAsWindow(eWindowShowStyle.Dialog);

            }

        }
        private void AnalyzerItemsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            txtBlkAnalyzerIssue.Text = string.Empty;
            TextBlockHelper TBH = new TextBlockHelper(txtBlkAnalyzerIssue);

            AnalyzerItemBase a = (AnalyzerItemBase)AnalyzerItemsGrid.CurrentItem;

            if (a != null)
            {                
                if (a.ItemClass != null)
                {
                    TBH.AddLineBreak();
                    TBH.AddBoldText("Item Type:");
                    TBH.AddLineBreak();
                    TBH.AddText(a.ItemClass.ToString());
                    TBH.AddLineBreak();
                }

                if (a.ItemName != null)
                {
                    TBH.AddLineBreak();
                    TBH.AddBoldText("Item Name:");
                    TBH.AddLineBreak();
                    TBH.AddText(a.ItemName.ToString());
                    TBH.AddLineBreak();
                }

                if (a.ItemParent != null)
                {
                    TBH.AddLineBreak();
                    TBH.AddBoldText("Item Parent:");
                    TBH.AddLineBreak();
                    TBH.AddText(a.ItemParent.ToString());
                    TBH.AddLineBreak();
                }

                if (a.Description != null)
                {
                    TBH.AddLineBreak();
                    TBH.AddBoldText("Issue:");
                    TBH.AddLineBreak();
                    TBH.AddText(a.Description.ToString());
                    TBH.AddLineBreak();
                }

                if (a.Details != null)
                {
                    TBH.AddLineBreak();
                    TBH.AddBoldText("Issue Details:");
                    TBH.AddLineBreak();
                    TBH.AddText(a.Details.ToString());
                    TBH.AddLineBreak();
                }

                if (a.Impact != null)
                {
                    TBH.AddLineBreak();
                    TBH.AddBoldText("Issue Impact:");
                    TBH.AddLineBreak();
                    TBH.AddText(a.Impact.ToString());
                    TBH.AddLineBreak();
                }

                if (a.HowToFix != null)
                {
                    TBH.AddLineBreak();
                    TBH.AddBoldText("How To Fix:");
                    TBH.AddLineBreak();
                    TBH.AddText(a.HowToFix.ToString());
                    TBH.AddLineBreak();
                }

                TBH.AddLineBreak();
                TBH.AddBoldText("Can be Auto Fixed:");
                TBH.AddLineBreak();
                TBH.AddText(a.CanAutoFix.ToString());
                TBH.AddLineBreak();
            }
        }
    }

}
