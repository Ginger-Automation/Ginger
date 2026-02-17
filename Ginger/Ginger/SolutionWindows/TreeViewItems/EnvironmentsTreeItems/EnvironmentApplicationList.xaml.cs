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
using Ginger.UserControls;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

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
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
                [
                    new GridColView() { Field = "Selected", Header = "Select", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox },
                    new GridColView() { Field = nameof(ApplicationPlatform.PlatformImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16 },
                    new GridColView() { Field = "AppName", Header = "Application Name", WidthWeight = 50, ReadOnly = true, BindingMode = BindingMode.OneWay },
                    // Show the friendly enum description
                    new GridColView() { Field = nameof(ApplicationPlatform.PlatformDescription), Header = "Platform", WidthWeight = 30, ReadOnly = true, BindingMode = BindingMode.OneWay },
                    new GridColView() { Field = "Description", Header = "Description", WidthWeight = 30, ReadOnly = true, BindingMode = BindingMode.OneWay },
                ]
            };

            AppsGrid.SetAllColumnsDefaultView(view);
            AppsGrid.InitViewItems();
            AppsGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Applications", new RoutedEventHandler(CheckUnCheckAllApplications));

            xAddApplicationToSolution.ButtonTextSize = 12;
            AppsGrid.DataSourceList = FilteredListToBeDisplayed;
        }
        private void CheckUnCheckAllApplications(object sender, RoutedEventArgs e)
        {
            IObservableList filteringEnvApplication = AppsGrid.DataSourceList;


            int selectedItems = CountSelectedItems();
            if (selectedItems < AppsGrid.DataSourceList.Count)
            {
                foreach (ApplicationPlatform ApplicationPlatform in filteringEnvApplication)
                {
                    ApplicationPlatform.Selected = true;
                }
            }
            else if (selectedItems == AppsGrid.DataSourceList.Count)
            {
                foreach (ApplicationPlatform ApplicationPlatform in filteringEnvApplication)
                {
                    ApplicationPlatform.Selected = false;
                }
            }

            AppsGrid.DataSourceList = filteringEnvApplication;
        }

        private int CountSelectedItems()
        {
            int counter = 0;
            foreach (ApplicationPlatform ApplicationPlatform in AppsGrid.DataSourceList)
            {
                if (ApplicationPlatform.Selected)
                {
                    counter++;
                }
            }
            return counter;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool ShowCancelButton = true)
        {

            Button okBtn = new Button
            {
                Content = "Ok"
            };
            ObservableList<Button> winButtons = [okBtn];
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

            ApplicationPlatform? selectedApp = null;
            applicationPage.ShowAsWindow(ref selectedApp);

            if (selectedApp != null)
            {
                AppsGrid.DataSourceList.Add(selectedApp);
            }

        }
    }
}
