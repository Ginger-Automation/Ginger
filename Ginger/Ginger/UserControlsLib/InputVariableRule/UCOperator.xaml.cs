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

using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using static Ginger.Variables.InputVariableRule;

namespace Ginger.UserControlsLib.InputVariableRule
{
    /// <summary>
    /// Interaction logic for UCOperator.xaml
    /// </summary>
    public partial class UCOperator : UserControl, INotifyPropertyChanged
    {
        public static DependencyProperty OperatorProperty = DependencyProperty.Register("Operator", typeof(eInputVariableOperator), typeof(UCOperator), new PropertyMetadata(OnOperatorPropertyChanged));

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public eInputVariableOperator Operator
        {
            get { return (eInputVariableOperator)GetValue(OperatorProperty); }
            set { SetValue(OperatorProperty, value); }
        }

        private static void OnOperatorPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCOperator;
            if (control != null)
            {
                control.OperationTypePropertyChanged((eInputVariableOperator)args.NewValue);
            }
        }

        private void OperationTypePropertyChanged(eInputVariableOperator oprator)
        {
            OnPropertyChanged(nameof(Operator));
            GingerCore.General.SelectComboValue(xOperatorComboBox, oprator.ToString());
        }

        public UCOperator()
        {
            InitializeComponent();
            InitOperationType();
        }        

        public static DataTemplate GetTemplate(string operatorProperty)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory mOperator = new FrameworkElementFactory(typeof(UCOperator));
          
            Binding operatorbinding = new Binding(operatorProperty);
            operatorbinding.Mode = BindingMode.TwoWay;
            operatorbinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            mOperator.SetBinding(UCOperator.OperatorProperty, operatorbinding);

            template.VisualTree = mOperator;
            return template;
        }

        private void InitOperationType()
        {
            GingerCore.General.FillComboFromEnumType(xOperatorComboBox, typeof(eInputVariableOperator));
            BindingHandler.ObjFieldBinding(xOperatorComboBox, ComboBox.SelectedValueProperty, this, nameof(Operator));
            xOperatorComboBox.SelectionChanged+=xOperatorComboBox_SelectionChanged;
        }

        private void xOperatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.Items.Count > 0)
            {
                Operator = (eInputVariableOperator)comboBox.SelectedValue;
            }
        }
    }
}
