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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.ExternalConfigurations;
using Ginger.UserControlsLib;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using Microsoft.CodeAnalysis;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using static Ginger.Environments.GingerOpsEnvWizardLib.GingerOpsAPIResponseInfo;

namespace Ginger.Environments.GingerOpsEnvWizardLib
{
    /// <summary>
    /// Interaction logic for AddGingerOpsEnvPage.xaml
    /// </summary>
    public partial class AddGingerOpsEnvPage : Page, IWizardPage
    {
        public GingerOpsConfigurationPage GingerOpsConfiguration;
        public GingerOpsAPIResponseInfo responseInfo;
        public Dictionary<string, GingerOpsProject> projectListGOps = [];
        public Dictionary<string, GingerOpsArchitectureB> architectureListGOps = [];
        public Dictionary<string, GingerOpsEnvironmentB> environmentListGOps = [];
        public GingerOpsAPI GingerOpsAPI;
        AddGingerOpsEnvWizard mWizard;

        public AddGingerOpsEnvPage()
        {
            responseInfo = new GingerOpsAPIResponseInfo();
            GingerOpsAPI = new();
            InitializeComponent();
            BindingHandler.ObjFieldBinding(xEnvironmentComboBox, MultiSelectComboBox.SelectedItemsProperty, responseInfo, nameof(GingerOpsAPIResponseInfo.GingerOpsEnvironmentB));
            xEnvironmentComboBox.Init(responseInfo, nameof(GingerOpsAPIResponseInfo.GingerOpsEnvironmentB));
        }

        private async void LoadComboBoxData()
        {
            // Fetching data from the API and populating ComboBoxes
            projectListGOps = await GingerOpsAPI.FetchProjectDataFromGOps(projectListGOps);

            xProjectComboBox.ItemsSource = projectListGOps.Values.ToList();
            xProjectComboBox.DisplayMemberPath = "Name";
            xProjectComboBox.SelectedValuePath = "Id";
        }



        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddGingerOpsEnvWizard)WizardEventArgs.Wizard;
                    mWizard.mWizardWindow.SetFinishButtonEnabled(false);
                    LoadComboBoxData();
                    foreach (var appPlat in environmentListGOps)
                    {
                        EnvApplication envApp = new EnvApplication
                        {
                            Name = appPlat.Value.Name,
                            ParentGuid = new Guid(appPlat.Key),
                            GingerOpsAppId = appPlat.Value.Id,
                            Active = true
                        };
                        mWizard.apps.Add(envApp);
                    }
                    break;
                case EventType.LeavingForNextPage:
                    xEnvironmentComboBox_SelectionChanged();
                    break;
                case EventType.Prev:
                    mWizard.mWizardWindow.SetFinishButtonEnabled(false);
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

            if (xProjectComboBox.SelectedItem is GingerOpsProject selectedProject)
            {
                // Get the project Id
                var projectId = selectedProject.Id;

                if (projectListGOps.TryGetValue(projectId, out var project))
                {
                    xArchitectureComboBox.ItemsSource = project.GingerOpsArchitecture;
                    xArchitectureComboBox.DisplayMemberPath = "Name";
                    xArchitectureComboBox.SelectedValuePath = "Id";
                }
            }
        }

        public async void xArchitectureComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            xEnvironmentComboBox.ItemsSource?.Clear();
            xEnvironmentComboBox.Text = string.Empty;

            if (xArchitectureComboBox.SelectedItem is GingerOpsArchitectureA selectedArchitecture)
            {
                string architectureId = selectedArchitecture.Id;

                // Fetch environments for the selected architecture
                architectureListGOps = await GingerOpsAPI.FetchEnvironmentDataFromGOps(architectureId, architectureListGOps);

                // Update the Environment ComboBox
                if (architectureListGOps.TryGetValue(architectureId, out var architecture))
                {
                    Dictionary<string, object> keyValuePairs = [];
                    foreach (var keyValue in architecture.GingerOpsEnvironment)
                    {
                        keyValuePairs.Add(keyValue.Name, keyValue.Id);
                    }
                    xEnvironmentComboBox.ItemsSource = keyValuePairs;
                    xEnvironmentComboBox.Name = "Name";

                }

                if (xEnvironmentComboBox.ItemsSource.IsNullOrEmpty())
                {
                    mWizard.mWizardWindow.NextButton(false);
                }
                else
                {
                    mWizard.mWizardWindow.NextButton(true);
                }
            }
        }

        public async void xEnvironmentComboBox_SelectionChanged()
        {
            try
            {
                mWizard.ImportedEnvs.Clear();
                if (xEnvironmentComboBox.SelectedItems != null)
                {
                    if (xEnvironmentComboBox.SelectedItems.Count > 0)
                    {
                        foreach (var env in xEnvironmentComboBox.SelectedItems)
                        {
                            await HandleEnvironmentSelection(env);
                        }

                        if (mWizard.ImportedEnvs.Any(k => k.GingerOpsStatus == "Import Successful"))
                        {
                            mWizard.mWizardWindow.SetFinishButtonEnabled(true);
                        }
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.AskToSelectItem);
                    mWizard.Prev();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get details from GingerOps", ex);
            }
        }

        private async Task HandleEnvironmentSelection(dynamic env)
        {
            string environmentId = env.Guid.ToString();

            mWizard.NewEnvironment = CreateNewEnvironment(env);

            // Fetch environments for the selected architecture
            environmentListGOps = await GingerOpsAPI.FetchApplicationDataFromGOps(environmentId, environmentListGOps);

            foreach (var ienv in environmentListGOps)
            {
                var appList = ienv.Value;
                await AddApplicationsToEnvironment(appList, mWizard.NewEnvironment);
            }
            bool isAlreadyLoaded = mWizard.ImportedEnvs.Any(k => k.Name == mWizard.NewEnvironment.Name && k.GingerOpsEnvId == mWizard.NewEnvironment.GingerOpsEnvId);
            if (!isAlreadyLoaded)
            {
                mWizard.ImportedEnvs.Add(mWizard.NewEnvironment);
            }
        }

        private ProjEnvironment CreateNewEnvironment(dynamic env)
        {
            bool isEnvExist = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Any(k => k.Name == env.Value);
            ProjEnvironment projEnvironment;
            if (isEnvExist)
            {
                projEnvironment = new ProjEnvironment
                {
                    Name = env.Value,
                    GingerOpsEnvId = env.Guid,
                    GOpsFlag = false,
                    Publish = true,
                    GingerOpsStatus = "Import Failed",
                    GingerOpsRemark = "Environment with Same Name already exist"
                };

            }
            else
            {
                projEnvironment = new ProjEnvironment
                {
                    Name = env.Value,
                    GingerOpsEnvId = env.Guid,
                    GOpsFlag = true,
                    Publish = true,
                    GingerOpsStatus = "Import Successful",
                    GingerOpsRemark = "Environment Imported Successfully"
                };
            }

            return projEnvironment;
        }

        private async Task AddApplicationsToEnvironment(GingerOpsAPIResponseInfo.GingerOpsEnvironmentB appList, ProjEnvironment newEnvironment)
        {
            foreach (var item in appList.GingerOpsApplications)
            {
                var platformType = GetPlatformType(item.GOpsApplicationParameters.FirstOrDefault(k => k.Name == "Application Type"));
                var appUrl = item.GOpsApplicationParameters.FirstOrDefault(k => k.Name == "Application URL")?.Value;


                var envApp = new EnvApplication
                {
                    Name = item.Name,
                    Platform = platformType,
                    GingerOpsAppId = item.Id,
                    Active = true,
                    Url = appUrl,
                    GOpsFlag = true
                };

                // Adding all other parameters to GeneralParams
                foreach (var param in item.GOpsApplicationParameters)
                {
                    if (param.Name is not "Application Type" and not "Application URL")
                    {
                        envApp.AddVariable(new VariableString() { Name = param.Name, Value = param.Value, GOpsFlag = true, SetAsInputValue = false, SetAsOutputValue = false });
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
            var existingPlatform = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(k => k.GingerOpsAppId == item.Id && k.AppName == item.Name);

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
                ? $"{appName}_GingerOps"
                : envApp.Name;

            envApp.Name = appNameWithSuffix;

            return new ApplicationPlatform
            {
                AppName = appNameWithSuffix,
                Platform = envApp.Platform,
                GingerOpsAppId = envApp.GingerOpsAppId,
                GOpsFlag = true
            };
        }

        private void UpdateExistingApplicationPlatform(ApplicationPlatform existingPlatform, EnvApplication envApp, string appName)
        {

            existingPlatform.AppName = appName;
            existingPlatform.Platform = envApp.Platform;

            // to support if any name change
            envApp.Name = existingPlatform.AppName;
            envApp.Platform = existingPlatform.Platform;
        }

    }
}
