#region License
/*
Copyright © 2014-2023 European Support Limited

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
using System.Windows;
using System.Windows.Controls;
using Ginger.UserControls;
using GingerCore.Environments;
using GingerCore;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.Environments
{
    /// <summary>
    /// Interaction logic for EnvsListPage.xaml
    /// </summary>
    public partial class EnvsListPage : Page
    {
        RepositoryFolder<ProjEnvironment> mFolder;        

        public EnvsListPage(RepositoryFolder<ProjEnvironment> folder)
        {
            InitializeComponent();
            mFolder = folder;

            //Set grid look and data
            SetGridView();
            SetGridData();
            grdEnvs.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(Refresh));
        }

        
        #region Functions
        private void SetGridView()
        {
            //Set the Tool Bar look
            grdEnvs.ShowEdit = Visibility.Collapsed;
            grdEnvs.ShowUpDown = Visibility.Collapsed;
            grdEnvs.ShowTagsFilter = Visibility.Visible;
            grdEnvs.ShowEdit = Visibility.Collapsed;
            grdEnvs.ShowRefresh = Visibility.Collapsed;
            grdEnvs.SetTitleLightStyle=true;

            //Set the Data Grid columns
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            
            view.GridColsView.Add(new GridColView() { Field = ProjEnvironment.Fields.Name, WidthWeight = 200 });          
            view.GridColsView.Add(new GridColView() { Field = ProjEnvironment.Fields.ReleaseVersion, WidthWeight = 80, Header = "Release Version" });
            view.GridColsView.Add(new GridColView() { Field = ProjEnvironment.Fields.Notes, WidthWeight = 500 });
            view.GridColsView.Add(new GridColView() { Field = nameof(RepositoryItemBase.FileName), Header="Local File Path", WidthWeight = 250 });

            grdEnvs.SetAllColumnsDefaultView(view);
            grdEnvs.InitViewItems();
        }

        private void SetGridData()
        {
            if (mFolder.IsRootFolder)
                grdEnvs.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>();
            else
                grdEnvs.DataSourceList = mFolder.GetFolderItems();
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            SetGridData();
        }
        #endregion Functions
    }
}
