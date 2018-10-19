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
using Ginger.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
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
        public ActionInputValueUserControl()
        {
            InitializeComponent();
        }

        public void BindControl(ActInputValue AIV)
        {
            mActInputValue = AIV;
            this.ValueTextBox.Visibility = Visibility.Collapsed;
            this.ValueDataGrid.Visibility = Visibility.Collapsed;
            this.ValueComboBox.Visibility = Visibility.Collapsed;
            
            // simple string or unknown type show text box
            if  (AIV.ParamType == typeof(string) || AIV.ParamType == null)
            {
                this.ValueTextBox.Visibility = Visibility.Visible;
                this.ValueTextBox.BindControl(AIV, nameof(ActInputValue.Value));
                this.ValueTextBox.Style = App.GetStyle("@TextBoxStyle");   // TODO: use const/enum so will pass compile check             
                return;
            }

            // Int
            if (AIV.ParamType == typeof(int))
            {
                this.ValueTextBox.Visibility = Visibility.Visible;
                this.ValueTextBox.BindControl(AIV, nameof(ActInputValue.Value));
                this.ValueTextBox.Style = App.GetStyle("@TextBoxStyle");   // TODO: use const/enum so will pass compile check             

                this.ValueTextBox.Background = Brushes.Yellow;  // TODO: make it accept only numbers !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                return;
            }

            
            // List - Show in Grid
            if (AIV.ParamType == typeof(List<string>))
            {
                ValueExpressionButton.Visibility = Visibility.Collapsed;
                this.ValueDataGrid.Visibility = Visibility.Visible;
                xUCcGrid.Visibility = Visibility.Visible;
                xUCcGrid.Title = AIV.Param;                
                ObservableList<dynamic> DynList = mActInputValue.ListStringValue; //  ll.GetList();
                DynList.CollectionChanged += ListCollectionChanged;
                xUCcGrid.DataSourceList = DynList;
                xUCcGrid.btnAdd.Click += AddItem;
                SetListGridView();
                return;
            }

            


            if (AIV.ParamType.IsEnum)
            {
                this.ValueComboBox.Visibility = Visibility.Visible;
                // TODO: get the enum values and fill combo
                this.ValueComboBox.BindControl(AIV, nameof(ActInputValue.Value));
                this.ValueComboBox.Style = App.GetStyle("@ComboBoxStyle");   // TODO: use const/enum so will pass compile check             
                return;
            }
        }

        private void AddItem(object sender, RoutedEventArgs e)
        {
            dynamic expando = new ExpandoObject();
            expando.Name = "";
            expando.Country = "";

            ((ObservableList<dynamic>)xUCcGrid.DataSourceList).Add(expando);
        }

        private void ListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ObservableList<dynamic> list = (ObservableList<dynamic>)xUCcGrid.DataSourceList;
            mActInputValue.ListStringValue = list;
        }

        void SetListGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;
            viewCols.Add(new GridColView() { Field = "Name", WidthWeight = 250 });
            viewCols.Add(new GridColView() { Field = "Country", WidthWeight = 250 });
            viewCols.Add(new GridColView() { Field = "value", WidthWeight = 50 });
            xUCcGrid.SetAllColumnsDefaultView(view);
            xUCcGrid.InitViewItems();
        }

      
        private void ValueExpressionButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage valueExpressionEditorPage = new ValueExpressionEditorPage(mActInputValue, nameof(ActInputValue.Value));
            valueExpressionEditorPage.ShowAsWindow();
        }
    }
}
