#region License
/*
Copyright © 2014-2018 European Support Limited

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

using Gherkin;
using Gherkin.Ast;
using GingerWPF.DragDropLib;
using Ginger.Repository;
using Ginger.SolutionWindows;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.UserControls;
using Ginger.UserControlsLib.TextEditor;
using Ginger.UserControlsLib.TextEditor.Gherkin;
using GingerCore;
using GingerCore.Activities;
using GingerCore.Variables;
using GingerPlugIns.TextEditorLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Amdocs.Ginger.Common;
using GingerWPF.UserControlsLib.UCTreeView;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Enums;
using GingerWPF.TreeViewItemsLib;
using Amdocs.Ginger.Repository;

namespace Ginger.GherkinLib
{
    /// <summary>
    /// Interaction logic for GherkinPage.xaml
    /// </summary>
    public partial class GherkinPage : Page, ITextEditorPage
    {
        GenericWindow genWin;
        public BusinessFlow mBizFlow;

        ObservableList<GherkinScenarioDefinition> mGherkinScenarioDefinition = new ObservableList<GherkinScenarioDefinition>();
        ObservableList<GherkinStep> mGherkinSteps = new ObservableList<GherkinStep>();
        ObservableList<GherkinTag> mTags = new ObservableList<GherkinTag>();

        ObservableList<GherkinParserException> mErrorsList = new ObservableList<GherkinParserException>();
        ObservableList<GherkinStep> mOptimizedSteps = new ObservableList<GherkinStep>();
        ObservableList<Guid> mSolTags = new ObservableList<Guid>();

        ActivitiesRepositoryPage ARP;

        int fileSize = 0;
        string folder;
        string FeatureName;
        string BFName;
        RepositoryFolder<BusinessFlow> targetBFFolder;
        bool isBFexists = false;
        string featureFileName = "";

        int ColorIndex = 0;

        public GherkinPage()
        {           
            InitializeComponent();

            folder = App.UserProfile.Solution.BusinessFlowsMainFolder;

            GherkinTextEditor.AddToolbarTool(General.GetImage("@Save_16x16.png"), Save_Click, "Save Gherkin Feature");
            GherkinTextEditor.SaveButton.Visibility = Visibility.Collapsed;

            GherkinTextEditor.AddToolbarTool(General.GetImage("@Grid_16x16.png"),  AddTable, "Add Examples Table");            

            DragDrop2.HookEventHandlers(GherkinTextEditor);

            // Set the grids
            SetScenariosGridView();
            ScenariosGrid.SetTitleLightStyle = true;
            ScenariosGrid.DataSourceList = mGherkinScenarioDefinition;

            SetStepsGridView();
            StepsGrid.SetTitleLightStyle = true;
            StepsGrid.DataSourceList = mGherkinSteps;

            TagsGrid.DataSourceList = mTags;
            TagsGrid.SetTitleLightStyle = true;
            SetTagsGrid();                        
            

            SetErrorsGridView();
            ErrorsGrid.SetTitleLightStyle = true;
            ErrorsGrid.DataSourceList = mErrorsList;

            OptimizedStepsGrid.SetTitleLightStyle = true;
            SetOptimizedGridView();
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            mSolTags.ClearAll();
            foreach (GherkinTag tag in mTags)
            {
                Guid guid = GetTagInSolution(tag.Name);
                if (guid != Guid.Empty)
                    mSolTags.Add(guid);
            }
            ARP.xActivitiesRepositoryGrid.Tags = mSolTags;
            SharedActivitiesFrame.Content = ARP;
        }

        private void Save_Click(TextEditorToolRoutedEventArgs Args)
        {
            Optimize();
            GherkinTextEditor.Save();
        }

        private void AddTable(TextEditorToolRoutedEventArgs Args)
        {

            this.GherkinTextEditor.textEditor.SelectedText = Environment.NewLine + "Examples:" + Environment.NewLine + "|A|B|C|" + Environment.NewLine + "|1|2|3|";

        }

        public bool Optimize()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            
            mErrorsList.ClearAll();
            ShowErros(false);
            
            ColorIndex = 0;

            mGherkinScenarioDefinition.ClearAll();
            mGherkinSteps.ClearAll();
            mTags.ClearAll();

            mOptimizedSteps.ClearAll();
            OptimizedStepsGrid.DataSourceList = mOptimizedSteps;

            var parser = new Parser();
            try
            {
                string txt = GherkinTextEditor.GetText();
                TextReader sr = new StringReader(txt);
                var gherkinDocument = parser.Parse(sr);

                if (gherkinDocument.Feature == null || gherkinDocument.Feature.Children==null)
                {
                    Mouse.OverrideCursor = null;
                    return false;
                }
                   

                foreach (ScenarioDefinition SD in gherkinDocument.Feature.Children)
                {
                    mGherkinScenarioDefinition.Add(new GherkinScenarioDefinition(SD));
                }


                FeatureName = System.IO.Path.GetFileName(GherkinTextEditor.FileName).Replace(".feature", "");
                foreach (var c in gherkinDocument.Comments)
                {
                    WriteTXT("Comments:" + c.Text);
                }

                foreach (Tag t in gherkinDocument.Feature.Tags)
                {
                    mTags.Add(new GherkinTag(t));
                }

                mSolTags.ClearAll();
                foreach (GherkinTag tag in mTags)
                {
                    Guid guid = GetTagInSolution(tag.Name.Substring(1));
                    if (guid != Guid.Empty && !mSolTags.Contains(guid))
                        mSolTags.Add(guid);
                }
                foreach (var c in gherkinDocument.Feature.Children)
                {
                    if (c.Keyword == "Scenario" || c.Keyword == "Scenario Outline")
                    {
                        WriteTXT("Keyword:" + c.Keyword);
                        WriteTXT("Name:" + c.Name);

                        if (c is Scenario)
                        {
                            foreach (Tag t in ((Gherkin.Ast.Scenario)c).Tags)
                            {
                                mTags.Add(new GherkinTag(t));
                                Guid guid = GetTagInSolution(t.Name.Substring(1));
                                if (guid != Guid.Empty && !mSolTags.Contains(guid))
                                    mSolTags.Add(guid);
                            }
                        }

                        if (c is ScenarioOutline)
                        {
                            foreach (Tag t in ((Gherkin.Ast.ScenarioOutline)c).Tags)
                            {
                                mTags.Add(new GherkinTag(t));
                                Guid guid = GetTagInSolution(t.Name.Substring(1));
                                if (guid != Guid.Empty && !mSolTags.Contains(guid))
                                    mSolTags.Add(guid);
                            }
                        }
                    }
                }
                foreach (var c in gherkinDocument.Feature.Children)
                {
                    if (c.Keyword == "Scenario" || c.Keyword == "Scenario Outline")
                    {
                        foreach (var step in c.Steps)
                        {
                            WriteTXT("Keyword:" + step.Keyword);
                            WriteTXT("Text:" + step.Text);

                            GherkinStep GS = new GherkinStep();
                            GS.Text = step.Text;
                            GS.Step = step;
                            mGherkinSteps.Add(GS);

                            String GherkingActivityName = GherkinGeneral.GetActivityGherkinName(step.Text);

                            GherkinStep OptimizedStep = SearchStepInOptimizedSteps(GherkingActivityName);
                            if (OptimizedStep == null)
                            {
                                GherkinStep NewOptimizedStep = new GherkinStep();
                                NewOptimizedStep.Text = GherkingActivityName;
                                NewOptimizedStep.Counter = 1;
                                NewOptimizedStep.ColorIndex = ColorIndex;
                                ColorIndex++;
                                NewOptimizedStep.AutomationStatus = GetStepAutomationStatus(GherkingActivityName);
                                GS.AutomationStatus = NewOptimizedStep.AutomationStatus;                               
                                mOptimizedSteps.Add(NewOptimizedStep);
                            }
                            else
                            {
                                OptimizedStep.Counter++;
                                GS.ColorIndex = OptimizedStep.ColorIndex;
                                GS.AutomationStatus = OptimizedStep.AutomationStatus;
                            }

                        }
                    }
                }
                
                // Warnings - TODO check other possisble warnings
                // Check Dups Scenario names 
                var query = mGherkinScenarioDefinition.GroupBy(x => x.Name)
                    .Where(g => g.Count() > 1)
                    .Select(y => y.Key)
                    .ToList();
                foreach (var v in query)
                {
                    IEnumerable<GherkinScenarioDefinition> SCS = from x in mGherkinScenarioDefinition where x.Name == v select x;
                    foreach (GherkinScenarioDefinition sc in SCS)
                    {
                        GherkinParserException PE = new GherkinParserException(sc.ScenarioDefintion.Location.Line, sc.ScenarioDefintion.Location.Column, "Duplicate Scenario Name: " + sc.Name);
                        mErrorsList.Add(PE);
                    }
                }
            }
            catch (CompositeParserException ex)
            {
                // we show the errors in the grid + mark in the textEditor                
                GherkinTextEditor.BackgroundRenderer.Segments.Clear();
                foreach (ParserException PE in ex.Errors)
                {
                    mErrorsList.Add(new GherkinParserException(PE));

                    var line = GherkinTextEditor.textEditor.Document.GetLineByNumber(PE.Location.Line);
                    GherkinTextEditor.BackgroundRenderer.Segments.Add(line);
                }
            }

            if (mErrorsList.Count > 0)
            {
                ShowErros(true);
                GherkinTextEditor.Focus();
            }
            else
            {
                GherkinTextEditor.BackgroundRenderer.Segments.Clear();
            }

            
            ARP.xActivitiesRepositoryGrid.Tags = mSolTags;
            SharedActivitiesFrame.Content = ARP;

            foreach(GherkinStep gStep in mOptimizedSteps)
            {

            }

            OptimizedStepsGrid.DataSourceList = mOptimizedSteps;
            Mouse.OverrideCursor = null;
            List<GherkinParserException> Errors = mErrorsList.Where(x => x.ErrorType == GherkinParserException.eErrorType.Error).ToList();
            bool result = mOptimizedSteps.Count > 0 && Errors.Count == 0;
            return result;
        }

        private void ShowErros(bool v)
        {
            if (v)
            {
                ErrorsGrid.Visibility = Visibility.Visible;
                ScenariosGrid.Visibility = Visibility.Collapsed;
                StepsGrid.Visibility = Visibility.Collapsed;
                TagsGrid.Visibility = Visibility.Collapsed;
                StepsSplitter.Visibility = Visibility.Collapsed;
                TagsSplitter.Visibility = Visibility.Collapsed;

            }
            else
            {
                ErrorsGrid.Visibility = Visibility.Collapsed;
                ScenariosGrid.Visibility = Visibility.Visible;
                StepsGrid.Visibility = Visibility.Visible;
                TagsGrid.Visibility = Visibility.Visible;
                StepsSplitter.Visibility = Visibility.Visible;
                TagsSplitter.Visibility = Visibility.Visible;
            }
        }

        private void PaintGrids()
        {
            ////TODO:hide columns or switch to ucGrid
        }

        private string GetStepAutomationStatus(string GherkinActivityName)
        {
            //TODO: get folder from feature file not BF

            //Search in repo

            // rule 1 - find in BF folder in repo 
            //TODO: get 
            string BusinessFlowFolder = "";
            string path = BusinessFlowFolder;
            ObservableList<Activity> activities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            //FIXME to use tags 
            Activity a2 = (from x in activities where x.ActivityName == GherkinActivityName select x).FirstOrDefault();
            if (a2 != null)
            {                
                return "Automated in Shared Repo - " + path;
            }

            // rule 2 - find in BF folder in common

            if (mBizFlow == null)
                return "Pending BF Creation";

            Activity a1 = (from x in mBizFlow.Activities where x.ActivityName == GherkinActivityName select x).FirstOrDefault();
            if (a1 != null)
            {
                if (a1.AutomationStatus == Activity.eActivityAutomationStatus.Automated)
                {
                    return "Automated in BF";
                }
                else
                {
                    return "Development in BF";
                }
            }
            else
            {

            }            
             return "Not Automated";
            // Search in shared repo
        }

        private void CreateActivityVariables(Activity a)
        {
            // Parmas in activity name will be %p1, %p2 etc...            
            int c = 0;
            while(true)
            {
                c++;

                //TODO: use const for %p
                int i = a.ActivityName.IndexOf("%p" + c);
                if (i>0)
                {
                    VariableString v = new VariableString();
                    v.Name = "p" + c;
                    a.Variables.Add(v);                    
                }
                else
                {
                    break;
                }
            } 
        }

        private void CreateActivitySelectionVariables(Activity a)
        {
            // Parmas in activity name will be %p1, %p2 etc...            
            string activityName = a.ActivityName;
            while (true)
            {
                //We can use c to define multiple selection lists if needed.
                string ColName = General.GetStringBetween(activityName, "<", ">");
                if (!string.IsNullOrEmpty(ColName))
                {
                    VariableSelectionList v = new VariableSelectionList();
                    v.Name = ColName;
                    a.Variables.Add(v);
                    activityName = activityName.Substring(activityName.IndexOf(">") + 1);
                }
                else
                {
                    break;
                }
            }
        }

        private GherkinStep SearchStepInOptimizedSteps(string Name)
        {
            // TODO: go RegEx Search
            GherkinStep GH = (from x in mOptimizedSteps where x.Text.ToUpper() == Name.ToUpper() select x).FirstOrDefault();
            return GH;
        }

        public void CreateNewBF(string BizFlowName,string fileName = null, RepositoryFolder<BusinessFlow> targetFolder = null)
        {
            if (GherkinTextEditor.FileName == null && fileName != null)
            {
                GherkinTextEditor.FileName = fileName;
            }

           
            if (targetFolder == null)
            {
                targetFolder = targetBFFolder;                
            }

            mBizFlow = App.CreateNewBizFlow(BizFlowName);
            mBizFlow.Source = BusinessFlow.eSource.Gherkin;
            mBizFlow.ExternalID = GherkinTextEditor.FileName.Replace(App.UserProfile.Solution.Folder, "~") ;                                                
            mBizFlow.Name = BizFlowName;
            mBizFlow.Activities.Clear();
            
            mBizFlow.ContainingFolder = targetFolder.FolderFullPath.Replace(App.UserProfile.Solution.Folder,"~");
            mBizFlow.ContainingFolderFullPath = targetFolder.FolderFullPath;            
            targetFolder.AddRepositoryItem(mBizFlow);
            targetFolder.RefreshFolderAndChildElementsSourceControlStatus();            
        }

        //TODO: show message on screen TBD!?
        private void WriteTXT(string t)
        {
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            ObservableList<Button> winButtons = new ObservableList<Button>();

            Button ImportButton = new Button();

            if (isBFexists)
            {
                ImportButton.Content = "Update " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
            } else
            {
                ImportButton.Content = "Generate " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);                
            }
            ImportButton.Click += new RoutedEventHandler(GeneratBFButton_Click);
            winButtons.Add(ImportButton);

            genWin = null;
            this.Height = 600;
            this.Width = 800;
            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, "Gherkin Feature File", this, winButtons);
        }

        public bool SavePrompt()
        {
            //set file size to check for changes
            string txt = GherkinTextEditor.GetText();
            if (fileSize != txt.Length && fileSize != 0)
            {
                fileSize = txt.Length; //TODO Reporter.ToUser(eUserMsgKeys.AskIfSureWantToClose);
                MessageBoxResult result = Reporter.ToUser(eUserMsgKeys.GherkinAskToSaveFeatureFile);
                if (result == MessageBoxResult.Yes)
                {
                    Save();
                    return true;
                }
                else if (result == MessageBoxResult.No)
                {
                    //Do nothing? this will still create optmized activities and even update BF without saving the feature file... not advised
                    return true;
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    //stop optimize so user can fix unwanted changes.
                    return false;
                }
            }
            fileSize = txt.Length;
            return true;            
        }

        private void GeneratBFButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isBFexists)
            {
                BusinessFlowsFolderTreeItem bfsFolder = new BusinessFlowsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>(),eBusinessFlowsTreeViewMode.ReadOnly);
                
                bfsFolder.IsGingerDefualtFolder = true;
                SingleItemTreeViewSelectionPage mTargetFolderSelectionPage = new SingleItemTreeViewSelectionPage(GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), eImageType.BusinessFlow, bfsFolder, SingleItemTreeViewSelectionPage.eItemSelectionType.Folder, true);

                List<object> selectedBfs = mTargetFolderSelectionPage.ShowAsWindow();
                if(selectedBfs !=null)
                {
                    targetBFFolder = (RepositoryFolder<BusinessFlow>)((ITreeViewItem)selectedBfs[0]).NodeObject();                
                }
                CreateNewBF(FeatureName);
                CreateActivities();
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mBizFlow);
                if (genWin != null)
                {
                    genWin.Close();
                }
                UpdateBFButton.Content = "Update " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                isBFexists = true;
                Reporter.ToUser(eUserMsgKeys.BusinessFlowUpdate, mBizFlow.ContainingFolder.Replace("BusinessFlows\\", "") + "\\" + mBizFlow.Name, "Created");
            }
            else
            {
                UpdateBFButton_Click();
                Reporter.ToUser(eUserMsgKeys.BusinessFlowUpdate, mBizFlow.ContainingFolder.Replace("BusinessFlows\\","") + "\\" + mBizFlow.Name, "Updated");
            }

            if(App.BusinessFlow == mBizFlow)
            {
                App.BusinessFlow = mBizFlow;
                App.BusinessFlow.SaveBackup();                
            }            
        }


        public void CreateActivities()
        {
            // We put all template optimized activitiy in Activities Group 

            ActivitiesGroup AG = (from x in mBizFlow.ActivitiesGroups where x.Name == "Optimized Activities" select x).FirstOrDefault();

            if (AG == null)
            {
                AG = new ActivitiesGroup();
                AG.Name = "Optimized Activities";
                mBizFlow.ActivitiesGroups.Add(AG);
            }
            ActivitiesGroup AG1 = (from x in mBizFlow.ActivitiesGroups where x.Name == "Optimized Activities - Not in Use" select x).FirstOrDefault();
            if (AG1 == null)
            {
                AG1 = new ActivitiesGroup();
                AG1.Name = "Optimized Activities - Not in Use";
                mBizFlow.ActivitiesGroups.Add(AG1);
            }          

            foreach(ActivityIdentifiers ia in AG.ActivitiesIdentifiers)
            {
                Activity a1 = (from x in mBizFlow.Activities where x.Guid == ia.ActivityGuid select x).FirstOrDefault();
                if (!AG1.CheckActivityInGroup(a1))
                    AG1.AddActivityToGroup(a1);
            }

            // Search each activity if not found create new
            foreach (GherkinStep GH in mOptimizedSteps)
            {                
                Activity a1 = (from x in mBizFlow.Activities where x.ActivityName == GH.Text select x).FirstOrDefault();
                if (a1 == null)
                {
                    if (GH.AutomationStatus == "Automated in Shared Repo - ")
                    {
                        ObservableList<Activity> activities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
                        Activity a2 = (from x in activities where x.ActivityName == GH.Text select x).FirstOrDefault();
                        //FIXME
                        if (a2 != null)
                        {
                            mBizFlow.AddActivity(a2);
                            a2.Active = false;                            
                            AG.AddActivityToGroup(a2);                            
                        }
                    }
                    else
                    {
                        Activity a = new Activity();
                        a.ActivityName = GH.Text;                        
                        a.Active = false;
                        a.TargetApplication = App.UserProfile.Solution.MainApplication;
                        a.ActionRunOption = Activity.eActionRunOption.ContinueActionsRunOnFailure;
                        CreateActivityVariables(a);
                        CreateActivitySelectionVariables(a);                        
                        mBizFlow.AddActivity(a);

                        AG.AddActivityToGroup(a);
                    }
                }
                //TODO: handle if exist we need to update !?
                else
                {
                    AG1.RemoveActivityFromGroup(a1);                                                                
                }
            }
            foreach (ActivityIdentifiers ia in AG1.ActivitiesIdentifiers)
            {
                Activity a1 = (from x in mBizFlow.Activities where x.Guid == ia.ActivityGuid select x).FirstOrDefault();
                if (AG.CheckActivityInGroup(a1))
                    AG.RemoveActivityFromGroup(a1);
            }
            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mBizFlow);
        }


        private Guid GetTagInSolution(string TagName)
        {
            if (TagName.StartsWith("@"))
                TagName = TagName.Substring(1);
            Guid TagGuid = (from x in App.UserProfile.Solution.Tags where x.Name == TagName select x.Guid).FirstOrDefault();            
            return TagGuid;
        }

        private void ColorsButton_Click(object sender, RoutedEventArgs e)
        {
            PaintGrids();
        }

        private void OptimizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavePrompt()) { Optimize(); }
        }

        private void UpdateBFButton_Click()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                string externalID = featureFileName.Replace(App.UserProfile.Solution.Folder, "~");
                if(BFName.EndsWith(".Ginger.BusinessFlow.xml"))
                {
                    BFName = Path.GetFileName(BFName).Replace(".Ginger.BusinessFlow.xml", "");
                }
                
                mBizFlow = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Where(x =>x.Source == BusinessFlow.eSource.Gherkin && (x.ExternalID == externalID || x.ExternalID == featureFileName)).SingleOrDefault();
                if (mBizFlow == null)
                {                    
                    CreateNewBF(FeatureName);
                }
                CreateActivities();
            }            
            finally
            {
                Mouse.OverrideCursor = null;
            }            
        }

        public bool Load(string FileName)
        {
            featureFileName = FileName;
            GherkinTextEditor.SetContentEditorTitleLabel(Path.GetFileName(FileName), (Style)TryFindResource("@ucGridTitleLightStyle"));
            GherkinDcoumentEditor g = new GherkinDcoumentEditor();                        
            g.OptimizedSteps = mOptimizedSteps;
            g.OptimizedTags = mTags;
            GherkinTextEditor.Init(FileName, g, true);
            mSolTags.ClearAll();
            foreach (GherkinTag tag in mTags)
            {
                Guid guid = GetTagInSolution(tag.Name);
                if (guid != Guid.Empty)
                    mSolTags.Add(guid);
            }
            
            ARP = new ActivitiesRepositoryPage(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Activity>(), null, mSolTags, ArrowButtonHandler);
            ARP.xActivitiesRepositoryGrid.EnableTagsPanel = false;
            SharedActivitiesFrame.Content = ARP;

            BFName = FileName.Replace(App.UserProfile.Solution.Folder, "");
            //to prevent creating a folder rather than putting them on BF level.
            if (BFName.Contains("Business Flows"))
            {
                BFName = BFName.Replace("Business Flows", "");
            }
            if (BFName.EndsWith(".feature"))
            {
                BFName = Path.GetFileName(FileName).Replace(".feature", "");
            }            
            // search if we have the BF defined already, so search in BF will work
            string externalID = FileName.Replace(App.UserProfile.Solution.Folder, "~");
            
            mBizFlow = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Where(x =>x.Source == BusinessFlow.eSource.Gherkin && (x.ExternalID == externalID || x.ExternalID == FileName)).SingleOrDefault();                           
            
            if (mBizFlow != null)
            {
                BFName = mBizFlow.FileName;
                isBFexists = true;
                UpdateBFButton.Content = "Update "+ GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
            }
            else
            {
                isBFexists = false;
                UpdateBFButton.Content = "Create "+ GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
            }
            SavePrompt();

            return Optimize();            
        }

        private void ArrowButtonHandler(object sender, RoutedEventArgs e)
        {
            foreach (Activity selectedItem in ARP.xActivitiesRepositoryGrid.Grid.SelectedItems)
            {
                GherkinTextEditor.textEditor.SelectedText = GherkinTextEditor.textEditor.SelectedText + selectedItem.ActivityName + Environment.NewLine;
            }    
        }

        public void Save()
        {
            Optimize();
            GherkinTextEditor.Save();
        }

        private void SetErrorsGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;

            viewCols.Add(new GridColView() { Field = GherkinParserException.Fields.ErrorImage,Header = " ", WidthWeight = 10, BindingMode = System.Windows.Data.BindingMode.OneWay, MaxWidth = 20, StyleType = GridColView.eGridColStyleType.Image });
            viewCols.Add(new GridColView() { Field = GherkinParserException.Fields.ErrorType, WidthWeight = 40, BindingMode = System.Windows.Data.BindingMode.OneWay});
            viewCols.Add(new GridColView() { Field = GherkinParserException.Fields.Line, WidthWeight = 30 , BindingMode = System.Windows.Data.BindingMode.OneWay});
            viewCols.Add(new GridColView() { Field = GherkinParserException.Fields.Column, WidthWeight = 30, BindingMode = System.Windows.Data.BindingMode.OneWay });
            viewCols.Add(new GridColView() { Field = GherkinParserException.Fields.Error, WidthWeight = 200, BindingMode = System.Windows.Data.BindingMode.OneWay });

            ErrorsGrid.SetAllColumnsDefaultView(view);
            ErrorsGrid.InitViewItems();

            ErrorsGrid.RowChangedEvent += ErrorsGrid_RowChangedEvent;
        }

        public class ErrorTypeConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter,
                    System.Globalization.CultureInfo culture)
            {
                string errorType = value.ToString();
                if (errorType.Equals(GherkinParserException.eErrorType.Error.ToString()))
                    return Brushes.Orange;
                if (errorType.Equals(GherkinParserException.eErrorType.Warning.ToString()))
                    return Brushes.Red;
                return Brushes.Gray;
            }

            public object ConvertBack(object value, Type targetType,
                object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private void ErrorsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            if (ErrorsGrid.CurrentItem != null)
            {
                int i = ((GherkinParserException)ErrorsGrid.CurrentItem).Line;
                this.GherkinTextEditor.HighlightLine(i);
            }
        }

        void SetScenariosGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;
            
            viewCols.Add(new GridColView() { Field = GherkinScenarioDefinition.Fields.Name, WidthWeight = 30, BindingMode = System.Windows.Data.BindingMode.OneWay });
            viewCols.Add(new GridColView() { Field = GherkinScenarioDefinition.Fields.Description, WidthWeight = 30, BindingMode = System.Windows.Data.BindingMode.OneWay });
            viewCols.Add(new GridColView() { Field = GherkinScenarioDefinition.Fields.Keyword, WidthWeight = 30, BindingMode = System.Windows.Data.BindingMode.OneWay });

            ScenariosGrid.SetAllColumnsDefaultView(view);
            ScenariosGrid.InitViewItems();

            ScenariosGrid.RowChangedEvent += ScenariosGrid_RowChangedEvent;
            ScenariosGrid.btnDelete.Click += BtnDelete_Click;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ScenariosGrid_RowChangedEvent(object sender, EventArgs e)
        {
            if (ScenariosGrid.CurrentItem != null)
            {
                int line = ((GherkinScenarioDefinition)ScenariosGrid.CurrentItem).ScenarioDefintion.Location.Line;
                GherkinTextEditor.HighlightLine(line);
            }
        }

        void SetOptimizedGridView()
        {
            if (isBFexists) { UpdateBFButton.Content = "Create Business Flow"; } else { UpdateBFButton.Content = "Update Business Flow"; }
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;
            
            viewCols.Add(new GridColView() { Field = GherkinStep.Fields.Text, WidthWeight = 200, BindingMode = System.Windows.Data.BindingMode.OneWay });
            viewCols.Add(new GridColView() { Field = GherkinStep.Fields.Counter, WidthWeight = 30, BindingMode = System.Windows.Data.BindingMode.OneWay });
            viewCols.Add(new GridColView() { Field = GherkinStep.Fields.AutomationStatus, WidthWeight = 60, BindingMode = System.Windows.Data.BindingMode.OneWay });

            OptimizedStepsGrid.SetAllColumnsDefaultView(view);
            OptimizedStepsGrid.InitViewItems();
        }

        void SetTagsGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;

            viewCols.Add(new GridColView() { Field = GherkinTag.Fields.Name, WidthWeight = 200, BindingMode = System.Windows.Data.BindingMode.OneWay });
            viewCols.Add(new GridColView() { Field = GherkinTag.Fields.Line, WidthWeight = 30, BindingMode = System.Windows.Data.BindingMode.OneWay });
            viewCols.Add(new GridColView() { Field = GherkinTag.Fields.Column, WidthWeight = 30, BindingMode = System.Windows.Data.BindingMode.OneWay });

            TagsGrid.SetAllColumnsDefaultView(view);
            TagsGrid.InitViewItems();

            TagsGrid.RowChangedEvent += TagsGrid_RowChangedEvent;
        }

        private void TagsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            if (TagsGrid.CurrentItem != null)
            {
                int line = ((GherkinTag)TagsGrid.CurrentItem).Line;
                GherkinTextEditor.HighlightLine(line);
            }
        }

        void SetStepsGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;

            viewCols.Add(new GridColView() { Field = GherkinStep.Fields.Text, WidthWeight = 200, BindingMode = System.Windows.Data.BindingMode.OneWay });            
            viewCols.Add(new GridColView() { Field = GherkinStep.Fields.AutomationStatus, WidthWeight = 60, BindingMode = System.Windows.Data.BindingMode.OneWay });

            StepsGrid.SetAllColumnsDefaultView(view);
            StepsGrid.InitViewItems();

            StepsGrid.RowChangedEvent += StepsGrid_RowChangedEvent;
        }

        private void StepsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            if (StepsGrid.CurrentItem != null)
            {
                int line = ((GherkinStep)StepsGrid.CurrentItem).Step.Location.Line;
                GherkinTextEditor.HighlightLine(line);
            }
        }
    }
}