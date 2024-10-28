﻿using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.CoreNET.External.Katalon.Conversion;
using Ginger.UserControls;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
                eImageType.Browser,
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

            KatalonConvertedPOMViewModel highlightedItem = (KatalonConvertedPOMViewModel)ImportedPOMGrid.CurrentItem;

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

            KatalonConvertedPOMViewModel highlightedItem = (KatalonConvertedPOMViewModel)ImportedPOMGrid.CurrentItem;

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
                    _ = ImportPOMsAsync();
                    break;
                default:
                    break;
            }
        }

        private async Task ImportPOMsAsync()
        {
            try
            {
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
}
