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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Ginger.UserControls;
using GingerCore.Environments;
using GingerCore;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;

namespace Ginger.Environments
{
    /// <summary>
    /// Interaction logic for AppsListPage.xaml
    /// </summary>
    public partial class AppsListPage : Page
    {
        public ProjEnvironment AppEnvironmnet { get; set; }
        public AppsListPage(ProjEnvironment env)
        {
            InitializeComponent();

            AppEnvironmnet = env;
            //Set grid look and data
            SetGridView();
            SetGridData();

            App.ObjFieldBinding(EnvNameTextBox, TextBox.TextProperty, env, ProjEnvironment.Fields.Name);

            grdApps.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddApp));          
            grdApps.AddToolbarTool("@Share_16x16.png", "Add Selected Applications to All Environments", new RoutedEventHandler(AddAppsToOtherEnvironments));

            TagsViewer.Init(AppEnvironmnet.Tags);
        }       

        private void AddApp(object sender, RoutedEventArgs e)
        {
            EnvApplication app = new EnvApplication();
            app.Name = "New";
            app.Active = true;  
            AppEnvironmnet.Applications.Add(app);
        }
        
        #region Functions
        private void SetGridView()
        {
            //Set the grid name
            grdApps.Title = "'" + AppEnvironmnet.Name + "' Environment Applications";
            grdApps.SetTitleLightStyle = true;

            //Set the Tool Bar look
            grdApps.ShowUpDown = Visibility.Collapsed;
            grdApps.ShowUndo = Visibility.Visible;
            
            //Set the Data Grid columns
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = EnvApplication.Fields.Active, WidthWeight = 100, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = EnvApplication.Fields.Name, WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = EnvApplication.Fields.Description, WidthWeight = 200 });
            view.GridColsView.Add(new GridColView() { Field = EnvApplication.Fields.Vendor, WidthWeight = 50 });
            view.GridColsView.Add(new GridColView() { Field = EnvApplication.Fields.CoreVersion, WidthWeight = 100, Header = "Core Version" });
            view.GridColsView.Add(new GridColView() { Field = EnvApplication.Fields.CoreProductName, WidthWeight = 150, Header = "Core Product Name" });
            view.GridColsView.Add(new GridColView() { Field = EnvApplication.Fields.AppVersion, WidthWeight = 150, Header = "Application Version" });
            view.GridColsView.Add(new GridColView() { Field = EnvApplication.Fields.Url, WidthWeight = 100, Header = "URL" });
            
            grdApps.SetAllColumnsDefaultView(view);
            grdApps.InitViewItems();
        }

        private void SetGridData()
        {            
            grdApps.DataSourceList = AppEnvironmnet.Applications;
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
                        if (env != AppEnvironmnet)
                        {
                            if (env.Applications.Where(x => x.Name == ((EnvApplication)obj).Name).FirstOrDefault() == null)
                            {
                                EnvApplication app = (EnvApplication)(((RepositoryItemBase)obj).CreateCopy());
                                env.Applications.Add(app);                                
                                appsWereAdded = true;
                            }
                        }
                    }
                }

                if (appsWereAdded)
                    Reporter.ToUser(eUserMsgKeys.ShareEnvAppWithAllEnvs);
            }
            else
                Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
        }

        #endregion Functions
    }
}
