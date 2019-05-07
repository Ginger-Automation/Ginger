#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.ValidationRules;
using Ginger.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ginger.UserControlsLib.ActionInputValueUserControlLib
{
    /// <summary>
    /// Interaction logic for ActionInputValueUserControl.xaml
    /// </summary>
    public partial class ActionInputValueUserControl : UserControl
    {
        ActInputValue mActInputValue;
        Context mContext;
        List<Attribute> mActionParamProperties;
        string mLabel;
        object mDefaultValue;
        public ActionInputValueUserControl(Context context, ActInputValue actInputValue, List<Attribute> actionParamProperties)
        {
            InitializeComponent();

            mActInputValue = actInputValue;
            mContext = context;
            mActionParamProperties = actionParamProperties;

            ResetControls();

            mLabel = char.ToUpper(mActInputValue.Param[0]) +  mActInputValue.Param.Substring(1);   // capitilize first letter only of the param name

            SetParamLayout();
            SetControlToInputValue();
            SetParamValidations();
        }

        private void SetParamValidations()
        {
            if (mActionParamProperties == null)
            {
                return;
            }

            foreach (Attribute attr in mActionParamProperties)
            {             
                if (attr.GetType() == typeof(MandatoryAttribute))
                {
                    xTextBoxInputPnl.Background = Brushes.Orange;
                    //TODO: add AddValidationRule in the UC - but there are 3 UCs !? !!!!!!!!!!!!!!!                  
                    xTextBoxInputTextBox.ValueTextBox.AddValidationRule(new EmptyValidationRule());
                }
                else if(attr.GetType() == typeof(MaxValueAttribute))
                {                           
                    xTextBoxInputTextBox.ValueTextBox.AddValidationRule(new MaxValueValidationRule(((MaxValueAttribute)attr).MaxValue));                    
                }
                else if(attr.GetType() == typeof(MinValueAttribute))
                {                    
                    xTextBoxInputTextBox.ValueTextBox.AddValidationRule(new MinValueValidationRule(((MinValueAttribute)attr).MinValue));                                        
                }          
                else if(attr.GetType() == typeof(MaxLengthAttribute))
                {
                    xTextBoxInputTextBox.ValueTextBox.AddValidationRule(new MaxLengthValidationRule(((MaxLengthAttribute)attr).MaxLength));
                }
                else if(attr.GetType() == typeof(MinLengthAttribute))
                {
                    xTextBoxInputTextBox.ValueTextBox.AddValidationRule(new MinLegthValidationRule(((MinLengthAttribute)attr).MinLength));
                }
                else if(attr.GetType() == typeof(InvalidValueAttribute))
                {
                    InvalidValueValidationRule IVR = (InvalidValueValidationRule)xTextBoxInputTextBox.ValueTextBox.GetValidationRule(TextBox.TextProperty, typeof(InvalidValueValidationRule));                   
                    if(IVR is null)
                    {
                        xTextBoxInputTextBox.ValueTextBox.AddValidationRule(new InvalidValueValidationRule(((InvalidValueAttribute)attr).InvalidValue));
                    }
                    else
                    {
                        IVR.InvalidValue.AddRange(((InvalidValueAttribute)attr).InvalidValue);
                    }
                }
                else if (attr.GetType() == typeof(ValidValueAttribute))
                {
                    ValidValueValidationRule VVR = (ValidValueValidationRule)xTextBoxInputTextBox.ValueTextBox.GetValidationRule(TextBox.TextProperty, typeof(ValidValueValidationRule));
                    if (VVR is null)
                    {
                        xTextBoxInputTextBox.ValueTextBox.AddValidationRule(new ValidValueValidationRule(((ValidValueAttribute)attr).ValidValue));
                    }
                    else
                    {
                        VVR.ValidValues.AddRange(((ValidValueAttribute)attr).ValidValue);
                    }
                }
            }
        }

        private void SetParamLayout()
        {                     
            // read and process param attrs like Label, Tooltip, Location
            if (mActionParamProperties == null)
            {
                return;
            }
            
            foreach (Attribute attr in mActionParamProperties)
            {                
                if (attr.GetType() == typeof(LabelAttribute))
                {
                    mLabel = ((LabelAttribute)attr).Label;   // We keep the label as is defined on the param - do not capitilize or change
                }
                else if (attr.GetType() == typeof(TooltipAttribute))
                {
                    xTextBoxInputPnl.ToolTip = ((TooltipAttribute)attr).Tooltip;
                }
                else if(attr.GetType() == typeof(DefaultAttribute))
                {
                    mDefaultValue = ((DefaultAttribute)attr).DefaultValue;
                }
                //TODO: handle layout here like Grid.Col/Row
            }

        }

        private void ResetControls()
        {
            xTextBoxInputPnl.Visibility = Visibility.Collapsed;
            xComboBoxInputPnl.Visibility = Visibility.Collapsed;
            xCheckBoxInput.Visibility = Visibility.Collapsed;
            xListInputGrid.Visibility = Visibility.Collapsed;
        }

        private void SetControlToInputValue()
        {
            if (string.IsNullOrEmpty(mActInputValue.Value) && mDefaultValue != null)
            {
                mActInputValue.Value = mDefaultValue.ToString();
            }
            // simple string or number or unknown type show text box
            if (mActInputValue.ParamType == typeof(string) || mActInputValue.ParamType == typeof(int) || mActInputValue.ParamType == null)
            {
                xTextBoxInputPnl.Visibility = Visibility.Visible;
                xTextBoxInputLabel.Content = mLabel;                                
                xTextBoxInputTextBox.Init(mContext, mActInputValue, nameof(ActInputValue.Value));
                return;
            }           

            if (mActInputValue.ParamType == typeof(bool))
            {
                xCheckBoxInput.Visibility = Visibility.Visible;
                xCheckBoxInput.Content = mLabel;               
                xCheckBoxInput.BindControl(mActInputValue, nameof(ActInputValue.Value));
                return;
            }

            if (mActInputValue.ParamType == typeof(DynamicListWrapper))
            {
                if(mActInputValue.ListDynamicValue == null && mDefaultValue!=null)
                {
                    mActInputValue.ListDynamicValue = (dynamic)mDefaultValue;
                }
                xListInputGrid.Visibility = Visibility.Visible;
                xListInputGrid.Title = mLabel;

                //set data
                ObservableList<dynamic> DynList = mActInputValue.ListDynamicValue;
                xListInputGrid.DataSourceList = DynList;

                //data changes catch
                DynList.CollectionChanged += ListCollectionChanged;
                xListInputGrid.Grid.CellEditEnding += Grid_CellEditEnding;
                xListInputGrid.btnAdd.Click += AddItem;
                SetListGridView();
                return;
            }

            if (mActInputValue.ParamType == typeof(EnumParamWrapper))
            {
                xComboBoxInputPnl.Visibility = Visibility.Visible;
                xComboBoxInputLabel.Content = string.Format("{0}:", mLabel);
                

                string enumValues = mActInputValue.ParamTypeEX.Replace("enum{", "");
                enumValues = enumValues.Replace("}", "");

                string[] avv = enumValues.Split(',');
                foreach(string v in avv)
                {
                    xComboBoxInputComboBox.Items.Add(v);
                }
                
                xComboBoxInputComboBox.BindControl(mActInputValue, nameof(ActInputValue.Value));
                return;
            }

            throw new System.Exception("unknown param type to create control: " + mActInputValue.ParamType.FullName);

        }

        //private string GetInputFieldformatedName()
        //{
            
            //// Make first letter upper case
            //string formatedName = char.ToUpper(mActInputValue.Param[0]) + mActInputValue.Param.Substring(1);

            ////split by Uper case
            //string[] split = Regex.Split(formatedName, @"(?<!^)(?=[A-Z])");
            //formatedName = string.Empty;
            //foreach (string str in split)
            //{
            //    formatedName += str + " ";
            //}

            //return (formatedName.Trim());
        //}

        #region List Grid Handlers

        void SetListGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;

            // Create grid columns based on list item properties
            List<string> props = mActInputValue.GetListItemProperties();
            foreach (string prop in props)
            {
                viewCols.Add(new GridColView() { Field = prop, WidthWeight = 10 });
                viewCols.Add(new GridColView() { Field = prop + "VE", Header = "...", WidthWeight = 1, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PagePanel.Resources["ValueExpressionButton"] });
            }

            xListInputGrid.SetAllColumnsDefaultView(view);
            xListInputGrid.InitViewItems();
        }

        private void GridVEButton_Click(object sender, RoutedEventArgs e)
        {
            dynamic currentListItem = (dynamic)xListInputGrid.CurrentItem;
            //get name of relevent field
            int currentColIndex = xListInputGrid.Grid.CurrentColumn.DisplayIndex;
            object field = xListInputGrid.Grid.Columns[currentColIndex - 1].Header;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(currentListItem, field.ToString(), mContext);
            VEEW.ShowAsWindow();
            UpdateListValues();
        }

        private void Grid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            UpdateListValues();
        }

        private void ListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateListValues();
        }

        private void UpdateListValues()
        {
            ObservableList<dynamic> list = (ObservableList<dynamic>)xListInputGrid.DataSourceList;
            mActInputValue.ListDynamicValue = list;
        }

        private void AddItem(object sender, RoutedEventArgs e)
        {
            dynamic expando = new ExpandoObject();
            // TODO set obj with item default value - expando.Name = "";
            ((ObservableList<dynamic>)xListInputGrid.DataSourceList).Add(expando);
        }
        #endregion


    }
}
