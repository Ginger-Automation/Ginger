#region License
/*
Copyright © 2014-2018 European Support Limited

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
using System;
using System.Windows;
using System.Windows.Controls;
using Ginger.SolutionGeneral;
using Ginger.UserControls;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace Ginger.SolutionWindows
{
    /// <summary>
    /// Interaction logic for AddApplicationPage.xaml
    /// </summary>
    public partial class AddApplicationPage : Page
    {
        GenericWindow _pageGenericWin = null;
        Solution mSolution;

        public AddApplicationPage(Solution Solution)
        {
            InitializeComponent();
            SetAppsGridView();
            InitGridData();
            mSolution = Solution;
        }

        private void InitGridData()
        {
            //TOOD: read from files of Ginger folder not in solution and put it in tree view organized + search, load the GUID so will help in search and map packages
            // Later on get this list from our public web site

            // meanwhile grid will do
            ObservableList<ApplicationPlatform> APs = new ObservableList<ApplicationPlatform>();
            APs.Add(new ApplicationPlatform() { AppName = "CRM", Platform = ePlatformType.Java, Description = "Amdocs Client Relationship Manager" });
            APs.Add(new ApplicationPlatform() { AppName = "CSM", Platform = ePlatformType.PowerBuilder, Description = "Amdocs CSM" });
            APs.Add(new ApplicationPlatform() { AppName = "MyWebApp", Platform = ePlatformType.Web, Description = "Web Application" });
            APs.Add(new ApplicationPlatform() { AppName = "MyNewWebApp", Platform = ePlatformType.Web, Description = "Web Application" });
            APs.Add(new ApplicationPlatform() { AppName = "MyWebServicesApp", Platform = ePlatformType.WebServices, Description = "WebServices Application" });
            APs.Add(new ApplicationPlatform() { AppName = "MyMobileApp", Platform = ePlatformType.Mobile, Description = "Mobile Application" });
            APs.Add(new ApplicationPlatform() { AppName = "Mediation", Platform = ePlatformType.Unix, Description = "Amdocs Mediation" });
            APs.Add(new ApplicationPlatform() { AppName = "MyDosApp", Platform = ePlatformType.DOS, Description = "DOS Application" });
            APs.Add(new ApplicationPlatform() { AppName = "MyJavaApp", Platform = ePlatformType.Java, Description = "Java Application" });
            APs.Add(new ApplicationPlatform() { AppName = "MyMainFrameApp", Platform = ePlatformType.MainFrame, Description = "MainFrame Application" });
            APs.Add(new ApplicationPlatform() { AppName = "MyWindowsApp", Platform = ePlatformType.Windows, Description = "Windows Application" });
            SelectApplicationGrid.DataSourceList = APs;
            SelectApplicationGrid.RowDoubleClick += SelectApplicationGrid_RowDoubleClick;
        }
       
        private void SetAppsGridView()
        {         
            SelectApplicationGrid.SelectionMode = DataGridSelectionMode.Single;
            SelectApplicationGrid.ShowDelete = System.Windows.Visibility.Collapsed;
            SelectApplicationGrid.ShowClearAll = System.Windows.Visibility.Collapsed;
            SelectApplicationGrid.ShowAdd = System.Windows.Visibility.Collapsed;
            SelectApplicationGrid.ShowDelete = System.Windows.Visibility.Collapsed;
            SelectApplicationGrid.ShowEdit = System.Windows.Visibility.Collapsed;
            SelectApplicationGrid.ShowUpDown = System.Windows.Visibility.Collapsed;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPlatform.AppName), Header = "Application", WidthWeight = 60 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPlatform.Core), WidthWeight = 60 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPlatform.CoreVersion), WidthWeight = 20 });      
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPlatform.Description), WidthWeight = 60 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPlatform.Platform), WidthWeight = 40 });
            SelectApplicationGrid.SetAllColumnsDefaultView(view);
            SelectApplicationGrid.InitViewItems();
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button CloseButton = new Button();
            CloseButton.Content = "OK";
            CloseButton.Click += new RoutedEventHandler(OKButton_Click);

            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(CloseButton);            

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, null, windowStyle, "Add Application to solution", this, winButtons, true);
            _pageGenericWin.Width = 800;
            _pageGenericWin.Height = 300;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (mSolution.ApplicationPlatforms == null)
            {
                mSolution.ApplicationPlatforms = new ObservableList<ApplicationPlatform>();
            }
            foreach (ApplicationPlatform selectedApp in SelectApplicationGrid.Grid.SelectedItems)
            {
                mSolution.SetUniqueApplicationName(selectedApp); 
                mSolution.ApplicationPlatforms.Add(selectedApp);                               
            }
            _pageGenericWin.Close();
        }

        private void SelectApplicationGrid_RowDoubleClick(object sender, EventArgs e)
        {
            mSolution.SetUniqueApplicationName((ApplicationPlatform)SelectApplicationGrid.Grid.SelectedItem);
            mSolution.ApplicationPlatforms.Add((ApplicationPlatform)SelectApplicationGrid.Grid.SelectedItem);
            _pageGenericWin.Close();
        }
    }
}