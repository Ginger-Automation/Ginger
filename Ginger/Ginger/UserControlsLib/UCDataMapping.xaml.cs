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

        public sealed class ValueValidationResult
        {
            public bool IsValid { get; }
            public string? ErrorMessage { get; }

            public ValueValidationResult(bool isValid, string? errorMessage = null)
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
                if (value == null)
                {
                    return;
                }
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

        bool EnableDataMapping = true;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
            {
                return;
            }
            PropertyChangedEventHandler? handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public UCDataMapping()
        {
            InitializeComponent();


            InitTypeOptions();
            InitValuesOptions();

            // IMMEDIATELY configure DatePicker to allow past dates
            if (xDatePickerWPF != null)
            {
                try
                {
                    xDatePickerWPF.DisplayDateStart = null;
                    xDatePickerWPF.DisplayDateEnd = null;
                    xDatePickerWPF.BlackoutDates.Clear();
                }
                catch { /* Ignore initialization errors */ }
            }

            // Subscribe to the Unloaded event to clean up event handlers
            this.Unloaded += UCDataMapping_Unloaded;

            // Subscribe to the Loaded event to ensure proper initialization
            this.Loaded += UCDataMapping_Loaded;

            // Subscribe to DataContext changes to refresh the control when the variable changes
            this.DataContextChanged += UCDataMapping_DataContextChanged;
        }



        private void UCDataMapping_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // When the DataContext changes (e.g., in a grid template), refresh the control
            if (e.NewValue != null && MappedType == eDataType.Value.ToString())
            {
                SetValueControlsData();
                SetValueControlsView();
            }
        }

        private void UCDataMapping_Loaded(object sender, RoutedEventArgs e)
        {
            // Force a refresh of the UI when the control is loaded
            // This ensures DateTime pickers are properly set up if the DataContext was set after initialization

            // FORCE DatePicker to allow past dates IMMEDIATELY on load
            if (xDatePickerWPF != null)
            {
                try
                {
                    xDatePickerWPF.DisplayDateStart = null;
                    xDatePickerWPF.DisplayDateEnd = null;
                    xDatePickerWPF.BlackoutDates.Clear();
                }
                catch { /* Ignore any setup errors */ }
            }

            if (MappedType == eDataType.Value.ToString())
            {
                SetValueControlsData();
                SetValueControlsView();
            }
        }

        private void UCDataMapping_Unloaded(object sender, RoutedEventArgs e)
        {
            // Clean up event handlers to prevent memory leaks
            xValueTextBox.TextChanged -= xValueTextBox_TextChanged;
            xValueTextBox.PreviewTextInput -= xValueTextBox_PreviewTextInput;
            xSelectionListComboBox.SelectionChanged -= xSelectionListComboBox_SelectionChanged;
            xDatePickerWPF.SelectedDateChanged -= xDatePickerWPF_SelectedDateChanged;
            if (mVariablesList != null)
            {
                mVariablesList.CollectionChanged -= VariabelsSourceList_CollectionChanged;
            }
            if (mGlobalVariablesList != null)
            {
                mGlobalVariablesList.CollectionChanged -= VariabelsSourceList_CollectionChanged;
            }
            if (xDateTimePicker != null)
            {
                xDateTimePicker.TextChanged -= xDateTimePicker_TextChanged;
            }
            this.Unloaded -= UCDataMapping_Unloaded;
            this.Loaded -= UCDataMapping_Loaded;
            this.DataContextChanged -= UCDataMapping_DataContextChanged;
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
                var mContext = Context.GetAsContext(WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().FirstOrDefault());
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


            // Hide all controls first
            xVariablesComboBox.Visibility = Visibility.Hidden;
            xOptionalValuesComboBox.Visibility = Visibility.Hidden;
            xSelectionListComboBox.Visibility = Visibility.Hidden;
            xDatePickerWPF.Visibility = Visibility.Hidden;
            xDateTimeWindow.Visibility = Visibility.Hidden;
            xValueTextBox.Visibility = Visibility.Hidden;
            xDSExpressionTxtbox.Visibility = Visibility.Hidden;
            xDSConfigBtn.Visibility = Visibility.Hidden;
            xDBValueExpression.Visibility = Visibility.Hidden;

            if (MappedType == eDataType.Variable.ToString())
            {
                xVariablesComboBox.Visibility = Visibility.Visible;
            }
            else if ((MappedType == eDataType.OutputVariable.ToString() || MappedType == eDataType.GlobalVariable.ToString() || MappedType == eDataType.ApplicationModelParameter.ToString())
                && xOptionalValuesComboBox != null)
            {
                xOptionalValuesComboBox.Visibility = Visibility.Visible;
            }
            else if (MappedType == eDataType.Value.ToString())
            {
                // Handle Selection List ComboBox visibility for VariableSelectionList
                if (IsSelectionListVariable() && xSelectionListComboBox != null)
                {
                    xSelectionListComboBox.Visibility = Visibility.Visible;
                    Reporter.ToLog(eLogLevel.DEBUG, "UCDataMapping: Showing Selection List ComboBox");
                }
                // Handle DateTime picker visibility for VariableDateTime - Use WPF DatePicker
                else if (IsDateTimeVariable() && xDatePickerWPF != null)
                {
                    // TRIPLE-FORCE past dates before showing
                    try
                    {
                        xDatePickerWPF.DisplayDateStart = null;
                        xDatePickerWPF.DisplayDateEnd = null;
                        xDatePickerWPF.BlackoutDates.Clear();

                        // Force immediate refresh
                        xDatePickerWPF.InvalidateVisual();
                        xDatePickerWPF.UpdateLayout();
                    }
                    catch { /* Ignore any setup errors */ }

                    // Now show the control
                    xDatePickerWPF.Visibility = Visibility.Visible;

                    // Setup with full configuration
                    SetupWPFDatePicker();

                    // FINAL CHECK - ensure constraints are still clear
                    try
                    {
                        if (DataContext is not VariableDateTime dateTimeVar ||
                            (string.IsNullOrEmpty(dateTimeVar.MinDateTime) && string.IsNullOrEmpty(dateTimeVar.MaxDateTime)))
                        {
                            // No variable constraints = no date restrictions
                            xDatePickerWPF.DisplayDateStart = null;
                            xDatePickerWPF.DisplayDateEnd = null;
                        }
                        xDatePickerWPF.BlackoutDates.Clear();
                    }
                    catch { /* Ignore final check errors */ }

                    Reporter.ToLog(eLogLevel.DEBUG, $"UCDataMapping: DateTime picker shown with UNLIMITED past date access");
                }
                else if (xValueTextBox != null)
                {
                    xValueTextBox.Visibility = Visibility.Visible;
                    Reporter.ToLog(eLogLevel.DEBUG, $"UCDataMapping: Showing TextBox for variable type: {DataContext?.GetType().Name}");
                }
            }
            else if (MappedType == eDataType.DataSource.ToString() && xDSExpressionTxtbox != null)
            {
                xDSExpressionTxtbox.Visibility = Visibility.Visible;
                xDSConfigBtn.Visibility = Visibility.Visible;
            }
            else if (MappedType == eDataType.ValueExpression.ToString() && xDBValueExpression != null)
            {
                xDBValueExpression.Visibility = Visibility.Visible;
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

        private void UpdateDateTimePickerSelection()
        {
            if (IsDateTimeVariable())
            {
                SetupWPFDatePicker();
            }
        }


        private void MarkMappedValueValidation()
        {
            // Cache string comparisons to avoid repeated ToString() calls and magic strings
            string mappedType = MappedType;

            bool isValid = true;
            string? validationMessage = null;

            // --------- Validation checks grouped and named ----------
            bool isMappedValueEmptyForNonValueType = mappedType != eDataType.None.ToString() && mappedType != eDataType.Value.ToString() && string.IsNullOrEmpty(MappedValue);

            bool isOutputVariableMissing = mappedType == eDataType.OutputVariable.ToString() && !GingerCore.General.CheckComboItemExist(xOptionalValuesComboBox, MappedValue, nameof(VariableBase.VariableInstanceInfo));

            bool isVariableMissing = mappedType == eDataType.Variable.ToString() && !GingerCore.General.CheckComboItemExist(xVariablesComboBox, MappedValue);

            bool isGlobalOrAMP_Missing = (mappedType == eDataType.GlobalVariable.ToString() || mappedType == eDataType.ApplicationModelParameter.ToString()) && !GingerCore.General.CheckComboItemExist(xOptionalValuesComboBox, MappedValue, "Guid");

            bool isDataSourceInvalid =
                mappedType == eDataType.DataSource.ToString() && GingerCoreNET.GeneralLib.General.CheckDataSource(MappedValue, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>()) != string.Empty;

            // Combine non-Value validations
            if (isMappedValueEmptyForNonValueType || isOutputVariableMissing || isGlobalOrAMP_Missing || isDataSourceInvalid || isVariableMissing)
            {
                isValid = false;
            }
            else if (mappedType == eDataType.Value.ToString())
            {
                // Validate for Value type specifically
                var valueValidationResult = ValidateValueForVariableType();
                isValid = valueValidationResult.IsValid;
                validationMessage = valueValidationResult.ErrorMessage;
            }

            // --------- Apply UI state based on validation ----------
            ApplyValidationUI(isValid, mappedType, validationMessage);
        }

        private void ApplyValidationUI(bool isValid, string mappedType, string? validationMessage)
        {
            if (!isValid)
            {
                BorderThickness = new Thickness(1);
                BorderBrush = Brushes.Red;

                // Set tooltip only if we have a validation message
                if (!string.IsNullOrEmpty(validationMessage))
                {
                    xValueTextBox.ToolTip = validationMessage;
                }
            }
            else
            {
                BorderThickness = new Thickness(0);
                BorderBrush = null;

                // Clear tooltip when valid for Value type
                if (mappedType == eDataType.Value.ToString())
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

        private ValueValidationResult ValidateValueForVariableType()
        {
            if (string.IsNullOrEmpty(MappedValue))
            {
                return new ValueValidationResult(true); // Empty value is valid for Value type
            }

            // Check if the value contains only valid characters
            if (MappedValue.Any(c => char.IsControl(c) && c != '\t' && c != '\n' && c != '\r'))
            {
                return new ValueValidationResult(false, "Value contains invalid control characters");
            }

            // If the DataContext is a number variable, validate it as a number
            if (DataContext is VariableNumber)
            {
                return ValidateNumericValue(MappedValue);
            }

            return new ValueValidationResult(true); // Default validation passes
        }

        private ValueValidationResult ValidateNumericValue(string value)
        {
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double numericValue))
            {
                return new ValueValidationResult(false, "Invalid numeric format");
            }

            return new ValueValidationResult(true);
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

        private static void OnVariabelsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is UCDataMapping control)
            {
                control.VariabelsSourcePropertyChanged((ObservableList<string>)args.NewValue);
            }
        }


        private void VariabelsSourcePropertyChanged(ObservableList<string> variabelsSourceList)
        {
            if (variabelsSourceList != null)
            {
                mVariablesList = variabelsSourceList;
                GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.Variable);
                mVariablesList.CollectionChanged += VariabelsSourceList_CollectionChanged;
                SetValueControlsData();
            }
        }
        private void VariabelsSourceList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetValueControlsData();
        }
        private static void OnOutputVariabelsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is UCDataMapping control)
            {
                control.OutputVariabelsSourcePropertyChanged((ObservableList<VariableBase>)args.NewValue);
            }
        }

        private void OutputVariabelsSourcePropertyChanged(ObservableList<VariableBase> outputVariablesList)
        {
            if (outputVariablesList != null)
            {
                mOutputVariablesList = outputVariablesList;
                GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.OutputVariable);
                mOutputVariablesList.CollectionChanged += VariabelsSourceList_CollectionChanged;
                SetValueControlsData();
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
                SetupWPFDatePicker();
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
            bool isDateTime = false;

            try
            {
                if (DataContext is VariableDateTime)
                {
                    isDateTime = true;
                }
                // Also check by VariableType string if available
                else if (DataContext is VariableBase variable)
                {
                    isDateTime = variable.VariableType?.Equals("DateTime", StringComparison.OrdinalIgnoreCase) == true;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"UCDataMapping: Error in IsDateTimeVariable: {ex.Message}");
                isDateTime = false;
            }

            return isDateTime;
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

        private void SetupWPFDatePicker()
        {
            if (xDatePickerWPF != null)
            {
                try
                {
                    if (DataContext is VariableDateTime dateTimeVariable)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, $"UCDataMapping: Setting up WPF DatePicker for {dateTimeVariable.Name}");

                        // For runtime configuration (Value mapping), completely remove all date restrictions
                        bool isRuntimeConfiguration = MappedType == eDataType.Value.ToString();

                        if (isRuntimeConfiguration)
                        {
                            // RUNTIME CONFIGURATION: Allow ALL dates (just like the regular variable editor)
                            xDatePickerWPF.DisplayDateStart = null;
                            xDatePickerWPF.DisplayDateEnd = null;
                            xDatePickerWPF.BlackoutDates.Clear();

                            Reporter.ToLog(eLogLevel.DEBUG, $"UCDataMapping: Runtime configuration - allowing ALL dates including past dates");
                        }
                        else
                        {
                            // Non-runtime configuration: Apply the computed constraints (original behavior)
                            xDatePickerWPF.DisplayDateStart = null;
                            xDatePickerWPF.DisplayDateEnd = null;
                            xDatePickerWPF.BlackoutDates.Clear();

                            if (!string.IsNullOrEmpty(dateTimeVariable.MinDateTime))
                            {
                                if (DateTime.TryParse(dateTimeVariable.MinDateTime, out DateTime minDate))
                                {
                                    xDatePickerWPF.DisplayDateStart = minDate;
                                    Reporter.ToLog(eLogLevel.DEBUG, $"UCDataMapping: Applied computed MinDate constraint: {minDate}");
                                }
                            }

                            if (!string.IsNullOrEmpty(dateTimeVariable.MaxDateTime))
                            {
                                if (DateTime.TryParse(dateTimeVariable.MaxDateTime, out DateTime maxDate))
                                {
                                    xDatePickerWPF.DisplayDateEnd = maxDate;
                                    Reporter.ToLog(eLogLevel.DEBUG, $"UCDataMapping: Applied computed MaxDate constraint: {maxDate}");
                                }
                            }
                        }

                        // Set current value
                        DateTime? currentValue = null;

                        if (!string.IsNullOrEmpty(MappedValue) && DateTime.TryParse(MappedValue, out DateTime parsedMappedValue))
                        {
                            currentValue = parsedMappedValue;
                        }
                        else if (!string.IsNullOrEmpty(dateTimeVariable.Value) && DateTime.TryParse(dateTimeVariable.Value, out DateTime parsedVarValue))
                        {
                            currentValue = parsedVarValue;
                        }
                        else if (!string.IsNullOrEmpty(dateTimeVariable.InitialDateTime) && DateTime.TryParse(dateTimeVariable.InitialDateTime, out DateTime parsedInitialValue))
                        {
                            currentValue = parsedInitialValue;
                        }
                        else
                        {
                            currentValue = DateTime.Today;
                        }

                        // Set the selected date
                        if (currentValue.HasValue)
                        {
                            xDatePickerWPF.SelectedDate = currentValue;

                            // Update MappedValue
                            string format = !string.IsNullOrEmpty(dateTimeVariable.DateTimeFormat) ? dateTimeVariable.DateTimeFormat : "MM/dd/yyyy";
                            string formattedValue = currentValue.Value.ToString(format, System.Globalization.CultureInfo.InvariantCulture);

                            if (string.IsNullOrEmpty(MappedValue))
                            {
                                MappedValue = formattedValue;
                            }
                        }
                    }
                    else
                    {
                        // No DateTime variable context - ensure past dates are allowed
                        xDatePickerWPF.DisplayDateStart = null;
                        xDatePickerWPF.DisplayDateEnd = null;
                        xDatePickerWPF.BlackoutDates.Clear();
                        xDatePickerWPF.SelectedDate = DateTime.Today;
                    }

                    Reporter.ToLog(eLogLevel.DEBUG, $"UCDataMapping: DatePicker configured - DisplayDateStart: {xDatePickerWPF.DisplayDateStart}, DisplayDateEnd: {xDatePickerWPF.DisplayDateEnd}");
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Error setting up WPF DatePicker: {ex.Message}", ex);

                    // EMERGENCY FALLBACK - Force unrestricted dates
                    try
                    {
                        xDatePickerWPF.DisplayDateStart = null;
                        xDatePickerWPF.DisplayDateEnd = null;
                        xDatePickerWPF.BlackoutDates.Clear();
                        xDatePickerWPF.SelectedDate = DateTime.Today;
                    }
                    catch { /* Even fallback failed, ignore */ }
                }
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

        private void xSelectionListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.Items.Count > 0 && comboBox.SelectedItem != null)
            {
                string selectedValue = comboBox.SelectedItem.ToString();
                MappedValue = selectedValue;
            }
        }

        private void UpdateSelectionListComboBoxSelection()
        {
            if (IsSelectionListVariable() && !string.IsNullOrEmpty(MappedValue))
            {
                if (xSelectionListComboBox.ItemsSource != null)
                {
                    xSelectionListComboBox.SelectedItem = MappedValue;
                }
            }
        }

        private void xDateTimePicker_TextChanged(object sender, EventArgs e)
        {
            if (DataContext is VariableDateTime dateTimeVariable)
            {
                try
                {
                    // For runtime configuration (Value mapping), skip all validation - allow any date
                    bool isRuntimeConfiguration = MappedType == eDataType.Value.ToString();

                    if (!isRuntimeConfiguration)
                    {
                        // Only validate for non-runtime configuration
                        if (!dateTimeVariable.CheckDateTimeWithInRange(xDateTimePicker.Value.ToString()))
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Input Value is not in range:- Maximum date :[{dateTimeVariable.MaxDateTime}], Minimum Date:[{dateTimeVariable.MinDateTime}]");
                            return;
                        }
                    }

                    // Update the MappedValue with the formatted date
                    string format = !string.IsNullOrEmpty(dateTimeVariable.DateTimeFormat) ? dateTimeVariable.DateTimeFormat : "MM/dd/yyyy";
                    string formattedValue = xDateTimePicker.Value.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
                    MappedValue = formattedValue;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Error handling DateTimePicker text changed: {ex.Message}", ex);
                }
            }
        }

        private void xDatePickerWPF_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DataContext is VariableDateTime dateTimeVariable && xDatePickerWPF.SelectedDate.HasValue)
            {
                try
                {
                    DateTime selectedDate = xDatePickerWPF.SelectedDate.Value;

                    // For runtime configuration (Value mapping), skip all validation - allow any date
                    bool isRuntimeConfiguration = MappedType == eDataType.Value.ToString();

                    if (!isRuntimeConfiguration)
                    {
                        // Only validate for non-runtime configuration
                        if (!dateTimeVariable.CheckDateTimeWithInRange(selectedDate.ToString()))
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Input Value is not in range:- Maximum date :[{dateTimeVariable.MaxDateTime}], Minimum Date:[{dateTimeVariable.MinDateTime}]");
                            return;
                        }
                    }

                    // Update the MappedValue with the formatted date
                    string format = !string.IsNullOrEmpty(dateTimeVariable.DateTimeFormat) ? dateTimeVariable.DateTimeFormat : "MM/dd/yyyy";
                    string formattedValue = selectedDate.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
                    MappedValue = formattedValue;

                    Reporter.ToLog(eLogLevel.DEBUG, $"UCDataMapping: WPF DatePicker selected date: {selectedDate}, formatted value: {formattedValue}");
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Error handling WPF DatePicker selection changed: {ex.Message}", ex);
                }
            }
        }

        private void SetGlobalVariabelsListValues()
        {
            mGlobalVariablesList = [];
            if (WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.Variables != null && WorkSpace.Instance.Solution.Variables.Count > 0)
            {
                mGlobalVariablesList = WorkSpace.Instance.Solution.Variables;
                GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.GlobalVariable);
            }
        }

        private void SetModelGlobalParametersListValues()
        {
            mModelGlobalParamsList = [];
            if (WorkSpace.Instance.SolutionRepository != null && WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>().Any())
            {
                mModelGlobalParamsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>();
                GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.ApplicationModelParameter);
            }
        }

        private void SetDataSourceValues()
        {
            if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>().Any())
            {
                GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.DataSource);
            }
        }
        #endregion

        #region Database
        private void SetDatabaseValues()
        {
            if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Any())
            {
                GingerCore.General.EnableComboItem(xMappedTypeComboBox, eDataType.ValueExpression);
            }
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

            // CRITICAL: Bind the DataContext to the current row data (the VariableBase object)
            // This ensures the UCDataMapping control gets the actual variable object as its DataContext
            ucDataMapping.SetBinding(FrameworkElement.DataContextProperty, new Binding());

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
