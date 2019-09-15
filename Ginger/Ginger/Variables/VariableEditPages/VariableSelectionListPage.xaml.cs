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
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore.Variables;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Variables
{
    /// <summary>
    /// Interaction logic for VariableSelectionListPage.xaml
    /// </summary>
    public partial class VariableSelectionListPage : Page
    {
        private VariableSelectionList mVar;
        public VariableSelectionListPage(VariableSelectionList var)
        {
            InitializeComponent();

            mVar = var;
            mVar.PropertyChanged += mVar_PropertyChanged;

            SetOptionalValuesGridData();
            SetOptionalValuesGridView();
            //events
            grdOptionalValues.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddOptionalValue));
            grdOptionalValues.btnDelete.AddHandler(Button.ClickEvent, new RoutedEventHandler(btnDelete_Click));
            grdOptionalValues.btnClearAll.AddHandler(Button.ClickEvent, new RoutedEventHandler(btnClearAll_Click));
            grdOptionalValues.grdMain.RowEditEnding += grdOptionalValues_RowEditEnding;
            grdOptionalValues.Grid.IsVisibleChanged += grdOptionalValues_IsVisibleChanged;

            comboSelectedValue.ItemsSource = mVar.OptionalValuesList;
            comboSelectedValue.DisplayMemberPath = nameof(OptionalValue.Value);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(comboSelectedValue, ComboBox.TextProperty, mVar, nameof(VariableSelectionList.SelectedValue));
            //comboSelectedValue.BindControl(mVar, nameof(VariableSelectionList.SelectedValue), mVar.OptionalValuesList.ToList<OptionalValue>());
        }

        private void SetOptionalValuesGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(OptionalValue.Value), WidthWeight = 10 });
            grdOptionalValues.SetAllColumnsDefaultView(view);
            grdOptionalValues.InitViewItems();
        }

        private void SetOptionalValuesGridData()
        {
            grdOptionalValues.DataSourceList = mVar.OptionalValuesList;
        }

        private void AddOptionalValue(object sender, RoutedEventArgs e)
        {
            if (grdOptionalValues.Grid.CurrentItem != null)
                grdOptionalValues.Grid.CommitEdit();
            grdOptionalValues.Grid.SelectedItem = null;
            grdOptionalValues.Grid.CurrentItem = null;   

            OptionalValue newVal = new OptionalValue(string.Empty);
            mVar.OptionalValuesList.Add(newVal);

            grdOptionalValues.Grid.SelectedItem = newVal;
            grdOptionalValues.Grid.CurrentItem = newVal;           
            UpdateOptionalValues();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            UpdateOptionalValues();
            grdOptionalValues.DataSourceList = mVar.OptionalValuesList;
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            UpdateOptionalValues();
            grdOptionalValues.DataSourceList = mVar.OptionalValuesList;
        }

        private void grdOptionalValues_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            UpdateOptionalValues();
        }

        private void grdOptionalValues_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateOptionalValues();
        }

        private void UpdateOptionalValues()
        {            
            mVar.OnPropertyChanged(nameof(VariableSelectionList.Formula));
            VerifySelectedValue();
        }

        void VerifySelectedValue()
        {
            //make sure the selected value is valid
            if (mVar.OptionalValuesList != null && mVar.OptionalValuesList.Count > 0)
            {
                if (mVar.SelectedValue == string.Empty || mVar.OptionalValuesList.Where(v => v.Value == mVar.SelectedValue).SingleOrDefault() == null)
                {
                    mVar.SelectedValue = mVar.OptionalValuesList[0].Value;
                }
            }
        }

        private void mVar_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableSelectionList.OptionalValues))
            {
                SetOptionalValuesGridData();
                UpdateOptionalValues();
            }
        }
    }
}
