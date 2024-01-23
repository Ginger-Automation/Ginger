using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.UserControls;
using Ginger.BusinessFlowPages;
using Ginger.Repository;
using Ginger.UserControls;
using Ginger.UserControlsLib;
using GingerCore;
using GingerCore.Activities;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using NPOI.OpenXmlFormats.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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
            AttachSyncPropertyChangedEventHandlers();
            InitBulkUpdateUCGrid();
            SetBulkUpdateUCGridItems(_activityBulkUpdateListItems);
            UpdateUIForPageMode();
        }

        private void AttachSyncPropertyChangedEventHandlers()
        {
            foreach (ActivityBulkUpdateListItem item in _activityBulkUpdateListItems)
            {
                item.SynchronisedPropertyChanged += BulkUpdateItem_SynchronisedPropertyChanged;
            }
        }

        private void BulkUpdateItem_SynchronisedPropertyChanged(ActivityBulkUpdateListItem sender, string propertyName)
        {
            foreach (ActivityBulkUpdateListItem item in _activityBulkUpdateListItems)
            {
                if (item.SelectedForSync)
                {
                    sender.SyncSiblingProperty(item, propertyName);
                }
            }
        }

        private void InitBulkUpdateUCGrid()
        {
            GridViewDef gridViewDef = new(GridViewDef.DefaultViewName)
            {
                GridColsView = new()
                {
                    new GridColView()
                    {
                        Header = "Sync Changes",
                        Field = nameof(ActivityBulkUpdateListItem.SelectedForSync),
                        WidthWeight = 40,
                        StyleType = GridColView.eGridColStyleType.CheckBox,
                        BindingMode = BindingMode.TwoWay
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
                        Header = "View Details",
                        Field = string.Empty,
                        WidthWeight = 50,
                        StyleType = GridColView.eGridColStyleType.Template,
                        CellTemplate = (DataTemplate)FindResource("ViewDetailsCellTemplate")
                    },
                    new GridColView()
                    {
                        Header = "Publish",
                        Field = nameof(ActivityBulkUpdateListItem.Publish),
                        StyleType = GridColView.eGridColStyleType.CheckBox,
                        WidthWeight = 40,
                        BindingMode = BindingMode.TwoWay
                    },
                    new GridColView()
                    {
                        Header = "Mandatory",
                        Field = nameof(ActivityBulkUpdateListItem.Mandatory),
                        StyleType = GridColView.eGridColStyleType.CheckBox,
                        WidthWeight = 40,
                        BindingMode = BindingMode.TwoWay
                    },
                    new GridColView()
                    {
                        Header = GingerDicser.GetTermResValue(eTermResKey.TargetApplication),
                        Field = nameof(ActivityBulkUpdateListItem.TargetApplication),
                        //CellValuesList = WorkSpace.Instance.Solution.GetSolutionTargetApplications().Select(targetApp => new ComboEnumItem()
                        //{ 
                        //    text = targetApp.Name, 
                        //    Value = targetApp.Name 
                        //}),
                        CellTemplate = (DataTemplate)FindResource("TargetApplicationCellTemplate"),
                        StyleType = GridColView.eGridColStyleType.Template,
                        WidthWeight = 60,
                        BindingMode = BindingMode.TwoWay
                    },
                    new GridColView()
                    {
                        Header = "Consumers",
                        Field = nameof(ActivityBulkUpdateListItem.Consumers),
                        CellTemplate = (DataTemplate)FindResource("ConsumerCellTemplate"),
                        StyleType = GridColView.eGridColStyleType.Template,
                        WidthWeight = 60,
                        BindingMode = BindingMode.TwoWay,
                    }
                }
            };
            BulkUpdateUCGrid.Title = "Bulk Update Shared Activities";
            BulkUpdateUCGrid.ShowRefresh = Visibility.Collapsed;
            BulkUpdateUCGrid.ShowAdd = Visibility.Collapsed;
            BulkUpdateUCGrid.ShowClearAll = Visibility.Collapsed;
            BulkUpdateUCGrid.ShowEdit = Visibility.Collapsed;
            BulkUpdateUCGrid.ShowDelete = Visibility.Collapsed;
            BulkUpdateUCGrid.ShowUpDown = Visibility.Collapsed;
            BulkUpdateUCGrid.SetAllColumnsDefaultView(gridViewDef);
            BulkUpdateUCGrid.InitViewItems();
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
            FrameworkElement loaderElement = CreateWindowLoaderElement(_observableLoaderVisibility,  _observableLoaderLabel);
            Button updateButton = CreateWindowUpdateButton();
            
            GingerCore.General.LoadGenericWindow(
                ref _window, 
                owner: App.MainWindow, 
                windowStyle: eWindowShowStyle.Dialog, 
                windowTitle: "Bulk Update Shared Repository Activities", 
                windowPage: this,
                windowBtnsList: new ObservableList<Button>() { updateButton },
                loaderElement: loaderElement);

            if(_window != null)
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
                Orientation = Orientation.Horizontal
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
            Task.Run(async () =>
            {
                Task saveAllTask = SaveModifiedActivitiesAsync();
                if (!saveAllTask.IsCompleted)
                {
                    ShowLoading("saving modified");
                    await saveAllTask;
                    HideLoading();
                    CloseWindow();
                }
                else
                {
                    CloseWindow();
                }
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

        private Task SaveModifiedActivitiesAsync()
        {
            IEnumerable<ActivityBulkUpdateListItem> modifiedActivityBulkUpdateListItems = 
                _activityBulkUpdateListItems
                .Where(item => item.IsModified);

            List<Task> saveTasks = [];
            foreach(ActivityBulkUpdateListItem item in modifiedActivityBulkUpdateListItems)
            {
                Task saveTask = Task.Run(() =>
                {
                    item.CommitChanges();
                    SaveHandler.Save(item.Activity); 
                    item.IsModified = false;
                });
                saveTasks.Add(saveTask);
            }
            return Task.WhenAll(saveTasks);
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

            public delegate void SynchronisedPropertyChangedEventHandler(ActivityBulkUpdateListItem sender, string propertyName);

            public event PropertyChangedEventHandler? PropertyChanged;
            public event SynchronisedPropertyChangedEventHandler? SynchronisedPropertyChanged;

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
                    RaiseSynchronisedPropertyChanged(nameof(Publish));
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
                    RaiseSynchronisedPropertyChanged(nameof(Mandatory));
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
                        ConsumersOptions = GetConsumersOptions();
                        ShowConsumerOptions = true;
                    }
                    else
                    {
                        ConsumersOptions = Array.Empty<Consumer>();
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

            public IEnumerable<Consumer> ConsumersOptions { get; private set; }

            public ObservableList<Consumer> Consumers
            {
                get => _consumers;
                set
                {
                    _consumers = value;
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
                _consumers = Activity.ConsumerApplications;

                ConsumersOptions = GetConsumersOptions();
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

            private Consumer[] GetConsumersOptions()
            {
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
                ePlatformType activityPlatform = GetApplicationPlatform(Activity.TargetApplication);
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

            private void RaiseSynchronisedPropertyChanged(string propertyName)
            {
                SynchronisedPropertyChangedEventHandler? handler = SynchronisedPropertyChanged;
                handler?.Invoke(sender: this, propertyName);
            }

            public void SyncSiblingProperty(ActivityBulkUpdateListItem sibling, string propertyName)
            {
                if (sibling == this)
                {
                    return;
                }

                if (string.Equals(propertyName, nameof(Publish)))
                {
                    sibling._publish = _publish;
                    sibling.RaisePropertyChanged(nameof(Publish));
                }
                else if (string.Equals(propertyName, nameof(Mandatory)))
                {
                    sibling._mandatory = _mandatory;
                    sibling.RaisePropertyChanged(nameof(Mandatory));
                }
            }

            public void CommitChanges()
            {
                Activity.ActivityName = _name;
                Activity.Mandatory = _mandatory;
                Activity.Publish = _publish;
                Activity.TargetApplication = _targetApplication;
                Activity.ConsumerApplications = _consumers;
            }
        }
    }
}
