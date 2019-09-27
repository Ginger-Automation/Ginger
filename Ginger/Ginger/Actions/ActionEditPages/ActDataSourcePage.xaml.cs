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
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using Ginger.UserControlsLib.TextEditor.Common;
using GingerCore.Actions;
using GingerCore.Actions.Java;
using GingerCore.DataSource;
using GingerCore.GeneralLib;
using GingerCore.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActJavaTableEditPage.xaml
    /// </summary>
    public partial class ActDataSourcePage : Page
    {
        List<String> mColNames = null;
        int mRowCount = 0;
        SelectedContentArgs mSelectedContentArgs;
        List<string> mDSNames = new List<string>();
        private DataSourceTable mDSTable;
        private string mDataSourceName;
        public ActDSTableElement mActDSTblElem = new ActDSTableElement(); 
        ObservableList<DataSourceBase> mDSList = new ObservableList<DataSourceBase>();
        ObservableList<DataSourceTable> mDSTableList = new ObservableList<DataSourceTable>();
        GenericWindow _pageGenericWin = null;
        bool okClicked = false;
        bool bTableDataLoaded = false;
        bool bGridViewSet = false;
        object mObj;
        string mAttrName;
        
        
        private enum BaseWindow
        {
            ValueExpression,
            ActEditPage,
            ValueExpressionEditor
        }
        BaseWindow eBaseWindow;

        public ActDataSourcePage(ActDSTableElement Act = null)
        {
            string VEOrg = "";
            eBaseWindow = BaseWindow.ActEditPage;
            if(Act.ValueExp != null)
                VEOrg = Act.ValueExp;

            mActDSTblElem = Act;
            InitializeComponent();

            InitPageData();

            grdTableData.btnRefresh.Visibility = Visibility.Visible;
            grdTableData.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshTable));

            
            ValueUC.Init(Context.GetAsContext(mActDSTblElem.Context), mActDSTblElem.GetOrCreateInputParam("Value"));

            ControlActionPanel.Visibility = Visibility.Visible;
            ActionRow.Height = new GridLength(55);            

            lblDescription.Visibility = Visibility.Collapsed;
            txtValueExpression.Visibility = Visibility.Hidden;
            VERow.Height = new GridLength(0);
            VEBorder.BorderThickness = new Thickness(0);
            if(VEOrg != "")
                SetDataSourceVEParams(VEOrg);
            ValueUC.ValueTextBox.TextChanged += ValueChanged;
        }

        private void ValueChanged(object sender, TextChangedEventArgs e)
        {
            UpdateValueExpression();
        }

        public ActDataSourcePage(SelectedContentArgs selectedContentArgs)
        {            
            eBaseWindow = BaseWindow.ValueExpressionEditor;
            mSelectedContentArgs = selectedContentArgs;

            string selValueExpression = mSelectedContentArgs.TextEditor.Text.Substring(mSelectedContentArgs.StartPos, mSelectedContentArgs.Length);
           
            InitializeComponent();
            InitPageData();            
            ControlActionPanel.Visibility = Visibility.Hidden;
            ActionRow.Height = new GridLength(0);

            SetDataSourceVEParams(selValueExpression);            
            ValueLabel.Visibility = Visibility.Collapsed;
            ValueUC.Visibility = Visibility.Collapsed;
            ValRow.Height = new GridLength(0);
            DSCol.Width = new GridLength(250);
        }

        public ActDataSourcePage(string DataSourceName,DataSourceTable ds)
        {            
            mDataSourceName = DataSourceName;
            InitializeComponent();
            InitPageData();
            
            GingerCore.General.SelectComboValue(cmbDataSourceName,DataSourceName);
            mDSTable = ds;
            GingerCore.General.SelectComboValue(cmbDataSourceTableName, mDSTable.Name);
            
            ControlActionPanel.Visibility = Visibility.Collapsed;
            ActionRow.Height = new GridLength(0);

            ValueLabel.Visibility = Visibility.Collapsed;
            ValueUC.Visibility = Visibility.Collapsed;
            ValRow.Height = new GridLength(0);
        }

        public ActDataSourcePage(object obj, string AttrName,string actiontype= "Set Value")
        {
            mObj = obj;
            mAttrName = AttrName;
            InitializeComponent();
            InitPageData();

            if (AttrName != "")
            {
                Binding b = new Binding();
                b.Source = mObj;
                b.Path = new PropertyPath(mAttrName);
                b.Mode = BindingMode.OneWay;
                b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                txtValueExpression.SetBinding(TextBlock.TextProperty, b);
            }

            ControlActionPanel.Visibility = Visibility.Collapsed;
            ActionRow.Height = new GridLength(0);
            GingerCore.General.SelectComboValue(ControlActionComboBox, actiontype);
            ValueLabel.Visibility = Visibility.Collapsed;
            ValueUC.Visibility = Visibility.Collapsed;
            ValRow.Height = new GridLength(0);

            if (mObj.GetType() == typeof(TextBox))
            {
                if(((TextBox)mObj).Text == "")
                {
                    ((TextBox)mObj).Text = mActDSTblElem.ValueExp;
                }
                SetDataSourceVEParams(((TextBox)mObj).Text);
            }                
            else if (mObj.GetType().GetProperty(mAttrName).GetValue(mObj) != null)
                SetDataSourceVEParams(mObj.GetType().GetProperty(mAttrName).GetValue(mObj).ToString());
        }

        public void SetDataSourceVEParams(string p)
        {
            if (p == "")
                return;
            string bMarkAsDone = "";
            string bMultiRow = "";
            string iColVal = "";
            string VarName = p.Replace("{DS Name=", "");
            VarName = VarName.Replace("}", "");
            if (p.IndexOf(" DST=") == -1)
                return;
            string DSName = p.Substring(9, p.IndexOf(" DST=") - 9);
            GingerCore.General.SelectComboValue(cmbDataSourceName, DSName);

            p = p.Substring(p.IndexOf(" DST=")).Trim();
            if (mDataSourceName == null)
            {
                return;
            }
            
            string Query = "";
            string rowNum = "0";
            string DSTable = "";
            if (mActDSTblElem.WhereConditions != null)
            {
                mActDSTblElem.WhereConditions.Clear();
            }
            try
            {
                DSTable = p.Substring(p.IndexOf("DST=") + 4, p.IndexOf(" ") - 4);
                GingerCore.General.SelectComboValue(cmbDataSourceTableName, DSTable);
                p = p.Substring(p.TrimStart().IndexOf(" ")).Trim();
                if (p.IndexOf("ACT=") != -1)
                {
                    p = p.Substring(p.TrimStart().IndexOf(" ")).Trim();
                }
                if(p.IndexOf("EP=") == 0)
                {                    
                    p = p.Substring(p.TrimStart().IndexOf(" ES=")).Trim();                    
                }
                else if (p.IndexOf("KEY=") == 0)
                {
                    if(mDSTable.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
                    {
                        string KeyName = p.Substring(p.IndexOf("KEY=") + 4, p.IndexOf("}") - 4).Replace("''","'");
                        if (GingerCore.General.CheckComboItemExist(cmbKeyName, KeyName) == true)
                            GingerCore.General.SelectComboValue(cmbKeyName, KeyName);
                        else
                            cmbKeyName.Text = KeyName;
                    }
                }
                else
                {
                    bMarkAsDone = p.Substring(p.IndexOf("MASD=") + 5, p.IndexOf(" ") - 5);
                    if (bMarkAsDone == "Y")
                        MarkAsDone.IsChecked = true;
                    else
                        MarkAsDone.IsChecked = false;
                    p = p.Substring(p.TrimStart().IndexOf(" ")).Trim();

                    if (p.IndexOf("MR=") == 0)
                    {
                        bMultiRow = p.Substring(p.IndexOf("MR=") + 3, p.IndexOf(" ") - 3);
                        p = p.Substring(p.TrimStart().IndexOf(" ")).Trim();
                    }
                    if (bMultiRow == "Y")
                        MultiRows.IsChecked = true;
                    else
                        MultiRows.IsChecked = false;                    

                    string DSIden = p.Substring(p.IndexOf("IDEN=") + 5, p.IndexOf(" ") - 5);
                    p = p.Substring(p.TrimStart().IndexOf(" ")).Trim();
                    if (DSIden == "Query")
                    {
                        ByQuery.IsChecked = true;
                        Query = p.Substring(p.IndexOf("QUERY=") + 6, p.Length - 7);
                        QueryVal.Text = Query;                        
                    }
                    else
                    {
                        iColVal = p.Substring(p.IndexOf("ICOLVAL=") + 8, p.IndexOf("IROW=") - 9);
                        GingerCore.General.SelectComboValue(cmbColumnValue, iColVal);
                        p = p.Substring(p.TrimStart().IndexOf("IROW="));
                        
                        string IRow = "";
                        if (p.IndexOf(" ") > 0)
                            IRow = p.Substring(p.IndexOf("IROW=") + 5, p.IndexOf(" ") - 5);
                        else
                            IRow = p.Substring(p.IndexOf("IROW=") + 5, p.IndexOf("}") - 5);
                        if (IRow == "NxtAvail")
                        {                           
                            NextAvailable.IsChecked = true;
                        }
                        else if (IRow == "RowNum")
                        {                         
                            RowNum.IsChecked = true;
                            p = p.Substring(p.TrimStart().IndexOf("ROWNUM="));
                            rowNum = p.Substring(p.IndexOf("ROWNUM=") + 7, p.LastIndexOf("}") - 7);
                            RowSelectorValue.Text = rowNum;
                        }
                        else if (IRow == "Where")
                        {
                            if (p.TrimStart().IndexOf("COND=") != -1)
                            {
                                
                                p = p.Substring(p.TrimStart().IndexOf("COND="));
                                string Cond = p.Substring(p.IndexOf("COND=") + 5, p.LastIndexOf("}") - 5);

                                Regex rxDSPattern = new Regex(@"{(\bDS Name=)\w+\b[^{}]*}", RegexOptions.Compiled);
                                MatchCollection matches = rxDSPattern.Matches(Cond);
                                for (int i = 0; i < matches.Count; i++)
                                {
                                    Cond=Cond.Replace(matches[i].Groups[0].Value, "<GINGER_COND_" + i + ">");
                                }
                                Cond = Cond.Replace("''", "~QUOTE~");
                                string[] arrAndCond = Cond.Trim().Split(new string[] { "AND" }, StringSplitOptions.None);
                                for (int iAndCount = 0; iAndCount < arrAndCond.Count(); iAndCount++)
                                {                                    
                                    ActDSConditon.eCondition wCond;
                                    ActDSConditon.eOperator wOpr = ActDSConditon.eOperator.Equals;
                                    string wColVal = "";
                                    string[] arrORCond = arrAndCond[iAndCount].Trim().Split(new string[] { "OR" }, StringSplitOptions.None);
                                    for (int iOrCount = 0; iOrCount < arrORCond.Count(); iOrCount++)
                                    {
                                        if (iAndCount == 0 && iOrCount == 0)
                                            wCond = ActDSConditon.eCondition.EMPTY;
                                        else if (iOrCount == 0)
                                            wCond = ActDSConditon.eCondition.AND;
                                        else
                                            wCond = ActDSConditon.eCondition.OR;
                                        string[] condVal = arrORCond[iOrCount].Trim().Split(new string[] { " " }, StringSplitOptions.None);
                                        string wCol = condVal[0].Replace("[","").Replace("]","");
                                        if (condVal[1] == "=")
                                        {
                                            wOpr = ActDSConditon.eOperator.Equals;
                                            if (arrORCond[iOrCount].IndexOf("'") != -1)
                                                wColVal = arrORCond[iOrCount].Substring(arrORCond[iOrCount].IndexOf("'") + 1, arrORCond[iOrCount].LastIndexOf("'") - arrORCond[iOrCount].IndexOf("'") - 1);
                                            else if(condVal.Length > 1)
                                                wColVal = condVal[2];
                                        }
                                        else if (condVal[1] == "<>")
                                        {
                                            wOpr = ActDSConditon.eOperator.NotEquals;
                                            if (arrORCond[iOrCount].IndexOf("'") != -1)
                                                wColVal = arrORCond[iOrCount].Substring(arrORCond[iOrCount].IndexOf("'") + 1, arrORCond[iOrCount].LastIndexOf("'") - arrORCond[iOrCount].IndexOf("'") - 1);
                                            else if (condVal.Length > 1)
                                                wColVal = condVal[2];
                                        }
                                        else if (condVal[1] == "LIKE")
                                        {
                                            if (arrORCond[iOrCount].IndexOf("'%") != -1 && arrORCond[iOrCount].IndexOf("%'") == -1)
                                            {
                                                wOpr = ActDSConditon.eOperator.EndsWith;
                                                wColVal = arrORCond[iOrCount].Substring(arrORCond[iOrCount].IndexOf("'%") + 2, arrORCond[iOrCount].LastIndexOf("'") - arrORCond[iOrCount].IndexOf("'%") - 2);
                                            }
                                            else if (arrORCond[iOrCount].IndexOf("'%") == -1 && arrORCond[iOrCount].IndexOf("%'") != -1)
                                            {
                                                wOpr = ActDSConditon.eOperator.StartsWith;
                                                wColVal = arrORCond[iOrCount].Substring(arrORCond[iOrCount].IndexOf("'") + 1, arrORCond[iOrCount].LastIndexOf("%'") - arrORCond[iOrCount].IndexOf("'") - 1);
                                            }
                                            else if (arrORCond[iOrCount].IndexOf("'%") != -1 && arrORCond[iOrCount].IndexOf("%'") != -1)
                                            {
                                                wOpr = ActDSConditon.eOperator.Contains;
                                                wColVal = arrORCond[iOrCount].Substring(arrORCond[iOrCount].IndexOf("'%") + 2, arrORCond[iOrCount].LastIndexOf("%'") - arrORCond[iOrCount].IndexOf("'%") - 2);
                                            }
                                        }
                                        else if (condVal.Count() > 2 && condVal[1] == "NOT" && condVal[2] == "LIKE")
                                        {
                                            if (arrORCond[iOrCount].IndexOf("'%") != -1 && arrORCond[iOrCount].IndexOf("%'") == -1)
                                            {
                                                wOpr = ActDSConditon.eOperator.NotEndsWith;
                                                wColVal = arrORCond[iOrCount].Substring(arrORCond[iOrCount].IndexOf("'%") + 2, arrORCond[iOrCount].LastIndexOf("'") - arrORCond[iOrCount].IndexOf("'%") - 2);
                                            }
                                            else if (arrORCond[iOrCount].IndexOf("'%") == -1 && arrORCond[iOrCount].IndexOf("%'") != -1)
                                            {
                                                wOpr = ActDSConditon.eOperator.NotStartsWith;
                                                wColVal = arrORCond[iOrCount].Substring(arrORCond[iOrCount].IndexOf("'") + 1, arrORCond[iOrCount].LastIndexOf("%'") - arrORCond[iOrCount].IndexOf("'") - 1);
                                            }
                                            else if (arrORCond[iOrCount].IndexOf("'%") != -1 && arrORCond[iOrCount].IndexOf("%'") != -1)
                                            {
                                                wOpr = ActDSConditon.eOperator.NotContains;
                                                wColVal = arrORCond[iOrCount].Substring(arrORCond[iOrCount].IndexOf("'%") + 2, arrORCond[iOrCount].LastIndexOf("%'") - arrORCond[iOrCount].IndexOf("'%") - 2);
                                            }
                                        }
                                        else if (condVal.Count() > 2 && condVal[1].ToUpper() == "IS" && condVal[2].ToUpper() == "NULL")
                                            wOpr = ActDSConditon.eOperator.IsNull;
                                        else if (condVal.Count() > 3 && condVal[1].ToUpper() == "IS" && condVal[2].ToUpper() == "NOT" & condVal[3].ToUpper() == "NULL")
                                            wOpr = ActDSConditon.eOperator.IsNotNull;

                                        for (int i = 0; i < matches.Count; i++)
                                        {
                                            wColVal=wColVal.Replace("<GINGER_COND_" + i + ">", matches[i].Groups[0].Value);
                                        }
                                        mActDSTblElem.AddDSCondition(wCond, wCol, wOpr, wColVal.Replace("~QUOTE~","'"), mColNames);
                                        
                                    }
                                }                                
                            }
                            else if (p.TrimStart().IndexOf("WCOLVAL=") != -1 && p.TrimStart().IndexOf("WOPR=") != -1)
                            {
                                p = p.Substring(p.TrimStart().IndexOf("WCOLVAL="));
                                string wColVal = p.Substring(p.IndexOf("WCOLVAL=") + 8, p.IndexOf("WOPR=") - 9);
                                p = p.Substring(p.TrimStart().IndexOf("WOPR="));
                                string wOpr = "";
                                ActDSConditon.eOperator wDSOpr = ActDSConditon.eOperator.Equals;
                                string wRowVal = "";
                                if (p.IndexOf("WROWVAL=") == -1)
                                    wOpr = p.Substring(p.IndexOf("WOPR=") + 5, p.IndexOf("}") - 5);
                                else
                                    wOpr = p.Substring(p.IndexOf("WOPR=") + 5, p.IndexOf("WROWVAL=") - 6);
                                if (wOpr != "Is Null" && wOpr != "Is Null")
                                {
                                    p = p.Substring(p.TrimStart().IndexOf("WROWVAL="));
                                    wRowVal = p.Substring(p.IndexOf("WROWVAL=") + 8, p.IndexOf("}") - 8);
                                }
                                foreach (ActDSConditon.eOperator opr in Enum.GetValues(typeof(ActDSConditon.eOperator)))
                                    if (opr.ToString().ToUpper() == wOpr.Replace(" ","").ToUpper())
                                        wDSOpr = opr;
                                mActDSTblElem.AddDSCondition(ActDSConditon.eCondition.EMPTY, wColVal, wDSOpr, wRowVal, mColNames);
                            }
                            else
                            {
                                mActDSTblElem.AddDSCondition(ActDSConditon.eCondition.EMPTY, mColNames[0], ActDSConditon.eOperator.Equals, "", mColNames);
                            }
                            Where.IsChecked = true;
                            
                            grdCondition.DataSourceList = mActDSTblElem.WhereConditions;        
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Length cannot be less than zero."))
                {
                    return;
                }
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
                return;
            }
        }

        private void InitPageData()
        {            
            mDSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            if (mDSList.Count == 0)
                return;
            foreach (DataSourceBase ds in mDSList)
                mDSNames.Add(ds.Name);
            GingerCore.General.FillComboFromList(cmbDataSourceName, mDSNames);
            
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(cmbDataSourceName, ComboBox.TextProperty, mActDSTblElem, ActDSTableElement.Fields.DSName);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(cmbDataSourceTableName, ComboBox.TextProperty, mActDSTblElem, ActDSTableElement.Fields.DSTableName);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(cmbColumnValue, ComboBox.TextProperty, mActDSTblElem, ActDSTableElement.Fields.LocateColTitle);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RowSelectorValue, ComboBox.TextProperty, mActDSTblElem, ActDSTableElement.Fields.LocateRowValue);
           

            if(mActDSTblElem == null || (mActDSTblElem.DSName ==null && mActDSTblElem.DSTableName == null) || (mActDSTblElem.DSName == "" && mActDSTblElem.DSTableName == "" ))
            {
                cmbDataSourceName.SelectedIndex = 0;
                mDataSourceName = mDSNames[0];
            }

            if (mActDSTblElem.ValueExp != null && mActDSTblElem.ValueExp != "")
                SetDataSourceVEParams(mActDSTblElem.ValueExp);

            GetTableDetails();
            GingerCore.General.FillComboFromEnumType(ControlActionComboBox, typeof(ActDSTableElement.eControlAction));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ControlActionComboBox, ComboBox.SelectedValueProperty, mActDSTblElem, ActJavaElement.Fields.ControlAction);

            ExcelFilePath.Init(Context.GetAsContext(mActDSTblElem.Context), mActDSTblElem, ActDSTableElement.Fields.ExcelPath, true, true, UCValueExpression.eBrowserType.File, "xlsx");
            ExcelSheetName.Init(Context.GetAsContext(mActDSTblElem.Context), mActDSTblElem, ActDSTableElement.Fields.ExcelSheetName, true);
            ExcelFilePath.ValueTextBox.TextChanged += ExcelFilePathTextBox_TextChanged;
            ExcelSheetName.ValueTextBox.TextChanged += ExcelSheetNameTextBox_TextChanged;
            ExcelFilePath.ValueTextBox.LostFocus += ExcelFilePathTextBox_LostFocus;
            ExcelSheetName.ValueTextBox.LostFocus += ExcelSheetNameTextBox_LostFocus; 
            
            UpdateValueExpression();
            //NextAvailable.IsChecked = mActDSTblElem.ByNextAvailable;
            SetComponents();
            SetTableDetails();
            grdTableData.SetTitleLightStyle = true;
            SetConditionGridView();
            SetConditionGridData();

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(Where, RadioButton.IsCheckedProperty, mActDSTblElem, ActDSTableElement.Fields.ByWhere);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(NextAvailable, RadioButton.IsCheckedProperty, mActDSTblElem, ActDSTableElement.Fields.ByNextAvailable);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RowNum, RadioButton.IsCheckedProperty, mActDSTblElem, ActDSTableElement.Fields.ByRowNum);

            grdCondition.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddWhereCondition));
            grdCondition.btnDelete.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteWhereCondition));
            grdCondition.Grid.LostFocus += Grid_LostFocus;
        }

        private void ExcelSheetNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateValueExpression();
        }

        private void ExcelFilePathTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateValueExpression();
        }
        
        private void ExcelSheetNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateValueExpression();
        }

        private void ExcelFilePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateValueExpression();
        }

        private void Grid_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateValueExpression();
        }      

        private void AddWhereCondition(object sender, RoutedEventArgs e)
        {
            ActDSConditon.eCondition defaultCond = ActDSConditon.eCondition.EMPTY;
            ObservableList<string> Condition = new ObservableList<string>();
            if(mActDSTblElem.WhereConditions.Count > 0)
            {
                foreach (ActDSConditon.eCondition item in Enum.GetValues(typeof(ActDSConditon.eCondition)))
                    if(item.ToString() != "EMPTY")
                        Condition.Add(item.ToString());
                defaultCond = ActDSConditon.eCondition.AND;
            }                
            mActDSTblElem.AddDSCondition(defaultCond,mColNames[0], ActDSConditon.eOperator.Equals ,"",mColNames);
            UpdateValueExpression();
        }

        private void DeleteWhereCondition(object sender, RoutedEventArgs e)
        {
            if(mActDSTblElem.WhereConditions.Count > 0)
            {
                mActDSTblElem.WhereConditions[0].PossibleCondValues = new ObservableList<string>();
                mActDSTblElem.WhereConditions[0].wCondition = ActDSConditon.eCondition.EMPTY;
            }
            grdCondition.DataSourceList = mActDSTblElem.WhereConditions;
            UpdateValueExpression();
        }
        private void RefreshTable(object sender, RoutedEventArgs e)
        {           
            SetGridData();
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (ErrorLabel.Content.ToString() != "")
            {
                if (Reporter.ToUser(eUserMsgKey.InvalidValueExpression, "Data Source") == Amdocs.Ginger.Common.eUserMsgSelection.No)
                {
                    return;
                }
            }
            if(mObj!= null)
            {
                if (mObj.GetType() == typeof(TextBox))
                    ((TextBox)mObj).Text = mActDSTblElem.ValueExp;
            }
            
            okClicked = true;
            _pageGenericWin.Close();
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            this.Width = 550;
            Button okBtn = new Button();
            okBtn.Content = "OK";
            okBtn.Click += new RoutedEventHandler(OKButton_Click);
            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(okBtn);
           
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, true, "Cancel");
        }

        private void SetComponents()
        {
            setColValList();
            mActDSTblElem.UpdateDSConditionColumns(mColNames);
            grdCondition.DataSourceList = mActDSTblElem.WhereConditions;
            RowSelectorValue.Items.Clear();
            for (int i = 0; i < mRowCount; i++)
            {
                RowSelectorValue.Items.Add(i.ToString());
            }
            if(mRowCount >0)
                RowSelectorValue.SelectedIndex = 0;
            if (grdCondition.DataSourceList.Count == 0)
            {
                WherePanel.Visibility = Visibility.Collapsed;
            }
        }

        private void setColValList()
        {
            if (mColNames == null)
                return;
            string ColumnName = mActDSTblElem.LocateColTitle;
            
                if (cmbColumnValue.SelectedItem == null || GingerCore.General.CheckComboItems(cmbColumnValue, mColNames) == false)
                {
                    cmbColumnValue.Items.Clear();
                    GingerCore.General.FillComboFromList(cmbColumnValue, mColNames);
                    cmbColumnValue.SelectedIndex = 0;
                }
            cmbColumnValue.Text = ColumnName;
            for (int i = 0; i < mColNames.Count; i++)
            {
                if ((mColNames[i].ToString() == "GINGER_ID" || mColNames[i].ToString() == "GINGER_LAST_UPDATED_BY" || mColNames[i].ToString() == "GINGER_LAST_UPDATE_DATETIME") && ControlActionComboBox.SelectedValue != null && ControlActionComboBox.SelectedValue.ToString() == "SetValue")
                    GingerCore.General.RemoveComboItem(cmbColumnValue,mColNames[i].ToString());
            }

            if (!mColNames.Contains("GINGER_USED"))
            {
                NextAvailable.Visibility = Visibility.Collapsed;
                MarkRow.Height = new GridLength(0);
            }
            else
            {
                NextAvailable.Visibility = Visibility.Visible;
                MarkRow.Height = new GridLength(25);
            }                        
                
        }
        
        private void Row_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }

        private void Customized_Checked(object sender, RoutedEventArgs e)
        {
            //if((RowNum != null && RowNum.IsChecked == true) || (NextAvailable!=null && NextAvailable.IsChecked == true))
            //    IdentifierRow.Height = new GridLength(240);
            //else
            //    IdentifierRow.Height = new GridLength(310);

            if (CustomizedPanel != null)
                 CustomizedPanel.Visibility = Visibility.Visible;
            if (QueryPanel != null)
                QueryPanel.Visibility = Visibility.Collapsed;

            if (MultiRows != null && Where !=null)
            {
                if(Where.IsChecked == true)
                {
                    MultiRows.IsEnabled = true;
                }
                else
                {
                    MultiRows.IsChecked = false;
                    MultiRows.IsEnabled = false;
                }                
            }
            mActDSTblElem.Customized = true;
            UpdateValueExpression();
            SetIdentifierHeight();

        }
        private void Customized_Unchecked(object sender, RoutedEventArgs e)
        {
            if (QueryPanel != null)
                QueryPanel.Visibility = Visibility.Visible;
            if (CustomizedPanel != null)
                CustomizedPanel.Visibility = Visibility.Collapsed;
            mActDSTblElem.Customized = false;
            UpdateValueExpression();

        }
        private void Query_Checked(object sender, RoutedEventArgs e)
        {
            //IdentifierRow.Height = new GridLength(173);

            if (QueryPanel != null)
                QueryPanel.Visibility = Visibility.Visible;
            if (CustomizedPanel != null)
                CustomizedPanel.Visibility = Visibility.Collapsed;
            if (MultiRows != null)
                MultiRows.IsEnabled = true;
            mActDSTblElem.ByQuery = true;
            UpdateValueExpression();
            SetIdentifierHeight();
            
        }

        private void GetTableDetails()
        {
            if (mDSTable == null)
            {
                //mValueCalculated = mValueCalculated.Replace(p, "ERROR: Data Source :" + DataSource + " not found ");
                return;
            }
            mColNames = mDSTable.DSC.GetColumnList(mDSTable.Name);

            mRowCount = mDSTable.DSC.GetRowCount(mDSTable.Name);
            
        }
     

        private void RowNum_Checked(object sender, RoutedEventArgs e)
        {
            if (RowSelectorValue != null)
            {
                RowSelectorValue.Visibility = Visibility.Visible;
                RowSelectorValueVE.Visibility = Visibility.Visible;
            }
            //RowSelectorValue.IsEnabled = true;
            mActDSTblElem.ByRowNum = true;

            if (WherePanel != null)
            {
                WherePanel.Visibility = Visibility.Collapsed;
                //WherePanelRow.Height = new GridLength(110, GridUnitType.Star);
                //WhereDataRow.Height = new GridLength(0);
            }
            //IdentifierRow.Height = new GridLength(240);
            if (MultiRows != null)
            {
                MultiRows.IsChecked = false;
                MultiRows.IsEnabled = false;
            }
            UpdateValueExpression();
            SetIdentifierHeight();
        }

        private void RowNum_Unchecked(object sender, RoutedEventArgs e)
        {
            RowSelectorValue.Visibility = Visibility.Collapsed;
            RowSelectorValueVE.Visibility = Visibility.Collapsed;
            mActDSTblElem.ByRowNum = false;
        }
            

        private void NextAvailable_Checked(object sender, RoutedEventArgs e)
        {            
            RowSelectorValue.Visibility = Visibility.Collapsed;            
            WherePanel.Visibility = Visibility.Collapsed;
            RowSelectorValue.Visibility = Visibility.Collapsed;
            RowSelectorValueVE.Visibility = Visibility.Collapsed;
            //IdentifierRow.Height = new GridLength(240);           
            MultiRows.IsChecked = false;
            MultiRows.IsEnabled = false;
            mActDSTblElem.ByNextAvailable = true;
            UpdateValueExpression();
            SetIdentifierHeight();
        }

        private void Where_Checked(object sender, RoutedEventArgs e)
        {
            mActDSTblElem.ByWhere = true;
            RowSelectorValue.Visibility = Visibility.Collapsed;
            RowSelectorValue.Visibility = Visibility.Collapsed;
            RowSelectorValueVE.Visibility = Visibility.Collapsed;
            WherePanel.Visibility = Visibility.Visible;            
            MultiRows.IsEnabled = true;
            //IdentifierRow.Height = new GridLength(310);
            if (mColNames == null)
                return;

            if (mActDSTblElem.WhereConditions.Count == 0)
            {
                ActDSConditon.eCondition defaultCond = ActDSConditon.eCondition.EMPTY;
                ObservableList<string> Condition = new ObservableList<string>();                
                mActDSTblElem.AddDSCondition(defaultCond, mColNames[0], ActDSConditon.eOperator.Equals,"",mColNames);
                UpdateValueExpression();
            }

            int count = mActDSTblElem.WhereConditions.Count;
            UpdateValueExpression();
            SetIdentifierHeight();
        }

        private void Where_Unchecked(object sender, RoutedEventArgs e)
        {
            WherePanel.Visibility = Visibility.Collapsed;
            SetIdentifierHeight();
            mActDSTblElem.ByWhere = false;
            //IdentifierRow.Height = new GridLength(240);
        }
        public void CommonVE ()
        {
            if (txtValueExpression == null || ControlActionComboBox.SelectedValue == null || ControlActionComboBox.SelectedValue == null)
                return;
            try
            {
                ErrorLabel.Content = "";
                txtValueExpression.Text = string.Empty;
                TextBlockHelper TBH = new TextBlockHelper(txtValueExpression);
                TBH.AddText("{DS Name=");
                TBH.AddBoldText(mDataSourceName);
                TBH.AddText(" DST=");
                TBH.AddBoldText(mDSTable.Name);

                TBH.AddText(" ACT=");
                if (ControlActionComboBox.SelectedValue.ToString() == "DeleteRow")
                    TBH.AddBoldText("DR");
                else if (ControlActionComboBox.SelectedValue.ToString() == "RowCount")
                    TBH.AddBoldText("RC");
                else if (ControlActionComboBox.SelectedValue.ToString() == "AvailableRowCount")
                    TBH.AddBoldText("ARC");
                else if (ControlActionComboBox.SelectedValue.ToString() == "DeleteAll")
                    TBH.AddBoldText("DA");
                else if (ControlActionComboBox.SelectedValue.ToString() == "ExportToExcel")
                    TBH.AddBoldText("ETE");
                else if (ControlActionComboBox.SelectedValue.ToString() == "MarkAllUnUsed")
                    TBH.AddBoldText("NA");
                else if (ControlActionComboBox.SelectedValue.ToString() == "MarkAllUsed")
                    TBH.AddBoldText("YA");
                else
                    TBH.AddBoldText("MASD");

                if (ControlActionComboBox.SelectedValue.ToString() == "ExportToExcel")
                {
                    TBH.AddBoldText(" EP=");
                    TBH.AddBoldText(ExcelFilePath.ValueTextBox.Text.Trim());
                    TBH.AddBoldText(" ES=");
                    TBH.AddBoldText(ExcelSheetName.ValueTextBox.Text.Trim());
                }
                else
                {
                    if (mDSTable.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
                    {
                        TBH.AddText(" KEY=");
                        if (cmbKeyName != null)
                        {
                            if (cmbKeyName.SelectedItem == null)
                                TBH.AddBoldText(cmbKeyName.Text.Replace("'", "''"));
                            else
                                TBH.AddBoldText(cmbKeyName.SelectedItem.ToString().Replace("'", "''"));
                        }
                    }
                    else
                    {
                        TBH.AddText(" MASD=");
                        //TBH.AddText("DST=" + mDSTable.Name + " MASD=");
                        if (MarkAsDone.IsChecked == true)
                            TBH.AddBoldText("Y");
                        else
                            TBH.AddBoldText("N");

                        TBH.AddText(" MR=");
                        if (MultiRows.IsChecked == true)
                            TBH.AddBoldText("Y");
                        else
                            TBH.AddBoldText("N");

                        TBH.AddText(" IDEN=");
                        if (SelectedCell.IsChecked == true)
                        {
                            List<object> SelectedItemsList = new List<object>();
                            if (grdTableData.Grid.SelectedCells.Count == 0)
                            {
                                ErrorLabel.Content = "Please select a Valid Cell from Table Data for Current Identifier";
                                ErrorLabel.Foreground = Brushes.Red;
                                //grdTableData.Grid.SelectedIndex = 0;
                                //SelectedItemsList = grdTableData.Grid.Items.Cast<object>().ToList();                        
                            }
                            else
                            {
                                SelectedItemsList = grdTableData.Grid.SelectedCells.Cast<object>().ToList();

                                string selColName = ((DataGridCellInfo)SelectedItemsList[0]).Column.Header.ToString();
                                selColName = selColName.Replace("__", "_");
                                string SelCellGingerId = ((DataRowView)((DataGridCellInfo)SelectedItemsList[0]).Item).Row["GINGER_ID"].ToString();

                                TBH.AddText("Query QUERY=Select " + selColName + " from " + mDSTable.Name + " WHERE GINGER_ID=" + SelCellGingerId);
                            }
                        }
                        else if (ByQuery.IsChecked == true)
                        {
                            TBH.AddText("Query QUERY=");
                            TBH.AddBoldText(QueryVal.Text);
                        }
                        else
                        {
                            //TBH.AddText("Cust ICOL=" + cmbColSelectorValue.SelectedItem.ToString() + " ICOLVAL=");
                            TBH.AddText("Cust ICOLVAL=");
                            if (cmbColumnValue.SelectedIndex != -1)
                                TBH.AddBoldText(cmbColumnValue.SelectedItem.ToString());
                            else
                                TBH.AddBoldText(cmbColumnValue.Text);
                            TBH.AddText(" IROW=");
                            if (RowNum.IsChecked == true)
                            {
                                TBH.AddUnderLineText("RowNum");
                                TBH.AddText(" ROWNUM=");
                                if (RowSelectorValue.SelectedIndex != -1)
                                    TBH.AddBoldText(RowSelectorValue.SelectedItem.ToString());
                                else
                                    TBH.AddBoldText(RowSelectorValue.Text);
                            }
                            else if (NextAvailable.IsChecked == true)
                            {
                                TBH.AddUnderLineText("NxtAvail");
                                //TBH.AddText("row number");
                            }
                            else if (Where.IsChecked == true)
                            {
                                TBH.AddUnderLineText("Where");
                                //TBH.AddText(" WCOL=");
                                //TBH.AddUnderLineText(WhereColumn.SelectedItem.ToString());
                                //TBH.AddText(" WCOLVAL=");
                                //if (WhereColumnTitle.SelectedIndex != -1)
                                //    TBH.AddBoldText(WhereColumnTitle.SelectedItem.ToString());
                                //else
                                //    TBH.AddBoldText(WhereColumnTitle.Text);
                                //TBH.AddText(" WOPR=");
                                //TBH.AddUnderLineText(WhereOperator.SelectedItem.ToString());
                                //TBH.AddText(" WROWVAL=");
                                ////TBH.AddBoldText(WhereColumnValue.Text);
                                //TBH.AddBoldText(WhereColumnValue.Text);

                                TBH.AddText(" COND=");
                                for (int i = 0; i < mActDSTblElem.WhereConditions.Count; i++)
                                {
                                    string wQuery = "";
                                    string wCond = mActDSTblElem.WhereConditions[i].wCondition.ToString();
                                    string wColVal = "[" + mActDSTblElem.WhereConditions[i].wTableColumn.ToString().Trim() + "]";
                                    string wOpr = mActDSTblElem.WhereConditions[i].wOperator.ToString();
                                    string wRowVal = mActDSTblElem.WhereConditions[i].wValue.ToString();
                                    if (wRowVal.IndexOf("{DS Name") == -1)
                                        wRowVal = wRowVal.Replace("'", "''");

                                    if (wCond == "EMPTY")
                                        wCond = "";

                                    if (wOpr == "Equals")
                                    {
                                        if (wColVal == "[GINGER_ID]")
                                        {
                                            wQuery = wQuery + " " + wCond + " " + wColVal + " = " + wRowVal;
                                        }
                                        else
                                        {
                                            wQuery = wQuery + " " + wCond + " " + wColVal + " = '" + wRowVal + "'";
                                        }
                                    }
                                    else if (wOpr == "NotEquals")
                                    {
                                        if (wColVal == "[GINGER_ID]")
                                        {
                                            wQuery = wQuery + " " + wCond + " " + wColVal + " <> " + wRowVal;
                                        }
                                        else
                                        {
                                            wQuery = wQuery + " " + wCond + " " + wColVal + " <> '" + wRowVal + "'";
                                        }
                                    }
                                    else if (wOpr == "Contains")
                                        wQuery = wQuery + " " + wCond + " " + wColVal + " LIKE " + "'%" + wRowVal + "%'";
                                    else if (wOpr == "NotContains")
                                        wQuery = wQuery + " " + wCond + " " + wColVal + " NOT LIKE " + "'%" + wRowVal + "%'";
                                    else if (wOpr == "StartsWith")
                                        wQuery = wQuery + " " + wCond + " " + wColVal + " LIKE '" + wRowVal + "%'";
                                    else if (wOpr == "NotStartsWith")
                                        wQuery = wQuery + " " + wCond + " " + wColVal + " NOT LIKE '" + wRowVal + "%'";
                                    else if (wOpr == "EndsWith")
                                        wQuery = wQuery + " " + wCond + " " + wColVal + " LIKE '%" + wRowVal + "'";
                                    else if (wOpr == "NotEndsWith")
                                        wQuery = wQuery + " " + wCond + " " + wColVal + " NOT LIKE '%" + wRowVal + "'";
                                    else if (wOpr == "IsNull")
                                        wQuery = wQuery + " " + wCond + " " + wColVal + " IS NULL";
                                    else if (wOpr == "IsNotNull")
                                        wQuery = wQuery + " " + wCond + " " + wColVal + " IS NOT NULL";

                                    TBH.AddBoldText(wQuery);
                                }
                            }
                        }
                    }
                }
                TBH.AddText("}");
                mActDSTblElem.ValueExp = TBH.GetText();
            }
            catch (Exception ex)
            {
                mActDSTblElem.ValueExp = "";
                Reporter.ToLog(eLogLevel.ERROR, "Failed", ex);
            }
        }
        
        private void UpdateValueExpression()
        {
            DataSourceBase DataSource = null;
            
            if (mDSList.Count != 0)
            {
                foreach (DataSourceBase ds in mDSList)
                    if (ds.Name == mDataSourceName)
                        DataSource = ds;

                if (DataSource.DSType == DataSourceBase.eDSType.MSAccess)
                {
                    CommonVE();
                }
                else if (DataSource.DSType == DataSourceBase.eDSType.LiteDataBase)
                {
                    if (ValueUC != null)
                    {
                        mActDSTblElem.ValueUC = mActDSTblElem.GetInputParamCalculatedValue("Value");
                    }
                    ErrorLabel.Content = "";
                    txtValueExpression.Text = string.Empty;
                    TextBlockHelper TBH = new TextBlockHelper(txtValueExpression);
                    TBH.AddText("{DS Name=");
                    TBH.AddBoldText(mDataSourceName);
                    TBH.AddText(" DST=");
                    TBH.AddBoldText(mDSTable.Name);
                    TBH.AddText(" MASD=");
                    
                    if (MarkAsDone.IsChecked == true)
                        TBH.AddBoldText("Y");
                    else
                        TBH.AddBoldText("N");
                    if (ControlActionComboBox.SelectedValue != null)
                    {
                        if (ControlActionComboBox.SelectedValue.ToString() == "ExportToExcel")
                        {
                            TBH.AddText(" Query QUERY=");
                            TBH.AddBoldText(ExcelFilePath.ValueTextBox.Text.Trim() + "," + ExcelSheetName.ValueTextBox.Text.Trim());
                            TBH.AddText("}");
                            mActDSTblElem.ValueExp = TBH.GetText();
                            return;
                        }
                    }
                    if (ByQuery.IsChecked == true)
                    {
                        TBH.AddText(" Query QUERY=");
                        TBH.AddBoldText(QueryVal.Text);
                        TBH.AddText("}");
                        mActDSTblElem.ValueExp = TBH.GetText();
                        return;
                    }
                    
                    if (mDSTable.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
                    {
                        TBH.AddText(" Query QUERY=");
                        TBH.AddText("db." + mDSTable.Name);
                        mActDSTblElem.IsKeyValueTable = true;
                        if (ControlActionComboBox.SelectedValue != null)
                        {
                            if (ControlActionComboBox.SelectedValue.ToString() == "GetValue")
                            {
                                if (cmbKeyName.SelectedItem == null)
                                {
                                    TBH.AddText(".select GINGER_KET_VALUE where GINGER_KEY_NAME=\"" + cmbKeyName.Text + "\"");
                                }
                                else
                                {
                                    TBH.AddText(".select GINGER_KEY_VALUE where GINGER_KEY_NAME=\"" + cmbKeyName.SelectedItem.ToString() + "\"");
                                }
                            }

                            else if (ControlActionComboBox.SelectedValue.ToString() == "SetValue")
                            {
                                if (cmbKeyName.SelectedItem == null)
                                {
                                    TBH.AddText(".update GINGER_KET_VALUE = \"" + mActDSTblElem.Value + "\" where GINGER_KEY_NAME=\"" + cmbKeyName.Text + "\"");
                                }
                                else
                                {
                                    TBH.AddText(".update GINGER_KEY_VALUE = \"" + mActDSTblElem.Value + "\" where GINGER_KEY_NAME=\"" + cmbKeyName.SelectedItem.ToString() + "\"");
                                }
                            }
                            else if (ControlActionComboBox.SelectedValue.ToString() == "DeleteRow")
                            {
                                if (cmbKeyName.SelectedItem == null)
                                {
                                    TBH.AddBoldText(".delete GINGER_KEY_NAME=\"" + cmbKeyName.Text + "\"");
                                }
                                else
                                {
                                    TBH.AddText(".delete GINGER_KEY_NAME=\"" + cmbKeyName.SelectedItem.ToString() + "\"");
                                }

                            }
                            else if (ControlActionComboBox.SelectedValue.ToString() == "RowCount")
                            {
                                TBH.AddBoldText(".count");
                                TBH.AddText("}");
                                mActDSTblElem.ValueExp = TBH.GetText();

                                return;
                            }
                        }
                        TBH.AddText("}");
                        mActDSTblElem.ValueExp = TBH.GetText();
                        return;
                    }
                    else if (Customized.IsChecked == true)
                    {
                        mActDSTblElem.IsKeyValueTable = false;
                        TBH.AddText(" Cust ICOLVAL=");
                        if (cmbColumnValue.SelectedIndex != -1)
                            TBH.AddBoldText(cmbColumnValue.SelectedItem.ToString());
                        else
                            TBH.AddBoldText(cmbColumnValue.Text);
                        TBH.AddText(" IROW=");
                        if (RowNum.IsChecked == true)
                        {
                            TBH.AddUnderLineText("RowNum");
                            TBH.AddText(" ROWNUM=");
                            if (RowSelectorValue.SelectedIndex != -1)
                                TBH.AddBoldText(RowSelectorValue.SelectedItem.ToString());
                            else
                                TBH.AddBoldText(RowSelectorValue.Text);
                        }
                        else if (NextAvailable.IsChecked == true)
                        {
                            TBH.AddUnderLineText("NxtAvail");
                        }
                        TBH.AddText(" Query QUERY=");

                        TBH.AddText("db." + mDSTable.Name);
                        //Get ColunmNA
                        if (cmbColumnValue.SelectedIndex != -1)
                        {
                            mActDSTblElem.LocateColTitle = cmbColumnValue.SelectedItem.ToString();
                        }
                        else
                        {
                            mActDSTblElem.LocateColTitle = cmbColumnValue.Text;
                        }
                        //add operation
                        if (ControlActionComboBox.SelectedValue != null)
                        {
                            if (ControlActionComboBox.SelectedValue.ToString() == "GetValue")
                            {
                                if (Where.IsChecked == true)
                                {
                                    TBH.AddBoldText(".select $ where ");
                                }
                                else
                                {
                                    TBH.AddBoldText(".find");
                                }
                            }
                            else if (ControlActionComboBox.SelectedValue.ToString() == "SetValue")
                            {
                                TBH.AddBoldText(".update ");
                            }
                            else if (ControlActionComboBox.SelectedValue.ToString() == "DeleteRow")
                            { TBH.AddBoldText(".delete"); }
                            else if (ControlActionComboBox.SelectedValue.ToString() == "RowCount")
                            {
                                TBH.AddBoldText(".count");
                                TBH.AddText("}");
                                mActDSTblElem.ValueExp = TBH.GetText();

                                return;
                            }
                            else if (ControlActionComboBox.SelectedValue.ToString() == "AvailableRowCount")
                            {
                                TBH.AddBoldText(".find GINGER_USED= \"False\"");
                                TBH.AddText("}");
                                mActDSTblElem.ValueExp = TBH.GetText();

                                return;
                            }
                            else if (ControlActionComboBox.SelectedValue.ToString() == "MarkAsDone")
                            {
                                TBH.AddBoldText(".update GINGER_USED= \"True\" where");
                            }
                            else if (ControlActionComboBox.SelectedValue.ToString() == "DeleteAll")
                            {
                                TBH.AddBoldText(".delete");
                                TBH.AddText("}");
                                mActDSTblElem.ValueExp = TBH.GetText();

                                return;
                            }
                            else if (ControlActionComboBox.SelectedValue.ToString() == "MarkAllUnUsed")
                            {
                                TBH.AddBoldText(".update GINGER_USED= \"False\" ");
                                TBH.AddText("}");
                                mActDSTblElem.ValueExp = TBH.GetText();

                                return;
                            }
                            else if (ControlActionComboBox.SelectedValue.ToString() == "MarkAllUsed")
                            {
                                TBH.AddBoldText(".update GINGER_USED= \"True\"");
                                TBH.AddText("}");
                                mActDSTblElem.ValueExp = TBH.GetText();

                                return;
                            }

                            // next where / other condition
                            if (Customized.IsChecked == true)
                            {
                                if (NextAvailable.IsChecked == true)
                                {
                                    if (ControlActionComboBox.SelectedValue.ToString() == "SetValue")
                                    {
                                        if (cmbColumnValue.SelectedIndex != -1)
                                        {
                                            TBH.AddBoldText(cmbColumnValue.SelectedItem.ToString() + " = \"" + mActDSTblElem.Value + "\" where GINGER_USED =\"False\"");
                                        }
                                        else
                                        {
                                            string col = cmbColumnValue.Text;
                                            TBH.AddBoldText(col + " = \"" + mActDSTblElem.Value + "\" where GINGER_USED =\"False\"");
                                        }

                                    }
                                    else
                                    {
                                        TBH.AddBoldText(" GINGER_USED =\"False\"");
                                    }
                                }
                                else if (RowNum.IsChecked == true)
                                {
                                    if (ControlActionComboBox.SelectedValue.ToString() == "SetValue")
                                    {
                                        if (cmbColumnValue.SelectedIndex != -1)
                                        {
                                            TBH.AddBoldText(cmbColumnValue.SelectedItem.ToString() + " = \"" + mActDSTblElem.Value + "\"");
                                        }
                                        else
                                        {
                                            string col = cmbColumnValue.Text;
                                            TBH.AddBoldText(col + " = \"" + mActDSTblElem.Value + "\"");
                                        }
                                    }
                                }
                                else if (Where.IsChecked == true)
                                {
                                    mActDSTblElem.ByWhere = true;
                                    if (ControlActionComboBox.SelectedValue.ToString() == "SetValue")
                                    {
                                        if (cmbColumnValue.SelectedIndex != -1)
                                        {
                                            TBH.AddBoldText(cmbColumnValue.SelectedItem.ToString() + " = \"" + mActDSTblElem.Value + "\" where");
                                        }
                                        else
                                        {
                                            string col = cmbColumnValue.Text;
                                            TBH.AddBoldText(col + " = \"" + mActDSTblElem.Value + "\" where");
                                        }

                                    }
                                    for (int i = 0; i < mActDSTblElem.WhereConditions.Count; i++)
                                    {
                                        string wQuery = "";
                                        string wCond = mActDSTblElem.WhereConditions[i].wCondition.ToString().ToLower();
                                        string wColVal = mActDSTblElem.WhereConditions[i].wTableColumn.ToString().Trim();
                                        string wOpr = mActDSTblElem.WhereConditions[i].wOperator.ToString();
                                        string wRowVal = mActDSTblElem.WhereConditions[i].wValue.ToString();

                                        if (wCond == "empty")
                                        {
                                            wCond = "";
                                        }

                                        if (wOpr == "Equals")
                                        {
                                            if (wColVal == "GINGER_ID")
                                            {
                                                wQuery = wQuery + " " + wCond + " " + wColVal + " = " + wRowVal;
                                            }
                                            else
                                            {
                                                wQuery = wQuery + " " + wCond + " " + wColVal + " = \"" + wRowVal + "\"";
                                            }
                                        }
                                        else if (wOpr == "NotEquals")
                                        {
                                            if (wColVal == "GINGER_ID")
                                            {
                                                wQuery = wQuery + " " + wCond + " " + wColVal + " != " + wRowVal;
                                            }
                                            else
                                            {
                                                wQuery = wQuery + " " + wCond + " " + wColVal + " !=  \"" + wRowVal + "\"";
                                            }
                                        }
                                        else if (wOpr == "Contains")
                                            wQuery = wQuery + " " + wCond + " " + wColVal + " " + wOpr + "\"" + wRowVal + "\"";
                                        //else if (wOpr == "NotContains")
                                        //    wQuery = wQuery + " " + wCond + " " + wColVal + " NOT LIKE " + "'%" + wRowVal + "%'";
                                        //else if (wOpr == "StartsWith")
                                        //    wQuery = wQuery + " " + wCond + " " + wColVal + " LIKE '" + wRowVal + "%'";
                                        //else if (wOpr == "NotStartsWith")
                                        //    wQuery = wQuery + " " + wCond + " " + wColVal + " NOT LIKE '" + wRowVal + "%'";
                                        //else if (wOpr == "EndsWith")
                                        //    wQuery = wQuery + " " + wCond + " " + wColVal + " LIKE '%" + wRowVal + "'";
                                        //else if (wOpr == "NotEndsWith")
                                        //    wQuery = wQuery + " " + wCond + " " + wColVal + " NOT LIKE '%" + wRowVal + "'";
                                        //else if (wOpr == "IsNull")
                                        //    wQuery = wQuery + " " + wCond + " " + wColVal + " IS NULL";
                                        //else if (wOpr == "IsNotNull")
                                        //    wQuery = wQuery + " " + wCond + " " + wColVal + " IS NOT NULL";
                                        TBH.AddText(wQuery);
                                    }
                                }
                            }
                        }
                    }
                    else if (SelectedCell.IsChecked == true)
                    {
                        TBH.AddText(" Query QUERY=");
                        mActDSTblElem.BySelectedCell = true;
                        List<object> SelectedItemsList = new List<object>();
                        if (grdTableData.Grid.SelectedCells.Count == 0)
                        {
                            ErrorLabel.Content = "Please select a Valid Cell from Table Data for Current Identifier";
                            ErrorLabel.Foreground = Brushes.Red;
                        }
                        else
                        {
                            SelectedItemsList = grdTableData.Grid.SelectedCells.Cast<object>().ToList();

                            string selColName = ((DataGridCellInfo)SelectedItemsList[0]).Column.Header.ToString();
                            selColName = selColName.Replace("__", "_");
                            string SelCellGingerId = ((DataRowView)((DataGridCellInfo)SelectedItemsList[0]).Item).Row["GINGER_ID"].ToString();

                            TBH.AddText("db." + mDSTable.Name + ".select $." + selColName + " where GINGER_ID=\"" + SelCellGingerId + "\"");
                            mActDSTblElem.VarName = selColName;
                        }
                    }
                    if (mDSTable.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
                    {
                        mActDSTblElem.MarkUpdate = false;
                    }
                    TBH.AddText("}");
                    mActDSTblElem.ValueExp = TBH.GetText();
                }
            }
            
        }

        private void ColumnValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {                          
                UpdateValueExpression();
            mActDSTblElem.LocateColTitle = cmbColumnValue.Text;
            if (cmbColumnValue.SelectedValue != null)
            {
                mActDSTblElem.LocateColTitle = cmbColumnValue.SelectedValue.ToString();
            }
        }

        private void RowSelectorValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mActDSTblElem.LocateRowValue =(string) RowSelectorValue.SelectedValue;  
           
            UpdateValueExpression();                   
        }

        //private void WhereColumnTitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    UpdateValueExpression();
        //}

        private void WhereProperty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateValueExpression();
        }

        //private void WhereOperator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    UpdateValueExpression();
        //    if (WhereOperator.SelectedValue.ToString() == ActDSTableElement.eRunColOperator.IsNull.ToString() || WhereOperator.SelectedValue.ToString() == ActDSTableElement.eRunColOperator.IsNotNull.ToString())
        //    {
        //        WhereColumnValue.Visibility = Visibility.Collapsed;
        //        WhereColumnValueVE.Visibility = Visibility.Collapsed;
        //    }
        //    else
        //    {
        //        WhereColumnValue.Visibility = Visibility.Visible;
        //        WhereColumnValueVE.Visibility = Visibility.Visible;
        //    }


        //}



        private void ColumnValue_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UpdateValueExpression();

            mActDSTblElem.LocateColTitle = cmbColumnValue.Text;

        }

        private void RowSelectorValue_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            mActDSTblElem.LocateRowValue = RowSelectorValue.Text;
            UpdateValueExpression();
                    
        }

        //private void WhereColumnTitle_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        //{
            
        //        UpdateValueExpression();
           
        //}
        //private void WhereColumnValue_LostFocus(object sender, RoutedEventArgs e)
        //{

        //    UpdateValueExpression();

        //}
        private void QueryValue_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateValueExpression();          
        }

        private void RowSelectorValueVE_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mActDSTblElem, ActDSTableElement.Fields.LocateRowValue, Context.GetAsContext(mActDSTblElem.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            RowSelectorValue.Text = mActDSTblElem.LocateRowValue;           
        }

        //private void WhereColumnValueVE_Click(object sender, RoutedEventArgs e)
        //{
        //    ValueExpressionEditorPage w = new ValueExpressionEditorPage(mActDSTblElem, ActDSTableElement.Fields.WhereColumnValue);
        //    w.ShowAsWindow(eWindowShowStyle.Dialog);
        //    WhereColumnValue.Text = mActDSTblElem.WhereColumnValue;
        //}
        private void QueryValVE_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mActDSTblElem, ActDSTableElement.Fields.QueryValue, Context.GetAsContext(mActDSTblElem.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            QueryVal.Text = mActDSTblElem.QueryValue;
        }

        public string VE
        {
            get
            {
                if (okClicked)
                {
                    return mActDSTblElem.ValueExp;
                }
                else
                {
                    return "";
                }
            }
        }

        private void QueryVal_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateValueExpression();
        }

        //private void WhereColumnValue_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    UpdateValueExpression();
        //}
        
        private void SetGridData()
        {
            if (mDSTable == null)
                return;
                mDSTable.DataTable = mDSTable.DSC.GetTable(mDSTable.Name);
            
            grdTableData.Grid.SetBinding(ItemsControl.ItemsSourceProperty, new Binding
            {
                Source = mDSTable.DataTable
            });
            grdTableData.UseGridWithDataTableAsSource(mDSTable.DataTable,false);
        }
        private void SetGridView()
        {
            if (mDSTable == null)
                return;

            //Set the grid name
            grdTableData.Title = "'" + mDSTable.Name + "' Table Data";
            grdTableData.ShowViewCombo = Visibility.Collapsed;
            //this.DataContext = new GridViewDef(GridViewDef.DefaultViewName);
            List<string> mColumnNames = mDSTable.DSC.GetColumnList(mDSTable.Name);
            int iColIndex = mColumnNames.Count - 1;

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            foreach (string colName in mColumnNames)
            {
                string colHeader = colName.Replace("_", "__");
                if (colName == "GINGER_ID")
                {
                    view.GridColsView.Add(new GridColView() { Field = colName, Header = colHeader, Order = 0, WidthWeight = 10, BindingMode = BindingMode.OneWay });
                }
                else if (colName == "GINGER_LAST_UPDATE_DATETIME" || colName == "GINGER_LAST_UPDATED_BY")
                {
                    view.GridColsView.Add(new GridColView() { Field = colName, Header = colHeader, WidthWeight = 20, BindingMode = BindingMode.OneWay });
                }
                else if (colName == "GINGER_USED")
                {
                    view.GridColsView.Add(new GridColView() { Field = colName, Header = colHeader, ReadOnly = true, WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox });
                }
                else
                {
                    view.GridColsView.Add(new GridColView() { Field = colName, Header = colHeader, WidthWeight = 30 });
                }
            }  
            if(bGridViewSet == false)
            {
                grdTableData.SetAllColumnsDefaultView(view);
                grdTableData.InitViewItems();
                bGridViewSet = true;
            }
            else
            {
                grdTableData.updateAndSelectCustomView(view);
            }

            foreach (DataGridColumn sCol in grdTableData.grdMain.Columns)
            {
                if (sCol.Header.ToString() == "GINGER__USED")
                {
                    sCol.DisplayIndex = 1;
                }
                if (sCol.Header.ToString() == "GINGER__LAST__UPDATED__BY" || sCol.Header.ToString() == "GINGER__LAST__UPDATE__DATETIME")
                {
                    sCol.DisplayIndex = iColIndex;
                    iColIndex--;
                }
            }            
        }

        private void SetConditionGridData()
        {
            grdCondition.DataSourceList=mActDSTblElem.WhereConditions;
        }
        private void SetConditionGridView()
        {
            //Set the grid name
            //grdCondition.Title = "Where";
            grdCondition.SetTitleLightStyle = true;
            grdCondition.ShowViewCombo = Visibility.Collapsed;

            List<ComboEnumItem> lstCond = GingerCore.General.GetEnumValuesForCombo(typeof(ActDSConditon.eCondition));
            List<ComboEnumItem> lstOper = GingerCore.General.GetEnumValuesForCombo(typeof(ActDSConditon.eOperator));

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = ActDSConditon.Fields.wCondition, Header = "And/Or", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(ActDSConditon.Fields.PossibleCondValues, ActDSConditon.Fields.wCondition) });
            //view.GridColsView.Add(new GridColView() { Field = ActDSConditon.Fields.wCondition, Header = "And/Or", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = lstCond });
            view.GridColsView.Add(new GridColView() { Field = ActDSConditon.Fields.wTableColumn, Header = "Column", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(ActDSConditon.Fields.PossibleColumnValues, ActDSConditon.Fields.wTableColumn) });
            view.GridColsView.Add(new GridColView() { Field = ActDSConditon.Fields.wOperator, Header = "Operator", WidthWeight = 10,StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList= lstOper });            
            view.GridColsView.Add(new GridColView() { Field = ActDSConditon.Fields.wValue, Header = "Value", WidthWeight = 30});
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 5, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.WhereGrid.Resources["ValueExpressionButton"] });

            grdCondition.SetAllColumnsDefaultView(view);
            grdCondition.InitViewItems();
        }

        private void SelectedCell_Checked(object sender, RoutedEventArgs e)
        {
            if (QueryPanel != null)
                QueryPanel.Visibility = Visibility.Collapsed;
            if (CustomizedPanel != null)
                CustomizedPanel.Visibility = Visibility.Collapsed;

            if (MultiRows != null)
            {
                MultiRows.IsChecked = false;
                MultiRows.IsEnabled = false;
            }
            //IdentifierRow.Height = new GridLength(108);
            UpdateValueExpression();
            SetIdentifierHeight();
        }

        private void grdTableData_CellChangedEvent(object sender, EventArgs e)
        {
            if (mDSTable.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
                return;

            if (ControlActionComboBox.SelectedValue.ToString() == "SetValue")
            {
                if (grdTableData.Grid.SelectedCells.Count > 0)
                {
                    string sSelCol = grdTableData.Grid.SelectedCells[0].Column.Header.ToString();
                    if (sSelCol == "GINGER__ID" || sSelCol == "GINGER__LAST__UPDATE__DATETIME" || sSelCol == "GINGER__LAST__UPDATED__BY")
                    {
                        grdTableData.Grid.SelectedCells.Clear();
                    }
                }
            }
            
            UpdateValueExpression();
        }
        private void SetIdentifierHeight()
        {
            if (mDSTable == null|| ControlActionComboBox.SelectedValue == null)
                return;

            if (ControlActionComboBox.SelectedValue.ToString() == "DeleteAll" || ControlActionComboBox.SelectedValue.ToString() == "RowCount" || ControlActionComboBox.SelectedValue.ToString() == "AvailableRowCount" || ControlActionComboBox.SelectedValue.ToString() == "MarkAllUnUsed" || ControlActionComboBox.SelectedValue.ToString() == "MarkAllUsed")
            {   
                IdentifierRow.Height = new GridLength(0);                
                return;
            }
            if(ControlActionComboBox.SelectedValue.ToString() == "ExportToExcel")
            {               
                IdentifierRow.Height = new GridLength(130);
                return;   
            }           
                
            if (mDSTable.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
                IdentifierRow.Height = new GridLength(100);
            else
            {                
                if (ByQuery.IsChecked == true)
                    IdentifierRow.Height = new GridLength(173);
                else if(SelectedCell.IsChecked == true)
                    IdentifierRow.Height = new GridLength(108);
                else
                {
                    if ((RowNum != null && RowNum.IsChecked == true) || (NextAvailable != null && NextAvailable.IsChecked == true))
                        IdentifierRow.Height = new GridLength(240);//240
                    else
                        IdentifierRow.Height = new GridLength(510);//310
                }
                //IdentifierRow.Height = new GridLength(IdentifierRow.Height.Value - ColIden.Height.Value);
                if (ControlActionComboBox.SelectedValue.ToString() == "MarkAsDone" || ControlActionComboBox.SelectedValue.ToString() == "DeleteRow")
                {   
                    ColIden.Height = new GridLength(0);
                }
                else
                {
                    ColIden.Height = new GridLength(45);
                    //IdentifierRow.Height = new GridLength(IdentifierRow.Height.Value + 45);                                        
                }

                if (ControlActionComboBox.SelectedValue.ToString() == ActDSTableElement.eControlAction.SetValue.ToString() && eBaseWindow == BaseWindow.ActEditPage)
                    ValRow.Height = new GridLength(43, GridUnitType.Star);
                else
                    ValRow.Height = new GridLength(0);

            }
        }
        private void MarkAsDone_Checked(object sender, RoutedEventArgs e)
        {
            UpdateValueExpression();
            mActDSTblElem.MarkUpdate = true;
        }

        private void MarkAsDone_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateValueExpression();
            mActDSTblElem.MarkUpdate = false;
        }

        private void cmbDataSourceName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            foreach(DataSourceBase ds in mDSList)
            {
                if(ds.Name == cmbDataSourceName.SelectedValue.ToString())
                {
                    mDataSourceName = cmbDataSourceName.SelectedValue.ToString();
                    //if (ds.FilePath.StartsWith("~"))
                    //{
                    //    ds.FileFullPath = ds.FilePath.Replace(@"~\","").Replace("~", "");
                    //    ds.FileFullPath = System.IO.Path.Combine( WorkSpace.Instance.Solution.Folder, ds.FileFullPath);
                    //}
                    ds.FileFullPath = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(ds.FilePath);

                    List<string> dsTableNames = new List<string>();
                    mDSTableList = ds.GetTablesList();
                    if (mDSTableList != null)
                    {
                        foreach (DataSourceTable dst in mDSTableList)
                            dsTableNames.Add(dst.Name);
                    }                    
                    GingerCore.General.FillComboFromList(cmbDataSourceTableName, dsTableNames);
                    cmbDataSourceTableName.SelectedIndex = 0;
                    if (mDSTableList != null && mDSTableList.Count > 0)
                        mDSTable = mDSTableList[0];
                    else
                        mDSTable = null;
                    break;
                }                    
            }            
        }
        
        private void cmbDataSourceTableName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDataSourceTableName == null || cmbDataSourceTableName.Items.Count==0)
                return;
            foreach (DataSourceTable dst in mDSTableList)
            {
                if (dst.Name == cmbDataSourceTableName.SelectedValue.ToString())
                {
                    mDSTable =dst;                    
                    break;
                }
            }
            GetTableDetails();
            SetTableDetails();
            TableDataExpander.IsExpanded = false;
            bTableDataLoaded = false;
            SelectedCell.IsChecked = false;
            SelectedCell.IsEnabled = false;
            Customized.IsChecked = true;

            if (NextAvailable.Visibility == Visibility.Visible)
            {
                if (mActDSTblElem.ByNextAvailable)
                {
                    NextAvailable.IsChecked = true;
                }
                else if (mActDSTblElem.ByRowNum)
                {
                    RowNum.IsChecked = true;
                    
                }
                else if (mActDSTblElem.ByWhere)
                {
                    Where.IsChecked = true;
                }
            }
            else if (mActDSTblElem.ByWhere)
            {
                Where.IsChecked = true;
            }
            else if (mActDSTblElem.ByQuery)
            {
                ByQuery.IsChecked = true;
            }
           

            HandleControlActionChange();
        }
        private void SetTableDetails()
        {
            SetIdentifierHeight();

            if (mDSTable == null)
                return;
            SetTableActions();

            if (ControlActionComboBox.SelectedValue != null && ControlActionComboBox.SelectedValue.ToString() == ActDSTableElement.eControlAction.ExportToExcel.ToString())
            {                
                ExcelGrid.Visibility = Visibility.Visible;
                KeyGrid.Visibility = Visibility.Collapsed;
                CustomizedGrid.Visibility = Visibility.Collapsed;
                MarkRowPanel.Visibility = Visibility.Collapsed;
                ExpTableCell.Text = "Excel Details";
                return;
            }           

            ExpTableCell.Text = "Table Cell Identifier";
            ExcelGrid.Visibility = Visibility.Collapsed;
            if (mDSTable.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
            {                
                MarkRowPanel.Visibility = Visibility.Collapsed;
                CustomizedGrid.Visibility = Visibility.Collapsed;
                KeyGrid.Visibility = Visibility.Visible;

                DataTable dTable = mDSTable.DSC.GetKeyName(mDSTable.Name);
                List<string> mKeyNames = new List<string>();
                if (dTable != null)
                {
                    for (int iRow = 0; iRow < dTable.Rows.Count; iRow++)
                    {
                        mKeyNames.Add(dTable.Rows[iRow].ItemArray[0].ToString());
                    }
                }
                if (mKeyNames != null)
                {
                    GingerCore.General.FillComboFromList(cmbKeyName, mKeyNames);
                    cmbKeyName.SelectedIndex = 0;
                }
                MarkRowPanel.Visibility = Visibility.Collapsed;
                MarkRow.Height = new GridLength(0);
            }
            else
            {
                SetComponents();
                if (mColNames != null && mColNames.Contains("GINGER_USED"))
                {
                    GingerCore.General.AddComboItem(ControlActionComboBox, ActDSTableElement.eControlAction.MarkAsDone);
                    GingerCore.General.AddComboItem(ControlActionComboBox, ActDSTableElement.eControlAction.MarkAllUsed);
                    GingerCore.General.AddComboItem(ControlActionComboBox, ActDSTableElement.eControlAction.MarkAllUnUsed);

                    if (MarkRowPanel.Visibility == Visibility.Collapsed && ControlActionComboBox.SelectedValue != null && !ControlActionComboBox.SelectedValue.ToString().Contains("All") && !ControlActionComboBox.SelectedValue.ToString().Contains("RowCount"))
                    {                       
                        MarkRowPanel.Visibility = Visibility.Visible;                        
                        IdentifierRow.Height = new GridLength(IdentifierRow.Height.Value + 25);
                    }
                }
                else
                {
                    GingerCore.General.RemoveComboItem(ControlActionComboBox, ActDSTableElement.eControlAction.MarkAsDone);
                    GingerCore.General.RemoveComboItem(ControlActionComboBox, ActDSTableElement.eControlAction.MarkAllUsed);
                    GingerCore.General.RemoveComboItem(ControlActionComboBox, ActDSTableElement.eControlAction.MarkAllUnUsed);

                    if (MarkRowPanel.Visibility == Visibility.Visible)
                    {                        
                        MarkRowPanel.Visibility = Visibility.Collapsed;                        
                        IdentifierRow.Height = new GridLength(IdentifierRow.Height.Value - 25);
                    }
                }

                KeyGrid.Visibility = Visibility.Collapsed;
                CustomizedGrid.Visibility = Visibility.Visible;
            }             
        }
        private void SetTableActions()
        {
            if (mDSTable.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
            {
                if (ControlActionComboBox.SelectedValue != null && ControlActionComboBox.SelectedValue.ToString().StartsWith("Mark"))
                    ControlActionComboBox.SelectedValue = ActDSTableElement.eControlAction.GetValue;

                //IdentifierRow.Height = new GridLength(100);

                GingerCore.General.RemoveComboItem(ControlActionComboBox, ActDSTableElement.eControlAction.MarkAsDone);
                GingerCore.General.RemoveComboItem(ControlActionComboBox, ActDSTableElement.eControlAction.MarkAllUnUsed);
                GingerCore.General.RemoveComboItem(ControlActionComboBox, ActDSTableElement.eControlAction.MarkAllUsed);
                GingerCore.General.RemoveComboItem(ControlActionComboBox, ActDSTableElement.eControlAction.AvailableRowCount);
            }
            else
            {
                SetComponents();
                if (mColNames != null && mColNames.Contains("GINGER_USED"))
                {
                    GingerCore.General.AddComboItem(ControlActionComboBox, ActDSTableElement.eControlAction.MarkAsDone);
                    GingerCore.General.AddComboItem(ControlActionComboBox, ActDSTableElement.eControlAction.MarkAllUsed);
                    GingerCore.General.AddComboItem(ControlActionComboBox, ActDSTableElement.eControlAction.MarkAllUnUsed);
                    GingerCore.General.AddComboItem(ControlActionComboBox, ActDSTableElement.eControlAction.AvailableRowCount);
                }
                else
                {
                    GingerCore.General.RemoveComboItem(ControlActionComboBox, ActDSTableElement.eControlAction.MarkAsDone);
                    GingerCore.General.RemoveComboItem(ControlActionComboBox, ActDSTableElement.eControlAction.MarkAllUsed);
                    GingerCore.General.RemoveComboItem(ControlActionComboBox, ActDSTableElement.eControlAction.MarkAllUnUsed);
                    GingerCore.General.RemoveComboItem(ControlActionComboBox, ActDSTableElement.eControlAction.AvailableRowCount);
                }
            }
        }

        private void SetIdentifierDetails()
        {
            SetIdentifierHeight();

            if (mDSTable == null)
                return;

            if (ControlActionComboBox.SelectedValue.ToString() == ActDSTableElement.eControlAction.ExportToExcel.ToString())
            {
                ExcelGrid.Visibility = Visibility.Visible;
                KeyGrid.Visibility = Visibility.Collapsed;
                CustomizedGrid.Visibility = Visibility.Collapsed;
                MarkRowPanel.Visibility = Visibility.Collapsed;
                ExpTableCell.Text = "Excel Details";                
                return;
            }
            else
            {
                ExpTableCell.Text = "Table Cell Identifier";
                ExcelGrid.Visibility = Visibility.Collapsed;

                if (mDSTable.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
                {
                    MarkRowPanel.Visibility = Visibility.Collapsed;
                    CustomizedGrid.Visibility = Visibility.Collapsed;
                    KeyGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    KeyGrid.Visibility = Visibility.Collapsed;
                    CustomizedGrid.Visibility = Visibility.Visible;
                    MarkRowPanel.Visibility = Visibility.Visible;
                }
            }           
        }
        private void HandleControlActionChange()
        {
            if (ControlActionComboBox.SelectedValue == null)
                return;            

            SetIdentifierHeight();
            setColValList();
            SetIdentifierDetails();

            if (ControlActionComboBox.SelectedValue.ToString() == ActDSTableElement.eControlAction.MarkAsDone.ToString())
            {
                MarkAsDone.IsChecked = true;
                MarkAsDone.IsEnabled = false;

                ColIden.Height = new GridLength(0);
                cmbColumnValue.Visibility = Visibility.Collapsed;
            }
            else if (ControlActionComboBox.SelectedValue.ToString() == ActDSTableElement.eControlAction.DeleteRow.ToString())
            {
                MarkAsDone.IsChecked = false;
                MarkAsDone.IsEnabled = false;
                
                ColIden.Height = new GridLength(0);
                cmbColumnValue.Visibility = Visibility.Collapsed;
            }
            else if (ControlActionComboBox.SelectedValue.ToString().Contains("All") || ControlActionComboBox.SelectedValue.ToString() == ActDSTableElement.eControlAction.RowCount.ToString()  || ControlActionComboBox.SelectedValue.ToString() == ActDSTableElement.eControlAction.AvailableRowCount.ToString())
            {
                MarkRowPanel.Visibility = Visibility.Collapsed;
            }
            else if (ControlActionComboBox.SelectedValue.ToString() == ActDSTableElement.eControlAction.ExportToExcel.ToString())
            {                
            }
            else
            {   
                MarkAsDone.IsEnabled = true;
                ColIden.Height = new GridLength(45);
                cmbColumnValue.Visibility = Visibility.Visible;
            }

            //Handle Multi Row for GetValue
            if(ControlActionComboBox.SelectedValue.ToString() == ActDSTableElement.eControlAction.GetValue.ToString())
            {
                MultiRows.IsChecked = false;
                MultiRows.Visibility = Visibility.Collapsed;
            }
            else
                MultiRows.Visibility = Visibility.Visible;

            //Show Value Panel only for Set Value
            if (ControlActionComboBox.SelectedValue.ToString() == ActDSTableElement.eControlAction.SetValue.ToString())
            {
                if(eBaseWindow == BaseWindow.ActEditPage)
                {
                    ValueLabel.Visibility = Visibility.Visible;
                    ValueUC.Visibility = Visibility.Visible;
                    ValRow.Height = new GridLength(43, GridUnitType.Star);
                }                
                if (grdTableData.Grid.SelectedCells.Count > 0)
                {
                    string sSelCol = grdTableData.Grid.SelectedCells[0].Column.Header.ToString();
                    if (sSelCol == "GINGER__ID" || sSelCol == "GINGER__LAST__UPDATE__DATETIME" || sSelCol == "GINGER__LAST__UPDATED__BY")
                    {
                        grdTableData.Grid.SelectedCells.Clear();
                    }
                }
            }            
            else
            {
                ValueLabel.Visibility = Visibility.Collapsed;
                ValueUC.Visibility = Visibility.Collapsed;
                ValRow.Height = new GridLength(0);
                if (bTableDataLoaded == true)
                {
                    SetGridView();
                }
            }
            UpdateValueExpression();
        }
        private void ControlActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            HandleControlActionChange();
        }

        private void TableDataExpander_Expanded(object sender, RoutedEventArgs e)
        {
            TableRow.Height = new GridLength(220);
            //TableDataExpander.IsExpanded = true;
            if(bTableDataLoaded == false)
            {
                SelectedCell.IsEnabled = true;
                
                SetGridView();
                
                grdTableData.Grid.SelectionMode = DataGridSelectionMode.Single;

                //if (mDSTable.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
                //    grdTableData.Grid.SelectionUnit = DataGridSelectionUnit.FullRow;
                //else
                    grdTableData.Grid.SelectionUnit = DataGridSelectionUnit.Cell;

                grdTableData.Grid.SelectedCellsChanged += grdTableData_CellChangedEvent;               

                SetGridData();
                //SelectedCell.IsChecked = true;
                UpdateValueExpression();
                bTableDataLoaded = true;
            }            
        }

        private void TableDataExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            TableRow.Height = new GridLength(35);
        }

        private void IdentiferExpander_Expanded(object sender, RoutedEventArgs e)
        {
            //IdentifierRow.Height = new GridLength(310);
            SetIdentifierHeight();
        }

        private void IdentiferExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            IdentifierRow.Height = new GridLength(35);
        }

        private void cmbKeyName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateValueExpression();
        }

        private void cmbKeyName_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UpdateValueExpression();
        }

        //private void UpdateButton_Click(object sender, RoutedEventArgs e)
        //{
        //    string txt = mSelectedContentArgs.TextEditor.Text.Substring(0, mSelectedContentArgs.StartPos);
        //    txt += mActDSTblElem.ValueExp;
        //    txt += mSelectedContentArgs.TextEditor.Text.Substring(mSelectedContentArgs.EndPos + 1);
        //    mSelectedContentArgs.TextEditor.Text = txt;
        //}
        public void UpdateContent()
        {
            string txt = mSelectedContentArgs.TextEditor.Text.Substring(0, mSelectedContentArgs.StartPos);
            txt += mActDSTblElem.ValueExp;
            txt += mSelectedContentArgs.TextEditor.Text.Substring(mSelectedContentArgs.EndPos + 1);
            mSelectedContentArgs.TextEditor.Text = txt;
        }

        //private void UpdateButton_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    if(UpdateButton.Visibility == Visibility.Visible)
        //        UpdateVERow.Height = new GridLength(30);
        //    else
        //        UpdateVERow.Height = new GridLength(0);
        //}

        private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            
        }

        private void grdCondition_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void GridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActDSConditon ADSC = (ActDSConditon)grdCondition.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(ADSC, ActDSConditon.Fields.wValue, Context.GetAsContext(mActDSTblElem.Context));
            VEEW.ShowAsWindow();
        }

        private void cmbKeyName_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateValueExpression();
        }

        private void MultiRows_Checked(object sender, RoutedEventArgs e)
        {
            UpdateValueExpression();
        }

        private void MultiRows_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateValueExpression();
        }

        private void NextAvailable_Unchecked(object sender, RoutedEventArgs e)
        {
            mActDSTblElem.ByNextAvailable = false;
        }

        private void ByQuery_Unchecked(object sender, RoutedEventArgs e)
        {
            mActDSTblElem.ByQuery = false;
            UpdateValueExpression();
        }

    }
}
