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

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for UcColorPickerList.xaml
    /// </summary>
    public partial class UcColorPickerList : UserControl
    {
        private static readonly int COLUMS = 5;

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(nameof(SelectedColor), typeof(string), typeof(UcColorPickerList), new PropertyMetadata(null, new PropertyChangedCallback(OnSelectedColorPropertyChanged)));
        public string SelectedColor
        {
            get
            {
                return (string)GetValue(SelectedColorProperty);
            }
            set
            {
                SetValue(SelectedColorProperty, value);
                xColorsCombo.SelectedValue = value;
            }
        }
        private static void OnSelectedColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as UcColorPickerList;
            if (control != null && e.NewValue != null)
            {
                control.SelectedColor = ((string)e.NewValue);
            }
        }

        public UcColorPickerList()
        {
            InitializeComponent();

            xColorsCombo.SelectedValuePath = "Name";
            xColorsCombo.ItemsSource = typeof(Colors).GetProperties();
        }

        private void xPanelGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid != null)
            {
                if (grid.RowDefinitions.Count == 0)
                {
                    for (int r = 0; r <= xColorsCombo.Items.Count / COLUMS; r++)
                    {
                        grid.RowDefinitions.Add(new RowDefinition());
                    }
                }
                if (grid.ColumnDefinitions.Count == 0)
                {
                    for (int c = 0; c < Math.Min(xColorsCombo.Items.Count, COLUMS); c++)
                    {
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                    }
                }
                for (int i = 0; i < grid.Children.Count; i++)
                {
                    Grid.SetColumn(grid.Children[i], i % COLUMS);
                    Grid.SetRow(grid.Children[i], i / COLUMS);
                }
            }
        }

        private void XColorsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xColorsCombo.IsDropDownOpen == false)
            {
                e.Handled = true;
                return;
            }

                if (xColorsCombo.SelectedValue != null)
            {
                SelectedColor = xColorsCombo.SelectedValue.ToString();
            }
        }

        private void XColorsCombo_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (xColorsCombo.IsDropDownOpen == false)
            {
                e.Handled = true;
                return;
            }
        }
    }
}
