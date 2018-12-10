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
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.UserControls;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ginger.UserControlsLib.ActionInputValueUserControlLib
{
    /// <summary>
    /// Interaction logic for ActionInputValueUserControl.xaml
    /// </summary>
    public partial class ActionInputValueUserControl : UserControl
    {
        ActInputValue mActInputValue;

        public ActionInputValueUserControl(ActInputValue actInputValue)
        {
            InitializeComponent();

            mActInputValue = actInputValue;

            ResetControls();

            SetControlToInputValue();
        }

        private void ResetControls()
        {
            xTextBoxInputPnl.Visibility = Visibility.Collapsed;
            xComboBoxInputPnl.Visibility = Visibility.Collapsed;
            xCheckBoxInput.Visibility = Visibility.Collapsed;
            xListInputGrid.Visibility = Visibility.Collapsed;
        }

        private void SetControlToInputValue()
        {           
            // simple string or number or unknown type show text box
            if  (mActInputValue.ParamType == typeof(string) || mActInputValue.ParamType == typeof(int) || mActInputValue.ParamType == null)
            {
                xTextBoxInputPnl.Visibility = Visibility.Visible;
                xTextBoxInputLabel.Content = string.Format("{0}:", GetInputFieldformatedName());
                xTextBoxInputTextBox.Init(mActInputValue, nameof(ActInputValue.Value));          
                return;
            }

            if (mActInputValue.ParamType.IsEnum)
            {
                xComboBoxInputPnl.Visibility = Visibility.Visible;
                xComboBoxInputLabel.Content = string.Format("{0}:", GetInputFieldformatedName());
                xComboBoxInputComboBox.BindControl(mActInputValue, nameof(ActInputValue.Value));
                return;
            }

            if (mActInputValue.ParamType == typeof(bool))
            {
                xCheckBoxInput.Visibility = Visibility.Visible;
                xCheckBoxInput.Content =  GetInputFieldformatedName();
                xCheckBoxInput.BindControl(mActInputValue, nameof(ActInputValue.Value));
                return;
            }

            if (mActInputValue.ParamType == typeof(DynamicListWrapper))
            {
                xListInputGrid.Visibility = Visibility.Visible;
                xListInputGrid.Title = GetInputFieldformatedName();          
                
                //set data
                ObservableList<dynamic> DynList = mActInputValue.ListDynamicValue;
                xListInputGrid.DataSourceList = DynList;

                //data changes catch
                DynList.CollectionChanged += ListCollectionChanged;
                xListInputGrid.Grid.CellEditEnding += Grid_CellEditEnding;

                xListInputGrid.btnAdd.Click += AddItem;
                SetListGridView();
                return;
            }
        }

        private string GetInputFieldformatedName()
        {
            // Make first letter upper case
            string formatedName = char.ToUpper(mActInputValue.Param[0]) + mActInputValue.Param.Substring(1);

            //split by Uper case
            string[] split = Regex.Split(formatedName, @"(?<!^)(?=[A-Z])");
            formatedName = string.Empty;
            foreach (string str in split)
            {
                formatedName += str + " ";
            }

            return (formatedName.Trim());
        }

        #region List Grid Handlers

        void SetListGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;

            // Create grid columns based on list item properties
            List<string> props = mActInputValue.GetListItemProperties();
            foreach (string prop in props)
            {
                viewCols.Add(new GridColView() { Field = prop, WidthWeight = 10 });
                viewCols.Add(new GridColView() { Field = prop + "VE", Header = "...", WidthWeight = 1, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PagePanel.Resources["ValueExpressionButton"] });
            }

            xListInputGrid.SetAllColumnsDefaultView(view);
            xListInputGrid.InitViewItems();
        }

        private void GridVEButton_Click(object sender, RoutedEventArgs e)
        {
            dynamic currentListItem = (dynamic)xListInputGrid.CurrentItem;
            //get name of relevent field
            int currentColIndex = xListInputGrid.Grid.CurrentColumn.DisplayIndex;
            object field = xListInputGrid.Grid.Columns[currentColIndex - 1].Header;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(currentListItem, field.ToString());
            VEEW.ShowAsWindow();
            UpdateListValues();
        }

        private void Grid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            UpdateListValues();
        }

        private void ListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateListValues();
        }

        private void UpdateListValues()
        {
            ObservableList<dynamic> list = (ObservableList<dynamic>)xListInputGrid.DataSourceList;
            mActInputValue.ListDynamicValue = list;
        }

        private void AddItem(object sender, RoutedEventArgs e)
        {
            dynamic expando = new ExpandoObject();
            // TODO set obj with item default value - expando.Name = "";
            ((ObservableList<dynamic>)xListInputGrid.DataSourceList).Add(expando);
        }
        #endregion


    }
}
