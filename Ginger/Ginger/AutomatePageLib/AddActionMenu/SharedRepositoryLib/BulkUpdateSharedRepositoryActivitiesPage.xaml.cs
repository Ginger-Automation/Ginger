using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.UserControls;
using Ginger.Repository;
using Ginger.UserControlsLib;
using GingerCore;
using GingerCore.Activities;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
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

        public BulkUpdateSharedRepositoryActivitiesPage()
        {
            InitializeComponent();
            _activityBulkUpdateListItems = GetSharedRepositoryActivitiesAsBulkUpdateListItems();
            SetBulkUpdateListViewItems(_activityBulkUpdateListItems);
            UpdateUIForPageMode();
        }

        private IEnumerable<ActivityBulkUpdateListItem> GetSharedRepositoryActivitiesAsBulkUpdateListItems()
        {
            IEnumerable<Activity> sharedRepositoryActivities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            IEnumerable<ActivityBulkUpdateListItem> activityBulkUpdateListItems = sharedRepositoryActivities.Select(activity => new ActivityBulkUpdateListItem(activity));
            return activityBulkUpdateListItems.ToList();
        }

        private void SetBulkUpdateListViewItems(IEnumerable<ActivityBulkUpdateListItem> activityBulkUpdateListItems)
        {            
            ActivityBulkUpdateListView.ItemsSource = activityBulkUpdateListItems;
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
            public event PropertyChangedEventHandler? PropertyChanged;

            public bool IsModified { get; set; }

            public Activity Activity { get; }

            public IEnumerable<Node> ConsumerOptions { get; }

            public bool ShowConsumerOptions { get; }

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

            public ActivityBulkUpdateListItem(Activity activity)
            {
                Activity = activity;
                AttachActivityPropertyChangedHandler();
                TargetBase targetApp = GetTargetApplication(activity.TargetApplication);
                ConsumerOptions = GetConsumerOptions(targetApp);
                ShowConsumerOptions = IsWebServicesTargetApplication(targetApp);
            }

            private TargetBase GetTargetApplication(string targetApplicationName)
            {
                return WorkSpace
                    .Instance
                    .Solution
                    .GetSolutionTargetApplications()
                    .First(targetApp => string.Equals(targetApp.Name, targetApplicationName));
            }

            private IEnumerable<Node> GetConsumerOptions(TargetBase targetApp)
            {
                IEnumerable<TargetBase> solutionTargetApps = WorkSpace
                    .Instance
                    .Solution
                    .GetSolutionTargetApplications();

                return
                    solutionTargetApps
                    .Where(t => t.Guid != targetApp.Guid)
                    .Select(t => new Consumer()
                    {
                        Name = t.Name,
                        ConsumerGuid = t.Guid,
                    })
                    .Select(consumer =>
                    {
                        Node node = new(title: consumer.Name) { Tag = consumer };
                        node.PropertyChanged += ConsumerMultiSelectComboBoxNode_PropertyChanged;

                        return node;
                    })
                    .ToArray();
            }

            private void ConsumerMultiSelectComboBoxNode_PropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (sender == null)
                {
                    return;
                }
                if (!string.Equals(e.PropertyName, nameof(Node.IsSelected)))
                {
                    return;
                }

                Node senderNode = (Node)sender;
                if (senderNode.Tag == null)
                {
                    return;
                }

                if (senderNode.IsSelected)
                {
                    Activity.ConsumerApplications.Add((Consumer)senderNode.Tag);
                }
                else
                {
                    Activity.ConsumerApplications.Remove((Consumer)senderNode.Tag);
                }

            }

            private bool IsWebServicesTargetApplication(TargetBase targetApp)
            {
                return
                    WorkSpace
                    .Instance
                    .Solution
                    .GetApplicationPlatformForTargetApp(targetApp.Name) == ePlatformType.WebServices;
            }

            private void AttachActivityPropertyChangedHandler()
            {
                string allProperties = string.Empty;
                PropertyChangedEventManager.AddHandler(source: Activity, handler: OnActivityPropertyChanged, propertyName: allProperties);
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
            }

            private void OnPropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler? handler = PropertyChanged;
                handler?.Invoke(sender: this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
