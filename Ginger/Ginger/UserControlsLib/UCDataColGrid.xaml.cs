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
using GingerCore.FlowControlLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.UserControls
{
    /// <summary>
    /// Interaction logic for UCDataColGrid.xaml
    /// </summary>
    public partial class UCDataColGrid : UserControl
    {
        private IObservableList l;
        public UCDataColGrid()
        {
            InitializeComponent();      
            this.DataContextChanged += UCDataColGrid_DataContextChanged;                        
        }

        private void UCDataColGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //TODO: check why we come here with non matching object
            if (!typeof(IObservableList).IsAssignableFrom(e.NewValue.GetType())) return; //avoid invalid cast exception

            this.Dispatcher.Invoke(() =>
            {
                l = (IObservableList)e.NewValue;
                if (l.Count == 0) return;

                // If it is input and we have only one row then no need to show the grid
                if (l[0].GetType() == typeof(ActInputValue) && l.Count == 1)
                {
                    ValueTextBox.Visibility = System.Windows.Visibility.Visible;
                    MainDataGrid2.Visibility = System.Windows.Visibility.Collapsed;

                    Binding bd = new Binding("Value");
                    ValueTextBox.DataContext = l[0];
                    ValueTextBox.SetBinding(TextBox.TextProperty, bd);
                }
                else
                {
                    MainDataGrid2.Visibility = System.Windows.Visibility.Visible;
                    ValueTextBox.Visibility = System.Windows.Visibility.Collapsed;
                    SetGridData();
                }
            });
        }

        private void SetGridData()
        {
            this.Dispatcher.Invoke(() =>
            {
                //TODO: fix me to be generic, meanwhile temp quick and dirty solution
                if (l[0].GetType() == typeof(ActInputValue))
                {
                    DataGridTextColumn DGTC = new DataGridTextColumn();
                    DGTC.Header = "Param";
                    Binding binding = new Binding("Param");
                    DGTC.Binding = binding;
                    DGTC.IsReadOnly = true;
                    this.MainDataGrid2.Columns.Add(DGTC);

                    DataGridTextColumn DGTC2 = new DataGridTextColumn();
                    DGTC2.Header = "Value";
                    Binding binding2 = new Binding("Value");
                    DGTC2.Binding = binding2;
                    this.MainDataGrid2.Columns.Add(DGTC2);

                    DataGridTextColumn DGTC3 = new DataGridTextColumn();
                    DGTC3.Header = "Run Time Value";
                    Binding binding3 = new Binding("ValueForDriver");
                    DGTC3.Binding = binding3;
                    DGTC3.IsReadOnly = true;
                    this.MainDataGrid2.Columns.Add(DGTC3);
                }

                if (l[0].GetType() == typeof(ActReturnValue))
                {
                    DataGridTextColumn DGTC = new DataGridTextColumn();
                    DGTC.Header = "Param";
                    Binding binding = new Binding("Param");
                    DGTC.Binding = binding;
                    DGTC.IsReadOnly = true;
                    this.MainDataGrid2.Columns.Add(DGTC);

                    DataGridTextColumn DGTC2 = new DataGridTextColumn();
                    DGTC2.Header = "Actual";
                    Binding binding2 = new Binding("Actual");
                    DGTC2.Binding = binding2;
                    this.MainDataGrid2.Columns.Add(DGTC2);

                    DataGridTextColumn DGTC3 = new DataGridTextColumn();
                    DGTC3.Header = "Expected";
                    Binding binding3 = new Binding("Expected");
                    DGTC3.Binding = binding3;
                    // DGTC3.IsReadOnly = true;
                    this.MainDataGrid2.Columns.Add(DGTC3);

                    DataGridTextColumn DGTC4 = new DataGridTextColumn();
                    DGTC4.Header = "Status";
                    Binding binding4 = new Binding("Status");
                    DGTC4.Binding = binding4;
                    DGTC4.IsReadOnly = true;
                    this.MainDataGrid2.Columns.Add(DGTC4);
                }

                if (l[0].GetType() == typeof(FlowControl))
                {
                    DataGridTextColumn DGTC = new DataGridTextColumn();
                    DGTC.Header = "Condition";
                    Binding binding = new Binding("Condition");
                    DGTC.Binding = binding;
                    DGTC.IsReadOnly = false;
                    this.MainDataGrid2.Columns.Add(DGTC);

                    DataGridTextColumn DGTC3 = new DataGridTextColumn();
                    DGTC3.Header = FlowControl.Fields.FlowControlAction;
                    Binding binding3 = new Binding(FlowControl.Fields.FlowControlAction);
                    DGTC3.Binding = binding3;
                    this.MainDataGrid2.Columns.Add(DGTC3);
                }
                MainDataGrid2.ItemsSource = l;
            });
        }

        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                (sender as TextBox).GetBindingExpression(TextBox.TextProperty).UpdateSource();
            });
        }
    }
}