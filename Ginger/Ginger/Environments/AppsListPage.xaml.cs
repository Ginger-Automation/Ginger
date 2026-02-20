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
using Amdocs.Ginger.CoreNET.Run.SolutionCategory;
using Amdocs.Ginger.Repository;
using Ginger.Environments.GingerOpsEnvWizardLib;
using Ginger.ExternalConfigurations;
using Ginger.SolutionWindows.TreeViewItems.EnvironmentsTreeItems;
using Ginger.UserControls;
using Ginger.UserControlsLib;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static Ginger.Environments.GingerOpsEnvWizardLib.GingerOpsAPIResponseInfo;

namespace Ginger.Environments
{
    /// <summary>
    /// Interaction logic for AppsListPage.xaml
    /// </summary>
    public partial class AppsListPage : GingerUIPage
    {
        public ProjEnvironment AppEnvironment { get; set; }
        public AddGingerOpsEnvPage AddGingerOpsEnvPage;
        public GingerOpsAPI GingerOpsAPI;
        public AppsListPage(ProjEnvironment env)
        {
            InitializeComponent();

            AppEnvironment = env;
            CurrentItemToSave = AppEnvironment;
            //Set grid look and data
            SetGridView();
            SetGridData();

            if (AppEnvironment.GOpsFlag)
            {
                xPublishcheckbox.IsEnabled = false;
                EnvNameTextBox.IsEnabled = false;
                xGASyncBtn.Visibility = Visibility.Visible;
            }
            else
            {
                EnvNameTextBox.IsEnabled = true;
                xPublishcheckbox.IsEnabled = true;
                xGASyncBtn.Visibility = Visibility.Collapsed;
                InitReleaseComboBox();
            }

            grdApps.AddToolbarTool("@Share_16x16.png", "Add Selected Applications to All Environments", new RoutedEventHandler(AddAppsToOtherEnvironments));
            BindingHandler.ObjFieldBinding(EnvNameTextBox, TextBox.TextProperty, env, ProjEnvironment.Fields.Name);
            EnvNameTextBox.AddValidationRule(new EnvironemntNameValidationRule());
            xShowIDUC.Init(AppEnvironment);
            BindingHandler.ObjFieldBinding(xPublishcheckbox, CheckBox.IsCheckedProperty, AppEnvironment, nameof(RepositoryItemBase.Publish));


            grdApps.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddApp));

            TagsViewer.Init(AppEnvironment.Tags);
        }

        private void AddApp(object sender, RoutedEventArgs e)
        {
            var ApplicationPlatforms = WorkSpace.Instance.Solution.ApplicationPlatforms.Where((app) => !AppEnvironment.CheckIfApplicationPlatformExists(app.Guid, app.AppName))?.ToList();

            ObservableList<ApplicationPlatform> DisplayedApplicationPlatforms = GingerCore.General.ConvertListToObservableList(ApplicationPlatforms);

            EnvironmentApplicationList applicationList = new(DisplayedApplicationPlatforms);
            applicationList.ShowAsWindow();

            IEnumerable<ApplicationPlatform> SelectedApplications = DisplayedApplicationPlatforms.Where((displayedApp) => displayedApp.Selected);

            AppEnvironment.AddApplications(SelectedApplications);
        }

        #region Functions

        private void InitReleaseComboBox()
        {
            ObservableList<SolutionCategoryValue> combList = SolutionGeneral.SolutionOperations.GetSolutionReleaseValues();
            xReleaseCombobox.BindControl(AppEnvironment, nameof(ProjEnvironment.ReleaseVersion), combList, nameof(SolutionCategoryValue.Value), nameof(SolutionCategoryValue.Guid));
            BindingHandler.ObjFieldBinding(xReleaseCombobox, ComboBox.SelectedValueProperty, AppEnvironment, nameof(ProjEnvironment.ReleaseVersion));
        }
        private void SetGridView()
        {
            //Set the grid name
            grdApps.Title = GetGrdAppsTitle();
            grdApps.SetTitleLightStyle = true;

            //Set the Tool Bar look
            grdApps.ShowUpDown = Visibility.Collapsed;
            grdApps.ShowUndo = Visibility.Visible;

            //Set the Data Grid columns
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
                [ new GridColView() { Field = nameof(EnvApplication.ItemImageType), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16 },
                  new GridColView() { Field = nameof(EnvApplication.Name), WidthWeight = 100 },
                  // Show friendly description (e.g. "Mobile/TV")
                  new GridColView() { Field = nameof(EnvApplication.PlatformDescription), Header = "Platform Type", WidthWeight = 100, ReadOnly = true, BindingMode = BindingMode.OneWay },
                  new GridColView() { Field = nameof(EnvApplication.Description), WidthWeight = 200 },
                  new GridColView() { Field = nameof(EnvApplication.AppVersion), WidthWeight = 150, Header = "Application Version" },
                  new GridColView() { Field = nameof(EnvApplication.Url), WidthWeight = 100, Header = "URL" },
                ]
            };
        }

        private void SetGridData()
        {
            AppEnvironment.Applications.ForEach((app) => app.SetDataFromAppPlatform(WorkSpace.Instance.Solution.ApplicationPlatforms));
            grdApps.DataSourceList = AppEnvironment.Applications;

            if (AppEnvironment.GOpsFlag)
            {
                grdApps.DisableGridColoumns();
                grdApps.btnClearAll.IsEnabled = false;
                grdApps.btnDelete.IsEnabled = false;
            }
        }

        private void AddAppsToOtherEnvironments(object sender, RoutedEventArgs e)
        {
            bool appsWereAdded = false;

            if (grdApps.Grid.SelectedItems.Count > 0)
            {
                foreach (object obj in grdApps.Grid.SelectedItems)
                {
                    ObservableList<ProjEnvironment> envs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>();
                    foreach (ProjEnvironment env in envs)
                    {
                        if (env != AppEnvironment)
                        {
                            if (env.Applications.FirstOrDefault(x => x.Name == ((EnvApplication)obj).Name) == null)
                            {
                                env.StartDirtyTracking();
                                EnvApplication app = (EnvApplication)(((RepositoryItemBase)obj).CreateCopy());
                                env.Applications.Add(app);
                                appsWereAdded = true;
                            }
                        }
                    }
                }

                if (appsWereAdded)
                {
                    Reporter.ToUser(eUserMsgKey.ShareEnvAppWithAllEnvs);
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        #endregion Functions

        private void EnvNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            grdApps.Title = GetGrdAppsTitle();
        }

        private string GetGrdAppsTitle()
        {
            string grdEnvName = General.EscapeAccessKey(EnvNameTextBox.Text);
            return $"'{grdEnvName}' Environment Applications";
        }

        private async void xGASyncBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                xGASyncBtn.IsEnabled = false;
                ShowLoader();
                AddGingerOpsEnvPage = new();
                GingerOpsAPI = new();
                AddGingerOpsEnvPage.environmentListGOps = await GingerOpsAPI.FetchApplicationDataFromGOps(AppEnvironment.GingerOpsEnvId.ToString(), AddGingerOpsEnvPage.environmentListGOps);

                foreach (var appEnv in AddGingerOpsEnvPage.environmentListGOps)
                {
                    var appListEnv = appEnv.Value.GingerOpsApplications;
                    foreach (var item in appListEnv)
                    {
                        var existingApp = AppEnvironment.Applications.FirstOrDefault(k => k.GingerOpsAppId == item.Id);
                        if (existingApp != null)
                        {
                            UpdateExistingApplication(existingApp, item);
                        }
                        else
                        {
                            AddNewApplication(item);
                        }
                    }
                }
                Reporter.ToUser(eUserMsgKey.GingerOpsSyncSuccess);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Sync with GingerOps", ex);
                Reporter.ToUser(eUserMsgKey.GingerOpsSyncFailed, ex.Message);
            }
            finally
            {
                HideLoader();
                xGASyncBtn.IsEnabled = true;
            }
        }

        private void UpdateExistingApplication(EnvApplication existingApp, GingerOpsApplication item)
        {
            bool parametersChanged = false;

            // Check if the name, platform, or URL has changed
            if (!existingApp.Name.Equals(item.Name) ||
                existingApp.Platform != MapToPlatformType(item.GOpsApplicationParameters.FirstOrDefault(k => k.Name == "Application Type")?.Value) ||
                !existingApp.Url.Equals(item.GOpsApplicationParameters.FirstOrDefault(k => k.Name == "Application URL")?.Value))
            {
                existingApp.Name = item.Name;
                existingApp.Platform = MapToPlatformType(item.GOpsApplicationParameters.FirstOrDefault(k => k.Name == "Application Type")?.Value);
                existingApp.Url = item.GOpsApplicationParameters.FirstOrDefault(k => k.Name == "Application URL")?.Value;
                parametersChanged = true;
            }

            // Create a hash set of existing parameters in GeneralParams
            var existingParamNames = new HashSet<string>(existingApp.Variables.Select(v => v.Name));

            // Check if any other parameters have changed or new parameters added
            foreach (var param in item.GOpsApplicationParameters)
            {
                if (param.Name is not "Application Type" and not "Application URL")
                {
                    if (!existingParamNames.Contains(param.Name) || existingApp.Variables.FirstOrDefault(v => v.Name == param.Name)?.Value != param.Value)
                    {
                        parametersChanged = true;
                        break;
                    }
                }
            }

            // Only update variables if parameters have actually changed
            if (parametersChanged)
            {
                existingApp.Variables.ClearAll();
                foreach (var param in item.GOpsApplicationParameters)
                {
                    if (param.Name is not "Application Type" and not "Application URL")
                    {
                        existingApp.AddVariable(new VariableString() { Name = param.Name, Value = param.Value, GOpsFlag = true, SetAsInputValue = false, SetAsOutputValue = false });
                    }
                }
            }

            UpdateApplicationPlatform(existingApp, item);
        }

        private void AddNewApplication(GingerOpsApplication item)
        {
            if (!string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(item.Id))
            {
                var platformType = MapToPlatformType(item.GOpsApplicationParameters.FirstOrDefault(k => k.Name == "Application Type")?.Value);
                var appUrl = item.GOpsApplicationParameters.FirstOrDefault(k => k.Name == "Application URL")?.Value;

                EnvApplication newEnvApp = new() { Name = item.Name, Platform = platformType, GingerOpsAppId = item.Id, Active = true, Url = appUrl, GOpsFlag = true };

                // Add all other parameters to GeneralParams
                foreach (var param in item.GOpsApplicationParameters)
                {
                    if (param.Name is not "Application Type" and not "Application URL")
                    {
                        newEnvApp.AddVariable(new VariableString() { Name = param.Name, Value = param.Value, GOpsFlag = true, SetAsInputValue = false, SetAsOutputValue = false });
                    }
                }

                AppEnvironment.Applications.Add(newEnvApp);

                UpdateApplicationPlatform(newEnvApp, item);
            }
        }

        private void UpdateApplicationPlatform(EnvApplication app, GingerOpsApplication item)
        {
            var existingPlatform = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(k => k.GingerOpsAppId == item.Id);
            if (existingPlatform == null)
            {
                AddApplicationPlatform(app, item);
            }
            else
            {
                existingPlatform.AppName = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(k => k.AppName == item.Name && k.GingerOpsAppId != item.Id) == null
                    ? item.Name : item.Name + "_GingerOps";
                existingPlatform.Platform = app.Platform;

                app.Name = existingPlatform.AppName;
            }
        }

        private void AddApplicationPlatform(EnvApplication app, GingerOpsApplication item)
        {
            var selectedApp = new ApplicationPlatform
            {
                AppName = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(k => k.AppName == item.Name) != null
                    ? item.Name + "_GingerOps" : item.Name,
                Platform = app.Platform,
                GingerOpsAppId = item.Id,
                GOpsFlag = true
            };

            app.Name = selectedApp.AppName;
            WorkSpace.Instance.Solution.ApplicationPlatforms.Add(selectedApp);
        }

        private ePlatformType MapToPlatformType(string applicationType)
        {
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

        private void HideLoader()
        {
            this.Dispatcher.Invoke(() =>
            {
                xProcessingImage.Visibility = Visibility.Hidden;
            });
        }

        private void ShowLoader()
        {
            this.Dispatcher.Invoke(() =>
            {
                xProcessingImage.Visibility = Visibility.Visible;
            });
        }
    }
}
