#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Drivers.Common;
using GingerCore.Platforms;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.WindowExplorer.Common
{
    /// <summary>
    /// Interaction logic for WindowExplorerPOMPage.xaml
    /// </summary>
    public partial class WindowExplorerPOMPage : Page
    {
        ObservableList<Act> actions = new ObservableList<Act>();
        ApplicationAgent mApplicationAgent;

        public WindowExplorerPOMPage(ApplicationAgent ApplicationAgent)
        {
            InitializeComponent();

            mApplicationAgent = ApplicationAgent;

            AppNameLabel.Content = mApplicationAgent.AppName;

            //BFNameLabel.Content = App.BusinessFlow.Name;


            //ActivityComboBox.ItemsSource = App.BusinessFlow.Activities;
            ActivityComboBox.DisplayMemberPath = nameof(Activity.ActivityName);

            Binding b = new Binding();
            //b.Source = App.BusinessFlow;
            b.Path = new PropertyPath("CurrentActivity");
            ActivityComboBox.SetBinding(ComboBox.SelectedValueProperty, b);

            FillPOMS();
            
            ActionsDataGrid.DataSourceList = actions;

            SetControlsGridView();

            SetPOMActivitiesGrid();

            SetActionsDataGridView();
        }

        private void SetPOMActivitiesGrid()
        {
            //Set the Tool Bar look
            POMActivitiesGrid.ShowAdd = Visibility.Collapsed;
            POMActivitiesGrid.ShowClearAll = Visibility.Collapsed;
            POMActivitiesGrid.ShowUpDown = Visibility.Collapsed;
            POMActivitiesGrid.ShowEdit = System.Windows.Visibility.Collapsed;
            POMActivitiesGrid.ShowDelete = System.Windows.Visibility.Collapsed;

            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            
            view.GridColsView.Add(new GridColView() { Field = nameof(Activity.ActivityName) , WidthWeight = 100 });

            // TODO: fix me temp input on desc - need to add input vars for activity
            view.GridColsView.Add(new GridColView() { Field = nameof(Activity.Description), Header = "Input", WidthWeight = 200 });   //!!!!!!!!!!!!!!!!!!!!!!!!!!!
            view.GridColsView.Add(new GridColView() { Field = nameof(Activity.Screen), Header = "Output", WidthWeight = 200 });//!!!!!!!!!!!!!!!!!!!!!!!!!!!

            POMActivitiesGrid.SetAllColumnsDefaultView(view);
            POMActivitiesGrid.InitViewItems();
        }

        private void SetActionsDataGridView()
        {
            //Set the Tool Bar look
            ActionsDataGrid.ShowAdd = Visibility.Collapsed;
            ActionsDataGrid.ShowClearAll = Visibility.Collapsed;
            ActionsDataGrid.ShowUpDown = Visibility.Collapsed;            
            ActionsDataGrid.ShowEdit = System.Windows.Visibility.Collapsed;
            ActionsDataGrid.ShowDelete = System.Windows.Visibility.Collapsed;
            
            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            
            view.GridColsView.Add(new GridColView() { Field = nameof(ActUIElement.Description), WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActUIElement.LocateBy), WidthWeight = 50 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActUIElement.LocateValue), WidthWeight = 50 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActUIElement.Value), WidthWeight = 100 });
            ActionsDataGrid.SetAllColumnsDefaultView(view);
            ActionsDataGrid.InitViewItems();
        }

        private void SetControlsGridView()
        {
            //Set the Tool Bar look
            ElementsGrid.ShowAdd = Visibility.Collapsed;
            ElementsGrid.ShowClearAll = Visibility.Collapsed;
            ElementsGrid.ShowUpDown = Visibility.Collapsed;
            //TODO: enable refresh to do refresh

            ElementsGrid.ShowEdit = System.Windows.Visibility.Collapsed;
            ElementsGrid.ShowDelete = System.Windows.Visibility.Collapsed;

            //TODO: add button to show all...        

            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementName), WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementType), WidthWeight = 60 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Mandatory), WidthWeight = 60 });
            
            ElementsGrid.SetAllColumnsDefaultView(view);
            ElementsGrid.InitViewItems();
        }

        private void FillPOMS()
        {
            //List<ApplicationPOM> list = new List<ApplicationPOM>();
            //string folder = Path.Combine( WorkSpace.Instance.Solution.Folder, @"Applications\" + mApplicationAgent.AppName + @"\Page Objects Models\" );  // TODO: use const or POM helper
            //string[] POMFolders = Directory.GetDirectories(folder);


            //string ext = RepositoryItem.FileExt(typeof(ApplicationPOM));
            //foreach (string f in POMFolders)
            //{
            //    string fol = Path.GetFileNameWithoutExtension(f);
            //    string filename = Path.Combine(f, fol + "." + ext + ".xml");
            //    ApplicationPOM pom = (ApplicationPOM)RepositoryItem.LoadFromFile(typeof(ApplicationPOM), filename);
            //    list.Add(pom);
            //}
            

            //POMNameComboBox.DisplayMemberPath = "Name";
            //POMNameComboBox.ItemsSource = list;
        }

        internal void ActionRecorded(object sender, POMEventArgs e)
        {
            //// if we didn't find the EI in the model we create new one
            ////TODO: might need to add a flag if auto create/add
            

            ////TODO: add ways to identify the page and correlate to POM
        }

        private void LearnButton_Click(object sender, RoutedEventArgs e)
        {
            ////TODO: temp fix hard coded !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }

        private void POMNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshPOMInfo();
        }

        private void RefreshPOMInfo()
        {
            
        }
    }
}
