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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.ApplicationModelsLib.POMModels
{
    /// <summary>
    /// Interaction logic for POMModelsPage.xaml
    /// </summary>
    public partial class POMModelsPage : Page
    {

        RepositoryFolder<ApplicationPOMModel> mPOMsFolder;

        public POMModelsPage(RepositoryFolder<ApplicationPOMModel> POMsFolder)
        {
            InitializeComponent();
            mPOMsFolder = POMsFolder;

            SetAPIModelGridData();
            SetAPIModelsGridView();
        }

        private void SetAPIModelGridData()
        {
            if (mPOMsFolder.IsRootFolder)
                xPOMModelsGrid.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>();
            else
                xPOMModelsGrid.DataSourceList = mPOMsFolder.GetFolderItems();
        }

        private void SetAPIModelsGridView()
        {
            xPOMModelsGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationAPIModel.Name), Header = "Name", ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationAPIModel.Description), Header = "Description", ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(RepositoryItem.RelativeFilePath), Header = "Local File Path", ReadOnly = true, BindingMode = BindingMode.OneWay });

            xPOMModelsGrid.SetAllColumnsDefaultView(view);
            xPOMModelsGrid.InitViewItems();

            xPOMModelsGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGridDataHandler));
            xPOMModelsGrid.ShowTagsFilter = Visibility.Visible;
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
