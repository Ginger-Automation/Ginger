#region License
/*
Copyright © 2014-2024 European Support Limited

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
using GingerCore.Variables;
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

namespace Ginger.UserControlsLib.InputVariableRule
{
    /// <summary>
    /// Interaction logic for UCTargetVariable.xaml
    /// </summary>
    public partial class UCTargetVariable : UserControl, INotifyPropertyChanged
    {
        public static DependencyProperty TargetVariabelsProperty = DependencyProperty.Register("TargetVariabels", typeof(ObservableList<VariableBase>), typeof(UCTargetVariable), new PropertyMetadata(OnTargetVariabelPropertyChanged));

        public static DependencyProperty TargetVariabelGuidProperty = DependencyProperty.Register("TargetVariabelGuid", typeof(Guid), typeof(UCTargetVariable), new PropertyMetadata(OnTargetVariabelGuidPropertyChanged));

        public static DependencyProperty SourceVariabelGuidProperty = DependencyProperty.Register("SourceVariabelGuid", typeof(Guid), typeof(UCTargetVariable), new PropertyMetadata(OnSourceVariabelGuidPropertyChanged));

        public delegate void OperationTypeEventHandler(VariableBase EventArgs);
        private static event OperationTypeEventHandler OperationTypeEvent;
        public void OnOperationValueEvent(VariableBase variable)
        {
            OperationTypeEventHandler handler = OperationTypeEvent;
            if (handler != null)
            {
                handler(variable);
            }
        }

        public static void SetOperationvalueEvent(OperationTypeEventHandler operationTypeEvent)
        {
            if (OperationTypeEvent == null)
            {
                OperationTypeEvent -= operationTypeEvent;
                OperationTypeEvent += operationTypeEvent;
            }
        }


        public ObservableList<VariableBase> TargeteVariables
        {
            get { return (ObservableList<VariableBase>)GetValue(TargetVariabelsProperty); }
            set { SetValue(TargetVariabelsProperty, value); }
        }


        public Guid TargetVariableGuid
        {
            get { return (Guid)GetValue(TargetVariabelGuidProperty); }
            set { SetValue(TargetVariabelGuidProperty, value); }
        }

        public Guid SourceVariableGuid
        {
            get { return (Guid)GetValue(SourceVariabelGuidProperty); }
            set { SetValue(SourceVariabelGuidProperty, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public UCTargetVariable()
        {
            InitializeComponent();
        }

        private static void OnTargetVariabelPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCTargetVariable;
            if (control != null)
            {
                control.TargetVariabelsPropertyChanged((ObservableList<VariableBase>)args.NewValue);
            }
        }

        private static void OnTargetVariabelGuidPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCTargetVariable;
            if (control != null)
            {
                control.TargetVariabelGuidPropertyChanged((Guid)args.NewValue);
            }
        }

        private void TargetVariabelsPropertyChanged(ObservableList<VariableBase> targetVariableList)
        {
            OnPropertyChanged(nameof(UCTargetVariable.TargeteVariables));
            SetComboboxData(targetVariableList);
        }

        private void TargetVariabelGuidPropertyChanged(Guid guid)
        {
            OnPropertyChanged(nameof(UCTargetVariable.TargetVariableGuid));
            SetComboBoxValue(guid);
            MarkTargetVariableValidation(guid);
        }

        private void MarkTargetVariableValidation(Guid guid)
        {
            bool isValid = true;

            if(!TargeteVariables.Any(x => x.Guid == guid))
            {
                isValid = false;
            }

            if (isValid == false)
            {
                this.BorderThickness = new Thickness(1);
                this.BorderBrush = Brushes.Red;
            }
            else
            {
                this.BorderThickness = new Thickness(0);
                this.BorderBrush = null;
            }
        }


        private static void OnSourceVariabelGuidPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCTargetVariable;
            if (control != null)
            {
                control.SourceVariabelGuidPropertyChanged((Guid)args.NewValue);
            }
        }

        private void SourceVariabelGuidPropertyChanged(Guid guid)
        {            
           OnPropertyChanged(nameof(UCTargetVariable.SourceVariableGuid));          
        }

        private void SetComboBoxValue(Guid guid)
        {
            xVariablesComboBox.SelectedValue = guid;
        }


        private void SetComboboxData(ObservableList<VariableBase> variableList)
        {
            if (variableList == null || variableList.Count == 0)
            {
                xVariablesComboBox.IsEnabled = false;
            }

            if (variableList != null && variableList.Count > 0)
            {
                xVariablesComboBox.DisplayMemberPath = nameof(VariableBase.Name);
                xVariablesComboBox.SelectedValuePath = nameof(VariableBase.Guid);
                xVariablesComboBox.ItemsSource = variableList.OrderBy(nameof(VariableBase.Name));
                xVariablesComboBox.SelectionChanged +=XVariablesComboBox_SelectionChanged;
            }
        }

        private void XVariablesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.Items.Count > 0)
            {
                VariableBase variableBase = (VariableBase)(comboBox).SelectedItem;
                if (variableBase!=null)
                {
                    TargetVariableGuid = variableBase.Guid;
                    OnOperationValueEvent(variableBase);
                }
            }
        }

        public static DataTemplate GetTemplate(string targetvariablesProperty = "", string targetVariablesguidProperty = "", string sourceVariableGuidProperty = "")
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory ucTargetVariable = new FrameworkElementFactory(typeof(UCTargetVariable));

            if (string.IsNullOrEmpty(sourceVariableGuidProperty) == false)
            {
                Binding sourcevariableGuidBinding = new Binding(sourceVariableGuidProperty);
                sourcevariableGuidBinding.Mode = BindingMode.TwoWay;
                sourcevariableGuidBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                ucTargetVariable.SetBinding(UCTargetVariable.SourceVariabelGuidProperty, sourcevariableGuidBinding);
            }

            if (string.IsNullOrEmpty(targetvariablesProperty) == false)
            {
                Binding targetvariablesBinding = new Binding(targetvariablesProperty);
                targetvariablesBinding.Mode = BindingMode.OneWay;
                targetvariablesBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                ucTargetVariable.SetBinding(UCTargetVariable.TargetVariabelsProperty, targetvariablesBinding);
            }

            if (string.IsNullOrEmpty(targetVariablesguidProperty) == false)
            {
                Binding targetVariablesBinding = new Binding(targetVariablesguidProperty);
                targetVariablesBinding.Mode = BindingMode.TwoWay;
                targetVariablesBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                ucTargetVariable.SetBinding(UCTargetVariable.TargetVariabelGuidProperty, targetVariablesBinding);
            }            

            template.VisualTree = ucTargetVariable;
            return template;
        }

    }
}
