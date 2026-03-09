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
using Ginger.SolutionGeneral;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Environments;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Microsoft.VisualStudio.Services.Common;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows
{
    /// <summary>
    /// Interaction logic for AddApplicationPage.xaml
    /// </summary>
    public partial class AddApplicationPage : Page
    {
        GenericWindow _pageGenericWin = null;
        Solution mSolution;
        bool isApplicationLoadedFromNewSolution = false;
        public AddApplicationPage(Solution Solution, bool isApplicationLoadedFromNewSolution)
        {
            InitializeComponent();
            SelectApplicationGrid.SelectionMode = DataGridSelectionMode.Single;

            SetAppsGridView();
            InitGridData();
            mSolution = Solution;
            this.isApplicationLoadedFromNewSolution = isApplicationLoadedFromNewSolution;
        }

        private void InitGridData()
        {
            //TOOD: read from files of Ginger folder not in solution and put it in tree view organized + search, load the GUID so will help in search and map packages
            // Later on get this list from our public web site

            // meanwhile grid will do
            ObservableList<ApplicationPlatform> APs =
            [
                new ApplicationPlatform() { AppName = "MyWebApp", Platform = ePlatformType.Web, Description = "Web Application" },
                new ApplicationPlatform() { AppName = "MyJavaApp", Platform = ePlatformType.Java, Description = "Java Application" },
                new ApplicationPlatform() { AppName = "MyWebServicesApp", Platform = ePlatformType.WebServices, Description = "WebServices Application" },
                new ApplicationPlatform() { AppName = "MyMobileApp", Platform = ePlatformType.Mobile, Description = "Mobile Application" },
                //new ApplicationPlatform() { AppName = "MyTvApp", Platform = ePlatformType.TV, Description = "Tv Application"},
                new ApplicationPlatform() { AppName = "Mediation", Platform = ePlatformType.Unix, Description = "Amdocs Mediation" },
                new ApplicationPlatform() { AppName = "MyDosApp", Platform = ePlatformType.DOS, Description = "DOS Application" },
                new ApplicationPlatform() { AppName = "MyMainFrameApp", Platform = ePlatformType.MainFrame, Description = "MainFrame Application" },
                new ApplicationPlatform() { AppName = "MyWindowsApp", Platform = ePlatformType.Windows, Description = "Windows Application" },
                new ApplicationPlatform() { AppName = "MyPowerBuilderApp", Platform = ePlatformType.PowerBuilder, Description = "Power Builder Application" },
            ];
            SelectApplicationGrid.DataSourceList = APs;
            SelectApplicationGrid.SearchVisibility = Visibility.Collapsed;
        }

        private void SetAppsGridView()
        {
            SelectApplicationGrid.ShowDelete = System.Windows.Visibility.Collapsed;
            SelectApplicationGrid.ShowClearAll = System.Windows.Visibility.Collapsed;
            SelectApplicationGrid.ShowAdd = System.Windows.Visibility.Collapsed;
            SelectApplicationGrid.ShowDelete = System.Windows.Visibility.Collapsed;
            SelectApplicationGrid.ShowEdit = System.Windows.Visibility.Collapsed;
            SelectApplicationGrid.ShowUpDown = System.Windows.Visibility.Collapsed;

            // Get enum descriptions as ComboEnumItem list (text = description, Value = enum)
            var platformComboItems = GingerCore.General.GetEnumValuesForCombo(typeof(ePlatformType));

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
                [
                    new GridColView() { Field = nameof(ApplicationPlatform.PlatformImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 24 },

            // Use ComboBox column bound to enum list so UI shows the EnumValueDescription text
            new GridColView()
            {
                Field = nameof(ApplicationPlatform.Platform),
                Header = "Select Platform",
                WidthWeight = 40,
                StyleType = GridColView.eGridColStyleType.ComboBox,
                CellValuesList = platformComboItems
            },
        ]
            };

            SelectApplicationGrid.SetAllColumnsDefaultView(view);
            SelectApplicationGrid.InitViewItems();
        }

        ApplicationPlatform? NewlyAddedApplicationPlatform = null;

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button CloseButton = new Button
            {
                Content = "OK"
            };
            CloseButton.Click += new RoutedEventHandler(OKButton_Click);
            ObservableList<Button> winButtons = [CloseButton];

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, null, windowStyle, "Add Application to solution", this, winButtons, true);
            _pageGenericWin.Width = 800;
            _pageGenericWin.Height = 300;

        }
        public void ShowAsWindow(ref ApplicationPlatform? NewlyAddedApplicationPlatform, eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {

            ShowAsWindow(windowStyle);
            NewlyAddedApplicationPlatform = this.NewlyAddedApplicationPlatform;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (mSolution.ApplicationPlatforms == null)
            {
                mSolution.ApplicationPlatforms = [];
            }
            var name = applicationName.Text.Trim();
            var description = applicationDescription.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                NameError.Text = "Name is mandatory";
                NameError.Visibility = Visibility.Visible;
                return;
            }

            ApplicationPlatform selectedApp = (ApplicationPlatform)SelectApplicationGrid.Grid.SelectedItem;



            if (!this.isApplicationLoadedFromNewSolution)
            {
                bool doesNameExist = WorkSpace.Instance.Solution.ApplicationPlatforms.Any((app) => app.AppName.Equals(name));

                if (doesNameExist)
                {

                    NameError.Text = "This Name already exists";
                    NameError.Visibility = Visibility.Visible;
                    return;
                }

            }


            eUserMsgSelection msgSelection = eUserMsgSelection.None;

            if (!this.isApplicationLoadedFromNewSolution)
            {
                msgSelection = Reporter.ToUser(eUserMsgKey.ShareApplicationToEnvironment);
            }
            selectedApp.AppName = name;
            selectedApp.Description = description;

            if (!this.isApplicationLoadedFromNewSolution && msgSelection.Equals(eUserMsgSelection.Yes))
            {
                var ProjEnvironments = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>();

                ProjEnvironments.ForEach((projEnv) =>
                {

                    projEnv.StartDirtyTracking();
                    projEnv.Applications.Add(new EnvApplication() { Name = selectedApp.AppName, ParentGuid = selectedApp.Guid, Platform = selectedApp.Platform, Active = true });
                    projEnv.OnPropertyChanged(nameof(projEnv.Applications));
                });
            }

            mSolution.ApplicationPlatforms.Add(selectedApp);


            if (!msgSelection.Equals(eUserMsgSelection.Yes))
            {
                this.NewlyAddedApplicationPlatform = selectedApp;
            }

            var DoesMatchingPlatformAgentExist = WorkSpace.Instance.SolutionRepository?.GetAllRepositoryItems<Agent>().Any((agent) => agent.Platform.Equals(selectedApp.Platform)) ?? true;

            if (!DoesMatchingPlatformAgentExist && !this.isApplicationLoadedFromNewSolution)
            {
                SetAgent(selectedApp);
            }

            _pageGenericWin.Close();
        }


        public void SetAgent(ApplicationPlatform selectedApp)
        {
            Agent agent = new Agent();
            AgentOperations agentOperations = new AgentOperations(agent);
            agent.AgentOperations = agentOperations;

            agent.Name = selectedApp.AppName;
            switch (selectedApp.Platform)
            {
                case ePlatformType.ASCF:
                    agent.DriverType = Agent.eDriverType.ASCF;
                    break;
                case ePlatformType.DOS:
                    agent.DriverType = Agent.eDriverType.DOSConsole;
                    break;
                case ePlatformType.Mobile:
                    agent.DriverType = Agent.eDriverType.Appium;
                    break;
                //case ePlatformType.TV:
                //    agent.DriverType = Agent.eDriverType.Appium;
                //    break;
                case ePlatformType.PowerBuilder:
                    agent.DriverType = Agent.eDriverType.PowerBuilder;
                    break;
                case ePlatformType.Unix:
                    agent.DriverType = Agent.eDriverType.UnixShell;
                    break;
                case ePlatformType.Web:
                    agent.DriverType = Agent.eDriverType.Selenium;
                    break;
                case ePlatformType.WebServices:
                    agent.DriverType = Agent.eDriverType.WebServices;
                    break;
                case ePlatformType.Windows:
                    agent.DriverType = Agent.eDriverType.WindowsAutomation;
                    break;
                case ePlatformType.Java:
                    agent.DriverType = Agent.eDriverType.JavaDriver;
                    break;
                case ePlatformType.MainFrame:
                    agent.DriverType = Agent.eDriverType.MainFrame3270;
                    break;
                default:
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "No default driver set for first agent");
                    break;
            }

            agent.AgentOperations.InitDriverConfigs();
            WorkSpace.Instance.SolutionRepository.AddRepositoryItem(agent);
        }
    }
}