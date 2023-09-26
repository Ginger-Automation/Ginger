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
using Amdocs.Ginger.UserControls;
using Amdocs.Ginger.Core;
using Amdocs.Ginger.Common.Enums;

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


        public ResolveConflictWindow(List<string> conflictPaths)
        {
            IsResolved = false;
            InitializeComponent();
            _conflicts = CreateConflictList(conflictPaths);
            SetGridView();
        }

        private ObservableList<Conflict> CreateConflictList(IEnumerable<string> conflictPaths)
        {
            ObservableList<Conflict> conflicts = new();
            foreach (string conflictPath in conflictPaths)
            {
                Conflict conflict = new(conflictPath)
                {
                    IsSelectedForResolution = true,
                    Resolution = ResolutionType.KeepLocal
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
                if(!conflict.IsSelectedForResolution || !conflict.CanResolve)
                {
                    continue;
                }

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
                        bool hasMergedItem = conflict.TryGetMergedItem(out RepositoryItemBase? mergedItem);
                        if(!hasMergedItem)
                        {
                            throw new InvalidOperationException($"No merged item available for file {conflict.Path}.");
                        }
                        if (mergedItem != null)
                        {
                            string content = serializer.SerializeToString(mergedItem);
                            SourceControlIntegration.ResolveConflictWithContent(WorkSpace.Instance.Solution.SourceControl, conflict.Path, content);
                        }
                        else
                        {
                            //in case of a Modify-Delete conflict, user might choose to not keep the modified file, so delete the file.
                            SourceControlIntegration.DeleteFile(WorkSpace.Instance.Solution.SourceControl, conflict.Path);
                        }
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
                if(!conflict.CanResolve)
                {
                    continue;
                }

                AnalyzeForConflict(conflict);
            }
        }

        private void AnalyzeForConflict(Conflict conflict)
        {
            if (!IsBusinessFlowFile(conflict.Path))
            {
                return;
            }

            if (conflict.Resolution == ResolutionType.CherryPick)
            {
                bool hasMergedItem = conflict.TryGetMergedItem(out RepositoryItemBase? mergedItem);
                if (hasMergedItem && mergedItem != null && mergedItem is BusinessFlow businessFlow)
                {
                    Task.Run(() => AnalyzeBusinessFlow(businessFlow));
                }
            }
        }

        private bool IsBusinessFlowFile(string filePath)
        {
            return filePath.EndsWith(".BusinessFlow.xml");
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
                }
                //else
                //{
                //    Reporter.ToUser(eUserMsgKey.AnalyzerFoundNoIssues);
                //}
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
                        Field = nameof(Conflict.IsSelectedForResolution),
                        Header = "Selected",
                        WidthWeight = 16,
                        StyleType = GridColView.eGridColStyleType.Template,
                        CellTemplate = ucGrid.GetGridCheckBoxTemplate(
                            selectedValueField: nameof(Conflict.IsSelectedForResolution), 
                            style: (Style)FindResource("@GridCellCheckBoxStyle"))
                    },
                    new GridColView()
                    {
                        Field = nameof(Conflict.RelativePath),
                        Header = "Conflicted File",
                        WidthWeight = 128,
                        AllowSorting = true,
                        BindingMode = BindingMode.OneWay,
                        ReadOnly = true
                    },
                    new GridColView()
                    {
                        Field = nameof(Conflict.Resolution),
                        Header = "Operation",
                        WidthWeight = 64,
                        BindingMode = BindingMode.TwoWay,
                        StyleType = GridColView.eGridColStyleType.Template,
                        CellTemplate = GetDataTemplateForOperationCell()
                    },
                    new GridColView()
                    {
                        Field = nameof(Conflict.CanResolve),
                        Header = "Ready For Resolution",
                        WidthWeight = 64,
                        BindingMode = BindingMode.TwoWay,
                        StyleType = GridColView.eGridColStyleType.Template,
                        CellTemplate = GetDataTemplateForReadyForResolutionCell()
                    }
                }
            };

            xConflictingItemsGrid.SetAllColumnsDefaultView(view);
            xConflictingItemsGrid.InitViewItems();
            xConflictingItemsGrid.SetTitleLightStyle = true;
            xConflictingItemsGrid.AddToolbarTool("@Checkbox_16x16.png", "Select/Unselect all", xConflictingItemsGrid_Toolbar_SelectUnselectAll);
            xConflictingItemsGrid.AddToolbarTool("@DropDownList_16x16.png", "Set resolution to all", xConflictingItemsGrid_Toolbar_SetSameResolutionToAll);
            xConflictingItemsGrid.DataSourceList = this._conflicts;
        }

        private DataTemplate GetDataTemplateForOperationCell()
        {
            return (DataTemplate)ResolveConflictsGrid.Resources["xOperationCellTemplate"];
        }

        public static List<ComboEnumItem> GetResolutionItemsSource()
        {
            //do not remove this method, it is actually called from XAML
            List<ComboEnumItem> resolutionOptions = GingerCore.General.GetEnumValuesForCombo(typeof(ResolutionType));
            return resolutionOptions;
        }

        private DataTemplate GetDataTemplateForReadyForResolutionCell()
        {
            return (DataTemplate)ResolveConflictsGrid.Resources["xReadyForResolutionCellTemplate"];
        }

        private void xConflictingItemsGrid_Toolbar_SelectUnselectAll(object? sender, RoutedEventArgs e)
        {
            bool isAnyUnselected = _conflicts.Any(conflict => !conflict.IsSelectedForResolution);

            foreach(Conflict conflict in _conflicts)
            {
                if (isAnyUnselected)
                {
                    //select all
                    conflict.IsSelectedForResolution = true;
                }
                else
                {
                    //unselect all
                    conflict.IsSelectedForResolution = false;
                }
            }
        }

        private void xConflictingItemsGrid_Toolbar_SetSameResolutionToAll(object? sender, RoutedEventArgs e)
        {
            if(xConflictingItemsGrid.CurrentItem != null && xConflictingItemsGrid.CurrentItem is Conflict selectedConflict)
            {
                Conflict.ResolutionType selectedItemResolutionType = selectedConflict.Resolution;
                foreach(Conflict conflict in _conflicts)
                {
                    conflict.Resolution = selectedItemResolutionType;
                }
            }
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
    }

    public sealed class ResolutionToCompareVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Conflict.ResolutionType resolveOperation && resolveOperation == Conflict.ResolutionType.CherryPick)
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
