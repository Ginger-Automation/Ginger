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
using System;
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
        
        //public static DependencyProperty VariabelTypeCustomeLabelProperty =
        //DependencyProperty.Register("VariabelTypeCustomeLabel", typeof(string), typeof(UCStoreTo), new PropertyMetadata(OnVariabelTypeCustomeLabelPropertyChanged));

        public static DependencyProperty ModelsGlobalParametersSourceProperty =
        DependencyProperty.Register("ModelsGlobalParametersSource", typeof(ObservableList<ComboItem>), typeof(UCDataMapping), new PropertyMetadata(OnModelsGlobalParametersSourcePropertyChanged));

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
        }

        #region Global
        private void InitTypeOptions()
        {
            GingerCore.General.FillComboItemsFromEnumType(xMappedTypeComboBox, typeof(ActReturnValue.eStoreTo));            
            //BindingHandler.ObjFieldBinding(xMappedTypeComboBox, ComboBox.SelectedValueProperty, this, nameof(MappedType));

            if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>().Count == 0)
            {
                GingerCore.General.DisableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.DataSource);
                xDSConfigBtn.IsEnabled = false;
            }
        }

        private static void OnMappedTypePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = (UCDataMapping)sender;
            if (control != null && control.DataContext != null && control.DataContext.GetType() == typeof(ActReturnValue))
            {
                control.MappedTypePropertyChanged((string)args.NewValue, "Set Value");
            }
            else
            {
                control.MappedTypePropertyChanged((string)args.NewValue, "Get Value");
            }
        }
        private void MappedTypePropertyChanged(string mappedTypeProperty, string controlAction)
        {
            Guid refGuid = new Guid();
            if (mappedTypeProperty == ActReturnValue.eStoreTo.Variable.ToString())
            {
                GingerCore.General.SelectComboValue(xMappedTypeComboBox, ActReturnValue.eStoreTo.Variable.ToString());
                if (String.IsNullOrEmpty(MappedValue) || MappedValue == xDSExpressionTxtbox.Text || Guid.TryParse(MappedValue, out refGuid) == true)
                {
                    xVariablesComboBox.SelectedIndex = 0;
                    MappedValue = xVariablesComboBox.SelectedItem.ToString();
                }
                else
                {
                    xVariablesComboBox.Text = MappedValue;
                }
            }
            else if (mappedTypeProperty == ActReturnValue.eStoreTo.ApplicationModelParameter.ToString())
            {
                GingerCore.General.SelectComboValue(xMappedTypeComboBox, ActReturnValue.eStoreTo.ApplicationModelParameter.ToString());
                if (Guid.TryParse(MappedValue, out refGuid) == false)
                {
                    xModelsParamsComboBox.SelectedIndex = 0;
                    MappedValue = ((ComboItem)xModelsParamsComboBox.SelectedItem).Value.ToString();
                }
                else
                {
                    xModelsParamsComboBox.SelectedValue = MappedValue;
                }
            }
            else if (mappedTypeProperty == ActReturnValue.eStoreTo.DataSource.ToString())
            {
                GingerCore.General.SelectComboValue(xMappedTypeComboBox, ActReturnValue.eStoreTo.DataSource.ToString());
                if (String.IsNullOrEmpty(MappedValue) || MappedValue == xVariablesComboBox.Text || Guid.TryParse(MappedValue, out refGuid) == true)
                {
                    ActDataSourcePage ADSP = new ActDataSourcePage(xDSExpressionTxtbox, this.MappedValue, controlAction);
                    ADSP.ShowAsWindow();
                    MappedValue = xDSExpressionTxtbox.Text;
                }
                else
                {
                    xDSExpressionTxtbox.Text = MappedValue;
                }
            }
            else if (mappedTypeProperty == ActReturnValue.eStoreTo.None.ToString())
            {
                GingerCore.General.SelectComboValue(xMappedTypeComboBox, ActReturnValue.eStoreTo.None.ToString());
                MappedValue = "";
                xDSExpressionTxtbox.Text = "";
                xVariablesComboBox.Text = "";
            }
            MarkMappedValueValidation();
        }
        private void xMappedTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xMappedTypeComboBox.SelectedValue == null)
            {
                return;
            }
            else
            {
                MappedType = xMappedTypeComboBox.SelectedValue.ToString();

                if (xMappedTypeComboBox.SelectedValue.ToString() == ActReturnValue.eStoreTo.None.ToString())
                {
                    xMappedValueColumn.Width = new GridLength(0);
                }
                else
                {
                    xMappedValueColumn.Width = new GridLength(105, GridUnitType.Star);
                }

                if (xMappedTypeComboBox.SelectedValue.ToString() == ActReturnValue.eStoreTo.Variable.ToString()
                    && xVariablesComboBox != null)
                {
                    xVariablesComboBox.Visibility = Visibility.Visible;
                }
                else
                {
                    xVariablesComboBox.Visibility = Visibility.Hidden;
                }

                if (xMappedTypeComboBox.SelectedValue.ToString() == ActReturnValue.eStoreTo.ApplicationModelParameter.ToString()
                    && xModelsParamsComboBox != null)
                {
                    xModelsParamsComboBox.Visibility = Visibility.Visible;
                }
                else
                {
                    xModelsParamsComboBox.Visibility = Visibility.Hidden;
                }

                if (xMappedTypeComboBox.SelectedValue.ToString() == ActReturnValue.eStoreTo.DataSource.ToString()
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
            if (String.IsNullOrEmpty(mappedValueProperty))
            {
                return;
            }
            if (xMappedTypeComboBox.SelectedValue == null)
            {
                return;
            }
            if (xMappedTypeComboBox.SelectedValue.ToString() == ActReturnValue.eStoreTo.Variable.ToString() && !mappedValueProperty.StartsWith("{DS Name"))
            {
                GingerCore.General.SelectComboValue(xVariablesComboBox, mappedValueProperty);
            }
            else if (xMappedTypeComboBox.SelectedValue.ToString() == ActReturnValue.eStoreTo.DataSource.ToString() && mappedValueProperty.StartsWith("{DS Name"))
            {
                xDSExpressionTxtbox.Text = mappedValueProperty;
            }
            MarkMappedValueValidation();
        }
        private void MarkMappedValueValidation()
        {
            bool isValid = true;
            if ((MappedType == ActReturnValue.eStoreTo.Variable.ToString() && !GingerCore.General.CheckComboItemExist(xVariablesComboBox, MappedValue))
                || (MappedType == ActReturnValue.eStoreTo.DataSource.ToString() && GingerCoreNET.GeneralLib.General.CheckDataSource(MappedValue, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>()) != "")
                || (MappedType == ActReturnValue.eStoreTo.ApplicationModelParameter.ToString() && !GingerCore.General.CheckComboItemExist(xModelsParamsComboBox, MappedValue, "Value")))
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
            if (enabelMapping == true)
            {
                if (xVariablesComboBox != null && xVariablesComboBox.Items.Count != 0)
                {
                    GingerCore.General.EnableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.Variable);
                }

                if (xModelsParamsComboBox != null && xModelsParamsComboBox.Items.Count != 0)
                {
                    GingerCore.General.EnableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.ApplicationModelParameter);
                }

                if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>().Count == 0)
                {
                    GingerCore.General.DisableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.DataSource);
                    xDSConfigBtn.IsEnabled = false;
                }
                else
                {
                    GingerCore.General.EnableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.DataSource);
                    xDSConfigBtn.IsEnabled = true;
                }
            }
        }      
        
        public static DataTemplate GetTemplate(string dataTypeProperty, string dataValueProperty, ObservableList<string> variabelsSourceList, string variabelsSourceProperty = "", string enableDataMappingProperty = "", ObservableList<GlobalAppModelParameter> modelsGlobalParamsList = null)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory Storeto = new FrameworkElementFactory(typeof(UCDataMapping));

            if (variabelsSourceList != null)
            {
                Storeto.SetValue(UCDataMapping.VariabelsSourceProperty, variabelsSourceList);
            }
            else
            {
                Binding comboItemsSourceBinding = new Binding(variabelsSourceProperty);
                comboItemsSourceBinding.Mode = BindingMode.TwoWay;
                comboItemsSourceBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                Storeto.SetBinding(UCDataMapping.VariabelsSourceProperty, comboItemsSourceBinding);
            }

            if (modelsGlobalParamsList != null)
            {
                ObservableList<ComboItem> appModelGlobalParamsComboItemsList = new ObservableList<ComboItem>();
                foreach (GlobalAppModelParameter param in modelsGlobalParamsList)
                {
                    appModelGlobalParamsComboItemsList.Add(new ComboItem() { text = param.PlaceHolder, Value = param.Guid });
                }

                Storeto.SetValue(UCDataMapping.ModelsGlobalParametersSourceProperty, appModelGlobalParamsComboItemsList);
            }

            //if (variabelNameTypeCustomeLabel != "")
            //{
            //    Storeto.SetValue(UCStoreTo.VariabelTypeCustomeLabelProperty, variabelNameTypeCustomeLabel);
            //}

            Binding selectedStoreToBinding = new Binding(dataTypeProperty);
            selectedStoreToBinding.Mode = BindingMode.TwoWay;
            selectedStoreToBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Storeto.SetBinding(UCDataMapping.MappedTypeProperty, selectedStoreToBinding);

            Binding selectedValueBinding = new Binding(dataValueProperty);
            selectedValueBinding.Mode = BindingMode.TwoWay;
            selectedValueBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Storeto.SetBinding(UCDataMapping.MappedValueProperty, selectedValueBinding);

            if (enableDataMappingProperty != "")
            {
                Binding allowStoreBinding = new Binding(enableDataMappingProperty);
                allowStoreBinding.Mode = BindingMode.OneWay;
                allowStoreBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                Storeto.SetBinding(UCDataMapping.EnableDataMappingProperty, allowStoreBinding);
            }

            template.VisualTree = Storeto;
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
            if (variabelsSourceList.Count == 0 || (variabelsSourceList.Count == 1 && variabelsSourceList[0] == string.Empty))
            {
                GingerCore.General.DisableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.Variable);
            }
            variabelsSourceList.CollectionChanged += VariabelsSourceList_CollectionChanged;
            xVariablesComboBox.ItemsSource = variabelsSourceList;
        }
        private void VariabelsSourceList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnMappedValuePropertyChanged(this, new DependencyPropertyChangedEventArgs(ComboBox.TextProperty, MappedValue, MappedValue));
        }
        private void xVariablesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xVariablesComboBox.SelectedItem != null)
            {
                MappedValue = xVariablesComboBox.SelectedItem.ToString();
            }
        }
        #endregion Variables

        #region ModelGlobalParameters
        private static void OnModelsGlobalParametersSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCDataMapping;
            if (control != null)
            {
                control.ModelsGlobalParametersSourcePropertyChanged((ObservableList<ComboItem>)args.NewValue);
            }
        }
        private void ModelsGlobalParametersSourcePropertyChanged(ObservableList<ComboItem> modelGlobalParamsList)
        {
            if (modelGlobalParamsList.Count == 0)
            {
                GingerCore.General.DisableComboItem(xMappedTypeComboBox, ActReturnValue.eStoreTo.ApplicationModelParameter);
            }

            xModelsParamsComboBox.DisplayMemberPath = nameof(ComboItem.text);
            xModelsParamsComboBox.SelectedValuePath = nameof(ComboItem.Value);
            xModelsParamsComboBox.ItemsSource = modelGlobalParamsList.OrderBy(nameof(ComboItem.text));
        }

        private void xModelsParamsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xModelsParamsComboBox.SelectedItem != null)
            {
                MappedValue = ((ComboItem)xModelsParamsComboBox.SelectedItem).Value.ToString();
            }
        }
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

        private void xDSExpressionTxtbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MappedValue = xDSExpressionTxtbox.Text;
        }
        #endregion DataSource

       
        //########################################
        //public List<string> ItemsSource
        //{
        //    get { return (List <string>)GetValue(VariabelsSourceProperty); }
        //    set { SetValue(VariabelsSourceProperty, value);}
        //}

        //private static void OnVariabelTypeCustomeLabelPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        //{
        //    var control = sender as UCStoreTo;
        //    if (control != null)
        //        control.OnVarLabelChanged((string)args.NewValue);
        //}
        //private void OnVarLabelChanged(string label)
        //{            
        //    mLabel = label;
        //    GingerCore.General.UpdateComboItem(cmbStoreTo, ActReturnValue.eStoreTo.Variable, label);                        
        //    GingerCore.General.DisableComboItem(cmbStoreTo, ActReturnValue.eStoreTo.Variable);
        //    GingerCore.General.DisableComboItem(cmbStoreTo,ActReturnValue.eStoreTo.DataSource);
        //    GingerCore.General.DisableComboItem(cmbStoreTo, ActReturnValue.eStoreTo.ApplicationModelParameter);            
        //}
    }
}
