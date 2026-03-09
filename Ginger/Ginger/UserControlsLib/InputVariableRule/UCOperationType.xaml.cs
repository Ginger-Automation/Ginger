#region License
/*
Copyright © 2014-2026 European Support Limited

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
using GingerCore.Variables;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static Ginger.Variables.InputVariableRule;

namespace Ginger.UserControlsLib.InputVariableRule
{
    /// <summary>
    /// Interaction logic for UCOperationType.xaml
    /// </summary>
    public partial class UCOperationType : UserControl, INotifyPropertyChanged
    {
        public static DependencyProperty SelectedTargetVariabelProperty = DependencyProperty.Register("SelectedTargetVariabel", typeof(VariableBase), typeof(UCOperationType), new PropertyMetadata(OnSelectedTargetVariabelPropertyChanged));

        public static DependencyProperty OperationTypeProperty = DependencyProperty.Register("OperationType", typeof(eInputVariableOperation), typeof(UCOperationType), new PropertyMetadata(OnOperationTypePropertyChanged));

        public VariableBase TargetVariable
        {
            get { return (VariableBase)GetValue(SelectedTargetVariabelProperty); }
            set { SetValue(SelectedTargetVariabelProperty, value); }
        }

        public eInputVariableOperation OperationType
        {
            get { return (eInputVariableOperation)GetValue(OperationTypeProperty); }
            set { SetValue(OperationTypeProperty, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private static void OnOperationTypePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is UCOperationType control)
            {
                control.OperationTypePropertyChanged((eInputVariableOperation)args.NewValue);
            }
        }

        private void SelectedTargetVariabelPropertyChanged(VariableBase selectedVar)
        {
            if (selectedVar != null && selectedVar.VariableType == "Selection List")
            {
                GingerCore.General.EnableComboItem(xOperationTypeComboBox, eInputVariableOperation.SetOptionalValues);
                OperationType = eInputVariableOperation.SetOptionalValues;
            }
            else
            {
                GingerCore.General.DisableComboItem(xOperationTypeComboBox, eInputVariableOperation.SetOptionalValues);
                OperationType = eInputVariableOperation.SetValue;
            }
        }

        private static void OnSelectedTargetVariabelPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is UCOperationType control)
            {
                control.SelectedTargetVariabelPropertyChanged((VariableBase)args.NewValue);
            }
        }

        private void OperationTypePropertyChanged(eInputVariableOperation operationType)
        {
            OnPropertyChanged(nameof(OperationType));
            GingerCore.General.SelectComboValue(xOperationTypeComboBox, operationType.ToString());
        }

        public UCOperationType()
        {
            InitializeComponent();
            InitOperationType();
        }

        public static DataTemplate GetTemplate(string selectedtargetvariableProperty, string operationtypePropperty)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory operationtype = new FrameworkElementFactory(typeof(UCOperationType));

            Binding selectedTargetVariableBinding = new Binding(selectedtargetvariableProperty)
            {
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            operationtype.SetBinding(UCOperationType.SelectedTargetVariabelProperty, selectedTargetVariableBinding);

            Binding operationTypebinding = new Binding(operationtypePropperty)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            operationtype.SetBinding(UCOperationType.OperationTypeProperty, operationTypebinding);

            template.VisualTree = operationtype;
            return template;
        }

        private void InitOperationType()
        {
            GingerCore.General.FillComboItemsFromEnumType(xOperationTypeComboBox, typeof(eInputVariableOperation));
            BindingHandler.ObjFieldBinding(xOperationTypeComboBox, ComboBox.SelectedValueProperty, this, nameof(OperationType));
            GingerCore.General.DisableComboItem(xOperationTypeComboBox, eInputVariableOperation.SetOptionalValues);
            xOperationTypeComboBox.SelectionChanged += XOperationTypeComboBox_SelectionChanged;
        }

        private void XOperationTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.Items.Count > 0)
            {
                OperationType = (eInputVariableOperation)comboBox.SelectedValue;
            }
        }
    }
}
