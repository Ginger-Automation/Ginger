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
using static Ginger.ConflictResolve.Conflict;
using GingerWPF.BusinessFlowsLib;
using Amdocs.Ginger.Common.SourceControlLib;
using GingerWPF.WizardLib;
using GingerCore.GeneralLib;
using System.Globalization;

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
        ObservableList<Conflict> conflictResolves = new ObservableList<Conflict>();
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
            resolveBtn.Click += new RoutedEventHandler(resolve_Click);

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, "Source Control Conflicts", this, new ObservableList<Button> { resolveBtn }, true, "Do Not Resolve", CloseWindow);
        }
        private void resolve_Click(object sender, EventArgs e)
        {
            IsResolved = true;
            Reporter.ToStatus(eStatusMsgKey.ResolveSourceControlConflicts);
            foreach (Conflict conflict in conflictResolves)
            {
                switch (conflict.Resolution)
                {
                    case ResolutionType.AcceptServer:
                        SourceControlIntegration.ResolveConflicts(WorkSpace.Instance.Solution.SourceControl, conflict.Path, eResolveConflictsSide.Server);
                        break;
                    case ResolutionType.KeepLocal:
                        SourceControlIntegration.ResolveConflicts(WorkSpace.Instance.Solution.SourceControl, conflict.Path, eResolveConflictsSide.Local);
                        break;
                    case ResolutionType.CherryPick:
                        string content = new NewRepositorySerializer().SerializeToString(conflict.GetMergedItem());
                        SourceControlIntegration.NewResolveConflict(WorkSpace.Instance.Solution.SourceControl, conflict.Path, content);
                        break;
                }
            }
            Reporter.HideStatusMessage();
            CloseWindow();
        }

        private void SetGridView()
        {
            foreach (string conflictPath in mConflictPaths)
            {
                Conflict conflict = new(conflictPath)
                {
                    Resolution = ResolutionType.AcceptServer
                };
                conflictResolves.Add(conflict);
            }

            GridViewDef view = new(GridViewDef.DefaultViewName)
            {
                GridColsView = new()
                {
                    new GridColView()
                    {
                        Field = nameof(Conflict.RelativePath),
                        Header = "Conflicted File",
                        WidthWeight = 150,
                        AllowSorting = true,
                        BindingMode = BindingMode.OneWay,
                        ReadOnly = true
                    },
                    new GridColView()
                    {
                        Field = nameof(Conflict.Resolution),
                        Header = "Operation",
                        WidthWeight = 90,
                        BindingMode = BindingMode.TwoWay,
                        StyleType = GridColView.eGridColStyleType.Template,
                        CellTemplate = CreateDataTemplateForCherryPickChangesColumn()
                    }
                    //new GridColView() 
                    //{ 
                    //    Field = nameof(ConflictResolve.resolveOperations), 
                    //    Header = "Operation", 
                    //    WidthWeight = 90, 
                    //    BindingMode = BindingMode.TwoWay, 
                    //    StyleType = GridColView.eGridColStyleType.Template, 
                    //    CellTemplate = ucGrid.GetGridComboBoxTemplate(
                    //        valuesList: GingerCore.General.GetEnumValuesForCombo(typeof(eResolveOperations)), 
                    //        selectedValueField: nameof(ConflictResolve.resolveOperations), 
                    //        allowEdit: false, 
                    //        selectedByDefault: true) 
                    //},
                    //new GridColView() 
                    //{ 
                    //    Field = "Compare and Merge", 
                    //    WidthWeight = 20, 
                    //    StyleType = GridColView.eGridColStyleType.Template, 
                    //    CellTemplate = (DataTemplate)this.ResolveConflictsGrid.Resources["xCompareAndMergeTemplate"] 
                    //}
                }
            };

            xConflictingItemsGrid.SetAllColumnsDefaultView(view);
            xConflictingItemsGrid.InitViewItems();
            xConflictingItemsGrid.SetTitleLightStyle = true;
            xConflictingItemsGrid.DataSourceList = this.conflictResolves;
        }

        private DataTemplate CreateDataTemplateForCherryPickChangesColumn()
        {
            DataTemplate dataTemplate = new();
            FrameworkElementFactory operationsComboBox = new(typeof(ComboBox));

            operationsComboBox.SetValue(ComboBox.ItemsSourceProperty, GingerCore.General.GetEnumValuesForCombo(typeof(ResolutionType)));
            operationsComboBox.SetValue(ComboBox.DisplayMemberPathProperty, nameof(ComboEnumItem.text));
            operationsComboBox.SetValue(ComboBox.SelectedValuePathProperty, nameof(ComboEnumItem.Value));
            operationsComboBox.SetValue(ComboBox.SelectedIndexProperty, 0);

            Binding operationsComboBoxSelectedValueBinding = new(path: nameof(Conflict.Resolution))
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            
            operationsComboBox.SetBinding(ComboBox.SelectedValueProperty, operationsComboBoxSelectedValueBinding);

            FrameworkElementFactory viewChangesButton = new(typeof(Button));

            viewChangesButton.SetValue(Button.ContentProperty, "View Changes");
            viewChangesButton.SetValue(Button.StyleProperty, Application.Current.Resources["@InputImageGridCellButtonStyle"]);
            viewChangesButton.AddHandler(Button.ClickEvent, new RoutedEventHandler(xCompareAndMergeButton_Click));

            Binding viewChangesButtonVisibilityBinding = new(path: nameof(Conflict.Resolution))
            {
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new ResolveOperationToButtonVisibilityConverter()
            };

            viewChangesButton.SetBinding(Button.VisibilityProperty, viewChangesButtonVisibilityBinding);

            FrameworkElementFactory stackPanel = new(typeof(StackPanel));
            stackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            stackPanel.AppendChild(operationsComboBox);
            stackPanel.AppendChild(viewChangesButton);

            dataTemplate.VisualTree = stackPanel;

            return dataTemplate;
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


        private void xCompareAndMergeButton_Click(object sender, RoutedEventArgs e)
        {
            Conflict conflict = (Conflict)((FrameworkElement)sender).DataContext;
            ResolveMergeConflictWizard wizard = new(conflict);
            WizardWindow.ShowWizard(wizard);

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

        private sealed class ResolveOperationToButtonVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if(value is Conflict.ResolutionType resolveOperation && resolveOperation == Conflict.ResolutionType.CherryPick)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
