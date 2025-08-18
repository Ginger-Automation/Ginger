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
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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
            ValueExpression,
            Value
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

        public sealed class ValidationResult
        {
            public bool IsValid { get; }
            public string ErrorMessage { get; }

            public ValidationResult(bool isValid, string errorMessage = null)
            {
                IsValid = isValid;
                ErrorMessage = errorMessage;
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
                
                // Whenever restrictions are set, ensure fundamental options remain available
                if (xMappedTypeComboBox != null)
                {
                    GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.None);
                    GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.Value);
                }
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
            
            // Subscribe to the Unloaded event to clean up event handlers
            this.Unloaded += UCDataMapping_Unloaded;
        }

        private void UCDataMapping_Unloaded(object sender, RoutedEventArgs e)
        {
            // Clean up event handlers to prevent memory leaks
            xValueTextBox.TextChanged -= xValueTextBox_TextChanged;
            xValueTextBox.PreviewTextInput -= xValueTextBox_PreviewTextInput;
            // Note: xSelectionListComboBox_SelectionChanged is managed manually in UpdateSelectionListComboBoxSelection
            this.Unloaded -= UCDataMapping_Unloaded;
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

        private void DisableAllTypeOptions()
        {
            // Disable all data type options initially - they will be enabled based on context availability
            GingerCore.General.DisableComboItem(xMappedTypeComboBox, eDataType.Variable);
            GingerCore.General.DisableComboItem(xMappedTypeComboBox, eDataType.GlobalVariable);
            GingerCore.General.DisableComboItem(xMappedTypeComboBox, eDataType.OutputVariable);
            GingerCore.General.DisableComboItem(xMappedTypeComboBox, eDataType.ApplicationModelParameter);
            GingerCore.General.DisableComboItem(xMappedTypeComboBox, eDataType.DataSource);
            GingerCore.General.DisableComboItem(xMappedTypeComboBox, eDataType.ValueExpression);
            
            // Always enable "None" and "Value" options - these should always be available
            GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.None);
            GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.Value);

            // Force refresh of combo box items to ensure changes are applied
            try
            {
                xMappedTypeComboBox.Items.Refresh();
            }
            catch
            {
                // Ignore any refresh errors
            }
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
            BindingOperations.ClearAllBindings(xSelectionListComboBox);
            BindingOperations.ClearAllBindings(xDSExpressionTxtbox);
            BindingOperations.ClearAllBindings(xValueTextBox);
            
            // Remove previous event handlers to avoid multiple subscriptions
            xValueTextBox.TextChanged -= xValueTextBox_TextChanged;
            xValueTextBox.PreviewTextInput -= xValueTextBox_PreviewTextInput;
            
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
            else if (MappedType == eDataType.Value.ToString())
            {
                if (IsSelectionListVariable())
                {
                    // For selection list variables, don't bind directly - handle through selection event
                    // The ComboBox selection will update MappedValue through the SelectionChanged event
                }
                else if (IsDateTimeVariable())
                {
                    // For DateTime variables, don't bind directly - handle through DateTimePicker event
                    // The DateTimePicker will update MappedValue through the TextChanged event
                }
                else
                {
                    BindingHandler.ObjFieldBinding(xValueTextBox, TextBox.TextProperty, this, nameof(MappedValue));
                    
                    // Add input validation for number variables
                    xValueTextBox.TextChanged += xValueTextBox_TextChanged;
                    xValueTextBox.PreviewTextInput += xValueTextBox_PreviewTextInput;
                }
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

                // Handle Selection List ComboBox visibility for VariableSelectionList
                if (MappedType == eDataType.Value.ToString() && IsSelectionListVariable() && xSelectionListComboBox != null)
                {
                    xSelectionListComboBox.Visibility = Visibility.Visible;
                    xValueTextBox.Visibility = Visibility.Hidden;
                    xDateTimePickerHost.Visibility = Visibility.Hidden;
                }
                // Handle DateTime Picker visibility for VariableDateTime
                else if (MappedType == eDataType.Value.ToString() && IsDateTimeVariable() && xDateTimePickerHost != null)
                {
                    xDateTimePickerHost.Visibility = Visibility.Visible;
                    xValueTextBox.Visibility = Visibility.Hidden;
                    xSelectionListComboBox.Visibility = Visibility.Hidden;
                }
                else
                {
                    xSelectionListComboBox.Visibility = Visibility.Hidden;
                    xDateTimePickerHost.Visibility = Visibility.Hidden;
                    
                    if (MappedType == eDataType.Value.ToString() && xValueTextBox != null)
                    {
                        xValueTextBox.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        xValueTextBox.Visibility = Visibility.Hidden;
                    }
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

            // Update selection list combo box if we're in Value mode with a selection list variable
            if (MappedType == eDataType.Value.ToString() && IsSelectionListVariable())
            {
                UpdateSelectionListComboBoxSelection();
            }
            // Update DateTime picker if we're in Value mode with a DateTime variable
            else if (MappedType == eDataType.Value.ToString() && IsDateTimeVariable())
            {
                UpdateDateTimePickerSelection();
            }

            MarkMappedValueValidation();
        }

        private void MarkMappedValueValidation()
        {
            bool isValid = true;
            string validationMessage = null;

            if ((MappedType != eDataType.None.ToString() && MappedType != eDataType.Value.ToString() && MappedValue == string.Empty)
                || (MappedType == eDataType.Variable.ToString() && !GingerCore.General.CheckComboItemExist(xVariablesComboBox, MappedValue))
                || ((MappedType == eDataType.OutputVariable.ToString() && !GingerCore.General.CheckComboItemExist(xOptionalValuesComboBox, MappedValue, nameof(VariableBase.VariableInstanceInfo)))
                || (MappedType == eDataType.GlobalVariable.ToString() || MappedType == eDataType.ApplicationModelParameter.ToString()) && !GingerCore.General.CheckComboItemExist(xOptionalValuesComboBox, MappedValue, "Guid"))
                || (MappedType == eDataType.DataSource.ToString() && GingerCoreNET.GeneralLib.General.CheckDataSource(MappedValue, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>()) != string.Empty))
            {
                isValid = false;
            }
            else if (MappedType == eDataType.Value.ToString())
            {
                var valueValidationResult = ValidateValueForVariableType();
                isValid = valueValidationResult.IsValid;
                validationMessage = valueValidationResult.ErrorMessage;
            }

            if (isValid == false)
            {
                this.BorderThickness = new Thickness(1);
                this.BorderBrush = Brushes.Red;
                
                // Set tooltip with validation message if available
                if (!string.IsNullOrEmpty(validationMessage))
                {
                    xValueTextBox.ToolTip = validationMessage;
                }
            }
            else
            {
                this.BorderThickness = new Thickness(0);
                this.BorderBrush = null;
                
                // Clear tooltip when valid
                if (MappedType == eDataType.Value.ToString())
                {
                    xValueTextBox.ToolTip = null;
                }
            }
        }

        private void xValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MarkMappedValueValidation();
        }

        private void xValueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Check if this is a number variable context based on the DataContext
            if (DataContext is VariableNumber)
            {
                // Allow only valid numeric input for number variables
                TextBox textBox = (TextBox)sender;
                e.Handled = !IsValidNumericInput(e.Text, textBox);
            }
        }

        private bool IsValidNumericInput(string inputText, TextBox textBox)
        {
            // Get the current text and construct what the new text would be
            string currentText = textBox.Text ?? "";
            int selectionStart = textBox.SelectionStart;
            int selectionLength = textBox.SelectionLength;
            
            // Construct the new text after the input
            string newText = currentText.Remove(selectionStart, selectionLength).Insert(selectionStart, inputText);
            
            // Allow empty string (user can delete everything)
            if (string.IsNullOrEmpty(newText))
            {
                return true;
            }
            
            // Allow single minus sign at the beginning for negative numbers
            if (newText == "-")
            {
                return true;
            }
            
            // Allow single decimal point
            if (newText == ".")
            {
                return true;
            }
            
            // Use regex to check for valid numeric input (including decimal numbers)
            // This allows: optional minus, digits, optional decimal point with digits
            var numericRegex = new Regex(@"^-?(\d+\.?\d*|\.\d+)$");
            return numericRegex.IsMatch(newText);
        }

        private ValidationResult ValidateValueForVariableType()
        {
            if (string.IsNullOrEmpty(MappedValue))
            {
                return new ValidationResult(true); // Empty value is valid for Value type
            }

            // Check if the value contains only valid characters
            if (MappedValue.Any(c => char.IsControl(c) && c != '\t' && c != '\n' && c != '\r'))
            {
                return new ValidationResult(false, "Value contains invalid control characters");
            }

            // If the DataContext is a number variable, validate it as a number
            if (DataContext is VariableNumber)
            {
                return ValidateNumericValue(MappedValue);
            }

            return new ValidationResult(true); // Default validation passes
        }

        private ValidationResult ValidateNumericValue(string value)
        {
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double numericValue))
            {
                return new ValidationResult(false, "Invalid numeric format");
            }

            return new ValidationResult(true);
        }

        private void xMappedTypeComboBox_DropDownOpened(object sender, EventArgs e)
        {
            _prevMappedType = MappedType;
        }

        private void xMappedTypeComboBox_DropDownClosed(object sender, EventArgs e)
        {
            // Handle restricted mapping types, but never restrict "Value" or "None" options
            if (RestrictedMappingTypes != null)
            {
                RestrictedMappingType? restrictedMappingType = RestrictedMappingTypes
                    .FirstOrDefault(restrictedType => string.Equals(restrictedType.Name, MappedType));

                // Never restrict "Value" or "None" options as they are fundamental
                if (restrictedMappingType != null && 
                    !string.Equals(_prevMappedType, MappedType) &&
                    MappedType != eDataType.Value.ToString() &&
                    MappedType != eDataType.None.ToString())
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

            // Ensure "Value" option is always available after any restriction processing
            GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.Value);
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
                
                // When data mapping is enabled, ensure fundamental options are available
                GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.None);
                GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.Value);
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
            else if (MappedType == eDataType.Value.ToString() && IsSelectionListVariable())
            {
                SetupSelectionListComboBox();
            }
            else if (MappedType == eDataType.Value.ToString() && IsDateTimeVariable())
            {
                SetupDateTimePicker();
            }

            MarkMappedValueValidation();
        }

        private bool IsSelectionListVariable()
        {
            // Check if the DataContext is a VariableSelectionList or VariableList
            return DataContext is VariableSelectionList || DataContext is VariableList;
        }

        private bool IsDateTimeVariable()
        {
            // Check if the DataContext is a VariableDateTime
            return DataContext is VariableDateTime;
        }

        private void SetupSelectionListComboBox()
        {
            if (DataContext is VariableSelectionList selectionListVariable)
            {
                // Clear any existing items
                xSelectionListComboBox.ItemsSource = null;
                
                // Create a list of option values from the VariableSelectionList
                ObservableList<string> optionValues = [];
                
                foreach (var optionalValue in selectionListVariable.OptionalValuesList)
                {
                    optionValues.Add(optionalValue.Value);
                }
                
                // Set the ComboBox items
                xSelectionListComboBox.ItemsSource = optionValues;
                
                // Set the current selection if it exists in the list
                if (!string.IsNullOrEmpty(MappedValue) && optionValues.Contains(MappedValue))
                {
                    xSelectionListComboBox.SelectedItem = MappedValue;
                }
                else if (optionValues.Count > 0)
                {
                    // Default to the first option and update the MappedValue
                    xSelectionListComboBox.SelectedIndex = 0;
                    MappedValue = optionValues[0];
                }
            }
            else if (DataContext is VariableList listVariable)
            {
                // Clear any existing items
                xSelectionListComboBox.ItemsSource = null;
                
                // Create a list of option values from the VariableList
                ObservableList<string> optionValues = [];
                
                // Parse the ValueList (which can be comma-separated or newline-separated)
                string formula = listVariable.GetFormula();
                if (!string.IsNullOrEmpty(formula))
                {
                    string[] values = formula.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var value in values)
                    {
                        string trimmedValue = value.Trim();
                        if (!string.IsNullOrEmpty(trimmedValue))
                        {
                            optionValues.Add(trimmedValue);
                        }
                    }
                }
                
                // Set the ComboBox items
                xSelectionListComboBox.ItemsSource = optionValues;
                
                // Set the current selection if it exists in the list
                if (!string.IsNullOrEmpty(MappedValue) && optionValues.Contains(MappedValue))
                {
                    xSelectionListComboBox.SelectedItem = MappedValue;
                }
                else if (optionValues.Count > 0)
                {
                    // Default to the first option and update the MappedValue
                    xSelectionListComboBox.SelectedIndex = 0;
                    MappedValue = optionValues[0];
                }
            }
        }

        private void SetupDateTimePicker()
        {
            if (DataContext is VariableDateTime dateTimeVariable)
            {
                // Set up the DateTimePicker with the variable's properties
                xDateTimePicker.CustomFormat = dateTimeVariable.DateTimeFormat;
                
                try
                {
                    xDateTimePicker.MinDate = Convert.ToDateTime(dateTimeVariable.MinDateTime);
                    xDateTimePicker.MaxDate = Convert.ToDateTime(dateTimeVariable.MaxDateTime);
                    
                    // Set the current value
                    if (!string.IsNullOrEmpty(MappedValue))
                    {
                        if (DateTime.TryParse(MappedValue, out DateTime parsedDate))
                        {
                            xDateTimePicker.Value = parsedDate;
                        }
                        else
                        {
                            // If MappedValue can't be parsed, use the variable's current value or initial value
                            string valueToUse = !string.IsNullOrEmpty(dateTimeVariable.Value) ? dateTimeVariable.Value : dateTimeVariable.InitialDateTime;
                            if (DateTime.TryParse(valueToUse, out DateTime defaultDate))
                            {
                                xDateTimePicker.Value = defaultDate;
                                MappedValue = defaultDate.ToString(dateTimeVariable.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
                            }
                        }
                    }
                    else
                    {
                        // If no MappedValue, use the variable's current value or initial value
                        string valueToUse = !string.IsNullOrEmpty(dateTimeVariable.Value) ? dateTimeVariable.Value : dateTimeVariable.InitialDateTime;
                        if (DateTime.TryParse(valueToUse, out DateTime defaultDate))
                        {
                            xDateTimePicker.Value = defaultDate;
                            MappedValue = defaultDate.ToString(dateTimeVariable.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any date parsing errors gracefully
                    Reporter.ToLog(eLogLevel.WARN, $"Error setting up DateTime picker: {ex.Message}");
                    try
                    {
                        xDateTimePicker.Value = DateTime.Now;
                        MappedValue = DateTime.Now.ToString(dateTimeVariable.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        // Last resort - use default format
                        xDateTimePicker.Value = DateTime.Now;
                        MappedValue = DateTime.Now.ToString("MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
            }
        }

        /// <summary>
        /// Updates only the DateTime picker value without triggering events
        /// </summary>
        private void UpdateDateTimePickerSelection()
        {
            if (xDateTimePicker != null && DataContext is VariableDateTime dateTimeVariable)
            {
                // Temporarily disable the text changed event to prevent loops
                xDateTimePicker.TextChanged -= xDateTimePicker_TextChanged;
                
                try
                {
                    if (!string.IsNullOrEmpty(MappedValue))
                    {
                        if (DateTime.TryParse(MappedValue, out DateTime parsedDate))
                        {
                            xDateTimePicker.Value = parsedDate;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, $"Error updating DateTime picker selection: {ex.Message}");
                }
                finally
                {
                    // Re-enable the text changed event
                    xDateTimePicker.TextChanged += xDateTimePicker_TextChanged;
                }
            }
        }

        private void xDateTimePicker_TextChanged(object sender, EventArgs e)
        {
            if (DataContext is VariableDateTime dateTimeVariable)
            {
                try
                {
                    // Validate the date is within the allowed range
                    if (!dateTimeVariable.CheckDateTimeWithInRange(xDateTimePicker.Value.ToString()))
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Selected date is not in range: Min=[{dateTimeVariable.MinDateTime}], Max=[{dateTimeVariable.MaxDateTime}]");
                        return;
                    }
                    
                    // Convert the selected date to the proper format and update MappedValue
                    string formattedDate = xDateTimePicker.Value.ToString(dateTimeVariable.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
                    if (MappedValue != formattedDate)
                    {
                        MappedValue = formattedDate;
                        
                        // Update the actual variable value as well
                        dateTimeVariable.Value = formattedDate;
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Error updating DateTime value: {ex.Message}");
                }
            }
        }

        private void xSelectionListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem != null)
            {
                // Update the MappedValue when the user selects a different option
                string selectedValue = comboBox.SelectedItem.ToString();
                if (MappedValue != selectedValue)
                {
                    MappedValue = selectedValue;
                    
                    // Update the actual variable value as well
                    if (DataContext is VariableSelectionList selectionListVariable)
                    {
                        selectionListVariable.Value = selectedValue;
                    }
                    else if (DataContext is VariableList listVariable)
                    {
                        listVariable.Value = selectedValue;
                    }
                }
            }
        }

        /// <summary>
        /// Updates only the selection in the selection list combo box without rebuilding the entire list
        /// </summary>
        private void UpdateSelectionListComboBoxSelection()
        {
            if (xSelectionListComboBox?.ItemsSource != null)
            {
                // Temporarily disable the selection changed event to prevent loops
                xSelectionListComboBox.SelectionChanged -= xSelectionListComboBox_SelectionChanged;
                
                try
                {
                    // Update the selection to match the current MappedValue
                    if (!string.IsNullOrEmpty(MappedValue))
                    {
                        // Check if the value exists in the combo box items
                        bool valueFound = false;
                        foreach (var item in xSelectionListComboBox.ItemsSource)
                        {
                            if (item.ToString() == MappedValue)
                            {
                                xSelectionListComboBox.SelectedItem = MappedValue;
                                valueFound = true;
                                break;
                            }
                        }
                        
                        // If value not found, clear selection
                        if (!valueFound)
                        {
                            xSelectionListComboBox.SelectedItem = null;
                        }
                    }
                    else
                    {
                        xSelectionListComboBox.SelectedItem = null;
                    }
                }
                finally
                {
                    // Re-enable the selection changed event
                    xSelectionListComboBox.SelectionChanged += xSelectionListComboBox_SelectionChanged;
                }
            }
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
            // Removed: xDBValueExpression.Visibility = Visibility.Visible; - this was causing interference
        }
        #endregion Database

        #region Template Creation
        /// <summary>
        /// Creates a DataTemplate for UCDataMapping control to be used in grid views
        /// </summary>
        /// <param name="options">Template configuration options</param>
        /// <returns>DataTemplate for the control</returns>
        public static DataTemplate GetTemplate(TemplateOptions options)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory ucDataMapping = new FrameworkElementFactory(typeof(UCDataMapping));

            // Bind data type property
            if (!string.IsNullOrEmpty(options._DataTypeProperty))
            {
                Binding dataTypeBinding = new Binding(options._DataTypeProperty)
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                ucDataMapping.SetBinding(MappedTypeProperty, dataTypeBinding);
            }

            // Bind data value property
            if (!string.IsNullOrEmpty(options._DataValueProperty))
            {
                Binding dataValueBinding = new Binding(options._DataValueProperty)
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                ucDataMapping.SetBinding(MappedValueProperty, dataValueBinding);
            }

            // Bind enable data mapping property
            if (!string.IsNullOrEmpty(options._EnableDataMappingProperty))
            {
                Binding enableDataMappingBinding = new Binding(options._EnableDataMappingProperty)
                {
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                ucDataMapping.SetBinding(EnableDataMappingProperty, enableDataMappingBinding);
            }

            // Bind variables source property
            if (!string.IsNullOrEmpty(options._VariabelsSourceProperty))
            {
                Binding variablesSourceBinding = new Binding(options._VariabelsSourceProperty)
                {
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                ucDataMapping.SetBinding(VariabelsSourceProperty, variablesSourceBinding);
            }
            else if (options._VariabelsSourceList != null)
            {
                ucDataMapping.SetValue(VariabelsSourceProperty, options._VariabelsSourceList);
            }

            // Bind output variables source property
            if (!string.IsNullOrEmpty(options._OutputVariabelsSourceProperty))
            {
                Binding outputVariablesSourceBinding = new Binding(options._OutputVariabelsSourceProperty)
                {
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                ucDataMapping.SetBinding(OutputVariabelsSourceProperty, outputVariablesSourceBinding);
            }

            // Set restricted mapping types
            if (options._RestrictedMappingTypes != null)
            {
                ucDataMapping.SetValue(RestrictedMappingTypesProperty, options._RestrictedMappingTypes);
            }

            template.VisualTree = ucDataMapping;
            return template;
        }
        #endregion Template Creation

    }
}
