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
        public static DependencyProperty SelectedTargetVariabelProperty_OV = 
        DependencyProperty.Register("SelectedTargetVariabel_OV", typeof(VariableBase), typeof(UCOperationValue), new PropertyMetadata(OnSelectedTargetVariabelPropertyChanged_OV));

        public static DependencyProperty OperationTypeProperty_OV = 
        DependencyProperty.Register("OperationType_OV", typeof(eInputVariableOperation), typeof(UCOperationValue), new PropertyMetadata(OnOperationTypePropertyChanged_OV));

        public static DependencyProperty OperationValueProperty = 
        DependencyProperty.Register("OperationValue", typeof(string), typeof(UCOperationValue), new PropertyMetadata(OnOperationValuePropertyChanged));

        public static DependencyProperty OperationValuesListProperty = 
        DependencyProperty.Register("OperationValuesList", typeof(ObservableList<OperationValues>), typeof(UCOperationValue), new PropertyMetadata(OnOperationValuesListPropertyChanged));

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

        public ObservableList<OperationValues> OperationValuesList
        {
            get { return (ObservableList<OperationValues>)GetValue(OperationValuesListProperty); }
            set { SetValue(OperationValuesListProperty, value); }
        }

        //public ObservableList<SelectableObject<string>> OperationSelectedValues
        //{
        //    get { return (ObservableList<SelectableObject<string>>)GetValue(OperationSelectedValuesProperty); }
        //    set { SetValue(OperationSelectedValuesProperty, value); }
        //}

        private ObservableList<SelectableObject<string>> mOperationSelectedValues = new ObservableList<SelectableObject<string>>();
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
            TargetVariable = targetVar;
            OnPropertyChanged(nameof(TargetVariable));            
            /////
            if (targetVar != null && targetVar.VariableType == "Selection List")
            {
                GingerCore.General.EnableComboItem(xOperationTypeComboBox, eInputVariableOperation.SetOptionalValues);
                OperationType = eInputVariableOperation.SetOptionalValues;
            }
            else
            {
                GingerCore.General.DisableComboItem(xOperationTypeComboBox, eInputVariableOperation.SetOptionalValues);
                OperationType= eInputVariableOperation.SetValue;
            }
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
            ///
            //GingerCore.General.SelectComboValue(xOperationTypeComboBox, operationType.ToString());
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
                xVisibilityOptions.SelectionChanged+=XVisibilityOptions_SelectionChanged;
                xPossibleValues.Visibility = Visibility.Collapsed;
                xSetValueTxtBox.Visibility = Visibility.Collapsed;
                multiselectCombobox.Visibility = Visibility.Collapsed;
                xSetValueTxtBox.ClearControlsBindings();
                xPossibleValues.ClearControlsBindings();
            }
            if (OperationType == eInputVariableOperation.SetValue)
            {
                xVisibilityOptions.Visibility = Visibility.Collapsed;
                xVisibilityOptions.ClearControlsBindings();
                multiselectCombobox.Visibility = Visibility.Collapsed;                
                if (TargetVariable != null && TargetVariable.VariableType == "Selection List")
                {
                    xSetValueTxtBox.ClearControlsBindings();
                    xPossibleValues.Visibility = Visibility.Visible;
                    BindingHandler.ObjFieldBinding(xPossibleValues, ComboBox.SelectedValueProperty, this, nameof(OperationValue));
                    xPossibleValues.SelectionChanged += XPossibleValues_SelectionChanged;
                    xPossibleValues.ItemsSource = ((VariableSelectionList)TargetVariable).OptionalValuesList.Select(x => x.Value).ToList();
                    xSetValueTxtBox.Visibility = Visibility.Collapsed;
                }
                else
                {
                    BindingHandler.ObjFieldBinding(xSetValueTxtBox, TextBox.TextProperty, this, nameof(OperationValue));
                    xSetValueTxtBox.TextChanged +=XSetValueTxtBox_TextChanged;
                    xSetValueTxtBox.Visibility = Visibility.Visible;
                    xPossibleValues.Visibility = Visibility.Collapsed;
                    xPossibleValues.ClearControlsBindings();
                }
            }
            if (TargetVariable != null && TargetVariable.VariableType == "Selection List" && OperationType == eInputVariableOperation.SetOptionalValues)
            {                
                xVisibilityOptions.Visibility = Visibility.Collapsed;
                xVisibilityOptions.ClearControlsBindings();
                xSetValueTxtBox.ClearControlsBindings();
                xPossibleValues.ClearControlsBindings();
                xPossibleValues.Visibility = Visibility.Collapsed;
                //OperationSelectedValues = new ObservableList<SelectableObject<string>>();
                //List<string> possibleValues = ((VariableSelectionList)TargetVariable).OptionalValuesList.Select(x => x.Value).ToList();
                //foreach(string possibleValue in possibleValues)
                //{
                //    if(OperationValuesList!=null && (OperationValuesList.Where(x=> x.Value == possibleValue).Count() == 1))
                //    {
                //        OperationSelectedValues.Add(new SelectableObject<string>(possibleValue, true));
                //    }   
                //    else
                //    {
                //        OperationSelectedValues.Add(new SelectableObject<string>(possibleValue, false));
                //    }
                //}               
                multiselectCombobox.Visibility = Visibility.Visible;
                multiselectCombobox.SetSelectedString(false);
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

        private void XPossibleValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.Items.Count > 0)
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
            var control = sender as UCOperationValue;
            if (control != null)
            {
                control.OperationValuePropertyChanged((string)args.NewValue);
            }
        }

        private void OperationValuePropertyChanged(string newValue)
        {
            OnPropertyChanged(nameof(OperationValue));
           // SetOperationValueControls();
        }

        private static void OnOperationValuesListPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCOperationValue;
            if (control != null)
            {
                control.OperationValuesListPropertyChanged((ObservableList<OperationValues>)args.NewValue);
            }
        }

        private void OperationValuesListPropertyChanged(ObservableList<OperationValues> newValue)
        {
            OnPropertyChanged(nameof(OperationValuesList));
            //SetOperationValueControls();
        }

        public UCOperationValue()
        {
            InitializeComponent();
            InitOperationConfiguration();           
        }

        public void InitOperationConfiguration()
        {           
            GingerCore.General.FillComboItemsFromEnumType(xVisibilityOptions, typeof(eVisibilityOptions));            
            xVisibilityOptions.SelectionChanged+=XVisibilityOptions_SelectionChanged;

            //BindingHandler.ObjFieldBinding(multiselectCombobox, ComboBox.ItemsSourceProperty, this, nameof(OperationSelectedValues), BindingMode.TwoWay);
            OperationSelectedValues = new ObservableList<SelectableObject<string>>();
            multiselectCombobox.Init(this, nameof(OperationSelectedValues));
            UCMultiSelectCombobox.SetMultiSelectEvent(MultiSelection);          
            UCTargetVariable.SetOperationvalueEvent(OperationValueEvent);

            BindingHandler.ObjFieldBinding(xOperationTypeComboBox, ComboBox.SelectedValueProperty, this, nameof(OperationType));            
            GingerCore.General.FillComboFromEnumType(xOperationTypeComboBox, typeof(eInputVariableOperation));
            xOperationTypeComboBox.SelectionChanged+=XOperationTypeComboBox_SelectionChanged;
            //BindingHandler.ObjFieldBinding(xOperationTypeComboBox, ComboBox.SelectedValueProperty, this, nameof(OperationType));
            //GingerCore.General.DisableComboItem(xOperationTypeComboBox, eInputVariableOperation.SetOptionalValues);            
            
        }
       
        private void MultiSelection(bool selection, UCOperationValue obj)
        {
            if(selection)
            {
                //OperationSelectedValues = obj.OperationSelectedValues;
                //OperationValuesList = new ObservableList<OperationValue>(new OperationValue { Value = OperationSelectedValues.Where(x => x.IsSelected == true).Select(x => x.TextData) });
                ObservableList<OperationValues> lst = new ObservableList<OperationValues>();

                foreach(SelectableObject<string> opValue in obj.OperationSelectedValues)
                {
                    if(opValue.IsSelected)
                    {
                        lst.Add(new OperationValues() { Value = opValue.TextData });
                    }
                }

                OperationValuesList = new ObservableList<OperationValues>(lst.ToList());                
            }
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

            Binding selectedTargetVariableBinding_OV = new Binding(selectedtargetvariableProperty);
            selectedTargetVariableBinding_OV.Mode = BindingMode.TwoWay;
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
