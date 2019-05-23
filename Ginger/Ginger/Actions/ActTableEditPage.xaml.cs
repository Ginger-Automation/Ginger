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
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Helpers;
using GingerCore.Drivers.Common;
using mshtml;
using Amdocs.Ginger.Common.UIElement;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActJavaTableEditPage.xaml
    /// </summary>
    public partial class ActTableEditPage : Page
    {
        //private ControlActionsPage CAP = null;
        List<String> mColNames = null;
        int mRowCount = 0;

        private System.Windows.Automation.AutomationElement AEControl;
        private int rowCount = -1;
        private int totalCellElements = 0;
        private int columnCount = 0;

        AutomationElement[,] gridArray;

        ElementInfo mElementInfo;
        ObservableList<Act> mActions = null;
        ObservableList<Act> mOriginalActions = null;

       //Check if its required
        private enum BaseWindow
        {
            WindowExplorer,
            ActEditPage
        }

        BaseWindow eBaseWindow;
       public ActTableElement mAct;

        public ActTableEditPage(ActTableElement Act=null)
        {
            eBaseWindow = BaseWindow.ActEditPage;
            mAct = Act;
            InitializeComponent();

            GingerCore.General.FillComboFromEnumObj(cmbColSelectorValue, mAct.ColSelectorValue);            
           
            GingerCore.General.FillComboFromEnumObj(WhereColumn, mAct.ColSelectorValue);
            

            GingerCore.General.FillComboFromEnumObj(WhereProperty, mAct.WhereProperty);
            GingerCore.General.FillComboFromEnumObj(WhereOperator, mAct.WhereOperator);
            GingerCore.General.FillComboFromEnumObj(ControlActionComboBox, mAct.ControlAction);           

            SetDescriptionDetails();
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(cmbColSelectorValue, ComboBox.SelectedValueProperty, mAct, ActTableElement.Fields.ColSelectorValue);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(cmbColumnValue, ComboBox.TextProperty, mAct, ActTableElement.Fields.LocateColTitle);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RowNum, CheckBox.IsCheckedProperty, mAct, ActTableElement.Fields.ByRowNum);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(AnyRow, CheckBox.IsCheckedProperty, mAct, ActTableElement.Fields.ByRandRow);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(BySelectedRow, CheckBox.IsCheckedProperty, mAct, ActTableElement.Fields.BySelectedRow);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(Where, CheckBox.IsCheckedProperty, mAct, ActTableElement.Fields.ByWhere);

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RowSelectorValue, ComboBox.TextProperty, mAct, ActTableElement.Fields.LocateRowValue);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(WhereColumn, ComboBox.SelectedValueProperty, mAct, ActTableElement.Fields.WhereColSelector);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(WhereColumnTitle, ComboBox.TextProperty, mAct, ActTableElement.Fields.WhereColumnTitle);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(WhereProperty, ComboBox.SelectedValueProperty, mAct, ActTableElement.Fields.WhereProperty);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(WhereOperator, ComboBox.SelectedValueProperty, mAct, ActTableElement.Fields.WhereOperator);     
            WhereColumnValue.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActTableElement.Fields.WhereColumnValue));

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ControlActionComboBox, ComboBox.SelectedValueProperty, mAct, ActTableElement.Fields.ControlAction);
            if (WhereColumn.SelectedIndex == -1)
                WhereColumn.SelectedIndex = 0;
            if (cmbColSelectorValue.SelectedIndex == -1)
                cmbColSelectorValue.SelectedIndex = 0;
            if (WhereProperty.SelectedIndex == -1)
                WhereProperty.SelectedIndex = 0;
            if (WhereOperator.SelectedIndex == -1)
                WhereOperator.SelectedIndex = 0;
        }

        public ActTableEditPage(ElementInfo ElementInfo, ObservableList<Act> Actions,ActTableElement Act=null)
        {            
            eBaseWindow = BaseWindow.WindowExplorer;
            mAct = new ActTableElement();
            mElementInfo = ElementInfo;
            mActions = Actions;
            ShowCellActions();
            InitializeComponent();            
            InitTableInfo();            
            SetComponents();
            SetDescriptionDetails();
        }

        void ShowCellActions()
        {
            // Keep original actions 
            mOriginalActions = new ObservableList<Act>();
            foreach (Act a in mActions)
            {
                mOriginalActions.Add(a);
            }
        }

        private void InitTableInfo()
        {
            //TODO: return common table info which all driver can return
            object o = mElementInfo.GetElementData();
            if (o != null)
            {
                string typeofo = o.GetType().ToString();
                //This is specific for Windows or PB driver.Should be part of driver. Why here?. 
                if (typeofo.Contains("HTMLTableClass"))
                {
                    HTMLTableInfo(o);
                }
                else
                {
                    mColNames = ((TableElementInfo)o).ColumnNames;
                    mRowCount = ((TableElementInfo)o).RowCount;
                }
            }
            else
            {
                mColNames = new List<string>();
                mColNames.Add("0");
                mRowCount = 0;
            }
        }

        private void HTMLTableInfo(object o)
        {
            List<string> colNames = new List<string>();
            mshtml.HTMLTable tab = (HTMLTable)o;
            bool collHeaderCols = true;
            foreach (mshtml.HTMLTableRow row in tab.rows)
            {
                if (!collHeaderCols) break;
                if (row != null)
                {
                    List<string> temp = new List<string>();
                    foreach (IHTMLElement cell in row.cells)
                    {
                        string text = cell.innerText;
                        colNames.Add(text);
                    }
                    collHeaderCols = false;
                }
            }
            mColNames = colNames;
            mRowCount = tab.rows.length;
        }

        public ActTableEditPage(AutomationElement AE)
        {   
            AEControl = AE;       
            calculateGridDimensions();
            LoadGridToArray();
            LoadColumnNameCombo();
            mRowCount = rowCount;
            InitializeComponent();
            SetComponents();
        }
        
        private void UpdateRelatedActions()
        {
            GingerCore.Actions.ActTableElement.eTableAction? previousSelectedControlAction = null;
            if (mActions != null)
            {               
                if (mActions.CurrentItem != null)
                    if (mActions.CurrentItem is ActTableElement)
                        previousSelectedControlAction = ((ActTableElement)mActions.CurrentItem).ControlAction;
                
                mActions.Clear();
            }                        

            //This is just example to show the actions can change
            // need to be per selected filter user did

            if (cmbColSelectorValue.SelectedIndex != -1)
                mAct.ColSelectorValue = (GingerCore.Actions.ActTableElement.eRunColSelectorValue)Enum.Parse(typeof(GingerCore.Actions.ActTableElement.eRunColSelectorValue), cmbColSelectorValue.SelectedValue.ToString());
            
            string description = "";
            string rowVal = "";
            string colVal = "";
            if (RowNum.IsChecked == true)
            {               
                mAct.LocateRowType = "Row Number";
                if (RowSelectorValue != null)
                {
                    if (RowSelectorValue.SelectedIndex != -1)
                        rowVal = RowSelectorValue.SelectedItem.ToString();                        
                    else
                        rowVal =  RowSelectorValue.Text;                    
                    description = " on Row:" + rowVal;
                }                    
            }
            else if (AnyRow.IsChecked == true){              
                mAct.LocateRowType = "Any Row";
                description = " on Random Row";
            }
            else if (BySelectedRow.IsChecked == true)
            {
                mAct.LocateRowType = "By Selected Row";
                description = " on Selected Row";
            }
            else if (Where.IsChecked == true){               
                mAct.LocateRowType = "Where";
                description = " on Row with condition";
            }

            if (cmbColumnValue != null)
            {                
                if (cmbColumnValue.SelectedIndex != -1)
                    colVal = cmbColumnValue.SelectedItem.ToString();
                else
                    colVal = cmbColumnValue.Text;
                description= description + " and Column:" + colVal;
            }

            if (eBaseWindow.Equals(BaseWindow.WindowExplorer) &&cmbColSelectorValue.SelectedIndex != -1 && WhereColumn != null && WhereColumn.SelectedIndex != -1 && RowSelectorValue != null
                && WhereProperty != null && WhereProperty.SelectedIndex != -1 && WhereOperator != null && WhereOperator.SelectedIndex != -1)
            {
                // Add some sample for specific cell                
                mActions.Add(new ActTableElement() { Description = "Get Value of Cell:" + description,
                                                     ControlAction = ActTableElement.eTableAction.GetValue,
                                                     ColSelectorValue = mAct.ColSelectorValue,
                                                     LocateColTitle = colVal,
                                                     LocateRowType = mAct.LocateRowType,
                                                     LocateRowValue = rowVal,
                                                     ByRowNum = (bool)RowNum.IsChecked,
                                                     ByRandRow = (bool)AnyRow.IsChecked,
                                                     BySelectedRow = (bool)BySelectedRow.IsChecked,
                                                     ByWhere = (bool)Where.IsChecked,
                                                     WhereColSelector =(GingerCore.Actions.ActTableElement.eRunColSelectorValue)Enum.Parse(typeof(GingerCore.Actions.ActTableElement.eRunColSelectorValue), WhereColumn.SelectedValue.ToString()),
                                                     WhereColumnTitle = WhereColumnTitle.Text,                                            
                                                     WhereColumnValue = WhereColumnValue.ValueTextBox.Text,                                                     
                                                     WhereOperator = (GingerCore.Actions.ActTableElement.eRunColOperator)Enum.Parse(typeof(GingerCore.Actions.ActTableElement.eRunColOperator), WhereOperator.SelectedValue.ToString()),
                                                     WhereProperty = (GingerCore.Actions.ActTableElement.eRunColPropertyValue)Enum.Parse(typeof(GingerCore.Actions.ActTableElement.eRunColPropertyValue), WhereProperty.SelectedValue.ToString())});               
                mActions.Add(new ActTableElement() { Description = "Set Value of Cell: " + description,
                                                     ControlAction = ActTableElement.eTableAction.SetValue,
                                                     ColSelectorValue = mAct.ColSelectorValue,
                                                     LocateColTitle = colVal,
                                                     LocateRowType = mAct.LocateRowType,
                                                     LocateRowValue = rowVal,
                                                     ByRowNum = (bool)RowNum.IsChecked,
                                                     ByRandRow = (bool)AnyRow.IsChecked,
                                                     BySelectedRow = (bool)BySelectedRow.IsChecked,
                                                     ByWhere = (bool)Where.IsChecked,
                                                     WhereColSelector =(GingerCore.Actions.ActTableElement.eRunColSelectorValue)Enum.Parse(typeof(GingerCore.Actions.ActTableElement.eRunColSelectorValue), WhereColumn.SelectedValue.ToString()),
                                                     WhereColumnTitle = WhereColumnTitle.Text,
                                                     WhereColumnValue = WhereColumnValue.ValueTextBox.Text,
                                                     WhereOperator = (GingerCore.Actions.ActTableElement.eRunColOperator)Enum.Parse(typeof(GingerCore.Actions.ActTableElement.eRunColOperator), WhereOperator.SelectedValue.ToString()),
                                                     WhereProperty = (GingerCore.Actions.ActTableElement.eRunColPropertyValue)Enum.Parse(typeof(GingerCore.Actions.ActTableElement.eRunColPropertyValue), WhereProperty.SelectedValue.ToString())});
                mActions.Add(new ActTableElement() { Description = "Type Value in Cell: " + description,
                                                    ControlAction = ActTableElement.eTableAction.Type,
                                                    ColSelectorValue = mAct.ColSelectorValue,
                                                    LocateColTitle = colVal,
                                                    LocateRowType = mAct.LocateRowType,
                                                    LocateRowValue = rowVal,
                                                    ByRowNum = (bool)RowNum.IsChecked,
                                                    ByRandRow = (bool)AnyRow.IsChecked,
                                                    BySelectedRow = (bool)BySelectedRow.IsChecked,
                                                    ByWhere = (bool)Where.IsChecked,
                                                    WhereColSelector = (GingerCore.Actions.ActTableElement.eRunColSelectorValue)Enum.Parse(typeof(GingerCore.Actions.ActTableElement.eRunColSelectorValue), WhereColumn.SelectedValue.ToString()),
                                                    WhereColumnTitle = WhereColumnTitle.Text,
                                                    WhereColumnValue = WhereColumnValue.ValueTextBox.Text,
                                                    WhereOperator = (GingerCore.Actions.ActTableElement.eRunColOperator)Enum.Parse(typeof(GingerCore.Actions.ActTableElement.eRunColOperator), WhereOperator.SelectedValue.ToString()),
                                                    WhereProperty = (GingerCore.Actions.ActTableElement.eRunColPropertyValue)Enum.Parse(typeof(GingerCore.Actions.ActTableElement.eRunColPropertyValue), WhereProperty.SelectedValue.ToString())});
                mActions.Add(new ActTableElement() { Description = "Click Cell:" + description,
                                                     ControlAction = ActTableElement.eTableAction.Click,
                                                     ColSelectorValue = mAct.ColSelectorValue,
                                                     LocateColTitle = colVal,
                                                     LocateRowType = mAct.LocateRowType,
                                                     LocateRowValue = rowVal,
                                                     ByRowNum = (bool)RowNum.IsChecked,
                                                     BySelectedRow = (bool)BySelectedRow.IsChecked,
                                                     ByRandRow = (bool)AnyRow.IsChecked,
                                                     ByWhere = (bool)Where.IsChecked,
                                                     WhereColSelector =(GingerCore.Actions.ActTableElement.eRunColSelectorValue)Enum.Parse(typeof(GingerCore.Actions.ActTableElement.eRunColSelectorValue), WhereColumn.SelectedValue.ToString()),
                                                     WhereColumnTitle = WhereColumnTitle.Text,
                                                     WhereColumnValue = WhereColumnValue.ValueTextBox.Text,
                                                     WhereOperator = (GingerCore.Actions.ActTableElement.eRunColOperator)Enum.Parse(typeof(GingerCore.Actions.ActTableElement.eRunColOperator), WhereOperator.SelectedValue.ToString()),
                                                     WhereProperty = (GingerCore.Actions.ActTableElement.eRunColPropertyValue)Enum.Parse(typeof(GingerCore.Actions.ActTableElement.eRunColPropertyValue), WhereProperty.SelectedValue.ToString())});
            }

            RestoreOriginalActions();

            //try to select the action the user had selected before
            if (previousSelectedControlAction != null)
            {
                if (mActions != null)
                    foreach (ActTableElement act in mActions)
                        if (act.ControlAction == previousSelectedControlAction)
                        {
                            mActions.CurrentItem = act;
                            break;
                        }
            }
            if (mActions != null && mActions.CurrentItem == null && mActions.Count > 0)
                mActions.CurrentItem = mActions[0]; 
           
            // Add unique actions for the selected filter/cell
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

        private void SetComponents()
        {
            ControlActionComboBox.Visibility = Visibility.Collapsed;
            ControlActionComboBox.Visibility = Visibility.Collapsed;
            OperationTypeRow.Height = new GridLength(0);

            cmbColumnValue.Items.Clear();            
            WhereColumnTitle.Items.Clear();

            for (int i = 0; i < mColNames.Count; i++)
            {
                cmbColumnValue.Items.Add(mColNames[i].ToString());
                WhereColumnTitle.Items.Add(mColNames[i].ToString());
            }
            
            cmbColumnValue.SelectedIndex = 0;
            WhereColumnTitle.SelectedIndex = 0;

            for (int i = 0; i < mRowCount; i++)
            {
                RowSelectorValue.Items.Add(i.ToString());
            }
            RowSelectorValue.SelectedIndex = 0;
         
            ActTableElement ACJT = new ActTableElement();
            GingerCore.General.FillComboFromEnumObj(cmbColSelectorValue, ACJT.ColSelectorValue);
            if (cmbColSelectorValue.SelectedIndex == -1)
                cmbColSelectorValue.SelectedIndex = 0;
            GingerCore.General.FillComboFromEnumObj(WhereColumn, ACJT.ColSelectorValue);
            if (WhereColumn.SelectedIndex == -1)
                WhereColumn.SelectedIndex = 0;
            GingerCore.General.FillComboFromEnumObj(WhereProperty, ACJT.WhereProperty);
            if (WhereProperty.SelectedIndex == -1)
                WhereProperty.SelectedIndex = 0;            
            GingerCore.General.FillComboFromEnumObj(WhereOperator, ACJT.WhereOperator);
            if (WhereOperator.SelectedIndex == -1)
                WhereOperator.SelectedIndex = 0;
            WherePanel.Visibility = Visibility.Collapsed;
            WhereDataRow.Height = new GridLength(0);
            WhereColumnValue.Init(Context.GetAsContext(mAct.Context), ACJT.GetOrCreateInputParam(ActTableElement.Fields.WhereColumnValue));
        }

        private void ColSelectorValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (eBaseWindow.Equals(BaseWindow.WindowExplorer))
            {
                cmbColumnValue.Items.Clear();
                for (int i = 0; i < mColNames.Count; i++)
                {
                    if (cmbColSelectorValue.SelectedValue.ToString() == ActTableElement.eRunColSelectorValue.ColTitle.ToString())                    
                        cmbColumnValue.Items.Add(mColNames[i].ToString());                    
                    else                    
                        cmbColumnValue.Items.Add(i.ToString());                    
                }               
                cmbColumnValue.SelectedIndex = 0;                
            }
            SetDescriptionDetails();
        }

        private void Row_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(eBaseWindow.Equals(BaseWindow.WindowExplorer))
                UpdateRelatedActions();
        }

        private void calculateGridDimensions()
        {
            AutomationElement gridHeaderElement;
            AutomationElement tempElement;
            string gridHeaderType;
            string tempType;

            gridHeaderElement = TreeWalker.ContentViewWalker.GetFirstChild(AEControl);
            while (true)
            {

                if (gridHeaderElement.Current.LocalizedControlType == "scroll bar")
                    gridHeaderElement = TreeWalker.ContentViewWalker.GetNextSibling(gridHeaderElement);
                else
                    break;
            }

            tempElement = gridHeaderElement;

            //Calculate total cells of Grid
            do
            {
                tempElement = TreeWalker.ContentViewWalker.GetNextSibling(tempElement);
                totalCellElements++;
            } while (tempElement != null);

            //Calculate total columns of grid
            tempElement = gridHeaderElement;
            gridHeaderType = gridHeaderElement.Current.LocalizedControlType;
            tempType = gridHeaderType;
            do
            {
                if (!(tempType.Equals(gridHeaderType))) break;

                columnCount++;

                tempElement = TreeWalker.ContentViewWalker.GetNextSibling(tempElement);
                if (tempElement != null)
                {
                    tempType = tempElement.Current.LocalizedControlType;
                }
            } while (tempElement != null && tempType.Equals(gridHeaderType));

            //Calculate Total rows based on Total cells and total columns
            rowCount = (totalCellElements) / columnCount;
        }

        private void LoadGridToArray()
        {
            AutomationElement tempElement;
            gridArray = new AutomationElement[rowCount, columnCount];
            tempElement = TreeWalker.ContentViewWalker.GetFirstChild(AEControl);
            for (int i = 0; i < rowCount; i++)
                for (int j = 0; j < columnCount; j++)
                {
                    gridArray[i, j] = tempElement;
                    tempElement = TreeWalker.ContentViewWalker.GetNextSibling(tempElement);
                }
        }

        private void LoadColumnNameCombo()
        {
            AutomationElement headerElement;
            mColNames = new List<string>();
            int k = 0;
            while (k < columnCount)
            {
                headerElement = gridArray[0, k];
                mColNames.Add(headerElement.Current.Name);
                k++;
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
            UpdateRelatedActions();
            SetDescriptionDetails();
        }

        private void AnyRow_Checked(object sender, RoutedEventArgs e)
        {
            RowSelectorValue.IsEnabled = false;
            WherePanel.Visibility = Visibility.Collapsed;
            WhereDataRow.Height = new GridLength(0);
            UpdateRelatedActions();
            SetDescriptionDetails();
        }

        private void Where_Checked(object sender, RoutedEventArgs e)
        {
            RowSelectorValue.IsEnabled = false;
            WherePanel.Visibility = Visibility.Visible;
            WhereDataRow.Height = new GridLength(100);
            UpdateRelatedActions();
            SetDescriptionDetails();
        }

        private void Where_Unchecked(object sender, RoutedEventArgs e)
        {
            WherePanel.Visibility = Visibility.Collapsed;
        }

        private void WhereColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            if (mColNames != null)
            {
                WhereColumnTitle.Items.Clear();
                for (int i = 0; i < mColNames.Count; i++)
                {
                    if (WhereColumn.SelectedValue.ToString() == ActTableElement.eRunColSelectorValue.ColTitle.ToString())
                        WhereColumnTitle.Items.Add(mColNames[i].ToString());
                    else
                        WhereColumnTitle.Items.Add(i.ToString());
                }
                WhereColumnTitle.SelectedIndex = 0;               
            }
            SetDescriptionDetails(); 
        }

        private void SetDescriptionDetails()
        {
            try
            {
                if (txtDescription == null || cmbColSelectorValue.SelectedItem == null)
                    return;

                txtDescription.Text = string.Empty;
                TextBlockHelper TBH = new TextBlockHelper(txtDescription);

                TBH.AddText("Select the grid cell located by ");
                TBH.AddUnderLineText(cmbColSelectorValue.SelectedItem.ToString());   
                TBH.AddText(" ");
                if (cmbColumnValue.SelectedIndex != -1)
                    TBH.AddBoldText(cmbColumnValue.SelectedItem.ToString());
                else
                    TBH.AddBoldText(cmbColumnValue.Text);
                TBH.AddText(" and ");  
                if (RowNum.IsChecked == true)
                {   
                    TBH.AddUnderLineText("row number ");
                    if (RowSelectorValue.SelectedIndex != -1)
                        TBH.AddBoldText(RowSelectorValue.SelectedItem.ToString());
                    else
                        TBH.AddBoldText(RowSelectorValue.Text);
                }
                else if (AnyRow.IsChecked == true)
                {                    
                    TBH.AddUnderLineText("random ");
                    TBH.AddText("row number");
                }
                else if (Where.IsChecked == true)
                {
                    TBH.AddText("the row located by a cell in ");
                    TBH.AddUnderLineText(WhereColumn.SelectedItem.ToString());
                    TBH.AddText(" ");
                    if (WhereColumnTitle.SelectedIndex != -1)
                        TBH.AddBoldText(WhereColumnTitle.SelectedItem.ToString());
                    else
                        TBH.AddBoldText(WhereColumnTitle.Text);
                    TBH.AddText(" having control property ");
                    TBH.AddUnderLineText(WhereProperty.SelectedItem.ToString());
                    TBH.AddText(" ");
                    TBH.AddUnderLineText(WhereOperator.SelectedItem.ToString());
                    TBH.AddText(" ");
                    TBH.AddBoldText(WhereColumnValue.ValueTextBox.Text);                    
                }                
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed", ex);
            }
        }

        private void ColumnValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (eBaseWindow.Equals(BaseWindow.WindowExplorer))
            {
                UpdateRelatedActions();
                SetDescriptionDetails();
            }  
        }

        private void RowSelectorValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mAct.LocateRowValue = RowSelectorValue.Text;
       
            SetDescriptionDetails();
            if (eBaseWindow.Equals(BaseWindow.WindowExplorer))
            {
                UpdateRelatedActions();              
            }             
        }

        private void WhereColumnTitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRelatedActions();
            SetDescriptionDetails();
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

        private void WhereColumnValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateRelatedActions();
            SetDescriptionDetails();
        }

        private void ColumnValue_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (eBaseWindow.Equals(BaseWindow.ActEditPage))
            {
                UpdateRelatedActions();
                SetDescriptionDetails();
            } 
        }

        private void RowSelectorValue_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            mAct.LocateRowValue = RowSelectorValue.Text;
            
            SetDescriptionDetails();
            if (eBaseWindow.Equals(BaseWindow.WindowExplorer))
            {
                UpdateRelatedActions();               
            }            
        }

        private void WhereColumnTitle_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (eBaseWindow.Equals(BaseWindow.ActEditPage))
            {
                UpdateRelatedActions();
                SetDescriptionDetails();
            } 
        }

        private void WhereColumnValue_LostFocus(object sender, RoutedEventArgs e)
        {

            mAct.WhereColumnValue = WhereColumnValue.ValueTextBox.Text;
            
            
            SetDescriptionDetails();

            if (eBaseWindow.Equals(BaseWindow.WindowExplorer))
            {
                UpdateRelatedActions();
            }    
        }

        private void RowSelectorValueVE_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, ActTableElement.Fields.LocateRowValue, Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            RowSelectorValue.Text = mAct.LocateRowValue;           
        }        
    }
}