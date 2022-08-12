using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.VariablesLib;
using GingerCore.GeneralLib;
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
using static Ginger.Variables.InputVariableRule;

namespace Ginger.UserControlsLib.InputVariableRule
{
    /// <summary>
    /// Interaction logic for UCOperationValue.xaml
    /// </summary>
    public partial class UCOperationValue : UserControl, INotifyPropertyChanged
    {
        public static DependencyProperty SelectedTargetVariabelProperty_OV = DependencyProperty.Register("SelectedTargetVariabel_OV", typeof(VariableBase), typeof(UCOperationValue), new PropertyMetadata(OnSelectedTargetVariabelPropertyChanged_OV));

        public static DependencyProperty OperationTypeProperty_OV = DependencyProperty.Register("OperationType_OV", typeof(eInputVariableOperation), typeof(UCOperationValue), new PropertyMetadata(OnOperationTypePropertyChanged_OV));

        public static DependencyProperty OperationValueProperty = DependencyProperty.Register("OperationValue", typeof(string), typeof(UCOperationValue), new PropertyMetadata(OnOperationValuePropertyChanged));

        public static DependencyProperty OperationValuesListProperty = DependencyProperty.Register("OperationValuesList", typeof(ObservableList<string>), typeof(UCOperationValue), new PropertyMetadata(OnOperationValuesListPropertyChanged));

        //public static DependencyProperty OperationSelectedValuesProperty = DependencyProperty.Register("OperationSelectedValues", typeof(ObservableList<SelectableObject<string>>), typeof(UCOperationValue), new PropertyMetadata(OnOperationSelectedValuesPropertyChanged));

        public enum eVisibilityOptions
        {
            Visible,
            Hide
        }
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

        public ObservableList<string> OperationValuesList
        {
            get { return (ObservableList<string>)GetValue(OperationValuesListProperty); }
            set { SetValue(OperationValuesListProperty, value); }
        }

        //public ObservableList<SelectableObject<string>> OperationSelectedValues
        //{
        //    get { return (ObservableList<SelectableObject<string>>)GetValue(OperationSelectedValuesProperty); }
        //    set { SetValue(OperationSelectedValuesProperty, value); }
        //}

        private ObservableList<SelectableObject<string>> mOperationSelectedValues;
        public ObservableList<SelectableObject<string>> OperationSelectedValues
        {
            get 
            {
                return mOperationSelectedValues;
            }
            set 
            {
                mOperationSelectedValues = value;                
                OnPropertyChanged(nameof(OperationSelectedValues));
            }
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
            var control = sender as UCOperationValue;
            if (control != null)
            {
                control.OperationTypePropertyChanged_OV((eInputVariableOperation)args.NewValue);
            }
        }

        private void SelectedTargetVariabelPropertyChanged_OV(VariableBase targetVar)
        {
            OnPropertyChanged(nameof(TargetVariable));
            TargetVariable = targetVar;
            SetOperationValueControls();
        }

        private static void OnSelectedTargetVariabelPropertyChanged_OV(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCOperationValue;
            if (control != null)
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

        //private static void OnOperationSelectedValuesPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        //{
        //    var control = sender as UCOperationValue;
        //    if (control != null)
        //    {
        //        control.OperationSelectedValuesPropertyChanged((ObservableList<SelectableObject<string>>)args.NewValue);
        //    }
        //}

        //private void OperationSelectedValuesPropertyChanged(ObservableList<SelectableObject<string>> oprationSelectedValues)
        //{
        //    OnPropertyChanged(nameof(OperationSelectedValues));
        //    OperationValuesList = new ObservableList<string>(oprationSelectedValues.Select(x => x.TextData).ToList());
        //}

        private void SetOperationValueControls()
        {
            if (OperationType == eInputVariableOperation.SetVisibility)
            {
                xVisibilityOptions.Visibility = Visibility.Visible;
                BindingHandler.ObjFieldBinding(xVisibilityOptions, ComboBox.SelectedValueProperty, this, nameof(OperationValue));
                xPossibleValues.Visibility = Visibility.Collapsed;
                xSetValueTxtBox.Visibility = Visibility.Collapsed;
                multiselectCombobox.Visibility = Visibility.Collapsed;
                xVisibilityOptions.SelectionChanged+=XVisibilityOptions_SelectionChanged;
            }
            if (OperationType == eInputVariableOperation.SetValue)
            {
                xVisibilityOptions.Visibility = Visibility.Collapsed;
                xVisibilityOptions.ClearControlsBindings();
                multiselectCombobox.Visibility = Visibility.Collapsed;
                if (TargetVariable != null && TargetVariable.VariableType == "Selection List")
                {
                    xPossibleValues.Visibility = Visibility.Visible;
                    xPossibleValues.ItemsSource = ((VariableSelectionList)TargetVariable).OptionalValuesList.Select(x => x.Value).ToList();
                    xSetValueTxtBox.Visibility = Visibility.Collapsed;
                }
                else
                {
                    xSetValueTxtBox.Visibility = Visibility.Visible;
                    xPossibleValues.Visibility = Visibility.Collapsed;
                    xSetValueTxtBox.TextChanged +=XSetValueTxtBox_TextChanged;
                }
            }
            if (TargetVariable != null && TargetVariable.VariableType == "Selection List" && OperationType == eInputVariableOperation.SetOptionalValues)
            {
                xVisibilityOptions.Visibility = Visibility.Collapsed;
                xVisibilityOptions.ClearControlsBindings();
                xPossibleValues.Visibility = Visibility.Collapsed;
                OperationSelectedValues = new ObservableList<SelectableObject<string>>();
                List<string> possibleValues = ((VariableSelectionList)TargetVariable).OptionalValuesList.Select(x => x.Value).ToList();
                foreach(string possibleValue in possibleValues)
                {
                    if(OperationValuesList!=null && OperationValuesList.Contains(possibleValue))
                    {
                        OperationSelectedValues.Add(new SelectableObject<string>(possibleValue, true));
                    }   
                    else
                    {
                        OperationSelectedValues.Add(new SelectableObject<string>(possibleValue, false));
                    }
                }               
                multiselectCombobox.Visibility = Visibility.Visible;                   
                xSetValueTxtBox.Visibility = Visibility.Collapsed;
            }
        }

        private void XSetValueTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txtBox = (TextBox)sender;
            if (txtBox.Text !=null)
            {
                string var = txtBox.Text;
                OperationValue = var;
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
            var control = sender as UCOperationValue;
            if (control != null)
            {
                control.OperationValuePropertyChanged((string)args.NewValue);
            }
        }

        private void OperationValuePropertyChanged(string newValue)
        {
            OnPropertyChanged(nameof(OperationValue));
        }

        private static void OnOperationValuesListPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCOperationValue;
            if (control != null)
            {
                control.OperationValuesListPropertyChanged((ObservableList<string>)args.NewValue);
            }
        }

        private void OperationValuesListPropertyChanged(ObservableList<string> newValue)
        {
            OnPropertyChanged(nameof(OperationValuesList));
        }

        public UCOperationValue()
        {
            InitializeComponent();
            InitOperationValue();
        }

        public void InitOperationValue()
        {
            GingerCore.General.FillComboItemsFromEnumType(xVisibilityOptions, typeof(eVisibilityOptions));          
            OperationSelectedValues = new ObservableList<SelectableObject<string>>();
            //BindingHandler.ObjFieldBinding(multiselectCombobox, ComboBox.ItemsSourceProperty, this, nameof(OperationSelectedValues), BindingMode.TwoWay);
            multiselectCombobox.Init(this, nameof(OperationSelectedValues));
            UCMultiSelectCombobox.SetMultiSelectEvent(MultiSelection);
            UCTargetVariable.SetOperationvalueEvent(OperationValueEvent);
        }

        private void MultiSelection(bool selection)
        {
            if(selection)
            {
                OperationValuesList = new ObservableList<string>(mOperationSelectedValues.Where(x => x.IsSelected == true).Select(x => x.TextData).ToList());
            }
        }

        private void OperationValueEvent(VariableBase variable)
        {
            TargetVariable = variable;
        }

        public static DataTemplate GetTemplate(string selectedtargetvariableProperty, string operationtypePropperty, string operationValueProperty, string operationValuesListProperty = "", string operationSelectedValuesProperty = "")
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory operationValue = new FrameworkElementFactory(typeof(UCOperationValue));

            Binding selectedTargetVariableBinding_OV = new Binding(selectedtargetvariableProperty);
            selectedTargetVariableBinding_OV.Mode = BindingMode.OneWay;
            selectedTargetVariableBinding_OV.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            operationValue.SetBinding(UCOperationValue.SelectedTargetVariabelProperty_OV, selectedTargetVariableBinding_OV);

            Binding operationTypebinding_OV = new Binding(operationtypePropperty);
            operationTypebinding_OV.Mode = BindingMode.TwoWay;
            operationTypebinding_OV.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            operationValue.SetBinding(UCOperationValue.OperationTypeProperty_OV, operationTypebinding_OV);

            Binding operationValueBinding = new Binding(operationValueProperty);
            operationValueBinding.Mode = BindingMode.TwoWay;
            operationValueBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            operationValue.SetBinding(UCOperationValue.OperationValueProperty, operationValueBinding);

            if(!string.IsNullOrEmpty(operationValuesListProperty))
            {
                Binding operationValuesListBinding = new Binding(operationValuesListProperty);
                operationValuesListBinding.Mode = BindingMode.TwoWay;
                operationValuesListBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                operationValue.SetBinding(UCOperationValue.OperationValuesListProperty, operationValuesListBinding);
            }

            //if (!string.IsNullOrEmpty(operationSelectedValuesProperty))
            //{
            //    Binding operationSelectedValuesBinding = new Binding(operationSelectedValuesProperty);
            //    operationSelectedValuesBinding.Mode = BindingMode.TwoWay;
            //    operationSelectedValuesBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            //    operationValue.SetBinding(UCOperationValue.OperationSelectedValuesProperty, operationSelectedValuesBinding);
            //}

            template.VisualTree = operationValue;
            return template;
        }        
    }
}
