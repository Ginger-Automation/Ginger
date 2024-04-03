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
using Ginger.UserControls;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using GingerCore.Environments;
using GingerTest.WizardLib;
using amdocs.ginger.GingerCoreNET;

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

            view.GridColsView.Add(new GridColView() { Field = "Selected", Header = "Select" ,WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPlatform.PlatformImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16 });
            view.GridColsView.Add(new GridColView() { Field = "AppName", Header = "Application Name", WidthWeight = 50, ReadOnly = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = "Platform", Header = "Platform", WidthWeight = 30, ReadOnly = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = "Description", Header = "Description", WidthWeight = 30, ReadOnly = true, BindingMode = BindingMode.OneWay });

            AppsGrid.SetAllColumnsDefaultView(view);
            AppsGrid.InitViewItems();

            xAddApplicationToSolution.ButtonTextSize = 12;
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

        private void AddApplicationToSolution(object sender, RoutedEventArgs e)
        {
            AddApplicationPage applicationPage = new(WorkSpace.Instance.Solution, false);

            applicationPage.ShowAsWindow();

            foreach (ApplicationPlatform selectedApp in applicationPage.SelectApplicationGrid.Grid.SelectedItems)
            {
                AppsGrid.DataSourceList.Add(selectedApp);
            }

        }
    }
}
