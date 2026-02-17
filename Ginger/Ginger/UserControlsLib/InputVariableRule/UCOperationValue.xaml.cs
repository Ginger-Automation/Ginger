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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.VariablesLib;
using Amdocs.Ginger.Repository;
using Ginger.Variables;
using GingerCore.GeneralLib;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static Ginger.Variables.InputVariableRule;

namespace Ginger.UserControlsLib.InputVariableRule
{
    /// <summary>
    /// Interaction logic for UCOperationValue.xaml
    /// </summary>
    public partial class UCOperationValue : UserControl, INotifyPropertyChanged
    {
        public static DependencyProperty SelectedTargetVariabelProperty_OV =
        DependencyProperty.Register("SelectedTargetVariabel_OV", typeof(VariableBase), typeof(UCOperationValue), new PropertyMetadata(OnSelectedTargetVariabelPropertyChanged_OV));

        public static DependencyProperty OperationTypeProperty_OV =
        DependencyProperty.Register("OperationType_OV", typeof(eInputVariableOperation), typeof(UCOperationValue), new PropertyMetadata(OnOperationTypePropertyChanged_OV));

        public static DependencyProperty OperationValueProperty =
        DependencyProperty.Register("OperationValue", typeof(string), typeof(UCOperationValue), new PropertyMetadata(OnOperationValuePropertyChanged));

        public static DependencyProperty OperationSelectedValuesProperty =
            DependencyProperty.Register("OperationSelectedValues", typeof(ObservableList<OperationValues>), typeof(UCOperationValue), new PropertyMetadata(OnOperationSelectedValuesPropertyChanged));

        public VariableBase TargetVariable
        {
            get { return (VariableBase)GetValue(SelectedTargetVariabelProperty_OV); }
            set { SetValue(SelectedTargetVariabelProperty_OV, value); }
        }

        public eInputVariableOperation OperationType
        {
            get { return (eInputVariableOperation)GetValue(OperationTypeProperty_OV); }
            set { SetValue(OperationTypeProperty_OV, value); }
        }

        public string OperationValue
        {
            get { return (string)GetValue(OperationValueProperty); }
            set { SetValue(OperationValueProperty, value); }
        }


        public ObservableList<OperationValues> OperationSelectedValues
        {
            get { return (ObservableList<OperationValues>)GetValue(OperationSelectedValuesProperty); }
            set { SetValue(OperationSelectedValuesProperty, value); }
        }


        private ObservableList<OptionalValue> mVariableValuesList = [];
        public ObservableList<OptionalValue> VariableValuesList
        {
            get
            {
                return mVariableValuesList;
            }
            set
            {
                mVariableValuesList = value;
                OnPropertyChanged(nameof(VariableValuesList));
            }
        }

        private Dictionary<string, object> _items;

        public Dictionary<string, object> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;

            }
        }


        private void xButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedItems = string.Join(",", OperationSelectedValues.Select(x => x.Value).ToArray());
            MessageBoxResult result = MessageBox.Show(selectedItems, "Confirmation");
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

        private static void OnOperationTypePropertyChanged_OV(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is UCOperationValue control)
            {
                control.OperationTypePropertyChanged_OV((eInputVariableOperation)args.NewValue);
            }
        }

        private void SelectedTargetVariabelPropertyChanged_OV(VariableBase targetVar)
        {
            TargetVariable = targetVar;
            OnPropertyChanged(nameof(TargetVariable));
            if (targetVar != null)
            {
                if (targetVar.VariableType == "Selection List")
                {
                    GingerCore.General.EnableComboItem(xOperationTypeComboBox, eInputVariableOperation.SetOptionalValues);
                    OperationType = eInputVariableOperation.SetOptionalValues;
                }
                else
                {
                    GingerCore.General.DisableComboItem(xOperationTypeComboBox, eInputVariableOperation.SetOptionalValues);
                    OperationType = eInputVariableOperation.SetValue;
                }
                OperationValue = targetVar.Value;
                SetOperationValueControls();
            }
        }

        private static void OnSelectedTargetVariabelPropertyChanged_OV(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is UCOperationValue control)
            {
                control.SelectedTargetVariabelPropertyChanged_OV((VariableBase)args.NewValue);
            }
        }

        private void OperationTypePropertyChanged_OV(eInputVariableOperation operationType)
        {
            OnPropertyChanged(nameof(OperationType));
            OperationType = operationType;
            SetOperationValueControls();
        }

        private static void OnOperationSelectedValuesPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is UCOperationValue control)
            {
                control.OperationSelectedValuesPropertyChanged((ObservableList<OperationValues>)args.NewValue);
            }
        }

        private void OperationSelectedValuesPropertyChanged(ObservableList<OperationValues> oprationSelectedValues)
        {
            OnPropertyChanged(nameof(OperationSelectedValues));
            OperationSelectedValues = oprationSelectedValues;
            SetOperationValueControls();
        }

        private void SetOperationValueControls()
        {
            if (OperationType == eInputVariableOperation.SetVisibility)
            {
                xSetValueTxtBox.ClearControlsBindings();
                xPossibleValues.ClearControlsBindings();
                txtNumberValue.ClearControlsBindings();
                xPossibleValues.Visibility = Visibility.Collapsed;
                xSetValueTxtBox.Visibility = Visibility.Collapsed;
                xVisibilityOptions.Visibility = Visibility.Visible;
                dateWindow.Visibility = Visibility.Collapsed;
                txtNumberValue.Visibility = Visibility.Collapsed;
                BindingHandler.ObjFieldBinding(xVisibilityOptions, ComboBox.SelectedValueProperty, this, nameof(OperationValue));
                xVisibilityOptions.SelectionChanged += XVisibilityOptions_SelectionChanged;
                MC.Visibility = Visibility.Collapsed;

            }
            if (OperationType == eInputVariableOperation.SetValue)
            {
                xVisibilityOptions.ClearControlsBindings();
                xVisibilityOptions.Visibility = Visibility.Collapsed;
                MC.Visibility = Visibility.Collapsed;
                if (TargetVariable != null && TargetVariable.VariableType == "Selection List")
                {
                    xSetValueTxtBox.ClearControlsBindings();
                    txtNumberValue.ClearControlsBindings();
                    xSetValueTxtBox.Visibility = Visibility.Collapsed;
                    xPossibleValues.Visibility = Visibility.Visible;
                    dateWindow.Visibility = Visibility.Collapsed;
                    txtNumberValue.Visibility = Visibility.Collapsed;
                    BindingHandler.ObjFieldBinding(xPossibleValues, ComboBox.SelectedValueProperty, this, nameof(OperationValue));
                    xPossibleValues.SelectionChanged += XPossibleValues_SelectionChanged;
                    xPossibleValues.ItemsSource = ((VariableSelectionList)TargetVariable).OptionalValuesList.Select(x => x.Value).ToList();
                }
                else if (TargetVariable != null && TargetVariable.VariableType == "DateTime")
                {
                    xSetValueTxtBox.ClearControlsBindings();
                    xPossibleValues.ClearControlsBindings();
                    txtNumberValue.ClearControlsBindings();
                    xSetValueTxtBox.Visibility = Visibility.Collapsed;
                    xPossibleValues.Visibility = Visibility.Collapsed;
                    dateWindow.Visibility = Visibility.Visible;
                    txtNumberValue.Visibility = Visibility.Collapsed;
                    if (!string.IsNullOrEmpty(((VariableDateTime)TargetVariable).InitialDateTime))
                    {
                        dpDate.Value = Convert.ToDateTime(((VariableDateTime)TargetVariable).InitialDateTime);
                    }

                    dpDate.CustomFormat = ((VariableDateTime)TargetVariable).DateTimeFormat;
                    dpDate.MinDate = Convert.ToDateTime(((VariableDateTime)TargetVariable).MinDateTime);
                    dpDate.MaxDate = Convert.ToDateTime(((VariableDateTime)TargetVariable).MaxDateTime);
                }
                else if (TargetVariable != null && TargetVariable.VariableType == "Number")
                {
                    xSetValueTxtBox.ClearControlsBindings();
                    xPossibleValues.ClearControlsBindings();
                    xSetValueTxtBox.Visibility = Visibility.Collapsed;
                    xPossibleValues.Visibility = Visibility.Collapsed;
                    dateWindow.Visibility = Visibility.Collapsed;
                    txtNumberValue.Visibility = Visibility.Visible;
                    BindingHandler.ObjFieldBinding(txtNumberValue, TextBox.TextProperty, this, nameof(OperationValue));
                    txtNumberValue.AddValidationRule(new NumberValidationRule());
                    txtNumberValue.AddValidationRule(new NumberValidationRule((VariableNumber)TargetVariable));
                }
                else if (TargetVariable != null)
                {
                    xPossibleValues.ClearControlsBindings();
                    txtNumberValue.ClearControlsBindings();
                    xPossibleValues.Visibility = Visibility.Collapsed;
                    dateWindow.Visibility = Visibility.Collapsed;
                    txtNumberValue.Visibility = Visibility.Collapsed;
                    BindingHandler.ObjFieldBinding(xSetValueTxtBox, TextBox.TextProperty, this, nameof(OperationValue));
                    xSetValueTxtBox.Visibility = Visibility.Visible;
                }
            }
            if (TargetVariable != null && TargetVariable.VariableType == "Selection List" && OperationType == eInputVariableOperation.SetOptionalValues)
            {
                xVisibilityOptions.ClearControlsBindings();
                xSetValueTxtBox.ClearControlsBindings();
                xPossibleValues.ClearControlsBindings();
                txtNumberValue.ClearControlsBindings();
                xPossibleValues.Visibility = Visibility.Collapsed;
                xVisibilityOptions.Visibility = Visibility.Collapsed;
                dateWindow.Visibility = Visibility.Collapsed;
                txtNumberValue.Visibility = Visibility.Collapsed;
                Items = [];
                foreach (OptionalValue optionalValue in ((VariableSelectionList)TargetVariable).OptionalValuesList)
                {
                    if (!string.IsNullOrEmpty(optionalValue.Value) && !Items.ContainsKey(optionalValue.Value))
                    {
                        Items.Add(optionalValue.Value, optionalValue.Guid.ToString());
                    }
                }
                MC.Visibility = Visibility.Visible;
                MC.ItemsSource = Items;
                MC.Init(this, nameof(OperationSelectedValues));
                xSetValueTxtBox.Visibility = Visibility.Collapsed;
            }
        }



        private void XPossibleValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.Items.Count > 0 && comboBox.SelectedValue != null)
            {
                OperationValue = comboBox.SelectedValue.ToString();
            }
        }

        private void XVisibilityOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.Items.Count > 0)
            {
                OperationValue = comboBox.SelectedValue.ToString();
            }
        }

        private static void OnOperationValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is UCOperationValue control)
            {
                control.OperationValuePropertyChanged((string)args.NewValue);
            }
        }

        private void OperationValuePropertyChanged(string newValue)
        {
            OnPropertyChanged(nameof(OperationValue));
            if (TargetVariable != null && OperationType != eInputVariableOperation.SetVisibility)
            {
                if (TargetVariable.VariableType == "Selection List")
                {
                    xPossibleValues.SelectedValue = newValue;
                }
                else if (TargetVariable.VariableType == "String")
                {
                    xSetValueTxtBox.Text = newValue;
                }
                else if (TargetVariable.VariableType == "DateTime" && !string.IsNullOrEmpty(newValue))
                {
                    DateTime dt;
                    DateTime.TryParseExact(newValue, ((VariableDateTime)TargetVariable).DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
                    if (dt != DateTime.MinValue)
                    {
                        dpDate.Value = dt;
                    }
                }
                else if (TargetVariable.VariableType == "Number")
                {
                    txtNumberValue.Text = newValue;
                }
            }
        }


        public UCOperationValue()
        {
            InitializeComponent();
            InitOperationConfiguration();
        }

        public void InitOperationConfiguration()
        {
            GingerCore.General.FillComboItemsFromEnumType(xVisibilityOptions, typeof(eVisibilityOptions));
            xVisibilityOptions.SelectionChanged += XVisibilityOptions_SelectionChanged;
            UCTargetVariable.SetOperationvalueEvent(OperationValueEvent);

            BindingHandler.ObjFieldBinding(xOperationTypeComboBox, ComboBox.SelectedValueProperty, this, nameof(OperationType));
            GingerCore.General.FillComboItemsFromEnumType(xOperationTypeComboBox, typeof(eInputVariableOperation));
            xOperationTypeComboBox.SelectionChanged += XOperationTypeComboBox_SelectionChanged;

            GingerCore.General.DisableComboItem(xOperationTypeComboBox, eInputVariableOperation.SetOptionalValues);
        }


        private void OperationValueEvent(VariableBase variable)
        {
            TargetVariable = variable;
        }

        private void XOperationTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.Items.Count > 0)
            {
                OperationType = (eInputVariableOperation)comboBox.SelectedValue;
            }
        }

        public static DataTemplate GetTemplate(string selectedtargetvariableProperty, string operationtypePropperty, string operationValueProperty, string operationValuesListProperty = "", string operationSelectedValuesProperty = "")
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory operationValue = new FrameworkElementFactory(typeof(UCOperationValue));

            Binding selectedTargetVariableBinding_OV = new Binding(selectedtargetvariableProperty)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            operationValue.SetBinding(UCOperationValue.SelectedTargetVariabelProperty_OV, selectedTargetVariableBinding_OV);

            Binding operationTypebinding_OV = new Binding(operationtypePropperty)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            operationValue.SetBinding(UCOperationValue.OperationTypeProperty_OV, operationTypebinding_OV);

            Binding operationValueBinding = new Binding(operationValueProperty)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            operationValue.SetBinding(UCOperationValue.OperationValueProperty, operationValueBinding);


            if (!string.IsNullOrEmpty(operationSelectedValuesProperty))
            {
                Binding operationSelectedValuesBinding = new Binding(operationSelectedValuesProperty)
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                operationValue.SetBinding(UCOperationValue.OperationSelectedValuesProperty, operationSelectedValuesBinding);
            }

            template.VisualTree = operationValue;
            return template;
        }

        private void dpDate_TextChanged(object sender, EventArgs e)
        {
            if (!((VariableDateTime)TargetVariable).CheckDateTimeWithInRange(dpDate.Value.ToString()))
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Input Value is not in range:- Maximum date :[{((VariableDateTime)TargetVariable).MaxDateTime}], Minimum Date:[{((VariableDateTime)TargetVariable).MinDateTime}]");
                dpDate.Focus();
                return;
            }
            else
            {
                OperationValue = dpDate.Text.ToString();
            }
        }
    }
}
