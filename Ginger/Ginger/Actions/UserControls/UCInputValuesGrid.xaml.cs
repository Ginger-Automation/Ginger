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
using Ginger.UserControls;
using System.Windows;
using System.Windows.Controls;
using Amdocs.Ginger.Repository;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class UCInputValuesGrid : UserControl
    {
        public ObservableList<ActInputValue> DataSource
        {
            get;
            set;
        }

        Context mContext = null;

        public UCInputValuesGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize and Bind the UCGrid, also defines the Titles and sets the AddRow Handler.
        /// </summary>
        /// <param name="dataSource">The List which all the ActInputValue are saved</param>
        /// <param name="gridTitle">Sets the Grid Title</param>
        /// <param name="paramTitle">Sets the Parameter Title</param>
        /// <param name="valueTitle">Sets the Value Title</param>
        /// <param name="valueForDriverTitle">Sets the ValueForDriver(Calculated Value) Title</param>
        public void Init(Context context, ObservableList<ActInputValue> dataSource, string gridTitle = "Input Values", string paramTitle = "Parameter Name", string valueTitle = "Parameter Value", string valueForDriverTitle = "Calculated Parameter Value")
        {
            mContext = context;
            DataSource = dataSource;
            SetVEGrid(gridTitle, paramTitle, valueTitle,  valueForDriverTitle);
        }

        public void SetVEGrid(string gridTitle, string paramTitle, string valueTitle, string valueForDriverTitle)
        {
            VEGrid.Title = gridTitle;
            VEGrid.SetTitleStyle((Style)TryFindResource("@ucGridTitleLightStyle"));

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Param), Header = paramTitle, WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Value), Header = valueTitle, WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.controlGrid.Resources["VEGridValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.ValueForDriver), Header = valueForDriverTitle, WidthWeight = 100 });
            VEGrid.SetAllColumnsDefaultView(view);
            VEGrid.InitViewItems();
            VEGrid.DataSourceList = DataSource;
            VEGrid.ShowRefresh = Visibility.Collapsed;
            VEGrid.ShowUpDown = Visibility.Collapsed;
            VEGrid.ShowEdit = Visibility.Collapsed;
            VEGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddRow));
        }

         private void VEGridInputGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = (ActInputValue)VEGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, nameof(ActInputValue.Value), mContext);
            VEEW.ShowAsWindow();
        }

        private void AddRow(object sender, RoutedEventArgs e)
        {
            DataSource.Add(new ActInputValue());
        }
    }
}
