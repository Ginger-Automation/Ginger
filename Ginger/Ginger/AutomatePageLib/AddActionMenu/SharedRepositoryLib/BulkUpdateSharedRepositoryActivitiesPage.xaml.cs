#region License
/*
Copyright © 2014-2026 European Support Limited

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
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.UserControls;
using Ginger.BusinessFlowPages;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Activities;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.AutomatePageLib.AddActionMenu.SharedRepositoryLib
{
    /// <summary>
    /// Interaction logic for BulkUpdateSharedRepositoryActivitiesPage.xaml
    /// </summary>
    public partial class BulkUpdateSharedRepositoryActivitiesPage : Page
    {
        private readonly IEnumerable<ActivityBulkUpdateListItem> _activityBulkUpdateListItems;

        private Observable<Visibility>? _observableLoaderVisibility;
        private Observable<string>? _observableLoaderLabel;
        private GenericWindow? _window;

        public BulkUpdateSharedRepositoryActivitiesPage(IEnumerable<Activity> activities)
        {
            InitializeComponent();
            _activityBulkUpdateListItems = activities.Select(activity => new ActivityBulkUpdateListItem(activity)).ToList();
            InitBulkUpdateUCGrid();
            SetBulkUpdateUCGridItems(_activityBulkUpdateListItems);
            UpdateUIForPageMode();
        }

        private void InitBulkUpdateUCGrid()
        {
            GridViewDef gridViewDef = new(GridViewDef.DefaultViewName)
            {
                GridColsView =
                [
                    new GridColView()
                    {
                        Header = "Selected",
                        Field = nameof(ActivityBulkUpdateListItem.SelectedForSync),
                        WidthWeight = 30,
                        StyleType = GridColView.eGridColStyleType.Template,
                        CellTemplate = (DataTemplate)FindResource("SyncChangesCellTemplate")
                    },
                    new GridColView()
                    {
                        Header = "Name",
                        Field = nameof(ActivityBulkUpdateListItem.Name),
                        WidthWeight = 80,
                        BindingMode = BindingMode.TwoWay
                    },
                    new GridColView()
                    {
                        Header = "Publish",
                        Field = nameof(ActivityBulkUpdateListItem.Publish),
                        StyleType = GridColView.eGridColStyleType.Template,
                        CellTemplate = (DataTemplate)FindResource("PublishCellTemplate"),
                        WidthWeight = 30,
                    },
                    new GridColView()
                    {
                        Header = "Mandatory",
                        Field = nameof(ActivityBulkUpdateListItem.Mandatory),
                        StyleType = GridColView.eGridColStyleType.Template,
                        CellTemplate = (DataTemplate)FindResource("MandatoryCellTemplate"),
                        WidthWeight = 40
                    },
                    new GridColView()
                    {
                        Header = GingerDicser.GetTermResValue(eTermResKey.TargetApplication),
                        Field = nameof(ActivityBulkUpdateListItem.TargetApplication),
                        CellTemplate = (DataTemplate)FindResource("TargetApplicationCellTemplate"),
                        StyleType = GridColView.eGridColStyleType.Template,
                        WidthWeight = 60
                    },
                    new GridColView()
                    {
                        Header = "Consumers",
                        Field = nameof(ActivityBulkUpdateListItem.Consumers),
                        CellTemplate = (DataTemplate)FindResource("ConsumerCellTemplate"),
                        StyleType = GridColView.eGridColStyleType.Template,
                        WidthWeight = 60,
                        BindingMode = BindingMode.TwoWay,
                    },
                    new GridColView()
                    {
                        Header = "View Details",
                        Field = string.Empty,
                        WidthWeight = 45,
                        StyleType = GridColView.eGridColStyleType.Template,
                        CellTemplate = (DataTemplate)FindResource("ViewDetailsCellTemplate")
                    }
                ]
            };
            BulkUpdateUCGrid.Title = "Bulk Update Shared Activities";

            BulkUpdateUCGrid.AddToolbarTool(
                "@CheckAllRow_16x16.png",
                "Select All",
                BulkUpdateUCGrid_Toolbar_SelectAllForSync);
            BulkUpdateUCGrid.AddToolbarTool(
                "@UnCheckAllRow_16x16.png",
                "Unselect All",
                BulkUpdateUCGrid_Toolbar_UnselectAllForSync);
            BulkUpdateUCGrid.AddToolbarTool(
                eImageType.Share,
                "Set highlighted Publish for all",
                BulkUpdateUCGrid_Toolbar_SyncPublish);
            BulkUpdateUCGrid.AddToolbarTool(
                eImageType.Mandatory,
                "Set highlighted Mandatoryfor all",
                BulkUpdateUCGrid_Toolbar_SyncMandatory);
            BulkUpdateUCGrid.AddToolbarTool(
                eImageType.Application,
                "Set highlighted Target Application for all",
                BulkUpdateUCGrid_Toolbar_SyncTargetApplication);

            BulkUpdateUCGrid.TextFilter = BulkUpdateUCGrid_TextFilter;

            BulkUpdateUCGrid.ShowRefresh = Visibility.Collapsed;
            BulkUpdateUCGrid.ShowAdd = Visibility.Collapsed;
            BulkUpdateUCGrid.ShowClearAll = Visibility.Collapsed;
            BulkUpdateUCGrid.ShowEdit = Visibility.Collapsed;
            BulkUpdateUCGrid.ShowDelete = Visibility.Collapsed;
            BulkUpdateUCGrid.ShowUpDown = Visibility.Collapsed;

            BulkUpdateUCGrid.SetAllColumnsDefaultView(gridViewDef);

            BulkUpdateUCGrid.InitViewItems();
        }

        private void BulkUpdateUCGrid_Toolbar_SelectAllForSync(object? sender, RoutedEventArgs e)
        {
            IEnumerable<ActivityBulkUpdateListItem> visibleItems = BulkUpdateUCGrid
                .GetFilteredItems()
                .Cast<ActivityBulkUpdateListItem>();

            foreach (ActivityBulkUpdateListItem item in visibleItems)
            {
                item.SelectedForSync = true;
            }
        }

        private void BulkUpdateUCGrid_Toolbar_UnselectAllForSync(object? sender, RoutedEventArgs e)
        {
            IEnumerable<ActivityBulkUpdateListItem> visibleItems = BulkUpdateUCGrid
                .GetFilteredItems()
                .Cast<ActivityBulkUpdateListItem>();

            foreach (ActivityBulkUpdateListItem item in visibleItems)
            {
                item.SelectedForSync = false;
            }
        }

        private void BulkUpdateUCGrid_Toolbar_SyncPublish(object? sender, RoutedEventArgs e)
        {
            IEnumerable<ActivityBulkUpdateListItem> visibleItems = BulkUpdateUCGrid
                .GetFilteredItems()
                .Cast<ActivityBulkUpdateListItem>();

            ActivityBulkUpdateListItem highlightedItem = (ActivityBulkUpdateListItem)BulkUpdateUCGrid.CurrentItem;

            foreach (ActivityBulkUpdateListItem item in visibleItems)
            {
                if (item.SelectedForSync)
                {
                    item.Publish = highlightedItem.Publish;
                }
            }
        }

        private void BulkUpdateUCGrid_Toolbar_SyncMandatory(object? sender, RoutedEventArgs e)
        {
            IEnumerable<ActivityBulkUpdateListItem> visibleItems = BulkUpdateUCGrid
                .GetFilteredItems()
                .Cast<ActivityBulkUpdateListItem>();

            ActivityBulkUpdateListItem highlightedItem = (ActivityBulkUpdateListItem)BulkUpdateUCGrid.CurrentItem;

            foreach (ActivityBulkUpdateListItem item in visibleItems)
            {
                if (item.SelectedForSync)
                {
                    item.Mandatory = highlightedItem.Mandatory;
                }
            }
        }

        private void BulkUpdateUCGrid_Toolbar_SyncTargetApplication(object? sender, RoutedEventArgs e)
        {
            IEnumerable<ActivityBulkUpdateListItem> visibleItems = BulkUpdateUCGrid
                .GetFilteredItems()
                .Cast<ActivityBulkUpdateListItem>();

            ActivityBulkUpdateListItem highlightedItem = (ActivityBulkUpdateListItem)BulkUpdateUCGrid.CurrentItem;

            foreach (ActivityBulkUpdateListItem item in visibleItems)
            {
                if (!item.SelectedForSync)
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

        private bool BulkUpdateUCGrid_TextFilter(object obj, string searchText)
        {
            ActivityBulkUpdateListItem item = (ActivityBulkUpdateListItem)obj;
            return
                (item.Name != null && item.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (item.TargetApplication != null && item.TargetApplication.Contains(searchText, StringComparison.OrdinalIgnoreCase));
        }

        private void SetBulkUpdateUCGridItems(IEnumerable<ActivityBulkUpdateListItem> activityBulkUpdateListItems)
        {
            BulkUpdateUCGrid.DataSourceList = new ObservableList<ActivityBulkUpdateListItem>(activityBulkUpdateListItems);
        }

        public void ShowAsWindow()
        {
            UpdateUIForWindowMode();

            _observableLoaderVisibility = new(value: Visibility.Collapsed);
            _observableLoaderLabel = new(value: string.Empty);
            FrameworkElement loaderElement = CreateWindowLoaderElement(_observableLoaderVisibility, _observableLoaderLabel);
            Button updateButton = CreateWindowUpdateButton();

            GingerCore.General.LoadGenericWindow(
                ref _window,
                owner: App.MainWindow,
                windowStyle: eWindowShowStyle.Dialog,
                windowTitle: "Bulk Update Shared Repository Activities",
                windowPage: this,
                windowBtnsList: [updateButton],
                loaderElement: loaderElement);

            if (_window != null)
            {
                _window.Closing += Window_Closing;
            }
        }

        private void UpdateUIForPageMode()
        {
            FooterGrid.Visibility = Visibility.Visible;
        }

        private void UpdateUIForWindowMode()
        {
            FooterGrid.Visibility = Visibility.Collapsed;
        }

        private void CloseWindow()
        {
            if (_window != null)
            {
                PerformUIOperation(() => _window.Close());
            }
        }

        private void Window_Closing(object? sender, CancelEventArgs e)
        {
            _window = null;
        }

        private bool IsShowingAsWindow()
        {
            return _window != null;
        }

        private FrameworkElement CreateWindowLoaderElement(Observable<Visibility> observableLoaderVisibility, Observable<string> observableLoaderLabel)
        {
            StackPanel loadingStackPanel = new()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            Binding loadingStackPanelVisibilityBinding = new()
            {
                Source = observableLoaderVisibility,
                Path = new PropertyPath("Value"),
                Mode = BindingMode.OneWay
            };
            BindingOperations.SetBinding(loadingStackPanel, StackPanel.VisibilityProperty, loadingStackPanelVisibilityBinding);

            ImageMakerControl processingImageMakerControl = new()
            {
                ImageType = eImageType.Processing,
                Width = 16,
                Height = 16
            };
            loadingStackPanel.Children.Add(processingImageMakerControl);

            Label loadingLabel = new()
            {
                Style = (Style)FindResource("LoadingMessageLabelStyle")
            };
            Binding loadingLabelContentBinding = new()
            {
                Source = observableLoaderLabel,
                Path = new PropertyPath("Value"),
                Mode = BindingMode.OneWay
            };
            BindingOperations.SetBinding(loadingLabel, Label.ContentProperty, loadingLabelContentBinding);
            loadingStackPanel.Children.Add(loadingLabel);

            return loadingStackPanel;
        }

        private Button CreateWindowUpdateButton()
        {
            Button updateButton = new()
            {
                Content = "Update",
            };
            updateButton.Click += UpdateButton_Click;

            return updateButton;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                ShowLoading("saving modified");
                SaveModifiedActivities();
                HideLoading();
                CloseWindow();
            });
        }

        private void ShowLoading(string loadingTaskDetail)
        {
            PerformUIOperation(() =>
            {
                if (IsShowingAsWindow())
                {
                    if (_observableLoaderVisibility != null && _observableLoaderLabel != null)
                    {
                        _observableLoaderLabel.Value = loadingTaskDetail;
                        _observableLoaderVisibility.Value = Visibility.Visible;
                    }
                }
                else
                {
                    LoadingLabel.Content = loadingTaskDetail;
                    LoadingStackPanel.Visibility = Visibility.Visible;
                }
            });
        }

        private void HideLoading()
        {
            PerformUIOperation(() =>
            {
                if (IsShowingAsWindow())
                {
                    if (_observableLoaderVisibility != null && _observableLoaderLabel != null)
                    {
                        _observableLoaderVisibility.Value = Visibility.Collapsed;
                        _observableLoaderLabel.Value = string.Empty;
                    }
                }
                else
                {
                    LoadingStackPanel.Visibility = Visibility.Collapsed;
                    LoadingLabel.Content = string.Empty;
                }
            });
        }

        private void PerformUIOperation(Action operation)
        {
            if (Dispatcher.Thread == Thread.CurrentThread)
            {
                operation();
            }
            else
            {
                Dispatcher.Invoke(operation);
            }
        }

        private void SaveModifiedActivities()
        {
            IEnumerable<ActivityBulkUpdateListItem> modifiedActivityBulkUpdateListItems =
                _activityBulkUpdateListItems
                .Where(item => item.IsModified);

            foreach (ActivityBulkUpdateListItem item in modifiedActivityBulkUpdateListItems)
            {
                item.CommitChanges();
                SaveHandler.Save(item.Activity);
                item.IsModified = false;
            }
        }

        private void ViewDetailsUCButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null || sender is not ucButton viewDetailsUCButton)
            {
                return;
            }

            if (viewDetailsUCButton.Tag == null || viewDetailsUCButton.Tag is not ActivityBulkUpdateListItem activityBulkUpdateListItem)
            {
                return;
            }

            Activity activity = activityBulkUpdateListItem.Activity;
            ActivityDetailsPage activityDetailsPage = new(activity, new Context() { Activity = activity }, General.eRIPageViewMode.View);
            GenericWindow? genericWindow = null;
            GingerCore.General.LoadGenericWindow(
                ref genericWindow!,
                owner: App.MainWindow,
                windowStyle: eWindowShowStyle.Dialog,
                windowTitle: "Activity Details",
                activityDetailsPage);
        }

        public sealed class ActivityBulkUpdateListItem : INotifyPropertyChanged
        {
            private bool _isModified;
            private string _name;
            private bool _publish;
            private bool _mandatory;
            private string _targetApplication;
            private ObservableList<Consumer> _consumers;
            private bool _showConsumerOptions;
            private bool _selectedForSync;

            public event PropertyChangedEventHandler? PropertyChanged;

            public bool IsModified
            {
                get => _isModified;
                set
                {
                    _isModified = value;
                    RaisePropertyChanged(nameof(IsModified));
                }
            }

            public Activity Activity { get; }

            public string Name
            {
                get => _name;
                set
                {
                    _name = value;
                    IsModified = true;
                    RaisePropertyChanged(nameof(Name));
                }
            }

            public bool Publish
            {
                get => _publish;
                set
                {
                    _publish = value;
                    IsModified = true;
                    RaisePropertyChanged(nameof(Publish));
                }
            }

            public bool Mandatory
            {
                get => _mandatory;
                set
                {
                    _mandatory = value;
                    IsModified = true;
                    RaisePropertyChanged(nameof(Mandatory));
                }
            }

            public IEnumerable<string> TargetApplicationOptions { get; }

            public string TargetApplication
            {
                get => _targetApplication;
                set
                {
                    _targetApplication = value;
                    if (GetApplicationPlatform(_targetApplication) == ePlatformType.WebServices)
                    {
                        ConsumersOptions = new(GetConsumersOptions());
                        ShowConsumerOptions = true;
                    }
                    else
                    {
                        ConsumersOptions = new(Array.Empty<Consumer>());
                        ShowConsumerOptions = false;
                    }
                    RaisePropertyChanged(nameof(TargetApplication));
                    IsModified = true;
                }
            }

            public bool ShowConsumerOptions
            {
                get => _showConsumerOptions;
                set
                {
                    _showConsumerOptions = value;
                    RaisePropertyChanged(nameof(ShowConsumerOptions));
                }
            }

            public ObservableList<Consumer> ConsumersOptions { get; private set; }

            public ObservableList<Consumer> Consumers
            {
                get => _consumers;
                set
                {
                    if (_consumers != null)
                    {
                        _consumers.CollectionChanged -= _consumers_CollectionChanged;
                    }
                    _consumers = value;
                    if (_consumers != null)
                    {
                        _consumers.CollectionChanged += _consumers_CollectionChanged;
                    }
                    IsModified = true;
                    RaisePropertyChanged(nameof(Consumers));
                }
            }

            public bool SelectedForSync
            {
                get => _selectedForSync;
                set
                {
                    _selectedForSync = value;
                    RaisePropertyChanged(nameof(SelectedForSync));
                }
            }

            public ActivityBulkUpdateListItem(Activity activity)
            {
                Activity = activity;

                _name = Activity.ActivityName;
                _publish = Activity.Publish;
                _mandatory = Activity.Mandatory;
                _targetApplication = Activity.TargetApplication;
                _consumers = new(Activity.ConsumerApplications);
                _consumers.CollectionChanged += _consumers_CollectionChanged;

                ConsumersOptions = new(GetConsumersOptions());
                if (GetApplicationPlatform(Activity.TargetApplication) == ePlatformType.WebServices)
                {
                    ShowConsumerOptions = true;
                }
                else
                {
                    ShowConsumerOptions = false;
                }
                TargetApplicationOptions = GetTargetApplicationOptions();
            }

            private void _consumers_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            {
                if (IsModified)
                {
                    return;
                }

                if (e.OldItems == null && e.NewItems == null)
                {
                    return;
                }

                if (e.OldItems == null || e.NewItems == null)
                {
                    IsModified = true;
                    return;
                }

                if (e.OldItems.Count == 0 && e.NewItems.Count == 0)
                {
                    return;
                }

                if (e.OldItems.Count != e.NewItems.Count)
                {
                    IsModified = true;
                    return;
                }

                bool someOldItemsMissing =
                    e.OldItems
                    .Cast<Consumer>()
                    .Any(oldConsumer =>
                        e.NewItems
                        .Cast<Consumer>()
                        .All(newConsumer => oldConsumer.ConsumerGuid != newConsumer.ConsumerGuid));

                if (someOldItemsMissing)
                {
                    IsModified = true;
                    return;
                }
            }

            private Consumer[] GetConsumersOptions()
            {
                if (GetApplicationPlatform(_targetApplication) != ePlatformType.WebServices)
                {
                    return [];
                }

                return WorkSpace
                    .Instance
                    .Solution
                    .GetSolutionTargetApplications()
                    .Where(t => !string.Equals(t.Name, _targetApplication))
                    .Where(t => GetApplicationPlatform(t.Name) != ePlatformType.WebServices)
                    .Select(t => new Consumer()
                    {
                        Name = t.Name,
                        ConsumerGuid = t.Guid,
                    })
                    .ToArray();
            }

            private string[] GetTargetApplicationOptions()
            {
                ePlatformType activityPlatform = GetApplicationPlatform(_targetApplication);
                return
                    WorkSpace
                    .Instance
                    .Solution
                    .GetSolutionTargetApplications()
                    .Where(t => GetApplicationPlatform(t.Name) == activityPlatform)
                    .Select(t => t.Name)
                    .ToArray();
            }

            private static ePlatformType GetApplicationPlatform(string targetAppName)
            {
                return
                    WorkSpace
                    .Instance
                    .Solution
                    .GetApplicationPlatformForTargetApp(targetAppName);
            }

            private void RaisePropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler? handler = PropertyChanged;
                handler?.Invoke(sender: this, new PropertyChangedEventArgs(propertyName));
            }

            public void CommitChanges()
            {
                Activity.ActivityName = _name;
                Activity.Mandatory = _mandatory;
                Activity.Publish = _publish;
                Activity.TargetApplication = _targetApplication;
                if (Activity.ConsumerApplications == null)
                {
                    Activity.ConsumerApplications = [];
                }
                else
                {
                    Activity.ConsumerApplications.ClearAll();
                }
                foreach (Consumer consumer in _consumers)
                {
                    Activity.ConsumerApplications.Add(consumer);
                }
            }
        }
    }
}
