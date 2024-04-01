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
using Amdocs.Ginger.CoreNET.Run.SolutionCategory;
using Amdocs.Ginger.Repository;
using Ginger.SolutionWindows.TreeViewItems.EnvironmentsTreeItems;
using Ginger.UserControls;
using Ginger.UserControlsLib;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Microsoft.VisualStudio.Services.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Environments
{
    /// <summary>
    /// Interaction logic for AppsListPage.xaml
    /// </summary>
    public partial class AppsListPage : GingerUIPage
    {
        public ProjEnvironment AppEnvironment { get; set; }
        public AppsListPage(ProjEnvironment env)
        {
            InitializeComponent();

            AppEnvironment = env;
            CurrentItemToSave = AppEnvironment;
            //Set grid look and data
            SetGridView();
            SetGridData();

            BindingHandler.ObjFieldBinding(EnvNameTextBox, TextBox.TextProperty, env, ProjEnvironment.Fields.Name);
            EnvNameTextBox.AddValidationRule(new EnvironemntNameValidationRule());
            xShowIDUC.Init(AppEnvironment);
            BindingHandler.ObjFieldBinding(xPublishcheckbox, CheckBox.IsCheckedProperty, AppEnvironment, nameof(RepositoryItemBase.Publish));

            InitReleaseComboBox();

            grdApps.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddApp));
            grdApps.AddToolbarTool("@Share_16x16.png", "Add Selected Applications to All Environments", new RoutedEventHandler(AddAppsToOtherEnvironments));

            TagsViewer.Init(AppEnvironment.Tags);
        }

        private void AddApp(object sender, RoutedEventArgs e)
        {
            var ApplicationPlatforms = WorkSpace.Instance.Solution.ApplicationPlatforms.Where((app) => !AppEnvironment.CheckIfApplicationPlatformExists(app.Guid, app.AppName))?.ToList();



            if (ApplicationPlatforms?.Count == 0)
            {

                Reporter.ToUser(eUserMsgKey.NoApplicationPlatformLeft, AppEnvironment.Name);
                return;
            }

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
            grdApps.Title = $"'{AppEnvironment.Name}' Environment Applications";
            grdApps.SetTitleLightStyle = true;

            //Set the Tool Bar look
            grdApps.ShowUpDown = Visibility.Collapsed;
            grdApps.ShowUndo = Visibility.Visible;

            //Set the Data Grid columns
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(EnvApplication.Active), WidthWeight = 100, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(EnvApplication.ItemImageType), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16 });
            view.GridColsView.Add(new GridColView() { Field = nameof(EnvApplication.Name), WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(EnvApplication.Platform), Header="Platform Type",WidthWeight = 100 });

            view.GridColsView.Add(new GridColView() { Field = nameof(EnvApplication.Description), WidthWeight = 200 });
            view.GridColsView.Add(new GridColView() { Field = nameof(EnvApplication.AppVersion), WidthWeight = 150, Header = "Application Version" });
            view.GridColsView.Add(new GridColView() { Field = nameof(EnvApplication.Url), WidthWeight = 100, Header = "URL" });

            grdApps.SetAllColumnsDefaultView(view);
            grdApps.InitViewItems();
        }

        private void SetGridData()
        {
            AppEnvironment.Applications.ForEach((app) => app.SetDataFromAppPlatform(WorkSpace.Instance.Solution.ApplicationPlatforms));
            grdApps.DataSourceList = AppEnvironment.Applications;
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
            grdApps.Title = $"'{EnvNameTextBox.Text}' Environment Applications";
        }
    }
}
