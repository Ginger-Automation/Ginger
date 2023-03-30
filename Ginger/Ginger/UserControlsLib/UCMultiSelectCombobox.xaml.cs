using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.VariablesLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Amdocs.Ginger.Common.VariablesLib;
using Ginger.UserControlsLib.InputVariableRule;
using GingerCore.Variables;
using GingerCore.GeneralLib;
using Amdocs.Ginger.Repository;

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for UCMultiSelectCombobox.xaml
    /// </summary>
    public partial class UCMultiSelectCombobox : UserControl, INotifyPropertyChanged
    {

        public static DependencyProperty MultiSelectValuesListProperty =
         DependencyProperty.Register("MultiSelectValuesList", typeof(ObservableList<SelectableObject<string>>), typeof(UCMultiSelectCombobox), 
             new PropertyMetadata(OnMultiSelectValuesListPropertyChanged));

        public static DependencyProperty VariableValuesListProperty =
        DependencyProperty.Register("VariableValuesList", typeof(ObservableList<OptionalValue>), typeof(UCMultiSelectCombobox), new PropertyMetadata(OnVariableValuesListPropertyChanged));


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private static void OnVariableValuesListPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCMultiSelectCombobox;
            if (control != null)
            {
                control.VariableValuesListPropertyChanged((ObservableList<OptionalValue>)args.NewValue);
            }
        }

        private void VariableValuesListPropertyChanged(ObservableList<OptionalValue> list)
        {
            SetControlsData();
        }

        private static void OnMultiSelectValuesListPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {

            var control = sender as UCMultiSelectCombobox;
            if (control != null)
            {
                control.MultiSelectValuesListPropertyChanged((ObservableList<SelectableObject<string>>)args.NewValue);
            }
        }

        private void MultiSelectValuesListPropertyChanged(ObservableList<SelectableObject<string>> list)
        {
            OnPropertyChanged(nameof(MultiSelectValuesList));
        }

        public ObservableList<SelectableObject<string>> MultiSelectValuesList
        {
            get { return (ObservableList<SelectableObject<string>>)GetValue(MultiSelectValuesListProperty); }
            set { SetValue(MultiSelectValuesListProperty, value); }
        }

        public ObservableList<OptionalValue> VariableValuesList
        {
            get { return (ObservableList<OptionalValue>)GetValue(VariableValuesListProperty); }
            set { SetValue(VariableValuesListProperty, value); }
        }

        //public delegate void MultiSelectEventHandler(bool EventArgs, UCOperationValue obj);
        //private static event MultiSelectEventHandler MultSelectEvent;
        //public void OnMultiSelectEvent(bool SelectionChanged, UCOperationValue obj)
        //{
        //    MultiSelectEventHandler handler = MultSelectEvent;
        //    if (handler != null)
        //    {
        //        handler(SelectionChanged, obj);
        //    }
        //}

        //public static void SetMultiSelectEvent(MultiSelectEventHandler multiSelectEvent)
        //{
        //    if (MultSelectEvent == null)
        //    {
        //        MultSelectEvent -= multiSelectEvent;
        //        MultSelectEvent += multiSelectEvent;
        //    }
        //}

        private object obj;
        private string AttrName;
        private Context mContext;

        public UCMultiSelectCombobox()
        {
            InitializeComponent();           
        }

        public void SetControlsData()
        {
            MultiSelectValuesList = new ObservableList<SelectableObject<string>>();
            List<string> possibleValues = VariableValuesList.Select(x => x.Value).ToList();
            foreach (string possibleValue in possibleValues)
            {                          
                if (VariableValuesList!=null && (VariableValuesList.Where(x => x.Value == possibleValue).Count() == 1))
                {
                    MultiSelectValuesList.Add(new SelectableObject<string>(possibleValue, true));
                }
                else
                {
                    MultiSelectValuesList.Add(new SelectableObject<string>(possibleValue, false));
                }
            }

        }

        public void SetControlsBinding(string variableValuesListProperty, string multiSelectValuesListProperty, UCOperationValue Source)
        {
            Binding variableValuesListBinding = new Binding(variableValuesListProperty);
            variableValuesListBinding.Mode = BindingMode.TwoWay;
            variableValuesListBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            //variableValuesListBinding.Source = Source;
            Source.SetBinding(UCMultiSelectCombobox.VariableValuesListProperty, variableValuesListBinding);

            Binding multiSelectValuesListBinding = new Binding(multiSelectValuesListProperty);
            multiSelectValuesListBinding.Mode = BindingMode.TwoWay;
            multiSelectValuesListBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            //multiSelectValuesListBinding.Source = Source;
            Source.SetBinding(UCMultiSelectCombobox.MultiSelectValuesListProperty, multiSelectValuesListBinding);

            //GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(this, UCMultiSelectCombobox.VariableValuesListProperty, Source, variableValuesListProperty);
            //GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(this, UCMultiSelectCombobox.MultiSelectValuesListProperty, Source, multiSelectValuesListProperty);

            //GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xMultiSelectCombobox, ComboBox.ItemsSourceProperty, MultiSelectValuesList, nameof(MultiSelectValuesListProperty));
        }

        //public void Init(object obj, string AttrName)
        //{
        //    //// If the VE is on stand alone form:
        //    this.obj = obj;
        //    this.AttrName = AttrName;          

        //    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xMultiSelectCombobox, ComboBox.ItemsSourceProperty, obj, AttrName);           
        //}

        //private void SetComboboxValues()
        //{
        //    UCOperationValue uCOperationValue = (UCOperationValue)this.obj;
        //    List<string> possibleValues = ((VariableSelectionList)uCOperationValue.TargetVariable).OptionalValuesList.Select(x => x.Value).ToList();
        //    foreach (string possibleValue in possibleValues)
        //    {
        //        if (uCOperationValue.OperationValuesList!=null && (uCOperationValue.OperationValuesList.Where(x => x.Value == possibleValue).Count() == 1))
        //        {
        //            SetCheckbox(possibleValue, true);
        //        }
        //        else
        //        {
        //            SetCheckbox(possibleValue, false);
        //        }
        //    }
        //}

        private void SetCheckbox(string value, bool IsChecked)
        {
            foreach (SelectableObject<string> cbObject in xMultiSelectCombobox.Items)
            {
                if (cbObject.TextData == value)
                {
                    cbObject.IsSelected = IsChecked;
                }                    
            }
        }

        public void SetSelectedString(bool triggerEvent = true)
        {
            StringBuilder sb = new StringBuilder();
            foreach (SelectableObject<string> cbObject in xMultiSelectCombobox.Items)
            {
                if (cbObject.IsSelected)
                    sb.AppendFormat("{0}, ", cbObject.TextData);
            }
            tbObjects.Text = sb.ToString().Trim().TrimEnd(',');

            if(triggerEvent)
            {
                //OnMultiSelectEvent(true, (UCOperationValue)this.obj);
            }            
        }

        private void OnCbObjectsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            comboBox.SelectedItem = null;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SetSelectedString();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SetSelectedString();
        }
    }
   
}
