using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.UserControls;
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
            _activityBulkUpdateListItems = activities.Select(activity => new ActivityBulkUpdateListItem(activity));
            InitBulkUpdateUCGrid();
            SetBulkUpdateUCGridItems(_activityBulkUpdateListItems);
            UpdateUIForPageMode();
        }

        private void InitBulkUpdateUCGrid()
        {
            GridViewDef gridViewDef = new(GridViewDef.DefaultViewName)
            {
                GridColsView = new()
                {
                    new GridColView()
                    {
                        Header = nameof(ActivityBulkUpdateListItem.Name),
                        Field = nameof(ActivityBulkUpdateListItem.Name),
                        WidthWeight = 80,
                        BindingMode = BindingMode.TwoWay
                    },
                    new GridColView()
                    {
                        Header = nameof(ActivityBulkUpdateListItem.Publish),
                        Field = nameof(ActivityBulkUpdateListItem.Publish),
                        StyleType = GridColView.eGridColStyleType.CheckBox,
                        WidthWeight = 60,
                        BindingMode = BindingMode.TwoWay
                    },
                    new GridColView()
                    {
                        Header = nameof(ActivityBulkUpdateListItem.Mandatory),
                        Field = nameof(ActivityBulkUpdateListItem.Mandatory),
                        StyleType = GridColView.eGridColStyleType.CheckBox,
                        WidthWeight = 60,
                        BindingMode = BindingMode.TwoWay
                    },
                    new GridColView()
                    {
                        Header = nameof(ActivityBulkUpdateListItem.TargetApplication),
                        Field = nameof(ActivityBulkUpdateListItem.TargetApplication),
                        CellValuesList = WorkSpace.Instance.Solution.GetSolutionTargetApplications().Select(targetApp => new ComboEnumItem()
                        { 
                            text = targetApp.Name, 
                            Value = targetApp.Name 
                        }),
                        StyleType = GridColView.eGridColStyleType.ComboBox,
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
            IEnumerable<ActivityBulkUpdateListItem> modifiedActivityBulkUpdateListItems = _activityBulkUpdateListItems
                .Where(activityBulkUpdateListItem => activityBulkUpdateListItem.IsModified);

            List<Task> saveTasks = [];
            foreach(ActivityBulkUpdateListItem activityBulkUpdateListItem in modifiedActivityBulkUpdateListItems)
            {
                Task saveTask = Task.Run(() => SaveHandler.Save(activityBulkUpdateListItem.Activity));
                saveTask = saveTask.ContinueWith(_ =>
                {
                    activityBulkUpdateListItem.IsModified = false;
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
                saveTasks.Add(saveTask);
            }
            return Task.WhenAll(saveTasks);
        }

        public sealed class ActivityBulkUpdateListItem : INotifyPropertyChanged
        {
            private bool _showConsumerOptions = false;

            public event PropertyChangedEventHandler? PropertyChanged;

            public bool IsModified { get; set; }

            public Activity Activity { get; }

            public ObservableList<Consumer> ConsumerOptions { get; }

            public ObservableList<Consumer> Consumers
            {
                get => Activity.ConsumerApplications;
                set
                {
                    Activity.ConsumerApplications = value;
                    IsModified = true;
                }
            }

            public bool ShowConsumerOptions
            {
                get => _showConsumerOptions;
                set
                {
                    _showConsumerOptions = value;
                    OnPropertyChanged(nameof(ShowConsumerOptions));
                }
            }

            public string Name
            {
                get => Activity.ActivityName;
                set
                {
                    Activity.ActivityName = value;
                    IsModified = true;
                }
            }

            public bool Publish
            {
                get => Activity.Publish;
                set
                {
                    Activity.Publish = value;
                    IsModified = true;
                }
            }

            public bool Mandatory
            {
                get => Activity.Mandatory;
                set
                {
                    Activity.Mandatory = value;
                    IsModified = true;
                }
            }

            public string TargetApplication
            {
                get => Activity.TargetApplication;
                set
                {
                    Activity.TargetApplication = value;
                    if (IsWebServicesTargetApplication(Activity.TargetApplication))
                    {
                        SetConsumerOptions(Activity.TargetApplication);
                        ShowConsumerOptions = true;
                    }
                    else
                    {
                        ConsumerOptions.ClearAll();
                        ShowConsumerOptions = false;
                    }
                    IsModified = true;
                }
            }

            public IEnumerable<string> TargetApplicationOptions { get; }

            public ActivityBulkUpdateListItem(Activity activity)
            {
                Activity = activity;
                AttachActivityPropertyChangedHandler();
                ConsumerOptions = new();
                if (IsWebServicesTargetApplication(Activity.TargetApplication))
                {
                    ShowConsumerOptions = true;
                    SetConsumerOptions(Activity.TargetApplication);
                }
                TargetApplicationOptions = GetTargetApplicationOptions();
            }

            private void SetConsumerOptions(string targetAppName)
            {
                IEnumerable<TargetBase> solutionTargetApps = WorkSpace
                    .Instance
                    .Solution
                    .GetSolutionTargetApplications();

                IEnumerable<Consumer> consumerOptions = 
                    solutionTargetApps
                    .Where(t => !string.Equals(t.Name, targetAppName))
                    .Select(t => new Consumer()
                    {
                        Name = t.Name,
                        ConsumerGuid = t.Guid,
                    })
                    .ToArray();

                ConsumerOptions.ClearAll();
                foreach(Consumer consumerOption in consumerOptions)
                {
                    ConsumerOptions.Add(consumerOption);
                }
            }

            private IEnumerable<string> GetTargetApplicationOptions()
            {
                ePlatformType activityPlatform = GetTargetApplicationPlatform(Activity.TargetApplication);
                return GetTargetApplicationWithPlatform(activityPlatform).Select(t => t.Name);
            }

            private IEnumerable<TargetBase> GetTargetApplicationWithPlatform(ePlatformType platform)
            {
                return
                    WorkSpace
                    .Instance
                    .Solution
                    .GetSolutionTargetApplications()
                    .Where(t => GetTargetApplicationPlatform(t.Name) == platform);
            }

            private bool IsWebServicesTargetApplication(string targetAppName)
            {
                return
                    WorkSpace
                    .Instance
                    .Solution
                    .GetApplicationPlatformForTargetApp(targetAppName) == ePlatformType.WebServices;
            }

            private ePlatformType GetTargetApplicationPlatform(string targetAppName)
            {
                return
                    WorkSpace
                       .Instance
                       .Solution
                       .GetApplicationPlatformForTargetApp(targetAppName);
            }

            private void AttachActivityPropertyChangedHandler()
            {
                string allProperties = string.Empty;
                PropertyChangedEventManager.AddHandler(source: Activity, handler: OnActivityPropertyChanged, propertyName: allProperties);
                CollectionChangedEventManager.AddHandler(source: Activity.ConsumerApplications, handler: OnActivityConsumerApplicationsCollectionChanged);
            }

            private void OnActivityPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (string.Equals(e.PropertyName, nameof(Activity.ActivityName)))
                {
                    OnPropertyChanged(nameof(Name));
                }
                else if (string.Equals(e.PropertyName, nameof(Activity.Publish)))
                {
                    OnPropertyChanged(nameof(Publish));
                }
                else if (string.Equals(e.PropertyName, nameof(Activity.Mandatory)))
                {
                    OnPropertyChanged(nameof(Mandatory));
                }
                else if (string.Equals(e.PropertyName, nameof(Activity.TargetApplication)))
                {
                    OnPropertyChanged(nameof(TargetApplication));
                }
                else if (string.Equals(e.PropertyName, nameof(Activity.ConsumerApplications)))
                {
                    OnPropertyChanged(nameof(Consumers));
                }
            }

            private void OnActivityConsumerApplicationsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            {
                IsModified = true;
            }

            private void OnPropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler? handler = PropertyChanged;
                handler?.Invoke(sender: this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
