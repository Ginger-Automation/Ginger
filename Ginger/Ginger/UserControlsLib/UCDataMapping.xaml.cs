#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        public static readonly DependencyProperty RestrictedMappingTypesProperty = DependencyProperty.Register("RestrictedMappingTypes",
            typeof(IEnumerable<RestrictedMappingType>), typeof(UCDataMapping));

        public enum eDataType
        {
            None,
            Variable,
            GlobalVariable,
            OutputVariable,
            ApplicationModelParameter,
            DataSource,
            ValueExpression
        }

        public sealed class RestrictedMappingType
        {
            public string Name { get; }
            public string? Reason { get; } = null;

            public RestrictedMappingType(string name, string? reason = null)
            {
                Name = name;
                Reason = reason;
            }
        }

        public sealed class TemplateOptions
        {
            public string _DataTypeProperty { get; set; }
            public string _DataValueProperty { get; set; }
            public string _EnableDataMappingProperty { get; set; } = "";
            public string _VariabelsSourceProperty { get; set; } = "";
            public ObservableList<string>? _VariabelsSourceList { get; set; } = null;
            public string _OutputVariabelsSourceProperty { get; set; } = "";
            public IEnumerable<RestrictedMappingType>? _RestrictedMappingTypes { get; set; } = null;

            public TemplateOptions(string dataTypeProperty, string dataValueProperty)
            {
                _DataTypeProperty = dataTypeProperty;
                _DataValueProperty = dataValueProperty;               
            }
        }

        public string MappedValue
        {
            get
            {
                return (string)GetValue(MappedValueProperty);
            }
            set
            {

                SetValue(MappedValueProperty, value);
                if (WorkSpace.Instance != null
                    && WorkSpace.Instance.RunsetExecutor != null
                    && WorkSpace.Instance.RunsetExecutor.RunSetConfig != null)
                {
                    WorkSpace.Instance.RunsetExecutor.RunSetConfig.DirtyStatus = eDirtyStatus.Modified;
                }
            }
        }

        public Guid MappedValueGUID
        {
            get
            {
                Guid convertedGUID = Guid.Empty;
                Guid.TryParse(MappedValue, out convertedGUID);
                return convertedGUID;
            }
            set
            {
                MappedValue = value.ToString();
            }
        }

        public IEnumerable<RestrictedMappingType> RestrictedMappingTypes
        {
            get
            {
                return (IEnumerable<RestrictedMappingType>)GetValue(RestrictedMappingTypesProperty);
            }
            set
            {
                SetValue(RestrictedMappingTypesProperty, value);
            }
        }

        private string? _prevMappedType = null;

        public string MappedType
        {
            get { return (string)GetValue(MappedTypeProperty); }
            set { SetValue(MappedTypeProperty, value); }
        }

        ObservableList<string> mVariablesList = null;
        ObservableList<VariableBase> mOutputVariablesList = null;
        ObservableList<VariableBase> mGlobalVariablesList = null;
        ObservableList<GlobalAppModelParameter> mModelGlobalParamsList = null;

        bool EnableDataMapping = false;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public UCDataMapping()
        {
            InitializeComponent();

            this.IsEnabled = false;
            InitTypeOptions();
            InitValuesOptions();
        }

        #region Global
        private void InitValuesOptions()
        {
            mVariablesList = [];
            mOutputVariablesList = [];
            SetGlobalVariabelsListValues();
            SetModelGlobalParametersListValues();
            SetDataSourceValues();
            SetDatabaseValues();
        }

        private void InitTypeOptions()
        {
            GingerCore.General.FillComboItemsFromEnumType(xMappedTypeComboBox, typeof(eDataType));
            BindingHandler.ObjFieldBinding(xMappedTypeComboBox, ComboBox.SelectedValueProperty, this, nameof(MappedType));

            DisableAllTypeOptions();
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
            BindingOperations.ClearAllBindings(xOptionalValuesComboBox);
            BindingOperations.ClearAllBindings(xDSExpressionTxtbox);
            if (MappedType == eDataType.Variable.ToString())
            {
                BindingHandler.ObjFieldBinding(xVariablesComboBox, ComboBox.SelectedValueProperty, this, nameof(MappedValue));
            }
            else if (MappedType == eDataType.GlobalVariable.ToString())
            {
                BindingHandler.ObjFieldBinding(xOptionalValuesComboBox, ComboBox.SelectedValueProperty, this, nameof(MappedValueGUID));
            }
            else if (MappedType == eDataType.OutputVariable.ToString())
            {
                BindingHandler.ObjFieldBinding(xOptionalValuesComboBox, ComboBox.SelectedValueProperty, this, nameof(MappedValue));
            }
            else if (MappedType == eDataType.ApplicationModelParameter.ToString())
            {
                BindingHandler.ObjFieldBinding(xOptionalValuesComboBox, ComboBox.SelectedValueProperty, this, nameof(MappedValueGUID));
            }
            else if (MappedType == eDataType.DataSource.ToString())
            {
                BindingHandler.ObjFieldBinding(xDSExpressionTxtbox, TextBox.TextProperty, this, nameof(MappedValue));
            }
            else if (MappedType == eDataType.ValueExpression.ToString())
            {
               var  mContext = Context.GetAsContext( WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().FirstOrDefault());
                xDBValueExpression.BindControl(mContext, this, nameof(MappedValue));
            }

            SetValueControlsData();

            SetValueControlsView();
        }

        private void SetValueControlsView()
        {
            if (MappedType == null)
            {
                return;
            }
            else
            {
                if (MappedType == eDataType.None.ToString())
                {
                    xMappedTypeColumn.Width = new GridLength(50, GridUnitType.Star);
                    xMappedTypeColumn.MaxWidth = double.MaxValue;
                    xMappedValueColumn.Width = new GridLength(0);
                }
                else
                {
                    xMappedTypeColumn.Width = new GridLength(50, GridUnitType.Star);
                    xMappedTypeColumn.MaxWidth = 180;
                    xMappedValueColumn.Width = new GridLength(50, GridUnitType.Star);
                }

                if (MappedType == eDataType.Variable.ToString()
                    && xVariablesComboBox != null)
                {
                    xVariablesComboBox.Visibility = Visibility.Visible;
                }
                else
                {
                    xVariablesComboBox.Visibility = Visibility.Hidden;
                }

                if ((MappedType == eDataType.OutputVariable.ToString() || MappedType == eDataType.GlobalVariable.ToString() || MappedType == eDataType.ApplicationModelParameter.ToString())
                    && xOptionalValuesComboBox != null)
                {
                    xOptionalValuesComboBox.Visibility = Visibility.Visible;
                }
                else
                {
                    xOptionalValuesComboBox.Visibility = Visibility.Hidden;
                }

                if (MappedType == eDataType.DataSource.ToString()  && xDSExpressionTxtbox != null)
                {
                    xDSExpressionTxtbox.Visibility = Visibility.Visible;
                    xDSConfigBtn.Visibility = Visibility.Visible;
                }
                else
                {
                    xDSExpressionTxtbox.Visibility = Visibility.Hidden;
                    xDSConfigBtn.Visibility = Visibility.Hidden;
                }

                if (MappedType == eDataType.ValueExpression.ToString() && xDBValueExpression != null)
                {
                    xDBValueExpression.Visibility = Visibility.Visible;
                }
                else
                {
                    xDBValueExpression.Visibility = Visibility.Hidden;
                }
            }
        }

        private static void OnMappedValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is UCDataMapping control)
            {
                control.MappedValuePropertyChanged((string)args.NewValue);
            }
        }
        private void MappedValuePropertyChanged(string mappedValueProperty)
        {
            if (MappedType == eDataType.OutputVariable.ToString()
                || MappedType == eDataType.GlobalVariable.ToString()
                || MappedType == eDataType.ApplicationModelParameter.ToString())
            {
                OnPropertyChanged(nameof(MappedValueGUID));
            }
            else
            {
                OnPropertyChanged(nameof(MappedValue));
            }

            MarkMappedValueValidation();
        }

        private void MarkMappedValueValidation()
        {
            bool isValid = true;

            if ((MappedType != eDataType.None.ToString() && MappedValue == string.Empty)
                || (MappedType == eDataType.Variable.ToString() && !GingerCore.General.CheckComboItemExist(xVariablesComboBox, MappedValue))
                || ((MappedType == eDataType.OutputVariable.ToString() && !GingerCore.General.CheckComboItemExist(xOptionalValuesComboBox, MappedValue, nameof(VariableBase.VariableInstanceInfo)))
                || (MappedType == eDataType.GlobalVariable.ToString() || MappedType == eDataType.ApplicationModelParameter.ToString()) && !GingerCore.General.CheckComboItemExist(xOptionalValuesComboBox, MappedValue, "Guid"))
                || (MappedType == eDataType.DataSource.ToString() && GingerCoreNET.GeneralLib.General.CheckDataSource(MappedValue, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>()) != string.Empty))
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

        private void xMappedTypeComboBox_DropDownOpened(object sender, EventArgs e)
        {
            _prevMappedType = MappedType;
        }

        private void xMappedTypeComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (RestrictedMappingTypes != null)
            {
                RestrictedMappingType? restrictedMappingType = RestrictedMappingTypes
                    .FirstOrDefault(restrictedType => string.Equals(restrictedType.Name, MappedType));

                if (restrictedMappingType != null && !string.Equals(_prevMappedType, MappedType))
                {
                    MappedType = _prevMappedType!;
                    string? reason = restrictedMappingType.Reason;
                    if (reason == null)
                    {
                        reason = $"{restrictedMappingType.Name} is not allowed for Mapped Runtime Value.";
                    }
                    Reporter.ToUser(eUserMsgKey.NotAllowedForMappedRuntimeValue, reason);
                }
            }

            if (_prevMappedType != MappedType)
            {
                //reset value between type selection
                MappedValue = string.Empty;
            }

            if (MappedType == eDataType.None.ToString())
            {
                MappedValue = string.Empty;
            }
        }

        private static void OnEnableDataMappingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is UCDataMapping control)
            {
                control.EnableDataMappingPropertyChanged((bool)args.NewValue);
            }
        }
        private void EnableDataMappingPropertyChanged(bool enabelMapping)
        {
            EnableDataMapping = enabelMapping;
            if (EnableDataMapping == true)
            {
                this.IsEnabled = true;
            }
        }

        private void SetValueControlsData()
        {
            if (MappedType == eDataType.Variable.ToString())
            {
                xVariablesComboBox.ItemsSource = mVariablesList;
            }
            else if (MappedType == eDataType.GlobalVariable.ToString())
            {
                xOptionalValuesComboBox.DisplayMemberPath = nameof(VariableBase.Name);
                xOptionalValuesComboBox.SelectedValuePath = nameof(VariableBase.Guid);
                xOptionalValuesComboBox.ItemsSource = mGlobalVariablesList.OrderBy(nameof(VariableBase.Name));
            }
            else if (MappedType == eDataType.OutputVariable.ToString())
            {
                xOptionalValuesComboBox.DisplayMemberPath = nameof(VariableBase.Path);
                xOptionalValuesComboBox.SelectedValuePath = nameof(VariableBase.VariableInstanceInfo);
                xOptionalValuesComboBox.ItemsSource = mOutputVariablesList;
            }
            else if (MappedType == eDataType.ApplicationModelParameter.ToString())
            {
                xOptionalValuesComboBox.DisplayMemberPath = nameof(GlobalAppModelParameter.PlaceHolder);
                xOptionalValuesComboBox.SelectedValuePath = nameof(GlobalAppModelParameter.Guid);
                xOptionalValuesComboBox.ItemsSource = mModelGlobalParamsList.OrderBy(nameof(GlobalAppModelParameter.PlaceHolder));
            }

            MarkMappedValueValidation();
        }

        private void DisableAllTypeOptions()
        {
            GingerCore.General.DisableComboItem(xMappedTypeComboBox, eDataType.Variable);
            GingerCore.General.DisableComboItem(xMappedTypeComboBox, eDataType.OutputVariable);
            GingerCore.General.DisableComboItem(xMappedTypeComboBox, eDataType.GlobalVariable);
            GingerCore.General.DisableComboItem(xMappedTypeComboBox, eDataType.ApplicationModelParameter);
            GingerCore.General.DisableComboItem(xMappedTypeComboBox, eDataType.DataSource);
            GingerCore.General.DisableComboItem(xMappedTypeComboBox, eDataType.ValueExpression);
            xDSConfigBtn.IsEnabled = false;
            xDBValueExpression.Visibility = Visibility.Collapsed;
        }

        public static DataTemplate GetTemplate(TemplateOptions options)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory ucDataMapping = new FrameworkElementFactory(typeof(UCDataMapping));

            if (!string.IsNullOrEmpty(options._VariabelsSourceProperty))
            {
                Binding variablesSourceBinding = new Binding(options._VariabelsSourceProperty)
                {
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                ucDataMapping.SetBinding(UCDataMapping.VariabelsSourceProperty, variablesSourceBinding);
            }
            else if (options._VariabelsSourceList != null)
            {
                ucDataMapping.SetValue(UCDataMapping.VariabelsSourceProperty, options._VariabelsSourceList);
            }


            if (!string.IsNullOrEmpty(options._OutputVariabelsSourceProperty))
            {
                Binding outputVariabelsSourceBinding = new Binding(options._OutputVariabelsSourceProperty)
                {
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                ucDataMapping.SetBinding(UCDataMapping.OutputVariabelsSourceProperty, outputVariabelsSourceBinding);
            }

            Binding mappedItemTypeBinding = new Binding(options._DataTypeProperty)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            ucDataMapping.SetBinding(UCDataMapping.MappedTypeProperty, mappedItemTypeBinding);

            Binding mappedValueBinding = new Binding(options._DataValueProperty)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            ucDataMapping.SetBinding(UCDataMapping.MappedValueProperty, mappedValueBinding);

            if (!string.IsNullOrEmpty(options._EnableDataMappingProperty))
            {
                Binding allowDataMappingBinding = new Binding(options._EnableDataMappingProperty)
                {
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                ucDataMapping.SetBinding(UCDataMapping.EnableDataMappingProperty, allowDataMappingBinding);
            }
            else
            {
                ucDataMapping.SetValue(UCDataMapping.EnableDataMappingProperty, true);
            }

            if (options._RestrictedMappingTypes != null)
            {
                ucDataMapping.SetValue(UCDataMapping.RestrictedMappingTypesProperty, options._RestrictedMappingTypes);
            }

            template.VisualTree = ucDataMapping;
            return template;
        }
        #endregion Global

        #region Variables
        private static void OnVariabelsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is UCDataMapping control)
            {
                control.VariabelsSourcePropertyChanged((ObservableList<string>)args.NewValue);
            }
        }
        private void VariabelsSourcePropertyChanged(ObservableList<string> variabelsSourceList)
        {
            if (variabelsSourceList == null || variabelsSourceList.Count == 0 || (variabelsSourceList.Count == 1 && variabelsSourceList[0] == string.Empty))
            {
                GingerCore.General.DisableComboItem(xMappedTypeComboBox, eDataType.Variable);
            }
            if (variabelsSourceList != null && variabelsSourceList.Where(x => string.IsNullOrEmpty(x) == false).ToList().Count > 0)
            {
                if (EnableDataMapping)
                {
                    GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.Variable);
                }
                mVariablesList = variabelsSourceList;
                mVariablesList.CollectionChanged += VariabelsSourceList_CollectionChanged;
                SetValueControlsData();
            }
        }
        private void VariabelsSourceList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetValueControlsData();
        }
        #endregion Variables

        #region Output Variables
        private static void OnOutputVariabelsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is UCDataMapping control)
            {
                control.OutputVariabelsSourcePropertyChanged((ObservableList<VariableBase>)args.NewValue);
            }
        }
        private void OutputVariabelsSourcePropertyChanged(ObservableList<VariableBase> outputVariabelsSourceList)
        {
            if (outputVariabelsSourceList == null || outputVariabelsSourceList.Count == 0)
            {
                GingerCore.General.DisableComboItem(xMappedTypeComboBox, eDataType.OutputVariable);
            }

            if (outputVariabelsSourceList != null && outputVariabelsSourceList.Count > 0)
            {
                if (EnableDataMapping)
                {
                    GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.OutputVariable);
                }
                mOutputVariablesList = outputVariabelsSourceList;
                mOutputVariablesList.CollectionChanged += OutputVariabelsSourceList_CollectionChanged;
                SetValueControlsData();
            }
        }
        private void OutputVariabelsSourceList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetValueControlsData();
        }
        #endregion Output Variables

        #region Global Variables
        private void SetGlobalVariabelsListValues()
        {
            mGlobalVariablesList = [];
            WorkSpace.Instance.Solution.Variables.CollectionChanged += GlobalVariables_CollectionChanged;
            foreach (VariableBase var in WorkSpace.Instance.Solution.Variables.Where(x => x.SupportSetValue == true).ToList())
            {
                mGlobalVariablesList.Add(var);
            }

            if (mGlobalVariablesList.Count > 0)
            {
                GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.GlobalVariable);
            }
        }

        private void GlobalVariables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //update list
            foreach (VariableString var in WorkSpace.Instance.Solution.Variables.Where(x => x is VariableString).ToList())
            {
                if (mGlobalVariablesList.Contains(var) == false)
                {
                    mGlobalVariablesList.Add(var);
                }
            }
            for (int indx = 0; indx < mGlobalVariablesList.Count; indx++)
            {
                if (WorkSpace.Instance.Solution.Variables.Contains(mGlobalVariablesList[indx]) == false)
                {
                    mGlobalVariablesList.Remove(mGlobalVariablesList[indx]);
                    indx--;
                }
            }

            SetValueControlsData();
        }
        #endregion Global Variables

        #region Model Global Parameters
        private void SetModelGlobalParametersListValues()
        {
            mModelGlobalParamsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>();
            mModelGlobalParamsList.CollectionChanged += MModelGlobalParamsList_CollectionChanged;

            if (mModelGlobalParamsList.Count > 0)
            {
                GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.ApplicationModelParameter);
            }
        }

        private void MModelGlobalParamsList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetValueControlsData();
        }
        #endregion Model Global Parameters

        #region DataSource
        private void SetDataSourceValues()
        {
            if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>().Any())
            {
                GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.DataSource);
                xDSConfigBtn.IsEnabled = true;
            }
        }

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

        #region Database
        private void SetDatabaseValues()
        {
            if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Any())
            {
                GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.ValueExpression);
            }
            xDBValueExpression.Visibility = Visibility.Visible;
        }
        #endregion Database

    }
}
