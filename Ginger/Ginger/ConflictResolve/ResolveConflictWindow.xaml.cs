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
using System.Linq;
using Ginger.AnalyzerLib;
using System.Threading.Tasks;

namespace Ginger.ConflictResolve
{
    /// <summary>
    /// Interaction logic for ResolveConflictWindow.xaml
    /// </summary>
    public partial class ResolveConflictWindow : Page
    {
        private GenericWindow? _genericWindow = null;
        private readonly ObservableList<Conflict> _conflicts;

        public bool IsResolved { get; private set; }


        public ResolveConflictWindow(List<string> conflictPaths, ResolutionType defaultResolutionType = ResolutionType.AcceptServer)
        {
            IsResolved = false;
            InitializeComponent();
            _conflicts = CreateConflictList(conflictPaths, defaultResolutionType);
            SetGridView();
        }

        private ObservableList<Conflict> CreateConflictList(IEnumerable<string> conflictPaths, ResolutionType defaultResolutionType)
        {
            ObservableList<Conflict> conflicts = new();
            foreach (string conflictPath in conflictPaths)
            {
                Conflict conflict = new(conflictPath)
                {
                    Resolution = defaultResolutionType
                };
                conflicts.Add(conflict);
            }
            return conflicts;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button resolveBtn = new()
            {
                Content = "Resolve"
            };
            resolveBtn.Click += new RoutedEventHandler(resolve_Click);

            Button analyzeBtn = new()
            {
                Content = "Analyze",
            };
            analyzeBtn.Click += analyzeBtn_Click;

            GingerCore.General.LoadGenericWindow(ref _genericWindow, App.MainWindow, windowStyle, "Source Control Conflicts", this, new ObservableList<Button> { resolveBtn, analyzeBtn }, true, "Do Not Resolve", CloseWindow);
        }
        private void resolve_Click(object sender, EventArgs e)
        {
            Reporter.ToStatus(eStatusMsgKey.ResolveSourceControlConflicts);
            foreach (Conflict conflict in _conflicts)
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
                        NewRepositorySerializer serializer = new();
                        string content = serializer.SerializeToString(conflict.GetMergedItem());
                        SourceControlIntegration.ResolveConflictWithContent(WorkSpace.Instance.Solution.SourceControl, conflict.Path, content);
                        break;
                    default:
                        break;
                }
            }
            IsResolved = true;
            Reporter.HideStatusMessage();
            CloseWindow();
        }

        private void analyzeBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach(Conflict conflict in _conflicts)
            {
                AnalyzeForConflict(conflict);
            }
        }

        private void AnalyzeForConflict(Conflict conflict)
        {
            if(!IsBusinessFlowFile(conflict.Path))
            {
                return;
            }

            Task.Run(() => AnalyzeBusinessFlow((BusinessFlow)conflict.GetMergedItem()));
        }

        private bool IsBusinessFlowFile(string filePath)
        {
            return filePath.EndsWith("BusinessFlow.xml");
        }

        private async Task AnalyzeBusinessFlow(BusinessFlow businessFlow)
        {
            Reporter.ToStatus(eStatusMsgKey.AnalyzerIsAnalyzing, null, businessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
            try
            {
                AnalyzerPage analyzerPage = null!;

                Dispatcher.Invoke(() => analyzerPage = new());

                analyzerPage.Init(WorkSpace.Instance.Solution, businessFlow, WorkSpace.Instance.AutomateTabSelfHealingConfiguration.AutoFixAnalyzerIssue);
                await analyzerPage.AnalyzeWithoutUI();
                Dispatcher.Invoke(() => Reporter.HideStatusMessage());
                if (analyzerPage.TotalHighAndCriticalIssues > 0)
                {
                    Reporter.ToUser(eUserMsgKey.AnalyzerFoundIssues);
                    Dispatcher.Invoke(() => analyzerPage.ShowAsWindow());
                    return;
                }
            }
            finally
            {
                Dispatcher.Invoke(() => Reporter.HideStatusMessage());
            }
        }

        private void SetGridView()
        {
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
                }
            };

            xConflictingItemsGrid.SetAllColumnsDefaultView(view);
            xConflictingItemsGrid.InitViewItems();
            xConflictingItemsGrid.SetTitleLightStyle = true;
            xConflictingItemsGrid.DataSourceList = this._conflicts;
        }

        private DataTemplate CreateDataTemplateForCherryPickChangesColumn()
        {
            DataTemplate dataTemplate = new();
            FrameworkElementFactory operationsComboBox = new(typeof(ComboBox));

            List<ComboEnumItem> operationOptions = GingerCore.General.GetEnumValuesForCombo(typeof(ResolutionType));
            if(!WorkSpace.Instance.BetaFeatures.AllowMergeConflict)
            {
                ComboEnumItem cherryPickOption = operationOptions.First(option => (ResolutionType)option.Value == ResolutionType.CherryPick);
                operationOptions.Remove(cherryPickOption);
            }

            operationsComboBox.SetValue(ComboBox.ItemsSourceProperty, operationOptions);
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
            _genericWindow.Close();
        }


        private void xCompareAndMergeButton_Click(object sender, RoutedEventArgs e)
        {
            Conflict conflict = (Conflict)((FrameworkElement)sender).DataContext;
            ResolveMergeConflictWizard wizard = new(conflict);
            WizardWindow.ShowWizard(wizard);

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
