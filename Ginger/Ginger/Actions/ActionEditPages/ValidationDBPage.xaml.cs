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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GingerCore.Environments;
using GingerCore.Actions;
using GingerCore.NoSqlBase;
using amdocs.ginger.GingerCoreNET;
using System.IO;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using Amdocs.Ginger.Common;
using System.Windows.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ValidationDBPage.xaml
    /// </summary>
    public partial class ValidationDBPage : Page
    {
        private ActDBValidation mAct;

        ProjEnvironment pe;
        EnvApplication EA;
        Database db;

        public ValidationDBPage(ActDBValidation act)
        {
            InitializeComponent();
        
            this.mAct = act;

            if (String.IsNullOrEmpty(mAct.GetInputParamValue("SQL")))
            {
                mAct.AddOrUpdateInputParamValue("SQL", mAct.GetInputParamValue("Value"));
            }

            FillAppComboBox();

            //New UI Controls:
            //Query Type selection radio button :
            QueryTypeRadioButton.Init(typeof(ActDBValidation.eQueryType), SqlSelection, mAct.GetOrCreateInputParam(ActDBValidation.Fields.QueryTypeRadioButton, ActDBValidation.eQueryType.FreeSQL.ToString()), QueryType_SelectionChanged);
            checkQueryType();

            //Free SQL
            //needs to be unmarked when fixed VE issue
            SQLUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActDBValidation.Fields.SQL));

            //Read from sql file
            QueryFile.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActDBValidation.Fields.QueryFile), true, true, UCValueExpression.eBrowserType.File, "sql", BrowseQueryFile_Click, WorkSpace.Instance.SolutionRepository.SolutionFolder);

            QueryFile.ValueTextBox.TextChanged += ValueTextBox_TextChanged;
            
            //OLD binding and UI 
            GingerCore.General.FillComboFromEnumObj(ValidationCfgComboBox, act.DBValidationType);

            //TODO: fix hard coded
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ValidationCfgComboBox, ComboBox.SelectedValueProperty, act,  "DBValidationType");

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(AppNameComboBox, ComboBox.TextProperty, act, ActDBValidation.Fields.AppName);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DBNameComboBox, ComboBox.TextProperty, act, ActDBValidation.Fields.DBName);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(TablesComboBox, ComboBox.TextProperty, act, ActDBValidation.Fields.Table);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(KeySpaceComboBox, ComboBox.TextProperty, act, ActDBValidation.Fields.Keyspace);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ColumnComboBox, ComboBox.TextProperty, act, ActDBValidation.Fields.Column);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtWhere, TextBox.TextProperty, act, ActDBValidation.Fields.Where);
            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(CommitDB, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActDBValidation.Fields.CommitDB));

            KeySpaceComboBox.Items.Add(mAct.Keyspace);
            ComboAutoSelectIfOneItemOnly(KeySpaceComboBox);
            TablesComboBox.Items.Add(mAct.Table);
            ComboAutoSelectIfOneItemOnly(TablesComboBox);
            ColumnComboBox.Items.Add(mAct.Column);
            ComboAutoSelectIfOneItemOnly(ColumnComboBox);
            SetVisibleControlsForAction();
            SetQueryParamsGrid();
        }

        private async void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(QueryFile.ValueTextBox.Text))
            {
                async Task<bool> UserKeepsTyping()
                {
                    string txt = QueryFile.ValueTextBox.Text;
                    await Task.Delay(2000);
                    return txt != QueryFile.ValueTextBox.Text;
                }
                if (await UserKeepsTyping() || QueryFile.ValueTextBox.Text == null) return;
            }
            string FileName = QueryFile.ValueTextBox.Text;
            if (FileName != "" && File.Exists(amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(FileName)))
            {  
                parseScriptHeader(FileName);  
            }
        }

        public void parseScriptHeader(string FileName)
        {
            mAct.QueryParams.Clear();
            string[] script = File.ReadAllLines(amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(FileName));

            foreach (string line in script)
            {
                var pattern = @"<<([^<^>].*?)>>"; // like div[1]
                                                  // Parse the XPath to extract the nodes on the path
                var matches = Regex.Matches(line, pattern);
                foreach(Match match in matches)
                {
                    ActInputValue AIV = (from aiv in mAct.QueryParams where aiv.Param == match.Groups[1].Value select aiv).FirstOrDefault();
                    if (AIV == null)
                    {
                        AIV = new ActInputValue();
                        // AIV.Active = true;

                        AIV.Param = match.Groups[1].Value;
                        mAct.QueryParams.Add(AIV);
                        AIV.Value = "";                        
                    }                    
                }
            }

            if (mAct.QueryParams.Count > 0)
                QueryParamsPanel.Visibility = Visibility.Visible;
            else
                QueryParamsPanel.Visibility = Visibility.Collapsed;
            QueryParamsGrid.DataSourceList = mAct.QueryParams;
        }
        private void SetQueryParamsGrid()
        {
            //Show/hide if needed
            //QueryParamsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddInputValue));//?? going to be hide in next line code

            QueryParamsGrid.SetTitleLightStyle = true;
            QueryParamsGrid.ClearTools();
            QueryParamsGrid.ShowDelete = System.Windows.Visibility.Visible;

            //List<GridColView> view = new List<GridColView>();
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Param), WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Value), WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["QueryParamExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.ValueForDriver), Header = "Value ForDriver", WidthWeight = 150, BindingMode = BindingMode.OneWay });

            QueryParamsGrid.SetAllColumnsDefaultView(view);
            QueryParamsGrid.InitViewItems();
        }

        private void ComboAutoSelectIfOneItemOnly(ComboBox comboBox)
        {
            if (comboBox.Items.Count == 1)
            {
                comboBox.SelectedItem = comboBox.Items[0];
            }
        }

        
        /// <summary>
        /// Fill the environments Applications combo box
        /// </summary>
        private void FillAppComboBox()
        {            
            AppNameComboBox.Items.Clear();

            if (Context.GetAsContext(mAct.Context) != null && Context.GetAsContext(mAct.Context).Environment != null)
            {                
                pe = (from e in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>() where e.Name == Context.GetAsContext(mAct.Context).Environment.Name select e).FirstOrDefault();

                if (pe == null)
                {
                    return;
                }
                foreach (EnvApplication ea in pe.Applications)
                {
                    AppNameComboBox.Items.Add(ea.Name);
                }
                ComboAutoSelectIfOneItemOnly(AppNameComboBox);
            }
        }

        private void AppNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: if there is only one item in the combo auto select it
            DBNameComboBox.Items.Clear();
            KeySpaceComboBox.Items.Clear();
            TablesComboBox.Items.Clear();
            ColumnComboBox.Items.Clear();

            if ((((ComboBox)sender).SelectedItem) == null) return;

            string app = ((ComboBox)sender).SelectedItem.ToString();
            EA = (from a in pe.Applications where a.Name == app select a).FirstOrDefault();
            foreach (Database db in EA.Dbs)
            {
                DBNameComboBox.Items.Add(db.Name);
                
            }
            ComboAutoSelectIfOneItemOnly(DBNameComboBox);
        }

        private void DBNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KeySpaceComboBox.Items.Clear();
            TablesComboBox.Items.Clear();
            ColumnComboBox.Items.Clear();
        }

        private void TablesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            ColumnComboBox.Items.Clear();
        }

        private void KeySpaceComboBox_DropDownOpened(object sender, EventArgs e)
        {
            KeySpaceComboBox.Items.Clear();
            string DBName = DBNameComboBox.Text;
            db = (Database) (from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
            if (db == null) return;
            if (db.DBType == Database.eDBTypes.Cassandra)
            {
                NoSqlBase NoSqlDriver = null;
                NoSqlDriver = new GingerCassandra(db);

                List<string> keyspace = NoSqlDriver.GetKeyspaceList();
                foreach (string s in keyspace)
                {
                    KeySpaceComboBox.Items.Add(s);
                }
            }else if (db.DBType == Database.eDBTypes.Couchbase)
            {
                NoSqlBase NoSqlDriver = null;
                NoSqlDriver = new GingerCouchbase(db);

                List<string> keyspace = NoSqlDriver.GetKeyspaceList();
                foreach (string s in keyspace)
                {
                    KeySpaceComboBox.Items.Add(s);
                }
            }
        }

        private void TablesComboBox_DropDownOpened(object sender, EventArgs e)
        {
            TablesComboBox.Items.Clear();
            string DBName = DBNameComboBox.Text; 
            db = (Database) (from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
            if (db == null) return;
            string KeySpace = KeySpaceComboBox.Text;
            List<string> Tables = db.GetTablesList(KeySpace);
            if (Tables == null)
            { 
                return;
            }
            foreach (string s in Tables)
            {
                TablesComboBox.Items.Add(s);
            }
        }
        
        private void ColumnComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ColumnComboBox.Items.Clear();
            string DBName = DBNameComboBox.Text;
            db = (Database) (from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
            if (db == null) return;
            string table;
            if (db.DBType == Database.eDBTypes.Cassandra)
            {
                 table = KeySpaceComboBox.Text + "." + TablesComboBox.Text;
            }
            else
            {
                table = TablesComboBox.Text;
            }
            List<string> Columns = db.GetTablesColumns(table);
            if (Columns == null)
            {
                return;
            }                
            foreach (string s in Columns)
            {
                ColumnComboBox.Items.Add(s);
            }
        }

        private void ValidationCfgComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetVisibleControlsForAction();
        }

        private void SetVisibleControlsForAction()
        {
            if (ValidationCfgComboBox.SelectedItem == null)
            {
                RadioButtonsSection.Visibility = System.Windows.Visibility.Visible;
                FreeSQLStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                SqlFile.Visibility = System.Windows.Visibility.Collapsed;
                FreeSQLStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                DoCommit.Visibility = System.Windows.Visibility.Collapsed;
                Keyspace.Visibility = System.Windows.Visibility.Collapsed;
                TableColWhereStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

            //Ugly code but working, find way to make it simple use the enum val from combo
            ActDBValidation.eDBValidationType validationType = (ActDBValidation.eDBValidationType)ValidationCfgComboBox.SelectedValue;

            switch (validationType)
            {
                case ActDBValidation.eDBValidationType.UpdateDB:
                    RadioButtonsSection.Visibility = System.Windows.Visibility.Visible;
                    checkQueryType();
                    TableColWhereStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    FreeSQLLabel.Content = "Update DB SQL:";
                    DoCommit.Visibility = System.Windows.Visibility.Visible;
                    Keyspace.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case ActDBValidation.eDBValidationType.FreeSQL:
                    checkQueryType();
                    RadioButtonsSection.Visibility = System.Windows.Visibility.Visible;
                    if (mAct.GetInputParamValue(ActDBValidation.Fields.QueryTypeRadioButton) == ActDBValidation.eQueryType.FreeSQL.ToString())
                    {
                        FreeSQLStackPanel.Visibility = System.Windows.Visibility.Visible;
                        SqlFile.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else
                    {
                        SqlFile.Visibility = System.Windows.Visibility.Visible;
                        FreeSQLStackPanel.Visibility = System.Windows.Visibility.Collapsed;

                        if(mAct.QueryParams != null)
                        {
                            if (mAct.QueryParams.Count > 0)
                                QueryParamsPanel.Visibility = Visibility.Visible;
                            else
                                QueryParamsPanel.Visibility = Visibility.Collapsed;
                            QueryParamsGrid.DataSourceList = mAct.QueryParams;
                        }
                    }
                    DoCommit.Visibility = System.Windows.Visibility.Collapsed;
                    TableColWhereStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    FreeSQLLabel.Content = "Free SQL:";
                    Keyspace.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case ActDBValidation.eDBValidationType.SimpleSQLOneValue:
                    checkQueryType();
                    try
                    {
                        string DBName = DBNameComboBox.Text;
                        db = (Database) (from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
                        if (!(db == null))
                        {
                            if (db.DBType == Database.eDBTypes.Cassandra)
                            {
                                Keyspace.Visibility = System.Windows.Visibility.Visible;
                            }
                            else
                            {
                                Keyspace.Visibility = System.Windows.Visibility.Collapsed;
                            }
                        }
                    }
                    catch { }
                    FreeSQLStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    RadioButtonsSection.Visibility = System.Windows.Visibility.Collapsed;
                    TableColWhereStackPanel.Visibility = System.Windows.Visibility.Visible;
                    DoCommit.Visibility = System.Windows.Visibility.Collapsed;
                    SqlFile.Visibility = System.Windows.Visibility.Collapsed;                    
                    break;
                case ActDBValidation.eDBValidationType.RecordCount:
                    checkQueryType();
                    try
                    {
                        string DBName = DBNameComboBox.Text;
                        db = (Database) (from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
                        if (!(db == null))
                        {
                            if (db.DBType == Database.eDBTypes.Cassandra)
                            {
                                Keyspace.Visibility = System.Windows.Visibility.Visible;
                            }
                            else
                            {
                                Keyspace.Visibility = System.Windows.Visibility.Collapsed;
                            }
                        }
                    }
                    catch { }
                    RadioButtonsSection.Visibility = System.Windows.Visibility.Collapsed;
                    FreeSQLStackPanel.Visibility = System.Windows.Visibility.Visible;
                    TableColWhereStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    DoCommit.Visibility = System.Windows.Visibility.Collapsed;
                    SqlFile.Visibility = System.Windows.Visibility.Collapsed;
                    FreeSQLLabel.Content = @"Record count - SELECT COUNT(1) FROM {Table} - Enter only Table name below (+optional WHERE clause)";
                    break;

            }
        }
        
        private void DBNamVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, ActDBValidation.Fields.DBName, Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            DBNameComboBox.Text = mAct.DBName;
        }
        
        private void AppNamVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, ActDBValidation.Fields.AppName, Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            AppNameComboBox.Text = mAct.AppName;
        }

        private void TablesVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, ActDBValidation.Fields.Table, Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            TablesComboBox.Text = mAct.Table;
        }

        private void ColumnsVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, ActDBValidation.Fields.Column, Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            ColumnComboBox.Text = mAct.Column;
        }

        private void KeySpaceVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, ActDBValidation.Fields.Keyspace, Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            KeySpaceComboBox.Text = mAct.Keyspace;
        }

        public void QueryType_SelectionChanged(object sender, RoutedEventArgs e)
        {
            mAct.AddOrUpdateInputParamValue(ActDBValidation.Fields.QueryTypeRadioButton, (((RadioButton)sender).Tag).ToString());
            if (mAct.GetInputParamValue(ActDBValidation.Fields.QueryTypeRadioButton) == ActDBValidation.eQueryType.FreeSQL.ToString())
            {

                SqlFile.Visibility = System.Windows.Visibility.Collapsed;
                FreeSQLStackPanel.Visibility = System.Windows.Visibility.Visible;

            }
            else if (mAct.GetInputParamValue(ActDBValidation.Fields.QueryTypeRadioButton) == ActDBValidation.eQueryType.SqlFile.ToString())
            {
                SqlFile.Visibility = System.Windows.Visibility.Visible;
                FreeSQLStackPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        } //TODO populate field selection changed

        public void checkQueryType()
        {
            if (mAct.GetInputParamValue(ActDBValidation.Fields.QueryTypeRadioButton) == ActDBValidation.eQueryType.FreeSQL.ToString())
            {
                FreeSQLStackPanel.Visibility = System.Windows.Visibility.Visible;
                SqlFile.Visibility = System.Windows.Visibility.Collapsed;               
            }
            else 
            {
                SqlFile.Visibility = System.Windows.Visibility.Visible;
                FreeSQLStackPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public void BrowseQueryFile_Click(object sender, RoutedEventArgs e)
        {
            string SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();
            if (!String.IsNullOrEmpty(QueryFile.ValueTextBox.Text))
            {
                if(!System.IO.File.Exists(QueryFile.ValueTextBox.Text))
                {
                    return;
                }
                string FileName = QueryFile.ValueTextBox.Text.ToUpper();
                if (FileName.Contains(SolutionFolder))
                {
                    FileName = FileName.Replace(SolutionFolder, @"~\");
                }

                QueryFile.ValueTextBox.Text = FileName;                
            }
        }

        private void QueryParamGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = (ActInputValue)QueryParamsGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, nameof(ActInputValue.Value), Context.GetAsContext(mAct.Context));
            VEEW.ShowAsWindow();
        }
    }
}
