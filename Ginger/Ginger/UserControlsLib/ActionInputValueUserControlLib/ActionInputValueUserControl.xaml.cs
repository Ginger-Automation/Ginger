#region License
/*
Copyright © 2014-2023 European Support Limited

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
using System.Text.RegularExpressions;
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
        bool mBrowseNeeded = false;
        Environment.SpecialFolder mFolderType = Environment.SpecialFolder.MyComputer;
        Ginger.Activities.UCValueExpression.eBrowserType mBrowseType = Ginger.Activities.UCValueExpression.eBrowserType.File;
        string mFileType = string.Empty;
        public ActionInputValueUserControl(Context context, ActInputValue actInputValue, List<Attribute> actionParamProperties)
        {
            InitializeComponent();

            mActInputValue = actInputValue;
            mContext = context;
            mActionParamProperties = actionParamProperties;

            ResetControls();
            
            mLabel = Regex.Replace(Regex.Replace(Regex.Replace(mActInputValue.Param, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2"), @"(\P{Ll}\p{Ll})", m => m.ToString().ToLower());
            mLabel = char.ToUpper(mLabel[0]) + mLabel.Substring(1);
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
                    InvalidValueValidationRule invalidValueValidationRule = (InvalidValueValidationRule)xTextBoxInputTextBox.ValueTextBox.GetValidationRule(TextBox.TextProperty, typeof(InvalidValueValidationRule));                   
                    if(invalidValueValidationRule is null)
                    {
                        xTextBoxInputTextBox.ValueTextBox.AddValidationRule(new InvalidValueValidationRule(((InvalidValueAttribute)attr).InvalidValue));
                    }
                    else
                    {
                        invalidValueValidationRule.InvalidValue.AddRange(((InvalidValueAttribute)attr).InvalidValue);
                    }
                }
                //TODO: implement valid value attribute validation
                //else if (attr.GetType() == typeof(ValidValueAttribute))
                //{
                //    ValidValueValidationRule validValueValidationRule = (ValidValueValidationRule)xTextBoxInputTextBox.ValueTextBox.GetValidationRule(TextBox.TextProperty, typeof(ValidValueValidationRule));
                //    if (validValueValidationRule is null)
                //    {
                //        xTextBoxInputTextBox.ValueTextBox.AddValidationRule(new ValidValueValidationRule(((ValidValueAttribute)attr).ValidValue));
                //    }
                //    else
                //    {
                //        validValueValidationRule.ValidValues.AddRange(((ValidValueAttribute)attr).ValidValue);
                //    }
                //}                
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
                    mLabel = ((LabelAttribute)attr).Label;
                }
                else if (attr.GetType() == typeof(TooltipAttribute))
                {
                    xTextBoxInputPnl.ToolTip = ((TooltipAttribute)attr).Tooltip;
                }
                else if (attr.GetType() == typeof(DefaultAttribute))
                {
                    mDefaultValue = ((DefaultAttribute)attr).DefaultValue;
                }
                else if (attr.GetType() == typeof(BrowseAttribute))
                {
                    mBrowseNeeded = ((BrowseAttribute)attr).IsNeeded;
                }
                else if (attr.GetType() == typeof(BrowseTypeAttribute))
                {
                    if (((BrowseTypeAttribute)attr).BrowseType.ToString() == Ginger.Activities.UCValueExpression.eBrowserType.File.ToString())
                    {
                        mBrowseType = Ginger.Activities.UCValueExpression.eBrowserType.File;
                    }
                    else
                    {
                        mBrowseType = Ginger.Activities.UCValueExpression.eBrowserType.Folder;
                    }
                }
                else if (attr.GetType() == typeof(FileTypeAttribute))
                {
                    mFileType = ((FileTypeAttribute)attr).FileType;
                }
                else if (attr.GetType() == typeof(FolderTypeAttribute))
                {
                    mFolderType = ((FolderTypeAttribute)attr).FolderType;
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
                xTextBoxInputTextBox.Init(mContext, mActInputValue, nameof(ActInputValue.Value),isBrowseNeeded : mBrowseNeeded, browserType : mBrowseType, fileType : mFileType, rootFolder: mFolderType);
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

            throw new System.Exception("unknown parameter type to create control: " + mActInputValue.ParamType.FullName);

        }        

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
            //get name of relevant field
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
