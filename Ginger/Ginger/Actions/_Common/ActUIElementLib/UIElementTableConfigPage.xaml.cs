#region License
/*
Copyright © 2014-2018 European Support Limited

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Helpers;
using GingerCore;
using GingerCore.Platforms.PlatformsInfo;
using GingerCore.Drivers.Common;
using Amdocs.Ginger.Common.UIElement;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for TableActionConfigPage.xaml
    /// </summary>
    public partial class UIElementTableConfigPage : Page
    {
        public ActUIElement mAct;
        int mRowCount = 0;
        List<String> mColNames = null;      
        PlatformInfoBase mPlatform;
        ElementInfo mElementInfo;
        ObservableList<Act> mActions = null;
        ObservableList<Act> mOriginalActions = null;       
        BaseWindow eBaseWindow;
        private enum BaseWindow
        {
            WindowExplorer,
            ActEditPage
        }

        List<GingerCore.General.ComboItem> operationTypeList;
       
        public UIElementTableConfigPage(ActUIElement Act, PlatformInfoBase Platform)
        {
            eBaseWindow = BaseWindow.ActEditPage;
            mAct = Act;                   
            mPlatform = Platform;
          

            InitializeComponent();
            ShowTableControlActionConfigPage(mPlatform);           
        }

        public UIElementTableConfigPage(ElementInfo ElementInfo, ObservableList<Act> Actions, ActUIElement Act = null)
        {
            eBaseWindow = BaseWindow.WindowExplorer;
            mAct = new ActUIElement();
            string targetApp = App.BusinessFlow.CurrentActivity.TargetApplication;
            mPlatform = PlatformInfoBase.GetPlatformImpl((from x in App.UserProfile.Solution.ApplicationPlatforms where x.AppName == targetApp select x.Platform).FirstOrDefault());

            if (ElementInfo.ElementType.Contains("JEditor"))
            {
                mAct.ElementType = eElementType.EditorPane;
                mAct.ElementAction = ActUIElement.eElementAction.JEditorPaneElementAction;
                mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.SubElementType, ActUIElement.eSubElementType.HTMLTable.ToString());
                mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.SubElementAction, ActUIElement.eElementAction.TableCellAction.ToString());
            }
            else
            {
                mAct.ElementType = eElementType.Table;
                mAct.ElementAction = ActUIElement.eElementAction.TableCellAction;
            }                 
            
            mElementInfo = ElementInfo;
            mActions = Actions;
            ShowCellActions();
            InitializeComponent();
            InitTableInfo();

            ShowTableControlActionConfigPage(mPlatform);
            SetComponents();
            SetDescriptionDetails();
        }

        public void TableActionFieldBinding()
        {
            RowSelectorPanelInit();
            WhereColumn.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.WhereColSelector,mAct.GetInputParamValue(ActUIElement.Fields.WhereColSelector)), typeof(ActUIElement.eTableElementRunColSelectorValue), isVENeeded: false, UCselectionChange: WhereColumn_SelectionChanged);
            WhereColumnTitle.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.WhereColumnTitle, mAct.GetInputParamValue(ActUIElement.Fields.WhereColumnTitle)), isVENeeded: true, UCselectionChange: WhereColumnTitle_SelectionChanged);
            WhereProperty.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.WhereProperty, mAct.GetInputParamValue(ActUIElement.Fields.WhereProperty)), typeof(ActUIElement.eTableElementRunColPropertyValue), isVENeeded: false, UCselectionChange: WhereProperty_SelectionChanged);
            WhereOperator.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.WhereOperator, mAct.GetInputParamValue(ActUIElement.Fields.WhereOperator)), typeof(ActUIElement.eTableElementRunColOperator), isVENeeded: false, UCselectionChange: WhereOperator_SelectionChanged);
            WhereColumnValue.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.WhereColumnValue, mAct.GetInputParamValue(ActUIElement.Fields.WhereColumnValue)));
            
            RowSelectorValue.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.LocateRowValue), isVENeeded: true, UCselectionChange: RowSelectorValue_SelectionChanged);          
        }
        
        private void InitTableInfo()
        {
            object o = mElementInfo.GetElementData();
            if (o != null)
            {
                mColNames = ((TableElementInfo)o).ColumnNames;
                mRowCount = ((TableElementInfo)o).RowCount;
            }
            else
            {
                mColNames = new List<string>();
                mColNames.Add("0");
                mRowCount = 0;
            }
        }

        private void SetComponents()
        {
            ControlActionComboBox.Visibility = Visibility.Collapsed;
            ControlActionComboBox.Visibility = Visibility.Collapsed;
            subElementTypeRow.Height = new GridLength(0);
            OperationTypeRow.Height = new GridLength(0);

            mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.ColSelectorValue, ActUIElement.eTableElementRunColSelectorValue.ColTitle.ToString());

            RowNum.IsChecked = true;
            for (int i = 0; i < mRowCount; i++)
            {
                RowSelectorValue.ComboBox.Items.Add(i.ToString());
            }
            RowSelectorValue.ComboBox.SelectedIndex = 0;
            mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereColSelector, ActUIElement.eTableElementRunColSelectorValue.ColTitle.ToString());
            mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereProperty, ActUIElement.eTableElementRunColPropertyValue.Value.ToString());
            mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereOperator, ActUIElement.eTableElementRunColOperator.Equals.ToString());            
        }


        private void SubElementActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SubElementActionComboBox.ComboBox.SelectedItem != null)
            {
                operationTypeList.Clear();

                ActUIElement.eElementAction selectedSubElementAction;

                if (Enum.TryParse(((GingerCore.General.ComboItem)SubElementActionComboBox.ComboBox.SelectedItem).Value.ToString(), out selectedSubElementAction))
                {
                    operationTypeList = mPlatform.GetTableControlActions(selectedSubElementAction).Select(x => new GingerCore.General.ComboItem() { Value = x.ToString(), text = x.ToString() }).ToList();
                    ControlActionComboBox.UpdateComboItems(operationTypeList);

                    if (selectedSubElementAction == ActUIElement.eElementAction.TableCellAction)
                    {
                        RowLabelPanel.Visibility = Visibility.Visible;
                        RowSelectorPanel.Visibility = Visibility.Visible;
                        RowSelectorValuePanel.Visibility = Visibility.Visible;
                        WhereColumnValue.Visibility = Visibility.Visible;
                        ColumnLabelPanel.Visibility = Visibility.Visible;
                        cmbColSelectorValuePanel.Visibility = Visibility.Visible;
                        cmbColumnValuePanel.Visibility = Visibility.Visible;
                       // cmbColSelectorValue.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.ColSelectorValue), typeof(ActUIElement.eTableElementRunColSelectorValue), isVENeeded: false, UCselectionChange: ColSelectorValue_SelectionChanged);
                       // cmbColumnValue.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.LocateColTitle), isVENeeded: true, UCselectionChange: ColumnValue_SelectionChanged);
                        //TableActionFieldBinding();
                    }
                    else if (selectedSubElementAction == ActUIElement.eElementAction.TableRowAction)
                    {
                        RowLabelPanel.Visibility = Visibility.Collapsed;
                        RowSelectorPanel.Visibility = Visibility.Collapsed;
                        RowSelectorValuePanel.Visibility = Visibility.Collapsed;
                        ColumnLabelPanel.Visibility = Visibility.Collapsed;
                        cmbColumnValuePanel.Visibility = Visibility.Collapsed;
                        cmbColSelectorValuePanel.Visibility = Visibility.Collapsed;
                        WherePanel.Visibility = Visibility.Collapsed;
                    }
                    else if (selectedSubElementAction == ActUIElement.eElementAction.TableAction)
                    {
                        RowLabelPanel.Visibility = Visibility.Collapsed;
                        RowSelectorPanel.Visibility = Visibility.Collapsed;
                        RowSelectorValuePanel.Visibility = Visibility.Collapsed;
                        ColumnLabelPanel.Visibility = Visibility.Collapsed;
                        cmbColumnValuePanel.Visibility = Visibility.Collapsed;
                        cmbColSelectorValuePanel.Visibility = Visibility.Collapsed;
                        WherePanel.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        public void RowSelectorPanelInit()
        {
            string currentValue = mAct.GetInputParamValue(ActUIElement.Fields.RowSelectorRadioParam);
            foreach (RadioButton rdb in RowSelectorPanel.Children)
                if (rdb.Tag.ToString() == currentValue)
                {
                    rdb.IsChecked = true;
                    break;
                }
        }

        public void ShowTableControlActionConfigPage(PlatformInfoBase mPlatform)
        {
            if (mAct.ElementAction == ActUIElement.eElementAction.JEditorPaneElementAction)
            {
                SubElementTypeComboBox.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.SubElementType),
                mPlatform.GetSubElementType(mAct.ElementType).ToList(), isVENeeded: false);

                operationTypeList = new List<GingerCore.General.ComboItem>();
                ActUIElement.eSubElementType elementType;
                if (Enum.TryParse(mAct.GetInputParamCalculatedValue(ActUIElement.Fields.SubElementType), out elementType))
                {
                }

                SubElementActionComboBox.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.SubElementAction),
                mPlatform.GetSubElementAction(elementType).ToList(), isVENeeded: false,
                UCselectionChange: SubElementActionComboBox_SelectionChanged);
                SubElementDetailsPanel.Visibility = Visibility.Visible;
                ActUIElement.eElementAction selectedSubElementAction;

                if (Enum.TryParse(mAct.GetInputParamCalculatedValue(ActUIElement.Fields.SubElementAction), out selectedSubElementAction))
                {
                    operationTypeList = mPlatform.GetTableControlActions(selectedSubElementAction).Select(x => new GingerCore.General.ComboItem() { Value = x.ToString(), text = x.ToString() }).ToList();
                }
            }
            else
            {
                operationTypeList = mPlatform.GetTableControlActions(mAct.ElementAction).Select(x => new GingerCore.General.ComboItem() { Value = x.ToString(), text = x.ToString() }).ToList();
            }
            ControlActionComboBox.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.ControlAction), operationTypeList, isVENeeded: true, UCselectionChange: ControlActionComboBox_SelectionChanged);

            if (mAct.ElementAction == ActUIElement.eElementAction.TableRowAction || mAct.GetInputParamValue(ActUIElement.Fields.SubElementAction) == ActUIElement.eElementAction.TableRowAction.ToString())
            {
                RowLabelPanel.Visibility = Visibility.Visible;
                RowSelectorPanel.Visibility = Visibility.Visible;
                RowSelectorValuePanel.Visibility = Visibility.Visible;
                WhereColumnValue.Visibility = Visibility.Visible;
                TableActionFieldBinding();
            }
            if (mAct.ElementAction == ActUIElement.eElementAction.TableCellAction|| mAct.GetInputParamValue(ActUIElement.Fields.SubElementAction) == ActUIElement.eElementAction.TableCellAction.ToString())
            {
                RowLabelPanel.Visibility = Visibility.Visible;
                RowSelectorPanel.Visibility = Visibility.Visible;
                RowSelectorValuePanel.Visibility = Visibility.Visible;
                WhereColumnValue.Visibility = Visibility.Visible;
                ColumnLabelPanel.Visibility = Visibility.Visible;
                cmbColSelectorValuePanel.Visibility = Visibility.Visible;
                cmbColumnValuePanel.Visibility = Visibility.Visible;
                cmbColSelectorValue.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.ColSelectorValue), typeof(ActUIElement.eTableElementRunColSelectorValue), isVENeeded: false, UCselectionChange: ColSelectorValue_SelectionChanged);
                cmbColumnValue.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.LocateColTitle), isVENeeded: true, UCselectionChange: ColumnValue_SelectionChanged);
                TableActionFieldBinding();
            }
        }

        private void SetDescriptionDetails()
        {
            try
            {
                txtDescription.Text = string.Empty;
                TextBlockHelper TBH = new TextBlockHelper(txtDescription);

                TBH.AddText("Select the grid cell located by ");
                TBH.AddUnderLineText(cmbColSelectorValue.ComboBox.SelectedItem.ToString());
                TBH.AddText(" ");
                if (cmbColumnValue.ComboBox.SelectedIndex != -1)
                    TBH.AddBoldText(cmbColumnValue.ComboBox.SelectedItem.ToString());
                else
                    TBH.AddBoldText(cmbColumnValue.ComboBox.Text);
                TBH.AddText(" and ");
                if (RowNum.IsChecked == true)
                {
                    TBH.AddUnderLineText(" row number ");
                    if (RowSelectorValue.ComboBox.SelectedIndex != -1)
                        TBH.AddBoldText(RowSelectorValue.ComboBox.SelectedItem.ToString());
                    else
                        TBH.AddBoldText(RowSelectorValue.ComboBox.Text);
                }
                else if (AnyRow.IsChecked == true)
                {
                    TBH.AddUnderLineText(" random ");
                    TBH.AddText(" row number");
                }
                else if (Where.IsChecked == true)
                {
                    TBH.AddText(" the row located by a cell in ");
                    TBH.AddUnderLineText(WhereColumn.ComboBox.SelectedItem.ToString());
                    TBH.AddText(" ");
                    if (WhereColumnTitle.ComboBox.SelectedIndex != -1)
                        TBH.AddBoldText(WhereColumnTitle.ComboBox.SelectedItem.ToString());
                    else
                        TBH.AddBoldText(WhereColumnTitle.ComboBox.Text);
                    TBH.AddText(" having control property ");
                    TBH.AddUnderLineText(WhereProperty.ComboBox.SelectedItem.ToString());
                    TBH.AddText(" ");
                    TBH.AddUnderLineText(WhereOperator.ComboBox.SelectedItem.ToString());
                    TBH.AddText(" ");
                    TBH.AddBoldText(WhereColumnValue.ValueTextBox.Text);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed", ex);
            }
        }

        private void WhereColumnTitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRelatedActions();
            SetDescriptionDetails();
        }
        private void WhereColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mColNames != null)
            {
                WhereColumnTitle.ComboBox.Items.Clear();
                for (int i = 0; i < mColNames.Count; i++)
                {
                    ActTableElement.eRunColSelectorValue WhereColumnTitleSelectedValue;
                    Enum.TryParse(((GingerCore.General.ComboEnumItem)WhereColumn.ComboBox.SelectedItem).Value.ToString(), out WhereColumnTitleSelectedValue);

                    if (WhereColumnTitleSelectedValue == ActTableElement.eRunColSelectorValue.ColTitle)
                        WhereColumnTitle.ComboBox.Items.Add(mColNames[i].ToString());
                    else if (WhereColumnTitleSelectedValue == ActTableElement.eRunColSelectorValue.ColNum)
                        WhereColumnTitle.ComboBox.Items.Add(i.ToString());
                    else
                    {
                        // if col name do thing for now
                    }
                }
                WhereColumnTitle.ComboBox.SelectedIndex = 0;
                
            }
           
                SetDescriptionDetails();
        }

        private void ColSelectorValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (eBaseWindow.Equals(BaseWindow.WindowExplorer))
            {
                cmbColumnValue.ComboBox.Items.Clear();
                for (int i = 0; i < mColNames.Count; i++)
                {
                    ActTableElement.eRunColSelectorValue colSelectorSelectedValue;
                    Enum.TryParse(((GingerCore.General.ComboEnumItem)cmbColSelectorValue.ComboBox.SelectedItem).Value.ToString(), out colSelectorSelectedValue);

                    if (colSelectorSelectedValue == ActTableElement.eRunColSelectorValue.ColTitle)
                        cmbColumnValue.ComboBox.Items.Add(mColNames[i].ToString());
                    else if (colSelectorSelectedValue == ActTableElement.eRunColSelectorValue.ColNum)
                        cmbColumnValue.ComboBox.Items.Add(i.ToString());
                    else
                    {
                        // if col name do thing for now
                    }
                }
                cmbColumnValue.ComboBox.SelectedIndex = 0;
            }
            SetDescriptionDetails();
        }

        private void ColumnValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (eBaseWindow.Equals(BaseWindow.WindowExplorer))
            {
                UpdateRelatedActions();
                SetDescriptionDetails();
            }
        }

        private void RowNum_Checked(object sender, RoutedEventArgs e)
        {
            if (RowSelectorValue != null)
                RowSelectorValue.IsEnabled = true;

            if (WherePanel != null)
            {
                WherePanel.Visibility = Visibility.Collapsed;
                WhereDataRow.Height = new GridLength(0);
            }
            mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.RowSelectorRadioParam, (((RadioButton)sender).Tag).ToString());
            UpdateRelatedActions();
            SetDescriptionDetails();
        }

        private void RowNum_Unchecked(object sender, RoutedEventArgs e)
        {
            RowSelectorValue.IsEnabled = false;
        }

        private void BySelectedRow_Checked(object sender, RoutedEventArgs e)
        {
            if (RowSelectorValue != null)
                RowSelectorValue.IsEnabled = true;

            if (WherePanel != null)
            {
                WherePanel.Visibility = Visibility.Collapsed;
                WhereDataRow.Height = new GridLength(0);
            }
            mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.RowSelectorRadioParam, (((RadioButton)sender).Tag).ToString());
            UpdateRelatedActions();
            SetDescriptionDetails();
        }

        private void AnyRow_Checked(object sender, RoutedEventArgs e)
        {
            RowSelectorValue.IsEnabled = false;
            WherePanel.Visibility = Visibility.Collapsed;
            mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.RowSelectorRadioParam, (((RadioButton)sender).Tag).ToString());
            WhereDataRow.Height = new GridLength(0);
            UpdateRelatedActions();
            SetDescriptionDetails();
        }

        private void Where_Checked(object sender, RoutedEventArgs e)
        {
            RowSelectorValue.IsEnabled = false;
            WherePanel.Visibility = Visibility.Visible;
            mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.RowSelectorRadioParam, (((RadioButton)sender).Tag).ToString());
            WhereDataRow.Height = new GridLength(100);       
                UpdateRelatedActions();
                SetDescriptionDetails();           
        }

        private void Where_Unchecked(object sender, RoutedEventArgs e)
        {
            WherePanel.Visibility = Visibility.Collapsed;
        }

        private void RowSelectorValueVE_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, ActTableElement.Fields.LocateRowValue);
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            RowSelectorValue.ComboBox.Text = mAct.GetOrCreateInputParam(ActUIElement.Fields.LocateRowValue).ToString();
        }

        private void RowSelectorValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowValue, RowSelectorValue.ComboBox.Text);
            App.AutomateTabGingerRunner.ProcessInputValueForDriver(mAct);
            SetDescriptionDetails();
            if (eBaseWindow.Equals(BaseWindow.WindowExplorer))
            {
                UpdateRelatedActions();
            }
        }

        private void WhereProperty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRelatedActions();
            SetDescriptionDetails();
        }

        private void WhereOperator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRelatedActions();
            SetDescriptionDetails();
        }

        private void ControlActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {           
            UpdateRelatedActions();
            SetDescriptionDetails();          

            Page setControlActionValuePage = GetControlActionValue();
            if (setControlActionValuePage == null)
            {
                ControlActionFrame.Content = null;
                ControlActionFrame.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                ControlActionFrame.Content = setControlActionValuePage;
                ControlActionFrame.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private Page GetControlActionValue()
        {
            Page getControlActionValuePage = new Page();
            StackPanel controlActionPanel = new StackPanel();
            if (ControlActionComboBox.ComboBox.SelectedItem != null && ControlActionComboBox.ComboBox.SelectedValue != null)
            {
                if (ControlActionComboBox.ComboBox.SelectedValue.ToString() == ActUIElement.eElementAction.SetValue.ToString() || ControlActionComboBox.ComboBox.SelectedValue.ToString() == ActUIElement.eElementAction.SetText.ToString() ||
                    ControlActionComboBox.ComboBox.SelectedValue.ToString() == ActUIElement.eElementAction.SendKeyPressRelease.ToString())
                {
                    Ginger.Actions.UCValueExpression textboxControlAction = new Ginger.Actions.UCValueExpression()
                    {
                        Name = "TableControlActionValue",
                        Width = 180
                    };

                    textboxControlAction.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.ControlActionValue), isVENeeded: true);
                    controlActionPanel.Children.Add(textboxControlAction);
                    getControlActionValuePage.Content = controlActionPanel;
                }
            }
            return getControlActionValuePage;
        }        

        void ShowCellActions()
        {
            mOriginalActions = new ObservableList<Act>();
            foreach (Act a in mActions)
            {
                mOriginalActions.Add(a);
            }
        }

        private void UpdateRelatedActions()
        {
           string previousSelectedControlAction = null;
            if (mActions != null)
            {
                if (mActions.CurrentItem != null)
                    if (mActions.CurrentItem is ActUIElement)
                        previousSelectedControlAction = ((ActUIElement)mActions.CurrentItem).GetOrCreateInputParam(ActUIElement.Fields.ControlAction).ToString();

                mActions.Clear();
            }

            if (cmbColSelectorValue.ComboBox.SelectedIndex != -1)

                mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.ColSelectorValue, ((GingerCore.General.ComboEnumItem)cmbColSelectorValue.ComboBox.SelectedItem).Value.ToString());

            string description = "";
            string rowVal = "";
            string colVal = "";

            if (RowNum.IsChecked == true)
            {
                mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowType, "Row Number");
                if (RowSelectorValue != null)
                {
                    if (RowSelectorValue.ComboBox.SelectedIndex != -1)
                        rowVal = RowSelectorValue.ComboBox.SelectedItem.ToString();
                    else
                        rowVal = RowSelectorValue.ComboBox.Text;
                    description = " on Row:" + rowVal;
                }
            }
            else if (AnyRow.IsChecked == true)
            {
                mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowType, "Any Row");
                description = " on Random Row";
            }
            else if (BySelectedRow.IsChecked == true)
            {
                mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowType, "By Selected Row");
                description = " on Selected Row";
            }
            else if (Where.IsChecked == true)
            {
                mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowType, "Where");
                description = " on Row with condition";
            }

            if (cmbColumnValue != null)
            {
                if (cmbColumnValue.ComboBox.SelectedIndex != -1)
                    colVal = cmbColumnValue.ComboBox.SelectedItem.ToString();
                else
                    colVal = cmbColumnValue.ComboBox.Text;
                description = description + " and Column:" + colVal;
            }

            if (eBaseWindow.Equals(BaseWindow.WindowExplorer) && cmbColSelectorValue.ComboBox.SelectedIndex != -1 && WhereColumn != null && WhereColumn.ComboBox.SelectedIndex != -1 && RowSelectorValue != null
                && WhereProperty != null && WhereProperty.ComboBox.SelectedIndex != -1 && WhereOperator != null && WhereOperator.ComboBox.SelectedIndex != -1)
            {
                List<string> descriptionString = new List<string>();
                descriptionString.Add("Get Value of Cell: ");
                descriptionString.Add("Set Value of Cell: ");
                descriptionString.Add("Type Value in Cell: ");
                descriptionString.Add("Click Cell: ");
                descriptionString.Add("WinClick Cell: ");
                descriptionString.Add("Get Selected Cell from Column: ");
                
                ActUIElement actObj = (ActUIElement)mAct.CreateCopy();
                actObj.AddOrUpdateInputParamValue(ActUIElement.Fields.ControlAction, ActUIElement.eElementAction.SetValue.ToString());
                actObj.Description = "Set Value of Cell: " + description;
                actObj.GetOrCreateInputParam(ActUIElement.Fields.WhereColSelector, mAct.GetInputParamValue(ActUIElement.Fields.WhereColSelector));
                actObj.GetOrCreateInputParam(ActUIElement.Fields.WhereColumnTitle, mAct.GetInputParamValue(ActUIElement.Fields.WhereColumnTitle));
                actObj.GetOrCreateInputParam(ActUIElement.Fields.WhereColumnValue, mAct.GetInputParamValue(ActUIElement.Fields.WhereColumnValue));
                actObj.GetOrCreateInputParam(ActUIElement.Fields.WhereOperator, mAct.GetInputParamValue(ActUIElement.Fields.WhereOperator));
                actObj.GetOrCreateInputParam(ActUIElement.Fields.WhereProperty, mAct.GetInputParamValue(ActUIElement.Fields.WhereProperty));
                mActions.Add(actObj);


                ActUIElement actObj1 = (ActUIElement)mAct.CreateCopy(); ;
                actObj1.AddOrUpdateInputParamValue(ActUIElement.Fields.ControlAction, ActUIElement.eElementAction.GetValue.ToString());
                actObj1.Description = "Get Value of Cell: " + description;
                actObj1.GetOrCreateInputParam(ActUIElement.Fields.WhereColSelector, mAct.GetInputParamValue(ActUIElement.Fields.WhereColSelector));
                actObj1.GetOrCreateInputParam(ActUIElement.Fields.WhereColumnTitle, mAct.GetInputParamValue(ActUIElement.Fields.WhereColumnTitle));
                actObj1.GetOrCreateInputParam(ActUIElement.Fields.WhereColumnValue, mAct.GetInputParamValue(ActUIElement.Fields.WhereColumnValue));
                actObj1.GetOrCreateInputParam(ActUIElement.Fields.WhereOperator, mAct.GetInputParamValue(ActUIElement.Fields.WhereOperator));
                actObj1.GetOrCreateInputParam(ActUIElement.Fields.WhereProperty, mAct.GetInputParamValue(ActUIElement.Fields.WhereProperty));
                mActions.Add(actObj1);


                ActUIElement actObj2 = (ActUIElement)mAct.CreateCopy(); ;
                actObj2.AddOrUpdateInputParamValue(ActUIElement.Fields.ControlAction, ActUIElement.eElementAction.Click.ToString());
                actObj2.Description = "Click Cell: " + description;
                actObj2.GetOrCreateInputParam(ActUIElement.Fields.WhereColSelector, mAct.GetInputParamValue(ActUIElement.Fields.WhereColSelector));
                actObj2.GetOrCreateInputParam(ActUIElement.Fields.WhereColumnTitle, mAct.GetInputParamValue(ActUIElement.Fields.WhereColumnTitle));
                actObj2.GetOrCreateInputParam(ActUIElement.Fields.WhereColumnValue, mAct.GetInputParamValue(ActUIElement.Fields.WhereColumnValue));
                actObj2.GetOrCreateInputParam(ActUIElement.Fields.WhereOperator, mAct.GetInputParamValue(ActUIElement.Fields.WhereOperator));
                actObj2.GetOrCreateInputParam(ActUIElement.Fields.WhereProperty, mAct.GetInputParamValue(ActUIElement.Fields.WhereProperty));
                mActions.Add(actObj2);

            }
            if (previousSelectedControlAction != null)
            {
                if (mActions != null)
                    foreach (ActUIElement act in mActions)
                        if (act.GetOrCreateInputParam(ActUIElement.Fields.ControlAction).ToString() == previousSelectedControlAction)
                        {
                            mActions.CurrentItem = act;
                            break;
                        }
            }
            if (mActions != null && mActions.CurrentItem == null && mActions.Count > 0)
                mActions.CurrentItem = mActions[0];
        }

        private void RestoreOriginalActions()
        {
            if (mActions != null && mOriginalActions != null)
            {
                foreach (Act a in mOriginalActions)
                {
                    mActions.Add(a);
                }
            }
        }             

        public ActUIElement CreateActUIElementObject(String descriptionString, String DescriptionValue, String rowVal, String colVal, String LocateRowType)
        {
            ActUIElement a = (ActUIElement)mAct.CreateCopy();

            a.Description = descriptionString + DescriptionValue;
            a.AddOrUpdateInputParamValue(ActUIElement.Fields.ControlAction, mAct.GetInputParamValue(ActUIElement.Fields.ControlAction));
            a.AddOrUpdateInputParamValue(ActUIElement.Fields.ColSelectorValue, ColSelectorValue.ToString());
            a.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateColTitle, colVal.ToString());
            a.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowType, LocateRowType);
            a.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowValue, rowVal.ToString());
            a.AddOrUpdateInputParamValue(ActUIElement.Fields.ByRowNum, RowNum.IsChecked.ToString());
            a.AddOrUpdateInputParamValue(ActUIElement.Fields.ByRandRow, AnyRow.IsChecked.ToString());
            a.AddOrUpdateInputParamValue(ActUIElement.Fields.BySelectedRow, BySelectedRow.IsChecked.ToString());
            a.AddOrUpdateInputParamValue(ActUIElement.Fields.ByWhere, Where.IsChecked.ToString());
            a.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereColSelector, mAct.GetInputParamValue(ActUIElement.Fields.WhereColSelector));
            a.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereColumnTitle, mAct.GetInputParamValue(ActUIElement.Fields.WhereColumnTitle));
            a.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereColumnValue, mAct.GetInputParamValue(ActUIElement.Fields.WhereColumnValue));
            a.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereOperator, mAct.GetInputParamValue(ActUIElement.Fields.WhereOperator));
            a.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereProperty, mAct.GetInputParamValue(ActUIElement.Fields.WhereProperty));

            return a;
        }

        public ActUIElement.eTableElementRunColOperator WhereOperator_Value
        {
            get
            {
                ActUIElement.eTableElementRunColOperator eVal = ActUIElement. eTableElementRunColOperator.Contains;//default value 
                if (Enum.TryParse<ActUIElement.eTableElementRunColOperator>(mAct.GetInputParamValue(ActUIElement.Fields.WhereOperator), out eVal))
                    return eVal;
                else
                    return eVal;           
            }
            set
            {
                mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereOperator, value.ToString());
            }
        }

        public ActUIElement.eTableElementRunColPropertyValue WhereProperty_Value
        {
            get
            {
                ActUIElement.eTableElementRunColPropertyValue eVal = ActUIElement.eTableElementRunColPropertyValue.Value;//default value 
                if (Enum.TryParse<ActUIElement.eTableElementRunColPropertyValue>(mAct.GetInputParamValue(ActUIElement.Fields.WhereProperty), out eVal))
                    return eVal;
                else
                    return eVal;
            }
            set
            {
                mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereProperty, value.ToString());
            }
        }

        public ActUIElement.eTableElementRunActionOn RunActionOn
        {
            get
            {
                ActUIElement.eTableElementRunActionOn eVal = ActUIElement.eTableElementRunActionOn.OnCellRowNumColName;//default value 
                if (Enum.TryParse<ActUIElement.eTableElementRunActionOn>(mAct.GetInputParamValue(ActUIElement.Fields.RunActionOn), out eVal))
                    return eVal;
                else
                    return eVal;
            }
            set
            {
                mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.RunActionOn, value.ToString());
            }
        }

        public ActUIElement.eTableElementRunColSelectorValue ColSelectorValue
        {
            get
            {
                ActUIElement.eTableElementRunColSelectorValue eVal = ActUIElement.eTableElementRunColSelectorValue.ColNum;//default value 
                if (Enum.TryParse<ActUIElement.eTableElementRunColSelectorValue>(mAct.GetInputParamValue(ActUIElement.Fields.ColSelectorValue), out eVal))
                    return eVal;
                else
                    return eVal;
            }
            set
            {
                mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.ColSelectorValue, value.ToString());
            }
        }

        public ActUIElement.eTableElementRunColSelectorValue WhereColSelector
        {
            get
            {
                ActUIElement.eTableElementRunColSelectorValue eVal = ActUIElement.eTableElementRunColSelectorValue.ColNum;//default value 
                if (Enum.TryParse<ActUIElement.eTableElementRunColSelectorValue>(mAct.GetInputParamValue(ActUIElement.Fields.WhereColSelector), out eVal))
                    return eVal;
                else
                    return eVal;
            }
            set
            {
                mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereColSelector, value.ToString());
            }
        }        
    }
}
