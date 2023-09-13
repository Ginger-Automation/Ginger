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

namespace Ginger.ConflictResolve
{
    /// <summary>
    /// Interaction logic for ResolveConflictWindow.xaml
    /// </summary>
    public partial class ResolveConflictWindow : Page
    {
        GenericWindow genWin = null;
        List<string> mConflictPaths;
        ObservableList<Conflict> conflictResolves = new ObservableList<Conflict>();
        private readonly ResolutionType _defaultResolutionType;

        public bool IsResolved { get; set; }


        public ResolveConflictWindow(List<string> conflictPaths, ResolutionType defaultResolutionType = ResolutionType.AcceptServer)
        {
            IsResolved = false;
            InitializeComponent();
            mConflictPaths = conflictPaths;
            _defaultResolutionType = defaultResolutionType;
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
                        NewRepositorySerializer serializer = new();
                        string content = serializer.SerializeToString(conflict.GetMergedItem());
                        SourceControlIntegration.ResolveConflictWithContent(WorkSpace.Instance.Solution.SourceControl, conflict.Path, content);
                        break;
                    default:
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
                    Resolution = _defaultResolutionType
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
            genWin.Close();
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
