#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.AnalyzerLib;
using Ginger.Run;
using Ginger.SourceControl;
using Ginger.UserControls;
using GingerCore;
using GingerCoreNET.SourceControl;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static Ginger.ConflictResolve.Conflict;

namespace Ginger.ConflictResolve
{
    /// <summary>
    /// Interaction logic for ResolveConflictWindow.xaml
    /// </summary>
    public partial class ResolveConflictWindow : Page
    {
        private GenericWindow? _genericWindow = null;
        private readonly ObservableList<Conflict> _conflicts;
        private ImageMakerControl _genericWindowLoaderIcon;
        private Button _resolveButton;
        private Button _analyzeButton;

        public bool IsResolved { get; private set; }


        public ResolveConflictWindow(List<string> conflictPaths)
        {
            IsResolved = false;
            InitializeComponent();
            _conflicts = CreateConflictList(conflictPaths);
            SetGridView();
            CreateControlsForGenericWindow();
        }

        private ObservableList<Conflict> CreateConflictList(IEnumerable<string> conflictPaths)
        {
            ObservableList<Conflict> conflicts = [];
            foreach (string conflictPath in conflictPaths)
            {
                bool isXMLFile = conflictPath.EndsWith(".xml");
                IEnumerable<ResolutionType> possibleResolutions;
                if (isXMLFile)
                {
                    possibleResolutions = Enum.GetValues<ResolutionType>();
                }
                else
                {
                    possibleResolutions = [ResolutionType.KeepLocal, ResolutionType.AcceptServer];
                }

                Conflict conflict = new(conflictPath, possibleResolutions)
                {
                    IsSelectedForResolution = true,
                    Resolution = ResolutionType.KeepLocal
                };
                conflicts.Add(conflict);
            }
            return conflicts;
        }

        [MemberNotNull(nameof(_genericWindowLoaderIcon), nameof(_resolveButton), nameof(_analyzeButton))]
        private void CreateControlsForGenericWindow()
        {
            _resolveButton = new()
            {
                Content = "Resolve",
                ToolTip = "Resolve selected conflicts"
            };
            _resolveButton.Click += new RoutedEventHandler(resolve_Click);

            _analyzeButton = new()
            {
                Content = "Analyze",
                ToolTip = "Analyze selected conflicts"
            };
            _analyzeButton.Click += analyzeBtn_Click;

            _genericWindowLoaderIcon = new ImageMakerControl()
            {
                Name = "xProcessingImage",
                Height = 30,
                Width = 30,
                ImageType = eImageType.Processing,
                Visibility = Visibility.Collapsed,
            };
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            GingerCore.General.LoadGenericWindow(
                ref _genericWindow,
                owner: App.MainWindow,
                windowStyle,
                windowTitle: "Source Control Conflicts",
                windowPage: this,
                windowBtnsList: [_resolveButton, _analyzeButton],
                showClosebtn: true,
                closeBtnText: "Do Not Resolve",
                closeEventHandler: CloseWindow,
                loaderElement: _genericWindowLoaderIcon);
        }

        private void resolve_Click(object sender, EventArgs e)
        {
            try
            {
                _genericWindowLoaderIcon.Visibility = Visibility.Visible;
                Reporter.ToStatus(eStatusMsgKey.ResolveSourceControlConflicts);
                List<Conflict> resolvedConflicts = [];
                foreach (Conflict conflict in _conflicts)
                {
                    bool wasConflictResolved = ResolveConflict(conflict);
                    if (wasConflictResolved)
                    {
                        resolvedConflicts.Add(conflict);
                    }
                }

                if (resolvedConflicts.Count == _conflicts.Count)
                {
                    IsResolved = true;
                }
                else
                {
                    IsResolved = false;
                }

                resolvedConflicts.ForEach(resolvedConflict => _conflicts.Remove(resolvedConflict));

                Reporter.ToUser(eUserMsgKey.ConflictsResolvedCount, resolvedConflicts.Count);
            }
            finally
            {
                _genericWindowLoaderIcon.Visibility = Visibility.Collapsed;
                Reporter.HideStatusMessage();
                if (IsResolved)
                {
                    CloseWindow();
                }
            }
        }

        private bool ResolveConflict(Conflict conflict)
        {
            if (!conflict.IsSelectedForResolution || !conflict.CanResolve)
            {
                return false;
            }

            bool wasConflictResolved = false;
            try
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
                        bool hasMergedItem = conflict.TryGetMergedItem(out RepositoryItemBase? mergedItem);
                        if (!hasMergedItem)
                        {
                            throw new InvalidOperationException($"Merge Conflict failed for file '{conflict.Path}', please retry again or choose other options like 'Keep Local' or 'Accept Server'.");
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
                wasConflictResolved = true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while resolving conflict for file '{conflict.Path}'.", ex);
            }

            return wasConflictResolved;
        }

        private void analyzeBtn_Click(object sender, RoutedEventArgs e)
        {
            AnalyzeSelectedConflict();
        }

        private void AnalyzeSelectedConflict()
        {
            Conflict selectedConflict = (Conflict)xConflictingItemsGrid.CurrentItem;
            if (selectedConflict == null)
            {
                Reporter.ToUser(eUserMsgKey.IssueWhileAnalyzingConflict, "No conflict found for analyzing.");
                return;
            }

            bool cannotResolve = !selectedConflict.CanResolve;
            if (cannotResolve)
            {
                Reporter.ToUser(eUserMsgKey.IssueWhileAnalyzingConflict, "Conflict is not ready for resolution, hence cannot be analyzed.");
                return;
            }

            RepositoryItemBase? itemForResolution;
            if (selectedConflict.Resolution == ResolutionType.AcceptServer)
            {
                itemForResolution = selectedConflict.GetRemoteItem();
            }
            else if (selectedConflict.Resolution == ResolutionType.KeepLocal)
            {
                itemForResolution = selectedConflict.GetLocalItem();
            }
            else
            {
                selectedConflict.TryGetMergedItem(out itemForResolution);
            }

            //if (itemForResolution == null)
            //{
            //    Reporter.ToUser(eUserMsgKey.IssueWhileAnalyzingConflict, "No merged item available for conflict for analyzing.");
            //    return;
            //}

            AnalyzeRepositoryItemBase(itemForResolution);
        }

        private void AnalyzeRepositoryItemBase(RepositoryItemBase? item)
        {
            if (item is BusinessFlow businessFlow)
            {
                Task.Run(() => AnalyzeBusinessFlow(businessFlow));
            }
            else if (item is RunSetConfig runSetConfig)
            {
                Task.Run(() => AnalyzeRunSetConfig(runSetConfig));
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.IssueWhileAnalyzingConflict, "Conflict analyzation is not available for this entity type.");
            }
        }

        private async Task AnalyzeBusinessFlow(BusinessFlow businessFlow)
        {
            try
            {
                Dispatcher.Invoke(() => _genericWindowLoaderIcon.Visibility = Visibility.Visible);
                Reporter.ToStatus(eStatusMsgKey.AnalyzerIsAnalyzing, null, businessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                AnalyzerPage analyzerPage = null!;

                Dispatcher.Invoke(() => analyzerPage = new());

                analyzerPage.Init(businessFlow, WorkSpace.Instance.Solution, applicationAgents: null, WorkSpace.Instance.AutomateTabSelfHealingConfiguration.AutoFixAnalyzerIssue);
                await analyzerPage.AnalyzeWithoutUI();
                Dispatcher.Invoke(() => Reporter.HideStatusMessage());
                Dispatcher.Invoke(() => analyzerPage.ShowAsWindow());
            }
            finally
            {
                Dispatcher.Invoke(() => _genericWindowLoaderIcon.Visibility = Visibility.Collapsed);
                Dispatcher.Invoke(() => Reporter.HideStatusMessage());
            }
        }

        private async Task AnalyzeRunSetConfig(RunSetConfig runSetConfig)
        {
            try
            {
                Dispatcher.Invoke(() => _genericWindowLoaderIcon.Visibility = Visibility.Visible);
                Reporter.ToStatus(eStatusMsgKey.AnalyzerIsAnalyzing, null, runSetConfig.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                AnalyzerPage analyzerPage = null!;

                Dispatcher.Invoke(() => analyzerPage = new());

                analyzerPage.Init(runSetConfig, WorkSpace.Instance.Solution);
                await analyzerPage.AnalyzeWithoutUI();
                Dispatcher.Invoke(() => Reporter.HideStatusMessage());
                if (analyzerPage.TotalHighAndCriticalIssues > 0)
                {
                    Reporter.ToUser(eUserMsgKey.AnalyzerFoundIssues);
                    Dispatcher.Invoke(() => analyzerPage.ShowAsWindow());
                }
            }
            finally
            {
                Dispatcher.Invoke(() => _genericWindowLoaderIcon.Visibility = Visibility.Collapsed);
                Dispatcher.Invoke(() => Reporter.HideStatusMessage());
            }
        }

        private void SetGridView()
        {
            GridViewDef view = new(GridViewDef.DefaultViewName)
            {
                GridColsView =
                [
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
                ]
            };

            xConflictingItemsGrid.SetAllColumnsDefaultView(view);
            xConflictingItemsGrid.InitViewItems();
            xConflictingItemsGrid.SetTitleLightStyle = true;
            xConflictingItemsGrid.AddToolbarTool("@Checkbox_16x16.png", "Select/Unselect all", xConflictingItemsGrid_Toolbar_SelectUnselectAll);
            xConflictingItemsGrid.AddToolbarTool("@DropDownList_16x16.png", "Set resolution to all", xConflictingItemsGrid_Toolbar_SetSameResolutionToAll);
            xConflictingItemsGrid.DataSourceList = _conflicts;
        }

        private DataTemplate GetDataTemplateForOperationCell()
        {
            return (DataTemplate)ResolveConflictsGrid.Resources["xOperationCellTemplate"];
        }

        private DataTemplate GetDataTemplateForReadyForResolutionCell()
        {
            return (DataTemplate)ResolveConflictsGrid.Resources["xReadyForResolutionCellTemplate"];
        }

        private void xConflictingItemsGrid_Toolbar_SelectUnselectAll(object? sender, RoutedEventArgs e)
        {
            bool isAnyUnselected = _conflicts.Any(conflict => !conflict.IsSelectedForResolution);

            foreach (Conflict conflict in _conflicts)
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
            if (xConflictingItemsGrid.CurrentItem is not null and Conflict selectedConflict)
            {
                Conflict.ResolutionType selectedItemResolutionType = selectedConflict.Resolution;
                foreach (Conflict conflict in _conflicts)
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
            if (_genericWindow != null)
            {
                _genericWindow.Close();
            }
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
