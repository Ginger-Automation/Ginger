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

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for UCTriggerValue.xaml
    /// </summary>
    public partial class UCTriggerValue : UserControl, INotifyPropertyChanged
    {       
        public static DependencyProperty SelectedSourceVariabelProperty = DependencyProperty.Register("SelectedSourceVariabel", typeof(VariableBase), typeof(UCTriggerValue), new PropertyMetadata(OnSelectedSourceVariabelPropertyChanged));
       
        public static DependencyProperty TriggerValueProperty = DependencyProperty.Register("TriggerValue", typeof(string), typeof(UCTriggerValue), new PropertyMetadata(OnTriggerValuePropertyChanged));
      
        public VariableBase SourceVariable
        {
            get { return (VariableBase)GetValue(SelectedSourceVariabelProperty); }
            set { SetValue(SelectedSourceVariabelProperty, value); }
        }

        public string TriggerValue
        {
            get { return (string)GetValue(TriggerValueProperty); }
            set { SetValue(TriggerValueProperty, value); }
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


        public UCTriggerValue()
        {
            InitializeComponent();
        }

        private static void OnSelectedSourceVariabelPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCTriggerValue;
            if (control != null)
            {
                control.SelectedSourceVariabelPropertyChanged((VariableBase)args.NewValue);
            }
        }

        private void SelectedSourceVariabelPropertyChanged(VariableBase selectedVar)
        {
            if (selectedVar != null && selectedVar.VariableType == "Selection List")
            {
                xValueExpressionTxtbox.Visibility = Visibility.Collapsed;
                xVariablesValuesComboBox.Visibility = Visibility.Visible;             
                xVariablesValuesComboBox.ItemsSource = ((VariableSelectionList)selectedVar).OptionalValuesList.Select(x=>x.Value).ToList();
                xVariablesValuesComboBox.SelectionChanged += XVariablesValuesComboBox_SelectionChanged;                
            }

            if (selectedVar.VariableType == "String")
            {
                xVariablesValuesComboBox.Visibility = Visibility.Collapsed;
                xValueExpressionTxtbox.Visibility = Visibility.Visible;
                xValueExpressionTxtbox.Text = selectedVar.Value;
                xValueExpressionTxtbox.TextChanged += XValueExpressionTxtbox_TextChanged;                               
            }            
        }

       
        private void XValueExpressionTxtbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txtBox = (TextBox)sender;
            if (txtBox.Text !=null)
            {
                string var = txtBox.Text;
                TriggerValue = var;
            }
        }
               
        private static void OnTriggerValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCTriggerValue;
            if (control != null)
            {
                control.TriggerValuePropertyChanged((string)args.NewValue);
            }
        }

        private void TriggerValuePropertyChanged(string newValue)
        {
            OnPropertyChanged(nameof(TriggerValue));
            if (SourceVariable != null && SourceVariable.VariableType == "Selection List")
            {                
                xVariablesValuesComboBox.SelectedValue = newValue;
            }

            if (SourceVariable.VariableType == "String")
            {
                xValueExpressionTxtbox.Text = newValue;
            }
        }

        private void XVariablesValuesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.Items.Count > 0)
            {
                string var = (comboBox).SelectedItem.ToString();
                TriggerValue = var;
            }
        }

        private static void OnInputVariabelsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCTriggerValue;
            if (control != null)
            {
                control.SelectedSourceVariabelPropertyChanged((VariableBase)args.NewValue);
            }
        }

        public static DataTemplate GetTemplate(string sourcevariable, string triggervalue)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory triggerValue = new FrameworkElementFactory(typeof(UCTriggerValue));
         
            Binding selectedsourceVariableBinding = new Binding(sourcevariable);
            selectedsourceVariableBinding.Mode = BindingMode.OneWay;
            selectedsourceVariableBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            triggerValue.SetBinding(UCTriggerValue.SelectedSourceVariabelProperty, selectedsourceVariableBinding);

            Binding triggerValuebinding = new Binding(triggervalue);
            triggerValuebinding.Mode = BindingMode.TwoWay;
            triggerValuebinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            triggerValue.SetBinding(UCTriggerValue.TriggerValueProperty, triggerValuebinding);
        
            template.VisualTree = triggerValue;
            return template;
        }

    }
}
