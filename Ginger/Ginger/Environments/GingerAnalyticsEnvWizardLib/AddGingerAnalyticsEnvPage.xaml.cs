using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using Amdocs.Ginger.Common;
using amdocs.ginger.GingerCoreNET;
using Ginger.ExternalConfigurations;
using System.Linq;
using static Ginger.Environments.GingerAnalyticsEnvWizardLib.GingerAnalyticsAPIResponseInfo;
using Microsoft.CodeAnalysis;
using GingerCore.Environments;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Ginger.UserControlsLib;
using GingerCore.GeneralLib;
using System.Windows;
using Amdocs.Ginger.Repository;
using GingerCore.Variables;

namespace Ginger.Environments.GingerAnalyticsEnvWizardLib
{
    /// <summary>
    /// Interaction logic for AddGingerAnalyticsEnvPage.xaml
    /// </summary>
    public partial class AddGingerAnalyticsEnvPage : Page,IWizardPage
    {
        public GingerAnalyticsConfigurationPage gingerAnalyticsConfiguration;
        public GingerAnalyticsAPIResponseInfo responseInfo;
        public Dictionary<string, GingerAnalyticsProject> projectListGA = new();
        public Dictionary<string, GingerAnalyticsArchitectureB> architectureListGA = new();
        public Dictionary<string, GingerAnalyticsEnvironmentB> environmentListGA = new();
        public GingerAnalyticsAPI GingerAnalyticsAPI { get; private set; }
        AddGingerAnalyticsEnvWizard mWizard;
        private string bearerToken = string.Empty;

        public AddGingerAnalyticsEnvPage()
        {
            responseInfo = new GingerAnalyticsAPIResponseInfo();
            GingerAnalyticsAPI = new();
            InitializeComponent();
            LoadComboBoxData();
            BindingHandler.ObjFieldBinding(xEnvironmentComboBox, MultiSelectComboBox.SelectedItemsProperty,responseInfo, nameof(GingerAnalyticsAPIResponseInfo.GingerAnalyticsEnvironmentB));

        }

        private async void LoadComboBoxData()
        {
            // Fetching data from the API and populating ComboBoxes
            projectListGA = await GingerAnalyticsAPI.FetchProjectDataFromGA(projectListGA);

            xProjectComboBox.ItemsSource = projectListGA.Values.ToList();
            xProjectComboBox.DisplayMemberPath = "Name";  
            xProjectComboBox.SelectedValuePath = "Id";    

        }

        

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddGingerAnalyticsEnvWizard)WizardEventArgs.Wizard;

                    foreach (var appPlat in environmentListGA)
                    {
                        EnvApplication envApp = new EnvApplication() { Name = appPlat.Value.Name, ParentGuid = new Guid(appPlat.Key), GingerAnalyticsAppId=appPlat.Value.Id };
                        envApp.Active = true;
                        mWizard.apps.Add(envApp);
                    }
                break;
                case EventType.LeavingForNextPage:
                    xEnvironmentComboBox_SelectionChanged();
                break;
                default:
                    break;

            }
        }

        private void xProjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(xEnvironmentComboBox.Text))
            {
                xEnvironmentComboBox.ItemsSource.Clear();
            }

            if (xProjectComboBox.SelectedItem is GingerAnalyticsProject selectedProject)
            {
                // Get the project Id
                var projectId = selectedProject.Id;

                if (projectListGA.TryGetValue(projectId, out var project))
                {
                    xArchitectureComboBox.ItemsSource = project.GingerAnalyticsArchitecture;
                    xArchitectureComboBox.DisplayMemberPath = "Name";
                    xArchitectureComboBox.SelectedValuePath = "Id";
                }
            }
        }

        public async void xArchitectureComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xArchitectureComboBox.SelectedItem is GingerAnalyticsArchitectureA selectedArchitecture)
            {
                string architectureId = selectedArchitecture.Id;

                // Fetch environments for the selected architecture
                architectureListGA = await GingerAnalyticsAPI.FetchEnvironmentDataFromGA(architectureId, architectureListGA);

                // Update the Environment ComboBox
                if (architectureListGA.TryGetValue(architectureId, out var architecture))
                {
                    Dictionary<string,object> keyValuePairs = new Dictionary<string,object>();
                    foreach (var keyValue in architecture.GingerAnalyticsEnvironment)
                    {
                        keyValuePairs.Add(keyValue.Name,keyValue.Id);
                    }
                    xEnvironmentComboBox.ItemsSource = keyValuePairs;
                    xEnvironmentComboBox.Name = "Name";
                    
                    xEnvironmentComboBox.Init(responseInfo, nameof(GingerAnalyticsAPIResponseInfo.GingerAnalyticsEnvironmentB));
                }
            }
        }

        public async void xEnvironmentComboBox_SelectionChanged()
        {
            try
            {
                foreach (var env in xEnvironmentComboBox.SelectedItems)
                {
                    await HandleEnvironmentSelection(env);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get details from Ginger Analytics", ex);
            }
        }

        private async Task HandleEnvironmentSelection(dynamic env)
        {
            string environmentId = env.Guid.ToString();

            mWizard.NewEnvironment = CreateNewEnvironment(env);

            // Fetch environments for the selected architecture
            environmentListGA = await GingerAnalyticsAPI.FetchApplicationDataFromGA(environmentId, environmentListGA);

            foreach (var ienv in environmentListGA)
            {
                var appList = ienv.Value;
                await AddApplicationsToEnvironment(appList, mWizard.NewEnvironment);
            }

            mWizard.ImportedEnvs.Add(mWizard.NewEnvironment);
        }

        private ProjEnvironment CreateNewEnvironment(dynamic env)
        {
            return new ProjEnvironment
            {
                Name = env.Value,
                GingerAnalyticsEnvId = env.Guid,
                GAFlag = true
            };
        }

        private async Task AddApplicationsToEnvironment(GingerAnalyticsAPIResponseInfo.GingerAnalyticsEnvironmentB appList, ProjEnvironment newEnvironment)
        {
            foreach (var item in appList.GingerAnalyticsApplications)
            {
                var platformType = GetPlatformType(item.GAApplicationParameters.FirstOrDefault(k => k.Name == "Application Type"));
                var appUrl = item.GAApplicationParameters.FirstOrDefault(k => k.Name == "Application URL")?.Value;


                var envApp = new EnvApplication
                {
                    Name = item.Name,
                    Platform = platformType,
                    GingerAnalyticsAppId = item.Id,
                    Active = true,
                    Url = appUrl,
                    GingerAnalyticsStatus = item.Status,
                    GingerAnalyticsRemark = "SuccessFully Imported"
                };

                // Adding all other parameters to GeneralParams
                foreach (var param in item.GAApplicationParameters)
                {
                    if (param.Name != "Application Type" && param.Name != "Application URL")
                    {
                        envApp.AddVariable(new VariableString() { Name = param.Name, Value = param.Value });
                    }
                }
                newEnvironment.Applications.Add(envApp);
                await AddOrUpdateApplicationPlatform(envApp, item);
            }
        }

        private ePlatformType GetPlatformType(dynamic item)
        {
            var applicationType = item?.Value;

            return applicationType switch
            {
                "Web" => ePlatformType.Web,
                "Mobile" => ePlatformType.Mobile,
                "Unix" => ePlatformType.Unix,
                "ASCF" => ePlatformType.ASCF,
                "DOS" => ePlatformType.DOS,
                "VBScript" => ePlatformType.VBScript,
                "WebServices" => ePlatformType.WebServices,
                "PowerBuilder" => ePlatformType.PowerBuilder,
                "Java" => ePlatformType.Java,
                "MainFrame" => ePlatformType.MainFrame,
                "Service" => ePlatformType.Service,
                "Windows" => ePlatformType.Windows,
                "NA" => ePlatformType.NA,
                _ => throw new ArgumentException($"Unknown platform type: {applicationType}")
            };
        }

        private async Task AddOrUpdateApplicationPlatform(EnvApplication envApp, dynamic item)
        {
            var existingPlatform = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(k => k.GingerAnalyticsAppId == item.Id);

            if (existingPlatform == null)
            {
                var newPlatform = CreateNewApplicationPlatform(envApp, item.Name);
                mWizard.tempAppPlat.Add(newPlatform);
            }
            else
            {
                UpdateExistingApplicationPlatform(existingPlatform, envApp, item.Name);
            }
        }

        private ApplicationPlatform CreateNewApplicationPlatform(EnvApplication envApp, string appName)
        {
            var appNameWithSuffix = WorkSpace.Instance.Solution.ApplicationPlatforms.Any(app => app.AppName.Equals(appName))
                ? $"{appName}_GingerAnalytics"
                : envApp.Name;

            return new ApplicationPlatform
            {
                AppName = appNameWithSuffix,
                Platform = envApp.Platform,
                GingerAnalyticsAppId = envApp.GingerAnalyticsAppId
            };
        }

        private void UpdateExistingApplicationPlatform(ApplicationPlatform existingPlatform, EnvApplication envApp, string appName)
        {
            var appNameWithSuffix = WorkSpace.Instance.Solution.ApplicationPlatforms.Any(app => app.AppName.Equals(appName))
                ? $"{appName}_GingerAnalytics"
                : appName;

            existingPlatform.AppName = appNameWithSuffix;
            existingPlatform.Platform = envApp.Platform;
        }

    }
}
