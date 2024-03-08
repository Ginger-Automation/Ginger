using Amdocs.Ginger.Common;
using Ginger.UserControls;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Controls.Primitives;

namespace Ginger.SolutionWindows.TreeViewItems.EnvironmentsTreeItems
{
    /// <summary>
    /// Interaction logic for EnvironmentApplicationList.xaml
    /// </summary>
    public partial class EnvironmentApplicationList : Page
    {
        ObservableList<ApplicationPlatform> FilteredListToBeDisplayed;
        GenericWindow _pageGenericWin = null;

        public EnvironmentApplicationList(ObservableList<ApplicationPlatform> FilteredListToBeDisplayed)
        {
            this.FilteredListToBeDisplayed = FilteredListToBeDisplayed;
            InitializeComponent();
            SetGridView();
        }

        private void SetGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = "Selected", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPlatform.PlatformImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16 });
            view.GridColsView.Add(new GridColView() { Field = "AppName", Header = "Application Name", WidthWeight = 50, ReadOnly = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = "Platform", Header = "Platform", WidthWeight = 30, ReadOnly = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = "Description", Header = "Description", WidthWeight = 30, ReadOnly = true, BindingMode = BindingMode.OneWay });

            AppsGrid.SetAllColumnsDefaultView(view);
            AppsGrid.InitViewItems();


            AppsGrid.DataSourceList = FilteredListToBeDisplayed;
        }
        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool ShowCancelButton = true)
        {
            Button okBtn = new Button();
            okBtn.Content = "Ok";
            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(okBtn);
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: okBtn, eventName: nameof(ButtonBase.Click), handler: OKButton_Click);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, ShowCancelButton, "Cancel");
        }
    
        public void OKButton_Click(object sender, RoutedEventArgs e)
        {
            _pageGenericWin?.Close();
        }
    }
}
