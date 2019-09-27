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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Drivers.Common;
using GingerCore.GeneralLib;
using GingerCore.Helpers;
using GingerCore.Platforms.PlatformsInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

        List<ComboItem> operationTypeList;        

        public UIElementTableConfigPage(ActUIElement Act, PlatformInfoBase Platform)
        {
            eBaseWindow = BaseWindow.ActEditPage;
            mAct = Act;
            mPlatform = Platform;

            InitializeComponent();
            ShowTableControlActionConfigPage(mPlatform);           
        }
      
        public UIElementTableConfigPage(ElementInfo ElementInfo, ObservableList<Act> Actions, Context context)
        {
            eBaseWindow = BaseWindow.WindowExplorer;
            mAct = new ActUIElement();
            mAct.Context = context;
            mAct.Description = "UI Element Table";
            string targetApp = context?.BusinessFlow.CurrentActivity.TargetApplication;
            mPlatform = PlatformInfoBase.GetPlatformImpl((from x in  WorkSpace.Instance.Solution.ApplicationPlatforms where x.AppName == targetApp select x.Platform).FirstOrDefault());

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

        public ObservableList<ActInputValue> GetTableRelatedInputValues()
        {
            return mAct.InputValues;
        }

        public void TableActionFieldBinding()
        {
            RowSelectorPanelInit();
            //TODO:Use binding for row locator type radio buttons
            //xLocateRowTypeRadioButtonGroup.Init(typeof(ActUIElement.eLocateRowTypeOptions), RowSelectorPanel, mAct.GetOrCreateInputParam(ActUIElement.Fields.LocateRowType));
            WhereColumn.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.WhereColSelector,mAct.GetInputParamValue(ActUIElement.Fields.WhereColSelector)), typeof(ActUIElement.eTableElementRunColSelectorValue), isVENeeded: false, UCselectionChange: WhereColumn_SelectionChanged);    
            WhereColumnTitle.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.WhereColumnTitle, mAct.GetInputParamValue(ActUIElement.Fields.WhereColumnTitle)), isVENeeded: true, UCselectionChange: WhereColumnTitle_SelectionChanged, context: Context.GetAsContext(mAct.Context));
            WhereProperty.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.WhereProperty, mAct.GetInputParamValue(ActUIElement.Fields.WhereProperty)), typeof(ActUIElement.eTableElementRunColPropertyValue), isVENeeded: false, UCselectionChange: WhereProperty_SelectionChanged);
            WhereOperator.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.WhereOperator, mAct.GetInputParamValue(ActUIElement.Fields.WhereOperator)), typeof(ActUIElement.eTableElementRunColOperator), isVENeeded: false, UCselectionChange: WhereOperator_SelectionChanged);
            WhereColumnValue.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActUIElement.Fields.WhereColumnValue, mAct.GetInputParamValue(ActUIElement.Fields.WhereColumnValue)));
            
            RowSelectorValue.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.LocateRowValue), isVENeeded: true, UCselectionChange: RowSelectorValue_SelectionChanged, context: Context.GetAsContext(mAct.Context));          
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

                if (Enum.TryParse(((ComboItem)SubElementActionComboBox.ComboBox.SelectedItem).Value.ToString(), out selectedSubElementAction))
                {
                    operationTypeList = mPlatform.GetTableControlActions(selectedSubElementAction).Select(x => new ComboItem() { Value = x.ToString(), text = x.ToString() }).ToList();
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
            string currentValue = mAct.GetInputParamValue(ActUIElement.Fields.LocateRowType);
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

                operationTypeList = new List<ComboItem>();
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
                    operationTypeList = mPlatform.GetTableControlActions(selectedSubElementAction).Select(x => new ComboItem() { Value = x.ToString(), text = x.ToString() }).ToList();
                }
            }
            else
            {
                operationTypeList = mPlatform.GetTableControlActions(mAct.ElementAction).Select(x => new ComboItem() { Value = x.ToString(), text = x.ToString() }).ToList();
            }
            ControlActionComboBox.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.ControlAction), operationTypeList, isVENeeded: true, UCselectionChange: ControlActionComboBox_SelectionChanged, context: Context.GetAsContext(mAct.Context));

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
                cmbColumnValue.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.LocateColTitle), isVENeeded: true, UCselectionChange: ColumnValue_SelectionChanged, context: Context.GetAsContext(mAct.Context));
                TableActionFieldBinding();
            }
            if(mAct.ElementAction == ActUIElement.eElementAction.TableAction)
            {
                RowLabelPanel.Visibility = Visibility.Collapsed;
                RowSelectorPanel.Visibility = Visibility.Collapsed;
                RowSelectorValuePanel.Visibility = Visibility.Collapsed;
                WhereColumnValue.Visibility = Visibility.Collapsed;
                ColumnLabelPanel.Visibility = Visibility.Collapsed;
                cmbColSelectorValuePanel.Visibility = Visibility.Collapsed;
                cmbColumnValuePanel.Visibility = Visibility.Collapsed;
                WherePanel.Visibility = Visibility.Collapsed;
            }
        }

        private void SetDescriptionDetails()
        {
            try
            {
                txtDescription.Text = string.Empty;
                TextBlockHelper TBH = new TextBlockHelper(txtDescription);

                TBH.AddText("Select the grid cell located by ");
                TBH.AddUnderLineText(cmbColSelectorValue.ComboBox.SelectedItem?.ToString());
                TBH.AddText(" ");
                if (cmbColumnValue.ComboBox.SelectedIndex != -1)
                    TBH.AddBoldText(cmbColumnValue.ComboBox.SelectedItem?.ToString());
                else
                    TBH.AddBoldText(cmbColumnValue.ComboBox.Text);
                TBH.AddText(" and ");
                if (RowNum.IsChecked == true)
                {
                    TBH.AddUnderLineText(" row number ");
                    if (RowSelectorValue.ComboBox.SelectedIndex != -1)
                        TBH.AddBoldText(RowSelectorValue.ComboBox.SelectedItem?.ToString());
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
                    TBH.AddUnderLineText(WhereColumn.ComboBox.SelectedItem?.ToString());
                    TBH.AddText(" ");
                    if (WhereColumnTitle.ComboBox.SelectedIndex != -1)
                        TBH.AddBoldText(WhereColumnTitle.ComboBox.SelectedItem?.ToString());
                    else
                        TBH.AddBoldText(WhereColumnTitle.ComboBox.Text);
                    TBH.AddText(" having control property ");
                    TBH.AddUnderLineText(WhereProperty.ComboBox.SelectedItem?.ToString());
                    TBH.AddText(" ");
                    TBH.AddUnderLineText(WhereOperator.ComboBox.SelectedItem?.ToString());
                    TBH.AddText(" ");
                    TBH.AddBoldText(WhereColumnValue.ValueTextBox.Text);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed", ex);
            }
        }

        private void WhereColumnTitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
                    Enum.TryParse(((ComboEnumItem)WhereColumn.ComboBox.SelectedItem).Value.ToString(), out WhereColumnTitleSelectedValue);

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
                    Enum.TryParse(((ComboEnumItem)cmbColSelectorValue.ComboBox.SelectedItem).Value.ToString(), out colSelectorSelectedValue);

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
            mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowType, (((RadioButton)sender).Tag).ToString());
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
            mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowType, (((RadioButton)sender).Tag).ToString());
            SetDescriptionDetails();
        }

        private void AnyRow_Checked(object sender, RoutedEventArgs e)
        {
            RowSelectorValue.IsEnabled = false;
            WherePanel.Visibility = Visibility.Collapsed;
            mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowType, (((RadioButton)sender).Tag).ToString());
            WhereDataRow.Height = new GridLength(0);
            SetDescriptionDetails();
        }

        private void Where_Checked(object sender, RoutedEventArgs e)
        {
            RowSelectorValue.IsEnabled = false;
            WherePanel.Visibility = Visibility.Visible;
            mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowType, (((RadioButton)sender).Tag).ToString());
            WhereDataRow.Height = new GridLength(100);       
                SetDescriptionDetails();           
        }

        private void Where_Unchecked(object sender, RoutedEventArgs e)
        {
            WherePanel.Visibility = Visibility.Collapsed;
        }

        private void RowSelectorValueVE_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, ActTableElement.Fields.LocateRowValue, Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            RowSelectorValue.ComboBox.Text = mAct.GetOrCreateInputParam(ActUIElement.Fields.LocateRowValue).ToString();
        }

        private void RowSelectorValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            mAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowValue, RowSelectorValue.ComboBox.SelectedValue.ToString());
            
            SetDescriptionDetails();
        }

        private void WhereProperty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetDescriptionDetails();
        }

        private void WhereOperator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetDescriptionDetails();
        }

        private void ControlActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {           
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

                    textboxControlAction.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActUIElement.Fields.ControlActionValue), isVENeeded: true);
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
