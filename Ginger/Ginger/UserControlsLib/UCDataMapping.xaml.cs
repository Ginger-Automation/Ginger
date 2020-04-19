#region License
/*
Copyright © 2014-2020 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using GingerCore.DataSource;
using GingerCore.GeneralLib;
using GingerCore.Variables;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Ginger.UserControlsLib
{
    public partial class UCDataMapping : UserControl, INotifyPropertyChanged
    {
        public static DependencyProperty MappedTypeProperty =
        DependencyProperty.Register("MappedType", typeof(string), typeof(UCDataMapping), new PropertyMetadata(OnMappedTypePropertyChanged));

        public static DependencyProperty MappedValueProperty =
        DependencyProperty.Register("MappedValue", typeof(string), typeof(UCDataMapping), new PropertyMetadata(OnMappedValuePropertyChanged));

        public static DependencyProperty EnableDataMappingProperty =
        DependencyProperty.Register("EnableDataMapping", typeof(bool), typeof(UCDataMapping), new PropertyMetadata(OnEnableDataMappingPropertyChanged));

        public static DependencyProperty VariabelsSourceProperty =
        DependencyProperty.Register("VariabelsSource", typeof(ObservableList<string>), typeof(UCDataMapping), new PropertyMetadata(OnVariabelsSourcePropertyChanged));

        public static DependencyProperty OutputVariabelsSourceProperty =
        DependencyProperty.Register("OutputVariabelsSource", typeof(ObservableList<VariableBase>), typeof(UCDataMapping), new PropertyMetadata(OnOutputVariabelsSourcePropertyChanged));

        public string MappedValue
        {
            get { return (string)GetValue(MappedValueProperty); }
            set { SetValue(MappedValueProperty, value); }
        }

        public string MappedType
        {
            get { return (string)GetValue(MappedTypeProperty); }
            set { SetValue(MappedTypeProperty, value); }
        }

        ObservableList<ComboItem> OutputVariablesComboList = null;

        ObservableList<ComboItem> _globalVariablesComboList = null;
        ObservableList<ComboItem> GlobalVariablesComboList
        {
            get
            {
                if (_globalVariablesComboList == null)
                {
                    _globalVariablesComboList = new ObservableList<ComboItem>();
                    foreach (VariableBase var in WorkSpace.Instance.Solution.Variables)
                    {
                        _globalVariablesComboList.Add(new ComboItem() { text = var.Name, Value = var.Guid.ToString() });
                    }
                }
                return _globalVariablesComboList;
            }
        }

        ObservableList<ComboItem> _modelGlobalParamsComboList = null;
        ObservableList<ComboItem> ModelGlobalParamsComboList
        {
            get
            {
                if (_modelGlobalParamsComboList == null)
                {
                    _modelGlobalParamsComboList = new ObservableList<ComboItem>();
                    foreach (GlobalAppModelParameter param in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>())
                    {
                        _modelGlobalParamsComboList.Add(new ComboItem() { text = param.PlaceHolder, Value = param.Guid.ToString() });
                    }                    
                }
                return _modelGlobalParamsComboList;
            }
        }

        bool EnableDataMapping = true;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public UCDataMapping()
        {           
            InitializeComponent();

            InitTypeOptions();
            InitValueOptions();
        }

        #region Global
        private void InitTypeOptions()
        {
            GingerCore.General.FillComboFromEnumType(xMappedTypeComboBox, typeof(ActReturnValue.eStoreTo));            
            BindingHandler.ObjFieldBinding(xMappedTypeComboBox, ComboBox.SelectedValueProperty, this, nameof(MappedType));

            GingerCore.General.DisableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.Variable);
            GingerCore.General.DisableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.OutputVariable);

            if (GlobalVariablesComboList.Count == 0)
            {
                GingerCore.General.DisableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.GlobalVariable);
            }

            if (ModelGlobalParamsComboList.Count == 0)
            {
                GingerCore.General.DisableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.ApplicationModelParameter);               
            }

            if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>().Count == 0)
            {
                GingerCore.General.DisableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.DataSource);
                xDSConfigBtn.IsEnabled = false;
            }
        }

        private void InitValueOptions()
        {
            xItemsComboBox.DisplayMemberPath = nameof(ComboItem.text);
            xItemsComboBox.SelectedValuePath = nameof(ComboItem.Value);
        }

        private static void OnMappedTypePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {            
            var control = (UCDataMapping)sender;
            control.MappedTypePropertyChanged();
        }
        private void MappedTypePropertyChanged()
        {
            OnPropertyChanged(nameof(MappedType));

            //set relevant value control binding
            BindingOperations.ClearAllBindings(xVariablesComboBox);
            BindingOperations.ClearAllBindings(xItemsComboBox);
            BindingOperations.ClearAllBindings(xDSExpressionTxtbox);
            
            if (MappedType == ActReturnValue.eStoreTo.Variable.ToString())
            {
                BindingHandler.ObjFieldBinding(xVariablesComboBox, ComboBox.SelectedValueProperty, this, nameof(MappedValue));
            }
            else if (MappedType == ActReturnValue.eStoreTo.GlobalVariable.ToString())
            {
                BindingHandler.ObjFieldBinding(xItemsComboBox, ComboBox.SelectedValueProperty, this, nameof(MappedValue));
                xItemsComboBox.ItemsSource = GlobalVariablesComboList.OrderBy(nameof(ComboItem.text));
            }
            else if (MappedType == ActReturnValue.eStoreTo.OutputVariable.ToString())
            {
                BindingHandler.ObjFieldBinding(xItemsComboBox, ComboBox.SelectedValueProperty, this, nameof(MappedValue));
                xItemsComboBox.ItemsSource = OutputVariablesComboList.OrderBy(nameof(ComboItem.text));
            }
            else if (MappedType == ActReturnValue.eStoreTo.ApplicationModelParameter.ToString())
            {
                BindingHandler.ObjFieldBinding(xItemsComboBox, ComboBox.SelectedValueProperty, this, nameof(MappedValue));
                xItemsComboBox.ItemsSource = ModelGlobalParamsComboList.OrderBy(nameof(ComboItem.text));
            }
            else if (MappedType == ActReturnValue.eStoreTo.DataSource.ToString())
            {
                BindingHandler.ObjFieldBinding(xDSExpressionTxtbox, TextBox.TextProperty, this, nameof(MappedValue));
            }
            else if (MappedType == ActReturnValue.eStoreTo.None.ToString())
            {
                MappedValue = "";
            }

            //set valid value control view
            SetValueControlsView();

            MarkMappedValueValidation();
        }

        private void SetValueControlsView()
        {
             if (MappedType == null)
            {
                return;
            }
            else
            {
                if (MappedType == ActReturnValue.eStoreTo.None.ToString())
                {
                    xMappedValueColumn.Width = new GridLength(0);
                }
                else
                {
                    xMappedValueColumn.Width = new GridLength(100, GridUnitType.Star);
                }

                if (MappedType == ActReturnValue.eStoreTo.Variable.ToString()
                    && xVariablesComboBox != null)
                {
                    xVariablesComboBox.Visibility = Visibility.Visible;
                }
                else
                {
                    xVariablesComboBox.Visibility = Visibility.Hidden;
                }

                if ((MappedType == ActReturnValue.eStoreTo.OutputVariable.ToString() || MappedType == ActReturnValue.eStoreTo.GlobalVariable.ToString() || MappedType == ActReturnValue.eStoreTo.ApplicationModelParameter.ToString())
                    && xItemsComboBox != null)
                {
                    xItemsComboBox.Visibility = Visibility.Visible;
                }
                else
                {
                    xItemsComboBox.Visibility = Visibility.Hidden;
                }

                if (MappedType == ActReturnValue.eStoreTo.DataSource.ToString()
                    && xDSExpressionTxtbox != null)
                {
                    xDSExpressionTxtbox.Visibility = Visibility.Visible;
                    xDSConfigBtn.Visibility = Visibility.Visible;
                }
                else
                {
                    xDSExpressionTxtbox.Visibility = Visibility.Hidden;
                    xDSConfigBtn.Visibility = Visibility.Hidden;
                }
            }
        }

        private static void OnMappedValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCDataMapping;
            if (control != null)
            {
                control.MappedValuePropertyChanged((string)args.NewValue);
            }
        }
        private void MappedValuePropertyChanged(string mappedValueProperty)
        {
            OnPropertyChanged(nameof(MappedValue));
            
            MarkMappedValueValidation();
        }

        private void MarkMappedValueValidation()
        {
            bool isValid = true;

            if (MappedValue == string.Empty
                || (MappedType == ActReturnValue.eStoreTo.Variable.ToString() && !GingerCore.General.CheckComboItemExist(xVariablesComboBox, MappedValue))
                || ((MappedType == ActReturnValue.eStoreTo.OutputVariable.ToString() || MappedType == ActReturnValue.eStoreTo.GlobalVariable.ToString() || MappedType == ActReturnValue.eStoreTo.ApplicationModelParameter.ToString()) && !GingerCore.General.CheckComboItemExist(xItemsComboBox, MappedValue, "Value"))
                || (MappedType == ActReturnValue.eStoreTo.DataSource.ToString() && GingerCoreNET.GeneralLib.General.CheckDataSource(MappedValue, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>()) != string.Empty))               
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

        private static void OnEnableDataMappingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCDataMapping;
            if (control != null)
            {
                control.EnableDataMappingPropertyChanged((bool)args.NewValue);
            }
        }
        private void EnableDataMappingPropertyChanged(bool enabelMapping)
        {
            EnableDataMapping = enabelMapping;
            if (EnableDataMapping == false)
            {
                GingerCore.General.DisableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.Variable);
                GingerCore.General.DisableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.OutputVariable);
                GingerCore.General.DisableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.GlobalVariable);
                GingerCore.General.DisableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.ApplicationModelParameter);
                GingerCore.General.DisableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.DataSource);
                xDSConfigBtn.IsEnabled = false;
            }
        }      
        
        public static DataTemplate GetTemplate(string dataTypeProperty, string dataValueProperty, string enableDataMappingProperty = "", string variabelsSourceProperty = "", ObservableList<string> variabelsSourceList=null, ObservableList<VariableBase> outputVariabelsSourceList = null)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory ucDataMapping = new FrameworkElementFactory(typeof(UCDataMapping));

            if (string.IsNullOrEmpty(variabelsSourceProperty) == false)
            {
                Binding comboItemsSourceBinding = new Binding(variabelsSourceProperty);
                comboItemsSourceBinding.Mode = BindingMode.TwoWay;
                comboItemsSourceBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                ucDataMapping.SetBinding(UCDataMapping.VariabelsSourceProperty, comboItemsSourceBinding);
            }
            else if(variabelsSourceList != null)
            {
                ucDataMapping.SetValue(UCDataMapping.VariabelsSourceProperty, variabelsSourceList);
            }

            if (outputVariabelsSourceList != null)
            {
                ucDataMapping.SetValue(UCDataMapping.OutputVariabelsSourceProperty, outputVariabelsSourceList);
            }

            Binding selectedStoreToBinding = new Binding(dataTypeProperty);
            selectedStoreToBinding.Mode = BindingMode.TwoWay;
            selectedStoreToBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            ucDataMapping.SetBinding(UCDataMapping.MappedTypeProperty, selectedStoreToBinding);

            Binding selectedValueBinding = new Binding(dataValueProperty);
            selectedValueBinding.Mode = BindingMode.TwoWay;
            selectedValueBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            ucDataMapping.SetBinding(UCDataMapping.MappedValueProperty, selectedValueBinding);

            if (enableDataMappingProperty != "")
            {
                Binding allowStoreBinding = new Binding(enableDataMappingProperty);
                allowStoreBinding.Mode = BindingMode.OneWay;
                allowStoreBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                ucDataMapping.SetBinding(UCDataMapping.EnableDataMappingProperty, allowStoreBinding);
            }

            template.VisualTree = ucDataMapping;
            return template;
        }
        #endregion Global

        #region Variables
        private static void OnVariabelsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCDataMapping;
            if (control != null)
            {
                control.VariabelsSourcePropertyChanged((ObservableList<string>)args.NewValue);
            }
        }
        private void VariabelsSourcePropertyChanged(ObservableList<string> variabelsSourceList)
        {
            if (variabelsSourceList == null || variabelsSourceList.Count == 0 || (variabelsSourceList.Count == 1 && variabelsSourceList[0] == string.Empty))
            {
                GingerCore.General.DisableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.Variable);
            }
            if (variabelsSourceList != null)
            {
                if (EnableDataMapping)
                {
                    GingerCore.General.EnableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.Variable);
                }
                variabelsSourceList.CollectionChanged += VariabelsSourceList_CollectionChanged;
                xVariablesComboBox.ItemsSource = variabelsSourceList;
            }
        }
        private void VariabelsSourceList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnMappedValuePropertyChanged(this, new DependencyPropertyChangedEventArgs(ComboBox.SelectedValueProperty, MappedValue, MappedValue));
        }
        #endregion Variables

        #region Output Variables
        private static void OnOutputVariabelsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCDataMapping;
            if (control != null)
            {
                control.OutputVariabelsSourcePropertyChanged((ObservableList<VariableBase>)args.NewValue);
            }
        }
        private void OutputVariabelsSourcePropertyChanged(ObservableList<VariableBase> outputVariabelsSourceList)
        {
            if (outputVariabelsSourceList == null || outputVariabelsSourceList.Count == 0)
            {
                GingerCore.General.DisableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.OutputVariable);
            }
           
            if (outputVariabelsSourceList != null)
            {
                if (EnableDataMapping)
                {
                    GingerCore.General.EnableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.OutputVariable);
                }
                outputVariabelsSourceList.CollectionChanged += OutputVariabelsSourceList_CollectionChanged;
                OutputVariablesComboList = new ObservableList<ComboItem>();
                foreach (VariableBase var in outputVariabelsSourceList)
                {
                    OutputVariablesComboList.Add(new ComboItem() { text = var.Name, Value = var.Guid.ToString() });
                }
            }
            
        }
        private void OutputVariabelsSourceList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnMappedValuePropertyChanged(this, new DependencyPropertyChangedEventArgs(ComboBox.SelectedValueProperty, MappedValue, MappedValue));
        }
        #endregion Output Variables

        #region ModelGlobalParameters
        #endregion ModelGlobalParameters

        #region DataSource
        private void xDSConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            ActDataSourcePage ADSP;
            if (((Button)sender).DataContext.GetType() == typeof(ActReturnValue))
            {
                ADSP = new ActDataSourcePage(xDSExpressionTxtbox, ActReturnValue.Fields.StoreToDataSource);
            }
            else
            {
                ADSP = new ActDataSourcePage(xDSExpressionTxtbox, ActReturnValue.Fields.StoreToDataSource, "Get Value");
            }
            ADSP.ShowAsWindow();
        }
        #endregion DataSource
    }
}
