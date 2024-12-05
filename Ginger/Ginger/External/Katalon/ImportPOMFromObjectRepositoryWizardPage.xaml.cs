#region License
/*
Copyright © 2014-2024 European Support Limited

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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.CoreNET.External.Katalon.Conversion;
using Ginger.UserControls;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.External.Katalon
{
    /// <summary>
    /// Interaction logic for ImportPOMFromObjectRepositoryWizardPage.xaml
    /// </summary>
    public partial class ImportPOMFromObjectRepositoryWizardPage : Page, IWizardPage
    {
        private readonly ImportKatalonObjectRepositoryWizard _wizard;
        private readonly ObservableList<KatalonObjectRepositoryToPOMConverter.Result> _conversionResults;

        public ImportPOMFromObjectRepositoryWizardPage(ImportKatalonObjectRepositoryWizard wizard)
        {
            InitializeComponent();

            _wizard = wizard;
            _conversionResults = [];
            _conversionResults.CollectionChanged += ConversionResults_CollectionChanged;

            InitImportedPOMGrid();
        }

        private void InitImportedPOMGrid()
        {
            GridViewDef view = new(GridViewDef.DefaultViewName)
            {
                GridColsView =
                [
                    new()
                    {
                        Field = nameof(KatalonConvertedPOMViewModel.Active),
                        Header = "Active",
                        WidthWeight = 10,
                        StyleType = GridColView.eGridColStyleType.CheckBox,
                    },
                    new()
                    {
                        Field = nameof(KatalonConvertedPOMViewModel.Name),
                        Header = "Name",
                        ReadOnly = true,
                        WidthWeight = 20,
                        StyleType = GridColView.eGridColStyleType.Text,
                    },
                    new()
                    {
                        Field = nameof(KatalonConvertedPOMViewModel.TargetApplication),
                        Header = "Target Application",
                        WidthWeight = 20,
                        StyleType = GridColView.eGridColStyleType.Template,
                        CellTemplate = (DataTemplate)FindResource("TargetApplicationCellTemplate"),
                    },
                    new()
                    {
                        Field = nameof(KatalonConvertedPOMViewModel.URL),
                        Header = "URL",
                        WidthWeight = 20,
                        StyleType = GridColView.eGridColStyleType.Text,
                    }
                ]
            };

            ImportedPOMGrid.AddToolbarTool(
                "@CheckAllRow_16x16.png",
                "Select All",
                ImportedPOMGrid_Toolbar_SelectAllForSync);
            ImportedPOMGrid.AddToolbarTool(
                "@UnCheckAllRow_16x16.png",
                "Unselect All",
                ImportedPOMGrid_Toolbar_UnselectAllForSync);
            ImportedPOMGrid.AddToolbarTool(
                eImageType.Application,
                "Set highlighted Target Application for all",
                ImportedPOMGrid_Toolbar_SyncTargetApplication);
            ImportedPOMGrid.AddToolbarTool(
                eImageType.Globe,
                "Set highlighted URL for all",
                ImportedPOMGrid_Toolbar_SyncURL);

            ImportedPOMGrid.SetAllColumnsDefaultView(view);
            ImportedPOMGrid.InitViewItems();
            ImportedPOMGrid.DataSourceList = _wizard.POMViewModels;
        }

        private void ImportedPOMGrid_Toolbar_SelectAllForSync(object? sender, RoutedEventArgs e)
        {
            IEnumerable<KatalonConvertedPOMViewModel> visibleItems = ImportedPOMGrid
                .GetFilteredItems()
                .Cast<KatalonConvertedPOMViewModel>();

            foreach (KatalonConvertedPOMViewModel item in visibleItems)
            {
                item.Active = true;
            }
        }

        private void ImportedPOMGrid_Toolbar_UnselectAllForSync(object? sender, RoutedEventArgs e)
        {
            IEnumerable<KatalonConvertedPOMViewModel> visibleItems = ImportedPOMGrid
                .GetFilteredItems()
                .Cast<KatalonConvertedPOMViewModel>();

            foreach (KatalonConvertedPOMViewModel item in visibleItems)
            {
                item.Active = false;
            }
        }

        private void ImportedPOMGrid_Toolbar_SyncTargetApplication(object? sender, RoutedEventArgs e)
        {
            IEnumerable<KatalonConvertedPOMViewModel> visibleItems = ImportedPOMGrid
                .GetFilteredItems()
                .Cast<KatalonConvertedPOMViewModel>();

            KatalonConvertedPOMViewModel? highlightedItem = (KatalonConvertedPOMViewModel)ImportedPOMGrid.CurrentItem;
            if (highlightedItem == null)
            {
                return;
            }

            foreach (KatalonConvertedPOMViewModel item in visibleItems)
            {
                if (!item.Active)
                {
                    continue;
                }

                bool highlightedTargetAppIsValid = item
                    .TargetApplicationOptions
                    .Any(t => string.Equals(t, highlightedItem.TargetApplication));

                if (highlightedTargetAppIsValid)
                {
                    item.TargetApplication = highlightedItem.TargetApplication;
                }
            }
        }

        private void ImportedPOMGrid_Toolbar_SyncURL(object? sender, RoutedEventArgs e)
        {
            IEnumerable<KatalonConvertedPOMViewModel> visibleItems = ImportedPOMGrid
                .GetFilteredItems()
                .Cast<KatalonConvertedPOMViewModel>();

            KatalonConvertedPOMViewModel? highlightedItem = (KatalonConvertedPOMViewModel)ImportedPOMGrid.CurrentItem;
            if (highlightedItem == null)
            {
                return;
            }

            foreach (KatalonConvertedPOMViewModel item in visibleItems)
            {
                if (!item.Active)
                {
                    continue;
                }

                item.URL = highlightedItem.URL;
            }
        }

        public void WizardEvent(WizardEventArgs e)
        {
            switch (e.EventType)
            {
                case EventType.Active:
                    _wizard.mWizardWindow?.SetFinishButtonEnabled(false);
                    _ = ImportPOMsAsync();
                    break;
                case EventType.LeavingForNextPage:
                    bool hasAnyInvalidPOM = false;
                    foreach (KatalonConvertedPOMViewModel pom in _wizard.POMViewModels)
                    {
                        if (pom.Active && !pom.IsValid())
                        {
                            hasAnyInvalidPOM = true;
                            pom.ShowAllErrorHighlights();
                        }
                    }
                    e.CancelEvent = hasAnyInvalidPOM;
                    if (!hasAnyInvalidPOM)
                    {
                        _wizard.AddPOMs();
                    }
                    break;
                default:
                    break;
            }
        }

        private async Task ImportPOMsAsync()
        {
            try
            {
                _wizard.ProcessStarted();
                _wizard.POMViewModels.ClearAll();
                _conversionResults.ClearAll();
                ImportedPOMGrid.DisableGridColoumns();
                await KatalonObjectRepositoryToPOMConverter.ConvertAsync(_wizard.SelectedDirectory, _conversionResults);
                ImportedPOMGrid.EnableGridColumns();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error while importing Katalon Object-Repository as Ginger POM", ex);
            }
            finally
            {
                _wizard.ProcessEnded();
            }
        }

        private void ConversionResults_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add)
            {
                return;
            }

            if (e.NewItems == null ||
                e.NewItems.Count <= 0 ||
                e.NewItems[0] is not KatalonObjectRepositoryToPOMConverter.Result conversionResult)
            {
                return;
            }

            _wizard.POMViewModels.Add(new(conversionResult.POM, conversionResult.Platform));
        }
    }

    public sealed class BoolToErrorBorderThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? new Thickness(uniformLength: 1) : new Thickness(uniformLength: 0);
            }
            return new Thickness(uniformLength: 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
