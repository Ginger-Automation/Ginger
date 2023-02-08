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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore;
using GingerCoreNET.SolutionRepositoryLib;
using GingerWPF.UserControlsLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GingerWPF.ApplicationModelsLib.APIModels
{
    public partial class APIModelsPage : Page
    {
        RepositoryFolder<ApplicationAPIModel> mApisFolder;

        public APIModelsPage(RepositoryFolder<ApplicationAPIModel> apisFolder)
        {
            InitializeComponent();

            mApisFolder = apisFolder;

            SetAPIModelGridData();
            SetAPIModelsGridView();
        }

        private void SetAPIModelGridData()
        {
            if (mApisFolder.IsRootFolder)
                xAPIModelsGrid.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationAPIModel>();
            else
                xAPIModelsGrid.DataSourceList = mApisFolder.GetFolderItems();
        }

        private void SetAPIModelsGridView()
        {
            xAPIModelsGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationAPIModel.Name), Header = "Name", ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationAPIModel.Description), Header = "Description", ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(RepositoryItem.RelativeFilePath), Header = "Local File Path", ReadOnly=true, BindingMode=BindingMode.OneWay });

            xAPIModelsGrid.SetAllColumnsDefaultView(view);
            xAPIModelsGrid.InitViewItems();

            xAPIModelsGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGridDataHandler));
            xAPIModelsGrid.ShowTagsFilter = Visibility.Visible;
        }

        private void RefreshGridDataHandler(object sender, RoutedEventArgs e)
        {
            SetAPIModelGridData();
        }

        public void ShowAsWindow()
        {
        }
    }
}
