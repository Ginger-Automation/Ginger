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
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Helpers;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Ginger.Variables
{
    /// <summary>
    /// Interaction logic for ActivityActionsDependenciesPage.xaml
    /// </summary>
    public partial class VariablesDependenciesPage : Page
    {
        GenericWindow _pageGenericWin = null;
        object mParentObject;
        eDependedItemsType mDepededItemType;
        DataTable mDependsDT;
        List<VariableBase> mParentListVars;        
        bool mItemsVarsDataIsValid = true;
        bool isHorizentalScrollAllowed = false;

        private enum eDependedItemsType
        {
            Actions,Activities
        }

        private enum eAutoCheckArea
        {
            Table,Column,Row
        }

        public VariablesDependenciesPage(object parentObject)
        {
            InitializeComponent();

            mParentObject = parentObject;
            ((RepositoryItemBase)mParentObject).SaveBackup();

            if (mParentObject is Activity)
            {
                mDepededItemType = eDependedItemsType.Actions;
                chkBoxEnableDisableDepControl.Content = "Enable Actions-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies Control";
                App.ObjFieldBinding(chkBoxEnableDisableDepControl, CheckBox.IsCheckedProperty, (Activity)mParentObject, Activity.Fields.EnableActionsVariablesDependenciesControl);                
                infoImage.ToolTip = "In case the Actions-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " dependencies control is enabled then a specific Action will be executed in run time only if it was mapped (checked) to a " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " value which was selected in run time." + System.Environment.NewLine +
                                  "The mapping is been done between the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " Actions and " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " from type Selection List.";
            }
            else if (mParentObject is BusinessFlow)
            {
                mDepededItemType = eDependedItemsType.Activities;
                chkBoxEnableDisableDepControl.Content = "Enable " + GingerDicser.GetTermResValue(eTermResKey.Activities) + "-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies Control";
                App.ObjFieldBinding(chkBoxEnableDisableDepControl, CheckBox.IsCheckedProperty, (BusinessFlow)mParentObject, BusinessFlow.Fields.EnableActivitiesVariablesDependenciesControl);
                infoImage.ToolTip = "In case the " + GingerDicser.GetTermResValue(eTermResKey.Activities) + "-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " dependencies control is enabled then a specific " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " will be executed in run time only if it was mapped (checked) to a " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " value which was selected in run time." + System.Environment.NewLine +
                                             "The mapping is been done between the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " and " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " from type Selection List.";
            }
            

            mDependsDT = new DataTable();
            SetDependenciesGrid();
            SetDependenciesHelper();
        }

        private void SetDependenciesGrid()
        {
            try
            {
                if (chkBoxEnableDisableDepControl.IsChecked == true)
                    grdDependencies.IsEnabled = true;

                //Set title
                switch (mDepededItemType)
                {
                    case (eDependedItemsType.Actions):
                        grdDependencies.Title = "Actions-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies";
                        break;
                    case (eDependedItemsType.Activities):
                        grdDependencies.Title = GingerDicser.GetTermResValue(eTermResKey.Activities) + "-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies";
                        break;
                }

                //set tool bar tools
                grdDependencies.AddToolbarTool("@CheckAllTable_16x16.png", "Check All Table", new RoutedEventHandler(CheckAllTable));
                grdDependencies.AddToolbarTool("@UnCheckAllTable_16x16.png", "Un-Check All Table", new RoutedEventHandler(UnCheckAllTable));
                grdDependencies.AddToolbarTool("@CheckAllColumn_16x16.png", "Check All Column", new RoutedEventHandler(CheckAllColumn));
                grdDependencies.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Un-Check All Column", new RoutedEventHandler(UnCheckAllColumn));
                grdDependencies.AddToolbarTool("@CheckAllRow_16x16.png", "Check All Row", new RoutedEventHandler(CheckAllRow));
                grdDependencies.AddToolbarTool("@UnCheckAllRow_16x16.png", "Un-Check All Row", new RoutedEventHandler(UnCheckAllRow));
                grdDependencies.AddToolbarTool("@HorizentalScroller_16x16.png", "Allow\\Un-Allow Horizontal Scroll", new RoutedEventHandler(AllowUnAllowHorizentalScroll));
                //set columns
                switch (mDepededItemType)
                {
                    case (eDependedItemsType.Actions):
                        DataColumn actionDesCol = new DataColumn();
                        actionDesCol.ColumnName = "Action Description";
                        actionDesCol.DataType = typeof(string);
                        actionDesCol.ReadOnly = true;
                        mDependsDT.Columns.Add(actionDesCol);
                        DataColumn actionTypeCol = new DataColumn();
                        actionTypeCol.ColumnName = "Action Type";
                        actionTypeCol.DataType = typeof(string);
                        actionTypeCol.ReadOnly = true;
                        mDependsDT.Columns.Add(actionTypeCol);
                        break;
                    case (eDependedItemsType.Activities):
                        DataColumn col1 = new DataColumn();
                        col1.ColumnName = GingerDicser.GetTermResValue(eTermResKey.Activity) + " Name";
                        col1.DataType = typeof(string);
                        col1.ReadOnly = true;
                        mDependsDT.Columns.Add(col1);
                        DataColumn col2 = new DataColumn();
                        col2.ColumnName = GingerDicser.GetTermResValue(eTermResKey.Activity) + " Description";
                        col2.DataType = typeof(string);
                        col2.ReadOnly = true;
                        mDependsDT.Columns.Add(col2);
                        break;
                }


                //get all Parent Selection List variables and create columns for them
                switch (mDepededItemType)
                {
                    case (eDependedItemsType.Actions):
                        mParentListVars = ((Activity)mParentObject).Variables.Where(v => v.GetType() == typeof(VariableSelectionList)).ToList();
                        break;
                    case (eDependedItemsType.Activities):
                        mParentListVars = ((BusinessFlow)mParentObject).Variables.Where(v => v.GetType() == typeof(VariableSelectionList)).ToList();
                        break;
                }

                if (mParentListVars != null && mParentListVars.Count > 0)
                {
                    List<string> addedColumns = new List<string>();
                    foreach (VariableBase listVar in mParentListVars)
                    {
                        foreach (OptionalValue varOptionalVal in ((VariableSelectionList)listVar).OptionalValuesList)
                        {
                            DataColumn varValueCol = new DataColumn();
                            varValueCol.ColumnName = listVar.Name + ":" + System.Environment.NewLine + varOptionalVal.Value;
                            varValueCol.DataType = typeof(bool);
                            if (addedColumns.Contains(listVar.Name + ">" + varOptionalVal.Value) == false)
                            {
                                mDependsDT.Columns.Add(varValueCol);
                                addedColumns.Add(listVar.Name + ">" + varOptionalVal.Value);
                            }
                            else
                            {
                                Reporter.ToUser(eUserMsgKeys.DuplicateVariable, listVar.Name, varOptionalVal.Value);
                                mParentListVars.Clear();
                                mDependsDT.Clear();
                                grdDependencies.UseGridWithDataTableAsSource(mDependsDT);
                                DependenciesGridSection.Visibility = System.Windows.Visibility.Collapsed;
                                DependenciesHelperSection.Visibility = System.Windows.Visibility.Collapsed;
                                mItemsVarsDataIsValid = false;
                                return;
                            }
                        }
                    }
                }

                //set row for each depended item and populate the dependencies data
                switch (mDepededItemType)
                {
                    case (eDependedItemsType.Actions):
                        if (((Activity)mParentObject).Acts.Count > 0)
                        {
                            foreach (Act act in ((Activity)mParentObject).Acts)
                            {
                                mDependsDT.Rows.Add(act.Description, act.ActionType);
                                DataRow actRow = mDependsDT.Rows[mDependsDT.Rows.Count - 1];

                                //load last saved dependencies for this action
                                int colIndex = 2;
                                foreach (VariableBase listVar in mParentListVars)
                                {
                                    VariableDependency varDep = null;
                                    if (act.VariablesDependencies != null)
                                        varDep = act.VariablesDependencies.Where(avd => avd.VariableName == listVar.Name && avd.VariableGuid == listVar.Guid).FirstOrDefault();
                                    if (varDep == null)
                                        varDep = act.VariablesDependencies.Where(avd => avd.VariableGuid == listVar.Guid).FirstOrDefault();
                                    foreach (OptionalValue varOptionalVal in ((VariableSelectionList)listVar).OptionalValuesList)
                                    {
                                        if (varDep != null)
                                        {
                                            if (varDep.VariableValues.Contains(varOptionalVal.Value))
                                                actRow[colIndex] = true;
                                            else
                                                actRow[colIndex] = false;
                                        }
                                        else
                                            actRow[colIndex] = false;
                                        colIndex++;
                                    }
                                }
                            }
                        }
                        break;
                    case (eDependedItemsType.Activities):
                        if (((BusinessFlow)mParentObject).Activities.Count > 0)
                        {
                            foreach (Activity activity in ((BusinessFlow)mParentObject).Activities)
                            {
                                mDependsDT.Rows.Add(activity.ActivityName, activity.Description);
                                DataRow actRow = mDependsDT.Rows[mDependsDT.Rows.Count - 1];

                                //load last saved dependencies for this activity
                                int colIndex = 2;
                                foreach (VariableBase listVar in mParentListVars)
                                {
                                    VariableDependency varDep = null;
                                    if (activity.VariablesDependencies != null)
                                        varDep = activity.VariablesDependencies.Where(avd => avd.VariableName == listVar.Name && avd.VariableGuid == listVar.Guid).FirstOrDefault();
                                    if (varDep == null)
                                        varDep = activity.VariablesDependencies.Where(avd => avd.VariableGuid == listVar.Guid).FirstOrDefault();
                                    foreach (OptionalValue varOptionalVal in ((VariableSelectionList)listVar).OptionalValuesList)
                                    {
                                        if (varDep != null)
                                        {
                                            if (varDep.VariableValues.Contains(varOptionalVal.Value))
                                                actRow[colIndex] = true;
                                            else
                                                actRow[colIndex] = false;
                                        }
                                        else
                                            actRow[colIndex] = false;
                                        colIndex++;
                                    }
                                }
                            }
                        }
                        break;
                }


                //combin with dataGrid                
                grdDependencies.UseGridWithDataTableAsSource(mDependsDT);
                grdDependencies.Grid.Loaded += grdMain_Loaded;
                grdDependencies.Grid.CellEditEnding += grdMain_CellEditEnding;
                grdDependencies.Grid.CurrentCellChanged += grdMain_CurrentCellChanged;                
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to load the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " Actions-Variables dependencies grid data", ex);
                Reporter.ToUser(eUserMsgKeys.ActionsDependenciesLoadFailed, ex.Message);
            }
        }

        private void grdMain_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //design columns                
                grdDependencies.grdMain.ColumnHeaderHeight = 50;
                try
                {
                    int colorIndx = 0;
                    int columnIndx = 2;
                    string[] variableColumnBackgroundColor = new string[] { "#FFFFCC", "#E5FFCC", "#CCE5FF", "#CCCCFF", "#FFCCFF", "#FFCC99", "#CCFFFF", "#FF99CC" };
                    foreach (VariableBase var in mParentListVars)
                    {
                        for (int indx = 0; indx < ((VariableSelectionList)var).OptionalValuesList.Count; indx++)
                        {
                            Style colStyle = new System.Windows.Style(typeof(DataGridColumnHeader));                          
                            SolidColorBrush backgroundColor = (SolidColorBrush)new BrushConverter().ConvertFromString(variableColumnBackgroundColor[colorIndx % 8]);
                            colStyle.Setters.Add(new Setter(DataGridColumnHeader.BackgroundProperty, backgroundColor));
                            SolidColorBrush foregroundColor = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("@Skin1_ColorA")).ToString());
                            colStyle.Setters.Add(new Setter(DataGridColumnHeader.ForegroundProperty, foregroundColor));
                            colStyle.Setters.Add(new Setter(DataGridColumnHeader.FontWeightProperty, FontWeights.Bold));
                            colStyle.Setters.Add(new Setter(DataGridColumnHeader.BorderThicknessProperty, new Thickness(0.5, 0.5, 0.5, 0.5)));
                            colStyle.Setters.Add(new Setter(DataGridColumnHeader.BorderBrushProperty, foregroundColor));
                            colStyle.Setters.Add(new Setter(DataGridColumnHeader.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
                            colStyle.Setters.Add(new EventSetter(Button.ClickEvent, new RoutedEventHandler(ColumnWasClicked)));

                            grdDependencies.grdMain.Columns[columnIndx].HeaderStyle = colStyle;
                            columnIndx++;
                        }
                        colorIndx++;
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to design the " + mDepededItemType.ToString() + "-Variables dependencies grid columns", ex);
                }
                grdDependencies.SetGridColumnsWidth();//fix columns width

                //design rows
                grdDependencies.grdMain.SelectionUnit = DataGridSelectionUnit.CellOrRowHeader;

                //design checkbox cells
                if (grdDependencies.grdMain.Columns.Count >= 2)
                {
                    for (int i = 0; i <= grdDependencies.grdMain.Columns.Count; i++)
                        if (grdDependencies.grdMain.Columns[i].Visibility == System.Windows.Visibility.Visible && i > 1)
                            ((DataGridCheckBoxColumn)grdDependencies.grdMain.Columns[i]).ElementStyle = (Style)FindResource("@CheckBoxGridCellElemntStyle");
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to design the " + mDepededItemType.ToString() + "-Variables dependencies grid data", ex);
            }
        }

        private void SetDependenciesHelper()
        {
            try
            {
                txtBlkDependenciesHelper.Text = string.Empty;
                TextBlockHelper TBH = new TextBlockHelper(txtBlkDependenciesHelper);

                //check if the mechanisem is enabeled
                switch (mDepededItemType)
                {
                    case (eDependedItemsType.Actions):
                        if (((Activity)mParentObject).EnableActionsVariablesDependenciesControl == false)
                        {
                            TBH.AddFormattedText("The Actions-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies control is disabled, in order to change the dependencies configurations you must enable it first.", Brushes.OrangeRed, true);
                            return;
                        }
                        break;
                    case (eDependedItemsType.Activities):
                        if (((BusinessFlow)mParentObject).EnableActivitiesVariablesDependenciesControl == false)
                        {
                            TBH.AddFormattedText("The " + GingerDicser.GetTermResValue(eTermResKey.Activities) + "-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies control is disabled, in order to change the dependencies configurations you must enable it first.", Brushes.OrangeRed, true);
                            return;
                        }
                        break;
                }


                //check for missing depended items/variables
                bool dependedItemsOrVarsAremissing = false;
                switch (mDepededItemType)
                {
                    case (eDependedItemsType.Actions):
                        if (((Activity)mParentObject).Acts.Count == 0)
                        {
                            TBH.AddFormattedText("Add Actions to the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " in order to configure dependencies.", Brushes.OrangeRed, true);
                            TBH.AddLineBreak();
                            dependedItemsOrVarsAremissing = true;
                        }
                        break;
                    case (eDependedItemsType.Activities):
                        if (((BusinessFlow)mParentObject).Activities.Count == 0)
                        {
                            TBH.AddFormattedText("Add " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " to the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " in order to configure dependencies.", Brushes.OrangeRed, true);
                            TBH.AddLineBreak();
                            dependedItemsOrVarsAremissing = true;
                        }
                        break;
                }

                if (mParentListVars == null || mParentListVars.Count == 0)
                {
                    switch (mDepededItemType)
                    {
                        case (eDependedItemsType.Actions):
                            TBH.AddFormattedText("Add " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " from type 'Selection List' to the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " in order to configure dependencies.", Brushes.OrangeRed, true);
                            break;
                        case (eDependedItemsType.Activities):
                            TBH.AddFormattedText("Add " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " from type 'Selection List' to the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " in order to configure dependencies.", Brushes.OrangeRed, true);
                            break;
                    }                    
                    TBH.AddLineBreak();
                    dependedItemsOrVarsAremissing = true;
                }
                if (dependedItemsOrVarsAremissing) return;

                //Set helper according to selected row               
                DataRowView currentRow = (DataRowView)grdDependencies.grdMain.CurrentItem;
                if (currentRow != null)
                {
                    TBH.AddFormattedText("'" + currentRow.Row[0].ToString() + "'", null, true, true);
                    switch (mDepededItemType)
                    {
                        case (eDependedItemsType.Actions):
                            TBH.AddUnderLineText(" Action Dependencies Configurations Meaning:");
                            break;
                        case (eDependedItemsType.Activities):
                            TBH.AddUnderLineText(" " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " Dependencies Configurations Meaning:");
                            break;
                    }                    
                    TBH.AddLineBreak();
                    Dictionary<string, List<string>> actionConfigs = new Dictionary<string, List<string>>();
                    int colsIndex = 2;
                    foreach (VariableBase var in mParentListVars)
                    {
                        List<string> configuredVals = new List<string>();
                        foreach (OptionalValue optVal in ((VariableSelectionList)var).OptionalValuesList)
                        {
                            if ((bool)currentRow[colsIndex] == true)
                                configuredVals.Add(optVal.Value);
                            colsIndex++;
                        }
                        actionConfigs.Add(var.Name, configuredVals);
                    }
                    List<KeyValuePair<string, List<string>>> notConfiguredVars = actionConfigs.Where(d => d.Value.Count == 0).ToList();
                    if (notConfiguredVars.Count == 0)
                    {
                        switch (mDepededItemType)
                        {
                            case (eDependedItemsType.Actions):
                                TBH.AddText("The action will be executed only in case:");
                                break;
                            case (eDependedItemsType.Activities):
                                TBH.AddText("The " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " will be executed only in case:");
                                break;
                        }                        
                        TBH.AddLineBreak();
                        bool isFirst = true;
                        foreach (KeyValuePair<string, List<string>> configuredVar in actionConfigs)
                        {
                            if (!isFirst)
                            {
                                TBH.AddText("And");
                                TBH.AddLineBreak();
                            }
                            isFirst = false;
                            TBH.AddBoldText("'" + configuredVar.Key + "' ");
                            TBH.AddText(GingerDicser.GetTermResValue(eTermResKey.Variable) + " selected value is: ");
                            string vars = string.Empty;
                            foreach(string configVal in configuredVar.Value)
                                vars= vars + "'" + configVal + "' Or ";
                            vars = vars.Remove(vars.Length - 4, 4);
                            TBH.AddText(vars);
                            TBH.AddLineBreak();
                        }
                    }
                    else
                    {
                        switch (mDepededItemType)
                        {
                            case (eDependedItemsType.Actions):
                                TBH.AddFormattedText("The action won't be executed in any case because it missing dependency configurations for the " + GingerDicser.GetTermResValue(eTermResKey.Variable) + "/s: ", Brushes.OrangeRed, true);
                                break;
                            case (eDependedItemsType.Activities):
                                TBH.AddFormattedText("The " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " won't be executed in any case because it missing dependency configurations for the " + GingerDicser.GetTermResValue(eTermResKey.Variable) + "/s: ", Brushes.OrangeRed, true);
                                break;
                        }                        
                        TBH.AddLineBreak();
                        foreach (KeyValuePair<string, List<string>> notConfiguredVar in notConfiguredVars)
                        {
                            TBH.AddFormattedText("'" + notConfiguredVar.Key + "'", Brushes.OrangeRed);
                            TBH.AddLineBreak();
                        }
                    }
                }
                else
                {
                    TBH.AddText("Select cell/row for information.");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to set the " + mDepededItemType.ToString() + "-Variables dependencies helper text", ex);
            }
        }

        private void grdMain_CurrentCellChanged(object sender, EventArgs e)
        {
            grdDependencies.grdMain.BeginEdit();

            //update helper details
            SetDependenciesHelper();
        }

        private void grdMain_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            foreach (DataRow rr in mDependsDT.Rows)
            {
                if (((DataRowView)(e.Row.Item)).Row.Equals(rr))
                {
                    rr[e.Column.DisplayIndex] = ((CheckBox)e.EditingElement).IsChecked;
                }
            }

            //update helper details 
            SetDependenciesHelper();
        }

        private void chkBoxEnableDisableDepControl_Click(object sender, RoutedEventArgs e)
        {
            if (chkBoxEnableDisableDepControl.IsChecked == true)
            {
                grdDependencies.IsEnabled = true;
            }
            else
            {
                grdDependencies.IsEnabled = false;
            }
            SetDependenciesHelper();
        }

        private void UnCheckAllTable(object sender, RoutedEventArgs e)
        {
            AutoCheckUncheck(eAutoCheckArea.Table, false);
        }

        private void CheckAllTable(object sender, RoutedEventArgs e)
        {
            AutoCheckUncheck(eAutoCheckArea.Table, true);
        }

        private void ColumnWasClicked(object sender, RoutedEventArgs e)
        {
            //select all column cells
            var columnHeader = sender as DataGridColumnHeader;
            if (columnHeader != null)
            {
                if (grdDependencies.Grid.Items != null && grdDependencies.Grid.Items.Count > 0)
                {
                    grdDependencies.Grid.SelectedCells.Clear();
                    foreach (var item in grdDependencies.Grid.Items)
                    {
                        grdDependencies.Grid.SelectedCells.Add(new DataGridCellInfo(item, columnHeader.Column));
                    }
                    grdDependencies.Grid.CurrentCell = new DataGridCellInfo(grdDependencies.Grid.Items[0], columnHeader.Column);
                }
            }
        }


        private void AllowUnAllowHorizentalScroll(object sender, RoutedEventArgs e)
        {
            if (isHorizentalScrollAllowed == false)
            {
                grdDependencies.AllowHorizentalScroll = true;
                grdDependencies.Grid.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                grdDependencies.SetGridColumnsWidth();
                isHorizentalScrollAllowed = true;
            }
            else
            {
                grdDependencies.AllowHorizentalScroll = false;
                grdDependencies.Grid.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                grdDependencies.SetGridColumnsWidth();
                isHorizentalScrollAllowed = false;
            }
        }

        private void UnCheckAllColumn(object sender, RoutedEventArgs e)
        {
            AutoCheckUncheck(eAutoCheckArea.Column, false);
        }

        private void CheckAllColumn(object sender, RoutedEventArgs e)
        {
            AutoCheckUncheck(eAutoCheckArea.Column, true);            
        }

        private void UnCheckAllRow(object sender, RoutedEventArgs e)
        {
            AutoCheckUncheck(eAutoCheckArea.Row, false);
        }

        private void CheckAllRow(object sender, RoutedEventArgs e)
        {
            AutoCheckUncheck(eAutoCheckArea.Row, true);
        }

        private void AutoCheckUncheck(eAutoCheckArea area, bool checkValue)
        {
            try
            {
                switch (area)
                {
                    case eAutoCheckArea.Table:
                        if (mDependsDT != null)
                        {
                            foreach (DataRow row in mDependsDT.Rows)
                                for (int i = 2; i < mDependsDT.Columns.Count; i++)
                                    row[i] = checkValue;
                        }
                        break;

                    case eAutoCheckArea.Column:
                        DataGridColumn selectedCol = grdDependencies.grdMain.CurrentColumn;
                        if (selectedCol != null)
                        {
                            if (selectedCol.DisplayIndex > 1)
                                foreach (DataRow row in mDependsDT.Rows)
                                    row[selectedCol.DisplayIndex] = checkValue;
                            else
                                Reporter.ToUser(eUserMsgKeys.SelectValidColumn);
                        }
                        else
                            Reporter.ToUser(eUserMsgKeys.SelectValidColumn);
                        break;

                    case eAutoCheckArea.Row:
                        if (grdDependencies.grdMain.CurrentItem != null)
                        {
                            DataRowView selectedRow = ((DataRowView)grdDependencies.grdMain.CurrentItem);
                            for (int i = 2; i < grdDependencies.grdMain.Columns.Count; i++)
                                selectedRow.Row[i] = checkValue;
                        }
                        else
                            Reporter.ToUser(eUserMsgKeys.SelectValidRow);
                        break;
                }

                //update helper details
                SetDependenciesHelper();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to do Auto check in 'VariablesDependenciesPage' grid", ex);
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button okBtn = new Button();
            okBtn.Content = "Ok";
            okBtn.Click += new RoutedEventHandler(okBtn_Click);
            
            Button undoBtn = new Button();
            undoBtn.Content = "Undo & Close";
            undoBtn.Click += new RoutedEventHandler(undoBtn_Click);

            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(okBtn);
            winButtons.Add(undoBtn);
            
            switch (mDepededItemType)
            {
                case (eDependedItemsType.Actions):
                    GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Edit '" + ((Activity)mParentObject).ActivityName + "' Actions-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies", this, winButtons, false, string.Empty, CloseWinClicked);           
                    break;
                case (eDependedItemsType.Activities):
                    GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Edit '" + ((BusinessFlow)mParentObject).Name + "' " + GingerDicser.GetTermResValue(eTermResKey.Activities) + "-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies", this, winButtons, false, string.Empty, CloseWinClicked);            
                    break;
            }            
        }

        private void UndoChangesAndClose()
        {
            Mouse.OverrideCursor = Cursors.Wait;            
            ((RepositoryItemBase)mParentObject).RestoreFromBackup(true);
            Mouse.OverrideCursor = null;

            _pageGenericWin.Close();
        }

        private void CloseWinClicked(object sender, EventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKeys.ToSaveChanges) == MessageBoxResult.No)
            {
                UndoChangesAndClose();
            }
            else
                _pageGenericWin.Close();
        }

        private void undoBtn_Click(object sender, RoutedEventArgs e)
        {
            UndoChangesAndClose();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mItemsVarsDataIsValid)
                {
                    //Clear old dependencies configurations
                    switch (mDepededItemType)
                    {
                        case (eDependedItemsType.Actions):
                            foreach (Act activityAct in ((Activity)mParentObject).Acts) activityAct.VariablesDependencies.Clear();
                            break;
                        case (eDependedItemsType.Activities):
                            foreach (Activity activity in ((BusinessFlow)mParentObject).Activities) activity.VariablesDependencies.Clear();
                            break;
                    }
                    
                    //Save new dependencies configurations
                    grdDependencies.StopGridSearch();
                    grdDependencies.grdMain.CommitEdit();
                    switch (mDepededItemType)
                    {
                        case (eDependedItemsType.Actions):
                            foreach (DataRowView row in grdDependencies.grdMain.Items)
                            {
                                Act act = ((Activity)mParentObject).Acts[grdDependencies.grdMain.Items.IndexOf(row)];
                                int colsIndex = 2;
                                foreach (VariableBase var in mParentListVars)
                                {
                                    foreach (OptionalValue optVal in ((VariableSelectionList)var).OptionalValuesList)
                                    {
                                        if ((bool)row[colsIndex] == true)
                                        {
                                            VariableDependency actAcd = act.VariablesDependencies.Where(acd => acd.VariableName == var.Name).FirstOrDefault();
                                            if (actAcd != null)
                                            {
                                                if (actAcd.VariableValues.Contains(optVal.Value) == false)
                                                    actAcd.VariableValues.Add(optVal.Value);
                                            }
                                            else
                                            {
                                                actAcd = new VariableDependency(var.Guid, var.Name, optVal.Value);
                                                act.VariablesDependencies.Add(actAcd);
                                            }
                                        }
                                        colsIndex++;
                                    }
                                }
                            }
                            break;
                        case (eDependedItemsType.Activities):
                            foreach (DataRowView row in grdDependencies.grdMain.Items)
                            {
                                Activity act = ((BusinessFlow)mParentObject).Activities[grdDependencies.grdMain.Items.IndexOf(row)];
                                int colsIndex = 2;
                                foreach (VariableBase var in mParentListVars)
                                {
                                    foreach (OptionalValue optVal in ((VariableSelectionList)var).OptionalValuesList)
                                    {
                                        if ((bool)row[colsIndex] == true)
                                        {
                                            VariableDependency actAcd = act.VariablesDependencies.Where(acd => acd.VariableName == var.Name).FirstOrDefault();
                                            if (actAcd != null)
                                            {
                                                if (actAcd.VariableValues.Contains(optVal.Value) == false)
                                                    actAcd.VariableValues.Add(optVal.Value);
                                            }
                                            else
                                            {
                                                actAcd = new VariableDependency(var.Guid, var.Name, optVal.Value);
                                                act.VariablesDependencies.Add(actAcd);
                                            }
                                        }
                                        colsIndex++;
                                    }
                                }
                            }
                            break;
                    }

                }

                //close window
                _pageGenericWin.Close();
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, ex.Message);
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to save the " + mDepededItemType.ToString() + "-Variables dependencies configurations", ex);
                _pageGenericWin.Close();
            }
        }
    }
}
