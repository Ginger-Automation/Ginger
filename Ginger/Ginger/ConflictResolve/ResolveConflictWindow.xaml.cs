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
using Amdocs.Ginger.CoreNET.ObjectCompare;
using Amdocs.Ginger.Repository;
using Ginger.SourceControl;
using Ginger.UserControls;
using GingerCore;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static Ginger.ConflictResolve.ConflictResolve;
using GingerWPF.BusinessFlowsLib;

namespace Ginger.ConflictResolve
{
    /// <summary>
    /// Interaction logic for ResolveConflictWindow.xaml
    /// </summary>
    public partial class ResolveConflictWindow : Page
    {
        GenericWindow genWin = null;
        List<string> mConflictPaths;
        public bool IsResolved { get; set; }
        ObservableList<ConflictResolve> conflictResolves = new ObservableList<ConflictResolve>();
        Dictionary<string, string> filePathDct = new Dictionary<string, string>();
        private string BranchName;
        public ResolveConflictWindow(List<string> conflictPaths, string branchName)
        {
            IsResolved = false;
            InitializeComponent();
            mConflictPaths = conflictPaths;
            BranchName = branchName;
            SetGridView();
        }
        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button resolveBtn = new Button();
            resolveBtn.Content = "Resolve";
            resolveBtn.Click += new RoutedEventHandler(xCompareAndMergeButton_Click);

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, "Source Control Conflicts", this, new ObservableList<Button> { resolveBtn }, true, "Do Not Resolve", CloseWindow);
        }
        private void resolve_Click(object sender, EventArgs e)
        {
            IsResolved = true;
            Reporter.ToStatus(eStatusMsgKey.ResolveSourceControlConflicts);
            foreach (ConflictResolve conflictResolve in conflictResolves)
            {
                switch (conflictResolve.resolveOperations)
                {
                    case eResolveOperations.AcceptServer:
                        SourceControlIntegration.ResolveConflicts(WorkSpace.Instance.Solution.SourceControl, conflictResolve.ConflictPath, eResolveConflictsSide.Server);
                        break;
                    case eResolveOperations.KeepLocal:
                        SourceControlIntegration.ResolveConflicts(WorkSpace.Instance.Solution.SourceControl, conflictResolve.ConflictPath, eResolveConflictsSide.Local);
                        break;
                    default:
                        //do nothing
                        break;
                }
            }
            Reporter.HideStatusMessage();
            CloseWindow();
        }

        private void SetGridView()
        {
            foreach (string conflictName in mConflictPaths)
            {
                ConflictResolve newObjConflict = new ConflictResolve();
                newObjConflict.ConflictPath = conflictName;
                newObjConflict.resolveOperations = eResolveOperations.AcceptServer;
                conflictResolves.Add(newObjConflict);
            }
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            viewCols.Add(new GridColView() { Field = nameof(ConflictResolve.RelativeConflictPath), Header = "Conflicted File", WidthWeight = 150, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            viewCols.Add(new GridColView() { Field = nameof(ConflictResolve.resolveOperations), Header = "Operation", WidthWeight = 90, BindingMode = BindingMode.TwoWay, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(GingerCore.General.GetEnumValuesForCombo(typeof(eResolveOperations)), nameof(ConflictResolve.resolveOperations), false, true) }); ;
            viewCols.Add(new GridColView() { Field = nameof(ConflictResolve.Merge), Header = "Comp. & Merge", StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xConflictingItemsGrid.Resources["xCompareAndMergeTemplate"] });
            view.GridColsView = viewCols;

            GridViewDef initView = new GridViewDef(GridViewDef.DefaultViewName);
            initView.GridColsView = new ObservableList<GridColView>();
            initView.GridColsView.Add(new GridColView() { Field = nameof(ConflictResolve.Merge), Header = "Comp. & Merge", Visible = false });



            xConflictingItemsGrid.SetAllColumnsDefaultView(view);

            xConflictingItemsGrid.AddCustomView(initView);
            xConflictingItemsGrid.ShowViewCombo = Visibility.Collapsed;

            xConflictingItemsGrid.ChangeGridView(initView.Name);
            xConflictingItemsGrid.InitViewItems();
            xConflictingItemsGrid.SetTitleLightStyle = true;
            xConflictingItemsGrid.DataSourceList = this.conflictResolves;
        }
        private void CloseWindow(object sender, EventArgs e)
        {
            IsResolved = false;
            CloseWindow();
        }
        private void CloseWindow()
        {
            genWin.Close();
        }
        private void xCompareAndMergeButton_Click(object sender, EventArgs e)
        {
            //MergeConflictWindow mergeConflictWindow = new MergeConflictWindow();
            //mergeConflictWindow.ShowAsWindow();
            string localPath = string.Empty;
            string serverPath = string.Empty;
            CreateSeparateLocalAndServerCopies(mConflictPaths[0], ref localPath, ref serverPath);
            string localXml = File.ReadAllText(localPath);
            BusinessFlow? localFlow = NewRepositorySerializer.DeserializeFromText(localXml) as BusinessFlow;
            string serverXml = File.ReadAllText(serverPath);
            BusinessFlow? serverFlow = NewRepositorySerializer.DeserializeFromText(serverXml) as BusinessFlow;

            List<string> objectCompare = ObjectCompare.CompareObjects(localFlow, serverFlow);
            BusinessFlowViewPage localFlowView = new(localFlow, new Context(), General.eRIPageViewMode.View, true, objectCompare);
            BusinessFlowViewPage serverFlowView = new(serverFlow, new Context(), General.eRIPageViewMode.View, true, objectCompare);
            localFlowView.mParallelView = serverFlowView;
            serverFlowView.mParallelView = localFlowView;
            localFlowView.EvtSyncTabChanged += serverFlowView.EvtTabChanged;
            serverFlowView.EvtSyncTabChanged += localFlowView.EvtTabChanged;
            CompareAndMergeConflictedItem compareAndMerge = new CompareAndMergeConflictedItem(localFlowView, serverFlowView, objectCompare);
            compareAndMerge.ShowAsWindow();
        }

        private void CreateSeparateLocalAndServerCopies(string filePath, ref string localXmlPath, ref string serverXmlPath)
        {
            try
            {
                string headText = "<<<<<<< HEAD";
                string equalText = "=======";
                //filePath = Path.Combine(RepositoryRootFolder, filePath);
                string[] fileOneLines = File.ReadAllLines(filePath);
                int headIndex = 0, equalIndex = 0, branchIndex = 0;
                List<string> lstLocalText = new List<string>();
                List<string> lstServerText = new List<string>();
                bool headIndexEncountered = false;
                string directoryName = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string extension = Path.GetExtension(filePath);
                for (int i = 0; i < fileOneLines.Length; i++)
                {
                    if (fileOneLines[i].Contains(headText))
                    {
                        headIndexEncountered = true;
                        headIndex = i;
                        continue;
                    }
                    if (fileOneLines[i].Contains(equalText))
                    {
                        equalIndex = i;
                        string localText = string.Empty;
                        for (int j = headIndex + 1; j < equalIndex; j++)
                        {
                            lstLocalText.Add(fileOneLines[j].Trim());
                        }
                        //lstLocalText.Add(localText);
                        //i = equalIndex + equalIndex - headIndex - 1;
                        continue;
                    }
                    if (fileOneLines[i].Contains(BranchName))
                    {
                        branchIndex = i;
                        string branchText = string.Empty;
                        for (int j = equalIndex + 1; j < branchIndex; j++)
                        {
                            lstServerText.Add(fileOneLines[j].Trim());
                        }
                        i = branchIndex;
                        headIndexEncountered = false;
                        continue;
                    }
                    if (!headIndexEncountered)
                    {
                        lstServerText.Add(fileOneLines[i].Trim());
                        lstLocalText.Add(fileOneLines[i].Trim());
                    }
                }
                localXmlPath = Path.Combine(directoryName, fileName + "_localChanges" + extension);
                CreateFile(localXmlPath, lstLocalText);
                serverXmlPath = Path.Combine(directoryName, fileName + "_serverChanges" + extension);
                CreateFile(serverXmlPath, lstServerText);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Issue in creating local and server files", ex);
            }
        }

        private void CreateFile(string fileName, List<string> xmlContent)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            using (StreamWriter sw = new StreamWriter(fileName, true))
            {
                for (int i = 0; i < xmlContent.Count; i++)
                {
                    string line = xmlContent[i];
                    sw.WriteLine(line);
                }
            }
        }
    }
}
